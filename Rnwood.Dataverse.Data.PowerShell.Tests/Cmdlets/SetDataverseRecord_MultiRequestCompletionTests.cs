using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using AwesomeAssertions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Multi-Request Completion tests for Set-DataverseRecord cmdlet.
    /// Migrated from Pester tests in tests/Set-DataverseRecord-MultiRequestCompletion.Tests.ps1
    /// </summary>
    public class SetDataverseRecord_MultiRequestCompletionTests : TestBase
    {
        private PS CreatePowerShellWithCmdlets()
        {
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseRecord", typeof(SetDataverseRecordCmdlet), null));
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseRecord", typeof(GetDataverseRecordCmdlet), null));
            
            var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            
            var ps = PS.Create();
            ps.Runspace = runspace;
            return ps;
        }

        [Fact]
        public void SetDataverseRecord_MultiRequest_PassThruWorksForSingleRequest()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var record = new Hashtable {
                ["firstname"] = "Test",
                ["lastname"] = "User"
            };
            
            // Act
            var inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(record));
            inputCollection.Complete();
            
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true)
              .AddParameter("BatchSize", 10);
            var results = ps.Invoke(inputCollection);

            // Assert - Verify PassThru worked (Id was set by completion handler)
            ps.HadErrors.Should().BeFalse(string.Join("; ", ps.Streams.Error.Select(e => e.Exception?.Message ?? e.ToString())));
            results.Should().HaveCount(1);
            
            var result = results[0];
            var id = (Guid)result.Properties["Id"].Value;
            id.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public void SetDataverseRecord_MultiRequest_BatchedOperations()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var records = new[] {
                new Hashtable { ["firstname"] = "Contact1", ["lastname"] = "Test", ["emailaddress1"] = "contact1@test.com" },
                new Hashtable { ["firstname"] = "Contact2", ["lastname"] = "Test", ["emailaddress1"] = "contact2@test.com" },
                new Hashtable { ["firstname"] = "Contact3", ["lastname"] = "Test", ["emailaddress1"] = "contact3@test.com" }
            };
            
            // Act
            var inputCollection = new PSDataCollection<PSObject>();
            foreach (var record in records)
                inputCollection.Add(PSObject.AsPSObject(record));
            inputCollection.Complete();
            
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true)
              .AddParameter("BatchSize", 10);
            var results = ps.Invoke(inputCollection);

            // Assert - Verify all records were created and PassThru worked for all
            ps.HadErrors.Should().BeFalse(string.Join("; ", ps.Streams.Error.Select(e => e.Exception?.Message ?? e.ToString())));
            results.Should().HaveCount(3);
            
            var expectedNames = new[] { "Contact1", "Contact2", "Contact3" };
            foreach (var result in results)
            {
                var id = (Guid)result.Properties["Id"].Value;
                id.Should().NotBe(Guid.Empty);
                result.Properties["firstname"].Value.ToString().Should().BeOneOf(expectedNames);
            }
            
            // Verify records were created in Dataverse
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", (Guid)results[0].Properties["Id"].Value)
              .AddParameter("Columns", new[] { "firstname" });
            var retrieved1 = ps.Invoke();
            retrieved1[0].Properties["firstname"].Value.Should().Be("Contact1");
        }

        [Fact]
        public void SetDataverseRecord_MultiRequest_NonBatchMode()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var record = new Hashtable {
                ["firstname"] = "Test",
                ["lastname"] = "User"
            };
            
            // Act - Use BatchSize 1 to test non-batch execution path
            var inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(record));
            inputCollection.Complete();
            
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true)
              .AddParameter("BatchSize", 1);
            var results = ps.Invoke(inputCollection);

            // Assert - Verify PassThru worked (Id was set by completion handler)
            ps.HadErrors.Should().BeFalse(string.Join("; ", ps.Streams.Error.Select(e => e.Exception?.Message ?? e.ToString())));
            results.Should().HaveCount(1);
            
            var result = results[0];
            var id = (Guid)result.Properties["Id"].Value;
            id.Should().NotBe(Guid.Empty);
            
            // Verify record was created
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", id)
              .AddParameter("Columns", new[] { "firstname" });
            var retrieved = ps.Invoke();
            
            retrieved[0].Properties["firstname"].Value.Should().Be("Test");
        }

        [Fact]
        public void SetDataverseRecord_BatchesWithCreateMultipleByDefault()
        {
            using var ps = CreatePowerShellWithCmdlets();
            var requests = new List<OrganizationRequest>();
            var mockConnection = CreateMockConnection(request =>
            {
                requests.Add(request);
                return null;
            }, "contact");

            var records = Enumerable.Range(1, 3)
                .Select(i => PSObject.AsPSObject(new Hashtable
                {
                    ["firstname"] = $"Bulk{i}",
                    ["lastname"] = "Create"
                }))
                .ToArray();

            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("BatchSize", 10);
            ps.Invoke(records);

            ps.HadErrors.Should().BeFalse(string.Join("; ", ps.Streams.Error.Select(e => e.Exception?.Message ?? e.ToString())));
            var createMultiple = requests.OfType<CreateMultipleRequest>()
                .Should().ContainSingle($"Captured requests: {string.Join(", ", requests.Select(r => $"{r.RequestName}:{r.GetType().FullName}"))}")
                .Which;
            createMultiple.Targets.Entities.Should().HaveCount(3);
            requests.OfType<ExecuteMultipleRequest>().Should().BeEmpty();
        }

        [Fact]
        public void SetDataverseRecord_BatchesUpdatesWithUpdateMultipleByDefault()
        {
            using var ps = CreatePowerShellWithCmdlets();
            var requests = new List<OrganizationRequest>();
            var mockConnection = CreateMockConnection(request =>
            {
                requests.Add(request);
                return null;
            }, "contact");

            var recordIds = Enumerable.Range(1, 2)
                .Select(i => Service!.Create(new Entity("contact")
                {
                    ["firstname"] = $"Before{i}",
                    ["lastname"] = "Update"
                }))
                .ToArray();
            var updates = recordIds.Select((id, i) => PSObject.AsPSObject(new Hashtable
            {
                ["contactid"] = id,
                ["firstname"] = $"After{i}"
            })).ToArray();

            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("BatchSize", 10);
            ps.Invoke(updates);

            ps.HadErrors.Should().BeFalse(string.Join("; ", ps.Streams.Error.Select(e => e.Exception?.Message ?? e.ToString())));
            var updateMultiple = requests.OfType<UpdateMultipleRequest>()
                .Should().ContainSingle($"Captured requests: {string.Join(", ", requests.Select(r => $"{r.RequestName}:{r.GetType().FullName}"))}")
                .Which;
            updateMultiple.Targets.Entities.Should().HaveCount(2);
            requests.OfType<ExecuteMultipleRequest>().Should().BeEmpty();
        }

        [Fact]
        public void SetDataverseRecord_UseExecuteMultiplePreservesLegacyBatching()
        {
            using var ps = CreatePowerShellWithCmdlets();
            var requests = new List<OrganizationRequest>();
            var mockConnection = CreateMockConnection(request =>
            {
                requests.Add(request);
                return null;
            }, "contact");

            var records = Enumerable.Range(1, 2)
                .Select(i => PSObject.AsPSObject(new Hashtable
                {
                    ["firstname"] = $"Legacy{i}",
                    ["lastname"] = "Create"
                }))
                .ToArray();

            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("BatchSize", 10)
              .AddParameter("UseExecuteMultiple", true);
            ps.Invoke(records);

            ps.HadErrors.Should().BeFalse(string.Join("; ", ps.Streams.Error.Select(e => e.Exception?.Message ?? e.ToString())));
            requests.OfType<CreateMultipleRequest>().Should().BeEmpty();
            requests.OfType<ExecuteMultipleRequest>().Should().ContainSingle()
                .Which.Requests.Should().HaveCount(2);
        }

        [Fact]
        public void SetDataverseRecord_FallsBackToExecuteMultipleWhenBulkMessageIsUnavailable()
        {
            using var ps = CreatePowerShellWithCmdlets();
            var requests = new List<OrganizationRequest>();
            string queriedMessageName = null;
            var mockConnection = CreateMockConnection(request =>
            {
                if (request is RetrieveMultipleRequest retrieveRequest &&
                    retrieveRequest.Query is QueryExpression query &&
                    query.EntityName == "sdkmessagefilter")
                {
                    queriedMessageName = query.LinkEntities.Single()
                        .LinkCriteria.Conditions.Single(condition => condition.AttributeName == "name")
                        .Values.Single().ToString();
                    var response = new RetrieveMultipleResponse();
                    response.Results["EntityCollection"] = new EntityCollection();
                    return response;
                }

                requests.Add(request);
                return null;
            }, "contact");

            var records = Enumerable.Range(1, 2)
                .Select(i => PSObject.AsPSObject(new Hashtable
                {
                    ["firstname"] = $"Fallback{i}",
                    ["lastname"] = "Create"
                }))
                .ToArray();

            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("BatchSize", 10);
            ps.Invoke(records);

            ps.HadErrors.Should().BeFalse(string.Join("; ", ps.Streams.Error.Select(e => e.ToString())));
            queriedMessageName.Should().Be("CreateMultiple");
            requests.OfType<CreateMultipleRequest>().Should().BeEmpty();
            requests.OfType<ExecuteMultipleRequest>().Should().ContainSingle()
                .Which.Requests.Should().HaveCount(2);
        }

        [Fact]
        public void SetDataverseRecord_EnableDuplicateDetectionUsesExecuteMultiple()
        {
            using var ps = CreatePowerShellWithCmdlets();
            var requests = new List<OrganizationRequest>();
            var mockConnection = CreateMockConnection(request =>
            {
                requests.Add(request);
                return null;
            }, "contact");

            var records = Enumerable.Range(1, 2)
                .Select(i => PSObject.AsPSObject(new Hashtable
                {
                    ["firstname"] = $"DuplicateDetection{i}",
                    ["lastname"] = "Create"
                }))
                .ToArray();

            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("BatchSize", 10)
              .AddParameter("EnableDuplicateDetection", true);
            ps.Invoke(records);

            ps.HadErrors.Should().BeFalse(string.Join("; ", ps.Streams.Error.Select(e => e.ToString())));
            requests.OfType<CreateMultipleRequest>().Should().BeEmpty();
            requests.OfType<ExecuteMultipleRequest>().Should().ContainSingle()
                .Which.Requests.Should().HaveCount(2);
        }

        [Fact]
        public void SetBatchProcessor_CompletionFailureReportsErrorAndContinuesBatch()
        {
            var errors = new List<ErrorRecord>();
            var mockConnection = CreateMockConnection("contact");
            var metadataFactory = new EntityMetadataFactory(mockConnection);
            var entityConverter = new DataverseEntityConverter(mockConnection, metadataFactory);
            var parameters = new SetDataverseRecordCmdlet();

            SetOperationContext CreateContext(string firstname, Action<OrganizationResponse> completion)
            {
                var target = new Entity("contact") { ["firstname"] = firstname };
                var context = new SetOperationContext(
                    PSObject.AsPSObject(new Hashtable { ["firstname"] = firstname }),
                    "contact",
                    null,
                    parameters,
                    metadataFactory,
                    entityConverter,
                    mockConnection,
                    null,
                    _ => { },
                    errors.Add,
                    _ => { },
                    _ => true)
                {
                    Target = target
                };
                context.Requests.Add(new CreateRequest { Target = target });
                context.ResponseCompletion = completion;
                return context;
            }

            var processor = new SetBatchProcessor(
                10,
                false,
                mockConnection,
                _ => { },
                _ => true,
                () => false,
                default,
                _ => { },
                () => Guid.Empty);
            processor.QueueOperation(CreateContext("CompletionFailure", _ => throw new InvalidOperationException("completion failed")));
            processor.QueueOperation(CreateContext("CompletionSuccess", _ => { }));
            processor.Flush();

            errors.Should().ContainSingle().Which.Exception.Message.Should().Be("completion failed");
            Service!.RetrieveMultiple(new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("firstname")
            }).Entities.Select(entity => entity.GetAttributeValue<string>("firstname"))
                .Should().BeEquivalentTo("CompletionFailure", "CompletionSuccess");
        }
    }
}
