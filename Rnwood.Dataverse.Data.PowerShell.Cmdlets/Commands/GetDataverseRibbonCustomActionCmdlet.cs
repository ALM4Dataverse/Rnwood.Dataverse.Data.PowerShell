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
    /// Retrieves custom actions from a Dataverse ribbon (entity-specific or application-wide).
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseRibbonCustomAction")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseRibbonCustomActionCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the logical name of the entity/table for which to retrieve ribbon custom actions.
        /// If not specified, retrieves from the application-wide ribbon.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the entity/table. If not specified, retrieves from application-wide ribbon.")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("EntityName", "TableName")]
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the ID of a specific custom action to retrieve.
        /// </summary>
        [Parameter(HelpMessage = "ID of a specific custom action to retrieve")]
        public string CustomActionId { get; set; }

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
            
            var customActionsElement = doc.Root.Element(ns + "CustomActions");
            if (customActionsElement == null)
            {
                WriteVerbose("No CustomActions element found in ribbon");
                return;
            }

            var customActions = customActionsElement.Elements(ns + "CustomAction");
            
            if (!string.IsNullOrEmpty(CustomActionId))
            {
                customActions = customActions.Where(ca => ca.Attribute("Id")?.Value == CustomActionId);
            }

            foreach (var customAction in customActions)
            {
                PSObject output = new PSObject();
                output.Properties.Add(new PSNoteProperty("Entity", Entity));
                output.Properties.Add(new PSNoteProperty("Id", customAction.Attribute("Id")?.Value));
                output.Properties.Add(new PSNoteProperty("Location", customAction.Attribute("Location")?.Value));
                output.Properties.Add(new PSNoteProperty("Sequence", customAction.Attribute("Sequence")?.Value));
                output.Properties.Add(new PSNoteProperty("Title", customAction.Attribute("Title")?.Value));
                
                // Get CommandUIDefinition
                var commandUIDef = customAction.Element(ns + "CommandUIDefinition");
                if (commandUIDef != null)
                {
                    // Get the first child element (Button, CheckBox, etc.)
                    var control = commandUIDef.Elements().FirstOrDefault();
                    if (control != null)
                    {
                        output.Properties.Add(new PSNoteProperty("ControlType", control.Name.LocalName));
                        output.Properties.Add(new PSNoteProperty("ControlId", control.Attribute("Id")?.Value));
                        output.Properties.Add(new PSNoteProperty("Command", control.Attribute("Command")?.Value));
                        output.Properties.Add(new PSNoteProperty("LabelText", control.Attribute("LabelText")?.Value));
                    }
                }
                
                output.Properties.Add(new PSNoteProperty("Xml", customAction.ToString()));
                
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
