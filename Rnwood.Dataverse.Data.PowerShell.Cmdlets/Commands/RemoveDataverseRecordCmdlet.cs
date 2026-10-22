



using Azure;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    ///<summary>Deletes records from a Dataverse organization.</summary>
    [Cmdlet(VerbsCommon.Remove, "DataverseRecord", SupportsShouldProcess = true, ConfirmImpact = ConfirmImpact.Medium)]
    public partial class RemoveDataverseRecordCmdlet : CustomLogicBypassableOrganizationServiceCmdlet, IDeleteOperationParameters
    {
        /// <summary>
        /// Record from pipeline. This allows piping in record to delete.
        /// </summary>
        [Parameter(ValueFromPipeline = true, HelpMessage = "Record from pipeline. This allows piping in record to delete.")]
        public PSObject InputObject { get; set; }
        /// <summary>
        /// The logical name of the table to operate on.
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = true, HelpMessage = "Logical name of table")]
        [Alias("EntityName")]
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        public string TableName { get; set; }
        /// <summary>
        /// Id of record to process
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = true, HelpMessage = "Id of record to process")]
        public Guid Id { get; set; }
        /// <summary>
        /// List of list of column names that identify records to delete based on the values of those columns in the InputObject. The first list that returns a match is used. If AllowMultipleMatches is not specified, an error will be raised if more than one record matches.
        /// </summary>
        [Parameter(Mandatory = false, HelpMessage = "List of list of column names that identify records to delete based on the values of those columns in the InputObject. The first list that returns a match is used. If AllowMultipleMatches is not specified, an error will be raised if more than one record matches.")]
        public string[][] MatchOn { get; set; }
        /// <summary>
        /// If specified, rejects lookup values that require a query to resolve a related record. Supply a reference object with TableName and Id, or TableName and KeyAttributes, instead.
        /// </summary>
        [Parameter(HelpMessage = "If specified, rejects lookup values that require a query to resolve a related record. Supply a reference object with TableName and Id, or TableName and KeyAttributes, instead.")]
        public SwitchParameter NoLookup { get; set; }
        /// <summary>
        /// If specified, allows deletion of multiple records when MatchOn criteria matches more than one record. Without this switch, an error is raised if MatchOn finds multiple matches.
        /// </summary>
        [Parameter(HelpMessage = "If specified, allows deletion of multiple records when MatchOn criteria matches more than one record. Without this switch, an error is raised if MatchOn finds multiple matches.")]
        public SwitchParameter AllowMultipleMatches { get; set; }
        /// <summary>
        /// Controls the maximum number of requests sent to Dataverse in one batch (where possible) to improve throughput. Specify 1 to disable. When value is 1, requests are sent to Dataverse one request at a time. When > 1, batching is used. Note that the batch will continue on error and any errors will be returned once the batch has completed. The error contains the input record to allow correlation.
        /// </summary>
        [Parameter(HelpMessage = "Controls the maximum number of requests sent to Dataverse in one batch (where possible) to improve throughput. Specify 1 to disable. When value is 1, requests are sent to Dataverse one request at a time. When > 1, batching is used. Note that the batch will continue on error and any errors will be returned once the batch has completed. The error contains the input record to allow correlation.")]
        public uint BatchSize { get; set; } = 100;
        
        /// <summary>
        /// Controls the maximum number of records to resolve in a single query when using MatchOn. Default is 500. Specify 1 to resolve one record at a time.
        /// </summary>
        [Parameter(HelpMessage = "Controls the maximum number of records to resolve in a single query when using MatchOn. Default is 500. Specify 1 to resolve one record at a time.")]
        public uint RetrievalBatchSize { get; set; } = 500;
        /// <summary>
        /// If specified, the cmdlet will not raise an error if the record does not exist.
        /// </summary>
        [Parameter(HelpMessage = "If specified, the cmdlet will not raise an error if the record does not exist.")]
        public SwitchParameter IfExists { get; set; }

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
        /// Number of times to retry each batch item on failure. Default is 0 (no retries).
        /// </summary>
        [Parameter(HelpMessage = "Number of times to retry each batch item on failure. Default is 0 (no retries).")]
        public int Retries { get; set; } = 0;

        /// <summary>
        /// Initial delay in seconds before first retry. Subsequent retries use exponential backoff. Default is 5s.
        /// </summary>
        [Parameter(HelpMessage = "Initial delay in seconds before first retry. Subsequent retries use exponential backoff. Default is 5s.")]
        public int InitialRetryDelay { get; set; } = 5;

        /// <summary>
        /// Maximum number of parallel delete operations. Default is 1 (parallel processing disabled).
        /// When set to a value greater than 1, records are processed in parallel using multiple connections.
        /// </summary>
        [Parameter(HelpMessage = "Maximum number of parallel delete operations. Default is 1 (parallel processing disabled). When set to a value greater than 1, records are processed in parallel using multiple connections.")]
        public int MaxDegreeOfParallelism { get; set; } = 1;

        // Explicit interface implementations for IDeleteOperationParameters
        bool IDeleteOperationParameters.IfExists => IfExists.IsPresent;
        CustomLogicBypassableOrganizationServiceCmdlet.BusinessLogicTypes[] IDeleteOperationParameters.BypassBusinessLogicExecution => BypassBusinessLogicExecution;
        Guid[] IDeleteOperationParameters.BypassBusinessLogicExecutionStepIds => BypassBusinessLogicExecutionStepIds;
        int IDeleteOperationParameters.Retries => Retries;
        int IDeleteOperationParameters.InitialRetryDelay => InitialRetryDelay;
        string[][] IDeleteOperationParameters.MatchOn => MatchOn;
        bool IDeleteOperationParameters.AllowMultipleMatches => AllowMultipleMatches.IsPresent;

        private EntityMetadataFactory _metadataFactory;
        private DeleteBatchProcessor _batchProcessor;
        private CancellationTokenSource _userCancellationCts;
        private DataverseEntityConverter _entityConverter;
        private List<MatchOnResolveItem> _matchOnResolveQueue;
        private ParallelDeleteProcessor _parallelProcessor;

        /// <summary>Initializes the cmdlet.</summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            // initialize cancellation token source for this pipeline invocation
            _userCancellationCts = new CancellationTokenSource();

            _metadataFactory = new EntityMetadataFactory(Connection);
            _entityConverter = new DataverseEntityConverter(Connection, _metadataFactory);
            _matchOnResolveQueue = new List<MatchOnResolveItem>();

            // Warn if affinity cookie is enabled (default) when using parallelization
            if (MaxDegreeOfParallelism > 1 && Connection != null && Connection.EnableAffinityCookie)
            {
                WriteWarning("Using parallelization with affinity cookie enabled may reduce performance. Consider using Get-DataverseConnection with -DisableAffinityCookie for better parallel performance. Note: Disabling affinity cookie may result in eventual consistency issues.");
            }

            if (MaxDegreeOfParallelism > 1)
            {
                // Use parallel processing
                _parallelProcessor = new ParallelDeleteProcessor(
                    MaxDegreeOfParallelism,
                    BatchSize,
                    Connection,
                    WriteVerbose,
                    WriteError,
                    WriteProgress,
                    ShouldProcess,
                    () => Stopping,
                    _userCancellationCts.Token,
                    this);
            }
            else if (BatchSize > 1)
            {
                _batchProcessor = new DeleteBatchProcessor(
                    BatchSize,
                    Connection,
                    WriteVerbose,
                    ShouldProcess,
                    () => Stopping,
                    _userCancellationCts.Token);
            }
        }

        /// <summary>Processes each record in the pipeline.</summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // Validate parameters
            if (Id == Guid.Empty && MatchOn == null)
            {
                WriteError(new ErrorRecord(
                    new ArgumentException("Either Id or MatchOn must be specified"),
                    null,
                    ErrorCategory.InvalidArgument,
                    InputObject));
                return;
            }

            // If MatchOn is specified, queue for batched resolution
            if (MatchOn != null)
            {
                try
                {
                    // Convert InputObject to Entity to get values for matching
                    // Use the existing entity converter instance for efficiency
                    var conversionOptions = new ConvertToDataverseEntityOptions { DisallowLookupQueries = NoLookup.IsPresent };
                    Entity inputEntity = _entityConverter.ConvertToDataverseEntity(InputObject, TableName, conversionOptions);

                    _matchOnResolveQueue.Add(new MatchOnResolveItem
                    {
                        InputObject = InputObject,
                        InputEntity = inputEntity,
                        ResolvedIds = new List<Guid>()
                    });

                    // Process batch if it's full
                    if (_matchOnResolveQueue.Count >= RetrievalBatchSize)
                    {
                        ProcessMatchOnBatch();
                    }
                }
                catch (Exception e)
                {
                    WriteError(new ErrorRecord(e, null, ErrorCategory.InvalidOperation, InputObject));
                }
                return;
            }

            // If Id is specified, process immediately
            if (Id != Guid.Empty)
            {
                ProcessDeleteById(InputObject, Id);
            }
        }

        /// <summary>
        /// Resolves record IDs based on MatchOn criteria.
        /// </summary>
        private List<Guid> ResolveRecordsByMatchOn()
        {
            List<Guid> resolvedIds = new List<Guid>();
            EntityMetadata entityMetadata = _metadataFactory.GetLimitedMetadata(TableName);

            // Convert InputObject to Entity to get values for matching
            var entityConverter = new DataverseEntityConverter(Connection, _metadataFactory);
            var conversionOptions = new ConvertToDataverseEntityOptions { DisallowLookupQueries = NoLookup.IsPresent };
            Entity inputEntity = entityConverter.ConvertToDataverseEntity(InputObject, TableName, conversionOptions);

            foreach (string[] matchOnColumnList in MatchOn)
            {
                QueryByAttribute matchOnQuery = new QueryByAttribute(TableName);
                matchOnQuery.ColumnSet = new ColumnSet(entityMetadata.PrimaryIdAttribute);

                foreach (string matchOnColumn in matchOnColumnList)
                {
                    object queryValue = inputEntity.GetAttributeValue<object>(matchOnColumn);

                    if (queryValue is EntityReference er)
                    {
                        queryValue = er.Id;
                    }

                    if (queryValue is OptionSetValue osv)
                    {
                        queryValue = osv.Value;
                    }

                    matchOnQuery.AddAttributeValue(matchOnColumn, queryValue);
                }

                var matchingRecords = Connection.RetrieveMultiple(matchOnQuery).Entities;

                if (matchingRecords.Count > 0)
                {
                    resolvedIds.AddRange(matchingRecords.Select(r => r.Id));
                    break; // First match set wins
                }
            }

            return resolvedIds;
        }

        /// <summary>
        /// Processes a batch of MatchOn resolutions using efficient batched queries.
        /// </summary>
        private void ProcessMatchOnBatch()
        {
            if (_matchOnResolveQueue == null || _matchOnResolveQueue.Count == 0)
            {
                return;
            }

            WriteVerbose($"Processing MatchOn batch of {_matchOnResolveQueue.Count} record(s)");

            EntityMetadata entityMetadata = _metadataFactory.GetLimitedMetadata(TableName);

            // Try each MatchOn column list in order
            foreach (string[] matchOnColumnList in MatchOn)
            {
                var itemsNeedingMatch = _matchOnResolveQueue.Where(item => item.ResolvedIds.Count == 0).ToList();
                if (itemsNeedingMatch.Count == 0)
                {
                    break;
                }

                if (matchOnColumnList.Length == 1)
                {
                    // Single column - use In operator for efficiency
                    var matchColumn = matchOnColumnList[0];
                    var matchValues = itemsNeedingMatch.Select(item =>
                    {
                        var val = item.InputEntity.GetAttributeValue<object>(matchColumn);
                        if (val is EntityReference er) return (object)er.Id;
                        if (val is OptionSetValue osv) return (object)osv.Value;
                        return val;
                    }).Distinct().ToList();

                    // Check if any values are null - this indicates a conversion problem
                    if (matchValues.Any(v => v == null))
                    {
                        WriteError(new ErrorRecord(
                            new Exception($"MatchOn column '{matchColumn}' has null value in InputObject - conversion may have failed"),
                            null,
                            ErrorCategory.InvalidData,
                            null));
                        continue;
                    }

                    // For single-column MatchOn with single value, use QueryByAttribute for compatibility
                    if (matchValues.Count == 1)
                    {
                        // Use QueryByAttribute - same as original non-batched code
                        QueryByAttribute matchOnQuery = new QueryByAttribute(TableName);
                        // Include both the primary ID and the match column for matching back
                        matchOnQuery.ColumnSet = new ColumnSet(new[] { entityMetadata.PrimaryIdAttribute, matchColumn });
                        matchOnQuery.AddAttributeValue(matchColumn, matchValues[0]);
                        
                        // When AllowMultipleMatches is off, limit to 2 records to detect multiple matches efficiently
                        if (!AllowMultipleMatches.IsPresent)
                        {
                            matchOnQuery.TopCount = 2;
                        }

                        WriteVerbose($"Retrieving records by MatchOn ({matchColumn}) using QueryByAttribute");

                        // Match back to items using streaming
                        foreach (var entity in QueryHelpers.ExecuteQueryWithPaging(matchOnQuery, Connection, WriteVerbose))
                        {
                            foreach (var item in itemsNeedingMatch)
                            {
                                var itemValue = item.InputEntity.GetAttributeValue<object>(matchColumn);
                                if (itemValue is EntityReference er) itemValue = er.Id;
                                if (itemValue is OptionSetValue osv) itemValue = osv.Value;

                                var recValue = entity.GetAttributeValue<object>(matchColumn);
                                if (recValue is EntityReference er2) recValue = er2.Id;
                                if (recValue is OptionSetValue osv2) recValue = osv2.Value;
                                
                                if (QueryHelpers.AreValuesEqual(itemValue, recValue))
                                {
                                    item.ResolvedIds.Add(entity.Id);
                                }
                            }
                        }
                    }
                    else
                    {
                        // For multiple values, use QueryExpression with In operator
                        var query = new QueryExpression(TableName)
                        {
                            // Include both the primary ID and the match column for matching back
                            ColumnSet = new ColumnSet(new[] { entityMetadata.PrimaryIdAttribute, matchColumn })
                        };

                        // When AllowMultipleMatches is off, limit to 2 records to detect multiple matches efficiently
                        if (!AllowMultipleMatches.IsPresent)
                        {
                            query.TopCount = 2;
                        }

                        query.Criteria.AddCondition(matchColumn, ConditionOperator.In, matchValues.ToArray());

                        WriteVerbose($"Retrieving records by MatchOn ({matchColumn}) in batch");

                        // Match back to items using streaming
                        foreach (var entity in QueryHelpers.ExecuteQueryWithPaging(query, Connection, WriteVerbose))
                        {
                            foreach (var item in itemsNeedingMatch)
                            {
                                var itemValue = item.InputEntity.GetAttributeValue<object>(matchColumn);
                                if (itemValue is EntityReference er) itemValue = er.Id;
                                if (itemValue is OptionSetValue osv) itemValue = osv.Value;

                                var recValue = entity.GetAttributeValue<object>(matchColumn);
                                if (recValue is EntityReference er2) recValue = er2.Id;
                                if (recValue is OptionSetValue osv2) recValue = osv2.Value;
                                
                                if (QueryHelpers.AreValuesEqual(itemValue, recValue))
                                {
                                    item.ResolvedIds.Add(entity.Id);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Multi-column - use Or with And conditions
                    var query = new QueryExpression(TableName)
                    {
                        // Include the primary ID and all match columns for matching back
                        ColumnSet = new ColumnSet(new[] { entityMetadata.PrimaryIdAttribute }.Concat(matchOnColumnList).ToArray())
                    };

                    // When AllowMultipleMatches is off, limit to 2 records to detect multiple matches efficiently
                    if (!AllowMultipleMatches.IsPresent)
                    {
                        query.TopCount = 2;
                    }

                    var orFilter = new FilterExpression(LogicalOperator.Or);

                    foreach (var item in itemsNeedingMatch)
                    {
                        var andFilter = new FilterExpression(LogicalOperator.And);
                        foreach (var matchColumn in matchOnColumnList)
                        {
                            var queryValue = item.InputEntity.GetAttributeValue<object>(matchColumn);
                            if (queryValue is EntityReference er1) queryValue = er1.Id;
                            if (queryValue is OptionSetValue osv1) queryValue = osv1.Value;

                            andFilter.AddCondition(matchColumn, ConditionOperator.Equal, queryValue);
                        }
                        orFilter.AddFilter(andFilter);
                    }

                    query.Criteria.AddFilter(orFilter);

                    WriteVerbose($"Retrieving records by MatchOn ({string.Join(",", matchOnColumnList)}) in batch");

                    // Match back to items using streaming
                    foreach (var entity in QueryHelpers.ExecuteQueryWithPaging(query, Connection, WriteVerbose))
                    {
                        foreach (var item in itemsNeedingMatch)
                        {
                            bool allMatch = matchOnColumnList.All(col =>
                            {
                                var itemValue = item.InputEntity.GetAttributeValue<object>(col);
                                var recValue = entity.GetAttributeValue<object>(col);

                                if (itemValue is EntityReference er1) itemValue = er1.Id;
                                if (itemValue is OptionSetValue osv1) itemValue = osv1.Value;
                                if (recValue is EntityReference er2) recValue = er2.Id;
                                if (recValue is OptionSetValue osv2) recValue = osv2.Value;

                                return QueryHelpers.AreValuesEqual(itemValue, recValue);
                            });
                            
                            if (allMatch)
                            {
                                item.ResolvedIds.Add(entity.Id);
                            }
                        }
                    }
                }
            }

            // Process all items in the batch - validate and delete
            foreach (var item in _matchOnResolveQueue)
            {
                if (item.ResolvedIds.Count == 0)
                {
                    if (!IfExists.IsPresent)
                    {
                        WriteError(new ErrorRecord(
                            new Exception("No records found matching the MatchOn criteria"),
                            null,
                            ErrorCategory.ObjectNotFound,
                            item.InputObject));
                    }
                    else
                    {
                        WriteVerbose("No records found matching the MatchOn criteria");
                    }
                }
                else if (item.ResolvedIds.Count > 1 && !AllowMultipleMatches.IsPresent)
                {
                    WriteError(new ErrorRecord(
                        new Exception($"MatchOn criteria matched {item.ResolvedIds.Count} records. Use -AllowMultipleMatches to delete all matching records."),
                        null,
                        ErrorCategory.InvalidOperation,
                        item.InputObject));
                }
                else
                {
                    // Process deletions for this item
                    foreach (var idToDelete in item.ResolvedIds)
                    {
                        ProcessDeleteById(item.InputObject, idToDelete);
                    }
                }
            }

            _matchOnResolveQueue.Clear();
        }

        /// <summary>
        /// Processes deletion of a single record by ID.
        /// </summary>
        private void ProcessDeleteById(PSObject inputObject, Guid idToDelete)
        {
            var context = new DeleteOperationContext(
                inputObject,
                TableName,
                idToDelete,
                this,
                _metadataFactory,
                Connection,
                WriteVerbose,
                WriteError,
                ShouldProcess);

            context.CreateRequest();

            if (_parallelProcessor != null)
            {
                WriteVerbose(string.Format("Queued delete of {0}:{1} for parallel processing", TableName, idToDelete));
                _parallelProcessor.QueueOperation(context);
            }
            else if (_batchProcessor != null)
            {
                WriteVerbose(string.Format("Added delete of {0}:{1} to batch", TableName, idToDelete));
                _batchProcessor.QueueOperation(context);
            }
            else
            {
                context.ExecuteNonBatched();
            }
        }

        /// <summary>Completes cmdlet processing.</summary>
        protected override void EndProcessing()
        {
            base.EndProcessing();

            // Process any remaining queued MatchOn items
            if (_matchOnResolveQueue != null && _matchOnResolveQueue.Count > 0)
            {
                ProcessMatchOnBatch();
            }

            if (_parallelProcessor != null)
            {
                _parallelProcessor.WaitForCompletion();
            }
            else if (_batchProcessor != null)
            {
                _batchProcessor.Flush();
                _batchProcessor.ProcessRetries();
            }

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


    }
}
