using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using AwesomeAssertions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Parallel Execution tests for Set-DataverseRecord cmdlet.
    /// FakeXrmEasy is thread-safe so parallel tests work without real Dataverse.
    /// </summary>
    public class SetDataverseRecord_ParallelExecutionTests : TestBase
    {
        private PS CreatePowerShellWithCmdlets()
        {
            var initialSessionState = InitialSessionState.CreateDefault2();
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
        public void SetDataverseRecord_Parallel_ProcessesCreatesWithMaxDegreeOfParallelism()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");

            // Act - Create 10 contacts using parallel processing with MaxDOP=2
            ps.AddScript(@"
                $records = 1..10 | ForEach-Object {
                    @{ firstname = ""Test$_""; lastname = ""User$_"" }
                }
                $records | Set-DataverseRecord -Connection $args[0] -TableName contact -MaxDegreeOfParallelism 2
            ");
            ps.AddArgument(mockConnection);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            var results = Service!.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet("firstname") });
            results.Entities.Should().HaveCount(10);
        }

        [Fact]
        public void SetDataverseRecord_Parallel_WorksWithMaxDegreeOfParallelismOne()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");

            // Act - With MaxDOP=1, should process sequentially (same as default)
            ps.AddScript(@"
                $records = 1..5 | ForEach-Object {
                    @{ firstname = ""Sequential$_""; lastname = ""User$_"" }
                }
                $records | Set-DataverseRecord -Connection $args[0] -TableName contact -MaxDegreeOfParallelism 1
            ");
            ps.AddArgument(mockConnection);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            var results = Service!.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet("firstname") });
            results.Entities.Should().HaveCount(5);
        }

        [Fact]
        public void SetDataverseRecord_Parallel_WarnsWhenAffinityCookieEnabled()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");

            // Act
            ps.AddScript(@"
                param($conn)
                @() | Set-DataverseRecord -Connection $conn -TableName contact -MaxDegreeOfParallelism 2
            ");
            ps.AddParameter("conn", mockConnection);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            ps.Streams.Warning.Should().Contain(w =>
                w.Message.Contains("Using parallelization with affinity cookie enabled may reduce performance"));
        }

        [Fact]
        public void SetDataverseRecord_Parallel_DoesNotWarnWhenAffinityCookieDisabled()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            mockConnection.EnableAffinityCookie = false;

            // Act
            ps.AddScript(@"
                param($conn)
                @() | Set-DataverseRecord -Connection $conn -TableName contact -MaxDegreeOfParallelism 2
            ");
            ps.AddParameter("conn", mockConnection);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            ps.Streams.Warning.Should().NotContain(w =>
                w.Message.Contains("Using parallelization with affinity cookie enabled may reduce performance"));
        }

        [Fact]
        public void SetDataverseRecord_Parallel_CombinesParallelProcessingWithBatching()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");

            // Act - Create 20 records with MaxDOP=3 and BatchSize=5
            // Should create 4 batches distributed across 3 parallel workers
            ps.AddScript(@"
                $records = 1..20 | ForEach-Object {
                    @{ firstname = ""Batch$_""; lastname = ""User$_"" }
                }
                $records | Set-DataverseRecord -Connection $args[0] -TableName contact -MaxDegreeOfParallelism 3 -BatchSize 5
            ");
            ps.AddArgument(mockConnection);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            var results = Service!.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet("firstname") });
            results.Entities.Should().HaveCount(20);
        }

        [Fact]
        public void SetDataverseRecord_Parallel_ProcessesUpdatesInParallel()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create initial records
            var ids = new List<Guid>();
            for (int i = 0; i < 10; i++)
            {
                var id = Guid.NewGuid();
                Service!.Create(new Entity("contact") { Id = id, ["firstname"] = $"Initial{i}", ["lastname"] = $"User{i}" });
                ids.Add(id);
            }

            // Act - Update all records in parallel
            ps.AddScript(@"
                param($ids, $conn)
                $records = $ids | ForEach-Object {
                    @{ Id = $_; firstname = 'Updated' }
                }
                $records | Set-DataverseRecord -Connection $conn -TableName contact -MaxDegreeOfParallelism 3
            ");
            ps.AddParameter("ids", ids.ToArray());
            ps.AddParameter("conn", mockConnection);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            var results = Service!.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet("firstname") });
            results.Entities.Should().OnlyContain(e => e.GetAttributeValue<string>("firstname") == "Updated");
        }

        [Fact]
        public void SetDataverseRecord_Parallel_HandlesErrorsInParallelProcessing()
        {
            // Arrange - Use TransientFailureSimulator to force errors in parallel mode
            var simulator = new TransientFailureSimulator(100, TransientFailureSimulator.ContainsCreateOrUpdate);
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection(simulator.Intercept, "contact");

            // Act - Try to create records with forced failures, use ErrorAction Continue so errors are captured
            ps.AddScript(@"
                param($conn)
                $records = 1..5 | ForEach-Object {
                    @{ firstname = ""Error$_""; lastname = ""User$_"" }
                }
                $records | Set-DataverseRecord -Connection $conn -TableName contact -MaxDegreeOfParallelism 2 -ErrorAction Continue
            ");
            ps.AddParameter("conn", mockConnection);
            ps.Invoke();

            // Assert - Check error streams directly (HadErrors is unreliable with async error queues)
            ps.Streams.Error.Should().NotBeEmpty("parallel workers should report errors to error stream");
            ps.Streams.Error.Should().Contain(e => e.Exception.Message.Contains("Simulated transient failure"));
            
            // No records should be created since all operations fail
            var results = Service!.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet("firstname") });
            results.Entities.Should().BeEmpty();
        }

        [Fact]
        public void SetDataverseRecord_Parallel_ProcessesUpsertsInParallel()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create one existing record
            var existingId = Guid.NewGuid();
            Service!.Create(new Entity("contact") { Id = existingId, ["firstname"] = "Existing", ["lastname"] = "User", ["emailaddress1"] = "existing@test.com" });

            // Act - Upsert mix of new and existing records
            ps.AddScript(@"
                param($existingId, $conn)
                $records = @(
                    @{ Id = $existingId; firstname = 'UpdatedExisting' },
                    @{ firstname = 'New1'; lastname = 'User1' },
                    @{ firstname = 'New2'; lastname = 'User2' }
                )
                $records | Set-DataverseRecord -Connection $conn -TableName contact -MaxDegreeOfParallelism 2 -Upsert
            ");
            ps.AddParameter("existingId", existingId);
            ps.AddParameter("conn", mockConnection);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            var results = Service!.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet("firstname") });
            results.Entities.Should().HaveCount(3);
            results.Entities.Should().Contain(e => e.GetAttributeValue<string>("firstname") == "UpdatedExisting");
            results.Entities.Should().Contain(e => e.GetAttributeValue<string>("firstname") == "New1");
            results.Entities.Should().Contain(e => e.GetAttributeValue<string>("firstname") == "New2");
        }

        [Fact]
        public void SetDataverseRecord_Parallel_RespectsPassThruParameter()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");

            // Act - Create records with PassThru to get output
            ps.AddScript(@"
                $records = 1..5 | ForEach-Object {
                    @{ firstname = ""PassThru$_""; lastname = ""User$_"" }
                }
                $results = $records | Set-DataverseRecord -Connection $args[0] -TableName contact -MaxDegreeOfParallelism 2 -PassThru
                $results
            ");
            ps.AddArgument(mockConnection);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(5);
            // PassThru returns PSObjects with Id property
            foreach (var result in results)
            {
                var pso = result.BaseObject as PSObject ?? result;
                pso.Properties["Id"].Should().NotBeNull();
            }
        }

        [Fact]
        public void SetDataverseRecord_Parallel_WorksWithMatchOnInParallelMode()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create initial records with unique emails
            for (int i = 0; i < 5; i++)
            {
                Service!.Create(new Entity("contact") 
                { 
                    Id = Guid.NewGuid(), 
                    ["firstname"] = $"Initial{i}", 
                    ["lastname"] = $"User{i}",
                    ["emailaddress1"] = $"user{i}@test.com" 
                });
            }

            // Act - Update records using MatchOn with parallel processing
            ps.AddScript(@"
                param($conn)
                $records = 0..4 | ForEach-Object {
                    @{ emailaddress1 = ""user$($_)@test.com""; firstname = ""Updated$_"" }
                }
                $records | Set-DataverseRecord -Connection $conn -TableName contact -MatchOn emailaddress1 -MaxDegreeOfParallelism 2
            ");
            ps.AddParameter("conn", mockConnection);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            var results = Service!.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet("firstname") });
            results.Entities.Should().OnlyContain(e => e.GetAttributeValue<string>("firstname").StartsWith("Updated"));
        }

        // Note: Retry logic in parallel mode with batching is tested in SetDataverseRecord_ParallelRetriesTests.cs
        // That test suite uses TransientFailureSimulator to validate retry behavior with:
        // - SetDataverseRecord_ParallelRetries_RetriesFailedOperationsWithBatching
        // - SetDataverseRecord_ParallelRetries_RetriesFailedOperationsWithoutBatching
        // - SetDataverseRecord_ParallelRetries_ExhaustsRetriesAndReportsError
    }
}
