using System;
using System.Collections;
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
    /// Id, Name, and ExcludeId parameter tests for Get-DataverseRecord cmdlet.
    /// Tests for querying by Id/Name parameters and excluding records with ExcludeId.
    /// </summary>
    public class GetDataverseRecord_IdNameExcludeTests : TestBase
    {
        private PS CreatePowerShellWithCmdlets()
        {
            var initialSessionState = InitialSessionState.CreateDefault2();
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

        [Fact]
        public void GetDataverseRecord_Id_MultipleIds()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var contact1 = new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "User1", ["lastname"] = "Test1" };
            var contact2 = new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "User2", ["lastname"] = "Test2" };
            var contact3 = new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "User3", ["lastname"] = "Test3" };
            var contact4 = new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "User4", ["lastname"] = "Test4" };
            
            Service!.Create(contact1);
            Service.Create(contact2);
            Service.Create(contact3);
            Service.Create(contact4);
            
            // Act - Get specific records by Id
            var targetIds = new[] { contact1.Id, contact3.Id };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", targetIds)
              .AddParameter("Columns", new[] { "firstname", "lastname" });
            
            var results = ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(2);
            var resultIds = results.Select(r => (Guid)r.Properties["Id"].Value).ToList();
            resultIds.Should().Contain(contact1.Id);
            resultIds.Should().Contain(contact3.Id);
        }

        [Fact]
        public void GetDataverseRecord_Id_SingleId()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var contact = new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "Single", ["lastname"] = "Record" };
            Service!.Create(contact);
            
            // Act
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", contact.Id)
              .AddParameter("Columns", new[] { "firstname", "lastname" });
            
            var results = ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["Id"].Value.Should().Be(contact.Id);
            results[0].Properties["firstname"].Value.Should().Be("Single");
            results[0].Properties["lastname"].Value.Should().Be("Record");
        }

        [Fact]
        public void GetDataverseRecord_Id_NotFound_ReturnsEmpty()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create one record
            Service!.Create(new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "Existing", ["lastname"] = "Record" });
            
            // Act - Try to get non-existent record
            var nonExistentId = Guid.NewGuid();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", nonExistentId)
              .AddParameter("Columns", new[] { "contactid" });
            
            var results = ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().BeEmpty();
        }

        [Fact]
        public void GetDataverseRecord_Name_MultipleNames()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "John", ["lastname"] = "Doe", ["fullname"] = "John Doe" });
            Service.Create(new Entity("contact") { ["firstname"] = "Jane", ["lastname"] = "Smith", ["fullname"] = "Jane Smith" });
            Service.Create(new Entity("contact") { ["firstname"] = "Bob", ["lastname"] = "Johnson", ["fullname"] = "Bob Johnson" });
            
            // Act - Get records by name (fullname is auto-generated from firstname + lastname)
            var targetNames = new[] { "John Doe", "Bob Johnson" };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Name", targetNames)
              .AddParameter("Columns", new[] { "firstname", "lastname" });
            
            var results = ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(2);
        }

        [Fact]
        public void GetDataverseRecord_Name_SingleName()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "Unique", ["lastname"] = "Person", ["fullname"] = "Unique Person" });
            
            // Act
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Name", "Unique Person")
              .AddParameter("Columns", new[] { "firstname", "lastname" });
            
            var results = ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["firstname"].Value.Should().Be("Unique");
            results[0].Properties["lastname"].Value.Should().Be("Person");
        }

        [Fact]
        public void GetDataverseRecord_Name_NotFound_ReturnsEmpty()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "Existing", ["lastname"] = "User" });
            
            // Act - Try to get non-existent name
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Name", "NonExistent Name")
              .AddParameter("Columns", new[] { "contactid" });
            
            var results = ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().BeEmpty();
        }

        [Fact]
        public void GetDataverseRecord_ExcludeId_MultipleIds()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var keep1 = new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "Keep1", ["lastname"] = "Test" };
            var exclude1 = new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "Exclude1", ["lastname"] = "Test" };
            var keep2 = new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "Keep2", ["lastname"] = "Test" };
            var exclude2 = new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "Exclude2", ["lastname"] = "Test" };
            
            Service!.Create(keep1);
            Service.Create(exclude1);
            Service.Create(keep2);
            Service.Create(exclude2);
            
            // Act - Exclude specific records
            var excludeIds = new[] { exclude1.Id, exclude2.Id };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("ExcludeId", excludeIds)
              .AddParameter("Columns", new[] { "firstname", "lastname" });
            
            var results = ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(2);
            var firstnames = results.Select(r => r.Properties["firstname"].Value.ToString()).ToList();
            firstnames.Should().Contain("Keep1");
            firstnames.Should().Contain("Keep2");
            firstnames.Should().NotContain("Exclude1");
            firstnames.Should().NotContain("Exclude2");
        }

        [Fact]
        public void GetDataverseRecord_ExcludeId_WithFilter()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var alice = new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "Alice", ["lastname"] = "MatchFilter" };
            var bob = new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "Bob", ["lastname"] = "MatchFilter" };
            var charlie = new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "Charlie", ["lastname"] = "MatchFilter" };
            var david = new Entity("contact") { Id = Guid.NewGuid(), ["firstname"] = "David", ["lastname"] = "NoMatch" };
            
            Service!.Create(alice);
            Service.Create(bob);
            Service.Create(charlie);
            Service.Create(david);
            
            // Act - Get records with filter but exclude Bob
            var excludeId = bob.Id;
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("FilterValues", new Hashtable { ["lastname"] = "MatchFilter" })
              .AddParameter("ExcludeId", excludeId);
            
            var results = ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(2);
            var firstnames = results.Select(r => r.Properties["firstname"].Value.ToString()).ToList();
            firstnames.Should().Contain("Alice");
            firstnames.Should().Contain("Charlie");
            firstnames.Should().NotContain("Bob");
        }

        [Fact]
        public void GetDataverseRecord_ExcludeId_EmptyArray_ReturnsAll()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "User1", ["lastname"] = "Test" });
            Service.Create(new Entity("contact") { ["firstname"] = "User2", ["lastname"] = "Test" });
            
            // Act - ExcludeId with empty array
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("ExcludeId", new Guid[0])
              .AddParameter("Columns", new[] { "firstname", "lastname" });
            
            var results = ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(2);
        }
    }
}
