using Rnwood.Dataverse.Data.PowerShell.Cmdlets.Commands.Model;
using Rnwood.Dataverse.Data.PowerShell.Commands.Model;
using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using ICSharpCode.SharpZipLib.Zip;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Packs a Dataverse solution folder using the Power Apps CLI.
    /// </summary>
    [Cmdlet(VerbsData.Compress, "DataverseSolutionFile", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class CompressDataverseSolutionFileCmdlet : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the path to the solution folder to pack.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, HelpMessage = "Path to the solution folder to pack.")]
        [ValidateNotNullOrEmpty]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the output path for the packed solution file (.zip).
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, HelpMessage = "Output path for the packed solution file (.zip).")]
        [ValidateNotNullOrEmpty]
        public string OutputPath { get; set; }

        /// <summary>
        /// Gets or sets the package type for packing. Can be 'Unmanaged', 'Managed', or 'Both'.
        /// </summary>
        [Parameter(HelpMessage = "Package type: 'Unmanaged' (default), 'Managed' (from a previous unpack 'Both'), or 'Both'.")]
        public SolutionPackageType PackageType { get; set; } = SolutionPackageType.Unmanaged;

        /// <summary>
        /// Gets or sets an optional path to a solution packager mapping file.
        /// </summary>
        [Parameter(HelpMessage = "Path to a solution packager mapping XML file. Passed as --map to pac solution pack.")]
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

            if (!Directory.Exists(resolvedPath))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new DirectoryNotFoundException($"Solution folder not found: {resolvedPath}"),
                    "FolderNotFound",
                    ErrorCategory.ObjectNotFound,
                    resolvedPath));
                return;
            }

            if (!ShouldProcess($"Solution folder '{resolvedPath}' to '{resolvedOutputPath}'", "Pack"))
            {
                return;
            }

            WriteVerbose($"Packing solution from '{resolvedPath}' to '{resolvedOutputPath}'");

            string workingPath = resolvedPath;
            string tempPath = null;

            try
            {
                // Always check for and pack .msapp folders automatically
                var msappFolders = Directory.GetDirectories(resolvedPath, "*.msapp", SearchOption.AllDirectories);
                if (msappFolders.Length > 0)
                {
                    WriteVerbose($"Found {msappFolders.Length} .msapp folder(s). Creating temporary copy to pack them...");
                    tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"dataverse_solution_{Guid.NewGuid():N}");
                    Directory.CreateDirectory(tempPath);

                    WriteVerbose($"Copying solution to temporary location: {tempPath}");
                    CopyDirectory(resolvedPath, tempPath);

                    WriteVerbose("Packing .msapp folders...");
                    PackMsappFolders(tempPath, this);

                    workingPath = tempPath;
                }

                // Build PAC CLI arguments
                var args = $"solution pack --zipfile \"{resolvedOutputPath}\" --folder \"{workingPath}\" --packagetype {PackageType}";

                if (!string.IsNullOrEmpty(MapFile))
                {
                    var resolvedMapFile = GetUnresolvedProviderPathFromPSPath(MapFile);
                    args += $" --map \"{resolvedMapFile}\"";
                }

                // Execute PAC CLI
                var result = PacCliHelper.ExecutePacCliWithOutput(this, args);

                if (result.ExitCode != 0)
                {
                    var errorMessage = $"PAC CLI pack failed with exit code {result.ExitCode}";
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

                WriteVerbose("Solution packed successfully.");
            }
            finally
            {
                // Clean up temp directory
                if (tempPath != null && Directory.Exists(tempPath))
                {
                    WriteVerbose($"Cleaning up temporary directory: {tempPath}");
                    try
                    {
                        Directory.Delete(tempPath, recursive: true);
                    }
                    catch (Exception ex)
                    {
                        WriteWarning($"Failed to delete temporary directory '{tempPath}': {ex.Message}");
                    }
                }
            }
        }

        internal static void PackMsappFolders(string basePath, Cmdlet cmdlet)
        {
            // Find all directories with .msapp extension
            var msappFolders = Directory.GetDirectories(basePath, "*.msapp", SearchOption.AllDirectories);

            if (msappFolders.Length == 0)
            {
                cmdlet.WriteVerbose("No .msapp folders found.");
                return;
            }

            cmdlet.WriteVerbose($"Found {msappFolders.Length} .msapp folder(s) to pack.");

            foreach (string msappFolder in msappFolders)
            {
                string parentDirectory = System.IO.Path.GetDirectoryName(msappFolder);
                string folderName = System.IO.Path.GetFileName(msappFolder);
                // The folder already has .msapp extension, so use it as-is for the file name
                string msappFile = System.IO.Path.Combine(parentDirectory, folderName);

                cmdlet.WriteVerbose($"Packing '{msappFolder}' to '{msappFile}'");

                // Delete existing .msapp file if it exists (shouldn't normally)
                if (File.Exists(msappFile))
                {
                    cmdlet.WriteVerbose($"Deleting existing .msapp file: {msappFile}");
                    File.Delete(msappFile);
                }

                // Create the .msapp file (which is a zip) using a temporary file to avoid path conflict
                string tempMsappFile = System.IO.Path.Combine(parentDirectory, Guid.NewGuid().ToString("N") + ".tmp");
                new FastZip().CreateZip(tempMsappFile, msappFolder, true, null);

                cmdlet.WriteVerbose($"Successfully packed '{msappFolder}'");

                // Delete the folder after successful packing
                cmdlet.WriteVerbose($"Deleting source folder: {msappFolder}");
                Directory.Delete(msappFolder, recursive: true);

                // Move the temporary zip file to the final location
                File.Move(tempMsappFile, msappFile);
            }

            cmdlet.WriteVerbose($"Finished packing {msappFolders.Length} .msapp folder(s).");
        }

        private void CopyDirectory(string sourceDir, string destDir)
        {
            // Create destination directory
            Directory.CreateDirectory(destDir);

            // Copy files
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = System.IO.Path.Combine(destDir, System.IO.Path.GetFileName(file));
                File.Copy(file, destFile, overwrite: true);
            }

            // Copy subdirectories
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = System.IO.Path.Combine(destDir, System.IO.Path.GetFileName(subDir));
                CopyDirectory(subDir, destSubDir);
            }
        }
    }
}
