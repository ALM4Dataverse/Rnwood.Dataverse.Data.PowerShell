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
    /// Lookup By Name tests for Set-DataverseRecord cmdlet.
    /// Migrated from Pester tests in tests/Set-DataverseRecord-LookupByName.Tests.ps1
    /// </summary>
    public class SetDataverseRecord_LookupByNameTests : TestBase
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
        public void SetDataverseRecord_Lookup_SetByGuid()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create parent contact
            var parentRecord = new Hashtable {
                ["firstname"] = "Parent",
                ["lastname"] = "Contact"
            };
            var inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(parentRecord));
            inputCollection.Complete();
            
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var parentResults = ps.Invoke(inputCollection);
            var parentId = (Guid)parentResults[0].Properties["Id"].Value;
            
            // Act - Create child with lookup to parent using GUID
            var childRecord = new Hashtable {
                ["firstname"] = "Child",
                ["lastname"] = "Contact",
                ["parentcontactid"] = parentId
            };
            inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(childRecord));
            inputCollection.Complete();
            
            ps.Commands.Clear();
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var childResults = ps.Invoke(inputCollection);

            // Assert
            ps.HadErrors.Should().BeFalse();
            childResults.Should().HaveCount(1);
            var childId = (Guid)childResults[0].Properties["Id"].Value;
            
            // Verify child was created
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", childId)
              .AddParameter("Columns", new[] { "firstname", "lastname", "parentcontactid" });
            var retrieved = ps.Invoke();
            
            retrieved[0].Properties["firstname"].Value.Should().Be("Child");
            retrieved[0].Properties["parentcontactid"].Value.Should().NotBeNull();
        }

        [Fact]
        public void SetDataverseRecord_Lookup_UpdateToNewTarget()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create parent contacts
            var parent1 = new Hashtable { ["firstname"] = "Parent1", ["lastname"] = "Original" };
            var inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(parent1));
            inputCollection.Complete();
            
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var parent1Results = ps.Invoke(inputCollection);
            var parent1Id = (Guid)parent1Results[0].Properties["Id"].Value;
            
            var parent2 = new Hashtable { ["firstname"] = "Parent2", ["lastname"] = "New" };
            inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(parent2));
            inputCollection.Complete();
            
            ps.Commands.Clear();
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var parent2Results = ps.Invoke(inputCollection);
            var parent2Id = (Guid)parent2Results[0].Properties["Id"].Value;
            
            // Create child with lookup to parent1
            var child = new Hashtable {
                ["firstname"] = "Child",
                ["lastname"] = "Update",
                ["parentcontactid"] = parent1Id
            };
            inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(child));
            inputCollection.Complete();
            
            ps.Commands.Clear();
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var childResults = ps.Invoke(inputCollection);
            var childId = (Guid)childResults[0].Properties["Id"].Value;
            
            // Act - Update lookup to parent2
            var updateRecord = new Hashtable {
                ["contactid"] = childId,
                ["parentcontactid"] = parent2Id
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
            
            // Verify child record updated
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", childId)
              .AddParameter("Columns", new[] { "firstname", "lastname" });
            var retrieved = ps.Invoke();
            
            retrieved[0].Properties["firstname"].Value.Should().Be("Child");
            retrieved[0].Properties["lastname"].Value.Should().Be("Update");
            
            // Verify all 3 records exist
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "contactid" });
            var allContacts = ps.Invoke();
            
            allContacts.Should().HaveCount(3);
        }

        [Fact]
        public void SetDataverseRecord_Lookup_ClearByNull()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create parent and child
            var parent = new Hashtable { ["firstname"] = "Parent", ["lastname"] = "Clear" };
            var inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(parent));
            inputCollection.Complete();
            
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var parentResults = ps.Invoke(inputCollection);
            var parentId = (Guid)parentResults[0].Properties["Id"].Value;
            
            var child = new Hashtable {
                ["firstname"] = "Child",
                ["lastname"] = "Clear",
                ["parentcontactid"] = parentId
            };
            inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(child));
            inputCollection.Complete();
            
            ps.Commands.Clear();
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var childResults = ps.Invoke(inputCollection);
            var childId = (Guid)childResults[0].Properties["Id"].Value;
            
            // Act - Clear the lookup
            var updateRecord = new Hashtable {
                ["contactid"] = childId,
                ["parentcontactid"] = null
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
            
            // Verify lookup was cleared
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", childId)
              .AddParameter("Columns", new[] { "firstname", "lastname" });
            var retrieved = ps.Invoke();
            
            retrieved[0].Properties["firstname"].Value.Should().Be("Child");
            retrieved[0].Properties["lastname"].Value.Should().Be("Clear");
            
            // Verify both records still exist
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "contactid" });
            var allContacts = ps.Invoke();
            
            allContacts.Should().HaveCount(2);
        }

        [Fact]
        public void SetDataverseRecord_Lookup_MultipleLookupFields()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create target contacts
            var parent = new Hashtable { ["firstname"] = "Parent", ["lastname"] = "Multi" };
            var inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(parent));
            inputCollection.Complete();
            
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var parentResults = ps.Invoke(inputCollection);
            var parentId = (Guid)parentResults[0].Properties["Id"].Value;
            
            var master = new Hashtable { ["firstname"] = "Master", ["lastname"] = "Multi" };
            inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(master));
            inputCollection.Complete();
            
            ps.Commands.Clear();
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var masterResults = ps.Invoke(inputCollection);
            var masterId = (Guid)masterResults[0].Properties["Id"].Value;
            
            // Act - Create contact with multiple lookup fields
            var contact = new Hashtable {
                ["firstname"] = "Multi",
                ["lastname"] = "Lookup",
                ["parentcontactid"] = parentId,
                ["masterid"] = masterId
            };
            inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(contact));
            inputCollection.Complete();
            
            ps.Commands.Clear();
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var results = ps.Invoke(inputCollection);

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            var contactId = (Guid)results[0].Properties["Id"].Value;
            
            // Verify contact was created
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", contactId)
              .AddParameter("Columns", new[] { "firstname" });
            var retrieved = ps.Invoke();
            
            retrieved[0].Properties["firstname"].Value.Should().Be("Multi");
            
            // Verify all 3 records exist
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "contactid" });
            var allContacts = ps.Invoke();
            
            allContacts.Should().HaveCount(3);
        }
    }
}
