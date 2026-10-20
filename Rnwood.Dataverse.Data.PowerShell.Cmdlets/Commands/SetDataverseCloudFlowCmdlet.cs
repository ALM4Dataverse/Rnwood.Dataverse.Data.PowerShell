using System;
using System.Management.Automation;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates a cloud flow in Dataverse. If a flow with the specified name already exists, it is updated; otherwise a new flow is created.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseCloudFlow", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(Guid))]
    public class SetDataverseCloudFlowCmdlet : OrganizationServiceCmdlet
    {
        /// <summary>
        /// Minimal valid cloud flow client data JSON used when creating a new flow without specifying ClientData.
        /// Includes a manual (button) trigger so that the flow is fully functional and can be subsequently updated.
        /// </summary>
        private const string DefaultClientData = "{\"properties\":{\"connectionReferences\":{},\"definition\":{\"$schema\":\"https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowdefinition.json#\",\"contentVersion\":\"1.0.0.0\",\"parameters\":{},\"triggers\":{\"manual\":{\"metadata\":{},\"type\":\"Request\",\"kind\":\"Button\",\"inputs\":{\"schema\":{\"type\":\"object\",\"properties\":{}}}}},\"actions\":{},\"outputs\":{}}},\"schemaVersion\":\"1.0.0.0\"}";

        /// <summary>
        /// Gets or sets the ID of the cloud flow to update. Use this parameter set to update an existing flow by its ID.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ById", HelpMessage = "The ID of the cloud flow to update.", ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the cloud flow. If a flow with this name exists it will be updated; otherwise a new flow is created.
        /// </summary>
        [Parameter(Mandatory = true, Position = 0, ParameterSetName = "ByName", HelpMessage = "The name of the cloud flow. If a flow with this name exists it will be updated; otherwise a new flow is created.")]
        [ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the new display name for the cloud flow (update only).
        /// </summary>
        [Parameter(HelpMessage = "The new display name for the cloud flow (only applies when updating an existing flow).")]
        public string NewName { get; set; }

        /// <summary>
        /// Gets or sets the description for the cloud flow.
        /// </summary>
        [Parameter(HelpMessage = "The description for the cloud flow.")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the flow definition JSON (clientdata). Required when creating a new flow unless a minimal definition is acceptable.
        /// </summary>
        [Parameter(HelpMessage = "The flow definition JSON (clientdata). When creating a new flow and this is not specified, a minimal empty flow definition is used.")]
        public string ClientData { get; set; }

        /// <summary>
        /// Gets or sets whether to activate the cloud flow.
        /// </summary>
        [Parameter(HelpMessage = "Activate the cloud flow.")]
        public SwitchParameter Activate { get; set; }

        /// <summary>
        /// Gets or sets whether to deactivate the cloud flow.
        /// </summary>
        [Parameter(HelpMessage = "Deactivate the cloud flow (set to draft).")]
        public SwitchParameter Deactivate { get; set; }

        /// <summary>
        /// Gets or sets whether to pass through the created or updated flow ID as output.
        /// </summary>
        [Parameter(HelpMessage = "Return the ID of the created or updated cloud flow.")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Processes the cmdlet request.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (Activate.IsPresent && Deactivate.IsPresent)
            {
                ThrowTerminatingError(new ErrorRecord(
                    new ArgumentException("Cannot specify both Activate and Deactivate switches."),
                    "InvalidParameters",
                    ErrorCategory.InvalidArgument,
                    null));
                return;
            }

            Guid flowId = Id;
            bool isCreate = false;

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
                    // Flow not found – will create
                    isCreate = true;
                    WriteVerbose($"Cloud flow '{Name}' not found. A new flow will be created.");
                }
                else
                {
                    flowId = flows.Entities[0].Id;
                    WriteVerbose($"Found cloud flow with ID: {flowId}");
                }
            }

            if (isCreate)
            {
                if (!ShouldProcess($"Cloud flow '{Name}'", "Create"))
                {
                    return;
                }

                WriteVerbose($"Creating new cloud flow '{Name}'...");

                var newFlow = new Entity("workflow")
                {
                    ["name"] = Name,
                    ["category"] = new OptionSetValue(5),
                    ["type"] = new OptionSetValue(1),
                    ["primaryentity"] = "none",
                    ["clientdata"] = string.IsNullOrEmpty(ClientData) ? DefaultClientData : ClientData
                };

                if (!string.IsNullOrEmpty(Description))
                {
                    newFlow["description"] = Description;
                }

                flowId = Connection.Create(newFlow);
                WriteVerbose($"Cloud flow created with ID: {flowId}");

                if (Activate.IsPresent)
                {
                    WriteVerbose("Activating new cloud flow...");
                    var activateRequest = new SetStateRequest
                    {
                        EntityMoniker = new EntityReference("workflow", flowId),
                        State = new OptionSetValue(1),
                        Status = new OptionSetValue(2)
                    };
                    Connection.Execute(activateRequest);
                    WriteVerbose("Cloud flow activated successfully.");
                }
            }
            else
            {
                if (!ShouldProcess($"Cloud flow '{(ParameterSetName == "ByName" ? Name : flowId.ToString())}'", "Update"))
                {
                    return;
                }

                // Retrieve current flow to verify it exists and get current values
                WriteVerbose($"Retrieving cloud flow {flowId}...");
                Entity flow;
                try
                {
                    flow = Connection.Retrieve("workflow", flowId, new ColumnSet("name", "description", "statecode", "statuscode"));
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

                var currentName = flow.GetAttributeValue<string>("name");
                var currentDescription = flow.GetAttributeValue<string>("description");
                var currentStateCode = flow.GetAttributeValue<OptionSetValue>("statecode")?.Value ?? 0;

                WriteVerbose($"Found cloud flow: {currentName} (State: {(currentStateCode == 0 ? "Draft" : "Activated")})");

                bool hasUpdates = false;
                var updateEntity = new Entity("workflow", flowId);

                if (!string.IsNullOrEmpty(NewName) && NewName != currentName)
                {
                    updateEntity["name"] = NewName;
                    hasUpdates = true;
                    WriteVerbose($"Setting name to: {NewName}");
                }

                if (!string.IsNullOrEmpty(Description) && Description != currentDescription)
                {
                    updateEntity["description"] = Description;
                    hasUpdates = true;
                    WriteVerbose($"Setting description to: {Description}");
                }

                if (!string.IsNullOrEmpty(ClientData))
                {
                    updateEntity["clientdata"] = ClientData;
                    hasUpdates = true;
                    WriteVerbose("Updating flow definition (clientdata).");
                }

                if (hasUpdates)
                {
                    WriteVerbose("Updating cloud flow properties...");
                    Connection.Update(updateEntity);
                    WriteVerbose("Cloud flow properties updated successfully.");
                }

                if (Activate.IsPresent)
                {
                    if (currentStateCode == 1)
                    {
                        WriteVerbose("Cloud flow is already activated. No state change needed.");
                    }
                    else
                    {
                        WriteVerbose("Activating cloud flow...");
                        var activateRequest = new SetStateRequest
                        {
                            EntityMoniker = new EntityReference("workflow", flowId),
                            State = new OptionSetValue(1),
                            Status = new OptionSetValue(2)
                        };
                        Connection.Execute(activateRequest);
                        WriteVerbose("Cloud flow activated successfully.");
                    }
                }
                else if (Deactivate.IsPresent)
                {
                    if (currentStateCode == 0)
                    {
                        WriteVerbose("Cloud flow is already in draft state. No state change needed.");
                    }
                    else
                    {
                        WriteVerbose("Deactivating cloud flow...");
                        var deactivateRequest = new SetStateRequest
                        {
                            EntityMoniker = new EntityReference("workflow", flowId),
                            State = new OptionSetValue(0),
                            Status = new OptionSetValue(1)
                        };
                        Connection.Execute(deactivateRequest);
                        WriteVerbose("Cloud flow deactivated successfully.");
                    }
                }

                if (!hasUpdates && !Activate.IsPresent && !Deactivate.IsPresent)
                {
                    WriteVerbose("No updates to apply.");
                }
            }

            if (PassThru.IsPresent)
            {
                WriteObject(flowId);
            }
        }
    }
}
