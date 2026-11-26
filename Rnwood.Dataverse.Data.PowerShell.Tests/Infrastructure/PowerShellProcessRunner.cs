using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure
{
    /// <summary>
    /// Helper class for running PowerShell scripts in a child process with the full module loaded.
    /// This is used by E2E tests to validate cmdlets in a complete PowerShell environment.
    /// </summary>
    public static class PowerShellProcessRunner
    {
        /// <summary>
        /// Runs a PowerShell script in a child process with the module loaded.
        /// Uses the TESTMODULEPATH environment variable to find the module.
        /// </summary>
        /// <param name="script">The PowerShell script to execute.</param>
        /// <param name="usePowerShellCore">
        /// If null (default), auto-detects based on target framework: pwsh.exe for net8.0, powershell.exe for net462.
        /// If true, uses pwsh.exe (PowerShell Core). If false, uses powershell.exe (Windows PowerShell).
        /// Can be overridden with TESTPOWERSHELL_EXECUTABLE environment variable (set to "pwsh" or "powershell").
        /// </param>
        /// <param name="timeoutSeconds">Maximum time to wait for the script to complete.</param>
        /// <returns>A result containing stdout, stderr, and exit code.</returns>
        public static PowerShellProcessResult Run(string script, bool? usePowerShellCore = null, int timeoutSeconds = 60, bool importModule = true)
        {
            var modulePath = GetModulePath();
            
            // Find the .psd1 manifest file in the module path
            var manifestPath = Path.Combine(modulePath, "Rnwood.Dataverse.Data.PowerShell.psd1");
            if (!File.Exists(manifestPath))
            {
                // Try to find any .psd1 file
                var psd1Files = Directory.GetFiles(modulePath, "*.psd1");
                if (psd1Files.Length == 0)
                {
                    throw new FileNotFoundException($"No .psd1 module manifest found in {modulePath}");
                }
                manifestPath = psd1Files[0];
            }
            
            var manifestLiteral = manifestPath.Replace("'", "''");

            // Build the full script that optionally imports the module before running the user script
            // Stream 6 redirection (6>&1) ensures Write-Host output is captured to stdout
            var prelude = "\n$ErrorActionPreference = 'Stop'\n$VerbosePreference = 'SilentlyContinue'\n$InformationPreference = 'Continue'\n";
            var moduleVariables = $"$moduleManifest = '{manifestLiteral}'\n$modulePath = Split-Path -Parent $moduleManifest\n";

            var importSection = importModule
                ? $"    Import-Module '{manifestLiteral}' -Force -ErrorAction Stop\n"
                : string.Empty;

            var fullScript = $@"{prelude}{moduleVariables}try {{
{importSection}    {script}
}} catch {{
    # Reset error action preference to ensure all Write-Error calls execute
    # (with 'Stop', the first Write-Error would throw and prevent subsequent ones)
    $ErrorActionPreference = 'Continue'
    # Output detailed exception information including inner exceptions
    $ex = $_.Exception
    Write-Error ""Exception: $($ex.GetType().FullName)""
    Write-Error ""Message: $($ex.Message)""
    
    # For AggregateException, output all inner exceptions
    if ($ex -is [System.AggregateException]) {{
        $aggEx = [System.AggregateException]$ex
        Write-Error ""AggregateException contains $($aggEx.InnerExceptions.Count) inner exception(s):""
        $i = 0
        foreach ($innerEx in $aggEx.InnerExceptions) {{
            $i++
            Write-Error ""  Inner Exception $i : $($innerEx.GetType().FullName)""
            Write-Error ""  Message: $($innerEx.Message)""
            if ($innerEx.InnerException) {{
                Write-Error ""    InnerException: $($innerEx.InnerException.GetType().FullName): $($innerEx.InnerException.Message)""
            }}
        }}
    }}
    
    # Output any inner exception
    if ($ex.InnerException) {{
        Write-Error ""InnerException: $($ex.InnerException.GetType().FullName)""
        Write-Error ""InnerException Message: $($ex.InnerException.Message)""
    }}
    
    Write-Error ""ScriptStackTrace: $($_.ScriptStackTrace)""
    exit 1
}}
";

            // Use a temp file to avoid command-line escaping issues with complex scripts
            var tempScriptPath = Path.Combine(Path.GetTempPath(), $"ps_test_{Guid.NewGuid():N}.ps1");
            try
            {
                File.WriteAllText(tempScriptPath, fullScript, Encoding.UTF8);
                
                var executable = DeterminePowerShellExecutable(usePowerShellCore);
                var arguments = $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -File \"{tempScriptPath}\"";

                var startInfo = new ProcessStartInfo
                {
                    FileName = executable,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                if (executable.Equals("powershell", StringComparison.OrdinalIgnoreCase) &&
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var inheritedModulePath = startInfo.EnvironmentVariables["PSModulePath"];
                    if (!string.IsNullOrEmpty(inheritedModulePath))
                    {
                        startInfo.EnvironmentVariables["PSModulePath"] = string.Join(
                            Path.PathSeparator.ToString(),
                            inheritedModulePath.Split(Path.PathSeparator)
                                .Where(path => !IsPowerShellCoreModulePath(path)));
                    }
                }

                var stdout = new StringBuilder();
                var stderr = new StringBuilder();

                using var process = new Process { StartInfo = startInfo };
                
                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                        stdout.AppendLine(e.Data);
                };
                
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                        stderr.AppendLine(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                if (!process.WaitForExit(timeoutSeconds * 1000))
                {
                    process.Kill();
                    throw new TimeoutException($"PowerShell script timed out after {timeoutSeconds} seconds");
                }

                // Wait for async output/error reads to complete. When using BeginOutputReadLine/BeginErrorReadLine,
                // WaitForExit(int) only waits for the process to exit, not for async read callbacks to finish.
                // Calling WaitForExit() without a timeout ensures all buffered output is flushed to the StringBuilder.
                process.WaitForExit();

                return new PowerShellProcessResult
                {
                    ExitCode = process.ExitCode,
                    StandardOutput = stdout.ToString().TrimEnd(),
                    StandardError = stderr.ToString().TrimEnd(),
                    Success = process.ExitCode == 0
                };
            }
            finally
            {
                // Clean up temp script file
                try { File.Delete(tempScriptPath); } catch { /* Ignore cleanup errors */ }
            }
        }

        /// <summary>
        /// Determines which PowerShell executable to use based on target framework and environment.
        /// </summary>
        /// <param name="usePowerShellCore">
        /// Optional override: true for pwsh, false for powershell, null for auto-detection.
        /// </param>
        /// <returns>The PowerShell executable name ("pwsh" or "powershell").</returns>
        private static string DeterminePowerShellExecutable(bool? usePowerShellCore)
        {
            // Check for environment variable override first
            var envOverride = Environment.GetEnvironmentVariable("TESTPOWERSHELL_EXECUTABLE");
            if (!string.IsNullOrWhiteSpace(envOverride))
            {
                envOverride = envOverride.Trim().ToLowerInvariant();
                if (envOverride == "pwsh" || envOverride == "pwsh.exe")
                {
                    return "pwsh";
                }
                else if (envOverride == "powershell" || envOverride == "powershell.exe")
                {
                    return "powershell";
                }
                // If invalid value, fall through to default logic
            }

            // If explicitly specified, use that value
            if (usePowerShellCore.HasValue)
            {
                return usePowerShellCore.Value ? "pwsh" : "powershell";
            }

            // Auto-detect based on target framework
            var targetFramework = GetTargetFramework();
            bool shouldUsePwsh;

            if (targetFramework.StartsWith("net4", StringComparison.OrdinalIgnoreCase) || 
                targetFramework.StartsWith(".NETFramework", StringComparison.OrdinalIgnoreCase))
            {
                // .NET Framework (net462) - use Windows PowerShell
                shouldUsePwsh = false;
            }
            else
            {
                // .NET Core/8+ (net8.0) - use PowerShell Core
                shouldUsePwsh = true;
            }

            // On non-Windows platforms, pwsh is the only option
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!shouldUsePwsh)
                {
                    throw new PlatformNotSupportedException(
                        "Windows PowerShell (powershell.exe) is only available on Windows. " +
                        "On Linux/macOS, PowerShell Core (pwsh) must be used. " +
                        "Set TESTPOWERSHELL_EXECUTABLE=pwsh or run tests with net8.0 target framework.");
                }
                return "pwsh";
            }

            // On Windows, verify the executable exists
            var executable = shouldUsePwsh ? "pwsh" : "powershell";
            
            // For pwsh, check if it's installed (it's not in Windows by default)
            if (shouldUsePwsh && !IsPowershellCoreAvailable())
            {
                throw new FileNotFoundException(
                    $"PowerShell Core (pwsh.exe) is required for net8.0 tests but was not found. " +
                    $"Please install PowerShell 7+ from https://aka.ms/powershell-release?tag=stable " +
                    $"or set TESTPOWERSHELL_EXECUTABLE=powershell to use Windows PowerShell instead.");
            }

            return executable;
        }

        private static bool IsPowerShellCoreModulePath(string path)
        {
            var segments = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            for (var i = 0; i + 2 < segments.Length; i++)
            {
                if (segments[i].Equals("PowerShell", StringComparison.OrdinalIgnoreCase) &&
                    (int.TryParse(segments[i + 1], out _) || Version.TryParse(segments[i + 1], out _)) &&
                    segments[i + 2].Equals("Modules", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the target framework of the currently executing assembly.
        /// </summary>
        private static string GetTargetFramework()
        {
            // Get the target framework from the executing assembly's attribute
            var assembly = typeof(PowerShellProcessRunner).Assembly;
            var attribute = assembly.GetCustomAttributes(typeof(System.Runtime.Versioning.TargetFrameworkAttribute), false)
                .FirstOrDefault() as System.Runtime.Versioning.TargetFrameworkAttribute;
            
            if (attribute != null && !string.IsNullOrEmpty(attribute.FrameworkName))
            {
                return attribute.FrameworkName;
            }
            
            // Fallback to current runtime framework
            return AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName 
                   ?? System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
        }

        /// <summary>
        /// Checks if PowerShell Core (pwsh) is available in the system PATH.
        /// </summary>
        private static bool IsPowershellCoreAvailable()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "pwsh",
                    Arguments = "-Version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    return false;
                }
                
                process.WaitForExit(5000);
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the module path from environment variable or default location.
        /// </summary>
        public static string GetModulePath()
        {
            var modulePath = Environment.GetEnvironmentVariable("TESTMODULEPATH");
            
            if (string.IsNullOrEmpty(modulePath))
            {
                // Try to find it relative to the test assembly
                var assemblyLocation = typeof(PowerShellProcessRunner).Assembly.Location;
                var assemblyDir = Path.GetDirectoryName(assemblyLocation)!;
                
                // Navigate up to solution root and find module
                var solutionRoot = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", "..", ".."));
                var defaultPath = Path.Combine(solutionRoot, "Rnwood.Dataverse.Data.PowerShell", "bin", "Debug", "netstandard2.0");
                
                if (Directory.Exists(defaultPath))
                {
                    modulePath = defaultPath;
                }
                else
                {
                    throw new InvalidOperationException(
                        "TESTMODULEPATH environment variable is not set and module not found at default location. " +
                        "Set TESTMODULEPATH to the built module directory (e.g., Rnwood.Dataverse.Data.PowerShell/bin/Debug/netstandard2.0)");
                }
            }

            if (!Directory.Exists(modulePath))
            {
                throw new DirectoryNotFoundException($"Module path does not exist: {modulePath}");
            }

            return modulePath;
        }
    }

    /// <summary>
    /// Result of running a PowerShell script in a child process.
    /// </summary>
    public class PowerShellProcessResult
    {
        /// <summary>
        /// The exit code of the PowerShell process.
        /// </summary>
        public int ExitCode { get; set; }

        /// <summary>
        /// The standard output from the script.
        /// </summary>
        public string StandardOutput { get; set; } = "";

        /// <summary>
        /// The standard error from the script.
        /// </summary>
        public string StandardError { get; set; } = "";

        /// <summary>
        /// True if the script completed successfully (exit code 0).
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets the combined output (stdout + stderr) always, useful for assertion context.
        /// </summary>
        public string GetFullOutput()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Exit code: {ExitCode}");
            sb.AppendLine("StdOut:");
            sb.AppendLine(StandardOutput);
            if (!string.IsNullOrEmpty(StandardError))
            {
                sb.AppendLine("StdErr:");
                sb.AppendLine(StandardError);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Throws an exception if the script failed.
        /// </summary>
        public void ThrowIfFailed()
        {
            if (!Success)
            {
                throw new Exception($"PowerShell script failed with exit code {ExitCode}:\n{GetFullOutput()}");
            }
        }
    }
}
