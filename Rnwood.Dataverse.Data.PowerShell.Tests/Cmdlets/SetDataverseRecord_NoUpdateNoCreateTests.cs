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
    /// NoUpdate/NoCreate flags tests for Set-DataverseRecord cmdlet.
    /// Migrated from Pester tests in tests/Set-DataverseRecord-NoUpdateNoCreate.Tests.ps1
    /// </summary>
    public class SetDataverseRecord_NoUpdateNoCreateTests : TestBase
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
        public void SetDataverseRecord_NoUpdate_DoesNotUpdateExisting()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create initial record
            var existing = new Hashtable {
                ["firstname"] = "Original",
                ["lastname"] = "User",
                ["emailaddress1"] = "original@example.com"
            };
            var inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(existing));
            inputCollection.Complete();
            
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var createResults = ps.Invoke(inputCollection);
            var contactId = (Guid)createResults[0].Properties["Id"].Value;
            
            // Act - Try to update with NoUpdate
            var updateAttempt = new Hashtable {
                ["contactid"] = contactId,
                ["firstname"] = "Updated",
                ["emailaddress1"] = "updated@example.com"
            };
            inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(updateAttempt));
            inputCollection.Complete();
            
            ps.Commands.Clear();
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("NoUpdate", true);
            ps.Invoke(inputCollection);

            // Assert - Record should NOT be updated
            ps.HadErrors.Should().BeFalse();
            
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", contactId)
              .AddParameter("Columns", new[] { "emailaddress1", "firstname" });
            var retrieved = ps.Invoke();
            
            retrieved[0].Properties["firstname"].Value.Should().Be("Original");
            retrieved[0].Properties["emailaddress1"].Value.Should().Be("original@example.com");
        }

        [Fact]
        public void SetDataverseRecord_NoUpdate_CreatesNewRecords()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Act - Create new record with NoUpdate
            var newRecord = new Hashtable {
                ["firstname"] = "New",
                ["lastname"] = "Record",
                ["emailaddress1"] = "new@example.com"
            };
            var inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(newRecord));
            inputCollection.Complete();
            
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("NoUpdate", true)
              .AddParameter("PassThru", true);
            var results = ps.Invoke(inputCollection);

            // Assert - Record should be created
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            var createdId = (Guid)results[0].Properties["Id"].Value;
            
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", createdId)
              .AddParameter("Columns", new[] { "emailaddress1", "firstname", "lastname" });
            var retrieved = ps.Invoke();
            
            retrieved[0].Properties["firstname"].Value.Should().Be("New");
            retrieved[0].Properties["lastname"].Value.Should().Be("Record");
            retrieved[0].Properties["emailaddress1"].Value.Should().Be("new@example.com");
        }

        [Fact]
        public void SetDataverseRecord_NoCreate_UpdatesExisting()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create initial record
            var existing = new Hashtable {
                ["firstname"] = "Original",
                ["lastname"] = "User",
                ["emailaddress1"] = "test@example.com"
            };
            var inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(existing));
            inputCollection.Complete();
            
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var createResults = ps.Invoke(inputCollection);
            var contactId = (Guid)createResults[0].Properties["Id"].Value;
            
            // Act - Update with NoCreate
            var updateData = new Hashtable {
                ["contactid"] = contactId,
                ["firstname"] = "Updated",
                ["emailaddress1"] = "updated@example.com"
            };
            inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(updateData));
            inputCollection.Complete();
            
            ps.Commands.Clear();
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("NoCreate", true);
            ps.Invoke(inputCollection);

            // Assert - Record should be updated
            ps.HadErrors.Should().BeFalse();
            
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", contactId)
              .AddParameter("Columns", new[] { "emailaddress1", "firstname" });
            var retrieved = ps.Invoke();
            
            retrieved[0].Properties["firstname"].Value.Should().Be("Updated");
            retrieved[0].Properties["emailaddress1"].Value.Should().Be("updated@example.com");
        }

        [Fact]
        public void SetDataverseRecord_NoCreate_DoesNotCreateNew()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Act - Try to create with NoCreate
            var newRecord = new Hashtable {
                ["firstname"] = "New",
                ["lastname"] = "Record",
                ["emailaddress1"] = "new@example.com"
            };
            var inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(newRecord));
            inputCollection.Complete();
            
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("NoCreate", true);
            ps.Invoke(inputCollection);

            // Assert - No records should be created
            ps.HadErrors.Should().BeFalse();
            
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "contactid" });
            var allContacts = ps.Invoke();
            
            allContacts.Should().BeEmpty();
        }
    }
}
