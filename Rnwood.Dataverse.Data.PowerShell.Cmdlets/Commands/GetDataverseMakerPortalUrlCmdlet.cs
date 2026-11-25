using System;
using System.Management.Automation;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	/// <summary>
	/// Generates a URL to open the Power Apps Maker Portal for the current environment.
	/// </summary>
	[Cmdlet(VerbsCommon.Get, "DataverseMakerPortalUrl")]
	[OutputType(typeof(string))]
	public class GetDataverseMakerPortalUrlCmdlet : OrganizationServiceCmdlet
	{
		/// <summary>
		/// Gets or sets the logical name of the table to open in the maker portal.
		/// </summary>
		[Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the table to open in the maker portal (e.g., 'account', 'contact').")]
		[Alias("EntityName", "LogicalName")]
		public string TableName { get; set; }

		/// <summary>
		/// Processes the cmdlet to generate the maker portal URL.
		/// </summary>
		protected override void ProcessRecord()
		{
			base.ProcessRecord();

			if (Connection == null)
			{
				ThrowTerminatingError(new ErrorRecord(
					new InvalidOperationException("No connection provided. Use -Connection parameter or set a default connection."),
					"NoConnection",
					ErrorCategory.InvalidOperation,
					null));
				return;
			}

			// Use ConnectedOrgId directly - avoids an extra WhoAmI network call
			Guid orgId = Connection.ConnectedOrgId;

			// Build the maker portal URL
			string baseUrl = "https://make.powerapps.com";
			string url = $"{baseUrl}/environments/{orgId:D}";

			// If table name is provided, navigate to that table
			if (!string.IsNullOrEmpty(TableName))
			{
				url += $"/entities/entity/{TableName}";
			}
			else
			{
				url += "/home";
			}

			WriteObject(url);
		}
	}
}
