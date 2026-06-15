using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Rnwood.Dataverse.Data.PowerShell.Commands.Model;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Specifies the import mode for solution imports.
    /// </summary>
    public enum ImportMode
    {
        /// <summary>
        /// Automatically determine the best import method based on solution existence and managed status.
        /// </summary>
        Auto,

        /// <summary>
        /// Skip upgrade logic and perform regular import.
        /// </summary>
        NoUpgrade,

        /// <summary>
        /// Import the solution using Stage and Upgrade mode.
        /// </summary>
        StageAndUpgrade,

        /// <summary>
        /// Import the solution as a holding solution staged for upgrade.
        /// </summary>
        HoldingSolution
    }

    /// <summary>
    /// Imports a solution to Dataverse using an asynchronous job with progress reporting.
    /// </summary>
    [Cmdlet(VerbsData.Import, "DataverseSolution", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class ImportDataverseSolutionCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the path to the solution file to import.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FromFile", HelpMessage = "Path to the solution file (.zip) to import.")]
        [ValidateNotNullOrEmpty]
        public string InFile { get; set; }

        /// <summary>
        /// Gets or sets the path to the solution folder to pack and import.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FromFolder", HelpMessage = "Path to the solution folder to pack and import.")]
        [ValidateNotNullOrEmpty]
        public string InFolder { get; set; }

        /// <summary>
        /// Gets or sets the package type for packing when using InFolder. Can be 'Unmanaged' or 'Managed'.
        /// </summary>
        [Parameter(ParameterSetName = "FromFolder", HelpMessage = "Package type: 'Unmanaged' (default) or 'Managed'.")]
        public ImportSolutionPackageType PackageType { get; set; } = ImportSolutionPackageType.Unmanaged;

        /// <summary>
        /// Gets or sets the solution file bytes to import.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "FromBytes", HelpMessage = "Solution file bytes to import.")]
        public byte[] SolutionFile { get; set; }

        /// <summary>
        /// Gets or sets whether any unmanaged customizations should be overwritten.
        /// </summary>
        [Parameter(HelpMessage = "Overwrite any unmanaged customizations that have been applied over existing managed solution components.")]
        public SwitchParameter OverwriteUnmanagedCustomizations { get; set; }

        /// <summary>
        /// Gets or sets whether workflows should be activated after import.
        /// </summary>
        [Parameter(HelpMessage = "Activate any processes (workflows) included in the solution after import.")]
        public SwitchParameter PublishWorkflows { get; set; }

        /// <summary>
        /// Gets or sets whether to skip product update dependencies.
        /// </summary>
        [Parameter(HelpMessage = "Skip enforcement of dependencies related to product updates.")]
        public SwitchParameter SkipProductUpdateDependencies { get; set; }

        /// <summary>
        /// Gets or sets the import mode.
        /// </summary>
        [Parameter(HelpMessage = "The import mode to use. Auto (default) automatically determines the best method based on solution existence and managed status.")]
        public ImportMode Mode { get; set; } = ImportMode.Auto;

        /// <summary>
        /// Gets or sets the connection references.
        /// </summary>
        [Parameter(HelpMessage = "Hashtable of connection reference schema names or connector names to connection IDs (e.g., @{'new_sharedconnectionref' = '00000000-0000-0000-0000-000000000000'} or @{'shared_sharepointonline' = '00000000-0000-0000-0000-000000000000'}). Keys can be either specific connection reference logical names or connector names (value after last '/' in connector ID). Connection references without a specific logical name mapping will fall back to checking by connector name.")]
        public Hashtable ConnectionReferences { get; set; } = new Hashtable();

        /// <summary>
        /// Gets or sets environment variable values as a hashtable.
        /// </summary>
        [Parameter(HelpMessage = "Hashtable of environment variable schema names to values (e.g., @{'new_apiurl' = 'https://api.example.com'}).")]
        public Hashtable EnvironmentVariables { get; set; } = new Hashtable();

        /// <summary>
        /// Gets or sets whether to convert to managed (obsolete).
        /// </summary>
        [Parameter(HelpMessage = "Obsolete. The system will convert unmanaged solution components to managed when you import a managed solution.")]
        public SwitchParameter ConvertToManaged { get; set; }

        /// <summary>
        /// Gets or sets whether to skip queue ribbon job.
        /// </summary>
        [Parameter(HelpMessage = "For internal use only.")]
        public SwitchParameter SkipQueueRibbonJob { get; set; }

        /// <summary>
        /// Gets or sets the layer desired order.
        /// </summary>
        [Parameter(HelpMessage = "For internal use only.")]
        public LayerDesiredOrder LayerDesiredOrder { get; set; }

        /// <summary>
        /// Gets or sets whether to use async ribbon processing.
        /// </summary>
        [Parameter(HelpMessage = "For internal use only.")]
        public SwitchParameter AsyncRibbonProcessing { get; set; }

        /// <summary>
        /// Gets or sets whether to use update if the existing solution present has same major and minor versions.
        /// </summary>
        [Parameter(HelpMessage = "Use update if the existing solution present has same major and minor versions. Only valid with Auto (default) or HoldingSolution mode. If the solution already exists in the target environment, compares the solution file with the target environment. If there are zero items removed ('TargetOnly' or 'InSourceAndTarget_BehaviourLessInclusiveInSource' status), uses simple install mode (no stage and upgrade or holding solution) for best performance.")]
        public SwitchParameter UseUpdateIfVersionMajorMinorMatches { get; set; }

        /// <summary>
        /// Gets or sets the polling interval in seconds for checking job status. Default is 5 seconds.
        /// </summary>
        [Parameter(HelpMessage = "Polling interval in seconds for checking job status. Default is 5.")]
        public int PollingIntervalSeconds { get; set; } = 5;

        /// <summary>
        /// Gets or sets the timeout in seconds for the import operation. Default is 7200 seconds (2 hours).
        /// </summary>
        [Parameter(HelpMessage = "Timeout in seconds for the import operation. Default is 7200 (2 hours).")]
        public int TimeoutSeconds { get; set; } = 7200;

        /// <summary>
        /// Gets or sets whether to skip validation of connection references.
        /// </summary>
        [Parameter(HelpMessage = "Skip validation that all required connection references are provided.")]
        public SwitchParameter SkipConnectionReferenceValidation { get; set; }

        /// <summary>
        /// Gets or sets whether to skip validation of environment variables.
        /// </summary>
        [Parameter(HelpMessage = "Skip validation that all required environment variables are provided.")]
        public SwitchParameter SkipEnvironmentVariableValidation { get; set; }

        /// <summary>
        /// Gets or sets whether to skip import if the solution version in the file is the same as the installed version.
        /// </summary>
        [Parameter(HelpMessage = "Skip import if the solution version in the file is the same as the installed version in the target environment.")]
        public SwitchParameter SkipIfSameVersion { get; set; }

        /// <summary>
        /// Gets or sets whether to skip import if the solution version in the file is lower than the installed version.
        /// </summary>
        [Parameter(HelpMessage = "Skip import if the solution version in the file is lower than the installed version in the target environment.")]
        public SwitchParameter SkipIfLowerVersion { get; set; }

        /// <summary>
        /// Gets or sets the maximum time in seconds to wait for existing solution history operations to complete before checking solution state.
        /// </summary>
        [Parameter(HelpMessage = "Maximum time in seconds to wait for existing solution history operations to complete before checking solution state. Default is 30.")]
        [ValidateRange(1, int.MaxValue)]
        public int SolutionHistoryWaitSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets whether to skip the solution history pre-check.
        /// </summary>
        [Parameter(HelpMessage = "Skip checking solution history for in-progress operations before checking existing solution state.")]
        public SwitchParameter SkipSolutionHistoryCheck { get; set; }

        /// <summary>
        /// Stores connection reference logical names extracted from the solution file with correct casing.
        /// Populated by ValidateSolutionComponents and used for case-insensitive matching against user-provided hashtable keys.
        /// </summary>
        private List<string> _solutionConnectionReferenceNames;

        /// <summary>
        /// Maps connection reference logical names to their connector IDs.
        /// Populated by ValidateSolutionComponents and used for connector ID fallback matching.
        /// </summary>
        private Dictionary<string, string> _connectionReferenceConnectorIds;

        /// <summary>
        /// Stores environment variable schema names extracted from the solution file with correct casing.
        /// Populated by ValidateSolutionComponents and used for case-insensitive matching against user-provided hashtable keys.
        /// </summary>
        private List<string> _solutionEnvironmentVariableNames;

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Load solution file
            byte[] solutionBytes;
            if (ParameterSetName == "FromFile")
            {
                if (!ShouldProcess($"Solution file '{InFile}'", "Import"))
                {
                    return;
                }

                var filePath = GetUnresolvedProviderPathFromPSPath(InFile);
                if (!File.Exists(filePath))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new FileNotFoundException($"Solution file not found: {filePath}"),
                        "FileNotFound",
                        ErrorCategory.ObjectNotFound,
                        filePath));
                    return;
                }

                WriteVerbose($"{filePath}");
                solutionBytes = File.ReadAllBytes(filePath);
            }
            else if (ParameterSetName == "FromFolder")
            {
                var folderPath = GetUnresolvedProviderPathFromPSPath(InFolder);
                if (!Directory.Exists(folderPath))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new DirectoryNotFoundException($"Solution folder not found: {folderPath}"),
                        "FolderNotFound",
                        ErrorCategory.ObjectNotFound,
                        folderPath));
                    return;
                }

                if (!ShouldProcess($"Solution folder '{folderPath}'", "Pack and Import"))
                {
                    return;
                }

                string tempZipPath = null;
                try
                {
                    // Pack the solution to a temp file
                    tempZipPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"solution_{Guid.NewGuid():N}.zip");
                    WriteVerbose($"Packing solution from '{folderPath}' to temporary file: {tempZipPath}");

                    string workingPath = folderPath;
                    string tempWorkingPath = null;

                    try
                    {
                        // Always check for and pack .msapp folders automatically
                        var msappFolders = Directory.GetDirectories(folderPath, "*.msapp", SearchOption.AllDirectories);
                        if (msappFolders.Length > 0)
                        {
                            WriteVerbose($"Found {msappFolders.Length} .msapp folder(s). Creating temporary copy to pack them...");
                            tempWorkingPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"dataverse_solution_{Guid.NewGuid():N}");
                            Directory.CreateDirectory(tempWorkingPath);

                            WriteVerbose($"Copying solution to temporary location: {tempWorkingPath}");
                            CopyDirectory(folderPath, tempWorkingPath);

                            WriteVerbose("Packing .msapp folders...");
                            CompressDataverseSolutionFileCmdlet.PackMsappFolders(tempWorkingPath, this);

                            workingPath = tempWorkingPath;
                        }

                        // Build PAC CLI arguments
                        var pacArgs = new[]
                        {
                            "solution", "pack",
                            "--zipfile", tempZipPath,
                            "--folder", workingPath,
                            "--packagetype", PackageType.ToString()
                        };

                        // Execute PAC CLI
                        var result = PacCliHelper.ExecutePacCliWithOutput(this, pacArgs);

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
                                folderPath));
                            return;
                        }

                        WriteVerbose("Solution packed successfully.");
                    }
                    finally
                    {
                        // Clean up temp working directory
                        if (tempWorkingPath != null && Directory.Exists(tempWorkingPath))
                        {
                            WriteVerbose($"Cleaning up temporary working directory: {tempWorkingPath}");
                            try
                            {
                                Directory.Delete(tempWorkingPath, recursive: true);
                            }
                            catch (Exception ex)
                            {
                                WriteWarning($"Failed to delete temporary directory '{tempWorkingPath}': {ex.Message}");
                            }
                        }
                    }

                    // Read the packed solution
                    WriteVerbose($"Reading packed solution from: {tempZipPath}");
                    solutionBytes = File.ReadAllBytes(tempZipPath);
                }
                finally
                {
                    // Clean up temp zip file after reading
                    if (tempZipPath != null && File.Exists(tempZipPath))
                    {
                        WriteVerbose($"Cleaning up temporary file: {tempZipPath}");
                        try
                        {
                            File.Delete(tempZipPath);
                        }
                        catch (Exception ex)
                        {
                            WriteWarning($"Failed to delete temporary file '{tempZipPath}': {ex.Message}");
                        }
                    }
                }
            }
            else
            {
                if (!ShouldProcess("Solution bytes", "Import"))
                {
                    return;
                }

                solutionBytes = SolutionFile;
            }

            WriteVerbose($"Solution file size: {solutionBytes.Length} bytes");

            // Validate parameter combinations
            if (UseUpdateIfVersionMajorMinorMatches.IsPresent && Mode != ImportMode.Auto && Mode != ImportMode.HoldingSolution)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException("-UseUpdateIfVersionMajorMinorMatches is only valid with Auto (default) or HoldingSolution mode."),
                    "InvalidParameterCombination",
                    ErrorCategory.InvalidArgument,
                    null));
            }

            // Determine import mode based on Mode parameter
            bool useNoUpgrade = Mode == ImportMode.NoUpgrade;
            bool useHoldingSolution = Mode == ImportMode.HoldingSolution;
            bool useStageAndUpgrade = Mode == ImportMode.Auto || Mode == ImportMode.StageAndUpgrade;

            // Extract solution info including version
            var (solutionUniqueName, isManaged, sourceSolutionVersion) = ExtractSolutionInfo(solutionBytes);
            WriteVerbose($"Source solution '{solutionUniqueName}' is {(isManaged ? "managed" : "unmanaged")}");

            if (sourceSolutionVersion != null)
            {
                WriteVerbose($"Source solution version: {sourceSolutionVersion}");
            }

            if (!SkipSolutionHistoryCheck.IsPresent)
            {
                SolutionHistoryHelpers.WaitForSolutionOperationsToComplete(
                    Connection,
                    SolutionHistoryHelpers.GetSolutionNamesToCheck(solutionUniqueName),
                    WriteVerbose,
                    SolutionHistoryWaitSeconds,
                    PollingIntervalSeconds);
            }

            // Validate solution components (connection references and environment variables)
            ValidateSolutionComponents(solutionBytes);

            // Check if this is an upgrade scenario and if the solution already exists
            bool shouldUseStageAndUpgrade = false;
            bool shouldUseHoldingSolution = false;
            bool solutionExists = DoesSolutionExist(solutionBytes);
            Version installedVersion = solutionExists ? GetInstalledSolutionVersion(solutionUniqueName) : null;

            // Version checking logic
            if (solutionExists && (SkipIfSameVersion.IsPresent || SkipIfLowerVersion.IsPresent))
            {
                WriteVerbose($"Installed solution version: {installedVersion}");

                int versionComparison = sourceSolutionVersion.CompareTo(installedVersion);

                if (SkipIfSameVersion.IsPresent && versionComparison == 0)
                {
                    WriteWarning($"Skipping import: Solution '{solutionUniqueName}' version {sourceSolutionVersion} is already installed (same version).");

                    // Check and update connection references and environment variables if provided
                    CheckAndUpdateSolutionComponents(solutionUniqueName, solutionBytes);

                    return;
                }

                if (SkipIfLowerVersion.IsPresent && versionComparison < 0)
                {
                    WriteWarning($"Skipping import: Solution '{solutionUniqueName}' version {sourceSolutionVersion} is lower than the installed version {installedVersion}.");

                    // Check and update connection references and environment variables if provided
                    CheckAndUpdateSolutionComponents(solutionUniqueName, solutionBytes);

                    return;
                }

                WriteVerbose($"Version check passed: source version {sourceSolutionVersion} vs installed version {installedVersion}");
            }

            if (useHoldingSolution)
            {
                WriteVerbose("HoldingSolution mode specified - checking if solution already exists...");

                // Check for existing holding solution (solutionname_Upgrade)
                string holdingSolutionName = $"{solutionUniqueName}_Upgrade";
                Version holdingSolutionVersion = GetInstalledSolutionVersion(holdingSolutionName);

                if (holdingSolutionVersion != null)
                {
                    WriteVerbose($"Found existing holding solution '{holdingSolutionName}' with version {holdingSolutionVersion}");

                    // Compare with source solution version
                    if (sourceSolutionVersion != null)
                    {
                        WriteVerbose($"Comparing source version {sourceSolutionVersion} with existing holding solution version {holdingSolutionVersion}");

                        if (sourceSolutionVersion.CompareTo(holdingSolutionVersion) == 0)
                        {
                            // Same version - skip import
                            WriteWarning($"Skipping import: Holding solution '{holdingSolutionName}' version {holdingSolutionVersion} already exists with the same version as the source solution.");

                            // Check and update connection references and environment variables if provided
                            CheckAndUpdateSolutionComponents(solutionUniqueName, solutionBytes);

                            return;
                        }
                        else
                        {
                            // Different version - fail with clear error
                            var errorMessage = new StringBuilder();
                            errorMessage.AppendLine($"Cannot import holding solution: A holding solution '{holdingSolutionName}' already exists with version {holdingSolutionVersion}, but the source solution has version {sourceSolutionVersion}.");
                            errorMessage.AppendLine();
                            errorMessage.AppendLine("To resolve this issue, you must either:");
                            errorMessage.AppendLine($"  1. Apply the existing upgrade by running: Invoke-DataverseSolutionUpgrade -SolutionName '{solutionUniqueName}'");
                            errorMessage.AppendLine($"  2. Remove the existing holding solution by running: Remove-DataverseSolution -UniqueName '{holdingSolutionName}'");
                            errorMessage.AppendLine();
                            errorMessage.AppendLine("Note: Applying the upgrade will delete the original solution and promote the holding solution. Removing the holding solution may result in data loss if it contains changes.");

                            ThrowTerminatingError(new ErrorRecord(
                                new InvalidOperationException(errorMessage.ToString()),
                                "HoldingSolutionVersionMismatch",
                                ErrorCategory.InvalidOperation,
                                holdingSolutionName));
                            return;
                        }
                    }
                    else
                    {
                        WriteWarning($"Unable to extract source solution version. Cannot verify if holding solution version matches.");
                    }
                }

                // No existing holding solution found, check if base solution exists
                if (solutionExists)
                {
                    shouldUseHoldingSolution = true;
                }
                else
                {
                    WriteWarning("Solution does not exist in the target environment. Falling back to regular import instead of upgrade.");
                }
            }
            else if (useStageAndUpgrade)
            {
                if (Mode == ImportMode.Auto)
                {
                    WriteVerbose("Auto mode - checking if solution already EXISTS and is managed...");
                }
                else
                {
                    WriteVerbose("StageAndUpgrade mode specified - checking if solution already exists...");
                }

                if (solutionExists && (Mode == ImportMode.StageAndUpgrade || isManaged))
                {
                    shouldUseStageAndUpgrade = true;
                    WriteVerbose("Solution exists and source is managed - using StageAndUpgradeAsyncRequest");
                }
                else
                {
                    if (!solutionExists)
                    {
                        WriteWarning("Solution does not exist in the target environment. Falling back to regular import instead of upgrade.");
                    }
                    else if (Mode == ImportMode.Auto && !isManaged)
                    {
                        WriteWarning("Source solution is unmanaged. Falling back to regular import instead of upgrade.");
                    }
                }
            }

            // Handle UseUpdateIfVersionMajorMinorMatches logic
            if (UseUpdateIfVersionMajorMinorMatches.IsPresent && solutionExists && (shouldUseStageAndUpgrade || shouldUseHoldingSolution))
            {
                WriteVerbose("UseUpdateIfVersionMajorMinorMatches is specified - checking major and minor version match...");
                if (installedVersion.Major == sourceSolutionVersion.Major &&
                    installedVersion.Minor == sourceSolutionVersion.Minor)
                {
                    WriteVerbose("Major and minor versions match - using Update import mode.");
                    shouldUseStageAndUpgrade = false;
                    shouldUseHoldingSolution = false;
                }
                else
                {
                    WriteVerbose("Major and minor versions does not match - Using Upgrade import mode.");
                }
            }

            // Build ComponentParameters from ConnectionReferences and EnvironmentVariables hashtables
            // Only include parameters that are discovered in the solution file
            EntityCollection componentParameters = GetComponentParameters();

            // Create the async import request
            OrganizationRequest importRequest;
            if (shouldUseStageAndUpgrade)
            {
                var stageRequest = new StageAndUpgradeAsyncRequest
                {
                    CustomizationFile = solutionBytes,
                    OverwriteUnmanagedCustomizations = OverwriteUnmanagedCustomizations.IsPresent,
                    PublishWorkflows = PublishWorkflows.IsPresent,
                    SkipProductUpdateDependencies = SkipProductUpdateDependencies.IsPresent,
                    ConvertToManaged = ConvertToManaged.IsPresent,
                    SkipQueueRibbonJob = SkipQueueRibbonJob.IsPresent,
                    AsyncRibbonProcessing = AsyncRibbonProcessing.IsPresent,
                    ComponentParameters = componentParameters != null && componentParameters.Entities.Any() ? componentParameters : null
                };

                if (LayerDesiredOrder != null)
                {
                    stageRequest.LayerDesiredOrder = LayerDesiredOrder;
                }

                importRequest = stageRequest;
                WriteVerbose("Using StageAndUpgradeAsyncRequest");
            }
            else
            {
                var importSolutionRequest = new ImportSolutionAsyncRequest
                {
                    CustomizationFile = solutionBytes,
                    OverwriteUnmanagedCustomizations = OverwriteUnmanagedCustomizations.IsPresent,
                    PublishWorkflows = PublishWorkflows.IsPresent,
                    SkipProductUpdateDependencies = SkipProductUpdateDependencies.IsPresent,
                    HoldingSolution = shouldUseHoldingSolution,
                    ConvertToManaged = ConvertToManaged.IsPresent,
                    SkipQueueRibbonJob = SkipQueueRibbonJob.IsPresent,
                    AsyncRibbonProcessing = AsyncRibbonProcessing.IsPresent,
                    ComponentParameters = componentParameters != null && componentParameters.Entities.Any() ? componentParameters : null
                };

                if (LayerDesiredOrder != null)
                {
                    importSolutionRequest.LayerDesiredOrder = LayerDesiredOrder;
                }

                importRequest = importSolutionRequest;
                WriteVerbose($"Using ImportSolutionAsyncRequest (HoldingSolution={shouldUseHoldingSolution})");
            }

            // Execute the async import request
            string importJobId;
            Guid asyncOperationId;
            if (importRequest is StageAndUpgradeAsyncRequest)
            {
                var response = (StageAndUpgradeAsyncResponse)Connection.Execute(importRequest);
                importJobId = response.ImportJobKey;
                asyncOperationId = response.AsyncOperationId;
            }
            else
            {
                var response = (ImportSolutionAsyncResponse)Connection.Execute(importRequest);
                importJobId = response.ImportJobKey;
                asyncOperationId = response.AsyncOperationId;
            }

            WriteVerbose($"Import job started. ImportJobKey: {importJobId}, AsyncOperationId: {asyncOperationId}");

            // Monitor the async operation
            var progressRecord = new ProgressRecord(1, "Importing Solution", "Importing solution...")
            {
                PercentComplete = 0
            };
            WriteProgress(progressRecord);

            var startTime = DateTime.UtcNow;
            var timeout = TimeSpan.FromSeconds(TimeoutSeconds);
            var pollingInterval = TimeSpan.FromSeconds(PollingIntervalSeconds);

            while (true)
            {
                // Check for timeout
                if (DateTime.UtcNow - startTime > timeout)
                {
                    progressRecord.RecordType = ProgressRecordType.Completed;
                    WriteProgress(progressRecord);
                    ThrowTerminatingError(new ErrorRecord(
                        new TimeoutException($"Solution import timed out after {TimeoutSeconds} seconds."),
                        "ImportTimeout",
                        ErrorCategory.OperationTimeout,
                        null));
                    return;
                }

                // Check if stopping has been requested
                if (Stopping)
                {
                    progressRecord.RecordType = ProgressRecordType.Completed;
                    WriteProgress(progressRecord);
                    WriteWarning("Import operation was stopped by user.");
                    return;
                }

                // Query the asyncoperation record to check status
                var query = new QueryExpression("asyncoperation")
                {
                    ColumnSet = new ColumnSet("statuscode", "message", "friendlymessage"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("asyncoperationid", ConditionOperator.Equal, asyncOperationId)
                        }
                    }
                };

                var asyncOperations = Connection.RetrieveMultiple(query);
                if (asyncOperations.Entities.Count == 0)
                {
                    progressRecord.RecordType = ProgressRecordType.Completed;
                    WriteProgress(progressRecord);
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Async operation {asyncOperationId} not found."),
                        "AsyncOperationNotFound",
                        ErrorCategory.ObjectNotFound,
                        asyncOperationId));
                    return;
                }

                var asyncOperation = asyncOperations.Entities[0];
                var statusCode = asyncOperation.GetAttributeValue<OptionSetValue>("statuscode")?.Value ?? 0;
                var message = asyncOperation.GetAttributeValue<string>("message");
                var friendlyMessage = asyncOperation.GetAttributeValue<string>("friendlymessage");

                // Query the importjob record to get progress
                var jobQuery = new QueryExpression("importjob")
                {
                    ColumnSet = new ColumnSet("progress"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("importjobid", ConditionOperator.Equal, importJobId)
                        }
                    }
                };

                var jobResults = Connection.RetrieveMultiple(jobQuery);
                int progress = 0;
                if (jobResults.Entities.Count > 0)
                {
                    progress = (int)jobResults.Entities[0].GetAttributeValue<double>("progress");
                }

                var statusDescription = GetStatusDescription(statusCode);
                progressRecord.StatusDescription = $"{statusDescription}";
                progressRecord.PercentComplete = progress;

                if (!string.IsNullOrEmpty(friendlyMessage))
                {
                    progressRecord.CurrentOperation = friendlyMessage;
                }
                else if (!string.IsNullOrEmpty(message))
                {
                    progressRecord.CurrentOperation = message;
                }

                WriteVerbose($"Import status: {statusDescription} (StatusCode={statusCode}, Progress={progress}%)");

                if (statusCode == 30) // Succeeded
                {
                    progressRecord.PercentComplete = 100;
                    progressRecord.RecordType = ProgressRecordType.Completed;
                    WriteProgress(progressRecord);
                    WriteVerbose("Import completed successfully.");

                    // Output the import job ID
                    var result = new PSObject();
                    result.Properties.Add(new PSNoteProperty("ImportJobId", importJobId));
                    result.Properties.Add(new PSNoteProperty("AsyncOperationId", asyncOperationId));
                    result.Properties.Add(new PSNoteProperty("Status", "Succeeded"));
                    WriteObject(result);
                    return;
                }
                else if (statusCode == 31) // Failed
                {
                    progressRecord.RecordType = ProgressRecordType.Completed;
                    WriteProgress(progressRecord);
                    var errorMessage = (friendlyMessage ?? "Unknown error") + " - " + message;
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Solution import failed: {errorMessage}"),
                        "ImportFailed",
                        ErrorCategory.InvalidOperation,
                        null));
                    return;
                }
                else if (statusCode == 32) // Canceled
                {
                    progressRecord.RecordType = ProgressRecordType.Completed;
                    WriteProgress(progressRecord);
                    WriteWarning("Solution import was canceled.");
                    return;
                }
                else
                {
                    // In progress - progress is set from importjob above
                    WriteProgress(progressRecord);
                }

                // Wait before polling again
                Thread.Sleep(pollingInterval);
            }
        }

        private EntityCollection GetComponentParameters()
        {
            EntityCollection componentParameters = null;

            int totalParams = (ConnectionReferences?.Count ?? 0) + (EnvironmentVariables?.Count ?? 0);

            if (totalParams > 0)
            {
                WriteVerbose($"Processing {totalParams} component parameter(s)...");
                componentParameters = new EntityCollection();

                // Process connection references - only include those discovered in the solution file
                if (ConnectionReferences != null && ConnectionReferences.Count > 0)
                {
                    WriteVerbose($"Processing {ConnectionReferences.Count} connection reference(s)...");

                    // Keep track of which connection references have been mapped (to avoid duplicates)
                    var mappedConnectionReferences = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    foreach (DictionaryEntry entry in ConnectionReferences)
                    {
                        var providedKey = entry.Key.ToString();
                        var connectionId = entry.Value.ToString();

                        // Check if the provided key matches a connection reference logical name directly
                        var correctlyCasedName = _solutionConnectionReferenceNames?.FirstOrDefault(n =>
                            string.Equals(n, providedKey, StringComparison.OrdinalIgnoreCase));

                        if (correctlyCasedName != null)
                        {
                            // Direct match by logical name
                            WriteVerbose($"  Setting connection reference '{correctlyCasedName}' to connection '{connectionId}' (matched by logical name)");

                            var componentParam = new Entity("connectionreference");
                            componentParam["connectionreferencelogicalname"] = correctlyCasedName;
                            componentParam["connectionid"] = connectionId;

                            componentParameters.Entities.Add(componentParam);
                            mappedConnectionReferences.Add(correctlyCasedName);
                        }
                        else if (_connectionReferenceConnectorIds != null)
                        {
                            // Check if the provided key matches a connector ID (fallback)
                            var matchingConnRefs = _connectionReferenceConnectorIds
                                .Where(kvp => DoesKeyMatchConnectorId(providedKey, kvp.Value))
                                .Select(kvp => kvp.Key)
                                .ToList();

                            if (matchingConnRefs.Count > 0)
                            {
                                // Found connection reference(s) with this connector ID
                                foreach (var connRefLogicalName in matchingConnRefs)
                                {
                                    // Only map if not already mapped by logical name
                                    if (!mappedConnectionReferences.Contains(connRefLogicalName))
                                    {
                                        var connectorName = GetConnectorNameFromId(_connectionReferenceConnectorIds[connRefLogicalName]);
                                        WriteVerbose($"  Setting connection reference '{connRefLogicalName}' to connection '{connectionId}' (matched by connector name '{connectorName}')");

                                        var componentParam = new Entity("connectionreference");
                                        componentParam["connectionreferencelogicalname"] = connRefLogicalName;
                                        componentParam["connectionid"] = connectionId;

                                        componentParameters.Entities.Add(componentParam);
                                        mappedConnectionReferences.Add(connRefLogicalName);
                                    }
                                    else
                                    {
                                        WriteVerbose($"  Skipping connection reference '{connRefLogicalName}' for connector key '{providedKey}' as it's already mapped by logical name.");
                                    }
                                }
                            }
                            else
                            {
                                WriteVerbose($"  Key '{providedKey}' does not match any connection reference logical name or connector name in the solution file, skipping.");
                            }
                        }
                        else
                        {
                            WriteVerbose($"  Connection reference '{providedKey}' is not in the solution file, skipping.");
                        }
                    }
                }

                // Process environment variables - only include those discovered in the solution file
                if (EnvironmentVariables != null && EnvironmentVariables.Count > 0)
                {
                    WriteVerbose($"Processing {EnvironmentVariables.Count} environment variable(s)...");

                    // Build list of correctly-cased names for querying existing values
                    // Only include those that are in the solution file
                    var correctlyCasedEnvVarNames = new List<string>();
                    foreach (DictionaryEntry entry in EnvironmentVariables)
                    {
                        var envVarName = entry.Key.ToString();
                        var correctlyCasedName = _solutionEnvironmentVariableNames?.FirstOrDefault(n =>
                            string.Equals(n, envVarName, StringComparison.OrdinalIgnoreCase));

                        if (correctlyCasedName != null)
                        {
                            correctlyCasedEnvVarNames.Add(correctlyCasedName);
                        }
                    }

                    // Query for existing environment variable values by schema name (using correct casing)
                    var existingEnvVarValuesBySchemaName = GetExistingEnvironmentVariableValueIds(correctlyCasedEnvVarNames);

                    foreach (DictionaryEntry entry in EnvironmentVariables)
                    {
                        var envVarSchemaName = entry.Key.ToString();
                        var envVarValue = entry.Value.ToString();

                        // Use the correctly-cased name from the solution file if available
                        var correctlyCasedName = _solutionEnvironmentVariableNames?.FirstOrDefault(n =>
                            string.Equals(n, envVarSchemaName, StringComparison.OrdinalIgnoreCase));

                        if (correctlyCasedName == null)
                        {
                            WriteVerbose($"  Environment variable '{envVarSchemaName}' is not in the solution file, skipping.");
                            continue;
                        }

                        WriteVerbose($"  Setting environment variable '{correctlyCasedName}' to value '{envVarValue}'");

                        var componentParam = new Entity("environmentvariablevalue");
                        componentParam["schemaname"] = correctlyCasedName;
                        componentParam["value"] = envVarValue;

                        // If there's an existing value record, include its ID for update
                        if (existingEnvVarValuesBySchemaName.TryGetValue(correctlyCasedName, out var existingValueId))
                        {
                            componentParam["environmentvariablevalueid"] = existingValueId;
                            WriteVerbose($"    Found existing value record with ID: {existingValueId}");
                        }

                        componentParameters.Entities.Add(componentParam);
                    }
                }
            }

            return componentParameters;
        }

        private bool DoesSolutionExist(byte[] solutionBytes)
        {
            try
            {
                // Extract the solution unique name from customizations.xml inside the zip
                var (solutionUniqueName, _, _) = ExtractSolutionInfo(solutionBytes);

                WriteVerbose($"Checking if solution '{solutionUniqueName}' exists in target environment...");

                var query = new QueryExpression("solution")
                {
                    ColumnSet = new ColumnSet("uniquename"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("uniquename", ConditionOperator.Equal, solutionUniqueName)
                        }
                    },
                    TopCount = 1
                };

                var solutions = Connection.RetrieveMultiple(query);

                bool exists = solutions.Entities.Count > 0;
                WriteVerbose($"Solution '{solutionUniqueName}' {(exists ? "exists" : "does not exist")} in target environment.");

                return exists;
            }
            catch (Exception ex)
            {
                WriteVerbose($"Error checking for existing solution: {ex.Message}");
                // If we can't determine, assume it doesn't exist and do regular import
                return false;
            }
        }

        private (string UniqueName, bool IsManaged, Version Version) ExtractSolutionInfo(byte[] solutionBytes)
        {
            using (var memoryStream = new MemoryStream(solutionBytes))
            using (var zipFile = new ZipFile(memoryStream))
            {
                // Find the solution.xml file in the solution
                var entryIndex = zipFile.FindEntry("solution.xml", true);

                if (entryIndex != -1)
                {
                    using (var stream = zipFile.GetInputStream(entryIndex))
                    using (var reader = new StreamReader(stream))
                    {
                        var xmlContent = reader.ReadToEnd();
                        var xdoc = XDocument.Parse(xmlContent);

                        // Navigate to the SolutionManifest element
                        var solutionManifest = xdoc.Root.Element("SolutionManifest");
                        if (solutionManifest == null)
                        {
                            ThrowTerminatingError(new ErrorRecord(new Exception("SolutionManifest element not found in solution.xml"), "SolutionManifestNotFound", ErrorCategory.InvalidData, null));
                            return (null, false, null);
                        }

                        // Extract the UniqueName from the SolutionManifest
                        var uniqueNameElement = solutionManifest.Element("UniqueName");

                        string uniqueName = null;
                        if (uniqueNameElement == null)
                        {
                            ThrowTerminatingError(new ErrorRecord(new Exception("UniqueName element not found in solution.xml"), "UniqueNameNotFound", ErrorCategory.InvalidData, null));
                            return (null, false, null);
                        }
                        uniqueName = uniqueNameElement.Value;

                        // Extract the Managed flag from the SolutionManifest
                        var managedElement = solutionManifest.Element("Managed");

                        bool isManaged = false;
                        if (managedElement != null && !string.IsNullOrEmpty(managedElement.Value))
                        {
                            isManaged = managedElement.Value == "1";
                            WriteVerbose($"Solution is {(isManaged ? "managed" : "unmanaged")}");
                        }
                        else
                        {
                            ThrowTerminatingError(new ErrorRecord(new Exception("Could not determine if solution is managed, assuming unmanaged"), "SolutionManagedStatusUnknown", ErrorCategory.InvalidData, null));
                        }

                        // Extract the Version from the SolutionManifest
                        var versionElement = solutionManifest.Element("Version");
                        Version version = null;
                        if (versionElement != null && !string.IsNullOrEmpty(versionElement.Value))
                        {
                            Version.TryParse(versionElement.Value, out version);
                        }

                        return (uniqueName, isManaged, version);
                    }
                }
            }

            return (null, false, null);
        }

        private void ValidateSolutionComponents(byte[] solutionBytes)
        {
            WriteVerbose("Validating solution components...");

            // Extract connection references and environment variables from the solution
            var solutionComponents = ExtractSolutionComponents(solutionBytes);

            // Store the extracted names with correct casing for later use
            _solutionConnectionReferenceNames = solutionComponents.ConnectionReferences;
            _solutionEnvironmentVariableNames = solutionComponents.EnvironmentVariables;

            // Use connector IDs from solution file
            _connectionReferenceConnectorIds = solutionComponents.ConnectorIds;

            // Validate connection references if not skipped
            if (!SkipConnectionReferenceValidation.IsPresent)
            {
                ValidateConnectionReferences(solutionComponents.ConnectionReferences);
            }

            // Validate environment variables if not skipped
            if (!SkipEnvironmentVariableValidation.IsPresent)
            {
                ValidateEnvironmentVariables(solutionComponents.EnvironmentVariables);
            }
        }

        private (List<string> ConnectionReferences, Dictionary<string, string> ConnectorIds, List<string> EnvironmentVariables) ExtractSolutionComponents(byte[] solutionBytes)
        {
            var connectionReferences = new List<string>();
            var connectorIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var environmentVariables = new List<string>();

            try
            {
                using (var memoryStream = new MemoryStream(solutionBytes))
                using (var zipFile = new ZipFile(memoryStream))
                {
                    // Find the customizations.xml file in the solution
                    var customizationsEntryIndex = zipFile.FindEntry("customizations.xml", true);

                    if (customizationsEntryIndex != -1)
                    {
                        using (var stream = zipFile.GetInputStream(customizationsEntryIndex))
                        using (var reader = new StreamReader(stream))
                        {
                            var xmlContent = reader.ReadToEnd();
                            var xdoc = XDocument.Parse(xmlContent);

                            // Extract connection references with their connector IDs
                            // Connection references are stored in the solution XML with specific schema
                            var connRefElements = xdoc.Descendants()
                                .Where(e => e.Name.LocalName == "connectionreference");

                            foreach (var connRef in connRefElements)
                            {
                                var logicalName = connRef.Attribute("connectionreferencelogicalname")?.Value;
                                if (!string.IsNullOrEmpty(logicalName))
                                {
                                    connectionReferences.Add(logicalName);
                                    
                                    // Extract connector ID from subelement (if present)
                                    // The structure can be: <connectionreference><connectorid>value</connectorid></connectionreference>
                                    var connectorIdElement = connRef.Element("connectorid") ?? 
                                                            connRef.Elements().FirstOrDefault(e => e.Name.LocalName == "connectorid");
                                    var connectorId = connectorIdElement?.Value ?? connRef.Attribute("connectorid")?.Value;
                                    
                                    if (!string.IsNullOrEmpty(connectorId))
                                    {
                                        connectorIds[logicalName] = connectorId;
                                        WriteVerbose($"Found connection reference '{logicalName}' with connector ID '{connectorId}' in solution");
                                    }
                                    else
                                    {
                                        WriteVerbose($"Found connection reference '{logicalName}' in solution (no connector ID found)");
                                    }
                                }
                            }
                        }
                    }

                    // Extract environment variables from separate files
                    foreach (ZipEntry entry in zipFile)
                    {
                        if (entry.Name.Contains("environmentvariabledefinitions/") &&
                            entry.Name.EndsWith("environmentvariabledefinition.xml", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                using (var stream = zipFile.GetInputStream(entry))
                                using (var reader = new StreamReader(stream))
                                {
                                    var xmlContent = reader.ReadToEnd();
                                    var xdoc = XDocument.Parse(xmlContent);

                                    // Get the schemaname from the root element attribute
                                    var root = xdoc.Root;
                                    var schemaName = root?.Attribute("schemaname")?.Value;

                                    if (!string.IsNullOrEmpty(schemaName))
                                    {
                                        environmentVariables.Add(schemaName);
                                        WriteVerbose($"Found environment variable in solution: {schemaName}");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                WriteVerbose($"Error parsing environment variable file {entry.Name}: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteVerbose($"Error extracting solution components: {ex.Message}");
            }

            return (connectionReferences, connectorIds, environmentVariables);
        }


        /// <summary>
        /// Extracts the connector name from a full connector ID path.
        /// For example, "/providers/Microsoft.PowerApps/apis/shared_sharepointonline" becomes "shared_sharepointonline".
        /// </summary>
        private string GetConnectorNameFromId(string connectorId)
        {
            if (string.IsNullOrEmpty(connectorId))
            {
                return connectorId;
            }

            var lastSlashIndex = connectorId.LastIndexOf('/');
            if (lastSlashIndex >= 0 && lastSlashIndex < connectorId.Length - 1)
            {
                return connectorId.Substring(lastSlashIndex + 1);
            }

            return connectorId;
        }

        /// <summary>
        /// Checks if a provided key matches a connector ID, comparing just the connector name (after last /).
        /// </summary>
        private bool DoesKeyMatchConnectorId(string providedKey, string connectorId)
        {
            if (string.IsNullOrEmpty(providedKey) || string.IsNullOrEmpty(connectorId))
            {
                return false;
            }

            // Extract connector names from both
            var providedConnectorName = GetConnectorNameFromId(providedKey);
            var actualConnectorName = GetConnectorNameFromId(connectorId);

            return string.Equals(providedConnectorName, actualConnectorName, StringComparison.OrdinalIgnoreCase);
        }

        private void ValidateConnectionReferences(List<string> requiredConnectionRefs)
        {
            if (requiredConnectionRefs == null || requiredConnectionRefs.Count == 0)
            {
                WriteVerbose("No connection references found in solution.");
                return;
            }

            WriteVerbose($"Validating {requiredConnectionRefs.Count} connection reference(s)...");

            var missingConnectionRefs = new List<(string LogicalName, string ConnectorId)>();

            foreach (var connRefName in requiredConnectionRefs)
            {
                // Check if this connection reference is provided in the parameters by logical name (case-insensitive)
                bool isProvidedByName = GetHashtableValueCaseInsensitive(ConnectionReferences, connRefName) != null;

                if (!isProvidedByName)
                {
                    // Check if provided by connector ID (fallback)
                    bool isProvidedByConnectorId = false;
                    string connectorId = null;
                    
                    if (_connectionReferenceConnectorIds != null && _connectionReferenceConnectorIds.TryGetValue(connRefName, out connectorId))
                    {
                        if (!string.IsNullOrEmpty(connectorId))
                        {
                            // Check if any key in the hashtable matches this connector ID
                            foreach (DictionaryEntry entry in ConnectionReferences)
                            {
                                var providedKey = entry.Key.ToString();
                                if (DoesKeyMatchConnectorId(providedKey, connectorId))
                                {
                                    isProvidedByConnectorId = true;
                                    WriteVerbose($"Connection reference '{connRefName}' is provided via connector ID '{GetConnectorNameFromId(connectorId)}' fallback.");
                                    break;
                                }
                            }
                        }
                    }

                    if (!isProvidedByConnectorId)
                    {
                        // Check if it exists in the target environment with a value
                        bool existsInTarget = CheckConnectionReferenceExistsInTarget(connRefName);

                        if (!existsInTarget)
                        {
                            missingConnectionRefs.Add((connRefName, connectorId));
                            WriteVerbose($"Connection reference '{connRefName}' is not provided and does not exist in target environment.");
                        }
                        else
                        {
                            WriteVerbose($"Connection reference '{connRefName}' exists in target environment.");
                        }
                    }
                }
                else
                {
                    WriteVerbose($"Connection reference '{connRefName}' is provided in parameters by logical name.");
                }
            }

            if (missingConnectionRefs.Count > 0)
            {
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine($"The following connection reference(s) are required but not provided:");
                foreach (var (logicalName, connectorId) in missingConnectionRefs)
                {
                    if (!string.IsNullOrEmpty(connectorId))
                    {
                        var connectorName = GetConnectorNameFromId(connectorId);
                        errorMessage.AppendLine($"  - {logicalName} (connector: {connectorName}, not found by logical name or connector ID)");
                    }
                    else
                    {
                        errorMessage.AppendLine($"  - {logicalName}");
                    }
                }
                errorMessage.AppendLine();
                errorMessage.AppendLine("Please provide values using the -ConnectionReferences parameter, or use -SkipConnectionReferenceValidation to skip this check.");
                errorMessage.AppendLine("You can specify connection references by logical name or by connector name (value after last '/' in connector ID).");

                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException(errorMessage.ToString()),
                    "MissingConnectionReferences",
                    ErrorCategory.InvalidArgument,
                    ConnectionReferences));
            }
        }

        private void ValidateEnvironmentVariables(List<string> requiredEnvVars)
        {
            if (requiredEnvVars == null || requiredEnvVars.Count == 0)
            {
                WriteVerbose("No environment variables found in solution.");
                return;
            }

            WriteVerbose($"Validating {requiredEnvVars.Count} environment variable(s)...");

            var missingEnvVars = new List<string>();

            foreach (var envVarName in requiredEnvVars)
            {
                // Check if this environment variable is provided in the parameters (case-insensitive)
                bool isProvided = GetHashtableValueCaseInsensitive(EnvironmentVariables, envVarName) != null;

                if (!isProvided)
                {
                    // Check if it exists in the target environment with a value
                    bool existsInTarget = CheckEnvironmentVariableExistsInTarget(envVarName);

                    if (!existsInTarget)
                    {
                        missingEnvVars.Add(envVarName);
                        WriteVerbose($"Environment variable '{envVarName}' is not provided and does not exist in target environment.");
                    }
                    else
                    {
                        WriteVerbose($"Environment variable '{envVarName}' exists in target environment.");
                    }
                }
                else
                {
                    WriteVerbose($"Environment variable '{envVarName}' is provided in parameters.");
                }
            }

            if (missingEnvVars.Count > 0)
            {
                var errorMessage = new StringBuilder();
                errorMessage.AppendLine($"The following environment variable(s) are required but not provided:");
                foreach (var envVar in missingEnvVars)
                {
                    errorMessage.AppendLine($"  - {envVar}");
                }
                errorMessage.AppendLine();
                errorMessage.AppendLine("Please provide values using the -EnvironmentVariables parameter, or use -SkipEnvironmentVariableValidation to skip this check.");

                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException(errorMessage.ToString()),
                    "MissingEnvironmentVariables",
                    ErrorCategory.InvalidArgument,
                    EnvironmentVariables));
            }
        }

        private bool CheckConnectionReferenceExistsInTarget(string connectionRefLogicalName)
        {
            var query = new QueryExpression("connectionreference")
            {
                ColumnSet = new ColumnSet("connectionreferencelogicalname", "connectionid"),
                Criteria = new FilterExpression
                {
                    Conditions =
                        {
                            new ConditionExpression("connectionreferencelogicalname", ConditionOperator.Equal, connectionRefLogicalName)
                        }
                },
                TopCount = 1
            };

            var results = Connection.RetrieveMultiple(query);

            if (results.Entities.Count > 0)
            {
                var connRef = results.Entities[0];
                var connectionId = connRef.GetAttributeValue<string>("connectionid");
                // Connection reference exists and has a value set
                return !string.IsNullOrEmpty(connectionId);
            }

            return false;
        }

        private bool CheckEnvironmentVariableExistsInTarget(string envVarSchemaName)
        {
            // Query for environment variable definition
            var defQuery = new QueryExpression("environmentvariabledefinition")
            {
                ColumnSet = new ColumnSet("environmentvariabledefinitionid", "schemaname"),
                Criteria = new FilterExpression
                {
                    Conditions =
                        {
                            new ConditionExpression("schemaname", ConditionOperator.Equal, envVarSchemaName)
                        }
                },
                TopCount = 1
            };

            var defResults = Connection.RetrieveMultiple(defQuery);

            if (defResults.Entities.Count > 0)
            {
                var envVarDef = defResults.Entities[0];
                var envVarDefId = envVarDef.Id;

                // Check if there's a value set for this environment variable
                var valueQuery = new QueryExpression("environmentvariablevalue")
                {
                    ColumnSet = new ColumnSet("value"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                            {
                                new ConditionExpression("environmentvariabledefinitionid", ConditionOperator.Equal, envVarDefId)
                            }
                    },
                    TopCount = 1
                };

                var valueResults = Connection.RetrieveMultiple(valueQuery);

                if (valueResults.Entities.Count > 0)
                {
                    var envVarValue = valueResults.Entities[0];
                    var value = envVarValue.GetAttributeValue<string>("value");
                    // Environment variable exists and has a value set
                    return !string.IsNullOrEmpty(value);
                }
            }

            return false;
        }

        private Dictionary<string, Guid> GetExistingEnvironmentVariableValueIds(List<string> schemaNames)
        {
            var result = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase);

            if (schemaNames == null || schemaNames.Count == 0)
            {
                return result;
            }

            WriteVerbose($"Querying for existing environment variable values for {schemaNames.Count} schema name(s)...");

            // Query for environment variable values by joining with definition
            // The schemaname field in environmentvariablevalue may not be reliably populated
            // (older records may contain a GUID instead of the actual schema name),
            // so we must use the schemaname from the environmentvariabledefinition instead.
            var query = new QueryExpression("environmentvariablevalue")
            {
                ColumnSet = new ColumnSet("environmentvariablevalueid")
            };

            // Add link to environmentvariabledefinition to get the reliable schema name
            var defLink = query.AddLink("environmentvariabledefinition", "environmentvariabledefinitionid", "environmentvariabledefinitionid");
            defLink.Columns = new ColumnSet("schemaname");
            defLink.EntityAlias = "def";

            // Filter by the definition's schemaname, not the value's schemaname
            defLink.LinkCriteria.AddCondition("schemaname", ConditionOperator.In, schemaNames.ToArray());

            var allResults = new List<Entity>();
            EntityCollection ec;
            do
            {
                ec = Connection.RetrieveMultiple(query);
                allResults.AddRange(ec.Entities);
                if (ec.MoreRecords)
                {
                    query.PageInfo.PageNumber++;
                    query.PageInfo.PagingCookie = ec.PagingCookie;
                }
            } while (ec.MoreRecords);

            foreach (var entity in allResults)
            {
                // Get the schema name from the linked definition entity (via alias)
                var schemaNameAlias = entity.GetAttributeValue<AliasedValue>("def.schemaname");
                var schemaName = schemaNameAlias?.Value as string;

                if (!string.IsNullOrEmpty(schemaName))
                {
                    result[schemaName] = entity.Id;
                    WriteVerbose($"  Found existing value for '{schemaName}': {entity.Id}");
                }
            }

            WriteVerbose($"Found {result.Count} existing environment variable value(s).");
            return result;

        }

        private string GetStatusDescription(int statusCode)
        {
            switch (statusCode)
            {
                case 0:
                    return "Waiting for resources";
                case 10:
                    return "Waiting";
                case 20:
                    return "In progress";
                case 21:
                    return "Pausing";
                case 22:
                    return "Canceling";
                case 30:
                    return "Succeeded";
                case 31:
                    return "Failed";
                case 32:
                    return "Canceled";
                default:
                    return $"Unknown status ({statusCode})";
            }
        }

        private string ExtractSolutionName(byte[] solutionBytes)
        {
            using (var memoryStream = new MemoryStream(solutionBytes))
            using (var zipFile = new ZipFile(memoryStream))
            {
                var entryIndex = zipFile.FindEntry("solution.xml", true);

                if (entryIndex != -1)
                {
                    using (var stream = zipFile.GetInputStream(entryIndex))
                    using (var reader = new StreamReader(stream))
                    {
                        var xmlContent = reader.ReadToEnd();
                        var xdoc = XDocument.Parse(xmlContent);
                        var solutionManifest = xdoc.Root?.Element("SolutionManifest");
                        return solutionManifest?.Element("UniqueName")?.Value;
                    }
                }
            }
            return null;
        }

        private Version GetInstalledSolutionVersion(string solutionUniqueName)
        {
            try
            {
                var query = new QueryExpression("solution")
                {
                    ColumnSet = new ColumnSet("version"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("uniquename", ConditionOperator.Equal, solutionUniqueName)
                        }
                    },
                    TopCount = 1
                };

                var solutions = Connection.RetrieveMultiple(query);

                if (solutions.Entities.Count > 0)
                {
                    var versionString = solutions.Entities[0].GetAttributeValue<string>("version");
                    if (!string.IsNullOrEmpty(versionString) && Version.TryParse(versionString, out var version))
                    {
                        return version;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteVerbose($"Error retrieving installed solution version: {ex.Message}");
            }

            return null;
        }

        private void CheckAndUpdateSolutionComponents(string solutionUniqueName, byte[] solutionBytes)
        {
            WriteVerbose("Checking and updating connection references and environment variables from solution...");

            // Extract connector IDs from solution file
            var solutionComponents = ExtractSolutionComponents(solutionBytes);
            var solutionConnRefConnectorIds = solutionComponents.ConnectorIds;

            // First try to find the _Upgrade solution, then fall back to the base solution
            string upgradeSolutionName = $"{solutionUniqueName}_Upgrade";
            Guid solutionId = Guid.Empty;
            string actualSolutionName = null;

            // Try to find the upgrade solution first
            var upgradeSolutionQuery = new QueryExpression("solution")
            {
                ColumnSet = new ColumnSet("solutionid", "uniquename"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("uniquename", ConditionOperator.Equal, upgradeSolutionName)
                    }
                },
                TopCount = 1
            };

            var upgradeSolutions = Connection.RetrieveMultiple(upgradeSolutionQuery);
            if (upgradeSolutions.Entities.Count > 0)
            {
                solutionId = upgradeSolutions.Entities[0].Id;
                actualSolutionName = upgradeSolutionName;
                WriteVerbose($"Found upgrade solution '{upgradeSolutionName}' with ID: {solutionId}");
            }
            else
            {
                // Upgrade solution not found, try the base solution
                WriteVerbose($"Upgrade solution '{upgradeSolutionName}' not found, checking for base solution '{solutionUniqueName}'");

                var solutionQuery = new QueryExpression("solution")
                {
                    ColumnSet = new ColumnSet("solutionid", "uniquename"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("uniquename", ConditionOperator.Equal, solutionUniqueName)
                        }
                    },
                    TopCount = 1
                };

                var solutions = Connection.RetrieveMultiple(solutionQuery);
                if (solutions.Entities.Count == 0)
                {
                    WriteVerbose($"Solution '{solutionUniqueName}' not found in target environment. Cannot determine which components belong to the solution.");
                    return;
                }

                solutionId = solutions.Entities[0].Id;
                actualSolutionName = solutionUniqueName;
                WriteVerbose($"Found solution '{solutionUniqueName}' with ID: {solutionId}");
            }

            // Query solution components to find connection references and environment variables in this solution
            var componentQuery = new QueryExpression("solutioncomponent")
            {
                ColumnSet = new ColumnSet("objectid", "componenttype"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("solutionid", ConditionOperator.Equal, solutionId),
                        new ConditionExpression("componenttype", ConditionOperator.In, new object[] { 380, 10091 }) // 380 = Environment Variable Definition, 10091 = Connection Reference
                    }
                }
            };

            var allSolutionComponents = new List<Entity>();
            EntityCollection result;
            do
            {
                result = Connection.RetrieveMultiple(componentQuery);
                allSolutionComponents.AddRange(result.Entities);
                if (result.MoreRecords)
                {
                    componentQuery.PageInfo.PageNumber++;
                    componentQuery.PageInfo.PagingCookie = result.PagingCookie;
                }
            } while (result.MoreRecords);

            WriteVerbose($"Found {allSolutionComponents.Count} connection reference(s) and environment variable(s) in solution");

            // Separate components by type
            var connectionReferenceIds = allSolutionComponents
                .Where(c => c.GetAttributeValue<OptionSetValue>("componenttype")?.Value == 635)
                .Select(c => c.GetAttributeValue<Guid>("objectid"))
                .ToList();

            var environmentVariableDefinitionIds = allSolutionComponents
                .Where(c => c.GetAttributeValue<OptionSetValue>("componenttype")?.Value == 380)
                .Select(c => c.GetAttributeValue<Guid>("objectid"))
                .ToList();

            // Build dictionaries of components to check - only those in the solution and provided by user
            Dictionary<string, string> connectionReferencesToCheck = new Dictionary<string, string>();
            Dictionary<string, string> environmentVariablesToCheck = new Dictionary<string, string>();

            // Query connection reference logical names for the IDs in the solution
            List<string> solutionConnRefNames = new List<string>();
            if (connectionReferenceIds.Count > 0)
            {
                var connRefQuery = new QueryExpression("connectionreference")
                {
                    ColumnSet = new ColumnSet("connectionreferenceid", "connectionreferencelogicalname"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("connectionreferenceid", ConditionOperator.In, connectionReferenceIds.ToArray())
                        }
                    }
                };

                var connRefs = Connection.RetrieveMultiple(connRefQuery);
                solutionConnRefNames = connRefs.Entities
                    .Select(e => e.GetAttributeValue<string>("connectionreferencelogicalname"))
                    .Where(n => !string.IsNullOrEmpty(n))
                    .ToList();
            }

            // Query environment variable schema names for the IDs in the solution
            List<string> solutionEnvVarNames = new List<string>();
            if (environmentVariableDefinitionIds.Count > 0)
            {
                var envVarDefQuery = new QueryExpression("environmentvariabledefinition")
                {
                    ColumnSet = new ColumnSet("environmentvariabledefinitionid", "schemaname"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("environmentvariabledefinitionid", ConditionOperator.In, environmentVariableDefinitionIds.ToArray())
                        }
                    }
                };

                var envVarDefs = Connection.RetrieveMultiple(envVarDefQuery);
                solutionEnvVarNames = envVarDefs.Entities
                    .Select(e => e.GetAttributeValue<string>("schemaname"))
                    .Where(n => !string.IsNullOrEmpty(n))
                    .ToList();
            }

            // Validate that all required connection references and environment variables have been provided
            // Temporarily store connector IDs for validation
            var previousConnectorIds = _connectionReferenceConnectorIds;
            _connectionReferenceConnectorIds = solutionConnRefConnectorIds;
            
            try
            {
                // This uses the same validation logic as the normal import path
                if (!SkipConnectionReferenceValidation.IsPresent)
                {
                    ValidateConnectionReferences(solutionConnRefNames);
                }

                if (!SkipEnvironmentVariableValidation.IsPresent)
                {
                    ValidateEnvironmentVariables(solutionEnvVarNames);
                }
            }
            finally
            {
                // Restore previous value
                _connectionReferenceConnectorIds = previousConnectorIds;
            }

            // Process connection references - build list of those to update
            if (ConnectionReferences != null && ConnectionReferences.Count > 0 && solutionConnRefNames.Count > 0)
            {
                // Keep track of which connection references have been mapped
                var mappedConnectionReferences = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                
                foreach (DictionaryEntry entry in ConnectionReferences)
                {
                    var providedKey = entry.Key.ToString();
                    var connectionId = entry.Value.ToString();

                    // Check if the provided key matches a connection reference logical name directly
                    var correctlyCasedName = solutionConnRefNames.FirstOrDefault(n =>
                        string.Equals(n, providedKey, StringComparison.OrdinalIgnoreCase));

                    if (correctlyCasedName != null)
                    {
                        // Direct match by logical name
                        connectionReferencesToCheck[correctlyCasedName] = connectionId;
                        mappedConnectionReferences.Add(correctlyCasedName);
                    }
                    else
                    {
                        // Check if the provided key matches a connector ID (fallback)
                        var matchingConnRefs = solutionConnRefConnectorIds
                            .Where(kvp => DoesKeyMatchConnectorId(providedKey, kvp.Value))
                            .Select(kvp => kvp.Key)
                            .ToList();

                        if (matchingConnRefs.Count > 0)
                        {
                            // Found connection reference(s) with this connector ID
                            foreach (var connRefLogicalName in matchingConnRefs)
                            {
                                // Only map if not already mapped by logical name
                                if (!mappedConnectionReferences.Contains(connRefLogicalName))
                                {
                                    connectionReferencesToCheck[connRefLogicalName] = connectionId;
                                    mappedConnectionReferences.Add(connRefLogicalName);
                                    var connectorName = GetConnectorNameFromId(solutionConnRefConnectorIds[connRefLogicalName]);
                                    WriteVerbose($"Mapped connection reference '{connRefLogicalName}' to connection ID via connector name '{connectorName}'.");
                                }
                            }
                        }
                        else
                        {
                            WriteVerbose($"Connection reference '{providedKey}' is not in the solution, skipping.");
                        }
                    }
                }
            }

            // Process environment variables - build list of those to update
            if (EnvironmentVariables != null && EnvironmentVariables.Count > 0 && solutionEnvVarNames.Count > 0)
            {
                foreach (DictionaryEntry entry in EnvironmentVariables)
                {
                    var envVarName = entry.Key.ToString();
                    var envVarValue = entry.Value.ToString();

                    // Find the correctly-cased name from the solution (case-insensitive match)
                    var correctlyCasedName = solutionEnvVarNames.FirstOrDefault(n =>
                        string.Equals(n, envVarName, StringComparison.OrdinalIgnoreCase));

                    if (correctlyCasedName != null)
                    {
                        // Use the correctly-cased name from the solution
                        environmentVariablesToCheck[correctlyCasedName] = envVarValue;
                    }
                    else
                    {
                        WriteVerbose($"Environment variable '{envVarName}' is not in the solution, skipping.");
                    }
                }
            }

            // Update connection references if needed
            if (connectionReferencesToCheck.Count > 0)
            {
                WriteVerbose($"Checking {connectionReferencesToCheck.Count} connection reference(s) from solution...");
                UpdateConnectionReferencesIfDifferent(connectionReferencesToCheck);
            }

            // Update environment variables if needed
            if (environmentVariablesToCheck.Count > 0)
            {
                WriteVerbose($"Checking {environmentVariablesToCheck.Count} environment variable(s) from solution...");
                UpdateEnvironmentVariablesIfDifferent(environmentVariablesToCheck);
            }
        }

        private void UpdateConnectionReferencesIfDifferent(Dictionary<string, string> connectionReferencesToSet)
        {
            foreach (var kvp in connectionReferencesToSet)
            {
                var logicalName = kvp.Key;
                var desiredConnectionId = kvp.Value;

                WriteVerbose($"Checking connection reference '{logicalName}'...");

                // Query for the connection reference by logical name
                var query = new QueryExpression("connectionreference")
                {
                    ColumnSet = new ColumnSet("connectionreferenceid", "connectionreferencelogicalname", "connectionid"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("connectionreferencelogicalname", ConditionOperator.Equal, logicalName)
                        }
                    },
                    TopCount = 1
                };

                var results = Connection.RetrieveMultiple(query);

                if (results.Entities.Count == 0)
                {
                    WriteVerbose($"  Connection reference '{logicalName}' not found in target environment. Skipping update (will be created during solution import).");
                    continue;
                }

                var connRef = results.Entities[0];
                var connRefId = connRef.Id;
                var currentConnectionId = connRef.GetAttributeValue<string>("connectionid");

                WriteVerbose($"  Current connection ID: {currentConnectionId ?? "(none)"}");
                WriteVerbose($"  Desired connection ID: {desiredConnectionId}");

                // Update only if different
                if (desiredConnectionId != currentConnectionId)
                {
                    WriteVerbose($"  Connection reference '{logicalName}' has different value. Updating...");

                    var updateEntity = new Entity("connectionreference", connRefId);
                    updateEntity["connectionid"] = desiredConnectionId;

                    Connection.Update(updateEntity);
                    WriteVerbose($"  Successfully updated connection reference '{logicalName}'");
                }
                else
                {
                    WriteVerbose($"  Connection reference '{logicalName}' already has the desired value. No update needed.");
                }
            }
        }

        private void UpdateEnvironmentVariablesIfDifferent(Dictionary<string, string> environmentVariablesToSet)
        {
            // Query for existing environment variable values by schema name
            var existingEnvVarValuesBySchemaName = GetExistingEnvironmentVariableValueIds(environmentVariablesToSet.Keys.ToList());

            foreach (var kvp in environmentVariablesToSet)
            {
                var schemaName = kvp.Key;
                var desiredValue = kvp.Value;

                WriteVerbose($"Checking environment variable '{schemaName}'...");

                // Query for the environment variable definition by schema name
                var defQuery = new QueryExpression("environmentvariabledefinition")
                {
                    ColumnSet = new ColumnSet("environmentvariabledefinitionid", "schemaname"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("schemaname", ConditionOperator.Equal, schemaName)
                        }
                    },
                    TopCount = 1
                };

                var defResults = Connection.RetrieveMultiple(defQuery);

                if (defResults.Entities.Count == 0)
                {
                    WriteVerbose($"  Environment variable definition '{schemaName}' not found in target environment. Skipping update (will be created during solution import).");
                    continue;
                }

                var envVarDef = defResults.Entities[0];
                var envVarDefId = envVarDef.Id;

                // Check if there's an existing value record
                if (existingEnvVarValuesBySchemaName.TryGetValue(schemaName, out var existingValueId))
                {
                    // Query the current value
                    var valueQuery = new QueryExpression("environmentvariablevalue")
                    {
                        ColumnSet = new ColumnSet("value"),
                        Criteria = new FilterExpression
                        {
                            Conditions =
                            {
                                new ConditionExpression("environmentvariablevalueid", ConditionOperator.Equal, existingValueId)
                            }
                        },
                        TopCount = 1
                    };

                    var valueResults = Connection.RetrieveMultiple(valueQuery);
                    if (valueResults.Entities.Count > 0)
                    {
                        var currentValue = valueResults.Entities[0].GetAttributeValue<string>("value");

                        WriteVerbose($"  Current value: {currentValue ?? "(none)"}");
                        WriteVerbose($"  Desired value: {desiredValue}");

                        // Update only if different
                        if (desiredValue != currentValue)
                        {
                            WriteVerbose($"  Environment variable '{schemaName}' has different value. Updating...");

                            var updateEntity = new Entity("environmentvariablevalue", existingValueId);
                            updateEntity["value"] = desiredValue;

                            Connection.Update(updateEntity);
                            WriteVerbose($"  Successfully updated environment variable value for '{schemaName}'");
                        }
                        else
                        {
                            WriteVerbose($"  Environment variable '{schemaName}' already has the desired value. No update needed.");
                        }
                    }
                }
                else
                {
                    // No existing value record - create one
                    WriteVerbose($"  Environment variable '{schemaName}' has no value record. Creating...");

                    var createEntity = new Entity("environmentvariablevalue");
                    createEntity["schemaname"] = schemaName;
                    createEntity["value"] = desiredValue;
                    createEntity["environmentvariabledefinitionid"] = new EntityReference("environmentvariabledefinition", envVarDefId);

                    var newValueId = Connection.Create(createEntity);
                    WriteVerbose($"  Successfully created environment variable value for '{schemaName}' (ID: {newValueId})");
                }
            }
        }

        /// <summary>
        /// Gets the value from a hashtable using case-insensitive key lookup.
        /// </summary>
        /// <param name="hashtable">The hashtable to search</param>
        /// <param name="key">The key to look up (case-insensitive)</param>
        /// <returns>The value if found, null otherwise</returns>
        private object GetHashtableValueCaseInsensitive(Hashtable hashtable, string key)
        {
            if (hashtable == null || key == null)
            {
                return null;
            }

            foreach (DictionaryEntry entry in hashtable)
            {
                if (string.Equals(entry.Key.ToString(), key, StringComparison.OrdinalIgnoreCase))
                {
                    return entry.Value;
                }
            }

            return null;
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
