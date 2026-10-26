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
    internal class SetBatchProcessor
    {
        private readonly List<SetOperationContext> _nextBatchItems;
        private readonly List<SetOperationContext> _pendingRetries;
        private readonly uint _batchSize;
        private readonly bool _useExecuteMultiple;
        private readonly IOrganizationService _connection;
        private readonly Action<string> _writeVerbose;
        private readonly Func<string, bool> _shouldProcess;
        private readonly Func<bool> _isStopping;
        private readonly CancellationToken _cancellationToken;
        private readonly Action<Guid?> _setCallerId;
        private readonly Func<Guid> _getCallerId;
        private Guid? _nextBatchCallerId;

        public SetBatchProcessor(
            uint batchSize,
            bool useExecuteMultiple,
            IOrganizationService connection,
            Action<string> writeVerbose,
            Func<string, bool> shouldProcess,
            Func<bool> isStopping,
            CancellationToken cancellationToken,
            Action<Guid?> setCallerId,
            Func<Guid> getCallerId)
        {
            _batchSize = batchSize;
            _useExecuteMultiple = useExecuteMultiple;
            _connection = connection;
            _writeVerbose = writeVerbose;
            _shouldProcess = shouldProcess;
            _isStopping = isStopping;
            _cancellationToken = cancellationToken;
            _setCallerId = setCallerId;
            _getCallerId = getCallerId;
            _nextBatchItems = new List<SetOperationContext>();
            _pendingRetries = new List<SetOperationContext>();
        }

        /// <summary>
        /// Adds an operation to the batch, executing the batch if it reaches the batch size or caller ID changes.
        /// </summary>
        public void QueueOperation(SetOperationContext context)
        {
            if (_nextBatchItems.Any() && _nextBatchCallerId != context.CallerId)
            {
                ExecuteBatch();
            }

            _nextBatchCallerId = context.CallerId;
            _nextBatchItems.Add(context);

            if (_nextBatchItems.Count >= _batchSize)
            {
                ExecuteBatch();
            }
        }

        /// <summary>
        /// Executes any remaining operations in the batch.
        /// </summary>
        public void Flush()
        {
            if (_nextBatchItems.Count > 0)
            {
                ExecuteBatch();
            }
        }

        /// <summary>
        /// Processes all pending retries.
        /// </summary>
        public void ProcessRetries()
        {
            while (!_isStopping() && !_cancellationToken.IsCancellationRequested && _pendingRetries.Count > 0)
            {
                DateTime now = DateTime.UtcNow;
                var readyForRetry = _pendingRetries.Where(r => r.NextRetryTime <= now).ToList();

                if (readyForRetry.Count == 0)
                {
                    // Calculate wait time for next retry
                    var nextRetryTime = _pendingRetries.Min(r => r.NextRetryTime);
                    var waitTimeMs = (int)Math.Max(100, (nextRetryTime - now).TotalMilliseconds);

                    // Write verbose message once and sleep for the full duration
                    _writeVerbose($"Waiting {waitTimeMs / 1000.0:F1}s for next retry batch...");
                    
                    // Sleep for the full wait time, checking cancellation periodically
                    int sleptMs = 0;
                    while (sleptMs < waitTimeMs && !_isStopping() && !_cancellationToken.IsCancellationRequested)
                    {
                        int sleepChunkMs = Math.Min(100, waitTimeMs - sleptMs);
                        Thread.Sleep(sleepChunkMs);
                        sleptMs += sleepChunkMs;
                    }

                    continue;
                }

                // Remove from pending and add to batch for retry
                foreach (var item in readyForRetry)
                {
                    _pendingRetries.Remove(item);
                    QueueOperation(item);
                }

                Flush();
            }
        }

        private void ExecuteBatch()
        {
            if (_nextBatchItems.Count == 0)
            {
                return;
            }

            List<SetOperationContext> batchItems = _nextBatchItems.ToList();
            if (!_shouldProcess("Execute batch of requests:\n" + string.Join("\n", batchItems.Select(r => r.ToString()))))
            {
                _nextBatchItems.Clear();
                return;
            }

            _nextBatchItems.Clear();

            if (_useExecuteMultiple)
            {
                ExecuteExecuteMultipleBatch(batchItems);
            }
            else
            {
                ExecuteBatchWithBulkMessages(batchItems);
            }
        }

        private void ExecuteBatchWithBulkMessages(List<SetOperationContext> batchItems)
        {
            List<SetOperationContext> executeMultipleItems = new List<SetOperationContext>();
            int index = 0;
            while (index < batchItems.Count)
            {
                if (!TryGetBulkOperation(batchItems[index], out OrganizationRequest request, out Entity target))
                {
                    executeMultipleItems.Add(batchItems[index]);
                    index++;
                    continue;
                }

                List<SetOperationContext> bulkItems = new List<SetOperationContext> { batchItems[index] };
                int nextIndex = index + 1;
                while (nextIndex < batchItems.Count &&
                    TryGetBulkOperation(batchItems[nextIndex], out OrganizationRequest nextRequest, out Entity nextTarget) &&
                    CanShareBulkRequest(request, target, nextRequest, nextTarget))
                {
                    bulkItems.Add(batchItems[nextIndex]);
                    nextIndex++;
                }

                ExecuteBulkGroup(bulkItems, request);
                index = nextIndex;
            }

            if (executeMultipleItems.Count > 0)
            {
                ExecuteExecuteMultipleBatch(executeMultipleItems);
            }
        }

        private static bool TryGetBulkOperation(SetOperationContext context, out OrganizationRequest request, out Entity target)
        {
            request = null;
            target = null;
            if (context.Requests.Count != 1)
            {
                return false;
            }

            request = context.Requests[0];
            switch (request)
            {
                case CreateRequest createRequest:
                    target = createRequest.Target;
                    return true;
                case UpdateRequest updateRequest:
                    target = updateRequest.Target;
                    return true;
                case UpsertRequest upsertRequest:
                    target = upsertRequest.Target;
                    return true;
                default:
                    return false;
            }
        }

        private static bool CanShareBulkRequest(
            OrganizationRequest request,
            Entity target,
            OrganizationRequest nextRequest,
            Entity nextTarget)
        {
            var requestParameters = request.Parameters
                .Where(parameter => !string.Equals(parameter.Key, "Target", StringComparison.OrdinalIgnoreCase))
                .ToList();
            var nextRequestParameters = nextRequest.Parameters
                .Where(parameter => !string.Equals(parameter.Key, "Target", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(parameter => parameter.Key, parameter => parameter.Value, StringComparer.OrdinalIgnoreCase);

            if (request.GetType() != nextRequest.GetType() ||
                !string.Equals(target.LogicalName, nextTarget.LogicalName, StringComparison.OrdinalIgnoreCase) ||
                requestParameters.Count != nextRequestParameters.Count)
            {
                return false;
            }

            foreach (var parameter in requestParameters)
            {
                if (!nextRequestParameters.TryGetValue(parameter.Key, out object value) ||
                    !Equals(parameter.Value, value))
                {
                    return false;
                }
            }

            return true;
        }

        private void ExecuteBulkGroup(List<SetOperationContext> contexts, OrganizationRequest firstRequest)
        {
            var targetEntities = new EntityCollection();
            if (firstRequest is CreateRequest createRequest)
            {
                targetEntities.EntityName = createRequest.Target.LogicalName;
                foreach (var context in contexts)
                {
                    targetEntities.Entities.Add(((CreateRequest)context.Requests[0]).Target);
                }
            }
            else if (firstRequest is UpdateRequest updateRequest)
            {
                targetEntities.EntityName = updateRequest.Target.LogicalName;
                foreach (var context in contexts)
                {
                    targetEntities.Entities.Add(((UpdateRequest)context.Requests[0]).Target);
                }
            }
            else
            {
                var upsertRequest = (UpsertRequest)firstRequest;
                targetEntities.EntityName = upsertRequest.Target.LogicalName;
                foreach (var context in contexts)
                {
                    targetEntities.Entities.Add(((UpsertRequest)context.Requests[0]).Target);
                }
            }

            OrganizationRequest bulkRequest;
            if (firstRequest is CreateRequest)
            {
                bulkRequest = new CreateMultipleRequest { Targets = targetEntities };
            }
            else if (firstRequest is UpdateRequest)
            {
                bulkRequest = new UpdateMultipleRequest { Targets = targetEntities };
            }
            else
            {
                bulkRequest = new UpsertMultipleRequest { Targets = targetEntities };
            }

            foreach (var parameter in firstRequest.Parameters)
            {
                if (!string.Equals(parameter.Key, "Target", StringComparison.OrdinalIgnoreCase))
                {
                    bulkRequest.Parameters[parameter.Key] = parameter.Value;
                }
            }

            Guid oldCallerId = _getCallerId();
            _setCallerId(contexts[0].CallerId);

            try
            {
                OrganizationResponse response;
                try
                {
                    response = _connection.Execute(bulkRequest);
                }
                catch (Exception e)
                {
                    HandleBatchException(contexts, e);
                    return;
                }

                if (response is CreateMultipleResponse ||
                    string.Equals(response.ResponseName, "CreateMultiple", StringComparison.OrdinalIgnoreCase))
                {
                    var createdIds = response.Results.TryGetValue("Ids", out object idsValue)
                        ? ((IEnumerable<Guid>)idsValue).ToArray()
                        : Array.Empty<Guid>();
                    for (int i = 0; i < contexts.Count; i++)
                    {
                        var targetId = i < createdIds.Length
                            ? createdIds[i]
                            : targetEntities.Entities[i].Id;
                        var singleResponse = new CreateResponse();
                        singleResponse.Results["id"] = targetId;
                        InvokeCompletion(contexts[i], singleResponse);
                    }
                }
                else if (response is UpdateMultipleResponse ||
                    string.Equals(response.ResponseName, "UpdateMultiple", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var context in contexts)
                    {
                        InvokeCompletion(context, new UpdateResponse());
                    }
                }
                else if (response is UpsertMultipleResponse ||
                    string.Equals(response.ResponseName, "UpsertMultiple", StringComparison.OrdinalIgnoreCase))
                {
                    var responses = ((IEnumerable<OrganizationResponse>)response.Results["Results"])
                        .Cast<UpsertResponse>()
                        .ToArray();
                    if (responses.Length != contexts.Count)
                    {
                        throw new InvalidOperationException(
                            $"UpsertMultiple returned {responses.Length} responses for {contexts.Count} records.");
                    }

                    for (int i = 0; i < contexts.Count; i++)
                    {
                        InvokeCompletion(contexts[i], responses[i]);
                    }
                }
                else
                {
                    throw new InvalidOperationException(
                        $"Unexpected response type '{response?.GetType().FullName}' for '{bulkRequest.RequestName}'.");
                }
            }
            finally
            {
                _setCallerId(oldCallerId);
            }
        }

        private static void InvokeCompletion(SetOperationContext context, OrganizationResponse response)
        {
            if (context.ResponseCompletions != null &&
                context.ResponseCompletions.Count > 0 &&
                context.ResponseCompletions[0] != null)
            {
                context.ResponseCompletions[0](response);
            }
        }

        private void ExecuteExecuteMultipleBatch(List<SetOperationContext> contexts)
        {
            ExecuteMultipleRequest batchRequest = new ExecuteMultipleRequest()
            {
                Settings = new ExecuteMultipleSettings()
                {
                    ReturnResponses = true,
                    ContinueOnError = true
                },
                Requests = new OrganizationRequestCollection(),
                RequestId = Guid.NewGuid()
            };

            var firstContext = contexts[0];
            if (firstContext.BypassBusinessLogicExecution?.Length > 0)
            {
                batchRequest.Parameters["BypassBusinessLogicExecution"] =
                    string.Join(",", firstContext.BypassBusinessLogicExecution.Select(o => o.ToString()));
            }
            if (firstContext.BypassBusinessLogicExecutionStepIds?.Length > 0)
            {
                batchRequest.Parameters["BypassBusinessLogicExecutionStepIds"] =
                    string.Join(",", firstContext.BypassBusinessLogicExecutionStepIds.Select(id => id.ToString()));
            }

            foreach (var context in contexts)
            {
                batchRequest.Requests.AddRange(context.Requests);
            }

            Guid oldCallerId = _getCallerId();
            _setCallerId(contexts[0].CallerId);
            try
            {
                ExecuteMultipleResponse response;
                try
                {
                    response = (ExecuteMultipleResponse)_connection.Execute(batchRequest);
                }
                catch (Exception e)
                {
                    HandleBatchException(contexts, e);
                    return;
                }

                int requestIndex = 0;
                foreach (var context in contexts)
                {
                    bool hasError = false;
                    bool handledError = false;
                    Exception firstError = null;

                    for (int i = 0; i < context.Requests.Count; i++)
                    {
                        if (requestIndex < response.Responses.Count)
                        {
                            var itemResponse = response.Responses[requestIndex];
                            if (itemResponse.Fault != null)
                            {
                                if (context.ResponseExceptionCompletion != null && !handledError)
                                {
                                    handledError = context.ResponseExceptionCompletion(itemResponse.Fault);
                                }

                                if (!handledError)
                                {
                                    StringBuilder details = new StringBuilder();
                                    QueryHelpers.AppendFaultDetails(itemResponse.Fault, details);
                                    firstError ??= new Exception(details.ToString());
                                    hasError = true;
                                }
                            }
                            else if (context.ResponseCompletions != null &&
                                i < context.ResponseCompletions.Count &&
                                context.ResponseCompletions[i] != null)
                            {
                                context.ResponseCompletions[i](itemResponse.Response);
                            }
                        }

                        requestIndex++;
                    }

                    if (hasError && !handledError)
                    {
                        HandleBatchException(new List<SetOperationContext> { context }, firstError);
                    }
                }
            }
            finally
            {
                _setCallerId(oldCallerId);
            }
        }

        private void HandleBatchException(IEnumerable<SetOperationContext> contexts, Exception exception)
        {
            foreach (var context in contexts)
            {
                if (context.RetriesRemaining > 0)
                {
                    context.ScheduleRetry(exception);
                    _pendingRetries.Add(context);
                }
                else
                {
                    context.ReportError(exception);
                }
            }
        }

}
    }
