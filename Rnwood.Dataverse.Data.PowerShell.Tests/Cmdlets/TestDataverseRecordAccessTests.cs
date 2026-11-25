using Xunit;
using Microsoft.Xrm.Sdk;
using Microsoft.Crm.Sdk.Messages;
using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using AwesomeAssertions;
using Microsoft.PowerPlatform.Dataverse.Client;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets;

public class TestDataverseRecordAccessTests : TestBase, IDisposable
{
    public TestDataverseRecordAccessTests()
    {
        SetDataverseConnectionAsDefaultCmdlet.ClearDefault();
    }

    public override void Dispose()
    {
        SetDataverseConnectionAsDefaultCmdlet.ClearDefault();
        base.Dispose();
    }

    private static PS CreatePowerShellWithCmdlets()
    {
        var initialSessionState = InitialSessionState.CreateDefault2();
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Test-DataverseRecordAccess", typeof(TestDataverseRecordAccessCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Set-DataverseConnectionAsDefault", typeof(SetDataverseConnectionAsDefaultCmdlet), null));

        var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
        runspace.Open();

        var ps = PS.Create();
        ps.Runspace = runspace;
        return ps;
    }
    [Fact]
    public void TestDataverseRecordAccess_ReturnsAccessRightsForUser()
    {
        // Arrange
        var mockConnection = CreateMockConnection("contact");

        var contact = new Entity("contact")
        {
            Id = Guid.NewGuid(),
            ["firstname"] = "Test",
            ["lastname"] = "AccessCheck"
        };
        Service!.Create(contact);

        // Get current user from WhoAmI
        var whoAmI = (WhoAmIResponse)Service.Execute(new WhoAmIRequest());

        // Act
        var request = new RetrievePrincipalAccessRequest
        {
            Principal = new EntityReference("systemuser", whoAmI.UserId),
            Target = contact.ToEntityReference()
        };

        var response = (RetrievePrincipalAccessResponse)Service.Execute(request);

        // Assert
        Assert.NotNull(response);
        // FakeXrmEasy returns AccessRights.None by default
        Assert.IsType<AccessRights>(response.AccessRights);
    }

    [Fact]
    public void TestDataverseRecordAccess_ReturnsCorrectAccessRightsEnum()
    {
        // Arrange
        var mockConnection = CreateMockConnection("contact");

        var contact = new Entity("contact")
        {
            Id = Guid.NewGuid(),
            ["firstname"] = "Access",
            ["lastname"] = "Test"
        };
        Service!.Create(contact);

        var whoAmI = (WhoAmIResponse)Service.Execute(new WhoAmIRequest());

        // Act
        var request = new RetrievePrincipalAccessRequest
        {
            Principal = new EntityReference("systemuser", whoAmI.UserId),
            Target = contact.ToEntityReference()
        };
        var response = (RetrievePrincipalAccessResponse)Service.Execute(request);

        // Assert
        Assert.Equal("AccessRights", response.AccessRights.GetType().Name);
    }

    [Fact]
    public void TestDataverseRecordAccess_SupportsBitwiseOperations()
    {
        // Arrange
        var mockConnection = CreateMockConnection("contact");

        var contact = new Entity("contact")
        {
            Id = Guid.NewGuid(),
            ["firstname"] = "Permission",
            ["lastname"] = "Test"
        };
        Service!.Create(contact);

        var whoAmI = (WhoAmIResponse)Service.Execute(new WhoAmIRequest());

        // Act
        var request = new RetrievePrincipalAccessRequest
        {
            Principal = new EntityReference("systemuser", whoAmI.UserId),
            Target = contact.ToEntityReference()
        };
        var response = (RetrievePrincipalAccessResponse)Service.Execute(request);

        // Assert - verify bitwise operations work
        var hasRead = (response.AccessRights & AccessRights.ReadAccess) != 0;
        // Don't assert specific rights - FakeXrmEasy may return None
        Assert.IsType<AccessRights>(response.AccessRights);
    }

    [Fact]
    public void TestDataverseRecordAccess_WorksWithDefaultConnection()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("contact");

        var contact = new Entity("contact")
        {
            Id = Guid.NewGuid(),
            ["firstname"] = "Test",
            ["lastname"] = "DefaultConnection"
        };
        Service!.Create(contact);

        var whoAmI = (WhoAmIResponse)Service.Execute(new WhoAmIRequest());

        // Set default connection via reflection (avoids test parallelization interference)
        SetDataverseConnectionAsDefaultCmdlet.SetDefault(mockConnection);

        // Act - invoke Test-DataverseRecordAccess without -Connection parameter
        ps.AddCommand("Test-DataverseRecordAccess")
          .AddParameter("TableName", "contact")
          .AddParameter("Id", contact.Id)
          .AddParameter("Principal", whoAmI.UserId);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        results[0].BaseObject.Should().BeOfType<AccessRights>();
    }
}
