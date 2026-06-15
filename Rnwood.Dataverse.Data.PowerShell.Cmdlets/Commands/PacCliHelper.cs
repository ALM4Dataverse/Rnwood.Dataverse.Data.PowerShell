using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Helper class for managing and executing the Power Apps CLI (PAC).
    /// </summary>
    internal static class PacCliHelper
    {
        /// <summary>
        /// Represents the result of executing a PAC CLI command.
        /// </summary>
        internal class PacCliResult
        {
            /// <summary>
            /// Gets or sets the exit code from the PAC CLI process.
            /// </summary>
            public int ExitCode { get; set; }
            
            /// <summary>
            /// Gets or sets the combined stdout and stderr output from the PAC CLI command.
            /// </summary>
            public string Output { get; set; }
        }
        /// <summary>
        /// Gets the path to the PAC CLI executable from PATH.
        /// </summary>
        /// <param name="cmdlet">The cmdlet requesting the PAC CLI (for verbose/warning output).</param>
        /// <returns>The full path to the PAC CLI executable.</returns>
        public static string GetPacCliPath(PSCmdlet cmdlet)
        {
            cmdlet.WriteVerbose("Looking for PAC CLI in system PATH...");
            string pacInPath = FindPacInPath();
            if (pacInPath != null)
            {
                cmdlet.WriteVerbose($"Found PAC CLI in PATH: {pacInPath}");
                return pacInPath;
            }

            throw new InvalidOperationException("PAC CLI not found in PATH. Please install Power Apps CLI from https://aka.ms/PowerAppsCLI");
        }

        private static string FindPacInPath()
        {
            string pathEnv = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(pathEnv))
            {
                return null;
            }

            string[] pathDirs = pathEnv.Split(Path.PathSeparator);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string pathExt = Environment.GetEnvironmentVariable("PATHEXT");
                if (string.IsNullOrEmpty(pathExt))
                {
                    pathExt = ".EXE;.BAT;.CMD";
                }
                string[] extensions = pathExt.Split(';');
                foreach (string dir in pathDirs)
                {
                    try
                    {
                        foreach (string ext in extensions)
                        {
                            string pacPath = Path.Combine(dir, "pac" + ext);
                            if (File.Exists(pacPath))
                            {
                                return pacPath;
                            }
                        }
                    }
                    catch
                    {
                        // Ignore invalid paths
                    }
                }

            }
            else
            {
                string[] candidates = { "pac", "pac.sh" };
                foreach (string dir in pathDirs)
                {
                    try
                    {
                        foreach (string candidate in candidates)
                        {
                            string pacPath = Path.Combine(dir, candidate);
                            if (File.Exists(pacPath))
                            {
#if !NETFRAMEWORK
                                try
                                {
                                    var mode = File.GetUnixFileMode(pacPath);
                                    if ((mode & UnixFileMode.UserExecute) != 0)
                                    {
                                        return pacPath;
                                    }
                                }
                                catch
                                {
                                    // Ignore if unable to check permissions
                                }
#else
                                // On .NET Framework (Windows only), assume executable if exists
                                return pacPath;
#endif
                            }
                        }
                    }
                    catch
                    {
                        // Ignore invalid paths
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Executes a PAC CLI command and returns the exit code.
        /// </summary>
        /// <param name="cmdlet">The cmdlet executing the command (for output).</param>
        /// <param name="argumentItems">The individual arguments to pass to PAC CLI.</param>
        /// <param name="workingDirectory">The working directory for the process.</param>
        /// <returns>The exit code from the PAC CLI process.</returns>
        [Obsolete("Use ExecutePacCliWithOutput instead to get detailed error output")]
        public static int ExecutePacCli(PSCmdlet cmdlet, string[] argumentItems, string workingDirectory = null)
        {
            var result = ExecutePacCliWithOutput(cmdlet, argumentItems, workingDirectory);
            return result.ExitCode;
        }

        /// <summary>
        /// Executes a PAC CLI command and returns the result with output.
        /// Each item in <paramref name="argumentItems"/> is passed as a separate, properly-escaped
        /// argument — no manual quoting of paths or values is needed.
        /// </summary>
        /// <param name="cmdlet">The cmdlet executing the command (for output).</param>
        /// <param name="argumentItems">The individual arguments to pass to PAC CLI.</param>
        /// <param name="workingDirectory">The working directory for the process.</param>
        /// <returns>A PacCliResult containing the exit code and collected output.</returns>
        public static PacCliResult ExecutePacCliWithOutput(PSCmdlet cmdlet, string[] argumentItems, string workingDirectory = null)
        {
            string pacPath = GetPacCliPath(cmdlet);

            cmdlet.WriteVerbose($"Executing: {pacPath} {string.Join(" ", argumentItems)}");

            var startInfo = new ProcessStartInfo
            {
                FileName = pacPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory ?? Directory.GetCurrentDirectory()
            };

#if NETFRAMEWORK
            // On .NET Framework, ArgumentList is not available — build a properly-escaped string.
            var escapedArgs = new System.Collections.Generic.List<string>();
            foreach (string arg in argumentItems)
            {
                escapedArgs.Add(EscapeWindowsArgument(arg));
            }
            startInfo.Arguments = string.Join(" ", escapedArgs);
#else
            // On .NET 5+, use ArgumentList so the runtime handles all quoting/escaping correctly.
            foreach (string arg in argumentItems)
            {
                startInfo.ArgumentList.Add(arg);
            }
#endif

            var verboseMessages = new ConcurrentQueue<string>();
            var warningMessages = new ConcurrentQueue<string>();
            var allOutputLines = new ConcurrentQueue<string>();

            using (var process = new Process { StartInfo = startInfo })
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        verboseMessages.Enqueue($"PAC: {e.Data}");
                        allOutputLines.Enqueue(e.Data);
                    }
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        warningMessages.Enqueue($"PAC: {e.Data}");
                        allOutputLines.Enqueue(e.Data);
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Poll for messages until process exits
                while (!process.HasExited)
                {
                    string msg;
                    while (verboseMessages.TryDequeue(out msg))
                    {
                        cmdlet.WriteVerbose(msg);
                    }
                    while (warningMessages.TryDequeue(out msg))
                    {
                        cmdlet.WriteWarning(msg);
                    }
                    Thread.Sleep(50); // Small delay to avoid busy waiting
                }

                // Ensure process has exited and dequeue any remaining messages
                process.WaitForExit();

                string msg2;
                while (verboseMessages.TryDequeue(out msg2))
                {
                    cmdlet.WriteVerbose(msg2);
                }
                while (warningMessages.TryDequeue(out msg2))
                {
                    cmdlet.WriteWarning(msg2);
                }

                // Collect all output for error reporting
                var outputLines = new System.Collections.Generic.List<string>();
                string line;
                while (allOutputLines.TryDequeue(out line))
                {
                    outputLines.Add(line);
                }

                return new PacCliResult
                {
                    ExitCode = process.ExitCode,
                    Output = string.Join(Environment.NewLine, outputLines)
                };
            }
        }

#if NETFRAMEWORK
        /// <summary>
        /// Escapes a single argument for use in a Windows command-line string
        /// (i.e. for <see cref="ProcessStartInfo.Arguments"/>).
        /// Handles spaces, quotes, and trailing backslashes correctly.
        /// </summary>
        private static string EscapeWindowsArgument(string arg)
        {
            if (string.IsNullOrEmpty(arg))
            {
                return "\"\"";
            }

            // If the value contains no characters that need quoting, pass it through.
            if (arg.IndexOfAny(new[] { ' ', '"', '\t', '\n', '\v' }) < 0)
            {
                return arg;
            }

            var sb = new StringBuilder("\"");
            int backslashCount = 0;

            foreach (char c in arg)
            {
                if (c == '\\')
                {
                    backslashCount++;
                }
                else if (c == '"')
                {
                    // Each backslash before a quote must be doubled, then add an escaped quote.
                    sb.Append('\\', backslashCount * 2 + 1);
                    sb.Append('"');
                    backslashCount = 0;
                }
                else
                {
                    sb.Append('\\', backslashCount);
                    sb.Append(c);
                    backslashCount = 0;
                }
            }

            // Trailing backslashes must be doubled before the closing quote.
            sb.Append('\\', backslashCount * 2);
            sb.Append('"');
            return sb.ToString();
        }
#endif
    }
}
