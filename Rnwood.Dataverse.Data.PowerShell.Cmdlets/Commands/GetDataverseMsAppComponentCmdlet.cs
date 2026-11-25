using ICSharpCode.SharpZipLib.Zip;
using SharpYaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves components from a .msapp file.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseMsAppComponent")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseMsAppComponentCmdlet : PSCmdlet
    {
        /// <summary>
        /// Gets or sets the path to the .msapp file.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FromPath", HelpMessage = "Path to the .msapp file")]
        [ValidateNotNullOrEmpty]
        public string MsAppPath { get; set; }

        /// <summary>
        /// Gets or sets the Canvas app PSObject returned by Get-DataverseCanvasApp with -IncludeDocument.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "FromObject", ValueFromPipeline = true, HelpMessage = "Canvas app PSObject from Get-DataverseCanvasApp -IncludeDocument")]
        [ValidateNotNull]
        public PSObject CanvasApp { get; set; }

        /// <summary>
        /// Gets or sets the name pattern of components to retrieve. Supports wildcards (* and ?).
        /// </summary>
        [Parameter(HelpMessage = "Name pattern of components to retrieve. Supports wildcards (* and ?)")]
        public string ComponentName { get; set; }

        /// <inheritdoc />
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            byte[] msappBytes;

            if (ParameterSetName == "FromObject")
            {
                // Get .msapp from the PSObject's document property
                var documentProp = CanvasApp.Properties["document"];
                if (documentProp == null || documentProp.Value == null)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException("CanvasApp object does not have a 'document' property. Use Get-DataverseCanvasApp with -IncludeDocument."),
                        "DocumentPropertyMissing",
                        ErrorCategory.InvalidArgument,
                        CanvasApp));
                    return;
                }

                string base64Document = documentProp.Value.ToString();
                msappBytes = Convert.FromBase64String(base64Document);
            }
            else
            {
                var filePath = GetUnresolvedProviderPathFromPSPath(MsAppPath);
                if (!File.Exists(filePath))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new FileNotFoundException($"MsApp file not found: {filePath}"),
                        "FileNotFound",
                        ErrorCategory.ObjectNotFound,
                        filePath));
                    return;
                }

                msappBytes = File.ReadAllBytes(filePath);
            }

            ExtractComponents(msappBytes);
        }

        private void ExtractComponents(byte[] msappBytes)
        {
            using (var memoryStream = new MemoryStream(msappBytes))
            using (var zipInputStream = new ZipInputStream(memoryStream))
            {
                ZipEntry entry;
                while ((entry = zipInputStream.GetNextEntry()) != null)
                {
                    string entryName = entry.Name.Replace('\\', '/');
                    // Component YAML files are in Src/Components/ folder and end with .pa.yaml
                    if (entryName.StartsWith("Src/Components/") && entryName.EndsWith(".pa.yaml"))
                    {
                        string componentName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(entryName));

                        // Apply name filter
                        if (!string.IsNullOrEmpty(ComponentName) && !MatchesPattern(componentName, ComponentName))
                        {
                            continue;
                        }

                        // Read content
                        byte[] entryBytes = ReadZipEntryBytes(zipInputStream);
                        string yamlContent = System.Text.Encoding.UTF8.GetString(entryBytes);

                        // Strip the ComponentDefinitions header
                        string processedYaml = StripComponentHeader(yamlContent, componentName);

                        // Create PSObject
                        var psObject = new PSObject();
                        psObject.Properties.Add(new PSNoteProperty("ComponentName", componentName));
                        psObject.Properties.Add(new PSNoteProperty("FilePath", entryName));
                        psObject.Properties.Add(new PSNoteProperty("YamlContent", processedYaml));
                        psObject.Properties.Add(new PSNoteProperty("Size", entryBytes.Length));

                        WriteObject(psObject);
                    }
                }
            }
        }

        private byte[] ReadZipEntryBytes(ZipInputStream zipInputStream)
        {
            using (var entryStream = new MemoryStream())
            {
                byte[] buffer = new byte[4096];
                int count;
                while ((count = zipInputStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    entryStream.Write(buffer, 0, count);
                }
                return entryStream.ToArray();
            }
        }

        private bool MatchesPattern(string value, string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
            {
                return true;
            }

            // Convert wildcard pattern to regex pattern
            string regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$";

            return System.Text.RegularExpressions.Regex.IsMatch(value, regexPattern, 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Strips the component header (e.g., "ComponentDefinitions:\n  ComponentName:") using SharpYaml to parse and extract the component content.
        /// </summary>
        private string StripComponentHeader(string yamlContent, string componentName)
        {
            try
            {
                // Parse YAML using SharpYaml
                var yamlObject = YamlSerializer.Deserialize<object>(yamlContent);

                // Expected structure: { ComponentDefinitions: { ComponentName: { ... properties ... } } }
                if (yamlObject is Dictionary<string, object?> root &&
                    root.TryGetValue("ComponentDefinitions", out var componentsObj) &&
                    componentsObj is Dictionary<string, object?> components &&
                    components.TryGetValue(componentName, out var componentContent))
                {
                    // Serialize just the component content without the header
                    var contentYaml = YamlSerializer.Serialize(componentContent);
                    return contentYaml;
                }

                var errorRecord = new ErrorRecord(
                    new InvalidDataException($"Could not parse component YAML structure for '{componentName}'."),
                    "ComponentYamlStructureInvalid",
                    ErrorCategory.InvalidData,
                    componentName);
                ThrowTerminatingError(errorRecord);
                return null;
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    ex,
                    "ComponentYamlParseError",
                    ErrorCategory.InvalidData,
                    componentName));
                return null;
            }
        }
    }
}
