using Xunit;
using Microsoft.Xrm.Sdk;
using Microsoft.Crm.Sdk.Messages;
using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using AwesomeAssertions;
using Microsoft.PowerPlatform.Dataverse.Client;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets;

public class GetDataverseWhoAmITests : TestBase, IDisposable
{
    public GetDataverseWhoAmITests()
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
        var initialSessionState = InitialSessionState.CreateDefault();
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataverseWhoAmI", typeof(GetDataverseWhoAmICmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Set-DataverseConnectionAsDefault", typeof(SetDataverseConnectionAsDefaultCmdlet), null));

        var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
        runspace.Open();

        var ps = PS.Create();
        ps.Runspace = runspace;
        return ps;
    }
    [Fact]
    public void GetDataverseWhoAmI_ReturnsIdentityInformation()
    {
        // Arrange
        var mockConnection = CreateMockConnection();

        // Act
        var request = new WhoAmIRequest();
        var response = (WhoAmIResponse)Service!.Execute(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotEqual(Guid.Empty, response.UserId);
        Assert.NotEqual(Guid.Empty, response.BusinessUnitId);
        Assert.NotEqual(Guid.Empty, response.OrganizationId);
    }

    [Fact]
    public void GetDataverseWhoAmI_ReturnsConsistentIdentity()
    {
        // Arrange
        var mockConnection = CreateMockConnection();

        // Act
        var response1 = (WhoAmIResponse)Service!.Execute(new WhoAmIRequest());
        var response2 = (WhoAmIResponse)Service.Execute(new WhoAmIRequest());

        // Assert
        Assert.Equal(response1.UserId, response2.UserId);
        Assert.Equal(response1.BusinessUnitId, response2.BusinessUnitId);
        Assert.Equal(response1.OrganizationId, response2.OrganizationId);
    }

    [Fact]
    public void GetDataverseWhoAmI_WorksWithDefaultConnection()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("contact");

        // Set default connection via reflection (avoids test parallelization interference)
        SetDataverseConnectionAsDefaultCmdlet.SetDefault(mockConnection);

        // Act - invoke Get-DataverseWhoAmI without -Connection parameter
        ps.AddCommand("Get-DataverseWhoAmI");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        var response = results[0].BaseObject as WhoAmIResponse;
        response.Should().NotBeNull();
        response!.UserId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void GetDataverseWhoAmI_IsReadOnly_DoesNotModifyData()
    {
        // Arrange
        var mockConnection = CreateMockConnection("contact");

        var contact = new Entity("contact")
        {
            Id = Guid.NewGuid(),
            ["firstname"] = "Test",
            ["lastname"] = "WhoAmI"
        };
        Service!.Create(contact);

        // Act
        var whoAmIResponse = (WhoAmIResponse)Service.Execute(new WhoAmIRequest());

        // Assert - WhoAmI executed successfully
        Assert.NotEqual(Guid.Empty, whoAmIResponse.UserId);

        // Verify contact was not affected
        var retrievedContact = Service.Retrieve("contact", contact.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("firstname", "lastname"));
        Assert.Equal("Test", retrievedContact["firstname"]);
        Assert.Equal("WhoAmI", retrievedContact["lastname"]);

        // Verify no additional records created
        var query = new Microsoft.Xrm.Sdk.Query.QueryExpression("contact");
        query.ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet("contactid");
        var contacts = Service.RetrieveMultiple(query).Entities;
        Assert.Single(contacts);
    }
}
