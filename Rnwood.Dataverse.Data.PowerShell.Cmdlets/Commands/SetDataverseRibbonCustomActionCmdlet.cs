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
    /// Creates or updates a custom action in a Dataverse ribbon (entity-specific or application-wide).
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseRibbonCustomAction", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(void))]
    public class SetDataverseRibbonCustomActionCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the logical name of the entity/table for which to set the ribbon custom action.
        /// If not specified, sets in the application-wide ribbon.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the entity/table. If not specified, sets in application-wide ribbon.")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("EntityName", "TableName")]
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the ID of the custom action.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the custom action")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the location where the custom action should appear.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Location where the custom action should appear (e.g., Mscrm.HomepageGrid.contact.MainTab.Actions.Controls._children)")]
        public string Location { get; set; }

        /// <summary>
        /// Gets or sets the sequence number that determines the order of the custom action.
        /// </summary>
        [Parameter(HelpMessage = "Sequence number (default: 100)")]
        public int Sequence { get; set; } = 100;

        /// <summary>
        /// Gets or sets the title of the custom action.
        /// </summary>
        [Parameter(HelpMessage = "Title of the custom action")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the XML content for the CommandUIDefinition.
        /// </summary>
        [Parameter(Mandatory = true, HelpMessage = "XML content for the CommandUIDefinition element (e.g., <Button Id='MyBtn' Command='MyCmd' ... />)")]
        public string CommandUIDefinitionXml { get; set; }

        /// <summary>
        /// Gets or sets whether to publish the customizations after setting the custom action.
        /// </summary>
        [Parameter(HelpMessage = "Publish the customizations after setting the custom action")]
        public SwitchParameter Publish { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            string target = string.IsNullOrEmpty(Entity) 
                ? "custom action in application-wide ribbon" 
                : $"custom action in ribbon for entity '{Entity}'";
                
            if (!ShouldProcess(target, "Update"))
            {
                return;
            }

            WriteVerbose($"Setting custom action '{Id}' in ribbon");
            SetCustomAction();
            WriteVerbose("Successfully set custom action");

            if (Publish.IsPresent)
            {
                WriteVerbose("Publishing customizations");
                PublishCustomizations();
                WriteVerbose("Customizations published successfully");
            }
        }

        private void SetCustomAction()
        {
            try
            {
                // Retrieve current ribbon
                string ribbonDiffXml = RetrieveRibbon();
                XDocument doc;
                
                if (string.IsNullOrEmpty(ribbonDiffXml))
                {
                    // Create new ribbon document
                    doc = new XDocument(new XElement("RibbonDiffXml"));
                }
                else
                {
                    doc = XDocument.Parse(ribbonDiffXml);
                }

                XNamespace ns = doc.Root.Name.Namespace;
                
                // Ensure CustomActions element exists
                var customActionsElement = doc.Root.Element(ns + "CustomActions");
                if (customActionsElement == null)
                {
                    customActionsElement = new XElement(ns + "CustomActions");
                    // Insert CustomActions as first element
                    doc.Root.AddFirst(customActionsElement);
                }

                // Find existing custom action or create new
                var existingAction = customActionsElement.Elements(ns + "CustomAction")
                    .FirstOrDefault(ca => ca.Attribute("Id")?.Value == Id);

                XElement customActionElement;
                if (existingAction != null)
                {
                    // Update existing
                    customActionElement = existingAction;
                    WriteVerbose($"Updating existing custom action '{Id}'");
                }
                else
                {
                    // Create new
                    customActionElement = new XElement(ns + "CustomAction");
                    customActionsElement.Add(customActionElement);
                    WriteVerbose($"Creating new custom action '{Id}'");
                }

                // Set attributes
                customActionElement.SetAttributeValue("Id", Id);
                customActionElement.SetAttributeValue("Location", Location);
                customActionElement.SetAttributeValue("Sequence", Sequence);
                if (!string.IsNullOrEmpty(Title))
                {
                    customActionElement.SetAttributeValue("Title", Title);
                }

                // Set CommandUIDefinition
                var commandUIDefElement = customActionElement.Element(ns + "CommandUIDefinition");
                if (commandUIDefElement == null)
                {
                    commandUIDefElement = new XElement(ns + "CommandUIDefinition");
                    customActionElement.Add(commandUIDefElement);
                }
                
                // Parse and add the provided XML
                XElement commandUIContent = XElement.Parse(CommandUIDefinitionXml);
                commandUIDefElement.RemoveAll();
                commandUIDefElement.Add(commandUIContent);

                // Save the updated ribbon
                SaveRibbon(doc.ToString());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set custom action: {ex.Message}", ex);
            }
        }

        private string RetrieveRibbon()
        {
            if (!string.IsNullOrEmpty(Entity))
            {
                // Retrieve entity-specific ribbon
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
                // Save entity ribbon
                QueryExpression query = new QueryExpression("ribbondiff")
                {
                    ColumnSet = new ColumnSet("ribbondiffid"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("entity", ConditionOperator.Equal, Entity)
                        }
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
                // Save application ribbon
                QueryExpression query = new QueryExpression("ribbondiff")
                {
                    ColumnSet = new ColumnSet("ribbondiffid"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("entity", ConditionOperator.Null)
                        }
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
                PublishXmlRequest publishRequest = new PublishXmlRequest
                {
                    ParameterXml = parameterXml
                };
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
