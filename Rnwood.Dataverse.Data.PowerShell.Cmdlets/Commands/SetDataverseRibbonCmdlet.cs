using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IO;
using System.IO.Compression;
using System.Management.Automation;
using System.Security;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates ribbon customizations in a Dataverse environment. Ribbons can be set for specific entities or for the application-wide ribbon.
    /// Note: Ribbon customizations typically require exporting/modifying/importing solutions. This cmdlet provides a simplified interface but may have limitations.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseRibbon", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    [OutputType(typeof(void))]
    public class SetDataverseRibbonCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the logical name of the entity/table for which to set the ribbon.
        /// If not specified, sets the application-wide ribbon.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the entity/table for which to set the ribbon. If not specified, sets the application-wide ribbon.")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("EntityName", "TableName")]
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the RibbonDiffXml content. This must be valid XML conforming to the Dataverse Ribbon schema.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "RibbonDiffXml content. Must be valid XML conforming to the Dataverse Ribbon schema.")]
        [Alias("RibbonXml", "Xml")]
        public string RibbonDiffXml { get; set; }

        /// <summary>
        /// Gets or sets whether to publish the customizations after setting the ribbon.
        /// </summary>
        [Parameter(HelpMessage = "Publish the customizations after setting the ribbon")]
        public SwitchParameter Publish { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Validate RibbonDiffXml is provided
            if (string.IsNullOrWhiteSpace(RibbonDiffXml))
            {
                throw new ArgumentException("RibbonDiffXml cannot be null or empty.", nameof(RibbonDiffXml));
            }

            if (!string.IsNullOrEmpty(Entity))
            {
                // Set entity-specific ribbon
                string target = $"ribbon for entity '{Entity}'";
                if (!ShouldProcess(target, "Update"))
                {
                    return;
                }

                WriteVerbose($"Setting ribbon for entity: {Entity}");
                SetEntityRibbon(Entity, RibbonDiffXml);
                WriteVerbose($"Successfully set entity ribbon");

                if (Publish.IsPresent)
                {
                    WriteVerbose($"Publishing customizations for entity: {Entity}");
                    PublishEntity(Entity);
                    WriteVerbose("Customizations published successfully");
                }
            }
            else
            {
                // Set application-wide ribbon
                string target = "application-wide ribbon";
                if (!ShouldProcess(target, "Update"))
                {
                    return;
                }

                WriteVerbose("Setting application-wide ribbon");
                SetApplicationRibbon(RibbonDiffXml);
                WriteVerbose("Successfully set application ribbon");

                if (Publish.IsPresent)
                {
                    WriteVerbose("Publishing application customizations");
                    PublishAllXml();
                    WriteVerbose("Customizations published successfully");
                }
            }
        }

        private void SetEntityRibbon(string entityLogicalName, string ribbonDiffXml)
        {
            try
            {
                // Create/update the ribboncommand solution component for entity
                // We'll store the ribbon diff in the ribbondiff table
                WriteWarning("Setting entity ribbons requires solution import/export workflow. " +
                            "Consider using solution export, modification, and import for production scenarios.");

                // Query for existing ribbon diff for this entity
                QueryExpression query = new QueryExpression("ribbondiff")
                {
                    ColumnSet = new ColumnSet("ribbondiffid", "entity"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("entity", ConditionOperator.Equal, entityLogicalName)
                        }
                    },
                    TopCount = 1
                };

                EntityCollection results = Connection.RetrieveMultiple(query);
                
                Entity ribbonDiffEntity;
                if (results.Entities.Count > 0)
                {
                    // Update existing ribbon diff
                    ribbonDiffEntity = results.Entities[0];
                    ribbonDiffEntity["ribbondiffxml"] = ribbonDiffXml;
                    Connection.Update(ribbonDiffEntity);
                    WriteVerbose($"Updated existing ribbon diff for entity '{entityLogicalName}'");
                }
                else
                {
                    // Create new ribbon diff
                    ribbonDiffEntity = new Entity("ribbondiff");
                    ribbonDiffEntity["entity"] = entityLogicalName;
                    ribbonDiffEntity["ribbondiffxml"] = ribbonDiffXml;

                    Connection.Create(ribbonDiffEntity);
                    WriteVerbose($"Created new ribbon diff for entity '{entityLogicalName}'");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set ribbon for entity '{entityLogicalName}': {ex.Message}", ex);
            }
        }

        private void SetApplicationRibbon(string ribbonDiffXml)
        {
            try
            {
                WriteWarning("Setting application ribbons requires solution import/export workflow. " +
                            "Consider using solution export, modification, and import for production scenarios.");

                // For application-level ribbons, entity field is null
                QueryExpression query = new QueryExpression("ribbondiff")
                {
                    ColumnSet = new ColumnSet("ribbondiffid", "entity"),
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
                    // Update existing application ribbon diff
                    ribbonDiffEntity = results.Entities[0];
                    ribbonDiffEntity["ribbondiffxml"] = ribbonDiffXml;
                    Connection.Update(ribbonDiffEntity);
                    WriteVerbose("Updated existing application ribbon diff");
                }
                else
                {
                    // Create new application ribbon diff
                    ribbonDiffEntity = new Entity("ribbondiff");
                    ribbonDiffEntity["ribbondiffxml"] = ribbonDiffXml;

                    Connection.Create(ribbonDiffEntity);
                    WriteVerbose("Created new application ribbon diff");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to set application ribbon: {ex.Message}", ex);
            }
        }

        private void PublishEntity(string entityLogicalName)
        {
            try
            {
                string parameterXml = $"<importexportxml><entities><entity>{SecurityElement.Escape(entityLogicalName)}</entity></entities></importexportxml>";

                PublishXmlRequest publishRequest = new PublishXmlRequest
                {
                    ParameterXml = parameterXml
                };

                Connection.Execute(publishRequest);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to publish customizations for entity '{entityLogicalName}': {ex.Message}", ex);
            }
        }

        private void PublishAllXml()
        {
            try
            {
                PublishAllXmlRequest publishRequest = new PublishAllXmlRequest();
                Connection.Execute(publishRequest);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to publish all customizations: {ex.Message}", ex);
            }
        }
    }
}
