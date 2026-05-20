using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System;
using System.IO;
using System.IO.Compression;
using System.Management.Automation;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves ribbon customizations from a Dataverse environment. Ribbons can be retrieved for specific entities or for the application-wide ribbon.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseRibbon")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseRibbonCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the logical name of the entity/table for which to retrieve the ribbon.
        /// If not specified, retrieves the application-wide ribbon.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the entity/table for which to retrieve the ribbon. If not specified, retrieves the application-wide ribbon.")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("EntityName", "TableName")]
        public string Entity { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            string ribbonDiffXml = null;

            if (!string.IsNullOrEmpty(Entity))
            {
                // Retrieve entity-specific ribbon
                WriteVerbose($"Retrieving ribbon for entity: {Entity}");
                ribbonDiffXml = RetrieveEntityRibbon(Entity);
                WriteVerbose($"Retrieved entity ribbon, length: {ribbonDiffXml?.Length ?? 0}");
            }
            else
            {
                // Retrieve application-wide ribbon
                WriteVerbose("Retrieving application-wide ribbon");
                ribbonDiffXml = RetrieveApplicationRibbon();
                WriteVerbose($"Retrieved application ribbon, length: {ribbonDiffXml?.Length ?? 0}");
            }

            // Create output object
            PSObject output = new PSObject();
            output.Properties.Add(new PSNoteProperty("Entity", Entity));
            output.Properties.Add(new PSNoteProperty("RibbonDiffXml", ribbonDiffXml));
            output.Properties.Add(new PSNoteProperty("IsApplicationRibbon", string.IsNullOrEmpty(Entity)));

            WriteObject(output);
        }

        private string RetrieveEntityRibbon(string entityLogicalName)
        {
            try
            {
                RetrieveEntityRibbonRequest request = new RetrieveEntityRibbonRequest
                {
                    EntityName = entityLogicalName,
                    RibbonLocationFilter = RibbonLocationFilters.All
                };

                RetrieveEntityRibbonResponse response = (RetrieveEntityRibbonResponse)Connection.Execute(request);
                
                if (response?.CompressedEntityXml == null)
                {
                    WriteWarning($"Entity '{entityLogicalName}' not found or has no ribbon customizations.");
                    return null;
                }

                // Decompress the XML
                return DecompressXml(response.CompressedEntityXml);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve ribbon for entity '{entityLogicalName}': {ex.Message}", ex);
            }
        }

        private string RetrieveApplicationRibbon()
        {
            try
            {
                RetrieveApplicationRibbonRequest request = new RetrieveApplicationRibbonRequest();
                RetrieveApplicationRibbonResponse response = (RetrieveApplicationRibbonResponse)Connection.Execute(request);
                
                if (response?.CompressedApplicationRibbonXml == null)
                {
                    WriteWarning("No application ribbon found.");
                    return null;
                }

                // Decompress the XML
                return DecompressXml(response.CompressedApplicationRibbonXml);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve application ribbon: {ex.Message}", ex);
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
