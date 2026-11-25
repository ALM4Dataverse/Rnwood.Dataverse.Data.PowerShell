using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using AwesomeAssertions;
using Microsoft.Xrm.Sdk;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Batched Retrieval tests for Set-DataverseRecord cmdlet.
    /// Migrated from Pester tests in tests/Set-DataverseRecord-BatchedRetrieval.Tests.ps1
    /// </summary>
    public class SetDataverseRecord_BatchedRetrievalTests : TestBase
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
        public void SetDataverseRecord_RetrievalBatchSize_AcceptsCustomValues()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var contact = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["contactid"] = Guid.NewGuid(),
                ["firstname"] = "Test"
            };
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("RetrievalBatchSize", (uint)100);
            ps.Invoke(new[] { PSObject.AsPSObject(contact) });
            
            // Assert
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void SetDataverseRecord_RetrievalBatchSize_AcceptsValueOfOneToDisableBatching()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var contact = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["contactid"] = Guid.NewGuid(),
                ["firstname"] = "Test"
            };
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("RetrievalBatchSize", (uint)1);
            ps.Invoke(new[] { PSObject.AsPSObject(contact) });
            
            // Assert
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void SetDataverseRecord_RetrievalBatch_BatchesRetrievalOfMultipleRecordsByID()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var contacts = new List<PSObject>();
            for (int i = 1; i <= 10; i++)
            {
                var contact = new Entity("contact")
                {
                    Id = Guid.NewGuid(),
                    ["contactid"] = Guid.NewGuid(),
                    ["firstname"] = $"Batch{i}"
                };
                contacts.Add(PSObject.AsPSObject(contact));
            }
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("RetrievalBatchSize", (uint)500);
            ps.Invoke(contacts);
            
            // Assert
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void SetDataverseRecord_RetrievalBatch_ProcessesRecordsWhenRetrievalBatchIsFull()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var contacts = new List<PSObject>();
            for (int i = 1; i <= 5; i++)
            {
                var contact = new Entity("contact")
                {
                    Id = Guid.NewGuid(),
                    ["contactid"] = Guid.NewGuid(),
                    ["firstname"] = $"Batch{i}"
                };
                contacts.Add(PSObject.AsPSObject(contact));
            }
            
            // Act - batch size of 3, so should process in 2 batches: 3 + 2
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("RetrievalBatchSize", (uint)3);
            ps.Invoke(contacts);
            
            // Assert
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void SetDataverseRecord_RetrievalBatch_ProcessesRemainingRecordsInEndProcessing()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var contacts = new List<PSObject>();
            for (int i = 1; i <= 7; i++)
            {
                var contact = new Entity("contact")
                {
                    Id = Guid.NewGuid(),
                    ["contactid"] = Guid.NewGuid(),
                    ["firstname"] = $"Batch{i}"
                };
                contacts.Add(PSObject.AsPSObject(contact));
            }
            
            // Act - batch size of 5, so process 5 in ProcessRecord, 2 in EndProcessing
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("RetrievalBatchSize", (uint)5);
            ps.Invoke(contacts);
            
            // Assert
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void SetDataverseRecord_RetrievalBatch_WorksWithRetrievalBatchSizeOfOne()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var contacts = new List<PSObject>();
            for (int i = 1; i <= 3; i++)
            {
                var contact = new Entity("contact")
                {
                    Id = Guid.NewGuid(),
                    ["contactid"] = Guid.NewGuid(),
                    ["firstname"] = $"NoBatch{i}"
                };
                contacts.Add(PSObject.AsPSObject(contact));
            }
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("RetrievalBatchSize", (uint)1);
            ps.Invoke(contacts);
            
            // Assert
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void SetDataverseRecord_RetrievalBatch_EmptyPipeline_DoesNotThrow()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Act - Empty array via pipeline (PowerShell processes 0 items, doesn't throw for mandatory pipeline params)
            ps.AddScript(@"
                param($connection)
                @() | Set-DataverseRecord -Connection $connection -TableName contact -RetrievalBatchSize 500 -PassThru
            ")
            .AddParameter("connection", mockConnection);
            var results = ps.Invoke();
            
            // Assert - Should process gracefully with no errors and no results
            ps.HadErrors.Should().BeFalse("empty pipeline should process 0 items without error");
            results.Should().BeEmpty("no records provided means no output");
        }

        [Fact]
        public void SetDataverseRecord_RetrievalBatch_HandlesSingleRecord()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var contact = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["contactid"] = Guid.NewGuid(),
                ["firstname"] = "Single"
            };
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("RetrievalBatchSize", (uint)500);
            ps.Invoke(new[] { PSObject.AsPSObject(contact) });
            
            // Assert
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void SetDataverseRecord_RetrievalBatch_HandlesRecordsThatDontNeedRetrieval_CreateOnly()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var contacts = new List<PSObject>();
            for (int i = 1; i <= 5; i++)
            {
                var contact = new Entity("contact")
                {
                    ["firstname"] = $"Create{i}"
                };
                contacts.Add(PSObject.AsPSObject(contact));
            }
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("RetrievalBatchSize", (uint)500);
            ps.Invoke(contacts);
            
            // Assert
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void SetDataverseRecord_RetrievalBatch_HandlesRecordsThatDontNeedRetrieval_UpdateAllColumns()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create contacts first
            var createdIds = new List<Guid>();
            for (int i = 1; i <= 5; i++)
            {
                var contact = new Entity("contact")
                {
                    Id = Guid.NewGuid(),
                    ["firstname"] = $"Create{i}"
                };
                Service!.Create(contact);
                createdIds.Add(contact.Id);
            }
            
            // Now update them with UpdateAllColumns
            var updates = createdIds.Select((id, i) =>
            {
                var contact = new Entity("contact")
                {
                    Id = id,
                    ["contactid"] = id,
                    ["firstname"] = $"Update{i + 1}"
                };
                return PSObject.AsPSObject(contact);
            }).ToList();
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("UpdateAllColumns", true)
              .AddParameter("RetrievalBatchSize", (uint)500);
            ps.Invoke(updates);
            
            // Assert
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void SetDataverseRecord_RetrievalBatch_HandlesMixOfRecordsWithAndWithoutIDs()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var contacts = new List<PSObject>();
            
            // Some with IDs (need retrieval)
            for (int i = 1; i <= 3; i++)
            {
                var contact = new Entity("contact")
                {
                    Id = Guid.NewGuid(),
                    ["contactid"] = Guid.NewGuid(),
                    ["firstname"] = $"WithId{i}"
                };
                contacts.Add(PSObject.AsPSObject(contact));
            }
            
            // Some without IDs (no retrieval needed - will create)
            for (int i = 4; i <= 6; i++)
            {
                var contact = new Entity("contact")
                {
                    ["firstname"] = $"NoId{i}"
                };
                contacts.Add(PSObject.AsPSObject(contact));
            }
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("RetrievalBatchSize", (uint)500);
            ps.Invoke(contacts);
            
            // Assert
            ps.HadErrors.Should().BeFalse();
        }
    }
}
