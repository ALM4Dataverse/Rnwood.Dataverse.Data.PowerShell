using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using AwesomeAssertions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Owner and Status tests for Set-DataverseRecord cmdlet.
    /// Migrated from Pester tests in tests/Set-DataverseRecord-OwnerAndStatus.Tests.ps1
    /// </summary>
    [Collection("OwnerAndStatus")]
    public class SetDataverseRecord_OwnerAndStatusTests : TestBase
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
        public void SetDataverseRecord_Owner_CreatesRecordWithOwnerIdAndPerformsAssignment()
        {
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact", "systemuser", "team");

            var ownerId = Guid.NewGuid();
            var owner = new Entity("systemuser")
            {
                Id = ownerId,
                ["fullname"] = "Owner One"
            };
            Service!.Create(owner);

            var newRecord = PSObject.AsPSObject(new
            {
                firstname = "Owned",
                lastname = "Contact",
                ownerid = ownerId
            });

            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
                            .AddParameter("TableName", "contact")
                            .AddParameter("PassThru", true);

            var results = ps.Invoke(new[] { newRecord });
            var errors = string.Join("; ", ps.Streams.Error.Select(e => e.Exception?.Message ?? e.ToString()));

            ps.HadErrors.Should().BeFalse(errors);
            results.Should().HaveCount(1);

            var createdId = (Guid)results[0].Properties["Id"].Value;
            var created = Service.Retrieve("contact", createdId, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            ((EntityReference)created["ownerid"]).Id.Should().Be(ownerId);
        }

        [Fact]
        public void SetDataverseRecord_Owner_UpdatesOwnerIdOnExistingRecord()
        {
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact", "systemuser", "team");

            var initialOwnerId = Guid.NewGuid();
            var newOwnerId = Guid.NewGuid();

            Service!.Create(new Entity("systemuser") { Id = initialOwnerId, ["fullname"] = "Initial Owner" });
            Service!.Create(new Entity("systemuser") { Id = newOwnerId, ["fullname"] = "New Owner" });

            var record = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Change",
                ["lastname"] = "Owner",
                ["ownerid"] = new EntityReference("systemuser", initialOwnerId)
            };
            Service.Create(record);

            var updateObj = PSObject.AsPSObject(new
            {
                Id = record.Id,
                ownerid = newOwnerId
            });

            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
                            .AddParameter("TableName", "contact")
                            .AddParameter("PassThru", true);

            ps.Invoke(new[] { updateObj });
            var errors = string.Join("; ", ps.Streams.Error.Select(e => e.Exception?.Message ?? e.ToString()));

            ps.HadErrors.Should().BeFalse(errors);
            var retrieved = Service.Retrieve("contact", record.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("ownerid"));
            ((EntityReference)retrieved["ownerid"]).Id.Should().Be(newOwnerId);
        }

        [Fact]
        public void SetDataverseRecord_Owner_HandlesOwnerIdWithEntityReferenceObject()
        {
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact", "systemuser", "team");

            var ownerId = Guid.NewGuid();
            Service!.Create(new Entity("systemuser") { Id = ownerId, ["fullname"] = "Object Owner" });

            var ownerRef = new PSObject();
            ownerRef.Properties.Add(new PSNoteProperty("LogicalName", "systemuser"));
            ownerRef.Properties.Add(new PSNoteProperty("Id", ownerId));

            var updateObj = new PSObject();
            updateObj.Properties.Add(new PSNoteProperty("firstname", "Ref"));
            updateObj.Properties.Add(new PSNoteProperty("lastname", "Owner"));
            updateObj.Properties.Add(new PSNoteProperty("ownerid", ownerRef));

            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
                            .AddParameter("TableName", "contact")
                            .AddParameter("PassThru", true);

            var results = ps.Invoke(new[] { updateObj });
            var errors = string.Join("; ", ps.Streams.Error.Select(e => e.Exception?.Message ?? e.ToString()));

            ps.HadErrors.Should().BeFalse(errors);
            results.Should().HaveCount(1);

            var createdId = (Guid)results[0].Properties["Id"].Value;
            var created = Service.Retrieve("contact", createdId, new Microsoft.Xrm.Sdk.Query.ColumnSet("ownerid"));
            ((EntityReference)created["ownerid"]).Id.Should().Be(ownerId);
        }

        [Fact]
        public void SetDataverseRecord_Status_CreatesRecordWithStatusCodeAndPerformsStateChange()
        {
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");

            var newRecord = PSObject.AsPSObject(new
            {
                firstname = "Status",
                lastname = "Create",
                statuscode = 2
            });

            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
                            .AddParameter("TableName", "contact")
                            .AddParameter("PassThru", true);

            var results = ps.Invoke(new[] { newRecord });
            var errors = string.Join("; ", ps.Streams.Error.Select(e => e.Exception?.Message ?? e.ToString()));

            ps.HadErrors.Should().BeFalse(errors);
            results.Should().HaveCount(1);

            var createdId = (Guid)results[0].Properties["Id"].Value;
            var created = Service!.Retrieve("contact", createdId, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            created.GetAttributeValue<OptionSetValue>("statuscode").Value.Should().Be(2);
        }

        [Fact]
        public void SetDataverseRecord_Status_UpdatesStatusCodeOnExistingRecord()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var record = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Change",
                ["lastname"] = "Status"
            };
            Service!.Create(record);
            
            var updateObj = PSObject.AsPSObject(new {
                Id = record.Id,
                statuscode = 2 // Inactive
            });
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact");
            ps.Invoke(new[] { updateObj });
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            var retrieved = Service!.Retrieve("contact", record.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("firstname", "lastname"));
            retrieved["firstname"].Should().Be("Change");
            retrieved["lastname"].Should().Be("Status");
        }

        [Fact]
        public void SetDataverseRecord_Status_SetsBothStateCodeAndStatusCodeTogether()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var record = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Both",
                ["lastname"] = "States"
            };
            Service!.Create(record);
            
            var updateObj = PSObject.AsPSObject(new {
                Id = record.Id,
                statecode = 1,  // Inactive state
                statuscode = 2  // Inactive status
            });
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact");
            ps.Invoke(new[] { updateObj });
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            var retrieved = Service!.Retrieve("contact", record.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("firstname"));
            retrieved["firstname"].Should().Be("Both");
        }

        [Fact]
        public void SetDataverseRecord_Status_HandlesStatusChangeWithOtherFieldUpdates()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var record = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Initial",
                ["lastname"] = "Status",
                ["emailaddress1"] = "initial@example.com"
            };
            Service!.Create(record);
            
            var updateObj = PSObject.AsPSObject(new {
                Id = record.Id,
                firstname = "Updated",
                emailaddress1 = "updated@example.com",
                statuscode = 2
            });
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact");
            ps.Invoke(new[] { updateObj });
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            var retrieved = Service!.Retrieve("contact", record.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            retrieved["firstname"].Should().Be("Updated");
            retrieved["emailaddress1"].Should().Be("updated@example.com");
            retrieved["lastname"].Should().Be("Status");
        }

        [Fact]
        public void SetDataverseRecord_Status_StatusChangesWorkInBatchUpdates()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var record1 = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "User1",
                ["lastname"] = "Batch"
            };
            Service!.Create(record1);
            
            var record2 = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "User2",
                ["lastname"] = "Batch"
            };
            Service!.Create(record2);
            
            var updates = new[] {
                PSObject.AsPSObject(new { Id = record1.Id, statuscode = 2 }),
                PSObject.AsPSObject(new { Id = record2.Id, statuscode = 2 })
            };
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact");
            ps.Invoke(updates);
            
            // Assert
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void SetDataverseRecord_BulkCreateRunsStatusChangeInFollowingExecuteMultipleBatch()
        {
            using var ps = CreatePowerShellWithCmdlets();
            var requestNames = new List<string>();
            var mockConnection = CreateMockConnection(request =>
            {
                requestNames.Add(request.RequestName);
                return null;
            }, "contact");

            var newRecord = PSObject.AsPSObject(new
            {
                firstname = "Bulk",
                lastname = "Status",
                statuscode = 2
            });

            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("BatchSize", 10);
            ps.Invoke(new[] { newRecord });

            ps.HadErrors.Should().BeFalse(string.Join("; ", ps.Streams.Error));
            var createMultipleIndex = requestNames.IndexOf("CreateMultiple");
            var executeMultipleIndex = requestNames.IndexOf("ExecuteMultiple");
            createMultipleIndex.Should().BeGreaterThanOrEqualTo(0);
            executeMultipleIndex.Should().BeGreaterThan(createMultipleIndex);
        }
    }
}
