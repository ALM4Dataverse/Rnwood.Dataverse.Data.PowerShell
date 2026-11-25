using System;
using System.Management.Automation;
using Microsoft.PowerPlatform.Dataverse.Client;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	/// <summary>
	/// Generates a URL to open the Power Platform Admin Center for the current environment.
	/// </summary>
	[Cmdlet(VerbsCommon.Get, "DataverseAdminPortalUrl")]
	[OutputType(typeof(string))]
	public class GetDataverseAdminPortalUrlCmdlet : OrganizationServiceCmdlet
	{
		/// <summary>
		/// Processes the cmdlet to generate the admin portal URL.
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

			// Build the admin portal URL for the specific environment
			string url = $"https://admin.powerplatform.microsoft.com/environments/{orgId:D}/hub";
			
			WriteObject(url);
		}
	}
}
