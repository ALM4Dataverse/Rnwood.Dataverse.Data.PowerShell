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
    /// Removes a custom action from a Dataverse ribbon (entity-specific or application-wide).
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseRibbonCustomAction", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(void))]
    public class RemoveDataverseRibbonCustomActionCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the logical name of the entity/table for which to remove the ribbon custom action.
        /// If not specified, removes from the application-wide ribbon.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the entity/table. If not specified, removes from application-wide ribbon.")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("EntityName", "TableName")]
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the ID of the custom action to remove.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the custom action to remove")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets whether to publish the customizations after removing the custom action.
        /// </summary>
        [Parameter(HelpMessage = "Publish the customizations after removing the custom action")]
        public SwitchParameter Publish { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            string target = string.IsNullOrEmpty(Entity) 
                ? $"custom action '{Id}' from application-wide ribbon" 
                : $"custom action '{Id}' from ribbon for entity '{Entity}'";
                
            if (!ShouldProcess(target, "Remove"))
            {
                return;
            }

            WriteVerbose($"Removing custom action '{Id}' from ribbon");
            RemoveCustomAction();
            WriteVerbose("Successfully removed custom action");

            if (Publish.IsPresent)
            {
                WriteVerbose("Publishing customizations");
                PublishCustomizations();
                WriteVerbose("Customizations published successfully");
            }
        }

        private void RemoveCustomAction()
        {
            try
            {
                // Retrieve current ribbon
                string ribbonDiffXml = RetrieveRibbon();
                
                if (string.IsNullOrEmpty(ribbonDiffXml))
                {
                    WriteWarning("No ribbon customizations found");
                    return;
                }

                XDocument doc = XDocument.Parse(ribbonDiffXml);
                XNamespace ns = doc.Root.Name.Namespace;
                
                var customActionsElement = doc.Root.Element(ns + "CustomActions");
                if (customActionsElement == null)
                {
                    WriteWarning("No CustomActions element found in ribbon");
                    return;
                }

                // Find and remove the custom action
                var customActionToRemove = customActionsElement.Elements(ns + "CustomAction")
                    .FirstOrDefault(ca => ca.Attribute("Id")?.Value == Id);

                if (customActionToRemove == null)
                {
                    WriteWarning($"Custom action '{Id}' not found in ribbon");
                    return;
                }

                customActionToRemove.Remove();
                WriteVerbose($"Removed custom action '{Id}' from ribbon XML");

                // Save the updated ribbon
                SaveRibbon(doc.ToString());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to remove custom action: {ex.Message}", ex);
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
                
                if (results.Entities.Count > 0)
                {
                    Entity ribbonDiffEntity = results.Entities[0];
                    ribbonDiffEntity["ribbondiffxml"] = ribbonDiffXml;
                    Connection.Update(ribbonDiffEntity);
                }
                else
                {
                    WriteWarning("No existing ribbon diff found for entity");
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
                
                if (results.Entities.Count > 0)
                {
                    Entity ribbonDiffEntity = results.Entities[0];
                    ribbonDiffEntity["ribbondiffxml"] = ribbonDiffXml;
                    Connection.Update(ribbonDiffEntity);
                }
                else
                {
                    WriteWarning("No existing ribbon diff found for application");
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
