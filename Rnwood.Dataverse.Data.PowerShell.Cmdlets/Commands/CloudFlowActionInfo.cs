using System;
using System.Collections.Generic;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Represents information about an action within a cloud flow.
    /// </summary>
    public class CloudFlowActionInfo
    {
        /// <summary>
        /// Gets or sets the unique identifier of the action within the flow.
        /// </summary>
        public string ActionId { get; set; }

        /// <summary>
        /// Gets or sets the display name of the action.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the type of action (e.g., "OpenApiConnection", "Compose", "Condition").
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the operation ID if this is a connector action.
        /// </summary>
        public string OperationId { get; set; }

        /// <summary>
        /// Gets or sets the description of the action.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets whether the action is enabled.
        /// </summary>
        public bool? Enabled { get; set; }

        /// <summary>
        /// Gets or sets the inputs to the action.
        /// </summary>
        public Dictionary<string, object> Inputs { get; set; }

        /// <summary>
        /// Gets or sets the run after configuration for the action.
        /// </summary>
        public Dictionary<string, object> RunAfter { get; set; }

        /// <summary>
        /// Gets or sets the metadata for the action.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Gets or sets the ID of the cloud flow this action belongs to.
        /// </summary>
        public Guid FlowId { get; set; }

        /// <summary>
        /// Gets or sets the name of the cloud flow this action belongs to.
        /// </summary>
        public string FlowName { get; set; }
    }
}
