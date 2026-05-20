using System;
using System.Management.Automation;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Removes (deletes) a cloud flow from Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseCloudFlow", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseCloudFlowCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the cloud flow to remove.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ById", HelpMessage = "The ID of the cloud flow to remove.", ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the cloud flow to remove.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ByName", HelpMessage = "The name of the cloud flow to remove.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Guid flowId = Id;
            string flowName = Name;

            // If searching by name, resolve to ID
            if (ParameterSetName == "ByName")
            {
                WriteVerbose($"Querying for cloud flow '{Name}'...");

                var query = new QueryExpression("workflow")
                {
                    ColumnSet = new ColumnSet("workflowid", "name"),
                    Criteria = new FilterExpression()
                };

                query.Criteria.AddCondition("name", ConditionOperator.Equal, Name);
                query.Criteria.AddCondition("category", ConditionOperator.Equal, 5);
                query.Criteria.AddCondition("type", ConditionOperator.Equal, 1);
                query.TopCount = 1;

                var flows = Connection.RetrieveMultiple(query);

                if (flows.Entities.Count == 0)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Cloud flow '{Name}' not found."),
                        "FlowNotFound",
                        ErrorCategory.ObjectNotFound,
                        Name));
                    return;
                }

                flowId = flows.Entities[0].Id;
                flowName = flows.Entities[0].GetAttributeValue<string>("name");
                WriteVerbose($"Found cloud flow '{flowName}' with ID: {flowId}");
            }
            else
            {
                // Retrieve flow name for better messages
                try
                {
                    var flow = Connection.Retrieve("workflow", flowId, new ColumnSet("name"));
                    flowName = flow.GetAttributeValue<string>("name");
                    WriteVerbose($"Found cloud flow: {flowName}");
                }
                catch
                {
                    // If we can't retrieve, use ID as name
                    flowName = flowId.ToString();
                }
            }

            if (!ShouldProcess($"Cloud flow '{flowName}'", "Remove"))
            {
                return;
            }

            WriteVerbose($"Deleting cloud flow {flowId}...");

            try
            {
                Connection.Delete("workflow", flowId);
                WriteVerbose("Cloud flow deleted successfully.");
                WriteObject($"Cloud flow '{flowName}' removed successfully.");
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Failed to delete cloud flow '{flowName}': {ex.Message}", ex),
                    "DeleteFailed",
                    ErrorCategory.OperationStopped,
                    flowId));
            }
        }
    }
}
