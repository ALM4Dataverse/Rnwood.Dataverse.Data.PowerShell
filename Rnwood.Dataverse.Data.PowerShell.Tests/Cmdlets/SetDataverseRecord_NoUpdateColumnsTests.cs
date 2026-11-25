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
    /// NoUpdateColumns tests for Set-DataverseRecord cmdlet.
    /// Migrated from Pester tests in tests/Set-DataverseRecord-NoUpdateColumns.Tests.ps1
    /// </summary>
    public class SetDataverseRecord_NoUpdateColumnsTests : TestBase
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
        public void SetDataverseRecord_NoUpdateColumns_ExcludesSpecifiedColumn()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create initial record
            var record = new Hashtable {
                ["firstname"] = "Original",
                ["lastname"] = "User",
                ["emailaddress1"] = "original@example.com",
                ["description"] = "Original description"
            };
            var inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(record));
            inputCollection.Complete();
            
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var createResults = ps.Invoke(inputCollection);
            var contactId = (Guid)createResults[0].Properties["Id"].Value;
            
            // Act - Update with NoUpdateColumns
            var updateData = new Hashtable {
                ["contactid"] = contactId,
                ["firstname"] = "Updated",
                ["emailaddress1"] = "updated@example.com",
                ["description"] = "This should be ignored"
            };
            inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(updateData));
            inputCollection.Complete();
            
            ps.Commands.Clear();
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("NoUpdateColumns", new[] { "description" });
            ps.Invoke(inputCollection);

            // Assert
            ps.HadErrors.Should().BeFalse();
            
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", contactId)
              .AddParameter("Columns", new[] { "firstname", "lastname", "emailaddress1", "description" });
            var retrieved = ps.Invoke();
            
            retrieved[0].Properties["firstname"].Value.Should().Be("Updated");
            retrieved[0].Properties["emailaddress1"].Value.Should().Be("updated@example.com");
            retrieved[0].Properties["description"].Value.Should().Be("Original description");
            retrieved[0].Properties["lastname"].Value.Should().Be("User");
        }

        [Fact]
        public void SetDataverseRecord_NoUpdateColumns_ExcludesMultipleColumns()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create initial record
            var record = new Hashtable {
                ["firstname"] = "John",
                ["lastname"] = "Doe",
                ["emailaddress1"] = "john@example.com",
                ["mobilephone"] = "555-1234",
                ["telephone1"] = "555-5678"
            };
            var inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(record));
            inputCollection.Complete();
            
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var createResults = ps.Invoke(inputCollection);
            var contactId = (Guid)createResults[0].Properties["Id"].Value;
            
            // Act - Update with multiple NoUpdateColumns
            var updateData = new Hashtable {
                ["contactid"] = contactId,
                ["firstname"] = "Jane",
                ["lastname"] = "Smith",
                ["emailaddress1"] = "jane@example.com",
                ["mobilephone"] = "999-0000",
                ["telephone1"] = "888-0000"
            };
            inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(updateData));
            inputCollection.Complete();
            
            ps.Commands.Clear();
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("NoUpdateColumns", new[] { "emailaddress1", "mobilephone", "telephone1" });
            ps.Invoke(inputCollection);

            // Assert
            ps.HadErrors.Should().BeFalse();
            
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", contactId)
              .AddParameter("Columns", new[] { "firstname", "lastname", "emailaddress1", "mobilephone", "telephone1" });
            var retrieved = ps.Invoke();
            
            retrieved[0].Properties["firstname"].Value.Should().Be("Jane");
            retrieved[0].Properties["lastname"].Value.Should().Be("Smith");
            retrieved[0].Properties["emailaddress1"].Value.Should().Be("john@example.com");
            retrieved[0].Properties["mobilephone"].Value.Should().Be("555-1234");
            retrieved[0].Properties["telephone1"].Value.Should().Be("555-5678");
        }

        [Fact]
        public void SetDataverseRecord_NoUpdateColumns_BatchUpdates()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create initial records
            var records = new[] {
                new Hashtable { ["firstname"] = "User1", ["lastname"] = "Test", ["emailaddress1"] = "user1@example.com" },
                new Hashtable { ["firstname"] = "User2", ["lastname"] = "Test", ["emailaddress1"] = "user2@example.com" }
            };
            var inputCollection = new PSDataCollection<PSObject>();
            foreach (var record in records)
                inputCollection.Add(PSObject.AsPSObject(record));
            inputCollection.Complete();
            
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var createResults = ps.Invoke(inputCollection);
            var ids = createResults.Select(r => (Guid)r.Properties["Id"].Value).ToArray();
            
            // Act - Batch update with NoUpdateColumns
            var updates = new[] {
                new Hashtable { ["contactid"] = ids[0], ["firstname"] = "UpdatedUser1", ["emailaddress1"] = "shouldnotchange1@example.com" },
                new Hashtable { ["contactid"] = ids[1], ["firstname"] = "UpdatedUser2", ["emailaddress1"] = "shouldnotchange2@example.com" }
            };
            inputCollection = new PSDataCollection<PSObject>();
            foreach (var update in updates)
                inputCollection.Add(PSObject.AsPSObject(update));
            inputCollection.Complete();
            
            ps.Commands.Clear();
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("NoUpdateColumns", new[] { "emailaddress1" });
            ps.Invoke(inputCollection);

            // Assert
            ps.HadErrors.Should().BeFalse();
            
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "contactid", "firstname", "emailaddress1" });
            var allContacts = ps.Invoke();
            
            allContacts.Should().HaveCount(2);
            
            var user1 = allContacts.First(c => (Guid)c.Properties["Id"].Value == ids[0]);
            user1.Properties["firstname"].Value.Should().Be("UpdatedUser1");
            user1.Properties["emailaddress1"].Value.Should().Be("user1@example.com");
            
            var user2 = allContacts.First(c => (Guid)c.Properties["Id"].Value == ids[1]);
            user2.Properties["firstname"].Value.Should().Be("UpdatedUser2");
            user2.Properties["emailaddress1"].Value.Should().Be("user2@example.com");
        }

        [Fact]
        public void SetDataverseRecord_NoUpdateColumns_NoEffectOnCreate()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Act - Create with NoUpdateColumns (should be ignored)
            var newRecord = new Hashtable {
                ["firstname"] = "New",
                ["lastname"] = "User",
                ["emailaddress1"] = "new@example.com"
            };
            var inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(newRecord));
            inputCollection.Complete();
            
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("NoUpdateColumns", new[] { "emailaddress1" })
              .AddParameter("PassThru", true);
            var results = ps.Invoke(inputCollection);

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            var createdId = (Guid)results[0].Properties["Id"].Value;
            
            // Verify all fields were set despite NoUpdateColumns
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", createdId)
              .AddParameter("Columns", new[] { "firstname", "lastname", "emailaddress1" });
            var retrieved = ps.Invoke();
            
            retrieved[0].Properties["firstname"].Value.Should().Be("New");
            retrieved[0].Properties["lastname"].Value.Should().Be("User");
            retrieved[0].Properties["emailaddress1"].Value.Should().Be("new@example.com");
        }

        [Fact]
        public void SetDataverseRecord_NoUpdateColumns_WhenColumnNotInInput()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create initial record
            var record = new Hashtable {
                ["firstname"] = "Test",
                ["lastname"] = "User",
                ["emailaddress1"] = "test@example.com"
            };
            var inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(record));
            inputCollection.Complete();
            
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var createResults = ps.Invoke(inputCollection);
            var contactId = (Guid)createResults[0].Properties["Id"].Value;
            
            // Act - Update with NoUpdateColumns for column not in input
            var updateData = new Hashtable {
                ["contactid"] = contactId,
                ["firstname"] = "Updated"
            };
            inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(updateData));
            inputCollection.Complete();
            
            ps.Commands.Clear();
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("NoUpdateColumns", new[] { "emailaddress1", "description" });
            ps.Invoke(inputCollection);

            // Assert - Should not error
            ps.HadErrors.Should().BeFalse();
            
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", contactId)
              .AddParameter("Columns", new[] { "firstname", "emailaddress1" });
            var retrieved = ps.Invoke();
            
            retrieved[0].Properties["firstname"].Value.Should().Be("Updated");
            retrieved[0].Properties["emailaddress1"].Value.Should().Be("test@example.com");
        }
    }
}
