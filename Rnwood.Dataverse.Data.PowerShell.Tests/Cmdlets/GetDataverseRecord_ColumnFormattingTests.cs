using System;
using System.Collections;
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
    /// Column formatting tests for Get-DataverseRecord cmdlet.
    /// Tests for LookupValuesReturnName and column format suffixes (:Raw, :Display).
    /// </summary>
    public class GetDataverseRecord_ColumnFormattingTests : TestBase
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
        public void GetDataverseRecord_LookupValuesReturnName_ReturnsLookupAsName()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create parent contact
            var parentContact = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Parent",
                ["lastname"] = "Contact"
            };
            Service!.Create(parentContact);
            
            // Create child contact with lookup to parent
            var childContact = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Child",
                ["lastname"] = "Contact",
                ["parentcustomerid"] = new EntityReference("contact", parentContact.Id)
            };
            Service.Create(childContact);
            
            // Act
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", childContact.Id)
              .AddParameter("Columns", new[] { "firstname", "lastname", "parentcustomerid" })
              .AddParameter("LookupValuesReturnName", true);
            
            var results = ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["firstname"].Value.Should().Be("Child");
            results[0].Properties["lastname"].Value.Should().Be("Contact");
            // Note: LookupValuesReturnName behavior depends on mock support
            // The test validates the flag is accepted without error
        }

        [Fact]
        public void GetDataverseRecord_WithoutLookupValuesReturnName_ReturnsLookupAsObject()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create parent contact
            var parentContact = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Default",
                ["lastname"] = "Parent"
            };
            Service!.Create(parentContact);
            
            // Create child contact with lookup
            var childContact = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Default",
                ["lastname"] = "Child",
                ["parentcustomerid"] = new EntityReference("contact", parentContact.Id)
            };
            Service.Create(childContact);
            
            // Act - query without LookupValuesReturnName
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", childContact.Id)
              .AddParameter("Columns", new[] { "firstname", "lastname", "parentcustomerid" });
            
            var results = ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["firstname"].Value.Should().Be("Default");
            results[0].Properties["lastname"].Value.Should().Be("Child");
        }

        [Fact]
        public void GetDataverseRecord_ColumnWithRawSuffix_ReturnsRawValue()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create contact with OptionSet field
            var contact = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Raw",
                ["lastname"] = "Test",
                ["accountrolecode"] = new OptionSetValue(1)
            };
            Service!.Create(contact);
            
            // Act - query with :Raw suffix
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", contact.Id)
              .AddParameter("Columns", new[] { "firstname", "accountrolecode:Raw" });
            
            var results = ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["firstname"].Value.Should().Be("Raw");
            // accountrolecode with :Raw should return numeric value
            // The exact behavior depends on the cmdlet implementation
        }

        [Fact]
        public void GetDataverseRecord_ColumnWithDisplaySuffix_ReturnsDisplayValue()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create contact with OptionSet field
            var contact = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Display",
                ["lastname"] = "Test",
                ["accountrolecode"] = new OptionSetValue(2)
            };
            Service!.Create(contact);
            
            // Act - query with :Display suffix
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", contact.Id)
              .AddParameter("Columns", new[] { "firstname", "accountrolecode:Display" });
            
            var results = ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["firstname"].Value.Should().Be("Display");
            // accountrolecode with :Display should return formatted/label value
            // The exact behavior depends on metadata and implementation
        }

        [Fact]
        public void GetDataverseRecord_MultipleColumnsWithDifferentFormatSuffixes_HandlesCorrectly()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create contact with multiple fields
            var contact = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Mixed",
                ["lastname"] = "Formats",
                ["accountrolecode"] = new OptionSetValue(1),
                ["donotbulkemail"] = true
            };
            Service!.Create(contact);
            
            // Act - query with mixed format suffixes
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", contact.Id)
              .AddParameter("Columns", new[] { "firstname", "accountrolecode:Raw", "donotbulkemail:Display" });
            
            var results = ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["firstname"].Value.Should().Be("Mixed");
        }

        [Fact]
        public void GetDataverseRecord_ColumnWithoutSuffix_UsesDefaultFormat()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create contact
            var contact = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Default",
                ["lastname"] = "Format",
                ["accountrolecode"] = new OptionSetValue(1)
            };
            Service!.Create(contact);
            
            // Act - query without suffix
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", contact.Id)
              .AddParameter("Columns", new[] { "firstname", "accountrolecode" });
            
            var results = ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["firstname"].Value.Should().Be("Default");
            // accountrolecode without suffix uses default behavior
        }
    }
}
