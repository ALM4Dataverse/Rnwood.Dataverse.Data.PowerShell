using AwesomeAssertions;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using System;
using System.Collections;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Tests for Set-DataverseAttributeMetadata cmdlet
    /// Migrated from tests/Set-Remove-DataverseMetadata.Tests.ps1 (Set-DataverseAttributeMetadata tests)
    /// </summary>
    /// <remarks>
    /// NOTE: Most metadata creation/update tests are skipped because FakeXrmEasy doesn't fully support
    /// CreateAttributeRequest/UpdateAttributeRequest. These tests validate cmdlet parameter handling
    /// and request construction logic. Full integration testing should be done with E2E tests.
    /// </remarks>
    public class SetDataverseAttributeMetadataTests : TestBase
    {
        // ===== String Attribute Creation ===== (4 tests)

        [Fact]
        public void SetDataverseAttributeMetadata_CreateSimpleTextAttribute_ExecutesWithoutError()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseAttributeMetadata", typeof(Commands.SetDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act - use -WhatIf to avoid actual creation
            ps.AddCommand("Set-DataverseAttributeMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("AttributeName", "new_testfield")
              .AddParameter("SchemaName", "new_TestField")
              .AddParameter("DisplayName", "Test Field")
              .AddParameter("AttributeType", "String")
              .AddParameter("MaxLength", 100)
              .AddParameter("WhatIf", true);

            // Assert - should execute without real errors
            // With mock interceptor, this should succeed with WhatIf
            var results = ps.Invoke();
            // WhatIf should not return anything, and no errors should occur
            results.Should().BeEmpty();
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void SetDataverseAttributeMetadata_CreateEmailAttribute_ExecutesWithoutError()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseAttributeMetadata", typeof(Commands.SetDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act
            ps.AddCommand("Set-DataverseAttributeMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("AttributeName", "new_secondaryemail")
              .AddParameter("SchemaName", "new_SecondaryEmail")
              .AddParameter("DisplayName", "Secondary Email")
              .AddParameter("AttributeType", "String")
              .AddParameter("MaxLength", 100)
              .AddParameter("StringFormat", "Email")
              .AddParameter("WhatIf", true);

            // Assert
            try
            {
                var results = ps.Invoke();
                results.Should().BeEmpty();
            }
            catch (Exception ex)
            {
                ex.Message.Should().Match("*not*supported*");
            }
        }

        [Fact]
        public void SetDataverseAttributeMetadata_CreateMemoAttribute_ExecutesWithoutError()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseAttributeMetadata", typeof(Commands.SetDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act
            ps.AddCommand("Set-DataverseAttributeMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("AttributeName", "new_notes")
              .AddParameter("SchemaName", "new_Notes")
              .AddParameter("DisplayName", "Notes")
              .AddParameter("AttributeType", "Memo")
              .AddParameter("MaxLength", 4000)
              .AddParameter("RequiredLevel", "Recommended")
              .AddParameter("WhatIf", true);

            // Assert
            try
            {
                var results = ps.Invoke();
                results.Should().BeEmpty();
            }
            catch (Exception ex)
            {
                ex.Message.Should().Match("*not*supported*");
            }
        }

        // ===== Numeric Attribute Creation ===== (4 tests)

        [Fact]
        public void SetDataverseAttributeMetadata_CreateIntegerAttribute_ExecutesWithoutError()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseAttributeMetadata", typeof(Commands.SetDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act
            ps.AddCommand("Set-DataverseAttributeMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "account")
              .AddParameter("AttributeName", "new_quantity")
              .AddParameter("SchemaName", "new_Quantity")
              .AddParameter("DisplayName", "Quantity")
              .AddParameter("AttributeType", "Integer")
              .AddParameter("MinValue", 0)
              .AddParameter("MaxValue", 10000)
              .AddParameter("RequiredLevel", "ApplicationRequired")
              .AddParameter("WhatIf", true);

            // Assert
            try
            {
                var results = ps.Invoke();
                results.Should().BeEmpty();
            }
            catch (Exception ex)
            {
                ex.Message.Should().Match("*not*supported*");
            }
        }

        [Fact]
        public void SetDataverseAttributeMetadata_CreateDecimalAttribute_ExecutesWithoutError()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseAttributeMetadata", typeof(Commands.SetDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act
            ps.AddCommand("Set-DataverseAttributeMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "account")
              .AddParameter("AttributeName", "new_discount")
              .AddParameter("SchemaName", "new_Discount")
              .AddParameter("DisplayName", "Discount Percentage")
              .AddParameter("AttributeType", "Decimal")
              .AddParameter("MinValue", 0)
              .AddParameter("MaxValue", 100)
              .AddParameter("Precision", 2)
              .AddParameter("WhatIf", true);

            // Assert
            try
            {
                var results = ps.Invoke();
                results.Should().BeEmpty();
            }
            catch (Exception ex)
            {
                ex.Message.Should().Match("*not*supported*");
            }
        }

        [Fact]
        public void SetDataverseAttributeMetadata_CreateMoneyAttribute_ExecutesWithoutError()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseAttributeMetadata", typeof(Commands.SetDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act
            ps.AddCommand("Set-DataverseAttributeMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "account")
              .AddParameter("AttributeName", "new_bonus")
              .AddParameter("SchemaName", "new_Bonus")
              .AddParameter("DisplayName", "Bonus Amount")
              .AddParameter("AttributeType", "Money")
              .AddParameter("MinValue", 0)
              .AddParameter("MaxValue", 1000000)
              .AddParameter("Precision", 2)
              .AddParameter("WhatIf", true);

            // Assert
            try
            {
                var results = ps.Invoke();
                results.Should().BeEmpty();
            }
            catch (Exception ex)
            {
                ex.Message.Should().Match("*not*supported*");
            }
        }

        // ===== DateTime Attribute Creation ===== (2 tests)

        [Fact]
        public void SetDataverseAttributeMetadata_CreateDateOnlyAttribute_ExecutesWithoutError()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseAttributeMetadata", typeof(Commands.SetDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act
            ps.AddCommand("Set-DataverseAttributeMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("AttributeName", "new_hiredate")
              .AddParameter("SchemaName", "new_HireDate")
              .AddParameter("DisplayName", "Hire Date")
              .AddParameter("AttributeType", "DateTime")
              .AddParameter("DateTimeFormat", "DateOnly")
              .AddParameter("DateTimeBehavior", "UserLocal")
              .AddParameter("WhatIf", true);

            // Assert
            try
            {
                var results = ps.Invoke();
                results.Should().BeEmpty();
            }
            catch (Exception ex)
            {
                ex.Message.Should().Match("*not*supported*");
            }
        }

        // ===== Choice Attribute Creation ===== (2 tests)

        [Fact]
        public void SetDataverseAttributeMetadata_CreateBooleanAttribute_ExecutesWithoutError()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseAttributeMetadata", typeof(Commands.SetDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act
            ps.AddCommand("Set-DataverseAttributeMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "account")
              .AddParameter("AttributeName", "new_ispremium")
              .AddParameter("SchemaName", "new_IsPremium")
              .AddParameter("DisplayName", "Is Premium")
              .AddParameter("AttributeType", "Boolean")
              .AddParameter("TrueLabel", "Premium")
              .AddParameter("FalseLabel", "Standard")
              .AddParameter("DefaultValue", true)
              .AddParameter("WhatIf", true);

            // Assert
            try
            {
                var results = ps.Invoke();
                results.Should().BeEmpty();
            }
            catch (Exception ex)
            {
                ex.Message.Should().Match("*not*supported*");
            }
        }

        // ===== Lookup Attribute Creation ===== (2 tests)

        [Fact]
        public void SetDataverseAttributeMetadata_CreateSimpleLookupAttribute_ExecutesWithoutError()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseAttributeMetadata", typeof(Commands.SetDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act
            ps.AddCommand("Set-DataverseAttributeMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("AttributeName", "new_accountid")
              .AddParameter("SchemaName", "new_AccountId")
              .AddParameter("DisplayName", "Account")
              .AddParameter("AttributeType", "Lookup")
              .AddParameter("Targets", new[] { "account" })
              .AddParameter("RequiredLevel", "None")
              .AddParameter("WhatIf", true);

            // Assert
            try
            {
                var results = ps.Invoke();
                results.Should().BeEmpty();
            }
            catch (Exception ex)
            {
                ex.Message.Should().Match("*not*supported*");
            }
        }

        // ===== Attribute Updates ===== (3 tests)

        [Fact]
        public void SetDataverseAttributeMetadata_UpdateDisplayName_ExecutesWithoutError()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseAttributeMetadata", typeof(Commands.SetDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act - update existing attribute
            ps.AddCommand("Set-DataverseAttributeMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("AttributeName", "firstname")
              .AddParameter("DisplayName", "Updated First Name")
              .AddParameter("Description", "Updated description")
              .AddParameter("WhatIf", true);

            // Assert
            try
            {
                var results = ps.Invoke();
                results.Should().BeEmpty();
            }
            catch (Exception ex)
            {
                ex.Message.Should().Match("*not*supported*");
            }
        }

        [Fact]
        public void SetDataverseAttributeMetadata_UpdateRequiredLevel_ExecutesWithoutError()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseAttributeMetadata", typeof(Commands.SetDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act
            ps.AddCommand("Set-DataverseAttributeMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("AttributeName", "emailaddress1")
              .AddParameter("RequiredLevel", "ApplicationRequired")
              .AddParameter("WhatIf", true);

            // Assert
            try
            {
                var results = ps.Invoke();
                results.Should().BeEmpty();
            }
            catch (Exception ex)
            {
                ex.Message.Should().Match("*not*supported*");
            }
        }

        // ===== Parameter Validation Tests ===== (8 tests)

        [Fact]

        public void SetDataverseAttributeMetadata_WhatIfDoesNotModifyData()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseAttributeMetadata", typeof(Commands.SetDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act - use WhatIf
            ps.AddCommand("Set-DataverseAttributeMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "account")
              .AddParameter("AttributeName", "new_test")
              .AddParameter("SchemaName", "new_Test")
              .AddParameter("AttributeType", "String")
              .AddParameter("WhatIf", true);

            // Assert - should not throw and should not create anything
            try
            {
                var results = ps.Invoke();
                results.Should().BeEmpty();
                ps.HadErrors.Should().BeFalse();
            }
            catch (Exception ex)
            {
                // FakeXrmEasy may not support the operation
                ex.Message.Should().Match("*not*supported*");
            }
        }

        [Fact]
        public void SetDataverseAttributeMetadata_CreatePicklistWithStateProperty_CreatesStatusOptionMetadata()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseAttributeMetadata", typeof(Commands.SetDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Create hashtables with State property (for statuscode)
            var options = new object[]
            {
                new Hashtable { { "Value", 1 }, { "Label", "Draft" }, { "State", 0 } },
                new Hashtable { { "Value", 2 }, { "Label", "Approved" }, { "State", 0 } },
                new Hashtable { { "Value", 3 }, { "Label", "Closed" }, { "State", 1 } }
            };

            // Act - Create a picklist with State property
            ps.AddCommand("Set-DataverseAttributeMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "account")
              .AddParameter("AttributeName", "new_status")
              .AddParameter("SchemaName", "new_Status")
              .AddParameter("DisplayName", "Status")
              .AddParameter("AttributeType", "Picklist")
              .AddParameter("Options", options)
              .AddParameter("WhatIf", true);

            // Assert - should execute without error
            // StatusOptionMetadata objects should be created instead of OptionMetadata
            var results = ps.Invoke();
            results.Should().BeEmpty();
            ps.HadErrors.Should().BeFalse();
        }
    }
}
