using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Rnwood.Dataverse.Data.PowerShell.Commands.Model;
using System.IO.Compression;
using System.Xml.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Compares a solution file with the state of that solution in the target environment.
    /// </summary>
    [Cmdlet(VerbsData.Compare, "DataverseSolutionComponents")]
    [OutputType(typeof(PSObject))]
    [OutputType(typeof(bool))]
    public class CompareDataverseSolutionComponentsCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Resolves a default connection only for comparisons that query Dataverse.
        /// File-to-file comparisons operate entirely on the supplied solution files.
        /// </summary>
        protected override void BeginProcessing()
        {
            if (ParameterSetName == "FileToFile" || ParameterSetName == "BytesToFile")
            {
                return;
            }

            base.BeginProcessing();
        }

        /// <summary>
        /// Gets or sets the Dataverse connection.
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "FileToEnvironment", HelpMessage = "Dataverse connection for comparing with environment.")]
        [Parameter(Mandatory = false, ParameterSetName = "BytesToEnvironment", HelpMessage = "Dataverse connection for comparing with environment.")]
        public override ServiceClient Connection { get => base.Connection; set => base.Connection = value; }

        /// <summary>
        /// Gets or sets the path to the solution file to compare.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FileToEnvironment", HelpMessage = "Path to the solution file (.zip) to compare with environment.")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FileToFile", HelpMessage = "Path to the source solution file (.zip) to compare with target solution file.")]
        [ValidateNotNullOrEmpty]
        public string SolutionFile { get; set; }

        /// <summary>
        /// Gets or sets the solution file bytes to compare.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "BytesToEnvironment", HelpMessage = "Solution file bytes to compare with environment.")]
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "BytesToFile", HelpMessage = "Source solution file bytes to compare with target solution file.")]
        public byte[] SolutionBytes { get; set; }

        /// <summary>
        /// Switch to compare a solution file with the environment.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "FileToEnvironment", HelpMessage = "Compare a solution file with the environment.")]
        public SwitchParameter FileToEnvironment { get; set; }

        /// <summary>
        /// Switch to compare solution bytes with the environment.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "BytesToEnvironment", HelpMessage = "Compare solution bytes with the environment.")]
        public SwitchParameter BytesToEnvironment { get; set; }

        /// <summary>
        /// Switch to compare two solution files.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "FileToFile", HelpMessage = "Compare two solution files.")]
        public SwitchParameter FileToFile { get; set; }

        /// <summary>
        /// Switch to compare solution bytes with a target solution file.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "BytesToFile", HelpMessage = "Compare solution bytes with a target solution file.")]
        public SwitchParameter BytesToFile { get; set; }

        /// <summary>
        /// Gets or sets the path to the target solution file to compare.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "FileToFile", HelpMessage = "Path to the target solution file (.zip) to compare.")]
        [Parameter(Mandatory = true, Position = 1, ParameterSetName = "BytesToFile", HelpMessage = "Path to the target solution file (.zip) to compare.")]
        [ValidateNotNullOrEmpty]
        public string TargetSolutionFile { get; set; }

        /// <summary>
        /// Gets or sets whether to reverse the comparison direction (compare environment to solution instead of solution to environment).
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "FileToEnvironment", HelpMessage = "Reverse the comparison direction.")]
        [Parameter(Mandatory = false, ParameterSetName = "BytesToEnvironment", HelpMessage = "Reverse the comparison direction.")]
        public SwitchParameter ReverseComparison { get; set; }

        /// <summary>
        /// Gets or sets whether to test if the changes are additive only.
        /// When specified, returns true/false based on whether there are zero removed components or less inclusive behavior changes.
        /// Full comparison results are output to verbose.
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "FileToEnvironment", HelpMessage = "Test if changes are additive (no removed components or less inclusive behavior changes). Returns true/false, outputs full results to verbose.")]
        [Parameter(Mandatory = false, ParameterSetName = "BytesToEnvironment", HelpMessage = "Test if changes are additive (no removed components or less inclusive behavior changes). Returns true/false, outputs full results to verbose.")]
        [Parameter(Mandatory = false, ParameterSetName = "FileToFile", HelpMessage = "Test if changes are additive (no removed components or less inclusive behavior changes). Returns true/false, outputs full results to verbose.")]
        [Parameter(Mandatory = false, ParameterSetName = "BytesToFile", HelpMessage = "Test if changes are additive (no removed components or less inclusive behavior changes). Returns true/false, outputs full results to verbose.")]
        public SwitchParameter TestIfAdditive { get; set; }

        // Private fields to store solution file bytes for subcomponent extraction
        private byte[] _sourceSolutionBytes;
        private byte[] _targetSolutionBytes;

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Load source solution file
            byte[] sourceSolutionBytes;
            if (ParameterSetName == "BytesToEnvironment" || ParameterSetName == "BytesToFile")
            {
                sourceSolutionBytes = SolutionBytes;
            }
            else
            {
                var filePath = GetUnresolvedProviderPathFromPSPath(SolutionFile);
                if (!File.Exists(filePath))
                {
                    ThrowTerminatingError(new ErrorRecord(
                new FileNotFoundException($"Solution file not found: {filePath}"),
                        "FileNotFound",
                       ErrorCategory.ObjectNotFound,
                      filePath));
                    return;
                }

                WriteVerbose($"Loading solution file from: {filePath}");
                sourceSolutionBytes = File.ReadAllBytes(filePath);
            }

            WriteVerbose($"Solution file size: {sourceSolutionBytes.Length} bytes");

            // Store the source bytes for subcomponent extraction
            _sourceSolutionBytes = sourceSolutionBytes;

            // Handle FileToFile and BytesToFile parameter sets
            if (ParameterSetName == "FileToFile" || ParameterSetName == "BytesToFile")
            {
                // Load target solution file
                var targetFilePath = GetUnresolvedProviderPathFromPSPath(TargetSolutionFile);
                if (!File.Exists(targetFilePath))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new FileNotFoundException($"Target solution file not found: {targetFilePath}"),
                        "TargetFileNotFound",
                        ErrorCategory.ObjectNotFound,
                        targetFilePath));
                    return;
                }

                WriteVerbose($"Loading target solution file from: {targetFilePath}");
                _targetSolutionBytes = File.ReadAllBytes(targetFilePath);
                WriteVerbose($"Target solution file size: {_targetSolutionBytes.Length} bytes");

                // Extract solution info from both files
                string sourceSolutionName = ExtractSolutionName(sourceSolutionBytes);
                string targetSolutionName = ExtractSolutionName(_targetSolutionBytes);

                WriteVerbose($"Comparing solution '{sourceSolutionName}' to '{targetSolutionName}'");

                // Compare components
                CompareComponentsFileToFile(sourceSolutionName);
            }
            else
            {
                // Handle FileToEnvironment and BytesToEnvironment parameter sets
                // Extract solution info and components from source file
                string sourceSolutionName = ExtractSolutionName(sourceSolutionBytes);
                var sourceExtractor = new FileComponentExtractor(Connection, this, sourceSolutionBytes);

                // Query target environment for the solution
                var solutionQuery = new QueryExpression("solution")
                {
                    ColumnSet = new ColumnSet("solutionid", "uniquename", "friendlyname"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("uniquename", ConditionOperator.Equal, sourceSolutionName)
                        }
                    },
                    TopCount = 1
                };

                var solutions = Connection.RetrieveMultiple(solutionQuery);

                if (solutions.Entities.Count == 0)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Solution '{sourceSolutionName}' not found in target environment."),
                        "SolutionNotFound",
                        ErrorCategory.ObjectNotFound,
                        sourceSolutionName));
                    return;
                }

                var solutionId = solutions.Entities[0].Id;
                WriteVerbose($"Found solution in target environment: {solutionId}");


                // Compare components
                CompareComponents(sourceSolutionName, solutionId);
            }
        }

        private void OutputComparisonResult(SolutionComponent sourceComponent, SolutionComponent targetComponent,
       SolutionComponentStatus status, string solutionName, bool isReversed)
        {
            var result = new PSObject();
            result.Properties.Add(new PSNoteProperty("SolutionName", solutionName));

            int componentType = sourceComponent?.ComponentType ?? targetComponent?.ComponentType ?? 0;
            var dummyComponent = new SolutionComponent
            {
                ComponentType = componentType,
                UniqueName = sourceComponent?.UniqueName ?? targetComponent?.UniqueName
            };
            result.Properties.Add(new PSNoteProperty("ComponentType", componentType));
            result.Properties.Add(new PSNoteProperty("ComponentTypeName", ComponentTypeResolver.GetComponentTypeName(Connection, dummyComponent)));


            // Add source and target ObjectIds
            result.Properties.Add(new PSNoteProperty("SourceObjectId", (object)sourceComponent?.ObjectId));
            result.Properties.Add(new PSNoteProperty("TargetObjectId", (object)targetComponent?.ObjectId));
            result.Properties.Add(new PSNoteProperty("SourceUniqueName", (object)sourceComponent?.UniqueName));
            result.Properties.Add(new PSNoteProperty("TargetUniqueName", (object)targetComponent?.UniqueName));

            result.Properties.Add(new PSNoteProperty("Status", status.ToString()));
            
            var sourceBehaviorEnum = RootComponentBehaviorExtensions.FromInt(sourceComponent?.RootComponentBehavior);
            var targetBehaviorEnum = RootComponentBehaviorExtensions.FromInt(targetComponent?.RootComponentBehavior);
            
            result.Properties.Add(new PSNoteProperty("SourceBehavior", sourceBehaviorEnum));
            result.Properties.Add(new PSNoteProperty("TargetBehavior", targetBehaviorEnum));
            
            result.Properties.Add(new PSNoteProperty("IsSubcomponent", sourceComponent?.IsSubcomponent ?? targetComponent?.IsSubcomponent ?? false));

            if ((sourceComponent?.IsSubcomponent ?? targetComponent?.IsSubcomponent ?? false) &&
                (sourceComponent?.ParentComponentType.HasValue ?? targetComponent?.ParentComponentType.HasValue ?? false))
            {
                int? parentComponentType = sourceComponent?.ParentComponentType ?? targetComponent?.ParentComponentType;
                result.Properties.Add(new PSNoteProperty("ParentComponentType", parentComponentType.Value));
                result.Properties.Add(new PSNoteProperty("ParentComponentTypeName", ComponentTypeResolver.GetComponentTypeName(Connection, new SolutionComponent { ComponentType = parentComponentType.Value })));
                result.Properties.Add(new PSNoteProperty("ParentTableName", sourceComponent?.ParentTableName ?? targetComponent?.ParentTableName));
            }

            WriteObject(result);
        }

        private string ExtractSolutionName(byte[] solutionBytes)
        {
            using (var memoryStream = new MemoryStream(solutionBytes))
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read))
            {
                var solutionXmlEntry = archive.Entries.FirstOrDefault(e =>
                   e.FullName.Equals("solution.xml", StringComparison.OrdinalIgnoreCase));

                if (solutionXmlEntry != null)
                {
                    using (var stream = solutionXmlEntry.Open())
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

        private void CompareComponents(string solutionName, Guid? targetSolutionId)
        {
            // Create extractors
            IComponentExtractor sourceExtractor = new FileComponentExtractor(Connection, this, _sourceSolutionBytes);
            IComponentExtractor targetExtractor = new EnvironmentComponentExtractor(Connection, this, targetSolutionId.Value);

            // Compare components
            var comparer = new SolutionComponentComparer(sourceExtractor, targetExtractor, this);
            var comparisonResults = comparer.CompareComponents();

            // Handle TestIfAdditive mode
            if (TestIfAdditive.IsPresent)
            {
                ProcessAdditiveTest(comparisonResults, solutionName);
            }
            else
            {
                // Output results normally
                foreach (var result in comparisonResults)
                {
                    OutputComparisonResult(result.SourceComponent, result.TargetComponent, result.Status, solutionName, isReversed: false);
                }
            }
        }

        private void CompareComponentsFileToFile(string solutionName)
        {
            // Create extractors - Connection can be null for file-to-file comparison
            IComponentExtractor sourceExtractor = new FileComponentExtractor(Connection, this, _sourceSolutionBytes);
            IComponentExtractor targetExtractor = new FileComponentExtractor(Connection, this, _targetSolutionBytes);

            // Compare components
            var comparer = new SolutionComponentComparer(sourceExtractor, targetExtractor, this);
            var comparisonResults = comparer.CompareComponents();

            // Handle TestIfAdditive mode
            if (TestIfAdditive.IsPresent)
            {
                ProcessAdditiveTest(comparisonResults, solutionName);
            }
            else
            {
                // Output results normally
                foreach (var result in comparisonResults)
                {
                    OutputComparisonResult(result.SourceComponent, result.TargetComponent, result.Status, solutionName, isReversed: false);
                }
            }
        }

        private void ProcessAdditiveTest(List<SolutionComponentComparisonResult> comparisonResults, string solutionName)
        {
            // Count problematic statuses (same logic as Import-DataverseSolution UseUpdateIfAdditive)
            int targetOnlyCount = comparisonResults.Count(r => r.Status == SolutionComponentStatus.InTargetOnly);
            int lessInclusiveCount = comparisonResults.Count(r => r.Status == SolutionComponentStatus.InSourceAndTarget_BehaviourLessInclusiveInSource);

            WriteVerbose($"Comparison results: {targetOnlyCount} InTargetOnly, {lessInclusiveCount} InSourceAndTarget_BehaviourLessInclusiveInSource");

            // Output full comparison results to verbose
            WriteVerbose($"Full comparison results ({comparisonResults.Count} total components):");
            foreach (var result in comparisonResults)
            {
                string componentName = result.SourceComponent?.UniqueName ?? result.TargetComponent?.UniqueName ?? "Unknown";
                int componentType = result.SourceComponent?.ComponentType ?? result.TargetComponent?.ComponentType ?? 0;
                var sourceBehavior = RootComponentBehaviorExtensions.FromInt(result.SourceComponent?.RootComponentBehavior);
                var targetBehavior = RootComponentBehaviorExtensions.FromInt(result.TargetComponent?.RootComponentBehavior);
                
                WriteVerbose($"  Component: Type {componentType} '{componentName}' - Status: {result.Status}, SourceBehavior: {sourceBehavior}, TargetBehavior: {targetBehavior}");
            }

            // Return true if additive (no removed components, no less inclusive behavior changes)
            bool isAdditive = (targetOnlyCount == 0 && lessInclusiveCount == 0);

            if (isAdditive)
            {
                WriteVerbose("Result: Changes are additive (no removed components or less inclusive behavior changes)");
            }
            else
            {
                WriteVerbose($"Result: Changes are NOT additive ({targetOnlyCount} removed, {lessInclusiveCount} less inclusive)");
            }

            // Output boolean result
            WriteObject(isAdditive);
        }
    }
}
