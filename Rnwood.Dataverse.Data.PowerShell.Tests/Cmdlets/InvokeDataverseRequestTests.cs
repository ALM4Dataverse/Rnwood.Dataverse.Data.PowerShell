using FluentAssertions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Tests for Invoke-DataverseRequest cmdlet.
    /// Migrated from Pester tests in tests/Invoke-DataverseRequest*.Tests.ps1 (24 tests)
    /// </summary>
    public class InvokeDataverseRequestTests : TestBase
    {
        // ===== Batching and Responses Tests =====

        [Fact]
        public void InvokeDataverseRequest_BatchesMultipleRequests_WithBatchSize5_ReturnsAllResponses()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseRequest", typeof(InvokeDataverseRequestCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Create 10 WhoAmI requests as array for pipeline
            var requests = Enumerable.Range(1, 10)
                .Select(_ => new WhoAmIRequest())
                .ToArray();

            // Act - Pipeline the requests to Invoke-DataverseRequest
            ps.AddScript(@"
                param($connection, $requests)
                $requests | Invoke-DataverseRequest -Connection $connection -BatchSize 5
            ")
            .AddParameter("connection", mockConnection)
            .AddParameter("requests", requests);

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(10);
            // Results are wrapped in PSObject - check BaseObject
            results.Should().AllSatisfy(r => r.BaseObject.Should().BeAssignableTo<WhoAmIResponse>());
        }

        [Fact]
        public void InvokeDataverseRequest_BatchSize1_NoBatching_ReturnsAllResponses()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseRequest", typeof(InvokeDataverseRequestCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Create 8 WhoAmI requests
            var requests = Enumerable.Range(1, 8)
                .Select(_ => new WhoAmIRequest())
                .ToArray();

            // Act - BatchSize 1 means no batching
            ps.AddScript(@"
                param($connection, $requests)
                $requests | Invoke-DataverseRequest -Connection $connection -BatchSize 1
            ")
            .AddParameter("connection", mockConnection)
            .AddParameter("requests", requests);

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(8);
        }

        [Fact]
        public void InvokeDataverseRequest_EmptyPipeline_DoesNotThrow()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseRequest", typeof(InvokeDataverseRequestCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Act - Empty array via pipeline
            ps.AddScript(@"
                param($connection)
                @() | Invoke-DataverseRequest -Connection $connection -BatchSize 5
            ")
            .AddParameter("connection", mockConnection);

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().BeEmpty();
        }

        // ===== Path Validation Tests =====

        [Fact]
        public void InvokeDataverseRequest_PathStartingWithSlashApi_ThrowsValidationError()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseRequest", typeof(InvokeDataverseRequestCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Act
            ps.AddCommand("Invoke-DataverseRequest")
                .AddParameter("Connection", mockConnection)
                .AddParameter("Method", "Get")
                .AddParameter("Path", "/api/data/v9.2/systemusers");

            Exception? caughtException = null;
            try
            {
                ps.Invoke();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert - Parameter validation error should occur
            (ps.HadErrors || caughtException != null).Should().BeTrue("Expected validation error for path starting with /api/");
            
            // Check either stream errors or exception for the expected message
            if (ps.Streams.Error.Count > 0)
            {
                ps.Streams.Error[0].Exception.Message.Should().Contain("should not start with");
            }
            else if (caughtException != null)
            {
                caughtException.Message.Should().Contain("should not start with");
            }
            else
            {
                // If HadErrors is true but no error details available, that's still a validation failure
                ps.HadErrors.Should().BeTrue("Expected error for invalid API path");
            }
        }

        [Fact]
        public void InvokeDataverseRequest_PathStartingWithApi_ThrowsValidationError()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseRequest", typeof(InvokeDataverseRequestCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Act
            ps.AddCommand("Invoke-DataverseRequest")
                .AddParameter("Connection", mockConnection)
                .AddParameter("Method", "Get")
                .AddParameter("Path", "api/data/v9.2/systemusers");

            Exception? caughtException = null;
            try
            {
                ps.Invoke();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert - Parameter validation error should occur
            (ps.HadErrors || caughtException != null).Should().BeTrue("Expected validation error for path starting with api/");
            
            // Check either stream errors or exception for the expected message
            if (ps.Streams.Error.Count > 0)
            {
                ps.Streams.Error[0].Exception.Message.Should().Contain("should not start with");
            }
            else if (caughtException != null)
            {
                caughtException.Message.Should().Contain("should not start with");
            }
            else
            {
                // If HadErrors is true but no error details available, that's still a validation failure
                ps.HadErrors.Should().BeTrue("Expected error for invalid API path");
            }
        }

        [Fact]
        public void InvokeDataverseRequest_PathStartingWithSlashAPICaseInsensitive_ThrowsValidationError()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseRequest", typeof(InvokeDataverseRequestCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Act
            ps.AddCommand("Invoke-DataverseRequest")
                .AddParameter("Connection", mockConnection)
                .AddParameter("Method", "Get")
                .AddParameter("Path", "/API/data/v9.2/systemusers");

            Exception? caughtException = null;
            try
            {
                ps.Invoke();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert - Parameter validation error should occur
            (ps.HadErrors || caughtException != null).Should().BeTrue("Expected validation error for path starting with /API/");
            
            // Check either stream errors or exception for the expected message
            if (ps.Streams.Error.Count > 0)
            {
                ps.Streams.Error[0].Exception.Message.Should().Contain("should not start with");
            }
            else if (caughtException != null)
            {
                caughtException.Message.Should().Contain("should not start with");
            }
            else
            {
                // If HadErrors is true but no error details available, that's still a validation failure
                ps.HadErrors.Should().BeTrue("Expected error for invalid API path");
            }
        }

        [Fact]
        public void InvokeDataverseRequest_PathStartingWithAPICaseInsensitive_ThrowsValidationError()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseRequest", typeof(InvokeDataverseRequestCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Act
            ps.AddCommand("Invoke-DataverseRequest")
                .AddParameter("Connection", mockConnection)
                .AddParameter("Method", "Get")
                .AddParameter("Path", "API/data/v9.2/systemusers");

            Exception? caughtException = null;
            try
            {
                ps.Invoke();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert - Parameter validation error should occur
            (ps.HadErrors || caughtException != null).Should().BeTrue("Expected validation error for path starting with API/");
            
            // Check either stream errors or exception for the expected message
            if (ps.Streams.Error.Count > 0)
            {
                ps.Streams.Error[0].Exception.Message.Should().Contain("should not start with");
            }
            else if (caughtException != null)
            {
                caughtException.Message.Should().Contain("should not start with");
            }
            else
            {
                // If HadErrors is true but no error details available, that's still a validation failure
                ps.HadErrors.Should().BeTrue("Expected error for invalid API path");
            }
        }

        [Fact]
        public void InvokeDataverseRequest_NavigationPathWithForwardSlash_IsAllowed()
        {
            var id = "1d936fda-9076-ef11-a671-6045bd0ab99c";

            Action act = () => InvokeDataverseRequestCmdlet.ValidateRestPath($"sample_entities({id})/Microsoft.Dynamics.CRM.sample_MyCustomApi");

            act.Should().NotThrow();
        }

        [Fact]
        public void InvokeDataverseRequest_NavigationPathWithMultipleForwardSlashes_IsAllowed()
        {
            var id = "1d936fda-9076-ef11-a671-6045bd0ab99c";

            Action act = () => InvokeDataverseRequestCmdlet.ValidateRestPath($"entities({id})/Microsoft.Dynamics.CRM.Action/SubPath");

            act.Should().NotThrow();
        }

        [Fact]
        public void InvokeDataverseRequest_SimpleResourceName_IsAllowed()
        {
            Action act = () => InvokeDataverseRequestCmdlet.ValidateRestPath("WhoAmI");

            act.Should().NotThrow();
        }

        [Fact]
        public void InvokeDataverseRequest_PathWithQueryStringContainingSlashes_IsAllowed()
        {
            Action act = () => InvokeDataverseRequestCmdlet.ValidateRestPath("WhoAmI?filter=value/with/slashes");

            act.Should().NotThrow();
        }

        [Fact]
        public void InvokeDataverseRequest_NavigationPathWithQueryStringContainingSlashes_IsAllowed()
        {
            var id = "1d936fda-9076-ef11-a671-6045bd0ab99c";

            Action act = () => InvokeDataverseRequestCmdlet.ValidateRestPath($"sample_entities({id})/Microsoft.Dynamics.CRM.Action?param=value/with/slash");

            act.Should().NotThrow();
        }

        // ===== PSObject Unwrapping Tests =====

        [Fact]
        public void InvokeDataverseRequest_EntityReferenceInHashtable_DoesNotThrowPSObjectSerializationError()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseRequest", typeof(InvokeDataverseRequestCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            var target = new EntityReference("contact", Guid.Parse("12345678-1234-1234-1234-123456789012"));

            // Act - Pass EntityReference in hashtable (PowerShell wraps it in PSObject)
            ps.AddScript(@"
                param($connection, $target)
                $params = @{ Target = $target }
                Invoke-DataverseRequest -Connection $connection -RequestName 'sample_MyCustomApi' -Parameters $params
            ")
            .AddParameter("connection", mockConnection)
            .AddParameter("target", target);

            ps.Invoke();

            // Assert - Should NOT get PSObject serialization error
            if (ps.HadErrors)
            {
                ps.Streams.Error[0].Exception.Message.Should().NotContain("PSObject");
                ps.Streams.Error[0].Exception.Message.Should().NotContain("cannot be serialized");
            }
        }

        [Fact]
        public void InvokeDataverseRequest_OptionSetValueInHashtable_DoesNotThrowPSObjectSerializationError()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseRequest", typeof(InvokeDataverseRequestCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            var priority = new OptionSetValue(1);

            // Act
            ps.AddScript(@"
                param($connection, $priority)
                $params = @{ Priority = $priority }
                Invoke-DataverseRequest -Connection $connection -RequestName 'sample_MyCustomApi' -Parameters $params
            ")
            .AddParameter("connection", mockConnection)
            .AddParameter("priority", priority);

            ps.Invoke();

            // Assert - Should NOT get PSObject serialization error
            if (ps.HadErrors)
            {
                ps.Streams.Error[0].Exception.Message.Should().NotContain("PSObject");
                ps.Streams.Error[0].Exception.Message.Should().NotContain("cannot be serialized");
            }
        }

        [Fact]
        public void InvokeDataverseRequest_MultipleSDKObjectsInHashtable_DoesNotThrowPSObjectSerializationError()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseRequest", typeof(InvokeDataverseRequestCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            var target = new EntityReference("contact", Guid.Parse("DC66FE5D-B854-4F9D-BA63-4CEA4257A8E9"));
            var priority = new OptionSetValue(1);

            // Act - Exact scenario from GitHub issue
            ps.AddScript(@"
                param($connection, $target, $priority)
                $params = @{ 
                    Target = $target
                    Priority = $priority
                }
                Invoke-DataverseRequest -Connection $connection -RequestName 'myapi_EscalateCase' -Parameters $params
            ")
            .AddParameter("connection", mockConnection)
            .AddParameter("target", target)
            .AddParameter("priority", priority);

            ps.Invoke();

            // Assert - Should NOT get PSObject serialization error
            if (ps.HadErrors)
            {
                ps.Streams.Error[0].Exception.Message.Should().NotContain("PSObject");
                ps.Streams.Error[0].Exception.Message.Should().NotContain("cannot be serialized");
            }
        }

        [Fact]
        public void InvokeDataverseRequest_NullValuesInParameters_DoesNotThrowPSObjectSerializationError()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseRequest", typeof(InvokeDataverseRequestCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Act
            ps.AddScript(@"
                param($connection)
                $params = @{ 
                    Target = $null
                    SomeValue = $null
                }
                Invoke-DataverseRequest -Connection $connection -RequestName 'sample_Api' -Parameters $params
            ")
            .AddParameter("connection", mockConnection);

            ps.Invoke();

            // Assert - Should NOT get PSObject serialization error
            if (ps.HadErrors)
            {
                ps.Streams.Error[0].Exception.Message.Should().NotContain("PSObject");
                ps.Streams.Error[0].Exception.Message.Should().NotContain("cannot be serialized");
            }
        }

        [Fact]
        public void InvokeDataverseRequest_PrimitiveTypesInParameters_DoesNotThrowPSObjectSerializationError()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseRequest", typeof(InvokeDataverseRequestCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Act
            ps.AddScript(@"
                param($connection)
                $params = @{ 
                    StringParam = 'test value'
                    NumberParam = 42
                    GuidParam = [Guid]'12345678-1234-1234-1234-123456789012'
                    BoolParam = $true
                }
                Invoke-DataverseRequest -Connection $connection -RequestName 'sample_Api' -Parameters $params
            ")
            .AddParameter("connection", mockConnection);

            ps.Invoke();

            // Assert - Should NOT get PSObject serialization error
            if (ps.HadErrors)
            {
                ps.Streams.Error[0].Exception.Message.Should().NotContain("PSObject");
                ps.Streams.Error[0].Exception.Message.Should().NotContain("cannot be serialized");
            }
        }

        [Fact]
        public void InvokeDataverseRequest_MoneyValueInParameters_DoesNotThrowPSObjectSerializationError()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseRequest", typeof(InvokeDataverseRequestCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            var money = new Money(100.50m);

            // Act
            ps.AddScript(@"
                param($connection, $money)
                $params = @{ Amount = $money }
                Invoke-DataverseRequest -Connection $connection -RequestName 'sample_Api' -Parameters $params
            ")
            .AddParameter("connection", mockConnection)
            .AddParameter("money", money);

            ps.Invoke();

            // Assert - Should NOT get PSObject serialization error
            if (ps.HadErrors)
            {
                ps.Streams.Error[0].Exception.Message.Should().NotContain("PSObject");
                ps.Streams.Error[0].Exception.Message.Should().NotContain("cannot be serialized");
            }
        }

        [Fact]
        public void InvokeDataverseRequest_PSCustomObjectInParameters_DoesNotThrowPSObjectSerializationError()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseRequest", typeof(InvokeDataverseRequestCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Act - PSCustomObject should NOT be unwrapped
            ps.AddScript(@"
                param($connection)
                $customObj = [PSCustomObject]@{
                    Name = 'Test'
                    Value = 123
                }
                $params = @{ CustomData = $customObj }
                Invoke-DataverseRequest -Connection $connection -RequestName 'sample_Api' -Parameters $params
            ")
            .AddParameter("connection", mockConnection);

            ps.Invoke();

            // Assert - Should NOT get PSObject serialization error
            if (ps.HadErrors)
            {
                ps.Streams.Error[0].Exception.Message.Should().NotContain("PSObject");
                ps.Streams.Error[0].Exception.Message.Should().NotContain("cannot be serialized");
            }
        }

        // ===== Response Conversion Tests =====

        [Fact]
        public void InvokeDataverseRequest_RequestParameterSet_ReturnsRawOrganizationResponse()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseRequest", typeof(InvokeDataverseRequestCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            var request = new WhoAmIRequest();

            // Act
            ps.AddCommand("Invoke-DataverseRequest")
                .AddParameter("Connection", mockConnection)
                .AddParameter("Request", request);

            var results = ps.Invoke();

            // Assert - Should return raw response, not converted
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].BaseObject.Should().BeOfType<WhoAmIResponse>();

            var response = results[0].BaseObject as WhoAmIResponse;
            response!.Results["UserId"].Should().NotBeNull();
        }

        [Fact]
        public void InvokeDataverseRequest_BatchingWithRequestParameterSet_DoesNotConvertResponses()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseRequest", typeof(InvokeDataverseRequestCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            var requests = Enumerable.Range(1, 3)
                .Select(_ => new WhoAmIRequest())
                .ToArray();

            // Act
            ps.AddScript(@"
                param($connection, $requests)
                $requests | Invoke-DataverseRequest -Connection $connection -BatchSize 10
            ")
            .AddParameter("connection", mockConnection)
            .AddParameter("requests", requests);

            var results = ps.Invoke();

            // Assert - Should NOT convert responses
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(3);
            // Results are wrapped in PSObject - check BaseObject
            results.Should().AllSatisfy(r => r.BaseObject.Should().BeAssignableTo<WhoAmIResponse>());
        }

    }
}
