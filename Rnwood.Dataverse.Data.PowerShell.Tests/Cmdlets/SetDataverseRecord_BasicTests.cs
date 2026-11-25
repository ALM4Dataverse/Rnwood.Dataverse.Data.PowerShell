using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using AwesomeAssertions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using System.Reflection;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Basic tests for Set-DataverseRecord cmdlet.
    /// Migrated from Pester tests in tests/Set-DataverseRecord.Tests.ps1
    /// </summary>
    public class SetDataverseRecord_BasicTests : TestBase
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
        public void SetDataverseRecord_CreateOnly_SingleRecord()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var record = new Hashtable {
                ["firstname"] = "John",
                ["lastname"] = "Doe",
                ["emailaddress1"] = "john.doe@example.com"
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
            results[0].Properties["Id"].Value.Should().BeOfType<Guid>();
            var createdId = (Guid)results[0].Properties["Id"].Value;
            createdId.Should().NotBe(Guid.Empty);
            
            // Verify record was created
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname", "emailaddress1" })
              .AddParameter("Id", createdId);
            var retrieved = ps.Invoke();
            
            ps.HadErrors.Should().BeFalse();
            retrieved.Should().HaveCount(1);
            retrieved[0].Properties["firstname"].Value.Should().Be("John");
            retrieved[0].Properties["lastname"].Value.Should().Be("Doe");
            retrieved[0].Properties["emailaddress1"].Value.Should().Be("john.doe@example.com");
        }

        [Fact]
        public void SetDataverseRecord_CreateOnly_MultipleRecords()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var records = new[] {
                new Hashtable { ["firstname"] = "Alice", ["lastname"] = "Smith" },
                new Hashtable { ["firstname"] = "Bob", ["lastname"] = "Johnson" },
                new Hashtable { ["firstname"] = "Charlie", ["lastname"] = "Williams" }
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
            results.All(r => r.Properties["Id"].Value is Guid).Should().BeTrue();
            results.All(r => (Guid)r.Properties["Id"].Value != Guid.Empty).Should().BeTrue();
            
            // Verify all records were created
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname" });
            var allContacts = ps.Invoke();
            
            allContacts.Where(c => c.Properties["firstname"].Value.Equals("Alice")).Should().HaveCount(1);
            allContacts.Where(c => c.Properties["firstname"].Value.Equals("Bob")).Should().HaveCount(1);
            allContacts.Where(c => c.Properties["firstname"].Value.Equals("Charlie")).Should().HaveCount(1);
        }

        [Fact]
        public void SetDataverseRecord_WithoutPassThru_ReturnsNothing()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var record = new Hashtable { ["firstname"] = "Test", ["lastname"] = "User" };
            
            // Act
            var inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(record));
            inputCollection.Complete();
            
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true);
            var results = ps.Invoke(inputCollection);

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().BeEmpty();
            
            // Verify record still created
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" });
            var allContacts = ps.Invoke();
            
            allContacts.Where(c => c.Properties["firstname"].Value.Equals("Test") && 
                                    c.Properties["lastname"].Value.Equals("User")).Should().HaveCount(1);
        }

        [Fact]
        public void SetDataverseRecord_Update_ExistingRecord()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create initial record using hashtable
            var createRecord = new Hashtable {
                ["firstname"] = "Original",
                ["lastname"] = "ToUpdate",
                ["emailaddress1"] = "update@test.com"
            };
            var inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(createRecord));
            inputCollection.Complete();
            
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var createResults = ps.Invoke(inputCollection);
            var contactId = (Guid)createResults[0].Properties["Id"].Value;
            
            // Create another record that should not change
            var otherRecord = new Hashtable {
                ["firstname"] = "Unchanged",
                ["lastname"] = "NoChange",
                ["emailaddress1"] = "nochange@test.com"
            };
            inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(otherRecord));
            inputCollection.Complete();
            
            ps.Commands.Clear();
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var otherResults = ps.Invoke(inputCollection);
            var otherId = (Guid)otherResults[0].Properties["Id"].Value;
            
            // Act - Update first record using hashtable with Id
            var updateRecord = new Hashtable {
                ["contactid"] = contactId,
                ["firstname"] = "Updated"
            };
            inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(updateRecord));
            inputCollection.Complete();
            
            ps.Commands.Clear();
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact");
            var updateResults = ps.Invoke(inputCollection);

            // Assert
            ps.HadErrors.Should().BeFalse();
            
            // Verify update occurred
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "contactid", "firstname", "lastname", "emailaddress1" });
            var allRecords = ps.Invoke();
            
            allRecords.Should().HaveCount(2);
            
            var updated = allRecords.First(r => (Guid)r.Properties["Id"].Value == contactId);
            updated.Properties["firstname"].Value.Should().Be("Updated");
            updated.Properties["lastname"].Value.Should().Be("ToUpdate");
            
            var unchanged = allRecords.First(r => (Guid)r.Properties["Id"].Value == otherId);
            unchanged.Properties["firstname"].Value.Should().Be("Unchanged");
            unchanged.Properties["lastname"].Value.Should().Be("NoChange");
        }

        [Fact]
        public void SetDataverseRecord_PassThru_ReturnsUniqueRecords()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var records = new[] {
                new Hashtable { ["firstname"] = "Alice", ["lastname"] = "Smith", ["emailaddress1"] = "alice@example.com" },
                new Hashtable { ["firstname"] = "Bob", ["lastname"] = "Johnson", ["emailaddress1"] = "bob@example.com" },
                new Hashtable { ["firstname"] = "Charlie", ["lastname"] = "Williams", ["emailaddress1"] = "charlie@example.com" },
                new Hashtable { ["firstname"] = "Diana", ["lastname"] = "Brown", ["emailaddress1"] = "diana@example.com" },
                new Hashtable { ["firstname"] = "Eve", ["lastname"] = "Davis", ["emailaddress1"] = "eve@example.com" }
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
            results.Should().HaveCount(5);
            
            // Verify unique IDs
            var uniqueIds = results.Select(r => (Guid)r.Properties["Id"].Value).Distinct().ToList();
            uniqueIds.Should().HaveCount(5);
            
            // Verify each input record is represented exactly once
            results.Where(r => r.Properties["firstname"].Value.Equals("Alice") && 
                                r.Properties["lastname"].Value.Equals("Smith")).Should().HaveCount(1);
            results.Where(r => r.Properties["firstname"].Value.Equals("Bob") && 
                                r.Properties["lastname"].Value.Equals("Johnson")).Should().HaveCount(1);
            results.Where(r => r.Properties["firstname"].Value.Equals("Charlie") && 
                                r.Properties["lastname"].Value.Equals("Williams")).Should().HaveCount(1);
            results.Where(r => r.Properties["firstname"].Value.Equals("Diana") && 
                                r.Properties["lastname"].Value.Equals("Brown")).Should().HaveCount(1);
            results.Where(r => r.Properties["firstname"].Value.Equals("Eve") && 
                                r.Properties["lastname"].Value.Equals("Davis")).Should().HaveCount(1);
        }

        [Fact]
        public void SetDataverseRecord_ValuesAlias_SingleHashtable()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var values = new Hashtable {
                ["firstname"] = "John",
                ["lastname"] = "Doe",
                ["emailaddress1"] = "john.doe@example.com"
            };
            
            // Act - Use -Values alias instead of -InputObject
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Values", values)  // Test the -Values alias
              .AddParameter("CreateOnly", true);
            var results = ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
            ps.Streams.Error.Should().BeEmpty();
            
            // Verify record was created with correct values
            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname", "emailaddress1" });
            var records = ps.Invoke();
            
            records.Should().HaveCount(1);
            records[0].Properties["firstname"].Value.Should().Be("John");
            records[0].Properties["lastname"].Value.Should().Be("Doe");
            records[0].Properties["emailaddress1"].Value.Should().Be("john.doe@example.com");
        }

          [Fact]
          public void SetDataverseRecord_ValuesAliasInLoop_ReturnsCreatedRecords()
          {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            var records = new[]
            {
              new Hashtable { ["firstname"] = "Alpha", ["lastname"] = "Loop" },
              new Hashtable { ["firstname"] = "Beta", ["lastname"] = "Loop" },
              new Hashtable { ["firstname"] = "Gamma", ["lastname"] = "Loop" },
              new Hashtable { ["firstname"] = "Delta", ["lastname"] = "Loop" }
            };

            // Act
            var results = new List<PSObject>();
            foreach (var values in records)
            {
              ps.Commands.Clear();
              ps.AddCommand("Set-DataverseRecord")
                .AddParameter("Connection", mockConnection)
                .AddParameter("TableName", "contact")
                .AddParameter("Values", values)
                .AddParameter("CreateOnly", true)
                .AddParameter("PassThru", true);
              results.AddRange(ps.Invoke());
              ps.HadErrors.Should().BeFalse();
            }

            // Assert
            results.Should().HaveCount(4);
            results.Select(r => r.Properties["Id"].Value).Should().AllSatisfy(value =>
            {
              value.Should().BeOfType<Guid>();
              ((Guid)value).Should().NotBe(Guid.Empty);
            });

            ps.Commands.Clear();
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" });
            var createdRecords = ps.Invoke();

            createdRecords.Should().Contain(r => r.Properties["firstname"].Value.Equals("Alpha") && r.Properties["lastname"].Value.Equals("Loop"));
            createdRecords.Should().Contain(r => r.Properties["firstname"].Value.Equals("Beta") && r.Properties["lastname"].Value.Equals("Loop"));
            createdRecords.Should().Contain(r => r.Properties["firstname"].Value.Equals("Gamma") && r.Properties["lastname"].Value.Equals("Loop"));
            createdRecords.Should().Contain(r => r.Properties["firstname"].Value.Equals("Delta") && r.Properties["lastname"].Value.Equals("Loop"));
          }

          [Fact]
          public void SetDataverseRecord_NonReadableColumn_ThrowsHelpfulErrorMessage()
          {
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnectionWithCustomMetadata(null, BuildOrganizationMetadataWithNonReadableColumn());

            var organizationId = Environment!.OrganizationId;
            var updateValues = new Hashtable
            {
              ["telemetryinstrumentationkey"] = "test-instrumentation-key-value"
            };

            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "organization")
              .AddParameter("Id", organizationId)
              .AddParameter("InputObject", updateValues)
              .AddParameter("ErrorAction", ActionPreference.Stop);

            Action invoke = () => ps.Invoke();

            invoke.Should().Throw<ActionPreferenceStopException>()
              .WithMessage("*not valid for read*")
              .WithMessage("*telemetryinstrumentationkey*")
              .WithMessage("*organization*")
              .WithMessage("*-UpdateAllColumns*");
          }

          private static List<EntityMetadata> BuildOrganizationMetadataWithNonReadableColumn()
          {
            return new List<EntityMetadata>
            {
              BuildEntityMetadata(
                logicalName: "organization",
                schemaName: "Organization",
                primaryIdAttribute: "organizationid",
                primaryNameAttribute: "name",
                objectTypeCode: 1,
                BuildGuidAttribute("organizationid", "OrganizationId"),
                BuildStringAttribute("name", "Name", isValidForRead: true),
                BuildStringAttribute("telemetryinstrumentationkey", "TelemetryInstrumentationKey", isValidForRead: false))
            };
          }

          private static EntityMetadata BuildEntityMetadata(
            string logicalName,
            string schemaName,
            string primaryIdAttribute,
            string primaryNameAttribute,
            int objectTypeCode,
            params AttributeMetadata[] attributes)
          {
            var metadata = new EntityMetadata();
            SetMetadataProperty(metadata, nameof(EntityMetadata.LogicalName), logicalName);
            SetMetadataProperty(metadata, nameof(EntityMetadata.SchemaName), schemaName);
            SetMetadataProperty(metadata, nameof(EntityMetadata.PrimaryIdAttribute), primaryIdAttribute);
            SetMetadataProperty(metadata, nameof(EntityMetadata.PrimaryNameAttribute), primaryNameAttribute);
            SetMetadataProperty(metadata, nameof(EntityMetadata.ObjectTypeCode), objectTypeCode);
            SetMetadataProperty(metadata, nameof(EntityMetadata.Attributes), attributes);
            return metadata;
          }

          private static AttributeMetadata BuildGuidAttribute(string logicalName, string schemaName)
          {
            var attribute = new AttributeMetadata();
            SetAttributeDefaults(attribute, logicalName, schemaName, AttributeTypeCode.Uniqueidentifier, isValidForRead: true);
            return attribute;
          }

          private static StringAttributeMetadata BuildStringAttribute(string logicalName, string schemaName, bool isValidForRead)
          {
            var attribute = new StringAttributeMetadata { MaxLength = 1073741823 };
            SetAttributeDefaults(attribute, logicalName, schemaName, AttributeTypeCode.String, isValidForRead);
            return attribute;
          }

          private static void SetAttributeDefaults(AttributeMetadata attribute, string logicalName, string schemaName, AttributeTypeCode type, bool isValidForRead)
          {
            SetMetadataProperty(attribute, nameof(AttributeMetadata.LogicalName), logicalName);
            SetMetadataProperty(attribute, nameof(AttributeMetadata.SchemaName), schemaName);
            SetMetadataProperty(attribute, nameof(AttributeMetadata.AttributeType), type);
            SetMetadataProperty(attribute, nameof(AttributeMetadata.IsValidForRead), isValidForRead);
          }

          private static void SetMetadataProperty(object target, string propertyName, object? value)
          {
            var property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            property?.SetValue(target, value);
          }
    }
}
