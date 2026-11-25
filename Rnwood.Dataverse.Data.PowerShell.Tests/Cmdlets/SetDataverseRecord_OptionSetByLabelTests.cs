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
    /// OptionSet By Label tests for Set-DataverseRecord cmdlet.
    /// Migrated from Pester tests in tests/Set-DataverseRecord-OptionSetByLabel.Tests.ps1
    /// </summary>
    public class SetDataverseRecord_OptionSetByLabelTests : TestBase
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
        public void SetDataverseRecord_OptionSet_CreateWithNumericValue()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var record = new Hashtable {
                ["firstname"] = "Test",
                ["lastname"] = "User",
                ["accountrolecode"] = 1
            };
            
            // Act
            var inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(record));
            inputCollection.Complete();
            
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var results = ps.Invoke(inputCollection);

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            var createdId = (Guid)results[0].Properties["Id"].Value;
            
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", createdId)
              .AddParameter("Columns", new[] { "accountrolecode" });
            var retrieved = ps.Invoke();
            
            retrieved[0].Properties["accountrolecode"].Value.Should().Be(1);
        }

        [Fact]
        public void SetDataverseRecord_OptionSet_UpdateWithNumericValue()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create initial record
            var record = new Hashtable {
                ["firstname"] = "Test",
                ["lastname"] = "User",
                ["accountrolecode"] = 1
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
            
            // Act - Update with different numeric value
            var updateRecord = new Hashtable {
                ["contactid"] = contactId,
                ["accountrolecode"] = 2
            };
            inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(updateRecord));
            inputCollection.Complete();
            
            ps.Commands.Clear();
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact");
            ps.Invoke(inputCollection);

            // Assert
            ps.HadErrors.Should().BeFalse();
            
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", contactId)
              .AddParameter("Columns", new[] { "accountrolecode" });
            var retrieved = ps.Invoke();
            
            retrieved[0].Properties["accountrolecode"].Value.Should().Be(2);
        }

        [Fact]
        public void SetDataverseRecord_OptionSet_ClearWithNull()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create record with OptionSet value
            var record = new Hashtable {
                ["firstname"] = "Test",
                ["lastname"] = "User",
                ["accountrolecode"] = 1
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
            
            // Act - Set OptionSet to null
            var updateRecord = new Hashtable {
                ["contactid"] = contactId,
                ["accountrolecode"] = null
            };
            inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(updateRecord));
            inputCollection.Complete();
            
            ps.Commands.Clear();
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact");
            ps.Invoke(inputCollection);

            // Assert
            ps.HadErrors.Should().BeFalse();
            
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", contactId)
              .AddParameter("Columns", new[] { "accountrolecode" });
            var retrieved = ps.Invoke();
            
            var accountRoleCode = retrieved[0].Properties.Cast<PSPropertyInfo>()
                .FirstOrDefault(p => p.Name == "accountrolecode")?.Value;
            accountRoleCode.Should().BeNull();
        }

        [Fact]
        public void SetDataverseRecord_OptionSet_MultipleFieldsInOneUpdate()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create record with multiple OptionSet fields
            var record = new Hashtable {
                ["firstname"] = "Multi",
                ["lastname"] = "Option",
                ["accountrolecode"] = 1
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
            
            // Act - Update multiple OptionSet fields
            var updateRecord = new Hashtable {
                ["contactid"] = contactId,
                ["accountrolecode"] = 2,
                ["firstname"] = "Updated"
            };
            inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(updateRecord));
            inputCollection.Complete();
            
            ps.Commands.Clear();
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact");
            ps.Invoke(inputCollection);

            // Assert
            ps.HadErrors.Should().BeFalse();
            
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", contactId)
              .AddParameter("Columns", new[] { "accountrolecode", "firstname", "lastname" });
            var retrieved = ps.Invoke();
            
            retrieved[0].Properties["accountrolecode"].Value.Should().Be(2);
            retrieved[0].Properties["firstname"].Value.Should().Be("Updated");
            retrieved[0].Properties["lastname"].Value.Should().Be("Option");
        }

        [Fact]
        public void SetDataverseRecord_OptionSet_BatchCreate()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var records = new[] {
                new Hashtable { ["firstname"] = "User1", ["lastname"] = "Test", ["accountrolecode"] = 1 },
                new Hashtable { ["firstname"] = "User2", ["lastname"] = "Test", ["accountrolecode"] = 2 },
                new Hashtable { ["firstname"] = "User3", ["lastname"] = "Test", ["accountrolecode"] = 1 }
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
              .AddParameter("PassThru", true);
            var results = ps.Invoke(inputCollection);

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(3);
            
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "accountrolecode" });
            var allContacts = ps.Invoke();
            
            allContacts.Should().HaveCount(3);
            allContacts.Where(c => (int)c.Properties["accountrolecode"].Value == 1).Should().HaveCount(2);
            allContacts.Where(c => (int)c.Properties["accountrolecode"].Value == 2).Should().HaveCount(1);
        }

        [Fact]
        public void SetDataverseRecord_Boolean_CreateAndUpdate()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Act - Create record with boolean field
            var record = new Hashtable {
                ["firstname"] = "Bool",
                ["lastname"] = "Test",
                ["donotbulkemail"] = true
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

            // Assert - Verify creation
            ps.HadErrors.Should().BeFalse();
            
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", contactId)
              .AddParameter("Columns", new[] { "donotbulkemail" });
            var retrieved = ps.Invoke();
            
            retrieved[0].Properties["donotbulkemail"].Value.Should().Be(true);
            
            // Act - Update boolean field
            var updateRecord = new Hashtable {
                ["contactid"] = contactId,
                ["donotbulkemail"] = false
            };
            inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(updateRecord));
            inputCollection.Complete();
            
            ps.Commands.Clear();
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact");
            ps.Invoke(inputCollection);
            
            // Assert - Verify update
            ps.HadErrors.Should().BeFalse();
            
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", contactId)
              .AddParameter("Columns", new[] { "donotbulkemail" });
            var updated = ps.Invoke();
            
            updated[0].Properties["donotbulkemail"].Value.Should().Be(false);
        }
    }
}
