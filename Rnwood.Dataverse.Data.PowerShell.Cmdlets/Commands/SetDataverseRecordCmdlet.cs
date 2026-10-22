using Azure;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{

    /// <summary>Creates or updates records in a Dataverse environment. If a matching record is found then it will be updated, otherwise a new record is created (some options can override this).
    /// This command can also handle creation/update of intersect records (many to many relationships).</summary>
    [Cmdlet(VerbsCommon.Set, "DataverseRecord", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public partial class SetDataverseRecordCmdlet : CustomLogicBypassableOrganizationServiceCmdlet, ISetOperationParameters
    {
        /// <summary>
        /// Object containing values to be used. Property names must match the logical names of Dataverse columns in the specified table and the property values are used to set the values of the Dataverse record being created/updated. The properties may include ownerid, statecode and statuscode which will assign and change the record state/status.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ValueFromRemainingArguments = true,
            HelpMessage = "Object containing values to be used. Property names must match the logical names of Dataverse columns in the specified table and the property values are used to set the values of the Dataverse record being created/updated. The properties may include ownerid, statecode and statuscode which will assign and change the record state/status.")]
        [Alias("Values")]
        public PSObject InputObject { get; set; }
        /// <summary>
        /// The logical name of the table to operate on.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of table")]
        [Alias("EntityName", "LogicalName")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        public string TableName { get; set; }

        /// <summary>
        /// Controls the maximum number of requests sent to Dataverse in one batch (where possible) to improve throughput. Specify 1 to disable batching.
        /// </summary>
        [Parameter(HelpMessage = "Controls the maximum number of requests sent to Dataverse in one batch (where possible) to improve throughput. Specify 1 to disable.")]
        public uint BatchSize { get; set; } = 100;

        /// <summary>
        /// Controls the maximum number of records to retrieve in a single query when checking for existing records. Default is 500. Specify 1 to retrieve one record at a time.
        /// </summary>
        [Parameter(HelpMessage = "Controls the maximum number of records to retrieve in a single query when checking for existing records. Default is 500. Specify 1 to retrieve one record at a time.")]
        public uint RetrievalBatchSize { get; set; } = 500;

        /// <summary>
        /// List of properties on the input object which are ignored and not attempted to be mapped to the record. Default is none.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "List of properties on the input object which are ignored and not attempted to be mapped to the record. Default is none.")]
        [ArgumentCompleter(typeof(Rnwood.Dataverse.Data.PowerShell.Commands.PSObjectPropertyNameArgumentCompleter))]
        public string[] IgnoreProperties { get; set; }

        /// <summary>
        /// ID of record to be created or updated.
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "ID of record to be created or updated.")]
        public Guid Id { get; set; }
        /// <summary>
        /// List of list of column names that identify an existing record to update based on the values of those columns in the InputObject. For update/create these are used if a record with an Id matching the value of the Id cannot be found. The first list that returns a match is used.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "List of list of column names that identify an existing record to update based on the values of those columns in the InputObject. For update/create these are used if a record with an Id matching the value of the Id cannot be found. The first list that returns a match is used.")]
        public string[][] MatchOn { get; set; }
        /// <summary>
        /// If specified, allows updating of multiple records when MatchOn criteria matches more than one record. Without this switch, an error is raised if MatchOn finds multiple matches.
        /// </summary>
        [Parameter(HelpMessage = "If specified, allows updating of multiple records when MatchOn criteria matches more than one record. Without this switch, an error is raised if MatchOn finds multiple matches.")]
        public SwitchParameter AllowMultipleMatches { get; set; }
        /// <summary>
        /// If specified, the InputObject is written to the pipeline as a PSObject with an Id property set indicating the primary key of the affected record (even if nothing was updated). The output is always converted to a PSObject format regardless of the input object type for uniformity.
        /// </summary>
        [Parameter(HelpMessage = "If specified, the InputObject is written to the pipeline as a PSObject with an Id property set indicating the primary key of the affected record (even if nothing was updated). The output is always converted to a PSObject format regardless of the input object type for uniformity.")]
        public SwitchParameter PassThru { get; set; }
        /// <summary>
        /// If specified, existing records matching the ID and or MatchOn columns will not be updated.
        /// </summary>
        [Parameter(HelpMessage = "If specified, existing records matching the ID and or MatchOn columns will not be updated.")]
        public SwitchParameter NoUpdate { get; set; }
        /// <summary>
        /// If specified, then no records will be created even if no existing records matching the ID and or MatchOn columns is found.
        /// </summary>
        [Parameter(HelpMessage = "If specified, then no records will be created even if no existing records matching the ID and or MatchOn columns is found.")]
        public SwitchParameter NoCreate { get; set; }
        /// <summary>
        /// List of column names which will not be included when updating existing records.
        /// </summary>
        [Parameter(HelpMessage = "List of column names which will not be included when updating existing records.")]
        [ArgumentCompleter(typeof(Rnwood.Dataverse.Data.PowerShell.Commands.ColumnNamesArgumentCompleter))]
        public string[] NoUpdateColumns { get; set; }
        /// <summary>
        /// If specified, the creation/updates will be done on behalf of the user with the specified ID. For best performance, sort the records using this value since a new batch request is needed each time this value changes.
        /// </summary>
        [Parameter(ValueFromPipelineByPropertyName = true, HelpMessage = "If specified, the creation/updates will be done on behalf of the user with the specified ID. For best performance, sort the records using this value since a new batch request is needed each time this value changes.")]
        public Guid? CallerId { get; set; }
        /// <summary>
        /// If specified, an update containing all supplied columns will be issued without retrieving the existing record for comparison (default is to remove unchanged columns). Id must be provided
        /// </summary>
        [Parameter(HelpMessage = "If specified, an update containing all supplied columns will be issued without retrieving the existing record for comparison (default is to remove unchanged columns). Id must be provided")]
        public SwitchParameter UpdateAllColumns { get; set; }
        /// <summary>
        /// If specified, no check for existing record is made and records will always be attempted to be created. Use this option when it's known that no existing matching records will exist to improve performance. See the -noupdate option for an alternative.
        /// </summary>
        [Parameter(HelpMessage = "If specified, no check for existing record is made and records will always be attempted to be created. Use this option when it's known that no existing matching records will exist to improve performance. See the -noupdate option for an alternative.")]
        public SwitchParameter CreateOnly { get; set; }
        /// <summary>
        /// If specified, upsert request will be used to create/update existing records as appropriate. With this option, -MatchOn must specify a single alternate key defined on the table. It is not supported for many-to-many intersect entities.
        /// </summary>
        [Parameter(HelpMessage = "If specified, upsert request will be used to create/update existing records as appropriate. With this option, -MatchOn must specify a single alternate key defined on the table. It is not supported for many-to-many intersect entities.")]
        public SwitchParameter Upsert { get; set; }
        /// <summary>
        /// Hashset mapping lookup column names in the target entity to column names in the referred-to table for resolving lookup references.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "Hashset of lookup column name in the target entity to column name in the referred to table with which to find the records.")]
        [ArgumentCompleter(typeof(Rnwood.Dataverse.Data.PowerShell.Commands.ColumnNamesArgumentCompleter))]
        public Hashtable LookupColumns
        {
            get;
            set;
        }
        /// <summary>
        /// Specifies the types of business logic (for example plugins) to bypass
        /// </summary>
        [Parameter(HelpMessage = "Specifies the types of business logic (for example plugins) to bypass")]
        public override BusinessLogicTypes[] BypassBusinessLogicExecution { get; set; }
        /// <summary>
        /// Specifies the IDs of plugin steps to bypass
        /// </summary>
        [Parameter(HelpMessage = "Specifies the IDs of plugin steps to bypass")]
        public override Guid[] BypassBusinessLogicExecutionStepIds { get; set; }

        /// <summary>
        /// Number of times to retry each record on failure. Default is 0 (no retries).
        /// </summary>
        [Parameter(HelpMessage = "Number of times to retry each record on failure. Default is 0 (no retries).")]
        public int Retries { get; set; } = 0;

        /// <summary>
        /// Initial delay in seconds before first retry. Subsequent retries use exponential backoff. Default is 5s.
        /// </summary>
        [Parameter(HelpMessage = "Initial delay in seconds before first retry. Subsequent retries use exponential backoff. Default is 5s.")]
        public int InitialRetryDelay { get; set; } = 5;

        /// <summary>
        /// Maximum number of parallel set operations. Default is 1 (parallel processing disabled).
        /// When set to a value greater than 1, records are processed in parallel using multiple connections.
        /// </summary>
        [Parameter(HelpMessage = "Maximum number of parallel set operations. Default is 1 (parallel processing disabled). When set to a value greater than 1, records are processed in parallel using multiple connections.")]
        public int MaxDegreeOfParallelism { get; set; } = 1;

        /// <summary>
        /// If specified, duplicate detection will be enabled by setting SuppressDuplicateDetection to false in create, update, and upsert requests.
        /// </summary>
        [Parameter(HelpMessage = "If specified, duplicate detection will be enabled by setting SuppressDuplicateDetection to false in create, update, and upsert requests.")]
        public SwitchParameter EnableDuplicateDetection { get; set; }

        /// <summary>
        /// Path to a directory containing file attachments to upload. When a file column value (GUID) changes, 
        /// the cmdlet looks for a subfolder with the matching GUID inside this directory and uploads the single file found there.
        /// This is designed to work with folders created by Set-DataverseRecordsFolder.
        /// </summary>
        [Parameter(HelpMessage = "Path to a directory containing file attachments. When a file column value (GUID) changes, looks for a subfolder with the matching GUID and uploads the file found there.")]
        public string FileDirectory { get; set; }

        // Explicit interface implementations for ISetOperationParameters
        bool ISetOperationParameters.NoUpdate => NoUpdate.IsPresent;
        bool ISetOperationParameters.NoCreate => NoCreate.IsPresent;
        bool ISetOperationParameters.CreateOnly => CreateOnly.IsPresent;
        bool ISetOperationParameters.Upsert => Upsert.IsPresent;
        bool ISetOperationParameters.PassThru => PassThru.IsPresent;
        bool ISetOperationParameters.UpdateAllColumns => UpdateAllColumns.IsPresent;
        CustomLogicBypassableOrganizationServiceCmdlet.BusinessLogicTypes[] ISetOperationParameters.BypassBusinessLogicExecution => BypassBusinessLogicExecution;
        Guid[] ISetOperationParameters.BypassBusinessLogicExecutionStepIds => BypassBusinessLogicExecutionStepIds;
        int ISetOperationParameters.Retries => Retries;
        int ISetOperationParameters.InitialRetryDelay => InitialRetryDelay;
        string[] ISetOperationParameters.NoUpdateColumns => NoUpdateColumns;
        string[][] ISetOperationParameters.MatchOn => MatchOn;
        Guid ISetOperationParameters.Id => Id;
        bool ISetOperationParameters.AllowMultipleMatches => AllowMultipleMatches.IsPresent;
        bool ISetOperationParameters.EnableDuplicateDetection => EnableDuplicateDetection.IsPresent;
        string ISetOperationParameters.FileDirectory => FileDirectory;

        private SetBatchProcessor _setBatchProcessor;
        private RetrievalBatchProcessor _retrievalBatchProcessor;
        private ParallelSetProcessor _parallelProcessor;
        private CancellationTokenSource _userCancellationCts;

        /// <summary>
        /// Initializes the cmdlet.
        /// </summary>


        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            // Initialize cancellation token source for this pipeline invocation
            _userCancellationCts = new CancellationTokenSource();

            recordCount = 0;
            entityMetadataFactory = new EntityMetadataFactory(Connection);
            entityConverter = new DataverseEntityConverter(Connection, entityMetadataFactory);

            // Warn if affinity cookie is enabled (default) when using parallelization
            if (MaxDegreeOfParallelism > 1 && Connection != null && Connection.EnableAffinityCookie)
            {
                WriteWarning("Using parallelization with affinity cookie enabled may reduce performance. Consider using Get-DataverseConnection with -DisableAffinityCookie for better parallel performance. Note: Disabling affinity cookie may result in eventual consistency issues.");
            }

            // Initialize retrieval batch processor
            _retrievalBatchProcessor = new RetrievalBatchProcessor(
                Connection,
                WriteVerbose,
                WriteError,
                () => Stopping,
                (int)RetrievalBatchSize,
                Retries,
                InitialRetryDelay,
                Id,
                MatchOn,
                AllowMultipleMatches.IsPresent);

            if (MaxDegreeOfParallelism > 1)
            {
                // Use parallel processing
                _parallelProcessor = new ParallelSetProcessor(
                    MaxDegreeOfParallelism,
                    BatchSize,
                    Connection,
                    WriteVerbose,
                    WriteError,
                    WriteProgress,
                    WriteObject,
                    ShouldProcess,
                    () => Stopping,
                    _userCancellationCts.Token,
                    callerId => Connection.CallerId = callerId.GetValueOrDefault(),
                    () => Connection.CallerId);
            }
            else if (BatchSize > 1)
            {
                // Initialize the batch processor
                _setBatchProcessor = new SetBatchProcessor(
                    BatchSize,
                    Connection,
                    WriteVerbose,
                    ShouldProcess,
                    () => Stopping,
                    _userCancellationCts.Token,
                    callerId => Connection.CallerId = callerId.GetValueOrDefault(),
                    () => Connection.CallerId);
            }
        }




        /// <summary>
        /// Completes cmdlet processing.
        /// </summary>


        protected override void EndProcessing()
        {
            base.EndProcessing();

            // Process any remaining queued records
            if (_retrievalBatchProcessor != null && _retrievalBatchProcessor.QueuedCount > 0)
            {
                _retrievalBatchProcessor.ProcessQueuedRecords(ProcessRecordWithExistingRecord);
            }

            // Wait for parallel processing to complete if it was used
            if (_parallelProcessor != null)
            {
                _parallelProcessor.WaitForCompletion();
            }
            else if (_setBatchProcessor != null)
            {
                // Flush batch processor if it was used
                _setBatchProcessor.Flush();

                // Process any pending retries
                _setBatchProcessor.ProcessRetries();
            }

            // Process any pending retrieval retries
            if (_retrievalBatchProcessor != null && _retrievalBatchProcessor.PendingRetryCount > 0)
            {
                _retrievalBatchProcessor.ProcessRetries(
                    ProcessSingleRecord,
                    () => 
                    { 
                        if (_parallelProcessor != null)
                        {
                            _parallelProcessor.WaitForCompletion();
                        }
                        else if (_setBatchProcessor != null)
                        {
                            _setBatchProcessor.Flush();
                        }
                    });
            }

            // Cleanup cancellation token source
            _userCancellationCts?.Dispose();
            _userCancellationCts = null;
        }

        /// <summary>
        /// Called when the user cancels the cmdlet.
        /// </summary>
        protected override void StopProcessing()
        {
            // Called when user presses Ctrl+C. Signal cancellation to any ongoing operations.
            try
            {
                _userCancellationCts?.Cancel();
            }
            catch { }
            base.StopProcessing();
        }



        private int recordCount;
        private EntityMetadataFactory entityMetadataFactory;
        private DataverseEntityConverter entityConverter;

        private ConvertToDataverseEntityOptions GetConversionOptions()
        {
            ConvertToDataverseEntityOptions options = new ConvertToDataverseEntityOptions();


            options.IgnoredPropertyName.Add("TableName");
            options.IgnoredPropertyName.Add("EntityName");
            options.IgnoredPropertyName.Add("Id");

            if (IgnoreProperties != null)
            {
                foreach (string ignoreProperty in IgnoreProperties)
                {
                    options.IgnoredPropertyName.Add(ignoreProperty);
                }
            }

            if (LookupColumns != null)
            {
                foreach (DictionaryEntry lookupColumn in LookupColumns)
                {
                    options.ColumnOptions[(string)lookupColumn.Key] = new ConvertToDataverseEntityColumnOptions() { LookupColumn = (string)lookupColumn.Value };
                }
            }

            return options;
        }
        /// <summary>
        /// Processes each record in the pipeline.
        /// </summary>


        protected override void ProcessRecord()
        {
            base.ProcessRecord();


            recordCount++;
            WriteVerbose("Processing record #" + recordCount);

            ProcessSingleRecord(InputObject, TableName, CallerId);
        }

        private void ProcessSingleRecord(PSObject inputObject, string tableName, Guid? callerId)
        {
            if (callerId.HasValue)
            {
                if (BatchSize > 1)
                {
                    throw new ArgumentException("CreatedOnBehalfBy not supported with BatchSize > 1");
                }
            }

            Entity target;

            try
            {
                target = entityConverter.ConvertToDataverseEntity(inputObject, tableName, GetConversionOptions());
            }
            catch (FormatException e)
            {
                // Conversion errors are not retryable
                WriteError(new ErrorRecord(new Exception("Error converting input object: " + e.Message, e), null, ErrorCategory.InvalidData, inputObject));
                return;
            }
            catch (Exception e)
            {
                // Other errors during conversion may be retryable
                if (Retries > 0)
                {
                    _retrievalBatchProcessor.ScheduleRecordRetry(inputObject, tableName, callerId, e);
                }
                else
                {
                    _retrievalBatchProcessor.RecordRetryDone(inputObject);
                    WriteError(new ErrorRecord(e, null, ErrorCategory.InvalidOperation, inputObject));
                }
                return;
            }

            EntityMetadata entityMetadata = entityMetadataFactory.GetLimitedMetadata(TableName);

            // Create a context to access utility methods
            var context = new SetOperationContext(
                inputObject,
                tableName,
                callerId,
                this,
                entityMetadataFactory,
                entityConverter,
                Connection,
                GetConversionOptions(),
                WriteVerbose,
                WriteError,
                WriteObject,
                ShouldProcess);
            context.Target = target;
            context.EntityMetadata = entityMetadata;

            // Check if this record needs retrieval
            if (context.NeedsRetrieval(entityMetadata, target))
            {
                // Queue for batched retrieval
                _retrievalBatchProcessor.QueueForRetrieval(new RetrievalBatchProcessor.RecordProcessingItem
                {
                    InputObject = inputObject,
                    Target = target,
                    EntityMetadata = entityMetadata,
                    ExistingRecord = null,
                    TableName = tableName,
                    CallerId = callerId
                });

                // Process the batch if it's full
                if (_retrievalBatchProcessor.QueuedCount >= RetrievalBatchSize)
                {
                    _retrievalBatchProcessor.ProcessQueuedRecords(ProcessRecordWithExistingRecord);
                }
            }
            else
            {
                // Process immediately without retrieval
                List<Entity> existingRecords;

                try
                {
                    existingRecords = context.GetExistingRecords(entityMetadata, target);
                }
                catch (Exception e)
                {
                    if (Retries > 0)
                    {
                        _retrievalBatchProcessor.ScheduleRecordRetry(inputObject, tableName, callerId, e);
                    }
                    else
                    {
                        _retrievalBatchProcessor.RecordRetryDone(inputObject);
                        WriteError(new ErrorRecord(e, null, ErrorCategory.InvalidOperation, inputObject));
                    }
                    return;
                }

                // Process each existing record found
                if (existingRecords.Count == 0)
                {
                    ProcessRecordWithExistingRecord(inputObject, tableName, callerId, target, entityMetadata, null);
                }
                else
                {
                    foreach (var existingRecord in existingRecords)
                    {
                        ProcessRecordWithExistingRecord(inputObject, tableName, callerId, target, entityMetadata, existingRecord);
                    }
                }
            }
        }


        private bool UpsertRecord(PSObject inputObject, string tableName, Guid? callerId, EntityMetadata entityMetadata, Entity target)
        {
            bool result = true;

            // Create context for this operation
            var context = new SetOperationContext(
                inputObject,
                tableName,
                callerId,
                this,
                entityMetadataFactory,
                entityConverter,
                Connection,
                GetConversionOptions(),
                WriteVerbose,
                WriteError,
                WriteObject,
                ShouldProcess);
            context.Target = target;
            context.EntityMetadata = entityMetadata;

            // Let context build the request and set up callbacks
            try
            {
                context.UpsertRecord();
            }
            catch (NotSupportedException e)
            {
                // Upsert not supported for intersect entities in non-batch mode
                throw;
            }

            // If no requests were created, return early
            if (context.Requests.Count == 0)
            {
                return result;
            }

            // Queue for parallel or batch processing if available, otherwise execute immediately
            if (_parallelProcessor != null)
            {
                WriteVerbose(string.Format("Queued upsert of {0}:{1} for parallel processing", tableName, context.Target.Id));
                _parallelProcessor.QueueOperation(context);
            }
            else if (_setBatchProcessor != null)
            {
                _setBatchProcessor.QueueOperation(context);
            }
            else
            {
                // Non-batch mode: execute all requests immediately
                if (ShouldProcess(context.Requests.Count == 1 ? context.Requests[0].RequestName : $"Execute {context.Requests.Count} requests"))
                {
                    Guid oldCallerId = Connection.CallerId;
                    try
                    {
                        if (callerId.HasValue)
                        {
                            Connection.CallerId = callerId.Value;
                        }

                        // Execute each request and call its completion callback
                        for (int i = 0; i < context.Requests.Count; i++)
                        {
                            var request = context.Requests[i];
                            
                            // For non-batch upsert, need to generate ID if not set
                            if (request is UpsertRequest upsertRequest)
                            {
                                if (upsertRequest.Target.Id == Guid.Empty && 
                                    (upsertRequest.Target.KeyAttributes == null || upsertRequest.Target.KeyAttributes.Count == 0))
                                {
                                    upsertRequest.Target.Id = Guid.NewGuid();
                                }
                            }

                            var response = Connection.Execute(request);

                            // Call completion callback for this request
                            if (context.ResponseCompletions != null && i < context.ResponseCompletions.Count && context.ResponseCompletions[i] != null)
                            {
                                context.ResponseCompletions[i](response);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        result = false;
                        WriteError(new ErrorRecord(e, null, ErrorCategory.WriteError, inputObject));
                    }
                    finally
                    {
                        Connection.CallerId = oldCallerId;
                    }
                }
            }

            return result;
        }



        
        private void CreateNewRecord(PSObject inputObject, string tableName, Guid? callerId, EntityMetadata entityMetadata, Entity target)
        {
            // Create context for this operation
            var context = new SetOperationContext(
                inputObject,
                tableName,
                callerId,
                this,
                entityMetadataFactory,
                entityConverter,
                Connection,
                GetConversionOptions(),
                WriteVerbose,
                WriteError,
                WriteObject,
                ShouldProcess);
            context.Target = target;
            context.EntityMetadata = entityMetadata;

            // Let context build the request and set up callbacks
            context.CreateNewRecord();

            // If no requests were created (e.g., NoCreate is set), return early
            if (context.Requests.Count == 0)
            {
                return;
            }

            // Queue for parallel or batch processing if available, otherwise execute immediately
            if (_parallelProcessor != null)
            {
                WriteVerbose(string.Format("Queued create of {0}:{1} for parallel processing", tableName, context.Target.Id));
                _parallelProcessor.QueueOperation(context);
            }
            else if (_setBatchProcessor != null)
            {
                _setBatchProcessor.QueueOperation(context);
            }
            else
            {
                // Non-batch mode: execute all requests immediately
                if (ShouldProcess(context.Requests.Count == 1 ? context.Requests[0].RequestName : $"Execute {context.Requests.Count} requests"))
                {
                    Guid oldCallerId = Connection.CallerId;
                    try
                    {
                        if (callerId.HasValue)
                        {
                            Connection.CallerId = callerId.Value;
                        }

                        // Execute each request and call its completion callback
                        for (int i = 0; i < context.Requests.Count; i++)
                        {
                            var request = context.Requests[i];
                            var response = Connection.Execute(request);

                            // Call completion callback for this request
                            if (context.ResponseCompletions != null && i < context.ResponseCompletions.Count && context.ResponseCompletions[i] != null)
                            {
                                context.ResponseCompletions[i](response);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        WriteError(new ErrorRecord(e, null, ErrorCategory.WriteError, inputObject));
                    }
                    finally
                    {
                        Connection.CallerId = oldCallerId;
                    }
                }
            }
        }

        private void UpdateExistingRecord(PSObject inputObject, string tableName, Guid? callerId, EntityMetadata entityMetadata, Entity target, Entity existingRecord)
        {
            // Create context for this operation
            var context = new SetOperationContext(
                inputObject,
                tableName,
                callerId,
                this,
                entityMetadataFactory,
                entityConverter,
                Connection,
                GetConversionOptions(),
                WriteVerbose,
                WriteError,
                WriteObject,
                ShouldProcess);
            context.Target = target;
            context.EntityMetadata = entityMetadata;
            context.ExistingRecord = existingRecord;

            // Let context build the request and set up callbacks
            context.UpdateExistingRecord();

            // If no requests were created (e.g., NoUpdate is set or no changes), return early
            if (context.Requests.Count == 0)
            {
                return;
            }

            // Queue for parallel or batch processing if available, otherwise execute immediately
            if (_parallelProcessor != null)
            {
                WriteVerbose(string.Format("Queued update of {0}:{1} for parallel processing", tableName, existingRecord.Id));
                _parallelProcessor.QueueOperation(context);
            }
            else if (_setBatchProcessor != null)
            {
                _setBatchProcessor.QueueOperation(context);
            }
            else
            {
                // Non-batch mode: execute all requests immediately
                if (ShouldProcess(context.Requests.Count == 1 ? context.Requests[0].RequestName : $"Execute {context.Requests.Count} requests"))
                {
                    Guid oldCallerId = Connection.CallerId;
                    try
                    {
                        if (callerId.HasValue)
                        {
                            Connection.CallerId = callerId.Value;
                        }

                        // Execute each request and call its completion callback
                        for (int i = 0; i < context.Requests.Count; i++)
                        {
                            var request = context.Requests[i];
                            var response = Connection.Execute(request);

                            // Call completion callback for this request
                            if (context.ResponseCompletions != null && i < context.ResponseCompletions.Count && context.ResponseCompletions[i] != null)
                            {
                                context.ResponseCompletions[i](response);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        WriteError(new ErrorRecord(e, null, ErrorCategory.WriteError, inputObject));
                    }
                    finally
                    {
                        Connection.CallerId = oldCallerId;
                    }
                }
            }
        }

        private void UpdateCompletion(Entity target, PSObject inputObject, Entity existingRecord, string updatedColumnSummary)
        {
            // Delegate to context method
            var context = new SetOperationContext(inputObject, TableName, null, this, entityMetadataFactory, entityConverter, Connection, GetConversionOptions(), WriteVerbose, WriteError, WriteObject, ShouldProcess);
            context.Target = target;
            context.ExistingRecord = existingRecord;
            context.UpdateCompletion(target, existingRecord, updatedColumnSummary);
        }


        private void ProcessRecordWithExistingRecord(PSObject inputObject, string tableName, Guid? callerId, Entity target, EntityMetadata entityMetadata, Entity existingRecord)
        {
            // This is the original processing logic from ProcessRecord, after GetExistingRecord
            if (Upsert.IsPresent)
            {
                UpsertRecord(inputObject, tableName, callerId, entityMetadata, target);
            }
            else
            {
                if (existingRecord != null)
                {
                    UpdateExistingRecord(inputObject, tableName, callerId, entityMetadata, target, existingRecord);
                }
                else
                {
                    CreateNewRecord(inputObject, tableName, callerId, entityMetadata, target);
                }
            }

            // Handle assignment and status changes
            if ((existingRecord != null && !NoUpdate) || (existingRecord == null && !NoCreate))
            {
                if (target.Contains("ownerid"))
                {
                    EntityReference ownerid = target.GetAttributeValue<EntityReference>("ownerid");
                    AssignRequest request = new AssignRequest()
                    {
                        Assignee = ownerid,
                        Target = target.ToEntityReference()
                    };
                    ApplyBypassBusinessLogicExecution(request);

                    if (_parallelProcessor != null)
                    {
                        // Create context for this assign operation
                        var assignContext = new SetOperationContext(inputObject, tableName, callerId, this, entityMetadataFactory, entityConverter, Connection, GetConversionOptions(), WriteVerbose, WriteError, WriteObject, ShouldProcess);
                        assignContext.Target = target;
                        assignContext.Requests.Add(request);
                        assignContext.ResponseCompletion = (response) => { 
                            WriteVerbose(string.Format("Record {0}:{1} assigned to {2}", target.LogicalName, target.Id, ownerid.Name));
                        };
                        
                        WriteVerbose(string.Format("Queued assignment of record {0}:{1} to {2} for parallel processing", TableName, target.Id, ownerid.Name));
                        _parallelProcessor.QueueOperation(assignContext);
                    }
                    else if (_setBatchProcessor != null)
                    {
                        // Create context for this assign operation
                        var assignContext = new SetOperationContext(inputObject, tableName, callerId, this, entityMetadataFactory, entityConverter, Connection, GetConversionOptions(), WriteVerbose, WriteError, WriteObject, ShouldProcess);
                        assignContext.Target = target;
                        assignContext.Requests.Add(request);
                        assignContext.ResponseCompletion = (response) => { 
                            WriteVerbose(string.Format("Record {0}:{1} assigned to {2}", target.LogicalName, target.Id, ownerid.Name));
                        };
                        
                        WriteVerbose(string.Format("Added assignment of record {0}:{1} to {2} to batch", TableName, target.Id, ownerid.Name));
                        _setBatchProcessor.QueueOperation(assignContext);
                    }
                    else
                    {
                        if (ShouldProcess(string.Format("Assign record {0}:{1} to {2}", TableName, target.Id, ownerid.Name)))
                        {
                            try
                            {
                                Connection.Execute(request);
                                WriteVerbose(string.Format("Record {0}:{1} assigned to {2}", target.LogicalName, target.Id, ownerid.Name));
                            }
                            catch (Exception e)
                            {
                                WriteError(new ErrorRecord(e, null, ErrorCategory.WriteError, inputObject));
                            }
                        }
                    }
                }

                if (target.Contains("statecode") || target.Contains("statuscode"))
                {
                    OptionSetValue statuscode = target.GetAttributeValue<OptionSetValue>("statuscode") ?? new OptionSetValue(-1);
                    OptionSetValue stateCode = target.GetAttributeValue<OptionSetValue>("statecode");

                    if (stateCode == null)
                    {
                        StatusAttributeMetadata statusMetadata =
                            (StatusAttributeMetadata)entityMetadata.Attributes.First(
                                a => a.LogicalName.Equals("statuscode", StringComparison.OrdinalIgnoreCase));

                        stateCode = new OptionSetValue(statusMetadata.OptionSet.Options.Cast<StatusOptionMetadata>()
                                .First(o => o.Value.Value == statuscode.Value)
                                .State.Value);
                    }

                    SetStateRequest request = new SetStateRequest()
                    {
                        EntityMoniker = target.ToEntityReference(),
                        State = stateCode,
                        Status = statuscode
                    };
                    ApplyBypassBusinessLogicExecution(request);

                    if (_parallelProcessor != null)
                    {
                        // Create context for this setstate operation
                        var setStateContext = new SetOperationContext(inputObject, tableName, callerId, this, entityMetadataFactory, entityConverter, Connection, GetConversionOptions(), WriteVerbose, WriteError, WriteObject, ShouldProcess);
                        setStateContext.Target = target;
                        setStateContext.Requests.Add(request);
                        setStateContext.ResponseCompletion = (response) => { 
                            WriteVerbose(string.Format("Record {0}:{1} status set to State:{2} Status: {3}", target.LogicalName, target.Id, stateCode.Value, statuscode.Value));
                        };
                        
                        WriteVerbose(string.Format("Queued set record {0}:{1} status to State:{2} Status: {3} for parallel processing", TableName, Id, stateCode.Value, statuscode.Value));
                        _parallelProcessor.QueueOperation(setStateContext);
                    }
                    else if (_setBatchProcessor != null)
                    {
                        // Create context for this setstate operation
                        var setStateContext = new SetOperationContext(inputObject, tableName, callerId, this, entityMetadataFactory, entityConverter, Connection, GetConversionOptions(), WriteVerbose, WriteError, WriteObject, ShouldProcess);
                        setStateContext.Target = target;
                        setStateContext.Requests.Add(request);
                        setStateContext.ResponseCompletion = (response) => { 
                            WriteVerbose(string.Format("Record {0}:{1} status set to State:{2} Status: {3}", target.LogicalName, target.Id, stateCode.Value, statuscode.Value));
                        };
                        
                        WriteVerbose(string.Format("Added set record {0}:{1} status to State:{2} Status: {3} to batch", TableName, Id, stateCode.Value, statuscode.Value));
                        _setBatchProcessor.QueueOperation(setStateContext);
                    }
                    else
                    {
                        if (ShouldProcess(string.Format("Set record {0}:{1} status to State:{2} Status: {3}", TableName, Id, stateCode.Value, statuscode.Value)))
                        {
                            try
                            {
                                Connection.Execute(request);
                                WriteVerbose(string.Format("Record {0}:{1} status set to State:{2} Status: {3}", target.LogicalName, target.Id, stateCode.Value, statuscode.Value));
                            }
                            catch (Exception e)
                            {
                                WriteError(new ErrorRecord(e, null, ErrorCategory.WriteError, inputObject));
                            }
                        }
                    }
                }
            }
        }




    }
}
