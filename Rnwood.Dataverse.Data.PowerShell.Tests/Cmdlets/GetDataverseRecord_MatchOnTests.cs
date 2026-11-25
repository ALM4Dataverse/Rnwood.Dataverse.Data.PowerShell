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
    /// MatchOn tests for Get-DataverseRecord cmdlet.
    /// Tests for MatchOn parameter functionality with pipeline input.
    /// </summary>
    public class GetDataverseRecord_MatchOnTests : TestBase
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
        public void GetDataverseRecord_MatchOn_SingleColumn()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "John", ["lastname"] = "Doe", ["emailaddress1"] = "john@test.com" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Jane", ["lastname"] = "Smith", ["emailaddress1"] = "jane@test.com" });
            
            // Act - Use MatchOn with pipeline input
            var inputRecord = new Hashtable { ["emailaddress1"] = "john@test.com" };
            var inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(inputRecord));
            inputCollection.Complete();
            
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("MatchOn", "emailaddress1")
              .AddParameter("Columns", new[] { "firstname", "emailaddress1" });
            var results = ps.Invoke(inputCollection);

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["firstname"].Value.Should().Be("John");
            results[0].Properties["emailaddress1"].Value.Should().Be("john@test.com");
        }

        [Fact]
        public void GetDataverseRecord_MatchOn_MultipleColumns()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "Alice", ["lastname"] = "Brown", ["emailaddress1"] = "alice.brown@test.com" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Bob", ["lastname"] = "Green", ["emailaddress1"] = "bob.green@test.com" });
            
            // Act
            var inputRecord = new Hashtable { ["firstname"] = "Alice", ["lastname"] = "Brown" };
            var inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(inputRecord));
            inputCollection.Complete();
            
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("MatchOn", new[] { "firstname", "lastname" })
              .AddParameter("Columns", new[] { "firstname", "lastname", "emailaddress1" });
            var results = ps.Invoke(inputCollection);

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["firstname"].Value.Should().Be("Alice");
            results[0].Properties["lastname"].Value.Should().Be("Brown");
        }

        [Fact]
        public void GetDataverseRecord_MatchOn_MultipleMatches_ThrowsError()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "John1", ["lastname"] = "Doe", ["emailaddress1"] = "test@test.com" });
            Service!.Create(new Entity("contact") { ["firstname"] = "John2", ["lastname"] = "Doe", ["emailaddress1"] = "test@test.com" });
            
            // Act
            var inputRecord = new Hashtable { ["emailaddress1"] = "test@test.com" };
            var inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(inputRecord));
            inputCollection.Complete();
            
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("MatchOn", "emailaddress1")
              .AddParameter("Columns", new[] { "emailaddress1" });
            var results = ps.Invoke(inputCollection);

            // Assert - Should have error about multiple matches
            ps.HadErrors.Should().BeTrue();
            ps.Streams.Error.Should().HaveCountGreaterThan(0);
            ps.Streams.Error[0].Exception.Message.Should().Contain("AllowMultipleMatches");
        }

        [Fact]
        public void GetDataverseRecord_MatchOn_AllowMultipleMatches()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "John", ["lastname"] = "TestUser", ["emailaddress1"] = "john@test.com" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Jane", ["lastname"] = "TestUser", ["emailaddress1"] = "jane@test.com" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Bob", ["lastname"] = "Different", ["emailaddress1"] = "bob@test.com" });
            
            // Act
            var inputRecord = new Hashtable { ["lastname"] = "TestUser" };
            var inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(inputRecord));
            inputCollection.Complete();
            
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("MatchOn", "lastname")
              .AddParameter("AllowMultipleMatches", true)
              .AddParameter("Columns", new[] { "firstname", "lastname", "emailaddress1" });
            var results = ps.Invoke(inputCollection);

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(2);
            results.All(r => r.Properties["lastname"].Value.Equals("TestUser")).Should().BeTrue();
        }

        [Fact]
        public void GetDataverseRecord_MatchOn_NoMatches_ReturnsEmpty()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Act - No records created, so MatchOn will find nothing
            var inputRecord = new Hashtable { ["emailaddress1"] = "nonexistent@test.com" };
            var inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(inputRecord));
            inputCollection.Complete();
            
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("MatchOn", "emailaddress1")
              .AddParameter("Columns", new[] { "contactid" });
            var results = ps.Invoke(inputCollection);

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().BeEmpty();
        }
    }
}
