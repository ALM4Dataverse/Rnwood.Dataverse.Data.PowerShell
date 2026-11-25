using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using AwesomeAssertions;
using Microsoft.Xrm.Sdk;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets;

/// <summary>
/// Tests for miscellaneous cmdlets:
/// - Get-DataverseConnectionReference
/// - Set-DataverseEnvironmentVariableValue
/// - Get-DataverseComponentDependency
/// - Argument Completers
/// </summary>
public class MiscellaneousCmdletsTests : TestBase
{
    private PS CreatePowerShellWithMiscCmdlets()
    {
        var initialSessionState = InitialSessionState.CreateDefault2();
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataverseConnectionReference", typeof(GetDataverseConnectionReferenceCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Set-DataverseConnectionReference", typeof(SetDataverseConnectionReferenceCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Remove-DataverseConnectionReference", typeof(RemoveDataverseConnectionReferenceCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataverseEnvironmentVariableDefinition", typeof(GetDataverseEnvironmentVariableDefinitionCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Set-DataverseEnvironmentVariableDefinition", typeof(SetDataverseEnvironmentVariableDefinitionCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataverseEnvironmentVariableValue", typeof(GetDataverseEnvironmentVariableValueCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Set-DataverseEnvironmentVariableValue", typeof(SetDataverseEnvironmentVariableValueCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Remove-DataverseEnvironmentVariableValue", typeof(RemoveDataverseEnvironmentVariableValueCmdlet), null));
        
        var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
        runspace.Open();
        
        var ps = PS.Create();
        ps.Runspace = runspace;
        return ps;
    }

    // Get-DataverseConnectionReference Tests

    [Fact]
    public void GetDataverseConnectionReference_RetrievesConnectionReferences()
    {
        // Arrange
        using var ps = CreatePowerShellWithMiscCmdlets();
        var mockConnection = CreateMockConnection("connectionreference");
        
        var connRef = new Entity("connectionreference")
        {
            Id = Guid.NewGuid(),
            ["connectionreferencelogicalname"] = "new_myconnref",
            ["connectionreferencedisplayname"] = "My Connection Reference",
            ["connectorid"] = "/providers/Microsoft.PowerApps/apis/shared_commondataservice"
        };
        Environment!.Seed(connRef);
        
        // Act
        ps.AddCommand("Get-DataverseConnectionReference")
          .AddParameter("Connection", mockConnection);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
    }

    [Fact]
    public void GetDataverseConnectionReference_FiltersBySchemaName()
    {
        // Arrange
        using var ps = CreatePowerShellWithMiscCmdlets();
        var mockConnection = CreateMockConnection("connectionreference");
        
        var connRef1 = new Entity("connectionreference")
        {
            Id = Guid.NewGuid(),
            ["connectionreferencelogicalname"] = "new_connref1",
            ["connectionreferencedisplayname"] = "Connection Ref 1"
        };
        var connRef2 = new Entity("connectionreference")
        {
            Id = Guid.NewGuid(),
            ["connectionreferencelogicalname"] = "new_connref2",
            ["connectionreferencedisplayname"] = "Connection Ref 2"
        };
        Environment!.Seed(connRef1, connRef2);
        
        // Act - Filter by schema name
        ps.AddCommand("Get-DataverseConnectionReference")
          .AddParameter("Connection", mockConnection)
          .AddParameter("ConnectionReferenceLogicalName", "new_connref1");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["ConnectionReferenceLogicalName"].Value.Should().Be("new_connref1");
    }

    // Set-DataverseEnvironmentVariableValue Tests

    [Fact]
    public void SetDataverseEnvironmentVariableValue_UpdatesEnvironmentVariableValue()
    {
        // Arrange
        using var ps = CreatePowerShellWithMiscCmdlets();
        var mockConnection = CreateMockConnection("environmentvariabledefinition", "environmentvariablevalue");
        
        var definitionId = Guid.NewGuid();
        var definition = new Entity("environmentvariabledefinition")
        {
            Id = definitionId,
            ["schemaname"] = "new_apiurl",
            ["displayname"] = "API URL",
            ["type"] = new OptionSetValue(100000000) // String
        };
        
        var existingValue = new Entity("environmentvariablevalue")
        {
            Id = Guid.NewGuid(),
            ["environmentvariabledefinitionid"] = new EntityReference("environmentvariabledefinition", definitionId),
            ["value"] = "https://old-api.example.com"
        };
        Environment!.Seed(definition, existingValue);
        
        // Act
        ps.AddCommand("Set-DataverseEnvironmentVariableValue")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SchemaName", "new_apiurl")
          .AddParameter("Value", "https://new-api.example.com");
        ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
    }

    [Fact]
    public void SetDataverseEnvironmentVariableValue_CreatesEnvironmentVariableValue()
    {
        // Arrange
        using var ps = CreatePowerShellWithMiscCmdlets();
        var mockConnection = CreateMockConnection("environmentvariabledefinition", "environmentvariablevalue");
        
        var definitionId = Guid.NewGuid();
        var definition = new Entity("environmentvariabledefinition")
        {
            Id = definitionId,
            ["schemaname"] = "new_newvar",
            ["displayname"] = "New Variable",
            ["type"] = new OptionSetValue(100000000) // String
        };
        Environment!.Seed(definition);
        
        // Act
        ps.AddCommand("Set-DataverseEnvironmentVariableValue")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SchemaName", "new_newvar")
          .AddParameter("Value", "new value");
        ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
    }

    [Fact]
    public void SetDataverseEnvironmentVariableValue_ConnectionReference_UpdatesConnectionReferenceValue()
    {
        // Arrange
        using var ps = CreatePowerShellWithMiscCmdlets();
        var mockConnection = CreateMockConnection("environmentvariabledefinition", "environmentvariablevalue");
        
        var definitionId = Guid.NewGuid();
        var definition = new Entity("environmentvariabledefinition")
        {
            Id = definitionId,
            ["schemaname"] = "new_connrefvar",
            ["displayname"] = "Connection Reference Variable",
            ["type"] = new OptionSetValue(100000003) // ConnectionReference type
        };
        Environment!.Seed(definition);
        
        // Act
        ps.AddCommand("Set-DataverseEnvironmentVariableValue")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SchemaName", "new_connrefvar")
          .AddParameter("Value", "/providers/Microsoft.PowerApps/apis/shared_commondataservice/connections/abc123");
        ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
    }

    [Fact]
    public void SetDataverseEnvironmentVariableValue_DataSource_UpdatesDataSourceValue()
    {
        // Arrange
        using var ps = CreatePowerShellWithMiscCmdlets();
        var mockConnection = CreateMockConnection("environmentvariabledefinition", "environmentvariablevalue");
        
        var definitionId = Guid.NewGuid();
        var definition = new Entity("environmentvariabledefinition")
        {
            Id = definitionId,
            ["schemaname"] = "new_datasourcevar",
            ["displayname"] = "Data Source Variable",
            ["type"] = new OptionSetValue(100000002) // DataSource type
        };
        Environment!.Seed(definition);
        
        // Act
        ps.AddCommand("Set-DataverseEnvironmentVariableValue")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SchemaName", "new_datasourcevar")
          .AddParameter("Value", "datasource-value");
        ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
    }

    [Fact]
    public void SetDataverseEnvironmentVariableValue_SupportsWhatIf()
    {
        // Arrange
        using var ps = CreatePowerShellWithMiscCmdlets();
        var mockConnection = CreateMockConnection("environmentvariabledefinition", "environmentvariablevalue");
        
        var definitionId = Guid.NewGuid();
        var definition = new Entity("environmentvariabledefinition")
        {
            Id = definitionId,
            ["schemaname"] = "new_whatifvar",
            ["displayname"] = "WhatIf Variable",
            ["type"] = new OptionSetValue(100000000)
        };
        Environment!.Seed(definition);
        
        // Act
        ps.AddCommand("Set-DataverseEnvironmentVariableValue")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SchemaName", "new_whatifvar")
          .AddParameter("Value", "whatif value")
          .AddParameter("WhatIf", true);
        ps.Invoke();

        // Assert - Should not error
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
    }

    // Get-DataverseComponentDependency Tests
    
    [Fact]
    public void GetDataverseEnvironmentVariableDefinition_RetrievesDefinitions()
    {
        // Arrange
        using var ps = CreatePowerShellWithMiscCmdlets();
        var mockConnection = CreateMockConnection("environmentvariabledefinition");
        
        var definition = new Entity("environmentvariabledefinition")
        {
            Id = Guid.NewGuid(),
            ["schemaname"] = "new_testvar",
            ["displayname"] = "Test Variable",
            ["type"] = new OptionSetValue(100000000)
        };
        Environment!.Seed(definition);
        
        // Act
        ps.AddCommand("Get-DataverseEnvironmentVariableDefinition")
          .AddParameter("Connection", mockConnection);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
    }

    [Fact]
    public void GetDataverseEnvironmentVariableValue_RetrievesValues()
    {
        // Arrange
        using var ps = CreatePowerShellWithMiscCmdlets();
        var mockConnection = CreateMockConnection("environmentvariabledefinition", "environmentvariablevalue");
        
        var definitionId = Guid.NewGuid();
        var definition = new Entity("environmentvariabledefinition")
        {
            Id = definitionId,
            ["schemaname"] = "new_getvaluevar",
            ["displayname"] = "Get Value Variable"
        };
        
        var value = new Entity("environmentvariablevalue")
        {
            Id = Guid.NewGuid(),
            ["environmentvariabledefinitionid"] = new EntityReference("environmentvariabledefinition", definitionId),
            ["value"] = "test value"
        };
        Environment!.Seed(definition, value);
        
        // Act
        ps.AddCommand("Get-DataverseEnvironmentVariableValue")
          .AddParameter("Connection", mockConnection);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveDataverseEnvironmentVariableValue_RemovesValue()
    {
        // Arrange
        using var ps = CreatePowerShellWithMiscCmdlets();
        var mockConnection = CreateMockConnection("environmentvariabledefinition", "environmentvariablevalue");
        
        var definitionId = Guid.NewGuid();
        var definition = new Entity("environmentvariabledefinition")
        {
            Id = definitionId,
            ["schemaname"] = "new_deletevar",
            ["displayname"] = "Delete Variable"
        };
        
        var valueId = Guid.NewGuid();
        var value = new Entity("environmentvariablevalue")
        {
            Id = valueId,
            ["environmentvariabledefinitionid"] = new EntityReference("environmentvariabledefinition", definitionId),
            ["value"] = "to be deleted"
        };
        Environment!.Seed(definition, value);
        
        // Act - Remove by SchemaName (the cmdlet requires SchemaName, not Id)
        ps.AddCommand("Remove-DataverseEnvironmentVariableValue")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SchemaName", "new_deletevar")
          .AddParameter("Confirm", false);
        ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
    }

    // Argument Completers Tests
    // Note: Some completers require a ServiceClient connection to query Dataverse metadata.
    // Tests that need live connection data are tested via E2E tests.
    // Here we test:
    // 1. ComponentTypeArgumentCompleter.TryParse works with static data
    // 2. Completers don't throw exceptions when invoked without a connection

    [Fact]
    public void ArgumentCompleter_ComponentType_TryParse_ParsesNumericValues()
    {
        // ComponentTypeArgumentCompleter.TryParse should parse numeric component type values
        var success = ComponentTypeArgumentCompleter.TryParse("1", out int result);
        
        success.Should().BeTrue("should parse numeric value '1'");
        result.Should().Be(1, "should return component type 1 (Entity)");
    }

    [Fact]
    public void ArgumentCompleter_ComponentType_TryParse_ParsesFriendlyNames()
    {
        // ComponentTypeArgumentCompleter.TryParse should parse friendly names
        var success = ComponentTypeArgumentCompleter.TryParse("Entity", out int result);
        
        success.Should().BeTrue("should parse friendly name 'Entity'");
        result.Should().Be(1, "should return component type 1 (Entity)");
    }

    [Fact]
    public void ArgumentCompleter_ComponentType_TryParse_ParsesAliases()
    {
        // ComponentTypeArgumentCompleter.TryParse should parse alternative names
        var success = ComponentTypeArgumentCompleter.TryParse("Table", out int result);
        
        success.Should().BeTrue("should parse alias 'Table'");
        result.Should().Be(1, "Table should map to component type 1 (Entity)");
    }

    [Fact]
    public void ArgumentCompleter_ComponentType_TryParse_IsCaseInsensitive()
    {
        // ComponentTypeArgumentCompleter.TryParse should be case-insensitive
        var success1 = ComponentTypeArgumentCompleter.TryParse("entity", out int result1);
        var success2 = ComponentTypeArgumentCompleter.TryParse("ENTITY", out int result2);
        var success3 = ComponentTypeArgumentCompleter.TryParse("Entity", out int result3);
        
        success1.Should().BeTrue("should parse lowercase 'entity'");
        success2.Should().BeTrue("should parse uppercase 'ENTITY'");
        success3.Should().BeTrue("should parse mixed case 'Entity'");
        result1.Should().Be(1);
        result2.Should().Be(1);
        result3.Should().Be(1);
    }

    [Fact]
    public void ArgumentCompleter_ComponentType_TryParse_ReturnsFalseForInvalidInput()
    {
        // ComponentTypeArgumentCompleter.TryParse should return false for invalid input
        var success1 = ComponentTypeArgumentCompleter.TryParse("InvalidComponentType", out _);
        var success2 = ComponentTypeArgumentCompleter.TryParse("", out _);
        var success3 = ComponentTypeArgumentCompleter.TryParse(null!, out _);
        
        success1.Should().BeFalse("should return false for 'InvalidComponentType'");
        success2.Should().BeFalse("should return false for empty string");
        success3.Should().BeFalse("should return false for null");
    }

    [Fact]
    public void ArgumentCompleter_ComponentType_TryParse_ParsesCommonComponentTypes()
    {
        // Verify common component types can be parsed
        var testCases = new Dictionary<string, int>
        {
            { "1", 1 },       // Entity by number
            { "Entity", 1 },  // Entity by name
            { "Attribute", 2 },
            { "Column", 2 },  // Attribute alias
            { "Form", 48 },   // SystemForm
            { "WebResource", 50 },
            { "PluginAssembly", 60 },
            { "Plugin", 60 }, // PluginAssembly alias
            { "AppModule", 152 },
            { "App", 152 },   // AppModule alias
        };
        
        foreach (var kvp in testCases)
        {
            var success = ComponentTypeArgumentCompleter.TryParse(kvp.Key, out int result);
            success.Should().BeTrue($"should parse '{kvp.Key}'");
            result.Should().Be(kvp.Value, $"'{kvp.Key}' should map to {kvp.Value}");
        }
    }

    [Fact]
    public void ArgumentCompleter_TableName_DoesNotThrow_WhenNoConnection()
    {
        // TableNameArgumentCompleter should not throw when invoked without a connection
        var completer = new TableNameArgumentCompleter();
        
        // Act & Assert - Should not throw
        Action act = () => completer.CompleteArgument(
            "Get-DataverseRecord",
            "TableName",
            "con",
            null!,
            new Hashtable());
        
        act.Should().NotThrow("completer should handle missing connection gracefully");
    }

    [Fact]
    public void ArgumentCompleter_ColumnName_DoesNotThrow_WhenNoConnection()
    {
        // ColumnNameArgumentCompleter should not throw when invoked without a connection
        var completer = new ColumnNameArgumentCompleter();
        
        // Act & Assert - Should not throw
        Action act = () => completer.CompleteArgument(
            "Get-DataverseRecord",
            "ColumnName",
            "first",
            null!,
            new Hashtable());
        
        act.Should().NotThrow("completer should handle missing connection gracefully");
    }

    [Fact]
    public void ArgumentCompleter_FilterValues_DoesNotThrow_WhenNoConnection()
    {
        // FilterValuesArgumentCompleter should not throw when invoked without a connection
        var completer = new FilterValuesArgumentCompleter();
        
        // Act & Assert - Should not throw
        Action act = () => completer.CompleteArgument(
            "Get-DataverseRecord",
            "FilterValues",
            "name",
            null!,
            new Hashtable { { "TableName", "contact" } });
        
        act.Should().NotThrow("completer should handle missing connection gracefully");
    }

    [Fact]
    public void ArgumentCompleter_Links_DoesNotThrow_WhenNoConnection()
    {
        // LinksArgumentCompleter should not throw when invoked without a connection
        var completer = new LinksArgumentCompleter();
        
        // Act & Assert - Should not throw
        Action act = () => completer.CompleteArgument(
            "Get-DataverseRecord",
            "Links",
            "acc",
            null!,
            new Hashtable { { "TableName", "contact" } });
        
        act.Should().NotThrow("completer should handle missing connection gracefully");
    }

    [Fact]
    public void ArgumentCompleter_WebResourceName_DoesNotThrow_WhenNoConnection()
    {
        // WebResourceNameArgumentCompleter should not throw when invoked without a connection
        var completer = new WebResourceNameArgumentCompleter();
        
        // Act & Assert - Should not throw
        Action act = () => completer.CompleteArgument(
            "Set-DataverseWebResource",
            "Name",
            "new_",
            null!,
            new Hashtable());
        
        act.Should().NotThrow("completer should handle missing connection gracefully");
    }

    [Fact]
    public void ArgumentCompleter_FormId_DoesNotThrow_WhenNoConnection()
    {
        // FormIdArgumentCompleter should not throw when invoked without a connection
        var completer = new FormIdArgumentCompleter();
        
        // Act & Assert - Should not throw
        Action act = () => completer.CompleteArgument(
            "Set-DataverseFormTab",
            "FormId",
            "",
            null!,
            new Hashtable());
        
        act.Should().NotThrow("completer should handle missing connection gracefully");
    }

    [Fact]
    public void ArgumentCompleter_FormName_DoesNotThrow_WhenNoConnection()
    {
        // FormNameArgumentCompleter should not throw when invoked without a connection
        var completer = new FormNameArgumentCompleter();
        
        // Act & Assert - Should not throw
        Action act = () => completer.CompleteArgument(
            "Get-DataverseFormXml",
            "FormName",
            "Main",
            null!,
            new Hashtable { { "TableName", "contact" } });
        
        act.Should().NotThrow("completer should handle missing connection gracefully");
    }

    [Fact]
    public void ArgumentCompleter_FormTabName_DoesNotThrow_WhenNoConnection()
    {
        // FormTabNameArgumentCompleter should not throw when invoked without a connection
        var completer = new FormTabNameArgumentCompleter();
        
        // Act & Assert - Should not throw
        Action act = () => completer.CompleteArgument(
            "Set-DataverseFormSection",
            "TabName",
            "General",
            null!,
            new Hashtable { { "FormId", Guid.NewGuid().ToString() } });
        
        act.Should().NotThrow("completer should handle missing connection gracefully");
    }

    [Fact]
    public void ArgumentCompleter_FormSectionName_DoesNotThrow_WhenNoConnection()
    {
        // FormSectionNameArgumentCompleter should not throw when invoked without a connection
        var completer = new FormSectionNameArgumentCompleter();
        
        // Act & Assert - Should not throw
        Action act = () => completer.CompleteArgument(
            "Set-DataverseFormControl",
            "SectionName",
            "Details",
            null!,
            new Hashtable { { "FormId", Guid.NewGuid().ToString() }, { "TabName", "General" } });
        
        act.Should().NotThrow("completer should handle missing connection gracefully");
    }

    [Fact]
    public void ArgumentCompleter_FormControlId_DoesNotThrow_WhenNoConnection()
    {
        // FormControlIdArgumentCompleter should not throw when invoked without a connection
        var completer = new FormControlIdArgumentCompleter();
        
        // Act & Assert - Should not throw
        Action act = () => completer.CompleteArgument(
            "Set-DataverseFormControl",
            "ControlId",
            "firstname",
            null!,
            new Hashtable
            {
                { "FormId", Guid.NewGuid().ToString() },
                { "TabName", "General" },
                { "SectionName", "Details" }
            });
        
        act.Should().NotThrow("completer should handle missing connection gracefully");
    }

    [Fact]
    public void ArgumentCompleter_ComponentType_DoesNotThrow_WhenInvoked()
    {
        // ComponentTypeArgumentCompleter should not throw when invoked
        var completer = new ComponentTypeArgumentCompleter();
        
        // Act & Assert - Should not throw
        Action act = () => completer.CompleteArgument(
            "Set-DataverseSolutionComponent",
            "ComponentType",
            "",
            null!,
            null!);
        
        act.Should().NotThrow("completer should not throw when invoked");
    }
}
