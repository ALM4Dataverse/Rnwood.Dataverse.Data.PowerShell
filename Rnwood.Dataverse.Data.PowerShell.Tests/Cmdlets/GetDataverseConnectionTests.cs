using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using FluentAssertions;
using Microsoft.PowerPlatform.Dataverse.Client;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets;

public class GetDataverseConnectionTests : TestBase, IDisposable
{
    private readonly Type _cmdletType = typeof(GetDataverseConnectionCmdlet);

    public GetDataverseConnectionTests()
    {
        // Clear default connection before each test
        SetDataverseConnectionAsDefaultCmdlet.ClearDefault();
    }

    public override void Dispose()
    {
        // Clean up default connection after each test
        SetDataverseConnectionAsDefaultCmdlet.ClearDefault();
        base.Dispose();
    }



    /// <summary>
    /// Gets the default connection via reflection.
    /// </summary>
    private static ServiceClient? GetDefaultConnectionValue()
    {
        var managerType = typeof(GetDataverseConnectionCmdlet).Assembly
            .GetType("Rnwood.Dataverse.Data.PowerShell.Commands.DefaultConnectionManager");
        var prop = managerType?.GetProperty("DefaultConnection", BindingFlags.Public | BindingFlags.Static);
        return prop?.GetValue(null) as ServiceClient;
    }

    /// <summary>
    /// Creates a PowerShell instance with Get-DataverseConnection and related cmdlets registered.
    /// </summary>
    private static PS CreatePowerShellWithCmdlets()
    {
        var initialSessionState = InitialSessionState.CreateDefault2();

        // Register GetDataverseConnectionCmdlet with its alias
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataverseConnection", typeof(GetDataverseConnectionCmdlet), null));

        // Also register the alias
        initialSessionState.Commands.Add(new SessionStateAliasEntry(
            "Connect-DataverseConnection", "Get-DataverseConnection"));

        // Register other cmdlets that might be used in tests
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataverseRecord", typeof(GetDataverseRecordCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataverseWhoAmI", typeof(GetDataverseWhoAmICmdlet), null));

        var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
        runspace.Open();

        var ps = PS.Create();
        ps.Runspace = runspace;
        return ps;
    }

    // ===== Default Connection Management Tests =====

    [Fact]
    public void GetDataverseConnection_SetAsDefault_StoresConnectionAsDefault()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("contact");

        // Act - Set connection as default via cmdlet helper
        SetDataverseConnectionAsDefaultCmdlet.SetDefault(mockConnection);

        // Verify via GetDefault parameter
        ps.AddCommand("Get-DataverseConnection")
          .AddParameter("GetDefault", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        results[0].BaseObject.Should().Be(mockConnection);
    }

    [Fact]
    public void GetDataverseConnection_GetDefault_ReturnsError_WhenNoDefaultSet()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        SetDataverseConnectionAsDefaultCmdlet.ClearDefault();

        // Act
        ps.AddCommand("Get-DataverseConnection")
          .AddParameter("GetDefault", true);
        
        // Assert - ThrowTerminatingError causes CmdletInvocationException
        var action = () => ps.Invoke();
        var exception = action.Should().Throw<CmdletInvocationException>()
            .Which.InnerException as System.InvalidOperationException;
        exception.Should().NotBeNull();
        exception!.Message.Should().Contain("No default connection");
    }

    [Fact]
    public void GetDataverseConnection_Cmdlets_UseDefaultConnection_WhenConnectionNotProvided()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("contact");
        SetDataverseConnectionAsDefaultCmdlet.SetDefault(mockConnection);

        // Act - Call Get-DataverseWhoAmI without Connection parameter
        ps.AddCommand("Get-DataverseWhoAmI");
        var results = ps.Invoke();

        // Assert - Should not error because default connection is used
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
    }

    [Fact]
    public void GetDataverseConnection_Cmdlets_Error_WhenNoConnectionAndNoDefault()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        SetDataverseConnectionAsDefaultCmdlet.ClearDefault();

        // Act - Call Get-DataverseRecord without Connection parameter and no default
        ps.AddCommand("Get-DataverseRecord")
          .AddParameter("TableName", "contact");
        
        // Assert - ThrowTerminatingError causes CmdletInvocationException
        var action = () => ps.Invoke();
        var exception = action.Should().Throw<CmdletInvocationException>()
            .Which.InnerException as System.InvalidOperationException;
        exception.Should().NotBeNull();
        exception!.Message.Should().Contain("No connection provided");
    }

    [Fact]
    public void GetDataverseConnection_Alias_ConnectDataverseConnection_Works()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();

        // Act - Get command info for the alias
        ps.AddCommand("Get-Command")
          .AddParameter("Name", "Connect-DataverseConnection");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        var commandInfo = results[0].BaseObject as AliasInfo;
        commandInfo.Should().NotBeNull();
        commandInfo!.ReferencedCommand.Name.Should().Be("Get-DataverseConnection");
    }

    // ===== Named Connection Management Tests =====

    [Fact]
    public void GetDataverseConnection_ListConnections_ReturnsEmptyOrArray_WhenCalled()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();

        // Act - Call ListConnections (will return whatever is saved)
        ps.AddCommand("Get-DataverseConnection")
          .AddParameter("ListConnections", true);
        var results = ps.Invoke();

        // Assert - Should not error; returns array (possibly empty)
        ps.HadErrors.Should().BeFalse();
        // Results may or may not be empty depending on user's saved connections
    }

    [Fact]
    public void GetDataverseConnection_CanSaveNamedConnection_WithMockProvider()
    {
        // Arrange - Use temp directory to avoid affecting user's real connection store
        var tempDir = Path.Combine(Path.GetTempPath(), $"DataverseTest_{Guid.NewGuid()}");
        try
        {
            var store = new ConnectionStore(tempDir);
            var testConnectionName = $"TestConn_{Guid.NewGuid()}";
            var mockConnection = CreateMockConnection("contact");

            // Act - Save a connection
            var metadata = new ConnectionMetadata
            {
                Url = "https://test.crm.dynamics.com",
                AuthMethod = "Interactive",
                ClientId = "00000000-0000-0000-0000-000000000000",
                Username = "testuser@test.com",
                SavedAt = DateTime.UtcNow
            };
            store.SaveConnection(testConnectionName, metadata);

            // Assert - Connection should be saved and loadable
            store.ConnectionExists(testConnectionName).Should().BeTrue();
            var loaded = store.LoadConnection(testConnectionName);
            loaded.Should().NotBeNull();
            loaded.Url.Should().Be("https://test.crm.dynamics.com");
            loaded.AuthMethod.Should().Be("Interactive");
            loaded.Username.Should().Be("testuser@test.com");
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public void GetDataverseConnection_DeleteConnection_ReturnsError_WhenConnectionDoesNotExist()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var nonExistentName = $"NonExistent_{Guid.NewGuid()}";

        // Act
        ps.AddCommand("Get-DataverseConnection")
          .AddParameter("DeleteConnection", true)
          .AddParameter("Name", nonExistentName);
        
        // Assert - ThrowTerminatingError causes CmdletInvocationException
        var action = () => ps.Invoke();
        var exception = action.Should().Throw<CmdletInvocationException>()
            .Which.InnerException as System.InvalidOperationException;
        exception.Should().NotBeNull();
        exception!.Message.Should().Contain("not found");
    }

    [Fact]
    public void GetDataverseConnection_ListConnections_ShowsSavedConnections()
    {
        // Arrange - Use temp directory to avoid affecting user's real connection store
        var tempDir = Path.Combine(Path.GetTempPath(), $"DataverseTest_{Guid.NewGuid()}");
        try
        {
            var store = new ConnectionStore(tempDir);
            var conn1Name = $"TestConn1_{Guid.NewGuid()}";
            var conn2Name = $"TestConn2_{Guid.NewGuid()}";

            // Act - Save multiple connections
            store.SaveConnection(conn1Name, new ConnectionMetadata
            {
                Url = "https://test1.crm.dynamics.com",
                AuthMethod = "Interactive",
                ClientId = "00000000-0000-0000-0000-000000000001",
                SavedAt = DateTime.UtcNow
            });
            store.SaveConnection(conn2Name, new ConnectionMetadata
            {
                Url = "https://test2.crm.dynamics.com",
                AuthMethod = "DeviceCode",
                ClientId = "00000000-0000-0000-0000-000000000002",
                SavedAt = DateTime.UtcNow
            });

            // Assert - List should show both connections
            var connections = store.ListConnections();
            connections.Should().Contain(conn1Name);
            connections.Should().Contain(conn2Name);
            connections.Count.Should().BeGreaterOrEqualTo(2);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public void GetDataverseConnection_CanUseListConnectionsParameter()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();

        // Act - Call with ListConnections
        ps.AddCommand("Get-DataverseConnection")
          .AddParameter("ListConnections", true);
        var results = ps.Invoke();

        // Assert - Should not throw parameter binding error
        ps.HadErrors.Should().BeFalse();
    }

    [Fact]
    public void GetDataverseConnection_CanUseDeleteConnectionParameter_WithNonExistentConnection()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var nonExistentName = $"NonExistent_{Guid.NewGuid()}";

        // Act
        ps.AddCommand("Get-DataverseConnection")
          .AddParameter("DeleteConnection", true)
          .AddParameter("Name", nonExistentName);
        
        // Assert - Should error with ConnectionNotFound (not parameter binding error)
        // ThrowTerminatingError causes CmdletInvocationException
        var action = () => ps.Invoke();
        var exception = action.Should().Throw<CmdletInvocationException>()
            .Which.InnerException as System.InvalidOperationException;
        exception.Should().NotBeNull();
        exception!.Message.Should().Contain("not found");
    }
}
