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
    /// Advanced filter grouping tests for Get-DataverseRecord cmdlet.
    /// Tests for 'and', 'or', 'not', 'xor' grouping with FilterValues parameter.
    /// </summary>
    public class GetDataverseRecord_FilterGroupingTests : TestBase
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
        public void GetDataverseRecord_FilterGrouping_AndKey()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Joe", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "Two" });
            
            // Act - Use a grouping hashtable to require firstname=Rob AND lastname=One
            var filter = new Hashtable
            {
                ["and"] = new object[] {
                    new Hashtable { ["firstname"] = "Rob" },
                    new Hashtable { ["lastname"] = "One" }
                }
            };
            
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("FilterValues", filter);
            
            var results = ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["firstname"].Value.Should().Be("Rob");
            results[0].Properties["lastname"].Value.Should().Be("One");
        }

        [Fact]
        public void GetDataverseRecord_FilterGrouping_NestedOrInsideAnd()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Joe", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Mary", ["lastname"] = "Two" });
            
            // Act - ( (firstname = Rob OR firstname = Joe) AND lastname = One ) => matches Rob One and Joe One
            var filter = new Hashtable
            {
                ["and"] = new object[] {
                    new Hashtable {
                        ["or"] = new object[] {
                            new Hashtable { ["firstname"] = "Rob" },
                            new Hashtable { ["firstname"] = "Joe" }
                        }
                    },
                    new Hashtable { ["lastname"] = "One" }
                }
            };
            
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("FilterValues", filter);
            
            var results = ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(2);
            var firstnames = results.Select(r => r.Properties["firstname"].Value.ToString()).ToList();
            firstnames.Should().Contain("Rob");
            firstnames.Should().Contain("Joe");
        }

        [Fact]
        public void GetDataverseRecord_ExcludeFilterGrouping_OrKey()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Joe", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Mary", ["lastname"] = "Two" });
            
            // Act - Exclude where firstname = Rob OR lastname = Two
            var excludeFilter = new Hashtable
            {
                ["or"] = new object[] {
                    new Hashtable { ["firstname"] = "Rob" },
                    new Hashtable { ["lastname"] = "Two" }
                }
            };
            
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("ExcludeFilterValues", excludeFilter);
            
            var results = ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["firstname"].Value.Should().Be("Joe");
        }
    }
}
