using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using ICSharpCode.SharpZipLib.Zip;
using Rnwood.Dataverse.Data.PowerShell.Cmdlets.Commands.Model;
using Rnwood.Dataverse.Data.PowerShell.Commands.Model;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Unpacks a Dataverse solution file using the Power Apps CLI.
    /// </summary>
    [Cmdlet(VerbsData.Expand, "DataverseSolutionFile", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class ExpandDataverseSolutionFileCmdlet : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the path to the solution file (.zip) to unpack.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Path to the solution file (.zip) to unpack.")]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the output path where the solution will be unpacked.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, HelpMessage = "Output path where the solution will be unpacked.")]
        [ValidateNotNullOrEmpty]
        public string OutputPath { get; set; }

        /// <summary>
        /// Gets or sets whether to unpack .msapp files into folders.
        /// </summary>
        [Parameter(HelpMessage = "Unpack .msapp files found in the solution into folders (same name without extension).")]
        public SwitchParameter UnpackMsapp { get; set; }

        /// <summary>
        /// Gets or sets the package type for unpacking. Can be 'Unmanaged', 'Managed', or 'Both'.
        /// </summary>
        [Parameter(HelpMessage = "Package type: 'Unmanaged' (default), 'Managed', or 'Both' for dual Managed and Unmanaged operation.")]
        public SolutionPackageType PackageType { get; set; } = SolutionPackageType.Unmanaged;

        /// <summary>
        /// Gets or sets the source control format for unpacking. Can be 'Yaml' or 'Xml'.
        /// </summary>
        [Parameter(HelpMessage = "Source control format: 'Yaml' (YAML source control format, requires PAC CLI 2.4.1+) or 'Xml' (legacy XML format). Passed as --solutionType to pac solution unpack.")]
        public SolutionSourceFormat? SourceFormat { get; set; }

        /// <summary>
        /// Gets or sets an optional path to a solution packager mapping file.
        /// </summary>
        [Parameter(HelpMessage = "Path to a solution packager mapping XML file. Passed as --map to pac solution unpack.")]
        public string MapFile { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Resolve paths
            string resolvedPath = GetUnresolvedProviderPathFromPSPath(Path);
            string resolvedOutputPath = GetUnresolvedProviderPathFromPSPath(OutputPath);

            if (!File.Exists(resolvedPath))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new FileNotFoundException($"Solution file not found: {resolvedPath}"),
                    "FileNotFound",
                    ErrorCategory.ObjectNotFound,
                    resolvedPath));
                return;
            }

            if (!ShouldProcess($"Solution file '{resolvedPath}' to '{resolvedOutputPath}'", "Unpack"))
            {
                return;
            }

            WriteVerbose($"Unpacking solution from '{resolvedPath}' to '{resolvedOutputPath}'");

            // Build PAC CLI arguments (always use clobber and allowDelete)
            var args = $"solution unpack --zipfile \"{resolvedPath}\" --folder \"{resolvedOutputPath}\" --packagetype {PackageType} --clobber --allowDelete";

            if (SourceFormat.HasValue)
            {
                // Note: --solutionType is not present in all PAC CLI versions; ignored here.
                WriteVerbose($"SourceFormat '{SourceFormat.Value}' specified but --solutionType is not supported by this PAC CLI version and will be ignored.");
            }

            if (!string.IsNullOrEmpty(MapFile))
            {
                var resolvedMapFile = GetUnresolvedProviderPathFromPSPath(MapFile);
                args += $" --map \"{resolvedMapFile}\"";
            }

            // Execute PAC CLI
            var result = PacCliHelper.ExecutePacCliWithOutput(this, args);

            if (result.ExitCode != 0)
            {
                var errorMessage = $"PAC CLI unpack failed with exit code {result.ExitCode}";
                if (!string.IsNullOrWhiteSpace(result.Output))
                {
                    errorMessage += $"{Environment.NewLine}PAC CLI output:{Environment.NewLine}{result.Output}";
                }
                
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException(errorMessage),
                    "PacCliFailed",
                    ErrorCategory.InvalidOperation,
                    resolvedPath));
                return;
            }

            WriteVerbose("Solution unpacked successfully.");

            // If UnpackMsapp is specified, find and unpack .msapp files
            if (UnpackMsapp.IsPresent)
            {
                WriteVerbose("Searching for .msapp files to unpack...");
                UnpackMsappFiles(resolvedOutputPath, this);
            }
        }

        internal static void UnpackMsappFiles(string basePath, Cmdlet cmdlet)
        {
            // Find all .msapp files in the unpacked solution
            string[] msappFiles = Directory.GetFiles(basePath, "*.msapp", SearchOption.AllDirectories);

            if (msappFiles.Length == 0)
            {
                cmdlet.WriteVerbose("No .msapp files found.");
                return;
            }

            cmdlet.WriteVerbose($"Found {msappFiles.Length} .msapp file(s) to unpack.");

            foreach (string msappFile in msappFiles)
            {
                string msappDirectory = System.IO.Path.GetDirectoryName(msappFile);
                string msappFileName = System.IO.Path.GetFileName(msappFile);
                string tempOutputFolder = System.IO.Path.Combine(msappDirectory, msappFileName + ".tmp");
                string outputFolder = System.IO.Path.Combine(msappDirectory, msappFileName);

                cmdlet.WriteVerbose($"Unpacking '{msappFile}' to '{outputFolder}'");

                // Always overwrite - delete existing folder if present
                if (Directory.Exists(tempOutputFolder))
                {
                    Directory.Delete(tempOutputFolder, recursive: true);
                }

                Directory.CreateDirectory(tempOutputFolder);


                // Unzip the .msapp file
                // Use ZipFile instead of FastZip to normalize path separators on non-Windows platforms
                // .msapp files created on Windows may contain backslashes in entry names
                // which need to be converted to platform-appropriate path separators
                using (var zipFile = new ZipFile(msappFile))
                {
                    foreach (ZipEntry entry in zipFile)
                    {
                        // Normalize the entry name by splitting on both separators and recombining
                        // This ensures platform-native path construction
                        string[] pathComponents = entry.Name.Split(new[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
                        
                        // Skip directory entries
                        if (entry.IsDirectory)
                        {
                            string dirPath = System.IO.Path.Combine(new[] { tempOutputFolder }.Concat(pathComponents).ToArray());
                            Directory.CreateDirectory(dirPath);
                            continue;
                        }
                        
                        // Ensure the directory exists for the file
                        string filePath = System.IO.Path.Combine(new[] { tempOutputFolder }.Concat(pathComponents).ToArray());
                        string fileDir = System.IO.Path.GetDirectoryName(filePath);
                        if (!string.IsNullOrEmpty(fileDir) && !Directory.Exists(fileDir))
                        {
                            Directory.CreateDirectory(fileDir);
                        }
                        
                        // Extract the file
                        using (var inputStream = zipFile.GetInputStream(entry))
                        using (var outputStream = File.Create(filePath))
                        {
                            inputStream.CopyTo(outputStream);
                        }
                    }
                }

                File.Delete(msappFile);
                Directory.Move(tempOutputFolder, outputFolder);

                cmdlet.WriteVerbose($"Successfully unpacked '{msappFile}'");

            }

            cmdlet.WriteVerbose($"Finished unpacking {msappFiles.Length} .msapp file(s).");
        }
    }
}
