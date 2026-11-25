using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using AwesomeAssertions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Tests for Remove-DataverseRecord cmdlet.
    /// Migrated from Pester tests in tests/Remove-DataverseRecord*.Tests.ps1
    /// Total: ~32 tests covering basic deletion, IfExists, WhatIf/Confirm, retries, and parallel processing
    /// </summary>
    public class RemoveDataverseRecordTests : TestBase
    {
        private PS CreatePowerShellWithCmdlets()
        {
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Remove-DataverseRecord", typeof(RemoveDataverseRecordCmdlet), null));
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseRecord", typeof(GetDataverseRecordCmdlet), null));
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseRecord", typeof(SetDataverseRecordCmdlet), null));
            
            var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            
            var ps = PS.Create();
            ps.Runspace = runspace;
            return ps;
        }

        // ===== Basic Removal Tests (from Remove-DataverseRecord.Tests.ps1) =====

        [Fact]
        public void RemoveDataverseRecord_DeletesSingleRecord()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var contactId = Guid.NewGuid();
            Service!.Create(new Entity("contact") { Id = contactId, ["firstname"] = "Test", ["lastname"] = "User" });
            
            // Act
            ps.AddCommand("Remove-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", contactId);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
           
            // Verify record was deleted (use RetrieveMultiple since Fake XrmEasy Retrieve throws exception for non-existent records)
            var query = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("contactid"),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition("contactid", ConditionOperator.Equal, contactId);
            var result = Service.RetrieveMultiple(query);
            result.Entities.Should().BeEmpty("record should have been deleted");
        }

        [Fact]
        public void RemoveDataverseRecord_DeletesMultipleRecordsInBatch()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            Service!.Create(new Entity("contact") { Id = id1, ["firstname"] = "Test1", ["lastname"] = "User1" });
            Service.Create(new Entity("contact") { Id = id2, ["firstname"] = "Test2", ["lastname"] = "User2" });

            // Act - delete via pipeline using PSCustomObjects (hashtables don't bind by property name)
            ps.AddScript(@"
                $records = @(
                    [PSCustomObject]@{ Id = $args[0]; TableName = 'contact' },
                    [PSCustomObject]@{ Id = $args[1]; TableName = 'contact' }
                )
                $records | Remove-DataverseRecord -Connection $args[2]
            ");
            ps.AddArgument(id1);
            ps.AddArgument(id2);
            ps.AddArgument(mockConnection);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            var remaining = Service.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet("contactid") });
            remaining.Entities.Should().BeEmpty();
        }

        // ===== MatchOn Support Tests =====

        [Fact]
        public void RemoveDataverseRecord_MatchOn_DeletesRecordUsingSingleColumn()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            Service!.Create(new Entity("contact") { Id = id1, ["firstname"] = "John", ["lastname"] = "Doe", ["emailaddress1"] = "john@test.com" });
            Service.Create(new Entity("contact") { Id = id2, ["firstname"] = "Jane", ["lastname"] = "Smith", ["emailaddress1"] = "jane@test.com" });

            // Act
            ps.AddScript(@"
                @{ emailaddress1 = 'john@test.com' } | 
                    Remove-DataverseRecord -Connection $args[0] -TableName contact -MatchOn emailaddress1
            ");
            ps.AddArgument(mockConnection);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            var remaining = Service.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet("emailaddress1") });
            remaining.Entities.Should().HaveCount(1);
            remaining.Entities[0].GetAttributeValue<string>("emailaddress1").Should().Be("jane@test.com");
        }

        [Fact]
        public void RemoveDataverseRecord_MatchOn_DeletesRecordUsingMultipleColumns()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            Service!.Create(new Entity("contact") { Id = id1, ["firstname"] = "Alice", ["lastname"] = "Brown", ["emailaddress1"] = "alice.brown@test.com" });
            Service.Create(new Entity("contact") { Id = id2, ["firstname"] = "Bob", ["lastname"] = "Green", ["emailaddress1"] = "bob.green@test.com" });

            // Act
            ps.AddScript(@"
                @{ firstname = 'Alice'; lastname = 'Brown' } | 
                    Remove-DataverseRecord -Connection $args[0] -TableName contact -MatchOn @('firstname', 'lastname')
            ");
            ps.AddArgument(mockConnection);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            var remaining = Service.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet("firstname") });
            remaining.Entities.Should().HaveCount(1);
            remaining.Entities[0].GetAttributeValue<string>("firstname").Should().Be("Bob");
        }

        [Fact]
        public void RemoveDataverseRecord_MatchOn_ErrorsWhenMultipleMatchesWithoutAllowFlag()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            Service!.Create(new Entity("contact") { Id = id1, ["firstname"] = "John1", ["lastname"] = "Doe", ["emailaddress1"] = "test@test.com" });
            Service.Create(new Entity("contact") { Id = id2, ["firstname"] = "John2", ["lastname"] = "Doe", ["emailaddress1"] = "test@test.com" });

            // Act - don't use ErrorAction Stop as it throws ActionPreferenceStopException
            ps.AddScript(@"
                @{ emailaddress1 = 'test@test.com' } | 
                    Remove-DataverseRecord -Connection $args[0] -TableName contact -MatchOn emailaddress1
            ");
            ps.AddArgument(mockConnection);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeTrue();
            ps.Streams.Error[0].Exception.Message.Should().Contain("AllowMultipleMatches");
            
            // Verify no records were deleted
            var remaining = Service.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet("contactid") });
            remaining.Entities.Should().HaveCount(2);
        }

        [Fact]
        public void RemoveDataverseRecord_MatchOn_DeletesMultipleRecordsWithAllowFlag()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var id3 = Guid.NewGuid();
            Service!.Create(new Entity("contact") { Id = id1, ["firstname"] = "John", ["lastname"] = "TestUser", ["emailaddress1"] = "john@test.com" });
            Service.Create(new Entity("contact") { Id = id2, ["firstname"] = "Jane", ["lastname"] = "TestUser", ["emailaddress1"] = "jane@test.com" });
            Service.Create(new Entity("contact") { Id = id3, ["firstname"] = "Bob", ["lastname"] = "Different", ["emailaddress1"] = "bob@test.com" });

            // Act
            ps.AddScript(@"
                @{ lastname = 'TestUser' } | 
                    Remove-DataverseRecord -Connection $args[0] -TableName contact -MatchOn lastname -AllowMultipleMatches
            ");
            ps.AddArgument(mockConnection);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            var remaining = Service.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet("firstname") });
            remaining.Entities.Should().HaveCount(1);
            remaining.Entities[0].GetAttributeValue<string>("firstname").Should().Be("Bob");
        }

        [Fact]
        public void RemoveDataverseRecord_MatchOn_NoErrorWhenNoMatchesWithIfExists()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");

            // Act
            ps.AddScript(@"
                @{ emailaddress1 = 'nonexistent@test.com' } | 
                    Remove-DataverseRecord -Connection $args[0] -TableName contact -MatchOn emailaddress1 -IfExists
            ");
            ps.AddArgument(mockConnection);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void RemoveDataverseRecord_MatchOn_ErrorsWhenNoMatchesWithoutIfExists()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");

            // Act - don't use ErrorAction Stop as it throws ActionPreferenceStopException
            ps.AddScript(@"
                @{ emailaddress1 = 'nonexistent@test.com' } | 
                    Remove-DataverseRecord -Connection $args[0] -TableName contact -MatchOn emailaddress1
            ");
            ps.AddArgument(mockConnection);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeTrue();
            ps.Streams.Error[0].Exception.Message.Should().Contain("No records found");
        }

        [Fact]
        public void RemoveDataverseRecord_MatchOn_ErrorsWhenNeitherIdNorMatchOnSpecified()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");

            // Act - don't use ErrorAction Stop as it throws ActionPreferenceStopException
            ps.AddScript(@"
                @{ firstname = 'Test' } | 
                    Remove-DataverseRecord -Connection $args[0] -TableName contact
            ");
            ps.AddArgument(mockConnection);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeTrue();
            ps.Streams.Error[0].Exception.Message.Should().Contain("Either Id or MatchOn must be specified");
        }

        // ===== IfExists Flag Tests (from Remove-DataverseRecord-IfExists.Tests.ps1) =====

        [Fact]
        public void RemoveDataverseRecord_IfExists_DeletesExistingRecordWithoutError()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var contactId = Guid.NewGuid();
            Service!.Create(new Entity("contact") { Id = contactId, ["firstname"] = "ToDelete", ["lastname"] = "Exists" });

            // Act
            ps.AddCommand("Remove-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", contactId)
              .AddParameter("IfExists", true);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            
            // Verify record was deleted (use RetrieveMultiple since FakeXrmEasy Retrieve throws exception for non-existent records)
            var query = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("contactid"),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition("contactid", ConditionOperator.Equal, contactId);
            var result = Service.RetrieveMultiple(query);
            result.Entities.Should().BeEmpty("record should have been deleted");
        }

        [Fact]
        public void RemoveDataverseRecord_IfExists_NoErrorWhenRecordDoesNotExist()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var keeperId = Guid.NewGuid();
            Service!.Create(new Entity("contact") { Id = keeperId, ["firstname"] = "Keeper", ["lastname"] = "Record" });
            
            var nonExistentId = Guid.NewGuid();

            // Act
            ps.AddCommand("Remove-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", nonExistentId)
              .AddParameter("IfExists", true);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            var remaining = Service.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet("firstname") });
            remaining.Entities.Should().HaveCount(1);
            remaining.Entities[0].GetAttributeValue<string>("firstname").Should().Be("Keeper");
        }

        [Fact]
        public void RemoveDataverseRecord_WithoutIfExists_ErrorsWhenRecordDoesNotExist()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var existingId = Guid.NewGuid();
            Service!.Create(new Entity("contact") { Id = existingId, ["firstname"] = "Existing", ["lastname"] = "Record" });
            
            var nonExistentId = Guid.NewGuid();

            // Act - don't use ErrorAction Stop as it throws ActionPreferenceStopException
            // instead of just setting HadErrors
            ps.AddCommand("Remove-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", nonExistentId);
            ps.Invoke();

            // Assert - should have errors because record doesn't exist
            ps.HadErrors.Should().BeTrue("deleting non-existent record without IfExists should error");
            var remaining = Service.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet("firstname") });
            remaining.Entities.Should().HaveCount(1);
            remaining.Entities[0].GetAttributeValue<string>("firstname").Should().Be("Existing");
        }

        [Fact]
        public void RemoveDataverseRecord_IfExists_WorksWithMultipleIdsInBatch()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var existingId = Guid.NewGuid();
            var keepId = Guid.NewGuid();
            Service!.Create(new Entity("contact") { Id = existingId, ["firstname"] = "Existing", ["lastname"] = "Delete" });
            Service.Create(new Entity("contact") { Id = keepId, ["firstname"] = "Keep", ["lastname"] = "Record" });
            
            var nonExistentId = Guid.NewGuid();

            // Act - delete both existing and non-existent
            // Note: Capture $args values into variables before ForEach-Object since $args 
            // inside scriptblock refers to scriptblock args, not script args
            ps.AddScript(@"
                $conn = $args[2]
                @($args[0], $args[1]) | ForEach-Object {
                    Remove-DataverseRecord -Connection $conn -TableName contact -Id $_ -IfExists
                }
            ");
            ps.AddArgument(existingId);
            ps.AddArgument(nonExistentId);
            ps.AddArgument(mockConnection);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            var remaining = Service.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet("firstname") });
            remaining.Entities.Should().HaveCount(1);
            remaining.Entities[0].GetAttributeValue<string>("firstname").Should().Be("Keep");
        }

        // ===== WhatIf Tests (from Remove-DataverseRecord-WhatIfConfirm.Tests.ps1) =====

        [Fact]
        public void RemoveDataverseRecord_WhatIf_DoesNotDeleteRecord()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            Service!.Create(new Entity("contact") { Id = id1, ["firstname"] = "WhatIf1", ["lastname"] = "Test" });
            Service.Create(new Entity("contact") { Id = id2, ["firstname"] = "WhatIf2", ["lastname"] = "Test" });

            // Act
            ps.AddCommand("Remove-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", id1)
              .AddParameter("WhatIf", true);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            var remaining = Service.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet("contactid") });
            remaining.Entities.Should().HaveCount(2);
            remaining.Entities.Should().Contain(e => e.Id == id1);
        }

        [Fact]
        public void RemoveDataverseRecord_WhatIf_WithPipeline_DoesNotDeleteRecords()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            Service!.Create(new Entity("contact") { Id = id1, ["firstname"] = "Pipe1", ["lastname"] = "WhatIf" });
            Service.Create(new Entity("contact") { Id = id2, ["firstname"] = "Pipe2", ["lastname"] = "WhatIf" });

            // Act - pipeline delete with WhatIf (use PSCustomObject for property binding)
            ps.AddScript(@"
                @(
                    [PSCustomObject]@{ Id = $args[0]; TableName = 'contact' },
                    [PSCustomObject]@{ Id = $args[1]; TableName = 'contact' }
                ) | Remove-DataverseRecord -Connection $args[2] -WhatIf
            ");
            ps.AddArgument(id1);
            ps.AddArgument(id2);
            ps.AddArgument(mockConnection);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            var remaining = Service.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet("contactid") });
            remaining.Entities.Should().HaveCount(2);
        }

        [Fact]
        public void RemoveDataverseRecord_WhatIf_WithBatch_DoesNotDeleteAnyRecords()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var id3 = Guid.NewGuid();
            Service!.Create(new Entity("contact") { Id = id1, ["firstname"] = "Batch1", ["lastname"] = "WhatIf" });
            Service.Create(new Entity("contact") { Id = id2, ["firstname"] = "Batch2", ["lastname"] = "WhatIf" });
            Service.Create(new Entity("contact") { Id = id3, ["firstname"] = "Batch3", ["lastname"] = "WhatIf" });

            // Act - batch delete with WhatIf
            // Note: Capture $args values before ForEach-Object to avoid scope issues
            ps.AddScript(@"
                $conn = $args[3]
                @($args[0], $args[1], $args[2]) | ForEach-Object {
                    Remove-DataverseRecord -Connection $conn -TableName contact -Id $_ -WhatIf
                }
            ");
            ps.AddArgument(id1);
            ps.AddArgument(id2);
            ps.AddArgument(id3);
            ps.AddArgument(mockConnection);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            var remaining = Service.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet("contactid") });
            remaining.Entities.Should().HaveCount(3);
        }

        // ===== Confirm Tests =====

        [Fact]
        public void RemoveDataverseRecord_ConfirmFalse_DeletesRecord()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var contactId = Guid.NewGuid();
            Service!.Create(new Entity("contact") { Id = contactId, ["firstname"] = "Confirm", ["lastname"] = "False" });

            // Act
            ps.AddCommand("Remove-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", contactId)
              .AddParameter("Confirm", false);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            
            // Verify record was deleted (use RetrieveMultiple since FakeXrmEasy Retrieve throws exception for non-existent records)
            var query = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("contactid"),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition("contactid", ConditionOperator.Equal, contactId);
            var result = Service.RetrieveMultiple(query);
            result.Entities.Should().BeEmpty("record should have been deleted");
        }

        [Fact]
        public void RemoveDataverseRecord_WhatIfOverridesConfirmFalse()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var contactId = Guid.NewGuid();
            Service!.Create(new Entity("contact") { Id = contactId, ["firstname"] = "Both", ["lastname"] = "Flags" });

            // Act - WhatIf should take precedence
            ps.AddCommand("Remove-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", contactId)
              .AddParameter("WhatIf", true)
              .AddParameter("Confirm", false);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            
            // Verify record still exists (WhatIf should prevent deletion)
            var query = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("contactid"),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition("contactid", ConditionOperator.Equal, contactId);
            var result = Service.RetrieveMultiple(query);
            result.Entities.Should().HaveCount(1, "WhatIf should prevent deletion");
        }

        // ===== Retry Tests (from Remove-DataverseRecord.Tests.ps1) =====
        // NOTE: Retry tests require failure simulation via interceptor that can distinguish
        // between metadata requests (which must succeed) and Delete/ExecuteMultiple requests
        // (which should fail). The current interceptor architecture runs the same interceptor
        // for all requests. These tests are validated via E2E tests with real Dataverse.

        [Fact]
        public void RemoveDataverseRecord_RetriesWholeBatchOnExecuteMultipleFailure()
        {
            // Arrange: fail the first ExecuteMultiple/Delete then succeed
            var simulator = new TransientFailureSimulator(1, TransientFailureSimulator.ContainsDelete);
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection(simulator.Intercept, "contact");

            var ids = new List<Guid>();
            for (int i = 0; i < 3; i++)
            {
                var id = Guid.NewGuid();
                Service!.Create(new Entity("contact") { Id = id, ["firstname"] = $"Retry{i}" });
                ids.Add(id);
            }

            ps.AddScript(@"
                param($ids, $conn)
                $ids | ForEach-Object {
                    [pscustomobject]@{ Id = $_; TableName = 'contact' }
                } | Remove-DataverseRecord -Connection $conn -BatchSize 3 -Retries 1 -InitialRetryDelay 0
            ");
            ps.AddArgument(ids.ToArray());
            ps.AddArgument(mockConnection);

            // Act
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse(string.Join(" | ", ps.Streams.Error.Select(e => e.ToString())));
            results.Should().BeEmpty();

            var remaining = Service!.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet(false) });
            remaining.Entities.Should().BeEmpty();
        }

        [Fact]
        public void RemoveDataverseRecord_EmitsErrorsWhenBatchRetriesExceeded()
        {
            // Arrange: fail more times than retries
            var simulator = new TransientFailureSimulator(3, TransientFailureSimulator.ContainsDelete);
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection(simulator.Intercept, "contact");

            var ids = new List<Guid>();
            for (int i = 0; i < 2; i++)
            {
                var id = Guid.NewGuid();
                Service!.Create(new Entity("contact") { Id = id, ["firstname"] = $"Fail{i}" });
                ids.Add(id);
            }

            ps.AddScript(@"
                param($ids, $conn)
                $ids | ForEach-Object {
                    [pscustomobject]@{ Id = $_; TableName = 'contact' }
                } | Remove-DataverseRecord -Connection $conn -BatchSize 2 -Retries 1 -InitialRetryDelay 0 -ErrorAction Continue
            ");
            ps.AddArgument(ids.ToArray());
            ps.AddArgument(mockConnection);

            // Act
            var results = ps.Invoke();

            // Assert: expect errors recorded and contacts still present
            ps.HadErrors.Should().BeTrue();
            ps.Streams.Error.Should().NotBeEmpty();
            results.Should().BeEmpty();

            var remaining = Service!.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet(false) });
            remaining.Entities.Should().HaveCount(2);
        }

        // ===== Parallel Processing Tests (from Remove-DataverseRecord.Tests.ps1) =====

        [Fact]
        public void RemoveDataverseRecord_ProcessesDeletesInParallel()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var ids = new List<Guid>();
            for (int i = 0; i < 10; i++)
            {
                var id = Guid.NewGuid();
                Service!.Create(new Entity("contact") { Id = id, ["firstname"] = $"Test{i}", ["lastname"] = $"User{i}" });
                ids.Add(id);
            }

            // Act - use PSCustomObject for proper parameter binding
            ps.AddScript(@"
                $ids = @($args[0..9])
                $ids | ForEach-Object {
                    [PSCustomObject]@{ Id = $_; TableName = 'contact' }
                } | Remove-DataverseRecord -Connection $args[10] -MaxDegreeOfParallelism 2 -Verbose
            ");
            foreach (var id in ids)
            {
                ps.AddArgument(id);
            }
            ps.AddArgument(mockConnection);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            var remaining = Service.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet("contactid") });
            remaining.Entities.Should().BeEmpty();
        }

        [Fact]
        public void RemoveDataverseRecord_ParallelWithDegreeOne_WorksSequentially()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var ids = new List<Guid>();
            for (int i = 0; i < 5; i++)
            {
                var id = Guid.NewGuid();
                Service!.Create(new Entity("contact") { Id = id, ["firstname"] = $"Test{i}", ["lastname"] = $"User{i}" });
                ids.Add(id);
            }

            // Act - use PSCustomObject for proper parameter binding
            ps.AddScript(@"
                $ids = @($args[0..4])
                $ids | ForEach-Object {
                    [PSCustomObject]@{ Id = $_; TableName = 'contact' }
                } | Remove-DataverseRecord -Connection $args[5] -MaxDegreeOfParallelism 1
            ");
            foreach (var id in ids)
            {
                ps.AddArgument(id);
            }
            ps.AddArgument(mockConnection);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            var remaining = Service.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet("contactid") });
            remaining.Entities.Should().BeEmpty();
        }

        [Fact]
        public void RemoveDataverseRecord_CombinesParallelProcessingWithBatching()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var ids = new List<Guid>();
            for (int i = 0; i < 20; i++)
            {
                var id = Guid.NewGuid();
                Service!.Create(new Entity("contact") { Id = id, ["firstname"] = $"Test{i}", ["lastname"] = $"User{i}" });
                ids.Add(id);
            }

            // Act - use PSCustomObject for proper parameter binding
            ps.AddScript(@"
                $ids = @($args[0..19])
                $ids | ForEach-Object {
                    [PSCustomObject]@{ Id = $_; TableName = 'contact' }
                } | Remove-DataverseRecord -Connection $args[20] -MaxDegreeOfParallelism 3 -BatchSize 5 -Verbose
            ");
            foreach (var id in ids)
            {
                ps.AddArgument(id);
            }
            ps.AddArgument(mockConnection);
            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            var remaining = Service.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet("contactid") });
            remaining.Entities.Should().BeEmpty();
        }

        // ===== Parallel + Retry Tests (from Remove-DataverseRecord-Parallel-Retries.Tests.ps1) =====
        // NOTE: These tests require failure simulation via interceptor that can distinguish
        // between metadata requests and Delete/ExecuteMultiple requests. The current
        // interceptor architecture applies the same logic to all requests.
        // These tests are validated via E2E tests with real Dataverse.

        [Fact]
        public void RemoveDataverseRecord_ParallelMode_RetriesFailedDeleteOperationsWithBatching()
        {
            var simulator = new TransientFailureSimulator(1, TransientFailureSimulator.ContainsDelete);
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection(simulator.Intercept, "contact");

            var ids = new List<Guid>();
            for (int i = 0; i < 6; i++)
            {
                var id = Guid.NewGuid();
                Service!.Create(new Entity("contact") { Id = id, ["firstname"] = $"Par{i}" });
                ids.Add(id);
            }

            ps.AddScript(@"
                param($ids, $conn)
                $ids | ForEach-Object {
                    [pscustomobject]@{ Id = $_; TableName = 'contact' }
                } | Remove-DataverseRecord -Connection $conn -BatchSize 3 -MaxDegreeOfParallelism 2 -Retries 1 -InitialRetryDelay 0
            ");
            ps.AddArgument(ids.ToArray());
            ps.AddArgument(mockConnection);

            var results = ps.Invoke();

            ps.HadErrors.Should().BeFalse(string.Join(" | ", ps.Streams.Error.Select(e => e.ToString())));
            results.Should().BeEmpty();

            var remaining = Service!.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet(false) });
            remaining.Entities.Should().BeEmpty();
        }

        [Fact]
        public void RemoveDataverseRecord_ParallelMode_ExhaustsRetriesAndReportsError()
        {
            // Ensure enough simulated failures to cover all parallel attempts (records * (retries+1))
            var simulator = new TransientFailureSimulator(8, TransientFailureSimulator.ContainsDelete);
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection(simulator.Intercept, "contact");

            var ids = new List<Guid>();
            for (int i = 0; i < 4; i++)
            {
                var id = Guid.NewGuid();
                Service!.Create(new Entity("contact") { Id = id, ["firstname"] = $"ParFail{i}" });
                ids.Add(id);
            }

            ps.AddScript(@"
                param($ids, $conn)
                $ids | ForEach-Object {
                    [pscustomobject]@{ Id = $_; TableName = 'contact' }
                } | Remove-DataverseRecord -Connection $conn -BatchSize 2 -MaxDegreeOfParallelism 2 -Retries 1 -InitialRetryDelay 0 -ErrorAction Continue
            ");
            ps.AddArgument(ids.ToArray());
            ps.AddArgument(mockConnection);

            var results = ps.Invoke();

            ps.HadErrors.Should().BeTrue();
            ps.Streams.Error.Should().NotBeEmpty();
            results.Should().BeEmpty();

            var remaining = Service!.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet(false) });
            remaining.Entities.Should().HaveCount(4);
        }
    }
}
