using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves actions from a cloud flow in Dataverse.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseCloudFlowAction")]
    [OutputType(typeof(CloudFlowActionInfo))]
    public class GetDataverseCloudFlowActionCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Gets or sets the ID of the cloud flow to retrieve actions from.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ById", HelpMessage = "The ID of the cloud flow to retrieve actions from.", ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public Guid FlowId { get; set; }

        /// <summary>
        /// Gets or sets the name of the cloud flow to retrieve actions from.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ByName", HelpMessage = "The name of the cloud flow to retrieve actions from.")]
        [ValidateNotNullOrEmpty]
        public string FlowName { get; set; }

        /// <summary>
        /// Gets or sets the name of a specific action to retrieve. Supports wildcards (* and ?).
        /// </summary>
        [Parameter(Position = 1, HelpMessage = "The name of a specific action to retrieve. Supports wildcards (* and ?). If not specified, all actions are returned.")]
        public string ActionName { get; set; }

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
                WriteWarning($"Cloud flow '{flowName}' has no client data. No actions found.");
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

            // Extract actions from the definition
            var actionsToken = flowDefinition.SelectToken("properties.definition.actions");
            if (actionsToken == null || actionsToken.Type != JTokenType.Object)
            {
                WriteWarning($"No actions found in cloud flow '{flowName}'.");
                return;
            }

            var actions = (JObject)actionsToken;
            WriteVerbose($"Found {actions.Properties().Count()} action(s) in flow definition");

            // Convert wildcards to regex pattern if ActionName is specified
            System.Text.RegularExpressions.Regex nameFilter = null;
            if (!string.IsNullOrEmpty(ActionName))
            {
                var regexPattern = "^" + System.Text.RegularExpressions.Regex.Escape(ActionName)
                    .Replace("\\*", ".*")
                    .Replace("\\?", ".") + "$";
                nameFilter = new System.Text.RegularExpressions.Regex(regexPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                WriteVerbose($"Filtering actions by name pattern: {ActionName}");
            }

            // Process each action
            foreach (var actionProperty in actions.Properties())
            {
                var actionId = actionProperty.Name;
                var actionToken = actionProperty.Value;

                if (actionToken.Type != JTokenType.Object)
                {
                    continue;
                }

                var actionObj = (JObject)actionToken;

                // Get action type
                var actionType = actionObj.SelectToken("type")?.Value<string>();
                if (string.IsNullOrEmpty(actionType))
                {
                    continue;
                }

                // Apply name filter if specified
                if (nameFilter != null && !nameFilter.IsMatch(actionId))
                {
                    continue;
                }

                var actionInfo = new CloudFlowActionInfo
                {
                    ActionId = actionId,
                    Name = actionId, // In flow JSON, the property name is the action name
                    Type = actionType,
                    FlowId = flowId,
                    FlowName = flowName
                };

                // Extract operation ID for connector actions
                var operationId = actionObj.SelectToken("inputs.parameters.operationId")?.Value<string>();
                if (!string.IsNullOrEmpty(operationId))
                {
                    actionInfo.OperationId = operationId;
                }

                // Extract description from metadata
                var description = actionObj.SelectToken("metadata.description")?.Value<string>();
                if (!string.IsNullOrEmpty(description))
                {
                    actionInfo.Description = description;
                }

                // Extract inputs
                var inputsToken = actionObj.SelectToken("inputs");
                if (inputsToken != null && inputsToken.Type == JTokenType.Object)
                {
                    actionInfo.Inputs = inputsToken.ToObject<Dictionary<string, object>>();
                }

                // Extract runAfter
                var runAfterToken = actionObj.SelectToken("runAfter");
                if (runAfterToken != null && runAfterToken.Type == JTokenType.Object)
                {
                    actionInfo.RunAfter = runAfterToken.ToObject<Dictionary<string, object>>();
                }

                // Extract metadata
                var metadataToken = actionObj.SelectToken("metadata");
                if (metadataToken != null && metadataToken.Type == JTokenType.Object)
                {
                    actionInfo.Metadata = metadataToken.ToObject<Dictionary<string, object>>();
                }

                WriteObject(actionInfo);
            }
        }
    }
}
