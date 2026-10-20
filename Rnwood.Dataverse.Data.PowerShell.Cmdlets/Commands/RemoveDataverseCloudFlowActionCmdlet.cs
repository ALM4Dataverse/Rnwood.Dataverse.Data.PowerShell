using System;
using System.Management.Automation;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Removes an action from a cloud flow in Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseCloudFlowAction", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.High)]
    public class RemoveDataverseCloudFlowActionCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the cloud flow containing the action.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ById", HelpMessage = "The ID of the cloud flow containing the action.", ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public Guid FlowId { get; set; }

        /// <summary>
        /// Gets or sets the name of the cloud flow containing the action.
        /// </summary>
        [Parameter(Mandatory = true, ParameterSetName = "ByName", HelpMessage = "The name of the cloud flow containing the action.")]
        [ValidateNotNullOrEmpty]
        public string FlowName { get; set; }

        /// <summary>
        /// Gets or sets the ID/name of the action to remove.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, HelpMessage = "The ID/name of the action to remove.")]
        [ValidateNotNullOrEmpty]
        public string ActionId { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            Guid flowId = FlowId;
            string flowName = FlowName;

            // If searching by name, resolve to ID
            if (ParameterSetName == "ByName")
            {
                WriteVerbose($"Querying for cloud flow '{FlowName}'...");

                var query = new QueryExpression("workflow")
                {
                    ColumnSet = new ColumnSet("workflowid", "name"),
                    Criteria = new FilterExpression()
                };

                query.Criteria.AddCondition("name", ConditionOperator.Equal, FlowName);
                query.Criteria.AddCondition("category", ConditionOperator.Equal, 5);
                query.Criteria.AddCondition("type", ConditionOperator.Equal, 1);
                query.TopCount = 1;

                var flows = Connection.RetrieveMultiple(query);

                if (flows.Entities.Count == 0)
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new InvalidOperationException($"Cloud flow '{FlowName}' not found."),
                        "FlowNotFound",
                        ErrorCategory.ObjectNotFound,
                        FlowName));
                    return;
                }

                flowId = flows.Entities[0].Id;
                flowName = flows.Entities[0].GetAttributeValue<string>("name");
                WriteVerbose($"Found cloud flow '{flowName}' with ID: {flowId}");
            }
            else
            {
                // Retrieve flow name
                try
                {
                    var flow = Connection.Retrieve("workflow", flowId, new ColumnSet("name"));
                    flowName = flow.GetAttributeValue<string>("name");
                }
                catch
                {
                    flowName = flowId.ToString();
                }
            }

            if (!ShouldProcess($"Action '{ActionId}' in cloud flow '{flowName}'", "Remove"))
            {
                return;
            }

            WriteVerbose($"Retrieving cloud flow definition for '{flowName}'...");

            // Retrieve the flow with clientdata
            Entity flowEntity;
            try
            {
                flowEntity = Connection.Retrieve("workflow", flowId, new ColumnSet("clientdata", "name"));
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Cloud flow with ID '{flowId}' not found or could not be retrieved.", ex),
                    "FlowNotFound",
                    ErrorCategory.ObjectNotFound,
                    flowId));
                return;
            }

            var clientData = flowEntity.GetAttributeValue<string>("clientdata");

            if (string.IsNullOrEmpty(clientData))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Cloud flow '{flowName}' has no client data."),
                    "NoClientData",
                    ErrorCategory.InvalidData,
                    flowId));
                return;
            }

            WriteVerbose("Parsing flow definition...");

            // Parse the JSON
            JObject flowDefinition;
            try
            {
                flowDefinition = JObject.Parse(clientData);
            }
            catch (JsonException ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Failed to parse cloud flow definition: {ex.Message}", ex),
                    "ParseError",
                    ErrorCategory.ParserError,
                    clientData));
                return;
            }

            // Find the actions object
            var actionsToken = flowDefinition.SelectToken("properties.definition.actions");

            if (actionsToken == null || actionsToken.Type != JTokenType.Object)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"No actions found in cloud flow '{flowName}'."),
                    "NoActions",
                    ErrorCategory.InvalidData,
                    flowId));
                return;
            }

            var actionsObj = (JObject)actionsToken;

            // Check if the action exists
            if (!actionsObj.ContainsKey(ActionId))
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Action '{ActionId}' not found in cloud flow '{flowName}'."),
                    "ActionNotFound",
                    ErrorCategory.ObjectNotFound,
                    ActionId));
                return;
            }

            WriteVerbose($"Removing action '{ActionId}'...");

            // Remove the action
            actionsObj.Remove(ActionId);

            // Save the updated definition back
            WriteVerbose("Saving updated flow definition...");

            var updateEntity = new Entity("workflow", flowId);
            updateEntity["clientdata"] = flowDefinition.ToString(Formatting.None);

            try
            {
                Connection.Update(updateEntity);
                WriteVerbose("Action removed successfully.");
                WriteObject($"Action '{ActionId}' removed from cloud flow '{flowName}' successfully.");
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Failed to remove action: {ex.Message}", ex),
                    "RemoveFailed",
                    ErrorCategory.OperationStopped,
                    ActionId));
            }
        }
    }
}
