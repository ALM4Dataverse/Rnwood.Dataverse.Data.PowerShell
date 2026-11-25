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

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets;

/// <summary>
/// Tests for View-related cmdlets:
/// - Get-DataverseView
/// - Set-DataverseView
/// - Remove-DataverseView
/// </summary>
public class ViewsTests : TestBase
{
    private PS CreatePowerShellWithViewCmdlets()
    {
        var initialSessionState = InitialSessionState.CreateDefault2();
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataverseView", typeof(GetDataverseViewCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Set-DataverseView", typeof(SetDataverseViewCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Remove-DataverseView", typeof(RemoveDataverseViewCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataverseRecord", typeof(GetDataverseRecordCmdlet), null));
        
        var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
        runspace.Open();
        
        var ps = PS.Create();
        ps.Runspace = runspace;
        return ps;
    }

    #region Get-DataverseView Tests

    [Fact]
    public void GetDataverseView_RetrievesSystemViewById()
    {
        // Arrange
        using var ps = CreatePowerShellWithViewCmdlets();
        var mockConnection = CreateMockConnection("savedquery", "contact");
        
        var viewId = Guid.NewGuid();
        var view = new Entity("savedquery")
        {
            Id = viewId,
            ["savedqueryid"] = viewId,
            ["name"] = "Active Contacts",
            ["returnedtypecode"] = "contact",
            ["fetchxml"] = "<fetch><entity name='contact'><attribute name='fullname'/></entity></fetch>",
            ["layoutxml"] = "<grid><row><cell name='fullname' width='200'/></row></grid>",
            ["querytype"] = 0
        };
        Service!.Create(view);

        // Act
        ps.AddCommand("Get-DataverseView")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", viewId);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        results[0].Properties["Id"].Value.Should().Be(viewId);
        results[0].Properties["Name"].Value.Should().Be("Active Contacts");
        results[0].Properties["ViewType"].Value.Should().Be("System");
    }

    [Fact]
    public void GetDataverseView_RetrievesPersonalViewById()
    {
        // Arrange
        using var ps = CreatePowerShellWithViewCmdlets();
        var mockConnection = CreateMockConnection("userquery", "contact");
        
        var viewId = Guid.NewGuid();
        var view = new Entity("userquery")
        {
            Id = viewId,
            ["userqueryid"] = viewId,
            ["name"] = "My Contacts View",
            ["returnedtypecode"] = "contact",
            ["fetchxml"] = "<fetch><entity name='contact'><attribute name='fullname'/></entity></fetch>",
            ["layoutxml"] = "<grid><row><cell name='fullname' width='200'/></row></grid>",
            ["querytype"] = 0
        };
        Service!.Create(view);

        // Act
        ps.AddCommand("Get-DataverseView")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", viewId)
          .AddParameter("ViewType", "Personal");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        results[0].Properties["Id"].Value.Should().Be(viewId);
        results[0].Properties["Name"].Value.Should().Be("My Contacts View");
        results[0].Properties["ViewType"].Value.Should().Be("Personal");
    }

    [Fact]
    public void GetDataverseView_RetrievesViewsByTableName()
    {
        // Arrange
        using var ps = CreatePowerShellWithViewCmdlets();
        var mockConnection = CreateMockConnection("savedquery", "contact");
        
        var view1 = new Entity("savedquery")
        {
            Id = Guid.NewGuid(),
            ["savedqueryid"] = Guid.NewGuid(),
            ["name"] = "Active Contacts",
            ["returnedtypecode"] = "contact",
            ["fetchxml"] = "<fetch><entity name='contact'><attribute name='fullname'/></entity></fetch>",
            ["layoutxml"] = "<grid><row><cell name='fullname' width='200'/></row></grid>",
            ["querytype"] = 0
        };
        view1["savedqueryid"] = view1.Id;
        
        var view2 = new Entity("savedquery")
        {
            Id = Guid.NewGuid(),
            ["savedqueryid"] = Guid.NewGuid(),
            ["name"] = "Inactive Contacts",
            ["returnedtypecode"] = "contact",
            ["fetchxml"] = "<fetch><entity name='contact'><attribute name='fullname'/></entity></fetch>",
            ["layoutxml"] = "<grid><row><cell name='fullname' width='200'/></row></grid>",
            ["querytype"] = 0
        };
        view2["savedqueryid"] = view2.Id;
        
        Service!.Create(view1);
        Service!.Create(view2);

        // Act
        ps.AddCommand("Get-DataverseView")
          .AddParameter("Connection", mockConnection)
          .AddParameter("TableName", "contact")
          .AddParameter("ViewType", "System");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(2);
        results.All(r => r.Properties["TableName"].Value.Equals("contact")).Should().BeTrue();
    }

    [Fact]
    public void GetDataverseView_RetrievesViewsByNameWithWildcard()
    {
        // Arrange
        using var ps = CreatePowerShellWithViewCmdlets();
        var mockConnection = CreateMockConnection("savedquery", "contact");
        
        var view1 = new Entity("savedquery")
        {
            Id = Guid.NewGuid(),
            ["name"] = "Active Contacts",
            ["returnedtypecode"] = "contact",
            ["fetchxml"] = "<fetch><entity name='contact'><attribute name='fullname'/></entity></fetch>",
            ["layoutxml"] = "<grid><row><cell name='fullname' width='200'/></row></grid>",
            ["querytype"] = 0
        };
        view1["savedqueryid"] = view1.Id;
        
        var view2 = new Entity("savedquery")
        {
            Id = Guid.NewGuid(),
            ["name"] = "Active Accounts",
            ["returnedtypecode"] = "account",
            ["fetchxml"] = "<fetch><entity name='account'><attribute name='name'/></entity></fetch>",
            ["layoutxml"] = "<grid><row><cell name='name' width='200'/></row></grid>",
            ["querytype"] = 0
        };
        view2["savedqueryid"] = view2.Id;
        
        Service!.Create(view1);
        Service!.Create(view2);

        // Act
        ps.AddCommand("Get-DataverseView")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Name", "Active*")
          .AddParameter("ViewType", "System");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(2);
        results.All(r => r.Properties["Name"].Value.ToString().StartsWith("Active")).Should().BeTrue();
    }

    [Fact]
    public void GetDataverseView_ReturnsColumnsWithWidthInfo()
    {
        // Arrange
        using var ps = CreatePowerShellWithViewCmdlets();
        var mockConnection = CreateMockConnection("savedquery", "contact");
        
        var viewId = Guid.NewGuid();
        var view = new Entity("savedquery")
        {
            Id = viewId,
            ["savedqueryid"] = viewId,
            ["name"] = "Contacts with Columns",
            ["returnedtypecode"] = "contact",
            ["fetchxml"] = "<fetch><entity name='contact'><attribute name='fullname'/><attribute name='emailaddress1'/></entity></fetch>",
            ["layoutxml"] = "<grid><row><cell name='fullname' width='200'/><cell name='emailaddress1' width='300'/></row></grid>",
            ["querytype"] = 0
        };
        Service!.Create(view);

        // Act
        ps.AddCommand("Get-DataverseView")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", viewId);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        var columns = results[0].Properties["Columns"].Value as object[];
        columns.Should().NotBeNull();
        columns.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public void GetDataverseView_ReturnsRawValuesWhenRawSwitch()
    {
        // Arrange
        using var ps = CreatePowerShellWithViewCmdlets();
        var mockConnection = CreateMockConnection("savedquery", "contact");
        
        var viewId = Guid.NewGuid();
        var view = new Entity("savedquery")
        {
            Id = viewId,
            ["savedqueryid"] = viewId,
            ["name"] = "Raw Test View",
            ["returnedtypecode"] = "contact",
            ["fetchxml"] = "<fetch><entity name='contact'><attribute name='fullname'/></entity></fetch>",
            ["layoutxml"] = "<grid><row><cell name='fullname' width='200'/></row></grid>",
            ["querytype"] = 0
        };
        Service!.Create(view);

        // Act
        ps.AddCommand("Get-DataverseView")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", viewId)
          .AddParameter("Raw", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        // Raw mode returns all entity attributes
        results[0].Properties["fetchxml"].Should().NotBeNull();
        results[0].Properties["layoutxml"].Should().NotBeNull();
    }

    #endregion

    #region Set-DataverseView Tests

    [Fact]
    public void SetDataverseView_CreatesNewSystemView()
    {
        // Arrange
        using var ps = CreatePowerShellWithViewCmdlets();
        var mockConnection = CreateMockConnection("savedquery", "contact");

        // Act
        ps.AddCommand("Set-DataverseView")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Name", "New Test View")
          .AddParameter("TableName", "contact")
          .AddParameter("Columns", new[] { "fullname", "emailaddress1" })
          .AddParameter("ViewType", "System")
          .AddParameter("PassThru", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        var viewId = (Guid)results[0].BaseObject;
        viewId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void SetDataverseView_CreatesNewPersonalView()
    {
        // Arrange
        using var ps = CreatePowerShellWithViewCmdlets();
        var mockConnection = CreateMockConnection("userquery", "contact");

        // Act
        ps.AddCommand("Set-DataverseView")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Name", "My Personal View")
          .AddParameter("TableName", "contact")
          .AddParameter("Columns", new[] { "fullname" })
          .AddParameter("ViewType", "Personal")
          .AddParameter("PassThru", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        var viewId = (Guid)results[0].BaseObject;
        viewId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void SetDataverseView_UpdatesExistingView()
    {
        // Arrange
        using var ps = CreatePowerShellWithViewCmdlets();
        var mockConnection = CreateMockConnection("savedquery", "contact");
        
        var viewId = Guid.NewGuid();
        var view = new Entity("savedquery")
        {
            Id = viewId,
            ["savedqueryid"] = viewId,
            ["name"] = "Original Name",
            ["returnedtypecode"] = "contact",
            ["fetchxml"] = "<fetch><entity name='contact'><attribute name='fullname'/></entity></fetch>",
            ["layoutxml"] = "<grid><row><cell name='fullname' width='200'/></row></grid>",
            ["querytype"] = 0
        };
        Service!.Create(view);

        // Act
        ps.AddCommand("Set-DataverseView")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", viewId)
          .AddParameter("Name", "Updated Name")
          .AddParameter("ViewType", "System")
          .AddParameter("PassThru", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        
        // Verify the view was updated
        ps.Commands.Clear();
        ps.AddCommand("Get-DataverseView")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", viewId);
        var updatedResults = ps.Invoke();
        
        updatedResults.Should().HaveCount(1);
        updatedResults[0].Properties["Name"].Value.Should().Be("Updated Name");
    }

    [Fact]
    public void SetDataverseView_CreatesViewWithDescription()
    {
        // Arrange
        using var ps = CreatePowerShellWithViewCmdlets();
        var mockConnection = CreateMockConnection("savedquery", "contact");

        // Act
        ps.AddCommand("Set-DataverseView")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Name", "View With Description")
          .AddParameter("TableName", "contact")
          .AddParameter("Description", "This is a test description")
          .AddParameter("Columns", new[] { "fullname" })
          .AddParameter("ViewType", "System")
          .AddParameter("PassThru", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
    }

    [Fact]
    public void SetDataverseView_CreatesViewWithOrderBy()
    {
        // Arrange
        using var ps = CreatePowerShellWithViewCmdlets();
        var mockConnection = CreateMockConnection("savedquery", "contact");

        // Act
        ps.AddCommand("Set-DataverseView")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Name", "Ordered View")
          .AddParameter("TableName", "contact")
          .AddParameter("Columns", new[] { "fullname", "createdon" })
          .AddParameter("OrderBy", new[] { "fullname", "createdon-" })
          .AddParameter("ViewType", "System")
          .AddParameter("PassThru", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
    }

    [Fact]
    public void SetDataverseView_CreatesViewWithFilter()
    {
        // Arrange
        using var ps = CreatePowerShellWithViewCmdlets();
        var mockConnection = CreateMockConnection("savedquery", "contact");
        
        var filter = new Hashtable { ["statecode"] = 0 };

        // Act
        ps.AddCommand("Set-DataverseView")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Name", "Filtered View")
          .AddParameter("TableName", "contact")
          .AddParameter("Columns", new[] { "fullname" })
          .AddParameter("FilterValues", new[] { filter })
          .AddParameter("ViewType", "System")
          .AddParameter("PassThru", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
    }

    [Fact]
    public void SetDataverseView_CreatesViewWithFetchXml()
    {
        // Arrange
        using var ps = CreatePowerShellWithViewCmdlets();
        var mockConnection = CreateMockConnection("savedquery", "contact");
        var fetchXml = "<fetch><entity name='contact'><attribute name='fullname'/><filter><condition attribute='statecode' operator='eq' value='0'/></filter></entity></fetch>";

        // Act
        ps.AddCommand("Set-DataverseView")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Name", "FetchXml View")
          .AddParameter("TableName", "contact")
          .AddParameter("FetchXml", fetchXml)
          .AddParameter("ViewType", "System")
          .AddParameter("PassThru", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
    }

    [Fact]
    public void SetDataverseView_UpdatesExistingViewByNameAndTableNameWithoutCreatingDuplicate()
    {
        // Arrange
        using var ps = CreatePowerShellWithViewCmdlets();
        var mockConnection = CreateMockConnection("savedquery", "contact");

        // Act - create with name/table only
        ps.AddCommand("Set-DataverseView")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Name", "No Duplicate View")
          .AddParameter("TableName", "contact")
          .AddParameter("Columns", new[] { "fullname", "emailaddress1" })
          .AddParameter("QueryType", "MainApplicationView")
          .AddParameter("Description", "First version")
          .AddParameter("ViewType", "System")
          .AddParameter("PassThru", true);
        var firstResults = ps.Invoke();

        ps.HadErrors.Should().BeFalse();
        firstResults.Should().HaveCount(1);
        var firstId = (Guid)firstResults[0].BaseObject;

        // Act - update with the same logical identity but no explicit ID
        ps.Commands.Clear();
        ps.AddCommand("Set-DataverseView")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Name", "No Duplicate View")
          .AddParameter("TableName", "contact")
          .AddParameter("Columns", new[] { "fullname", "emailaddress1", "telephone1" })
          .AddParameter("QueryType", "MainApplicationView")
          .AddParameter("Description", "Updated version")
          .AddParameter("ViewType", "System")
          .AddParameter("PassThru", true);
        var secondResults = ps.Invoke();

        // Assert - the second call should update, not create a duplicate
        ps.HadErrors.Should().BeFalse();
        secondResults.Should().HaveCount(1);
        var secondId = (Guid)secondResults[0].BaseObject;
        secondId.Should().Be(firstId);

        ps.Commands.Clear();
        ps.AddCommand("Get-DataverseView")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Name", "No Duplicate View")
          .AddParameter("TableName", "contact")
          .AddParameter("ViewType", "System");
        var checkResults = ps.Invoke();

        checkResults.Should().HaveCount(1);
        checkResults[0].Properties["Id"].Value.Should().Be(firstId);
        checkResults[0].Properties["Description"].Value.Should().Be("Updated version");
    }

    [Fact]
    public void SetDataverseView_UpdatesColumns_WhenExistingLayoutXmlIsEmpty()
    {
        // Arrange
        using var ps = CreatePowerShellWithViewCmdlets();
        var mockConnection = CreateMockConnection("savedquery", "contact");

        var viewId = Guid.NewGuid();
        var view = new Entity("savedquery")
        {
            Id = viewId,
            ["savedqueryid"] = viewId,
            ["name"] = "Empty Layout View",
            ["returnedtypecode"] = "contact",
            ["fetchxml"] = "<fetch><entity name='contact'><attribute name='fullname'/></entity></fetch>",
            ["layoutxml"] = string.Empty,
            ["querytype"] = 0
        };
        Service!.Create(view);

        // Act
        ps.AddCommand("Set-DataverseView")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", viewId)
          .AddParameter("ViewType", "System")
          .AddParameter("Columns", new object[]
          {
              new Hashtable { ["name"] = "fullname", ["width"] = 200 },
              new Hashtable { ["name"] = "emailaddress1", ["width"] = 150 },
              new Hashtable { ["name"] = "telephone1", ["width"] = 100 }
          })
          .AddParameter("PassThru", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        ((Guid)results[0].BaseObject).Should().Be(viewId);

        var updated = Service.Retrieve("savedquery", viewId, new Microsoft.Xrm.Sdk.Query.ColumnSet(true));
        updated.GetAttributeValue<string>("layoutxml").Should().Contain("fullname");
        updated.GetAttributeValue<string>("layoutxml").Should().Contain("emailaddress1");
        updated.GetAttributeValue<string>("layoutxml").Should().Contain("telephone1");
    }

    [Fact]
    public void SetDataverseView_NoUpdate_DoesNotModifyExistingView()
    {
        // Arrange
        using var ps = CreatePowerShellWithViewCmdlets();
        var mockConnection = CreateMockConnection("savedquery", "contact");
        
        var viewId = Guid.NewGuid();
        var view = new Entity("savedquery")
        {
            Id = viewId,
            ["savedqueryid"] = viewId,
            ["name"] = "Original Name",
            ["returnedtypecode"] = "contact",
            ["fetchxml"] = "<fetch><entity name='contact'><attribute name='fullname'/></entity></fetch>",
            ["layoutxml"] = "<grid><row><cell name='fullname' width='200'/></row></grid>",
            ["querytype"] = 0
        };
        Service!.Create(view);

        // Act
        ps.AddCommand("Set-DataverseView")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", viewId)
          .AddParameter("Name", "Should Not Update")
          .AddParameter("ViewType", "System")
          .AddParameter("NoUpdate", true)
          .AddParameter("PassThru", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        
        // Verify the view was NOT updated
        ps.Commands.Clear();
        ps.AddCommand("Get-DataverseView")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", viewId);
        var checkResults = ps.Invoke();
        
        checkResults.Should().HaveCount(1);
        checkResults[0].Properties["Name"].Value.Should().Be("Original Name");
    }

    [Fact]
    public void SetDataverseView_ThrowsError_WhenBothInsertBeforeAndInsertAfterSpecified()
    {
        // Arrange
        using var ps = CreatePowerShellWithViewCmdlets();
        var mockConnection = CreateMockConnection("savedquery", "contact");
        
        var viewId = Guid.NewGuid();
        var view = new Entity("savedquery")
        {
            Id = viewId,
            ["savedqueryid"] = viewId,
            ["name"] = "Test View",
            ["returnedtypecode"] = "contact",
            ["fetchxml"] = "<fetch><entity name='contact'><attribute name='fullname'/></entity></fetch>",
            ["layoutxml"] = "<grid><row><cell name='fullname' width='200'/></row></grid>",
            ["querytype"] = 0
        };
        Service!.Create(view);

        // Act
        ps.AddCommand("Set-DataverseView")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", viewId)
          .AddParameter("AddColumns", new[] { "emailaddress1" })
          .AddParameter("InsertColumnsBefore", "fullname")
          .AddParameter("InsertColumnsAfter", "fullname")
          .AddParameter("ViewType", "System");

        Exception? caughtException = null;
        try
        {
            ps.Invoke();
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        (ps.HadErrors || caughtException != null).Should().BeTrue();
    }

    [Fact]
    public void SetDataverseView_ThrowsError_WhenInsertBeforeUsedWithoutAddColumns()
    {
        // Arrange
        using var ps = CreatePowerShellWithViewCmdlets();
        var mockConnection = CreateMockConnection("savedquery", "contact");
        
        var viewId = Guid.NewGuid();
        var view = new Entity("savedquery")
        {
            Id = viewId,
            ["savedqueryid"] = viewId,
            ["name"] = "Test View",
            ["returnedtypecode"] = "contact",
            ["fetchxml"] = "<fetch><entity name='contact'><attribute name='fullname'/></entity></fetch>",
            ["layoutxml"] = "<grid><row><cell name='fullname' width='200'/></row></grid>",
            ["querytype"] = 0
        };
        Service!.Create(view);

        // Act
        ps.AddCommand("Set-DataverseView")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", viewId)
          .AddParameter("InsertColumnsBefore", "fullname")
          .AddParameter("ViewType", "System");

        Exception? caughtException = null;
        try
        {
            ps.Invoke();
        }
        catch (Exception ex)
        {
            caughtException = ex;
        }

        // Assert
        (ps.HadErrors || caughtException != null).Should().BeTrue();
    }

    [Fact]
    public void SetDataverseView_SupportsWhatIfWithoutCreatingView()
    {
        // Arrange
        using var ps = CreatePowerShellWithViewCmdlets();
        var mockConnection = CreateMockConnection("savedquery", "contact");

        // Act - use WhatIf
        ps.AddScript(@"
            param($connection)
            Set-DataverseView -Connection $connection -Name 'WhatIf Test' -TableName 'contact' -Columns 'fullname' -ViewType System -WhatIf
        ")
        .AddParameter("connection", mockConnection);
        var results = ps.Invoke();

        // Assert - no view should be created
        ps.Commands.Clear();
        ps.AddCommand("Get-DataverseView")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Name", "WhatIf Test")
          .AddParameter("ViewType", "System");
        var checkResults = ps.Invoke();
        
        checkResults.Should().BeEmpty();
    }

    #endregion

    #region Remove-DataverseView Tests

    [Fact]
    public void RemoveDataverseView_RemovesSystemViewById()
    {
        // Arrange
        using var ps = CreatePowerShellWithViewCmdlets();
        var mockConnection = CreateMockConnection("savedquery", "contact");
        
        var viewId = Guid.NewGuid();
        var view = new Entity("savedquery")
        {
            Id = viewId,
            ["savedqueryid"] = viewId,
            ["name"] = "View To Delete",
            ["returnedtypecode"] = "contact",
            ["fetchxml"] = "<fetch><entity name='contact'><attribute name='fullname'/></entity></fetch>",
            ["layoutxml"] = "<grid><row><cell name='fullname' width='200'/></row></grid>",
            ["querytype"] = 0
        };
        Service!.Create(view);

        // Act
        ps.AddCommand("Remove-DataverseView")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", viewId)
          .AddParameter("ViewType", "System")
          .AddParameter("Confirm", false);
        ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        
        // Verify view was deleted
        ps.Commands.Clear();
        ps.AddCommand("Get-DataverseView")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", viewId)
          .AddParameter("ViewType", "System");
        var checkResults = ps.Invoke();
        
        checkResults.Should().BeEmpty();
    }

    [Fact]
    public void RemoveDataverseView_RemovesPersonalViewById()
    {
        // Arrange
        using var ps = CreatePowerShellWithViewCmdlets();
        var mockConnection = CreateMockConnection("userquery", "contact");
        
        var viewId = Guid.NewGuid();
        var view = new Entity("userquery")
        {
            Id = viewId,
            ["userqueryid"] = viewId,
            ["name"] = "Personal View To Delete",
            ["returnedtypecode"] = "contact",
            ["fetchxml"] = "<fetch><entity name='contact'><attribute name='fullname'/></entity></fetch>",
            ["layoutxml"] = "<grid><row><cell name='fullname' width='200'/></row></grid>",
            ["querytype"] = 0
        };
        Service!.Create(view);

        // Act
        ps.AddCommand("Remove-DataverseView")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", viewId)
          .AddParameter("ViewType", "Personal")
          .AddParameter("Confirm", false);
        ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        
        // Verify view was deleted
        ps.Commands.Clear();
        ps.AddCommand("Get-DataverseView")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", viewId)
          .AddParameter("ViewType", "Personal");
        var checkResults = ps.Invoke();
        
        checkResults.Should().BeEmpty();
    }

    #endregion
}
