using SharpYaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Adds or updates a screen in a .msapp file using YAML-first packaging.
    /// Controls/*.json files are automatically regenerated from YAML.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseMsAppScreen", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class SetDataverseMsAppScreenCmdlet : PSCmdlet
    {
        private static readonly UTF8Encoding Utf8WithoutBom = new UTF8Encoding(false);

        /// <summary>
        /// Gets or sets the path to the .msapp file.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "YamlContent-FromPath", HelpMessage = "Path to the .msapp file")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "YamlFile-FromPath", HelpMessage = "Path to the .msapp file")]
        [ValidateNotNullOrEmpty]
        public string MsAppPath { get; set; }

        /// <summary>
        /// Gets or sets the Canvas app PSObject returned by Get-DataverseCanvasApp with -IncludeDocument.
        /// The document property will be updated with the modified .msapp.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "YamlContent-FromObject", ValueFromPipeline = true, HelpMessage = "Canvas app PSObject from Get-DataverseCanvasApp -IncludeDocument")]
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "YamlFile-FromObject", ValueFromPipeline = true, HelpMessage = "Canvas app PSObject from Get-DataverseCanvasApp -IncludeDocument")]
        [ValidateNotNull]
        public PSObject CanvasApp { get; set; }

        /// <summary>
        /// Gets or sets the screen name.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, HelpMessage = "Name of the screen")]
        [ValidateNotNullOrEmpty]
        public string ScreenName { get; set; }

        /// <summary>
        /// Gets or sets the YAML content for the screen (without header - will be added automatically).
        /// </summary>
        [Parameter(Mandatory = true, Position = 2, ParameterSetName = "YamlContent-FromPath", HelpMessage = "YAML content for the screen (without header)")]
        [Parameter(Mandatory = true, Position = 2, ParameterSetName = "YamlContent-FromObject", HelpMessage = "YAML content for the screen (without header)")]
        [ValidateNotNullOrEmpty]
        public string YamlContent { get; set; }

        /// <summary>
        /// Gets or sets the path to a YAML file containing the screen definition (without header).
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "YamlFile-FromPath", HelpMessage = "Path to a YAML file containing the screen definition (without header)")]
        [Parameter(Mandatory = true, ParameterSetName = "YamlFile-FromObject", HelpMessage = "Path to a YAML file containing the screen definition (without header)")]
        [ValidateNotNullOrEmpty]
        public string YamlFilePath { get; set; }

        /// <summary>
        /// Gets or sets optional paths to .msapp files used as template sources during pack.
        /// Controls in these msapps provide prototype rules, ControlPropertyState, and metadata
        /// for newly-created controls. Useful when building a new msapp from YAML only —
        /// supply the reference msapp so that all control types resolve with full fidelity.
        /// </summary>
        [Parameter(HelpMessage = "Optional .msapp file(s) used as template sources for control prototypes during pack")]
        public string[] TemplateSourceMsAppPath { get; set; }

        /// <inheritdoc />
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            WriteWarning("YAML-first Canvas app modification is experimental. The Power Apps MSAPP format it not fully understood/supported and the results may need to be validated in Power Apps Studio.");

            bool isFromObject = ParameterSetName.Contains("FromObject");
            string targetPath = null;
            string tempMsappPath = null;

            try
            {
                // Get YAML content
                string yaml;
                if (!string.IsNullOrEmpty(YamlFilePath))
                {
                    var yamlPath = GetUnresolvedProviderPathFromPSPath(YamlFilePath);
                    if (!File.Exists(yamlPath))
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new FileNotFoundException($"YAML file not found: {yamlPath}"),
                            "FileNotFound",
                            ErrorCategory.ObjectNotFound,
                            yamlPath));
                        return;
                    }
                    yaml = File.ReadAllText(yamlPath);
                }
                else
                {
                    yaml = YamlContent;
                }

                // Add header to YAML content
                string fullYaml = AddScreenHeader(yaml, ScreenName);

                if (isFromObject)
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
                    byte[] msappBytes = Convert.FromBase64String(base64Document);

                    // Write to temp file
                    tempMsappPath = Path.Combine(Path.GetTempPath(), $"msapp_{Guid.NewGuid():N}.msapp");
                    File.WriteAllBytes(tempMsappPath, msappBytes);
                    targetPath = tempMsappPath;
                }
                else
                {
                    targetPath = GetUnresolvedProviderPathFromPSPath(MsAppPath);
                    if (!File.Exists(targetPath))
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new FileNotFoundException($"MsApp file not found: {targetPath}"),
                            "FileNotFound",
                            ErrorCategory.ObjectNotFound,
                            targetPath));
                        return;
                    }
                }

                string action = isFromObject 
                    ? $"Set screen '{ScreenName}' in Canvas app" 
                    : $"Set screen '{ScreenName}' in .msapp file '{targetPath}'";
                
                if (!ShouldProcess(action, action, "Set Screen"))
                {
                    return;
                }

                WriteVerbose("Using YAML-first packaging to update msapp with automatic Controls JSON generation");

                // Resolve template source paths
                string[]? resolvedTemplatePaths = null;
                if (TemplateSourceMsAppPath is not null && TemplateSourceMsAppPath.Length > 0)
                {
                    resolvedTemplatePaths = new string[TemplateSourceMsAppPath.Length];
                    for (int i = 0; i < TemplateSourceMsAppPath.Length; i++)
                    {
                        resolvedTemplatePaths[i] = GetUnresolvedProviderPathFromPSPath(TemplateSourceMsAppPath[i]);
                    }
                }

                // Use MsAppPackagingHelper to modify the msapp
                MsAppPackagingHelper.ModifyMsApp(targetPath, unpackDir =>
                {
                    // Update Src/{ScreenName}.pa.yaml
                    var screenYamlPath = Path.Combine(unpackDir, "Src", $"{ScreenName}.pa.yaml");
                    // Handle Windows-style paths in zip
                    if (!File.Exists(screenYamlPath))
                    {
                        screenYamlPath = Path.Combine(unpackDir, $"Src\\{ScreenName}.pa.yaml");
                    }

                    // Check if this is a new screen
                    bool isNewScreen = !File.Exists(screenYamlPath);
                    if (isNewScreen)
                    {
                        WriteVerbose($"Creating new screen '{ScreenName}'");
                        // Ensure Src directory exists
                        Directory.CreateDirectory(Path.Combine(unpackDir, "Src"));
                    }
                    else
                    {
                        WriteVerbose($"Updating existing screen '{ScreenName}'");
                    }

                    File.WriteAllText(screenYamlPath, fullYaml, Utf8WithoutBom);
                    WriteVerbose($"Updated {screenYamlPath}");
                }, additionalTemplateSourcePaths: resolvedTemplatePaths);

                if (isFromObject)
                {
                    // Read back and update the PSObject
                    byte[] modifiedBytes = File.ReadAllBytes(tempMsappPath);
                    string base64 = Convert.ToBase64String(modifiedBytes);
                    CanvasApp.Properties["document"].Value = base64;
                    WriteVerbose($"Screen '{ScreenName}' set successfully in Canvas app object with regenerated Controls JSON");
                }
                else
                {
                    WriteVerbose($"Screen '{ScreenName}' set successfully in .msapp file with regenerated Controls JSON");
                }
            }
            finally
            {
                if (tempMsappPath != null && File.Exists(tempMsappPath))
                {
                    try
                    {
                        File.Delete(tempMsappPath);
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }
                }
            }
        }

        /// <summary>
        /// Adds the screen header using SharpYaml to properly serialize the YAML structure.
        /// Transforms the provided content into: "Screens:\n  ScreenName:\n    ..."
        /// </summary>
        private string AddScreenHeader(string yamlContent, string screenName)
        {
            try
            {
                // Parse the screen content YAML
                var screenContent = YamlSerializer.Deserialize<object>(yamlContent);

                // Create the full structure: Screens -> ScreenName -> content
                var fullStructure = new Dictionary<string, object?>
                {
                    ["Screens"] = new Dictionary<string, object?>
                    {
                        [screenName] = screenContent
                    }
                };

                // Serialize with proper indentation
                var result = YamlSerializer.Serialize(fullStructure);
                return result;
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Failed to parse YAML content: {ex.Message}", ex),
                    "YamlParseError",
                    ErrorCategory.InvalidData,
                    yamlContent));
                return null; // Never reached due to ThrowTerminatingError
            }
        }
    }
}
