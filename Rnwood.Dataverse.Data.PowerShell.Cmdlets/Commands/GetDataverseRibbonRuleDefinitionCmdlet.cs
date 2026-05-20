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
    /// Retrieves rule definitions from a Dataverse ribbon (entity-specific or application-wide).
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseRibbonRuleDefinition")]
    [OutputType(typeof(PSObject))]
    public class GetDataverseRibbonRuleDefinitionCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the logical name of the entity/table for which to retrieve ribbon rule definitions.
        /// If not specified, retrieves from the application-wide ribbon.
        /// </summary>
        [Parameter(Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the entity/table. If not specified, retrieves from application-wide ribbon.")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Alias("EntityName", "TableName")]
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the ID of a specific rule definition to retrieve.
        /// </summary>
        [Parameter(HelpMessage = "ID of a specific rule definition to retrieve")]
        public string RuleId { get; set; }

        /// <summary>
        /// Gets or sets the type of rules to retrieve (EnableRule or DisplayRule).
        /// </summary>
        [Parameter(HelpMessage = "Type of rules to retrieve (EnableRule or DisplayRule)")]
        [ValidateSet("EnableRule", "DisplayRule")]
        public string RuleType { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            string ribbonDiffXml = RetrieveRibbon();
            
            if (string.IsNullOrEmpty(ribbonDiffXml))
            {
                WriteVerbose("No ribbon customizations found");
                return;
            }

            XDocument doc = XDocument.Parse(ribbonDiffXml);
            XNamespace ns = doc.Root.Name.Namespace;
            
            var ruleDefinitionsElement = doc.Root.Element(ns + "RuleDefinitions");
            if (ruleDefinitionsElement == null)
            {
                WriteVerbose("No RuleDefinitions element found in ribbon");
                return;
            }

            // Process EnableRules if no type specified or EnableRule requested
            if (string.IsNullOrEmpty(RuleType) || RuleType == "EnableRule")
            {
                var enableRulesElement = ruleDefinitionsElement.Element(ns + "EnableRules");
                if (enableRulesElement != null)
                {
                    ProcessRules(enableRulesElement, ns + "EnableRule", "EnableRule");
                }
            }

            // Process DisplayRules if no type specified or DisplayRule requested
            if (string.IsNullOrEmpty(RuleType) || RuleType == "DisplayRule")
            {
                var displayRulesElement = ruleDefinitionsElement.Element(ns + "DisplayRules");
                if (displayRulesElement != null)
                {
                    ProcessRules(displayRulesElement, ns + "DisplayRule", "DisplayRule");
                }
            }
        }

        private void ProcessRules(XElement rulesContainer, XName ruleName, string ruleType)
        {
            var rules = rulesContainer.Elements(ruleName);
            
            if (!string.IsNullOrEmpty(RuleId))
            {
                rules = rules.Where(r => r.Attribute("Id")?.Value == RuleId);
            }

            foreach (var rule in rules)
            {
                PSObject output = new PSObject();
                output.Properties.Add(new PSNoteProperty("Entity", Entity));
                output.Properties.Add(new PSNoteProperty("RuleType", ruleType));
                output.Properties.Add(new PSNoteProperty("Id", rule.Attribute("Id")?.Value));
                
                // Get all child elements (the actual rule conditions)
                var conditions = rule.Elements()
                    .Select(e => new { Type = e.Name.LocalName, Attributes = e.Attributes().ToDictionary(a => a.Name.LocalName, a => a.Value) })
                    .ToArray();
                output.Properties.Add(new PSNoteProperty("Conditions", conditions));
                
                output.Properties.Add(new PSNoteProperty("Xml", rule.ToString()));
                
                WriteObject(output);
            }
        }

        private string RetrieveRibbon()
        {
            if (!string.IsNullOrEmpty(Entity))
            {
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
