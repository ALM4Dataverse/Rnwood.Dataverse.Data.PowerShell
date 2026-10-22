using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Xml.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves command definitions from a Dataverse ribbon (entity-specific or application-wide).
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseRibbonCommandDefinition")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseRibbonCommandDefinitionCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the logical name of the entity/table for which to retrieve ribbon command definitions.
        /// If not specified, retrieves from the application-wide ribbon.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the entity/table. If not specified, retrieves from application-wide ribbon.")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("EntityName", "TableName")]
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the ID of a specific command definition to retrieve.
        /// </summary>
        [Parameter(HelpMessage = "ID of a specific command definition to retrieve")]
        public string CommandId { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Retrieve the ribbon
            string ribbonDiffXml = RetrieveRibbon();
            
            if (string.IsNullOrEmpty(ribbonDiffXml))
            {
                WriteVerbose("No ribbon customizations found");
                return;
            }

            // Parse the XML
            XDocument doc = XDocument.Parse(ribbonDiffXml);
            XNamespace ns = doc.Root.Name.Namespace;
            
            var commandDefinitionsElement = doc.Root.Element(ns + "CommandDefinitions");
            if (commandDefinitionsElement == null)
            {
                WriteVerbose("No CommandDefinitions element found in ribbon");
                return;
            }

            var commandDefinitions = commandDefinitionsElement.Elements(ns + "CommandDefinition");
            
            if (!string.IsNullOrEmpty(CommandId))
            {
                commandDefinitions = commandDefinitions.Where(cd => cd.Attribute("Id")?.Value == CommandId);
            }

            foreach (var commandDef in commandDefinitions)
            {
                PSObject output = new PSObject();
                output.Properties.Add(new PSNoteProperty("Entity", Entity));
                output.Properties.Add(new PSNoteProperty("Id", commandDef.Attribute("Id")?.Value));
                
                // Get EnableRules
                var enableRulesElement = commandDef.Element(ns + "EnableRules");
                if (enableRulesElement != null)
                {
                    var enableRules = enableRulesElement.Elements(ns + "EnableRule")
                        .Select(er => er.Attribute("Id")?.Value)
                        .Where(id => !string.IsNullOrEmpty(id))
                        .ToArray();
                    output.Properties.Add(new PSNoteProperty("EnableRules", enableRules));
                }
                
                // Get DisplayRules
                var displayRulesElement = commandDef.Element(ns + "DisplayRules");
                if (displayRulesElement != null)
                {
                    var displayRules = displayRulesElement.Elements(ns + "DisplayRule")
                        .Select(dr => dr.Attribute("Id")?.Value)
                        .Where(id => !string.IsNullOrEmpty(id))
                        .ToArray();
                    output.Properties.Add(new PSNoteProperty("DisplayRules", displayRules));
                }
                
                // Get Actions
                var actionsElement = commandDef.Element(ns + "Actions");
                if (actionsElement != null)
                {
                    var actions = actionsElement.Elements()
                        .Select(a => new { Type = a.Name.LocalName, Library = a.Attribute("Library")?.Value, FunctionName = a.Attribute("FunctionName")?.Value })
                        .ToArray();
                    output.Properties.Add(new PSNoteProperty("Actions", actions));
                }
                
                output.Properties.Add(new PSNoteProperty("Xml", commandDef.ToString()));
                
                WriteObject(output);
            }
        }

        private string RetrieveRibbon()
        {
            if (!string.IsNullOrEmpty(Entity))
            {
                // Retrieve entity-specific ribbon
                WriteVerbose($"Retrieving ribbon for entity: {Entity}");
                RetrieveEntityRibbonRequest request = new RetrieveEntityRibbonRequest
                {
                    EntityName = Entity,
                    RibbonLocationFilter = RibbonLocationFilters.All
                };

                RetrieveEntityRibbonResponse response = (RetrieveEntityRibbonResponse)Connection.Execute(request);
                return DecompressXml(response.CompressedEntityXml);
            }
            else
            {
                // Retrieve application-wide ribbon
                WriteVerbose("Retrieving application-wide ribbon");
                RetrieveApplicationRibbonRequest request = new RetrieveApplicationRibbonRequest();
                RetrieveApplicationRibbonResponse response = (RetrieveApplicationRibbonResponse)Connection.Execute(request);
                return DecompressXml(response.CompressedApplicationRibbonXml);
            }
        }

        private string DecompressXml(byte[] compressedData)
        {
            if (compressedData == null || compressedData.Length == 0)
                return null;

            using (MemoryStream memoryStream = new MemoryStream(compressedData))
            using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
            using (StreamReader reader = new StreamReader(gzipStream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
