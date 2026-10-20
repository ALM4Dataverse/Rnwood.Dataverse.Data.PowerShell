using System;
using System.Management.Automation;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Represents information about a cloud flow in Dataverse.
    /// </summary>
    public class CloudFlowInfo
    {
        /// <summary>
        /// Gets or sets the unique identifier of the cloud flow.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the display name of the cloud flow.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the cloud flow.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the category of the workflow (5 = Modern Flow/Cloud Flow).
        /// </summary>
        public int Category { get; set; }

        /// <summary>
        /// Gets or sets the state of the flow (Draft or Activated).
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the status of the flow.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the primary entity the flow is associated with.
        /// </summary>
        public string PrimaryEntity { get; set; }

        /// <summary>
        /// Gets or sets the owner ID of the flow.
        /// </summary>
        public Guid OwnerId { get; set; }

        /// <summary>
        /// Gets or sets the date the flow was created.
        /// </summary>
        public DateTime? CreatedOn { get; set; }

        /// <summary>
        /// Gets or sets the date the flow was last modified.
        /// </summary>
        public DateTime? ModifiedOn { get; set; }

        /// <summary>
        /// Gets or sets the type of workflow (1 = Definition).
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Gets or sets whether the flow triggers on create.
        /// </summary>
        public bool? TriggerOnCreate { get; set; }

        /// <summary>
        /// Gets or sets whether the flow triggers on delete.
        /// </summary>
        public bool? TriggerOnDelete { get; set; }

        /// <summary>
        /// Gets or sets the client data JSON containing the flow definition.
        /// </summary>
        public string ClientData { get; set; }
    }
}
