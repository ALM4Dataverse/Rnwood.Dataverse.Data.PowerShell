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
    /// Removes a command definition from a Dataverse ribbon (entity-specific or application-wide).
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseRibbonCommandDefinition", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(void))]
    public class RemoveDataverseRibbonCommandDefinitionCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the logical name of the entity/table for which to remove the ribbon command definition.
        /// If not specified, removes from the application-wide ribbon.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the entity/table. If not specified, removes from application-wide ribbon.")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("EntityName", "TableName")]
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the ID of the command definition to remove.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the command definition to remove")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets whether to publish the customizations after removing the command definition.
        /// </summary>
        [Parameter(HelpMessage = "Publish the customizations after removing the command definition")]
        public SwitchParameter Publish { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            string target = string.IsNullOrEmpty(Entity) 
                ? $"command definition '{Id}' from application-wide ribbon" 
                : $"command definition '{Id}' from ribbon for entity '{Entity}'";
                
            if (!ShouldProcess(target, "Remove"))
            {
                return;
            }

            WriteVerbose($"Removing command definition '{Id}' from ribbon");
            RemoveCommandDefinition();
            WriteVerbose("Successfully removed command definition");

            if (Publish.IsPresent)
            {
                WriteVerbose("Publishing customizations");
                PublishCustomizations();
                WriteVerbose("Customizations published successfully");
            }
        }

        private void RemoveCommandDefinition()
        {
            try
            {
                string ribbonDiffXml = RetrieveRibbon();
                
                if (string.IsNullOrEmpty(ribbonDiffXml))
                {
                    WriteWarning("No ribbon customizations found");
                    return;
                }

                XDocument doc = XDocument.Parse(ribbonDiffXml);
                XNamespace ns = doc.Root.Name.Namespace;
                
                var commandDefinitionsElement = doc.Root.Element(ns + "CommandDefinitions");
                if (commandDefinitionsElement == null)
                {
                    WriteWarning("No CommandDefinitions element found in ribbon");
                    return;
                }

                var commandDefToRemove = commandDefinitionsElement.Elements(ns + "CommandDefinition")
                    .FirstOrDefault(cd => cd.Attribute("Id")?.Value == Id);

                if (commandDefToRemove == null)
                {
                    WriteWarning($"Command definition '{Id}' not found in ribbon");
                    return;
                }

                commandDefToRemove.Remove();
                WriteVerbose($"Removed command definition '{Id}' from ribbon XML");

                SaveRibbon(doc.ToString());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to remove command definition: {ex.Message}", ex);
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
