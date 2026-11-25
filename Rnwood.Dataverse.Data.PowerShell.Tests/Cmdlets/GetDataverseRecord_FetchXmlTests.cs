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
    /// FetchXml tests for Get-DataverseRecord cmdlet.
    /// Tests for querying with FetchXml parameter.
    /// </summary>
    public class GetDataverseRecord_FetchXmlTests : TestBase
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
        public void GetDataverseRecord_FetchXml_BasicQueryWithFilter()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create test data
            Service!.Create(new Entity("contact") { ["firstname"] = "John", ["lastname"] = "Doe", ["emailaddress1"] = "john@example.com" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Jane", ["lastname"] = "Smith", ["emailaddress1"] = "jane@example.com" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Bob", ["lastname"] = "Doe", ["emailaddress1"] = "bob@example.com" });
            
            // Act - Execute FetchXml query
            var fetchXml = @"
<fetch>
  <entity name=""contact"">
    <attribute name=""firstname"" />
    <attribute name=""lastname"" />
    <attribute name=""emailaddress1"" />
    <filter type=""and"">
      <condition attribute=""lastname"" operator=""eq"" value=""Doe"" />
    </filter>
  </entity>
</fetch>";
            
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("FetchXml", fetchXml);
            
            var results = ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(2);
            results.Should().OnlyContain(r => r.Properties["lastname"].Value.ToString() == "Doe");
        }

        [Fact]
        public void GetDataverseRecord_FetchXml_WithOrdering()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create test data in random order
            Service!.Create(new Entity("contact") { ["firstname"] = "Charlie", ["lastname"] = "Test", ["donotbulkemail"] = true });
            Service!.Create(new Entity("contact") { ["firstname"] = "Alice", ["lastname"] = "Test", ["donotbulkemail"] = false });
            Service!.Create(new Entity("contact") { ["firstname"] = "Bob", ["lastname"] = "Test", ["donotbulkemail"] = true });
            
            // Act - Execute FetchXml with ordering
            var fetchXml = @"
<fetch>
  <entity name=""contact"">
    <attribute name=""firstname"" />
    <attribute name=""lastname"" />
    <order attribute=""firstname"" descending=""false"" />
    <filter type=""and"">
      <condition attribute=""lastname"" operator=""eq"" value=""Test"" />
    </filter>
  </entity>
</fetch>";
            
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("FetchXml", fetchXml);
            
            var results = ps.Invoke();
            
            // Assert - results should be sorted
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(3);
            results[0].Properties["firstname"].Value.Should().Be("Alice");
            results[1].Properties["firstname"].Value.Should().Be("Bob");
            results[2].Properties["firstname"].Value.Should().Be("Charlie");
        }

        [Fact]
        public void GetDataverseRecord_FetchXml_MultipleAndConditions()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create test data
            Service!.Create(new Entity("contact") { ["firstname"] = "Match", ["lastname"] = "Both", ["emailaddress1"] = "match@example.com" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Match", ["lastname"] = "Wrong", ["emailaddress1"] = "wrong@example.com" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Wrong", ["lastname"] = "Both", ["emailaddress1"] = "wrong2@example.com" });
            
            // Act - Execute FetchXml with AND conditions
            var fetchXml = @"
<fetch>
  <entity name=""contact"">
    <attribute name=""firstname"" />
    <attribute name=""lastname"" />
    <attribute name=""emailaddress1"" />
    <filter type=""and"">
      <condition attribute=""firstname"" operator=""eq"" value=""Match"" />
      <condition attribute=""lastname"" operator=""eq"" value=""Both"" />
    </filter>
  </entity>
</fetch>";
            
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("FetchXml", fetchXml);
            
            var results = ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["firstname"].Value.Should().Be("Match");
            results[0].Properties["lastname"].Value.Should().Be("Both");
            results[0].Properties["emailaddress1"].Value.Should().Be("match@example.com");
        }

        [Fact]
        public void GetDataverseRecord_FetchXml_LikeOperator()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create test data
            Service!.Create(new Entity("contact") { ["firstname"] = "TestUser1", ["lastname"] = "Smith" });
            Service!.Create(new Entity("contact") { ["firstname"] = "TestUser2", ["lastname"] = "Smith" });
            Service!.Create(new Entity("contact") { ["firstname"] = "OtherUser", ["lastname"] = "Smith" });
            
            // Act - Execute FetchXml with LIKE
            var fetchXml = @"
<fetch>
  <entity name=""contact"">
    <attribute name=""firstname"" />
    <attribute name=""lastname"" />
    <filter type=""and"">
      <condition attribute=""firstname"" operator=""like"" value=""TestUser%"" />
    </filter>
  </entity>
</fetch>";
            
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("FetchXml", fetchXml);
            
            var results = ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(2);
            results.Should().OnlyContain(r => r.Properties["firstname"].Value.ToString()!.StartsWith("TestUser"));
        }

        [Fact]
        public void GetDataverseRecord_FetchXml_TopClause()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create test data
            for (int i = 1; i <= 10; i++)
            {
                Service!.Create(new Entity("contact") { ["firstname"] = $"User{i}", ["lastname"] = "Test" });
            }
            
            // Act - Execute FetchXml with TOP
            var fetchXml = @"
<fetch top=""3"">
  <entity name=""contact"">
    <attribute name=""firstname"" />
    <attribute name=""lastname"" />
  </entity>
</fetch>";
            
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("FetchXml", fetchXml);
            
            var results = ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(3);
        }

        [Fact]
        public void GetDataverseRecord_FetchXml_OrConditions()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create test data
            Service!.Create(new Entity("contact") { ["firstname"] = "Alice", ["lastname"] = "Smith" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Bob", ["lastname"] = "Jones" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Charlie", ["lastname"] = "Brown" });
            
            // Act - Execute FetchXml with OR conditions
            var fetchXml = @"
<fetch>
  <entity name=""contact"">
    <attribute name=""firstname"" />
    <attribute name=""lastname"" />
    <filter type=""or"">
      <condition attribute=""lastname"" operator=""eq"" value=""Smith"" />
      <condition attribute=""lastname"" operator=""eq"" value=""Jones"" />
    </filter>
  </entity>
</fetch>";
            
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("FetchXml", fetchXml);
            
            var results = ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(2);
            var lastnames = results.Select(r => r.Properties["lastname"].Value.ToString()).ToList();
            lastnames.Should().Contain("Smith");
            lastnames.Should().Contain("Jones");
            lastnames.Should().NotContain("Brown");
        }
    }
}
