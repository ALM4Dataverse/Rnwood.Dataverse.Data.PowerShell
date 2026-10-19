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
    /// Advanced Parameters tests for Set-DataverseRecord cmdlet.
    /// Migrated from Pester tests in tests/Set-DataverseRecord-AdvancedParameters.Tests.ps1
    /// </summary>
    public class SetDataverseRecord_AdvancedParametersTests : TestBase
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
        public void SetDataverseRecord_UpdateAllColumns_SkipsRetrieve()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create initial record
            var record = new Hashtable {
                ["firstname"] = "Original",
                ["lastname"] = "User",
                ["emailaddress1"] = "original@example.com"
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
            
            // Act - Update with UpdateAllColumns
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
              .AddParameter("UpdateAllColumns", true);
            ps.Invoke(inputCollection);

            // Assert
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
        public void SetDataverseRecord_IgnoreProperties_ExcludesFromOperation()
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
            
            // Act - Update with IgnoreProperties
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
              .AddParameter("IgnoreProperties", new[] { "description" });
            ps.Invoke(inputCollection);

            // Assert
            ps.HadErrors.Should().BeFalse();
            
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", contactId)
              .AddParameter("Columns", new[] { "description", "emailaddress1", "firstname" });
            var retrieved = ps.Invoke();
            
            retrieved[0].Properties["firstname"].Value.Should().Be("Updated");
            retrieved[0].Properties["emailaddress1"].Value.Should().Be("updated@example.com");
            retrieved[0].Properties["description"].Value.Should().Be("Original description");
        }

        [Fact]
        public void SetDataverseRecord_IgnoreProperties_MultipleProperties()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create initial record
            var record = new Hashtable {
                ["firstname"] = "Multi",
                ["lastname"] = "Ignore",
                ["emailaddress1"] = "multi@example.com"
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
            
            // Act - Update with multiple ignored properties
            var updateData = new Hashtable {
                ["contactid"] = contactId,
                ["firstname"] = "Should be ignored",
                ["lastname"] = "Updated",
                ["emailaddress1"] = "Should be ignored"
            };
            inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(updateData));
            inputCollection.Complete();
            
            ps.Commands.Clear();
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("IgnoreProperties", new[] { "firstname", "emailaddress1" });
            ps.Invoke(inputCollection);

            // Assert
            ps.HadErrors.Should().BeFalse();
            
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", contactId)
              .AddParameter("Columns", new[] { "emailaddress1", "firstname", "lastname" });
            var retrieved = ps.Invoke();
            
            retrieved[0].Properties["firstname"].Value.Should().Be("Multi");
            retrieved[0].Properties["lastname"].Value.Should().Be("Updated");
            retrieved[0].Properties["emailaddress1"].Value.Should().Be("multi@example.com");
        }

        [Fact]
        public void SetDataverseRecord_CallerId_CreateWithDelegation()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Act - Create record with CallerId (requires BatchSize 1 for FakeXrmEasy)
            var callerId = Guid.NewGuid();
            var record = new Hashtable {
                ["firstname"] = "Delegated",
                ["lastname"] = "Create"
            };
            var inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(record));
            inputCollection.Complete();
            
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("CallerId", callerId)
              .AddParameter("BatchSize", 1)
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
              .AddParameter("Columns", new[] { "firstname" });
            var retrieved = ps.Invoke();
            
            retrieved[0].Properties["firstname"].Value.Should().Be("Delegated");
        }

        [Fact]
        public void SetDataverseRecord_CallerId_UpdateWithDelegation()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create initial record
            var record = new Hashtable {
                ["firstname"] = "Original",
                ["lastname"] = "User"
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
            
            // Act - Update with CallerId
            var callerId = Guid.NewGuid();
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
              .AddParameter("CallerId", callerId)
              .AddParameter("BatchSize", 1);
            ps.Invoke(inputCollection);

            // Assert
            ps.HadErrors.Should().BeFalse();
            
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", contactId)
              .AddParameter("Columns", new[] { "firstname" });
            var retrieved = ps.Invoke();
            
            retrieved[0].Properties["firstname"].Value.Should().Be("Updated");
        }
    }
}
