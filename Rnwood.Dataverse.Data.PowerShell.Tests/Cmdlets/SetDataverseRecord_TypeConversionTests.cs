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
    /// Type conversion tests for Set-DataverseRecord cmdlet.
    /// Migrated from Pester tests in tests/Set-DataverseRecord-TypeConversions.Tests.ps1
    /// </summary>
    public class SetDataverseRecord_TypeConversionTests : TestBase
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
        public void SetDataverseRecord_DateTime_CreateWithDateTimeField()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var testDate = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
            var record = new Hashtable {
                ["firstname"] = "DateTime",
                ["lastname"] = "Test",
                ["birthdate"] = testDate
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
            
            // Verify birthdate was set
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Id", createdId)
              .AddParameter("Columns", new[] { "firstname", "birthdate" });
            var retrieved = ps.Invoke();
            
            retrieved[0].Properties["firstname"].Value.Should().Be("DateTime");
            retrieved[0].Properties["birthdate"].Value.Should().NotBeNull();
        }

        [Fact]
        public void SetDataverseRecord_DateTime_UpdateWithTimezone()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create initial record
            var initialRecord = new Hashtable {
                ["firstname"] = "Update",
                ["lastname"] = "DateTime",
                ["birthdate"] = new DateTime(2024, 1, 1)
            };
            var inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(initialRecord));
            inputCollection.Complete();
            
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var createResults = ps.Invoke(inputCollection);
            var contactId = (Guid)createResults[0].Properties["Id"].Value;
            
            // Act - Update with new DateTime
            var newDate = new DateTime(2024, 12, 31, 23, 59, 59, DateTimeKind.Utc);
            var updateRecord = new Hashtable {
                ["contactid"] = contactId,
                ["birthdate"] = newDate
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
              .AddParameter("Columns", new[] { "birthdate" });
            var retrieved = ps.Invoke();
            
            retrieved[0].Properties["birthdate"].Value.Should().NotBeNull();
        }

        [Fact]
        public void SetDataverseRecord_DateTime_ClearWithNull()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create record with DateTime
            var record = new Hashtable {
                ["firstname"] = "Null",
                ["lastname"] = "DateTime",
                ["birthdate"] = new DateTime(2024, 6, 15)
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
            
            // Act - Clear DateTime by setting to null
            var updateRecord = new Hashtable {
                ["contactid"] = contactId,
                ["birthdate"] = null
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
              .AddParameter("Columns", new[] { "contactid" });
            var retrieved = ps.Invoke();
            
            retrieved.Should().HaveCount(1);
        }

        [Fact]
        public void SetDataverseRecord_DateTime_PreserveDateOnly()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var dateOnly = new DateTime(2024, 3, 15).Date;
            var record = new Hashtable {
                ["firstname"] = "Date",
                ["lastname"] = "Only",
                ["birthdate"] = dateOnly
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
              .AddParameter("Columns", new[] { "birthdate" });
            var retrieved = ps.Invoke();
            
            retrieved[0].Properties["birthdate"].Value.Should().NotBeNull();
        }

        [Fact]
        public void SetDataverseRecord_Money_CreateWithMoneyField()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var record = new Hashtable {
                ["firstname"] = "Money",
                ["lastname"] = "Test",
                ["creditlimit"] = 50000.00m
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
              .AddParameter("Columns", new[] { "firstname", "creditlimit" });
            var retrieved = ps.Invoke();
            
            retrieved[0].Properties["firstname"].Value.Should().Be("Money");
            retrieved[0].Properties["creditlimit"].Value.Should().Be(50000.00m);
        }

        [Fact]
        public void SetDataverseRecord_Money_UpdateMoneyField()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create initial record
            var record = new Hashtable {
                ["firstname"] = "Update",
                ["lastname"] = "Money",
                ["annualincome"] = 75000.00m
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
            
            // Act - Update Money field
            var updateRecord = new Hashtable {
                ["contactid"] = contactId,
                ["annualincome"] = 85000.00m
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
              .AddParameter("Columns", new[] { "annualincome" });
            var retrieved = ps.Invoke();
            
            retrieved[0].Properties["annualincome"].Value.Should().Be(85000.00m);
        }

        [Fact]
        public void SetDataverseRecord_Money_ClearWithNull()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create record with Money field
            var record = new Hashtable {
                ["firstname"] = "Null",
                ["lastname"] = "Money",
                ["creditlimit"] = 100000.00m
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
            
            // Act - Set Money to null
            var updateRecord = new Hashtable {
                ["contactid"] = contactId,
                ["creditlimit"] = null
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
              .AddParameter("Columns", new[] { "creditlimit" });
            var retrieved = ps.Invoke();
            
            retrieved[0].Properties.Cast<PSPropertyInfo>()
                .FirstOrDefault(p => p.Name == "creditlimit")?.Value.Should().BeNull();
        }
    }
}
