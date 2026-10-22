using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Creates or updates web resources in a Dataverse environment.
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "DataverseWebResource", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    [OutputType(typeof(PSObject))]
    public class SetDataverseWebResourceCmdlet : OrganizationServiceCmdlet
    {
        private const string PARAMSET_OBJECT = "InputObject";
        private const string PARAMSET_FILE = "File";
        private const string PARAMSET_FOLDER = "Folder";

        /// <summary>
        /// Gets or sets the input object containing web resource properties.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_OBJECT, Mandatory = true, ValueFromPipeline = true, HelpMessage = "Object containing web resource properties (name, displayname, webresourcetype, content, etc.)")]
        public PSObject InputObject { get; set; }

        /// <summary>
        /// If specified, rejects lookup values that require a query to resolve a related record. Supply a reference object with TableName and Id, or TableName and KeyAttributes, instead.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_OBJECT, HelpMessage = "If specified, rejects lookup values that require a query to resolve a related record. Supply a reference object with TableName and Id, or TableName and KeyAttributes, instead.")]
        public SwitchParameter NoLookup { get; set; }

        /// <summary>
        /// Gets or sets the ID of the web resource to update.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_OBJECT, HelpMessage = "ID of the web resource to update. If not specified, creates a new web resource or matches by name.")]
        [Parameter(ParameterSetName = PARAMSET_FILE, HelpMessage = "ID of the web resource to update. If not specified, creates a new web resource or matches by name.")]
        public Guid? Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the web resource.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_OBJECT, HelpMessage = "Name of the web resource. Used to match existing resources if Id is not provided.")]
        [Parameter(ParameterSetName = PARAMSET_FILE, Mandatory = true, HelpMessage = "Name of the web resource. Used to match existing resources if Id is not provided.")]
        [ArgumentCompleter(typeof(WebResourceNameArgumentCompleter))]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the file path containing the web resource content.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_FILE, Mandatory = true, HelpMessage = "File path containing the web resource content")]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the folder path containing multiple web resource files.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_FOLDER, Mandatory = true, HelpMessage = "Folder path containing multiple web resource files. File names are used as web resource names.")]
        public string Folder { get; set; }

        /// <summary>
        /// Gets or sets the display name for the web resource.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_FILE, HelpMessage = "Display name for the web resource")]
        [Parameter(ParameterSetName = PARAMSET_FOLDER, HelpMessage = "Display name prefix for web resources created from folder")]
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the description for the web resource.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_FILE, HelpMessage = "Description for the web resource")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the web resource type. Auto-detected from file extension if not specified.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_FILE, HelpMessage = "Web resource type. Auto-detected from file extension if not specified.")]
        public WebResourceType? WebResourceType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to publish the web resource after creation/update.
        /// </summary>
        [Parameter(HelpMessage = "If set, publishes the web resource after creation/update")]
        public SwitchParameter Publish { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to return the created/updated web resource object.
        /// </summary>
        [Parameter(HelpMessage = "If set, returns the created/updated web resource object")]
        public SwitchParameter PassThru { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to skip updating existing web resources.
        /// </summary>
        [Parameter(HelpMessage = "If set, skips updating existing web resources (only creates new ones)")]
        public SwitchParameter NoUpdate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to skip creating new web resources.
        /// </summary>
        [Parameter(HelpMessage = "If set, skips creating new web resources (only updates existing ones)")]
        public SwitchParameter NoCreate { get; set; }

        /// <summary>
        /// Gets or sets the file filter for folder operations.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_FOLDER, HelpMessage = "File filter pattern for folder operations (e.g., '*.js', '*.html'). Default is '*.*'")]
        public string FileFilter { get; set; } = "*.*";

        /// <summary>
        /// Gets or sets the publisher prefix for new web resources.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_FILE, HelpMessage = "Publisher prefix for new web resources (e.g., 'new_'). Required for new web resources if Name doesn't contain a prefix.")]
        [Parameter(ParameterSetName = PARAMSET_FOLDER, HelpMessage = "Publisher prefix for new web resources (e.g., 'new_'). Required for new web resources if file names don't contain a prefix.")]
        public string PublisherPrefix { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to only update if the file is newer than the existing web resource.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_FILE, HelpMessage = "If set, only updates if the file modification time is newer than the web resource modified time")]
        [Parameter(ParameterSetName = PARAMSET_FOLDER, HelpMessage = "If set, only updates files that are newer than the corresponding web resources")]
        public SwitchParameter IfNewer { get; set; }

        /// <inheritdoc />
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            switch (ParameterSetName)
            {
                case PARAMSET_OBJECT:
                    ProcessInputObject();
                    break;

                case PARAMSET_FILE:
                    ProcessFile(Path, Name);
                    break;

                case PARAMSET_FOLDER:
                    ProcessFolder();
                    break;
            }
        }

        private void ProcessInputObject()
        {
            var metadataFactory = new EntityMetadataFactory(Connection);
            var converter = new DataverseEntityConverter(Connection, metadataFactory);
            var entity = converter.ConvertToDataverseEntity(InputObject, "webresource", new ConvertToDataverseEntityOptions { DisallowLookupQueries = NoLookup.IsPresent });

            if (Id.HasValue)
            {
                entity.Id = Id.Value;
                entity.Attributes["webresourceid"] = Id.Value;
            }

            Guid webResourceId = CreateOrUpdateWebResource(entity);

            if (PassThru)
            {
                var retrievedEntity = Connection.Retrieve("webresource", webResourceId, new ColumnSet(true));
                var psObject = converter.ConvertToPSObject(retrievedEntity, new ColumnSet(true), _ => ValueType.Display);
                WriteObject(psObject);
            }
        }

        private void ProcessFile(string filePath, string webResourceName)
        {
            if (!File.Exists(filePath))
            {
                WriteError(new ErrorRecord(
                    new FileNotFoundException($"File not found: {filePath}"),
                    "FileNotFound",
                    ErrorCategory.ObjectNotFound,
                    filePath));
                return;
            }

            var fileInfo = new FileInfo(filePath);
            var fileBytes = File.ReadAllBytes(filePath);
            var contentBase64 = Convert.ToBase64String(fileBytes);

            // Auto-detect web resource type from extension if not specified
            int resourceType = WebResourceType.HasValue ? (int)WebResourceType.Value : DetectWebResourceType(filePath);

            var entity = new Entity("webresource");

            // Check if web resource exists
            Guid? existingId = Id ?? FindWebResourceByName(webResourceName);

            if (existingId.HasValue)
            {
                if (NoUpdate)
                {
                    WriteVerbose($"Skipping update of existing web resource: {webResourceName}");
                    return;
                }

                // Check IfNewer flag
                if (IfNewer)
                {
                    // Use RetrieveUnpublishedRequest to get unpublished web resource
                    var retrieveUnpublishedRequest = new Microsoft.Crm.Sdk.Messages.RetrieveUnpublishedRequest
                    {
                        Target = new EntityReference("webresource", existingId.Value),
                        ColumnSet = new ColumnSet("modifiedon")
                    };
                    var response = (Microsoft.Crm.Sdk.Messages.RetrieveUnpublishedResponse)Connection.Execute(retrieveUnpublishedRequest);
                    var existingEntity = response.Entity;
                    
                    if (existingEntity.Contains("modifiedon"))
                    {
                        var modifiedOn = existingEntity.GetAttributeValue<DateTime>("modifiedon");
                        if (fileInfo.LastWriteTimeUtc <= modifiedOn.ToUniversalTime())
                        {
                            WriteVerbose($"Skipping update - file is not newer than web resource: {webResourceName}");
                            return;
                        }
                    }
                }

                entity.Id = existingId.Value;
                entity.Attributes["webresourceid"] = existingId.Value;
                entity.Attributes["content"] = contentBase64;

                if (!string.IsNullOrEmpty(DisplayName))
                    entity.Attributes["displayname"] = DisplayName;

                if (!string.IsNullOrEmpty(Description))
                    entity.Attributes["description"] = Description;

                if (ShouldProcess(webResourceName, "Update web resource"))
                {
                    Connection.Update(entity);
                    WriteVerbose($"Updated web resource: {webResourceName} (ID: {existingId.Value})");

                    if (Publish)
                    {
                        PublishWebResource(existingId.Value);
                    }

                    if (PassThru)
                    {
                        var retrievedEntity = Connection.Retrieve("webresource", existingId.Value, new ColumnSet(true));
                        var metadataFactory = new EntityMetadataFactory(Connection);
                        var converter = new DataverseEntityConverter(Connection, metadataFactory);
                        WriteObject(converter.ConvertToPSObject(retrievedEntity, new ColumnSet(true), _ => ValueType.Display));
                    }
                }
            }
            else
            {
                if (NoCreate)
                {
                    WriteVerbose($"Skipping creation of new web resource: {webResourceName}");
                    return;
                }

                // Validate name has publisher prefix
                if (!webResourceName.Contains("_") && string.IsNullOrEmpty(PublisherPrefix))
                {
                    WriteError(new ErrorRecord(
                        new ArgumentException("Web resource name must contain a publisher prefix (e.g., 'new_myresource') or provide -PublisherPrefix parameter"),
                        "InvalidName",
                        ErrorCategory.InvalidArgument,
                        webResourceName));
                    return;
                }

                string fullName = webResourceName.Contains("_") ? webResourceName : $"{PublisherPrefix}_{webResourceName}";

                entity.Attributes["name"] = fullName;
                entity.Attributes["displayname"] = DisplayName ?? fullName;
                entity.Attributes["content"] = contentBase64;
                entity.Attributes["webresourcetype"] = new OptionSetValue(resourceType);

                if (!string.IsNullOrEmpty(Description))
                    entity.Attributes["description"] = Description;

                if (ShouldProcess(fullName, "Create web resource"))
                {
                    var newId = Connection.Create(entity);
                    WriteVerbose($"Created web resource: {fullName} (ID: {newId})");

                    if (Publish)
                    {
                        PublishWebResource(newId);
                    }

                    if (PassThru)
                    {
                        var retrievedEntity = Connection.Retrieve("webresource", newId, new ColumnSet(true));
                        var metadataFactory = new EntityMetadataFactory(Connection);
                        var converter = new DataverseEntityConverter(Connection, metadataFactory);
                        WriteObject(converter.ConvertToPSObject(retrievedEntity, new ColumnSet(true), _ => ValueType.Display));
                    }
                }
            }
        }

        private void ProcessFolder()
        {
            if (!Directory.Exists(Folder))
            {
                WriteError(new ErrorRecord(
                    new DirectoryNotFoundException($"Folder not found: {Folder}"),
                    "FolderNotFound",
                    ErrorCategory.ObjectNotFound,
                    Folder));
                return;
            }

            var files = Directory.GetFiles(Folder, FileFilter);

            foreach (var file in files)
            {
                var fileName = System.IO.Path.GetFileName(file);
                var nameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(file);
                
                // Sanitize the name to create a valid web resource name
                // Replace invalid characters with underscores
                var webResourceName = new string(nameWithoutExtension.Select(c => 
                    char.IsLetterOrDigit(c) || c == '_' ? c : '_').ToArray());

                ProcessFile(file, webResourceName);
            }
        }

        private Guid CreateOrUpdateWebResource(Entity entity)
        {
            Guid webResourceId;

            if (entity.Contains("webresourceid") && entity.GetAttributeValue<Guid>("webresourceid") != Guid.Empty)
            {
                webResourceId = entity.GetAttributeValue<Guid>("webresourceid");

                if (NoUpdate)
                {
                    WriteVerbose($"Skipping update of existing web resource ID: {webResourceId}");
                    return webResourceId;
                }

                if (ShouldProcess(entity.GetAttributeValue<string>("name") ?? webResourceId.ToString(), "Update web resource"))
                {
                    Connection.Update(entity);
                    WriteVerbose($"Updated web resource: {webResourceId}");

                    if (Publish)
                    {
                        PublishWebResource(webResourceId);
                    }
                }
            }
            else
            {
                if (NoCreate)
                {
                    WriteWarning($"Skipping creation of new web resource");
                    return Guid.Empty;
                }

                if (ShouldProcess(entity.GetAttributeValue<string>("name") ?? "new web resource", "Create web resource"))
                {
                    webResourceId = Connection.Create(entity);
                    WriteVerbose($"Created web resource: {webResourceId}");

                    if (Publish)
                    {
                        PublishWebResource(webResourceId);
                    }
                }
                else
                {
                    return Guid.Empty;
                }
            }

            return webResourceId;
        }

        private Guid? FindWebResourceByName(string name)
        {
            var query = new QueryExpression("webresource");
            query.ColumnSet = new ColumnSet("webresourceid");
            query.Criteria.AddCondition("name", ConditionOperator.Equal, name);
            query.TopCount = 1;

            // Use RetrieveUnpublishedMultipleRequest to find unpublished web resources
            var request = new Microsoft.Crm.Sdk.Messages.RetrieveUnpublishedMultipleRequest { Query = query };
            var response = (Microsoft.Crm.Sdk.Messages.RetrieveUnpublishedMultipleResponse)Connection.Execute(request);
            var results = response.EntityCollection;

            return results.Entities.Count > 0 ? results.Entities[0].Id : (Guid?)null;
        }

        private void PublishWebResource(Guid webResourceId)
        {
            var publishXml = $"<importexportxml><webresources><webresource>{webResourceId}</webresource></webresources></importexportxml>";

            var request = new OrganizationRequest("PublishXml");
            request["ParameterXml"] = publishXml;

            Connection.Execute(request);
            WriteVerbose($"Published web resource: {webResourceId}");
        }

        private int DetectWebResourceType(string filePath)
        {
            var extension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();

            if (extension == ".htm" || extension == ".html") return 1;  // HTML
            if (extension == ".css") return 2;                          // CSS
            if (extension == ".js") return 3;                           // JavaScript
            if (extension == ".xml") return 4;                          // Data (XML)
            if (extension == ".png") return 5;                          // PNG
            if (extension == ".jpg" || extension == ".jpeg") return 6;  // JPG
            if (extension == ".gif") return 7;                          // GIF
            if (extension == ".xap") return 8;                          // Silverlight (XAP)
            if (extension == ".xsl" || extension == ".xslt") return 9;  // XSL
            if (extension == ".ico") return 10;                         // ICO
            if (extension == ".svg") return 11;                         // SVG
            if (extension == ".resx") return 12;                        // RESX

            return 1; // Default to HTML
        }
    }
}
