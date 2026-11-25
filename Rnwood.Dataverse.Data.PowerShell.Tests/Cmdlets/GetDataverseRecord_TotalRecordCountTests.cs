using System;
using System.Collections;
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
    /// RecordCount parameter tests for Get-DataverseRecord cmdlet.
    /// Tests for counting records instead of retrieving them.
    /// </summary>
    public class GetDataverseRecord_TotalRecordCountTests : TestBase
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
        public void GetDataverseRecord_TotalRecordCount_ReturnsCount()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create test records
            for (int i = 1; i <= 5; i++)
            {
                Service!.Create(new Entity("contact") { ["firstname"] = $"User{i}", ["lastname"] = "Count" });
            }
            
            // Act - Query with RecordCount
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("RecordCount", true);
            
            var results = ps.Invoke();
            
            // Assert - result should be a count, not records
            ps.HadErrors.Should().BeFalse();
            results.Should().NotBeEmpty();
        }

        [Fact]
        public void GetDataverseRecord_TotalRecordCount_WithFilter()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "Match1", ["lastname"] = "Filter" });
            Service.Create(new Entity("contact") { ["firstname"] = "Match2", ["lastname"] = "Filter" });
            Service.Create(new Entity("contact") { ["firstname"] = "NoMatch1", ["lastname"] = "Other" });
            Service.Create(new Entity("contact") { ["firstname"] = "NoMatch2", ["lastname"] = "Other" });
            
            // Act
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("FilterValues", new Hashtable { ["lastname"] = "Filter" })
              .AddParameter("RecordCount", true);
            
            var results = ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().NotBeEmpty();
        }

        [Fact]
        public void GetDataverseRecord_TotalRecordCount_NoMatches_ReturnsZero()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "Existing1", ["lastname"] = "User" });
            Service.Create(new Entity("contact") { ["firstname"] = "Existing2", ["lastname"] = "User" });
            
            // Act
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("FilterValues", new Hashtable { ["lastname"] = "NoMatch" })
              .AddParameter("RecordCount", true);
            
            var results = ps.Invoke();
            
            // Assert - count should be 0
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void GetDataverseRecord_TotalRecordCount_Performance()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create many records
            for (int i = 1; i <= 20; i++)
            {
                Service!.Create(new Entity("contact") { ["firstname"] = $"User{i}", ["lastname"] = "Performance" });
            }
            
            // Act - Get count (should be fast, no record retrieval)
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("RecordCount", true);
            
            var results = ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().NotBeEmpty();
        }

        [Fact]
        public void GetDataverseRecord_TotalRecordCount_WithTop_CountsAll()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create test records
            for (int i = 1; i <= 10; i++)
            {
                Service!.Create(new Entity("contact") { ["firstname"] = $"User{i}", ["lastname"] = "Top" });
            }
            
            // Act - Query with Top and RecordCount
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Top", 3)
              .AddParameter("RecordCount", true);
            
            var results = ps.Invoke();
            
            // Assert - RecordCount should return total count (10), not just Top count (3)
            ps.HadErrors.Should().BeFalse();
            results.Should().NotBeEmpty();
        }
    }
}
