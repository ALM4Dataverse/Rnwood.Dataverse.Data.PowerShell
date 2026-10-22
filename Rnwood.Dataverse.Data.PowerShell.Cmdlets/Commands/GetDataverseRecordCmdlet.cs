using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;

namespace Rnwood.Dataverse.Data.PowerShell.Commands
{
    /// <summary>
    /// Retrieves records from a Dataverse table using various query methods.
    /// </summary>
    [Cmdlet(VerbsCommon.Get, "DataverseRecord")]
    [OutputType(typeof(IEnumerable<PSObject>))]
    public partial class GetDataverseRecordCmdlet : OrganizationServiceCmdlet
    {
        private const int MaxXorItems = 8;

        private const string PARAMSET_FETCHXML = "FetchXml";

        private const string PARAMSET_SIMPLE = "Simple";

        private const string PARAMSET_MATCHON = "MatchOn";

        /// <summary>
        /// Gets or sets the logical name of the table to query.
        /// </summary>
        [ArgumentCompleter(typeof(TableNameArgumentCompleter))]
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = true, Position = 0, HelpMessage = "Logical name of table for which to retrieve records")]
        [Parameter(ParameterSetName = PARAMSET_MATCHON, Mandatory = true, HelpMessage = "Logical name of table for which to retrieve records")]
        [Alias("EntityName")]
        public string TableName { get; set; }

        /// <summary>
        /// Gets or sets the input object containing values to match against.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_MATCHON, Mandatory = true, ValueFromPipeline = true, HelpMessage = "Object containing values to match against. Property names must match column names in the table.")]
        public PSObject InputObject { get; set; }

        /// <summary>
        /// List of list of column names to match against values in the InputObject. The first list that returns matches is used.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_MATCHON, Mandatory = true, HelpMessage = "List of list of column names to match against values in the InputObject. The first list that returns matches is used.")]
        public string[][] MatchOn { get; set; }

        /// <summary>
        /// If specified, rejects lookup values that require a query to resolve a related record. Supply a reference object with TableName and Id, or TableName and KeyAttributes, instead.
        /// </summary>
        [Parameter(HelpMessage = "If specified, rejects lookup values that require a query to resolve a related record. Supply a reference object with TableName and Id, or TableName and KeyAttributes, instead.")]
        public SwitchParameter NoLookup { get; set; }

        /// <summary>
        /// If specified, returns all records matching the MatchOn criteria. Without this switch, an error is raised if MatchOn finds multiple matches.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_MATCHON, HelpMessage = "If specified, returns all records matching the MatchOn criteria. Without this switch, an error is raised if MatchOn finds multiple matches.")]
        public SwitchParameter AllowMultipleMatches { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to write the total record count to verbose output.
        /// </summary>
        [Parameter(HelpMessage = "If set writes total record count matching query to verbose output")]
        public SwitchParameter VerboseRecordCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to return only the count of records instead of the records themselves.
        /// </summary>
        [Parameter(HelpMessage = "If set, writes total record count matching query to output instead of results")]
        public SwitchParameter RecordCount { get; set; }
        /// <summary>
        /// FetchXml to use
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_FETCHXML, HelpMessage = "FetchXml to use")]
        public string FetchXml
        {
            get;
            set;
        }
        /// <summary>
        /// One or more hashtables that define filters to apply to the query.
        /// Each hashtable's entries are combined with AND; multiple hashtables are combined with OR.
        /// Filter keys may be "column" or "column:Operator" where Operator is a
        /// name from the ConditionOperator enum (e.g. GreaterThan, NotEqual).
        /// Values may be a literal, an array (treated as IN), $null (treated as ISNULL),
        /// or a nested hashtable with keys "value" and "operator" to specify operator and value
        /// (e.g. @{age=@{value=25; operator='GreaterThan'}}).
        /// Supports grouped filters using 'and', 'or', 'not', or 'xor' keys with nested hashtables for complex logical expressions.
        /// </summary>
        [ArgumentCompleter(typeof(FilterValuesArgumentCompleter))]
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = false, HelpMessage = "One or more hashtables to filter records. Each hashtable's entries are combined with AND; multiple hashtables are combined with OR. Keys may be 'column' or 'column:Operator' (Operator is a ConditionOperator name). Values may be a literal, an array (treated as IN), $null (treated as ISNULL), or a nested hashtable with keys 'value' and 'operator' (e.g. @{age=@{value=25; operator='GreaterThan'}}). Supports grouped filters using 'and', 'or', 'not', or 'xor' keys with nested hashtables for complex logical expressions. Examples: @{firstname='bob'; age=25}, @{firstname='sue'} => (firstname=bob AND age=25) OR (firstname=sue).")]
        [Alias("IncludeFilter")]
        public Hashtable[] FilterValues
        {
            get;
            set;
        }
        /// <summary>
        /// Extra criteria to apply to query
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, HelpMessage = "Extra criteria to apply to query")]
        public FilterExpression Criteria
        {
            get;
            set;
        }
        /// <summary>
        /// Link entities to apply to query. Accepts DataverseLinkEntity objects or simplified hashtable syntax.
        /// Hashtable format: @{ 'fromEntity.fromAttribute' = 'toEntity.toAttribute'; type = 'Inner'; alias = 'aliasName'; filter = @{...}; links = @(...) }.
        /// The 'filter' key uses the same format as FilterValues and supports 'and', 'or', 'not', 'xor' operators.
        /// The 'links' key allows nested child joins.
        /// </summary>
        [ArgumentCompleter(typeof(LinksArgumentCompleter))]
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, HelpMessage = "Link entities to apply to query. Accepts DataverseLinkEntity objects or simplified hashtable syntax: @{ 'fromEntity.fromAttribute' = 'toEntity.toAttribute'; type = 'Inner'; alias = 'aliasName'; filter = @{...}; links = @(...) }. The 'filter' key uses the same format as FilterValues and supports 'and', 'or', 'not', 'xor' operators. The 'links' key allows nested child joins.")]
        public DataverseLinkEntity[] Links
        {
            get;
            set;
        }
        /// <summary>
        /// List of hashtables of field names/values to exclude. Defaults to a NOT EQUAL condition for values (or IS NOT NULL when $null is supplied).
        /// Multiple hashtables are combined using AND by default; use -ExcludeFilterOr to combine them using OR instead.
        /// For example: @{firstname="bob", age=25}, @{lastname="smith"}:
        /// - Default (AND): excludes records matching both hashtables (firstname='bob' AND age=25) AND (lastname='smith').
        /// - With -ExcludeFilterOr (OR): excludes records matching either hashtable (firstname='bob' AND age=25) OR (lastname='smith').
        /// Supports grouped filters using 'and', 'or', 'not', or 'xor' keys with nested hashtables for complex logical expressions.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = false, HelpMessage = "List of hashtables of field names/values to exclude. Defaults to a NOT EQUAL condition for values (or IS NOT NULL when $null is supplied). Multiple hashtables are combined using AND by default; use -ExcludeFilterOr to combine them using OR instead. Supports grouped filters using 'and', 'or', 'not', or 'xor' keys with nested hashtables for complex logical expressions. e.g. @{firstname=\"bob\", age=25}, @{lastname=\"smith\"}")]
        [Alias("ExcludeFilter")]
        [ArgumentCompleter(typeof(FilterValuesArgumentCompleter))]
        public Hashtable[] ExcludeFilterValues
        {
            get;
            set;
        }

        /// <summary>
        /// When specified, multiple exclude hashtables are combined using OR instead of the default AND.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = false, HelpMessage = "If specified multiple hashtables exclude filters will be logically combined using OR instead of the default of AND")]
        public SwitchParameter ExcludeFilterOr
        {
            get;
            set;
        }
        /// <summary>
        /// If specified only active records (statecode=0 or isactive=true) will be output
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = false, HelpMessage = "If specified only active records (statecode=0 or isactive=true) will be output")]
        public SwitchParameter ActiveOnly
        {
            get;
            set;
        }
        /// <summary>
        /// List of primary keys (IDs) of records to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, HelpMessage = "List of primary keys (IDs) of records to retrieve.")]
        public Guid[] Id
        {
            get;
            set;
        }
        /// <summary>
        /// List of names (primary attribute value) of records to retrieve.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, HelpMessage = "List of names (primary attribute value) of records to retrieve.")]
        public string[] Name
        {
            get;
            set;
        }
        /// <summary>
        /// List of record ids to exclude
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, HelpMessage = "List of record ids to exclude")]
        public Guid[] ExcludeId
        {
            get;
            set;
        }
        /// <summary>
        /// List of columns to return in records (default is all). Each column name may be suffixed with :Raw or :Display to override the value type which will be output from the default
        /// </summary>
        [ArgumentCompleter(typeof(ColumnNamesArgumentCompleter))]
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = false, HelpMessage = "List of columns to return in records (default is all). Each column name may be suffixed with :Raw or :Display to override the value type which will be output from the default")]
        [Parameter(ParameterSetName = PARAMSET_MATCHON, Mandatory = false, HelpMessage = "List of columns to return in records (default is all). Each column name may be suffixed with :Raw or :Display to override the value type which will be output from the default")]
        public string[] Columns
        {
            get;
            set;
        }
        /// <summary>
        /// List of columns to exclude from records (default is none). Ignored if Columns parameter is used.s
        /// </summary>
        [ArgumentCompleter(typeof(ColumnNamesArgumentCompleter))]
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = false, HelpMessage = "List of columns to exclude from records (default is none). Ignored if Columns parameter is used.s")]
        [Parameter(ParameterSetName = PARAMSET_MATCHON, Mandatory = false, HelpMessage = "List of columns to exclude from records (default is none). Ignored if Columns parameter is used.s")]
        public string[] ExcludeColumns
        {
            get;
            set;
        }
        /// <summary>
        /// List of columns to order records by. Suffix column name with - to sort descending. e.g \
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = false, HelpMessage = "List of columns to order records by. Suffix column name with - to sort descending. e.g \"age-\", \"lastname\" will sort by age descending then lastname ascending")]
        [Parameter(ParameterSetName = PARAMSET_MATCHON, Mandatory = false, HelpMessage = "List of columns to order records by. Suffix column name with - to sort descending. e.g \"age-\", \"lastname\" will sort by age descending then lastname ascending")]
        public string[] OrderBy
        {
            get;
            set;
        }
        /// <summary>
        /// Number of records to limit result to. Default is all results.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = false, HelpMessage = "Number of records to limit result to. Default is all results.")]
        [Parameter(ParameterSetName = PARAMSET_MATCHON, Mandatory = false, HelpMessage = "Number of records to limit result to. Default is all results.")]
        public int? Top
        {
            get;
            set;
        }
        /// <summary>
        /// Number of records to request per page. Default is 1000.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, Mandatory = false, HelpMessage = "Number of records to request per page. Default is 1000.")]
        [Parameter(ParameterSetName = PARAMSET_MATCHON, Mandatory = false, HelpMessage = "Number of records to request per page. Default is 1000.")]
        public int? PageSize
        {
            get;
            set;
        }

        /// <summary>
        /// Controls the maximum number of records to resolve in a single query when using MatchOn. Default is 500. Specify 1 to resolve one record at a time.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_MATCHON, HelpMessage = "Controls the maximum number of records to resolve in a single query when using MatchOn. Default is 500. Specify 1 to resolve one record at a time.")]
        public uint RetrievalBatchSize { get; set; } = 500;
        /// <summary>
        /// Outputs Names for lookup values. The default behaviour is to output the ID.
        /// </summary>
        [Parameter(HelpMessage = "Outputs Names for lookup values. The default behaviour is to output the ID.")]
        public SwitchParameter LookupValuesReturnName { get; set; }

        /// <summary>
        /// Includes system columns in output. Default is all columns except system columns. Ignored if Columns parameter is used.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, HelpMessage = "Includes system columns in output. Default is all columns except system columns. Ignored if Columns parameter is used.")]
        [Parameter(ParameterSetName = PARAMSET_MATCHON, HelpMessage = "Includes system columns in output. Default is all columns except system columns. Ignored if Columns parameter is used.")]
        public SwitchParameter IncludeSystemColumns { get; set; }

        /// <summary>
        /// Includes file and image columns in output. Default does not include them. Ignored if Columns parameter is used.
        /// </summary>
        [Parameter(ParameterSetName = PARAMSET_SIMPLE, HelpMessage = "Includes file and image metadata columns in output. Default does not include them. Ignored if Columns parameter is used.")]
        [Parameter(ParameterSetName = PARAMSET_MATCHON, HelpMessage = "Includes file and image metadata columns in output. Default does not include them. Ignored if Columns parameter is used")]
        public SwitchParameter IncludeFileAndImageMetadataColumns { get; set; }

        /// <summary>
        /// If specified, retrieves unpublished records instead of including published ones.
        /// </summary>
        [Parameter(HelpMessage = "If specified, retrieves unpublished records instead of including published ones.")]
        public SwitchParameter Unpublished { get; set; }

        private EntityMetadataFactory entiyMetadataFactory;
        private DataverseEntityConverter entityConverter;
        private EntityMetadata entityMetadata;
        private List<GetMatchOnItem> _matchOnQueue;
        private System.Threading.CancellationTokenSource _userCancellationCts;

        /// <summary>
        /// Initializes the cmdlet and sets up required helpers.
        /// </summary>
        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            // Initialize cancellation token source for this pipeline invocation
            _userCancellationCts = new System.Threading.CancellationTokenSource();

            // Initialize the queue for MatchOn parameter set
            if (ParameterSetName == PARAMSET_MATCHON)
            {
                _matchOnQueue = new List<GetMatchOnItem>();
            }
        }

        /// <summary>
        /// Processes each record by executing the configured query and writing results to the pipeline.
        /// </summary>
        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            // For MatchOn parameter set, queue items for batched processing
            if (ParameterSetName == PARAMSET_MATCHON)
            {
                entiyMetadataFactory = entiyMetadataFactory ?? new EntityMetadataFactory(Connection);
                entityConverter = entityConverter ?? new DataverseEntityConverter(Connection, entiyMetadataFactory);
                entityMetadata = entityMetadata ?? entiyMetadataFactory.GetLimitedMetadata(TableName);

                // Convert InputObject to Entity to extract match values
                var conversionOptions = new ConvertToDataverseEntityOptions { DisallowLookupQueries = NoLookup.IsPresent };
                Entity inputEntity = entityConverter.ConvertToDataverseEntity(InputObject, TableName, conversionOptions);

                _matchOnQueue.Add(new GetMatchOnItem
                {
                    InputObject = InputObject,
                    InputEntity = inputEntity,
                    MatchedRecords = new List<Entity>()
                });

                // Process batch if it's full
                if (_matchOnQueue.Count >= RetrievalBatchSize)
                {
                    ProcessMatchOnBatch();
                }
            }
            else
            {
                // For non-MatchOn parameter sets, execute query immediately
                ExecuteQuery();
            }
        }

        /// <summary>
        /// Completes cmdlet processing by handling any remaining queued MatchOn items.
        /// </summary>
        protected override void EndProcessing()
        {
            base.EndProcessing();

            // Process any remaining queued MatchOn items
            if (ParameterSetName == PARAMSET_MATCHON && _matchOnQueue != null && _matchOnQueue.Count > 0)
            {
                ProcessMatchOnBatch();
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

        /// <summary>
        /// Strips :Raw or :Display suffixes from column names.
        /// </summary>
        private string[] StripColumnSuffixes(string[] columns)
        {
            if (columns == null) return null;
            return columns.Select(c => c.Split(':')[0]).ToArray();
        }

        /// <summary>
        /// Gets the value type suffix for a specific column name.
        /// Returns null if no suffix, "Raw" if :Raw suffix, "Display" if :Display suffix.
        /// </summary>
        private string GetColumnSuffix(string columnName)
        {
            if (Columns == null || columnName == null) return null;

            var matchingColumn = Columns.FirstOrDefault(c =>
                c.Split(':')[0].Equals(columnName, StringComparison.OrdinalIgnoreCase));

            if (matchingColumn == null) return null;

            var parts = matchingColumn.Split(':');
            if (parts.Length > 1)
            {
                var suffix = parts[1].ToLowerInvariant();
                if (suffix == "raw") return "Raw";
                if (suffix == "display") return "Display";
            }

            return null;
        }

        private ValueType GetColumnValueType(AttributeMetadata attribute)
        {
            // Check if there's an explicit suffix for this column
            string suffix = GetColumnSuffix(attribute.LogicalName);
            if (suffix != null)
            {
                return suffix == "Display" ? ValueType.Display : ValueType.Raw;
            }

            // Default behavior based on attribute type
            ValueType result;

            if (attribute.AttributeType.Value == AttributeTypeCode.Lookup || attribute.AttributeType.Value == AttributeTypeCode.Uniqueidentifier)
            {
                result = LookupValuesReturnName ? ValueType.Display : ValueType.Raw;
            }
            else
            {
                result = ValueType.Raw;
            }

            return result;
        }

        private void ExecuteQuery()
        {
            entiyMetadataFactory = new EntityMetadataFactory(Connection);
            entityConverter = new DataverseEntityConverter(Connection, entiyMetadataFactory);

            QueryExpression query;

            switch (ParameterSetName)
            {
                case PARAMSET_FETCHXML:
                    query = GetFetchXmlQuery();
                    Columns = query.ColumnSet.AllColumns ? null : query.ColumnSet.Columns.ToArray();   // Capture columns requested in FetchXML for use when converting to PSObject
                    entityMetadata = entiyMetadataFactory.GetLimitedMetadata(query.EntityName);
                    break;
                case PARAMSET_SIMPLE:
                    entityMetadata = entiyMetadataFactory.GetLimitedMetadata(TableName);
                    query = GetSimpleQuery();
                    Columns = query.ColumnSet.AllColumns ? null : query.ColumnSet.Columns.ToArray();
                    break;
                case PARAMSET_MATCHON:
                    entityMetadata = entiyMetadataFactory.GetLimitedMetadata(TableName);
                    query = GetMatchOnQuery();
                    Columns = query.ColumnSet.AllColumns ? null : query.ColumnSet.Columns.ToArray();
                    break;
                default:
                    throw new NotImplementedException($"ParameterSetName not implemented: {ParameterSetName}");
            }

            if (RecordCount || VerboseRecordCount)
            {
                long recordCount = GetRecordCount(query);

                WriteObject(recordCount);
                WriteVerbose("Total records matching query: " + recordCount);
            }

            if (!RecordCount)
            {
                long recordCount = 0;
                foreach (Entity entity in GetRecords(query))
                {
                    PSObject output = entityConverter.ConvertToPSObject(entity, GetColumnSet(""), GetColumnValueType);

                    WriteObject(output);

                    recordCount++;

                    if (Top.HasValue && recordCount == Top.Value)
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Processes a batch of MatchOn items using efficient batched queries.
        /// </summary>
        private void ProcessMatchOnBatch()
        {
            if (_matchOnQueue.Count == 0)
            {
                return;
            }

            WriteVerbose($"Processing MatchOn batch of {_matchOnQueue.Count} record(s)");

            // Try each MatchOn column list in order until records are found
            foreach (string[] matchOnColumnList in MatchOn)
            {
                var itemsNeedingMatch = _matchOnQueue.Where(item => item.MatchedRecords.Count == 0).ToList();
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

                    var query = new QueryExpression(TableName)
                    {
                        ColumnSet = Columns != null ? new ColumnSet(StripColumnSuffixes(Columns).Concat(new[] { matchColumn }).Distinct().ToArray()) : new ColumnSet(true)
                    };

                    // When AllowMultipleMatches is off, limit to 2 records to detect multiple matches efficiently
                    if (!AllowMultipleMatches.IsPresent)
                    {
                        query.TopCount = 2;
                    }

                    if (matchValues.Count == 1)
                    {
                        query.Criteria.AddCondition(matchColumn, ConditionOperator.Equal, matchValues[0]);
                    }
                    else
                    {
                        query.Criteria.AddCondition(matchColumn, ConditionOperator.In, matchValues.ToArray());
                    }

                    // Apply ordering, top, and page size if specified
                    ApplyOutputParameters(query);

                    WriteVerbose($"Retrieving records by MatchOn ({matchColumn}) in batch");

                    // Match back to items using streaming to avoid buffering all records
                    foreach (var entity in QueryHelpers.ExecuteQueryWithPaging(query, Connection, WriteVerbose, () => Stopping, _userCancellationCts?.Token, Unpublished.IsPresent))
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
                                item.MatchedRecords.Add(entity);
                                item.MatchedOnColumns = matchOnColumnList;
                            }
                        }
                    }
                }
                else
                {
                    // Multi-column - use Or with And conditions
                    var query = new QueryExpression(TableName)
                    {
                        ColumnSet = Columns != null ? new ColumnSet(StripColumnSuffixes(Columns).Concat(matchOnColumnList).Distinct().ToArray()) : new ColumnSet(true)
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

                    // Apply ordering, top, and page size if specified
                    ApplyOutputParameters(query);

                    WriteVerbose($"Retrieving records by MatchOn ({string.Join(",", matchOnColumnList)}) in batch");

                    // Match back to items using streaming to avoid buffering all records
                    foreach (var entity in QueryHelpers.ExecuteQueryWithPaging(query, Connection, WriteVerbose, () => Stopping, _userCancellationCts?.Token, Unpublished.IsPresent))
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
                                item.MatchedRecords.Add(entity);
                                item.MatchedOnColumns = matchOnColumnList;
                            }
                        }
                    }
                }
            }

            // Output matched records
            foreach (var item in _matchOnQueue)
            {
                if (item.MatchedRecords.Count == 0)
                {
                    // No matches found - no output
                }
                else if (item.MatchedRecords.Count == 1)
                {
                    // Single match - output it
                    PSObject output = entityConverter.ConvertToPSObject(item.MatchedRecords[0], GetColumnSet(""), GetColumnValueType);
                    WriteObject(output);
                }
                else if (item.MatchedRecords.Count > 1)
                {
                    if (AllowMultipleMatches.IsPresent)
                    {
                        // Multiple matches allowed - output all
                        foreach (var record in item.MatchedRecords)
                        {
                            PSObject output = entityConverter.ConvertToPSObject(record, GetColumnSet(""), GetColumnValueType);
                            WriteObject(output);
                        }
                    }
                    else
                    {
                        // Multiple matches not allowed - error
                        string matchOnSummary = string.Join(", ", (item.MatchedOnColumns ?? MatchOn[0]).Select(c => $"{c}='{item.InputEntity.GetAttributeValue<object>(c)}'"));
                        WriteError(new ErrorRecord(
                            new Exception($"Match on values {matchOnSummary} resulted in more than one record. Use -AllowMultipleMatches to retrieve all matching records."),
                            null,
                            ErrorCategory.InvalidOperation,
                            item.InputObject));
                    }
                }
            }

            _matchOnQueue.Clear();
        }

        private QueryExpression GetSimpleQuery()
        {
            QueryExpression query = new QueryExpression(TableName);
            if (RecordCount)
            {
                query.ColumnSet = new ColumnSet(false);
            }
            else if (Columns != null && !Columns.Any(c => c.Split(':')[0].Equals("calendarrules", StringComparison.OrdinalIgnoreCase)))
            {
                query.ColumnSet = new ColumnSet(StripColumnSuffixes(Columns));
            }
            else
            {
                query.ColumnSet = new ColumnSet(DataverseEntityConverter.GetAllColumnNames(entityMetadata, IncludeSystemColumns, ExcludeColumns, IncludeFileAndImageMetadataColumns));
            }
            query.Criteria.FilterOperator = LogicalOperator.And;

            if (Id != null)
            {
                query.Criteria.AddCondition(entityMetadata.PrimaryIdAttribute, ConditionOperator.In, Id.Cast<object>().ToArray());
            }

            if (ActiveOnly.IsPresent)
            {
                AddActiveFilter(query);
            }

            if (ExcludeId != null)
            {
                query.Criteria.AddCondition(entityMetadata.PrimaryIdAttribute, ConditionOperator.NotIn, ExcludeId.Cast<object>().ToArray());
            }

            if (Name != null)
            {
                FilterExpression nameFilterExpression = query.Criteria.AddFilter(LogicalOperator.Or);

                foreach (string name in Name)
                {
                    nameFilterExpression.AddCondition(entityMetadata.PrimaryNameAttribute, ConditionOperator.Equal, name);
                }
            }

            if (FilterValues != null)
            {
                FilterExpression includesFilterExpression = query.Criteria.AddFilter(LogicalOperator.Or);
                FilterHelpers.ProcessHashFilterValues(includesFilterExpression, FilterValues, false);
            }

            if (ExcludeFilterValues != null)
            {
                // Build the inverted exclude filters. Top-level combination across the exclude
                // hashtables is AND by default (a record is excluded only if it matches all
                // provided hashtables). When -ExcludeFilterOr is specified the top-level
                // combination becomes OR (a record is excluded if it matches any provided hashtable).
                FilterExpression excludesFilterExpression = query.Criteria.AddFilter(ExcludeFilterOr.IsPresent ? LogicalOperator.Or : LogicalOperator.And);
                FilterHelpers.ProcessHashFilterValues(excludesFilterExpression, ExcludeFilterValues, true);
            }

            if (Criteria != null)
            {
                query.Criteria.AddFilter(Criteria);
            }

            if (Links != null)
            {
                query.LinkEntities.AddRange(Links.Select(l => l.LinkEntity));
            }

            if (OrderBy != null)
            {
                foreach (string orderByColumnName in OrderBy)
                {
                    query.AddOrder(orderByColumnName.TrimEnd('+', '-'), orderByColumnName.EndsWith("-") ? OrderType.Descending : OrderType.Ascending);
                }
            }

            return query;
        }

        private QueryExpression GetMatchOnQuery()
        {
            // Convert InputObject to Entity to extract match values
            var conversionOptions = new ConvertToDataverseEntityOptions { DisallowLookupQueries = NoLookup.IsPresent };
            Entity inputEntity = entityConverter.ConvertToDataverseEntity(InputObject, TableName, conversionOptions);

            QueryExpression finalQuery = null;
            int matchCount = 0;

            // Try each MatchOn column list in order
            foreach (string[] matchOnColumnList in MatchOn)
            {
                QueryByAttribute candidateQuery = new QueryByAttribute(TableName);
                candidateQuery.ColumnSet = Columns != null ? new ColumnSet(StripColumnSuffixes(Columns)) : new ColumnSet(true);

                if (!AllowMultipleMatches.IsPresent)
                {
                    candidateQuery.TopCount = 2; // Get 2 to detect multiple matches
                }

                // Add conditions for each column in the match list
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

                    candidateQuery.AddAttributeValue(matchOnColumn, queryValue);
                }

                // Execute the query to see if this match criteria returns records
                var testResults = Connection.RetrieveMultiple(candidateQuery).Entities;
                matchCount = testResults.Count;

                if (matchCount == 1)
                {
                    // Convert to QueryExpression for consistency
                    finalQuery = new QueryExpression(TableName);
                    finalQuery.ColumnSet = candidateQuery.ColumnSet;
                    finalQuery.TopCount = candidateQuery.TopCount;
                    finalQuery.Criteria.FilterOperator = LogicalOperator.And;
                    for (int i = 0; i < candidateQuery.Attributes.Count; i++)
                    {
                        finalQuery.Criteria.AddCondition(candidateQuery.Attributes[i], ConditionOperator.Equal, candidateQuery.Values[i]);
                    }

                    // Apply ordering, top, and page size if specified
                    ApplyOutputParameters(finalQuery);
                    break;
                }
                else if (matchCount > 1)
                {
                    if (AllowMultipleMatches.IsPresent)
                    {
                        // Convert to QueryExpression without TopCount restriction
                        finalQuery = new QueryExpression(TableName);
                        finalQuery.ColumnSet = candidateQuery.ColumnSet;
                        finalQuery.Criteria.FilterOperator = LogicalOperator.And;
                        for (int i = 0; i < candidateQuery.Attributes.Count; i++)
                        {
                            finalQuery.Criteria.AddCondition(candidateQuery.Attributes[i], ConditionOperator.Equal, candidateQuery.Values[i]);
                        }

                        // Apply ordering, top, and page size if specified
                        ApplyOutputParameters(finalQuery);
                        break;
                    }
                    else
                    {
                        string matchOnSummary = string.Join(", ", matchOnColumnList.Select(c => $"{c}='{inputEntity.GetAttributeValue<object>(c)}'"));
                        throw new Exception($"Match on values {matchOnSummary} resulted in more than one record. Use -AllowMultipleMatches to retrieve all matching records.");
                    }
                }
            }

            if (finalQuery == null)
            {
                // No matches found - return an empty query that will return no results
                finalQuery = new QueryExpression(TableName);
                finalQuery.ColumnSet = Columns != null ? new ColumnSet(StripColumnSuffixes(Columns)) : new ColumnSet(true);
                finalQuery.Criteria.AddCondition(entityMetadata.PrimaryIdAttribute, ConditionOperator.Equal, Guid.Empty);
            }

            return finalQuery;
        }

        /// <summary>
        /// Applies output-related parameters (OrderBy, Top) to a query.
        /// PageSize is handled by GetRecords method.
        /// </summary>
        private void ApplyOutputParameters(QueryExpression query)
        {
            if (OrderBy != null)
            {
                foreach (string orderByColumnName in OrderBy)
                {
                    query.AddOrder(orderByColumnName.TrimEnd('+', '-'), orderByColumnName.EndsWith("-") ? OrderType.Descending : OrderType.Ascending);
                }
            }

            if (Top.HasValue)
            {
                query.TopCount = Top.Value;
            }
        }

        // Filter parsing and helper methods were moved to FilterHelpers to
        // avoid duplication between Get-DataverseRecord and DataverseLinkEntity.
        private QueryExpression GetFetchXmlQuery()
        {
            QueryExpression query;
            FetchXmlToQueryExpressionRequest translateQueryRequest = new FetchXmlToQueryExpressionRequest { FetchXml = FetchXml };
            var translateQueryResponse = (FetchXmlToQueryExpressionResponse)Connection.Execute(translateQueryRequest);
            query = translateQueryResponse.Query;
            return query;
        }

        // Combination and hashtable conversion helpers moved to FilterHelpers.

        // ToHashtable moved to FilterHelpers.

        private long GetRecordCount(QueryExpression query)
        {
            ColumnSet oldColumnSet = query.ColumnSet;
            query.ColumnSet = new ColumnSet(false);

            long result = GetRecords(query).LongCount();

            query.ColumnSet = oldColumnSet;
            return result;
        }

        private IEnumerable<Entity> GetRecords(QueryExpression query)
        {
            return QueryHelpers.ExecuteQueryWithPaging(query, Connection, WriteVerbose, () => Stopping, _userCancellationCts?.Token, Unpublished.IsPresent);
        }


        private void AddActiveFilter(QueryExpression query)
        {
            bool filterIncludesState = (FilterValues != null
                    && FilterValues.Any(fv =>
                        fv.Keys.Cast<string>().Any(k =>
                            k.Equals("statuscode", StringComparison.OrdinalIgnoreCase)
                            || k.Equals("statecode", StringComparison.OrdinalIgnoreCase)
                            || k.Equals("isdisabled", StringComparison.OrdinalIgnoreCase))
                        )
                    )
                    || (ExcludeFilterValues != null
                        && ExcludeFilterValues.Any(fv =>
                            fv.Keys.Cast<string>().Any(k =>
                                k.Equals("statuscode", StringComparison.OrdinalIgnoreCase)
                                || k.Equals("statecode", StringComparison.OrdinalIgnoreCase)
                                || k.Equals("isdisabled", StringComparison.OrdinalIgnoreCase))
                            )
                        );

            if (!filterIncludesState)
            {
                AddActiveFilter(query.Criteria, TableName);

                if (entityMetadata.IsIntersect.GetValueOrDefault())
                {
                    ManyToManyRelationshipMetadata manyToManyRelationshipMetadata = entityMetadata.ManyToManyRelationships[0];

                    LinkEntity link1 = query.AddLink(manyToManyRelationshipMetadata.Entity1LogicalName, manyToManyRelationshipMetadata.Entity1IntersectAttribute, manyToManyRelationshipMetadata.Entity1IntersectAttribute);
                    AddActiveFilter(link1.LinkCriteria, manyToManyRelationshipMetadata.Entity1LogicalName);

                    LinkEntity link2 = query.AddLink(manyToManyRelationshipMetadata.Entity2LogicalName, manyToManyRelationshipMetadata.Entity2IntersectAttribute, manyToManyRelationshipMetadata.Entity2IntersectAttribute);
                    AddActiveFilter(link2.LinkCriteria, manyToManyRelationshipMetadata.Entity2LogicalName);
                }
            }
        }

        private void AddActiveFilter(FilterExpression criteria, string entityName)
        {
            EntityMetadata entityMetadata = entiyMetadataFactory.GetLimitedMetadata(entityName);

            if (entityMetadata.Attributes.Any(
                    a => string.Equals(a.LogicalName, "statecode", StringComparison.OrdinalIgnoreCase)))
            {
                criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            }

            if (entityMetadata.Attributes.Any(
                    a => string.Equals(a.LogicalName, "isdisabled", StringComparison.OrdinalIgnoreCase)))
            {
                criteria.AddCondition("isdisabled", ConditionOperator.Equal, false);
            }
        }

        private ColumnSet GetColumnSet(string path)
        {
            Func<string, string> getPath = (c) =>
                {
                    string[] elements = c.Split(new[] { '.' });
                    return string.Join(".", elements.Take(elements.Count() - 1));
                };

            string[] columnNames = null;
            if (Columns != null)
            {
                columnNames = Columns.Select(f => f.Split(':')[0]).Except(ExcludeColumns ?? new string[0]).ToArray();
            }

            return new ColumnSet((columnNames ?? DataverseEntityConverter.GetAllColumnNames(entityMetadata, IncludeSystemColumns, ExcludeColumns, IncludeFileAndImageMetadataColumns)).Where(c => getPath(c) == path).ToArray());
        }
    }
}