using System;
using System.Linq;
using System.Management.Automation;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
	/// <summary>
	/// Generates a URL to open a record in the Dataverse web interface.
	/// </summary>
	[Cmdlet(VerbsCommon.Get, "DataverseRecordUrl", DefaultParameterSetName = "ByAppUniqueName")]
	[OutputType(typeof(string))]
	public class GetDataverseRecordUrlCmdlet : OrganizationServiceCmdlet
	{
		/// <summary>
		/// Gets or sets the logical name of the table (entity).
		/// </summary>
		[Parameter(Mandatory = true, Position = 0, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of the table (e.g., 'account', 'contact').")]
		[Alias("EntityName", "LogicalName")]
		public string TableName { get; set; }

		/// <summary>
		/// Gets or sets the ID of the record. If not provided, generates a URL for creating a new record.
		/// </summary>
		[Parameter(Mandatory = false, Position = 1, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of the record. If not provided, generates a URL for creating a new record.")]
		[Alias("RecordId")]
		public Guid? Id { get; set; }

		/// <summary>
		/// Gets or sets the unique name of the app to open the record in a specific model-driven app.
		/// </summary>
		[Parameter(Mandatory = false, ParameterSetName = "ByAppUniqueName", ValueFromPipelineByPropertyName = true, HelpMessage = "Unique name of the app to open the record in a specific model-driven app. The app ID will be looked up (including unpublished apps).")]
		[Alias("UniqueName")]
		public string AppUniqueName { get; set; }

		/// <summary>
		/// Gets or sets the app ID to open the record in a specific model-driven app.
		/// </summary>
		[Parameter(Mandatory = false, ParameterSetName = "ByAppId", HelpMessage = "App ID to open the record in a specific model-driven app.")]
		public Guid? AppId { get; set; }

		/// <summary>
		/// Gets or sets the form ID to open a specific form.
		/// </summary>
		[Parameter(Mandatory = false, HelpMessage = "Form ID to open a specific form for the record.")]
		public Guid? FormId { get; set; }

		/// <summary>
		/// Processes the cmdlet to generate the record URL.
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

			// Extract the base URL from the connection using the WebApplication endpoint
			string baseUrl = null;
			if (Connection.ConnectedOrgPublishedEndpoints?.ContainsKey(Microsoft.Xrm.Sdk.Discovery.EndpointType.WebApplication) == true)
			{
				baseUrl = Connection.ConnectedOrgPublishedEndpoints[Microsoft.Xrm.Sdk.Discovery.EndpointType.WebApplication]?.ToString();
			}

			if (string.IsNullOrEmpty(baseUrl))
			{
				ThrowTerminatingError(new ErrorRecord(
					new InvalidOperationException("Unable to determine organization URL from connection."),
					"InvalidConnection",
					ErrorCategory.InvalidOperation,
					null));
				return;
			}

			// Remove trailing slash
			baseUrl = baseUrl.TrimEnd('/');

			// Resolve AppId from AppUniqueName if provided
			Guid? resolvedAppId = AppId;
			if (!string.IsNullOrEmpty(AppUniqueName))
			{
				WriteVerbose($"Looking up app module by unique name: {AppUniqueName}");
				
				var query = new QueryExpression("appmodule")
				{
					ColumnSet = new ColumnSet("appmoduleid"),
					Criteria = new FilterExpression()
				};
				query.Criteria.AddCondition("uniquename", ConditionOperator.Equal, AppUniqueName);

				// Query including unpublished apps
				var appModules = QueryHelpers.ExecuteQueryWithPaging(query, Connection, WriteVerbose, unpublished: true);
				var appModule = appModules.FirstOrDefault();

				if (appModule == null)
				{
					ThrowTerminatingError(new ErrorRecord(
						new InvalidOperationException($"App module with unique name '{AppUniqueName}' not found."),
						"AppModuleNotFound",
						ErrorCategory.ObjectNotFound,
						AppUniqueName));
					return;
				}

				resolvedAppId = appModule.Id;
				WriteVerbose($"Resolved app module ID: {resolvedAppId}");
			}

			// Build the URL
			string url;
			if (Id.HasValue)
			{
				// URL for existing record
				url = $"{baseUrl}/main.aspx?etn={TableName}&id={Id.Value:D}&pagetype=entityrecord";
			}
			else
			{
				// URL for creating new record
				url = $"{baseUrl}/main.aspx?etn={TableName}&pagetype=entityrecord";
			}

			// Add optional parameters
			if (resolvedAppId.HasValue)
			{
				url += $"&appid={resolvedAppId.Value:D}";
			}

			if (FormId.HasValue)
			{
				url += $"&formid={FormId.Value:D}";
			}

			WriteObject(url);
		}
	}
}
