using System;
using System.Management.Automation;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves cloud flow information from a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseCloudFlow")]
    [OutputType(typeof(CloudFlowInfo))]
    public class GetDataverseCloudFlowCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the name of the cloud flow to retrieve. Supports wildcards (* and ?).
        /// </summary>
        [Parameter(Position = 0, HelpMessage = "The name of the cloud flow to retrieve. Supports wildcards (* and ?). If not specified, all cloud flows are returned.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the ID of the cloud flow to retrieve.
        /// </summary>
        [Parameter(HelpMessage = "The ID of the cloud flow to retrieve.")]
        public Guid? Id { get; set; }

        /// <summary>
        /// Gets or sets whether to filter for activated flows only.
        /// </summary>
        [Parameter(HelpMessage = "Filter to return only activated cloud flows.")]
        public SwitchParameter Activated { get; set; }

        /// <summary>
        /// Gets or sets whether to filter for draft flows only.
        /// </summary>
        [Parameter(HelpMessage = "Filter to return only draft cloud flows.")]
        public SwitchParameter Draft { get; set; }

        /// <summary>
        /// Gets or sets whether to include the client data JSON in the output.
        /// </summary>
        [Parameter(HelpMessage = "Include the client data JSON containing the flow definition in the output.")]
        public SwitchParameter IncludeClientData { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            WriteVerbose("Querying cloud flows from Dataverse environment...");

            // Build query
            var query = new QueryExpression("workflow")
            {
                ColumnSet = new ColumnSet(
                    "workflowid",
                    "name",
                    "description",
                    "category",
                    "statecode",
                    "statuscode",
                    "primaryentity",
                    "ownerid",
                    "createdon",
                    "modifiedon",
                    "type",
                    "triggeroncreate",
                    "triggerondelete"
                )
            };

            // Include clientdata if requested
            if (IncludeClientData.IsPresent)
            {
                query.ColumnSet.AddColumn("clientdata");
            }

            // Filter for modern cloud flows (category = 5)
            query.Criteria.AddCondition("category", ConditionOperator.Equal, 5);

            // Filter for definition type (type = 1)
            query.Criteria.AddCondition("type", ConditionOperator.Equal, 1);

            // Add filters
            if (Id.HasValue)
            {
                query.Criteria.AddCondition("workflowid", ConditionOperator.Equal, Id.Value);
                WriteVerbose($"Filtering by ID: {Id.Value}");
            }

            if (!string.IsNullOrEmpty(Name))
            {
                // Check if Name contains wildcards
                if (Name.Contains("*") || Name.Contains("?"))
                {
                    // Convert wildcards to SQL LIKE pattern
                    string likePattern = Name.Replace("*", "%").Replace("?", "_");
                    query.Criteria.AddCondition("name", ConditionOperator.Like, likePattern);
                    WriteVerbose($"Filtering by name pattern: {Name} (LIKE: {likePattern})");
                }
                else
                {
                    query.Criteria.AddCondition("name", ConditionOperator.Equal, Name);
                    WriteVerbose($"Filtering by name: {Name}");
                }
            }

            if (Activated.IsPresent && !Draft.IsPresent)
            {
                query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 1);
                WriteVerbose("Filtering for activated flows only");
            }
            else if (Draft.IsPresent && !Activated.IsPresent)
            {
                query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
                WriteVerbose("Filtering for draft flows only");
            }

            // Execute query
            var flows = Connection.RetrieveMultiple(query);

            WriteVerbose($"Found {flows.Entities.Count} cloud flow(s)");

            // Convert to CloudFlowInfo objects
            foreach (var flow in flows.Entities)
            {
                var flowInfo = new CloudFlowInfo
                {
                    Id = flow.Id,
                    Name = flow.GetAttributeValue<string>("name"),
                    Description = flow.GetAttributeValue<string>("description"),
                    Category = flow.GetAttributeValue<OptionSetValue>("category")?.Value ?? 0,
                    State = GetStateName(flow.GetAttributeValue<OptionSetValue>("statecode")?.Value ?? 0),
                    Status = GetStatusName(flow.GetAttributeValue<OptionSetValue>("statuscode")?.Value ?? 0),
                    PrimaryEntity = flow.GetAttributeValue<string>("primaryentity"),
                    OwnerId = flow.GetAttributeValue<EntityReference>("ownerid")?.Id ?? Guid.Empty,
                    CreatedOn = flow.GetAttributeValue<DateTime?>("createdon"),
                    ModifiedOn = flow.GetAttributeValue<DateTime?>("modifiedon"),
                    Type = flow.GetAttributeValue<OptionSetValue>("type")?.Value ?? 0,
                    TriggerOnCreate = flow.Contains("triggeroncreate") ? flow.GetAttributeValue<bool?>("triggeroncreate") : null,
                    TriggerOnDelete = flow.Contains("triggerondelete") ? flow.GetAttributeValue<bool?>("triggerondelete") : null
                };

                if (IncludeClientData.IsPresent)
                {
                    flowInfo.ClientData = flow.GetAttributeValue<string>("clientdata");
                }

                WriteObject(flowInfo);
            }
        }

        private string GetStateName(int stateCode)
        {
            switch (stateCode)
            {
                case 0:
                    return "Draft";
                case 1:
                    return "Activated";
                default:
                    return $"Unknown ({stateCode})";
            }
        }

        private string GetStatusName(int statusCode)
        {
            switch (statusCode)
            {
                case 1:
                    return "Draft";
                case 2:
                    return "Activated";
                default:
                    return $"Unknown ({statusCode})";
            }
        }
    }
}
