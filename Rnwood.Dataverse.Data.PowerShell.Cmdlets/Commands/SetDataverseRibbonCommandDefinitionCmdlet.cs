using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;
using System.Security;
using System.Text;
using System.Xml.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates a command definition in a Dataverse ribbon (entity-specific or application-wide).
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseRibbonCommandDefinition", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(void))]
    public class SetDataverseRibbonCommandDefinitionCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the logical name of the entity/table for which to set the ribbon command definition.
        /// If not specified, sets in the application-wide ribbon.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the entity/table. If not specified, sets in application-wide ribbon.")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("EntityName", "TableName")]
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the ID of the command definition.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the command definition")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the XML content for the command definition (EnableRules, DisplayRules, Actions elements).
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "XML content for the command definition (e.g., <EnableRules>...</EnableRules><DisplayRules>...</DisplayRules><Actions>...</Actions>)")]
        public string CommandDefinitionXml { get; set; }

        /// <summary>
        /// Gets or sets whether to publish the customizations after setting the command definition.
        /// </summary>
        [Parameter(HelpMessage = "Publish the customizations after setting the command definition")]
        public SwitchParameter Publish { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            string target = string.IsNullOrEmpty(Entity) 
                ? "command definition in application-wide ribbon" 
                : $"command definition in ribbon for entity '{Entity}'";
                
            if (!ShouldProcess(target, "Update"))
            {
                return;
            }

            WriteVerbose($"Setting command definition '{Id}' in ribbon");
            SetCommandDefinition();
            WriteVerbose("Successfully set command definition");

            if (Publish.IsPresent)
            {
                WriteVerbose("Publishing customizations");
                PublishCustomizations();
                WriteVerbose("Customizations published successfully");
            }
        }

        private void SetCommandDefinition()
        {
            try
            {
                // Retrieve current ribbon
                string ribbonDiffXml = RetrieveRibbon();
                XDocument doc;
                
                if (string.IsNullOrEmpty(ribbonDiffXml))
                {
                    doc = new XDocument(new XElement("RibbonDiffXml"));
                }
                else
                {
                    doc = XDocument.Parse(ribbonDiffXml);
                }

                XNamespace ns = doc.Root.Name.Namespace;
                
                // Ensure CommandDefinitions element exists
                var commandDefinitionsElement = doc.Root.Element(ns + "CommandDefinitions");
                if (commandDefinitionsElement == null)
                {
                    commandDefinitionsElement = new XElement(ns + "CommandDefinitions");
                    // Insert after CustomActions if it exists, otherwise as first element
                    var customActions = doc.Root.Element(ns + "CustomActions");
                    if (customActions != null)
                    {
                        customActions.AddAfterSelf(commandDefinitionsElement);
                    }
                    else
                    {
                        doc.Root.AddFirst(commandDefinitionsElement);
                    }
                }

                // Find existing command definition or create new
                var existingCommand = commandDefinitionsElement.Elements(ns + "CommandDefinition")
                    .FirstOrDefault(cd => cd.Attribute("Id")?.Value == Id);

                XElement commandDefElement;
                if (existingCommand != null)
                {
                    commandDefElement = existingCommand;
                    commandDefElement.RemoveAll();
                    WriteVerbose($"Updating existing command definition '{Id}'");
                }
                else
                {
                    commandDefElement = new XElement(ns + "CommandDefinition");
                    commandDefinitionsElement.Add(commandDefElement);
                    WriteVerbose($"Creating new command definition '{Id}'");
                }

                // Set attributes
                commandDefElement.SetAttributeValue("Id", Id);

                // Parse and add the provided XML content
                XElement contentElement = XElement.Parse($"<root>{CommandDefinitionXml}</root>");
                foreach (var child in contentElement.Elements())
                {
                    commandDefElement.Add(child);
                }

                // Save the updated ribbon
                SaveRibbon(doc.ToString());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set command definition: {ex.Message}", ex);
            }
        }

        private string RetrieveRibbon()
        {
            if (!string.IsNullOrEmpty(Entity))
            {
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
                RetrieveApplicationRibbonRequest request = new RetrieveApplicationRibbonRequest();
                RetrieveApplicationRibbonResponse response = (RetrieveApplicationRibbonResponse)Connection.Execute(request);
                return DecompressXml(response.CompressedApplicationRibbonXml);
            }
        }

        private void SaveRibbon(string ribbonDiffXml)
        {
            WriteWarning("Setting ribbon customizations requires solution import/export workflow for production scenarios.");

            if (!string.IsNullOrEmpty(Entity))
            {
                QueryExpression query = new QueryExpression("ribbondiff")
                {
                    ColumnSet = new ColumnSet("ribbondiffid"),
                    Criteria = new FilterExpression
                    {
                        Conditions = { new ConditionExpression("entity", ConditionOperator.Equal, Entity) }
                    },
                    TopCount = 1
                };

                EntityCollection results = Connection.RetrieveMultiple(query);
                Entity ribbonDiffEntity;
                if (results.Entities.Count > 0)
                {
                    ribbonDiffEntity = results.Entities[0];
                    ribbonDiffEntity["ribbondiffxml"] = ribbonDiffXml;
                    Connection.Update(ribbonDiffEntity);
                }
                else
                {
                    ribbonDiffEntity = new Entity("ribbondiff");
                    ribbonDiffEntity["entity"] = Entity;
                    ribbonDiffEntity["ribbondiffxml"] = ribbonDiffXml;
                    Connection.Create(ribbonDiffEntity);
                }
            }
            else
            {
                QueryExpression query = new QueryExpression("ribbondiff")
                {
                    ColumnSet = new ColumnSet("ribbondiffid"),
                    Criteria = new FilterExpression
                    {
                        Conditions = { new ConditionExpression("entity", ConditionOperator.Null) }
                    },
                    TopCount = 1
                };

                EntityCollection results = Connection.RetrieveMultiple(query);
                Entity ribbonDiffEntity;
                if (results.Entities.Count > 0)
                {
                    ribbonDiffEntity = results.Entities[0];
                    ribbonDiffEntity["ribbondiffxml"] = ribbonDiffXml;
                    Connection.Update(ribbonDiffEntity);
                }
                else
                {
                    ribbonDiffEntity = new Entity("ribbondiff");
                    ribbonDiffEntity["ribbondiffxml"] = ribbonDiffXml;
                    Connection.Create(ribbonDiffEntity);
                }
            }
        }

        private void PublishCustomizations()
        {
            if (!string.IsNullOrEmpty(Entity))
            {
                string parameterXml = $"<importexportxml><entities><entity>{SecurityElement.Escape(Entity)}</entity></entities></importexportxml>";
                PublishXmlRequest publishRequest = new PublishXmlRequest { ParameterXml = parameterXml };
                Connection.Execute(publishRequest);
            }
            else
            {
                PublishAllXmlRequest publishRequest = new PublishAllXmlRequest();
                Connection.Execute(publishRequest);
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
