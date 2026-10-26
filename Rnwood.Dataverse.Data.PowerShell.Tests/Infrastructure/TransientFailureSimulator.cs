using System;
using System.Linq;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure
{
    /// <summary>
    /// Small helper to simulate transient failures for specific request types without
    /// impacting metadata/bootstrap requests. Plug into CreateMockConnection via the
    /// requestInterceptor parameter.
    /// </summary>
    internal sealed class TransientFailureSimulator
    {
        private readonly Func<OrganizationRequest, bool> _shouldFail;
        private int _remainingFailures;
        private readonly int _errorCode;

        public TransientFailureSimulator(int failureCount, Func<OrganizationRequest, bool> shouldFail, int errorCode = -2147012746)
        {
            _remainingFailures = failureCount;
            _shouldFail = shouldFail ?? (_ => false);
            _errorCode = errorCode;
        }

        public OrganizationResponse? Intercept(OrganizationRequest request)
        {
            if (_remainingFailures <= 0)
            {
                return null;
            }

            if (!_shouldFail(request))
            {
                return null;
            }

            _remainingFailures--;

            var fault = new OrganizationServiceFault
            {
                ErrorCode = _errorCode,
                Message = "Simulated transient failure for retry testing"
            };

            throw new FaultException<OrganizationServiceFault>(
                fault,
                new FaultReason(fault.Message));
        }

        public static bool ContainsDelete(OrganizationRequest request)
        {
            if (request is DeleteRequest)
            {
                return true;
            }

            if (request is ExecuteMultipleRequest em)
            {
                return em.Requests?.OfType<DeleteRequest>().Any() == true;
            }

            return false;
        }

        public static bool ContainsCreateOrUpdate(OrganizationRequest request)
        {
            if (request is CreateRequest ||
                request is UpdateRequest ||
                request is UpsertRequest ||
                request is CreateMultipleRequest ||
                request is UpdateMultipleRequest ||
                request is UpsertMultipleRequest)
            {
                return true;
            }

            if (request is ExecuteMultipleRequest em)
            {
                return em.Requests?.Any(r => r is CreateRequest || r is UpdateRequest) == true;
            }

            return false;
        }
    }
}