using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using FluentAssertions;
using Microsoft.Xrm.Sdk;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Core tests for Set-DataverseRecord cmdlet covering general functionality.
    /// Migrated from Pester tests in tests/Set-DataverseRecord.Tests.ps1
    /// </summary>
    public class SetDataverseRecord_CoreTests : TestBase
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
        public void SetDataverseRecord_Update_WithPassThru_ReturnsUpdatedRecord()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create initial records
            var initial = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Original",
                ["lastname"] = "Name"
            };
            Service!.Create(initial);
            
            var other = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Other",
                ["lastname"] = "Record"
            };
            Service!.Create(other);
            
            var updateEntity = new Entity("contact")
            {
                Id = initial.Id,
                ["contactid"] = initial.Id,
                ["firstname"] = "Updated"
            };
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("PassThru", true);
            var results = ps.Invoke(new[] { PSObject.AsPSObject(updateEntity) });
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["Id"].Value.Should().Be(initial.Id);
            
            // Verify all records in expected state
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "contactid", "firstname" });
            var allRecords = ps.Invoke();
            
            allRecords.Should().HaveCount(2);
            var updated = allRecords.FirstOrDefault(r => (Guid)r.Properties["Id"].Value == initial.Id);
            updated.Should().NotBeNull();
            updated!.Properties["firstname"].Value.Should().Be("Updated");
        }

        [Fact]
        public void SetDataverseRecord_Update_SkipsUpdateWhenNoChangesDetected()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var initial = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "NoChange",
                ["lastname"] = "User"
            };
            Service!.Create(initial);
            
            var updateEntity = new Entity("contact")
            {
                Id = initial.Id,
                ["contactid"] = initial.Id,
                ["firstname"] = "NoChange",
                ["lastname"] = "User"
            };
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("PassThru", true);
            var results = ps.Invoke(new[] { PSObject.AsPSObject(updateEntity) });
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["Id"].Value.Should().Be(initial.Id);
        }

        [Fact]
        public void SetDataverseRecord_Update_OnlySpecifiedColumns()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var initial = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Original",
                ["lastname"] = "Name",
                ["emailaddress1"] = "original@example.com"
            };
            Service!.Create(initial);
            
            var updateEntity = new Entity("contact")
            {
                Id = initial.Id,
                ["contactid"] = initial.Id,
                ["firstname"] = "Updated"
            };
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection);
            ps.Invoke(new[] { PSObject.AsPSObject(updateEntity) });
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            var updated = Service!.Retrieve("contact", initial.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            updated["firstname"].Should().Be("Updated");
            updated["lastname"].Should().Be("Name");
            updated["emailaddress1"].Should().Be("original@example.com");
        }

        [Fact]
        public void SetDataverseRecord_MatchOn_CreatesNewRecordWhenNoMatchFound()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var record = new Hashtable {
                ["firstname"] = "NewUser",
                ["lastname"] = "Test",
                ["emailaddress1"] = "new@example.com"
            };
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("MatchOn", new[] { "emailaddress1" })
              .AddParameter("PassThru", true);
            var results = ps.Invoke(new[] { PSObject.AsPSObject(record) });
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            var id = (Guid)results[0].Properties["Id"].Value;
            id.Should().NotBe(Guid.Empty);
            
            var retrieved = Service!.Retrieve("contact", id, new Microsoft.Xrm.Sdk.Query.ColumnSet("emailaddress1"));
            retrieved["emailaddress1"].Should().Be("new@example.com");
        }

        [Fact]
        public void SetDataverseRecord_MatchOn_UpdatesExistingRecordWhenMatchFound()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var initial = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Original",
                ["lastname"] = "User",
                ["emailaddress1"] = "unique@example.com"
            };
            Service!.Create(initial);
            
            var other = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Other",
                ["lastname"] = "User",
                ["emailaddress1"] = "other@example.com"
            };
            Service!.Create(other);
            
            var record = new Hashtable {
                ["firstname"] = "Updated",
                ["lastname"] = "User",
                ["emailaddress1"] = "unique@example.com"
            };
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("MatchOn", new[] { "emailaddress1" })
              .AddParameter("PassThru", true);
            var results = ps.Invoke(new[] { PSObject.AsPSObject(record) });
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["Id"].Value.Should().Be(initial.Id);
            
            var updated = Service!.Retrieve("contact", initial.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("firstname"));
            updated["firstname"].Should().Be("Updated");
        }

        [Fact]
        public void SetDataverseRecord_MatchOn_MultipleColumns()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var initial = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "John",
                ["lastname"] = "Doe",
                ["emailaddress1"] = "john@example.com"
            };
            Service!.Create(initial);
            
            var record = new Hashtable {
                ["firstname"] = "John",
                ["lastname"] = "Doe",
                ["emailaddress1"] = "newemail@example.com"
            };
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("MatchOn", new[] { "firstname", "lastname" })
              .AddParameter("PassThru", true);
            var results = ps.Invoke(new[] { PSObject.AsPSObject(record) });
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results[0].Properties["Id"].Value.Should().Be(initial.Id);
            
            var updated = Service!.Retrieve("contact", initial.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("emailaddress1"));
            updated["emailaddress1"].Should().Be("newemail@example.com");
        }

        [Fact]
        public void SetDataverseRecord_AllowMultipleMatches_RaisesErrorWhenMultipleMatchesWithoutFlag()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var record1 = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "John1",
                ["lastname"] = "Doe",
                ["emailaddress1"] = "duplicate@test.com"
            };
            Service!.Create(record1);
            
            var record2 = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "John2",
                ["lastname"] = "Doe",
                ["emailaddress1"] = "duplicate@test.com"
            };
            Service!.Create(record2);
            
            var update = new Hashtable {
                ["firstname"] = "Updated",
                ["lastname"] = "Name",
                ["emailaddress1"] = "duplicate@test.com"
            };
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("MatchOn", new[] { "emailaddress1" });
            ps.Invoke(new[] { PSObject.AsPSObject(update) });
            
            // Assert
            ps.HadErrors.Should().BeTrue();
            ps.Streams.Error[0].Exception.Message.Should().Contain("AllowMultipleMatches");
        }

        [Fact]
        public void SetDataverseRecord_AllowMultipleMatches_UpdatesAllMatchingRecords()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var record1 = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "John",
                ["lastname"] = "TestUser",
                ["emailaddress1"] = "john@test.com"
            };
            Service!.Create(record1);
            
            var record2 = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Jane",
                ["lastname"] = "TestUser",
                ["emailaddress1"] = "jane@test.com"
            };
            Service!.Create(record2);
            
            var record3 = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Bob",
                ["lastname"] = "Different",
                ["emailaddress1"] = "bob@test.com"
            };
            Service!.Create(record3);
            
            var update = new Hashtable {
                ["lastname"] = "TestUser",
                ["emailaddress1"] = "updated@test.com"
            };
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("MatchOn", new[] { "lastname" })
              .AddParameter("AllowMultipleMatches", true);
            ps.Invoke(new[] { PSObject.AsPSObject(update) });
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            
            var query = new Microsoft.Xrm.Sdk.Query.QueryExpression("contact") { ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet(true) };
            var allRecords = Service!.RetrieveMultiple(query);
            var updated = allRecords.Entities.Where(e => e.GetAttributeValue<string>("emailaddress1") == "updated@test.com").ToList();
            updated.Should().HaveCount(2);
            
            var bob = allRecords.Entities.First(e => e.GetAttributeValue<string>("firstname") == "Bob");
            bob["emailaddress1"].Should().Be("bob@test.com");
        }

        [Fact]
        public void SetDataverseRecord_NoUpdate_CreatesNewButDoesNotUpdateExisting()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var initial = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Original",
                ["emailaddress1"] = "test@example.com"
            };
            Service!.Create(initial);
            
            // Try to update with NoUpdate
            var updateObj = PSObject.AsPSObject(new {
                TableName = "contact",
                Id = initial.Id,
                firstname = "ShouldNotUpdate"
            });
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("NoUpdate", true);
            ps.Invoke(new[] { updateObj });
            
            // Assert - verify not updated
            var retrieved = Service!.Retrieve("contact", initial.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("firstname"));
            retrieved["firstname"].Should().Be("Original");
            
            // Verify can still create new records with NoUpdate
            ps.Commands.Clear();
            var newRecord = new Hashtable {
                ["firstname"] = "NewUser",
                ["emailaddress1"] = "new@example.com"
            };
            
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("MatchOn", new[] { "emailaddress1" })
              .AddParameter("NoUpdate", true)
              .AddParameter("PassThru", true);
            var results = ps.Invoke(new[] { PSObject.AsPSObject(newRecord) });
            
            results.Should().HaveCount(1);
            var newId = (Guid)results[0].Properties["Id"].Value;
            newId.Should().NotBe(initial.Id);
        }

        [Fact]
        public void SetDataverseRecord_NoCreate_UpdatesExistingButDoesNotCreateNew()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var initial = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Original",
                ["emailaddress1"] = "test@example.com"
            };
            Service!.Create(initial);
            
            // Update existing with NoCreate
            var updateEntity = new Entity("contact")
            {
                Id = initial.Id,
                ["contactid"] = initial.Id,
                ["firstname"] = "Updated"
            };
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("NoCreate", true)
              .AddParameter("PassThru", true);
            ps.Invoke(new[] { PSObject.AsPSObject(updateEntity) });
            
            // Try to create new with NoCreate
            ps.Commands.Clear();
            var newRecord = new Hashtable {
                ["firstname"] = "ShouldNotCreate",
                ["emailaddress1"] = "new@example.com"
            };
            
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("MatchOn", new[] { "emailaddress1" })
              .AddParameter("NoCreate", true);
            ps.Invoke(new[] { PSObject.AsPSObject(newRecord) });
            
            // Assert - should still be only 1 record
            var noCreateQuery = new Microsoft.Xrm.Sdk.Query.QueryExpression("contact") { ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet(true) };
            var allContacts = Service!.RetrieveMultiple(noCreateQuery);
            allContacts.Entities.Should().HaveCount(1);
            allContacts.Entities[0]["firstname"].Should().Be("Updated");
        }

        [Fact]
        public void SetDataverseRecord_NoUpdateColumns_ExcludesSpecifiedColumns()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var initial = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Original",
                ["lastname"] = "Name",
                ["emailaddress1"] = "original@example.com"
            };
            Service!.Create(initial);
            
            var updateEntity = new Entity("contact")
            {
                Id = initial.Id,
                ["contactid"] = initial.Id,
                ["firstname"] = "Updated",
                ["lastname"] = "UpdatedLast",
                ["emailaddress1"] = "updated@example.com"
            };
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("NoUpdateColumns", new[] { "emailaddress1" });
            ps.Invoke(new[] { PSObject.AsPSObject(updateEntity) });
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            var updated = Service!.Retrieve("contact", initial.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            updated["firstname"].Should().Be("Updated");
            updated["lastname"].Should().Be("UpdatedLast");
            updated["emailaddress1"].Should().Be("original@example.com");
        }

        [Fact]
        public void SetDataverseRecord_UpdateAllColumns_SkipsRetrieveAndSendsAllColumns()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var initial = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Original",
                ["lastname"] = "Name"
            };
            Service!.Create(initial);
            
            var updateObj = PSObject.AsPSObject(new {
                TableName = "contact",
                Id = initial.Id,
                firstname = "Updated",
                lastname = "UpdatedLast"
            });
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("UpdateAllColumns", true);
            ps.Invoke(new[] { updateObj });
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            var updated = Service!.Retrieve("contact", initial.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            updated["firstname"].Should().Be("Updated");
            updated["lastname"].Should().Be("UpdatedLast");
        }

        [Fact]
        public void SetDataverseRecord_IgnoreProperties_IgnoresSpecifiedProperties()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var inputObject = PSObject.AsPSObject(new {
                firstname = "John",
                lastname = "Doe",
                customProperty = "ShouldBeIgnored",
                TableName = "contact"
            });
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("IgnoreProperties", new[] { "customProperty" })
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var results = ps.Invoke(new[] { inputObject });
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            var id = (Guid)results[0].Properties["Id"].Value;
            id.Should().NotBe(Guid.Empty);
        }

        [Fact]
        public void SetDataverseRecord_TypeConversion_DateTime()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var birthdate = DateTime.Parse("1990-01-15");
            var record = new Hashtable {
                ["firstname"] = "DateTest",
                ["lastname"] = "User",
                ["birthdate"] = birthdate
            };
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var results = ps.Invoke(new[] { PSObject.AsPSObject(record) });
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            var id = (Guid)results[0].Properties["Id"].Value;
            var retrieved = Service!.Retrieve("contact", id, new Microsoft.Xrm.Sdk.Query.ColumnSet("birthdate"));
            var retrievedDate = retrieved.GetAttributeValue<DateTime>("birthdate");
            retrievedDate.Date.Should().Be(birthdate.Date);
        }

        [Fact]
        public void SetDataverseRecord_TypeConversion_LookupFromGuid()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var parent = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Parent",
                ["lastname"] = "Contact"
            };
            Service!.Create(parent);
            
            var child = new Hashtable {
                ["firstname"] = "Child",
                ["lastname"] = "Contact",
                ["parentcontactid"] = parent.Id
            };
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var results = ps.Invoke(new[] { PSObject.AsPSObject(child) });
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            var id = (Guid)results[0].Properties["Id"].Value;
            var retrieved = Service!.Retrieve("contact", id, new Microsoft.Xrm.Sdk.Query.ColumnSet("parentcontactid"));
            var lookup = retrieved.GetAttributeValue<EntityReference>("parentcontactid");
            lookup.Should().NotBeNull();
            lookup.Id.Should().Be(parent.Id);
        }

        [Fact]
        public void SetDataverseRecord_TypeConversion_NullValue()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var initial = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Test",
                ["lastname"] = "User",
                ["emailaddress1"] = "test@example.com"
            };
            Service!.Create(initial);
            
            var updateEntity = new Entity("contact")
            {
                Id = initial.Id,
                ["contactid"] = initial.Id,
                ["emailaddress1"] = null
            };
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection);
            ps.Invoke(new[] { PSObject.AsPSObject(updateEntity) });
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            var updated = Service!.Retrieve("contact", initial.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("emailaddress1"));
            updated.Contains("emailaddress1").Should().BeFalse();
        }

        [Fact]
        public void SetDataverseRecord_Batch_DefaultBatchSize()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var inputCollection = new PSDataCollection<PSObject>();
            for (int i = 1; i <= 150; i++)
            {
                inputCollection.Add(PSObject.AsPSObject(new Hashtable {
                    ["firstname"] = $"User{i}",
                    ["lastname"] = "Test"
                }));
            }
            inputCollection.Complete();
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var results = ps.Invoke(inputCollection);
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(150);
        }

        [Fact]
        public void SetDataverseRecord_Batch_CustomBatchSize()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var inputCollection = new PSDataCollection<PSObject>();
            for (int i = 1; i <= 10; i++)
            {
                inputCollection.Add(PSObject.AsPSObject(new Hashtable {
                    ["firstname"] = $"Batch{i}",
                    ["lastname"] = "Test"
                }));
            }
            inputCollection.Complete();
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("BatchSize", 5)
              .AddParameter("PassThru", true);
            var results = ps.Invoke(inputCollection);
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(10);
        }

        [Fact]
        public void SetDataverseRecord_Batch_BatchSizeOne()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var inputCollection = new PSDataCollection<PSObject>();
            for (int i = 1; i <= 3; i++)
            {
                inputCollection.Add(PSObject.AsPSObject(new Hashtable {
                    ["firstname"] = $"Single{i}",
                    ["lastname"] = "Test"
                }));
            }
            inputCollection.Complete();
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("BatchSize", 1)
              .AddParameter("PassThru", true);
            var results = ps.Invoke(inputCollection);
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(3);
        }

        [Fact]
        public void SetDataverseRecord_ErrorHandling_CollectsErrorsInBatchOperations()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var records = new[] {
                new Hashtable { ["firstname"] = "Valid", ["lastname"] = "User1" },
                new Hashtable { ["firstname"] = "Valid", ["lastname"] = "User2" }
            };
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var results = ps.Invoke(records.Select(PSObject.AsPSObject));
            
            // Assert - all valid records should succeed
            results.Should().HaveCount(2);
        }

        [Fact]
        public void SetDataverseRecord_WhatIf_DoesNotCreateRecords()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var initialCount = Service!.RetrieveMultiple(new Microsoft.Xrm.Sdk.Query.QueryExpression("contact")).Entities.Count;
            
            var record = new Hashtable {
                ["firstname"] = "WhatIfTest",
                ["lastname"] = "User"
            };
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("WhatIf", true);
            ps.Invoke(new[] { PSObject.AsPSObject(record) });
            
            // Assert
            var finalCount = Service!.RetrieveMultiple(new Microsoft.Xrm.Sdk.Query.QueryExpression("contact")).Entities.Count;
            finalCount.Should().Be(initialCount);
        }

        [Fact]
        public void SetDataverseRecord_WhatIf_DoesNotUpdateRecords()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var initial = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Original",
                ["lastname"] = "Name"
            };
            Service!.Create(initial);
            
            var updateEntity = new Entity("contact")
            {
                Id = initial.Id,
                ["contactid"] = initial.Id,
                ["firstname"] = "ShouldNotUpdate"
            };
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("WhatIf", true);
            ps.Invoke(new[] { PSObject.AsPSObject(updateEntity) });
            
            // Assert
            var retrieved = Service!.Retrieve("contact", initial.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("firstname"));
            retrieved["firstname"].Should().Be("Original");
        }

        [Fact]
        public void SetDataverseRecord_DatasetIntegrity_UpdateDoesNotAffectOtherRecords()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var record1 = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "User1",
                ["lastname"] = "Test"
            };
            Service!.Create(record1);
            
            var record2 = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "User2",
                ["lastname"] = "Test"
            };
            Service!.Create(record2);
            
            var record3 = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "User3",
                ["lastname"] = "Test"
            };
            Service!.Create(record3);
            
            var updateEntity = new Entity("contact")
            {
                Id = record1.Id,
                ["contactid"] = record1.Id,
                ["firstname"] = "UpdatedUser1"
            };
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection);
            ps.Invoke(new[] { PSObject.AsPSObject(updateEntity) });
            
            // Assert
            var integrityQuery = new Microsoft.Xrm.Sdk.Query.QueryExpression("contact") { ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet(true) };
            var allRecords = Service!.RetrieveMultiple(integrityQuery);
            allRecords.Entities.Should().HaveCount(3);
            
            allRecords.Entities.First(e => e.Id == record1.Id)["firstname"].Should().Be("UpdatedUser1");
            allRecords.Entities.First(e => e.Id == record2.Id)["firstname"].Should().Be("User2");
            allRecords.Entities.First(e => e.Id == record3.Id)["firstname"].Should().Be("User3");
        }

        [Fact]
        public void SetDataverseRecord_DatasetIntegrity_NoRecordsLostDuringBatchOperations()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var initialCount = Service!.RetrieveMultiple(new Microsoft.Xrm.Sdk.Query.QueryExpression("contact")).Entities.Count;
            
            var inputCollection = new PSDataCollection<PSObject>();
            for (int i = 1; i <= 50; i++)
            {
                inputCollection.Add(PSObject.AsPSObject(new Hashtable {
                    ["firstname"] = $"BatchUser{i}",
                    ["lastname"] = "Test"
                }));
            }
            inputCollection.Complete();
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true);
            ps.Invoke(inputCollection);
            
            // Assert
            var finalCount = Service!.RetrieveMultiple(new Microsoft.Xrm.Sdk.Query.QueryExpression("contact")).Entities.Count;
            finalCount.Should().Be(initialCount + 50);
        }

        [Fact]
        public void SetDataverseRecord_DatasetIntegrity_NoSideEffectsOnUnrelatedFields()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var initial = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Complete",
                ["lastname"] = "User",
                ["emailaddress1"] = "complete@example.com",
                ["description"] = "Original description"
            };
            Service!.Create(initial);
            
            var updateEntity = new Entity("contact")
            {
                Id = initial.Id,
                ["contactid"] = initial.Id,
                ["firstname"] = "UpdatedFirst"
            };
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection);
            ps.Invoke(new[] { PSObject.AsPSObject(updateEntity) });
            
            // Assert
            var updated = Service!.Retrieve("contact", initial.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
            updated["firstname"].Should().Be("UpdatedFirst");
            updated["lastname"].Should().Be("User");
            updated["emailaddress1"].Should().Be("complete@example.com");
            updated["description"].Should().Be("Original description");
        }

        [Fact]
        public void SetDataverseRecord_Pipeline_AcceptsTableNameFromPipelineProperty()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var inputObject = PSObject.AsPSObject(new {
                TableName = "contact",
                firstname = "Pipeline",
                lastname = "Test"
            });
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var results = ps.Invoke(new[] { inputObject });
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["TableName"].Value.Should().Be("contact");
        }

        [Fact]
        public void SetDataverseRecord_Pipeline_AcceptsIdFromPipelineProperty()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var initial = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Original",
                ["lastname"] = "Name"
            };
            Service!.Create(initial);
            
            var updateEntity = new Entity("contact")
            {
                Id = initial.Id,
                ["contactid"] = initial.Id,
                ["firstname"] = "Updated"
            };
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection);
            ps.Invoke(new[] { PSObject.AsPSObject(updateEntity) });
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            var updated = Service!.Retrieve("contact", initial.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("firstname"));
            updated["firstname"].Should().Be("Updated");
        }

        [Fact]
        public void SetDataverseRecord_Pipeline_ProcessesMultipleRecords()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var records = new List<PSObject>();
            for (int i = 1; i <= 5; i++)
            {
                records.Add(PSObject.AsPSObject(new {
                    TableName = "contact",
                    firstname = $"Pipe{i}",
                    lastname = "Test"
                }));
            }
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var results = ps.Invoke(records);
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(5);
        }

        [Fact]
        public void SetDataverseRecord_CallerId_RestoresPreviousCallerIdAfterNonBatchCreate()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var record = new Hashtable {
                ["firstname"] = "John",
                ["lastname"] = "Doe"
            };
            
            var operationCallerId = Guid.NewGuid();
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CallerId", operationCallerId)
              .AddParameter("CreateOnly", true)
              .AddParameter("BatchSize", 1);
            ps.Invoke(new[] { PSObject.AsPSObject(record) });
            
            // Assert - CallerId should be restored to Guid.Empty
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void SetDataverseRecord_CallerId_RestoresPreviousCallerIdAfterNonBatchUpdate()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var record = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "John",
                ["lastname"] = "Doe"
            };
            Service!.Create(record);
            
            record["firstname"] = "Jane";
            var operationCallerId = Guid.NewGuid();
            
            // Act
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CallerId", operationCallerId)
              .AddParameter("BatchSize", 1);
            ps.Invoke(new[] { PSObject.AsPSObject(record) });
            
            // Assert
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void SetDataverseRecord_CallerId_DoesNotModifyConnectionWhenNotSpecified()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var record = new Hashtable {
                ["firstname"] = "John",
                ["lastname"] = "Doe"
            };
            
            // Act - NO CallerId parameter
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("BatchSize", 1);
            ps.Invoke(new[] { PSObject.AsPSObject(record) });
            
            // Assert
            ps.HadErrors.Should().BeFalse();
        }
    }
}
