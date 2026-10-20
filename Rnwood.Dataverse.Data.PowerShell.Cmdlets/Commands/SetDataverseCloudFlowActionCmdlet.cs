using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates an action within a cloud flow in Dataverse. If the action already exists it is updated; otherwise a new action is created.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseCloudFlowAction", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public class SetDataverseCloudFlowActionCmdlet : OrganizationServiceCmdlet
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
        /// Gets or sets the ID/name of the action to create or update.
        /// </summary>
        [Parameter(Mandatory = true, Position = 1, HelpMessage = "The ID/name of the action to create or update.")]
        [ValidateNotNullOrEmpty]
        public string ActionId { get; set; }

        /// <summary>
        /// Gets or sets the type of the action. Required when creating a new action (e.g. 'Http', 'Response', 'Compose', 'InitializeVariable', 'Scope').
        /// </summary>
        [Parameter(HelpMessage = "The type of the action. Required when creating a new action (e.g. 'Http', 'Response', 'Compose', 'InitializeVariable', 'Scope').")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the inputs for the action.
        /// </summary>
        [Parameter(HelpMessage = "The inputs for the action as a hashtable or JSON string.")]
        public object Inputs { get; set; }

        /// <summary>
        /// Gets or sets the description for the action.
        /// </summary>
        [Parameter(HelpMessage = "The description for the action.")]
        public string Description { get; set; }

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

            // Ensure actions container exists
            var definitionToken = flowDefinition.SelectToken("properties.definition");
            if (definitionToken == null || definitionToken.Type != JTokenType.Object)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Cloud flow '{flowName}' definition structure is invalid (missing properties.definition)."),
                    "InvalidFlowStructure",
                    ErrorCategory.InvalidData,
                    flowId));
                return;
            }

            var definitionObj = (JObject)definitionToken;
            if (definitionObj["actions"] == null || definitionObj["actions"].Type != JTokenType.Object)
            {
                definitionObj["actions"] = new JObject();
            }

            var actionsObj = (JObject)definitionObj["actions"];

            // Find or create the action
            var actionToken = actionsObj[ActionId];
            bool isCreate = actionToken == null || actionToken.Type != JTokenType.Object;

            if (isCreate)
            {
                if (string.IsNullOrEmpty(Type))
                {
                    ThrowTerminatingError(new ErrorRecord(
                        new ArgumentException($"Action '{ActionId}' not found in cloud flow '{flowName}'. The -Type parameter is required when creating a new action."),
                        "TypeRequiredForCreate",
                        ErrorCategory.InvalidArgument,
                        ActionId));
                    return;
                }

                if (!ShouldProcess($"Action '{ActionId}' in cloud flow '{flowName}'", "Create"))
                {
                    return;
                }

                WriteVerbose($"Action '{ActionId}' not found. Creating new action of type '{Type}'...");
                var newAction = new JObject
                {
                    ["type"] = Type,
                    ["runAfter"] = new JObject()
                };
                actionsObj[ActionId] = newAction;
                actionToken = newAction;
            }
            else
            {
                if (!ShouldProcess($"Action '{ActionId}' in cloud flow '{flowName}'", "Update"))
                {
                    return;
                }

                WriteVerbose($"Updating existing action '{ActionId}'...");

                if (!string.IsNullOrEmpty(Type))
                {
                    ((JObject)actionToken)["type"] = Type;
                }
            }

            var actionObj = (JObject)actionToken;
            bool hasUpdates = isCreate;

            // Update inputs if provided
            if (Inputs != null)
            {
                WriteVerbose("Updating action inputs...");
                JToken inputsToken;

                if (Inputs is string inputsString)
                {
                    try
                    {
                        inputsToken = JToken.Parse(inputsString);
                    }
                    catch (JsonException ex)
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new ArgumentException($"Failed to parse Inputs as JSON: {ex.Message}", ex),
                            "InvalidInputsJson",
                            ErrorCategory.InvalidArgument,
                            Inputs));
                        return;
                    }
                }
                else if (Inputs is Hashtable inputsHashtable)
                {
                    inputsToken = JToken.FromObject(inputsHashtable);
                }
                else if (Inputs is PSObject inputsPSObject)
                {
                    var hashtable = new Hashtable();
                    foreach (var property in inputsPSObject.Properties)
                    {
                        hashtable[property.Name] = property.Value;
                    }
                    inputsToken = JToken.FromObject(hashtable);
                }
                else
                {
                    try
                    {
                        inputsToken = JToken.FromObject(Inputs);
                    }
                    catch (Exception ex)
                    {
                        ThrowTerminatingError(new ErrorRecord(
                            new ArgumentException($"Failed to convert Inputs to JSON: {ex.Message}", ex),
                            "InvalidInputsType",
                            ErrorCategory.InvalidArgument,
                            Inputs));
                        return;
                    }
                }

                actionObj["inputs"] = inputsToken;
                hasUpdates = true;
            }

            // Update description if provided
            if (!string.IsNullOrEmpty(Description))
            {
                WriteVerbose($"Setting action description to: {Description}");

                if (actionObj["metadata"] == null || actionObj["metadata"].Type != JTokenType.Object)
                {
                    actionObj["metadata"] = new JObject();
                }

                ((JObject)actionObj["metadata"])["description"] = Description;
                hasUpdates = true;
            }

            if (!hasUpdates)
            {
                WriteWarning("No updates to apply. Please specify at least one property to update (Type, Inputs, or Description).");
                return;
            }

            // Save the updated definition back
            WriteVerbose("Saving updated flow definition...");

            var updateEntity = new Entity("workflow", flowId);
            updateEntity["clientdata"] = flowDefinition.ToString(Formatting.None);

            try
            {
                Connection.Update(updateEntity);
                WriteVerbose($"Action '{ActionId}' in cloud flow '{flowName}' saved successfully.");
            }
            catch (Exception ex)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new InvalidOperationException($"Failed to save action: {ex.Message}", ex),
                    "UpdateFailed",
                    ErrorCategory.OperationStopped,
                    ActionId));
            }
        }
    }
}
