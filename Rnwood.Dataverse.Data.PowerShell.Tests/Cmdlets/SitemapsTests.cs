using System;
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
/// Tests for Sitemap-related cmdlets:
/// - Get-DataverseSitemap
/// - Set-DataverseSitemap
/// - Remove-DataverseSitemap
/// - Get-DataverseSitemapEntry
/// - Set-DataverseSitemapEntry
/// - Remove-DataverseSitemapEntry
/// </summary>
public class SitemapsTests : TestBase
{
    private const string SampleSitemapXml = @"<SiteMap>
        <Area Id=""SFA"" ResourceId=""Area_Sales"" Title=""Sales"">
            <Group Id=""SFA"" ResourceId=""Group_Sales"" Title=""Sales"">
                <SubArea Id=""nav_conts"" Entity=""contact"" ResourceId=""SubArea_Contacts"" Title=""Contacts""/>
                <SubArea Id=""nav_accts"" Entity=""account"" ResourceId=""SubArea_Accounts"" Title=""Accounts""/>
            </Group>
        </Area>
        <Area Id=""Settings"" ResourceId=""Area_Settings"" Title=""Settings"">
            <Group Id=""System"" ResourceId=""Group_System"" Title=""System"">
                <SubArea Id=""nav_users"" Entity=""systemuser"" ResourceId=""SubArea_Users"" Title=""Users""/>
            </Group>
        </Area>
    </SiteMap>";

    private PS CreatePowerShellWithSitemapCmdlets()
    {
        var initialSessionState = InitialSessionState.CreateDefault2();
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataverseSitemap", typeof(GetDataverseSitemapCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Set-DataverseSitemap", typeof(SetDataverseSitemapCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Remove-DataverseSitemap", typeof(RemoveDataverseSitemapCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataverseSitemapEntry", typeof(GetDataverseSitemapEntryCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Set-DataverseSitemapEntry", typeof(SetDataverseSitemapEntryCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Remove-DataverseSitemapEntry", typeof(RemoveDataverseSitemapEntryCmdlet), null));
        
        var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
        runspace.Open();
        
        var ps = PS.Create();
        ps.Runspace = runspace;
        return ps;
    }

    #region Get-DataverseSitemap Tests

    [Fact]
    public void GetDataverseSitemap_RetrievesSitemapById()
    {
        // Arrange
        using var ps = CreatePowerShellWithSitemapCmdlets();
        var mockConnection = CreateMockConnection("sitemap");
        
        var sitemapId = Guid.NewGuid();
        var sitemap = new Entity("sitemap")
        {
            Id = sitemapId,
            ["sitemapid"] = sitemapId,
            ["sitemapname"] = "Test Sitemap",
            ["sitemapnameunique"] = "test_sitemap",
            ["sitemapxml"] = SampleSitemapXml
        };
        Service!.Create(sitemap);

        // Act
        ps.AddCommand("Get-DataverseSitemap")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", sitemapId);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        
        // Results should be SitemapInfo objects
        var sitemapInfo = results[0].BaseObject as SitemapInfo;
        sitemapInfo.Should().NotBeNull();
        sitemapInfo!.Id.Should().Be(sitemapId);
        sitemapInfo.Name.Should().Be("Test Sitemap");
        sitemapInfo.UniqueName.Should().Be("test_sitemap");
    }

    [Fact]
    public void GetDataverseSitemap_RetrievesSitemapByName()
    {
        // Arrange
        using var ps = CreatePowerShellWithSitemapCmdlets();
        var mockConnection = CreateMockConnection("sitemap");
        
        var sitemapId = Guid.NewGuid();
        var sitemap = new Entity("sitemap")
        {
            Id = sitemapId,
            ["sitemapid"] = sitemapId,
            ["sitemapname"] = "Sales App Sitemap",
            ["sitemapnameunique"] = "sales_sitemap",
            ["sitemapxml"] = SampleSitemapXml
        };
        Service!.Create(sitemap);

        // Act
        ps.AddCommand("Get-DataverseSitemap")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Name", "Sales App Sitemap");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        var sitemapInfo = results[0].BaseObject as SitemapInfo;
        sitemapInfo!.Name.Should().Be("Sales App Sitemap");
    }

    [Fact]
    public void GetDataverseSitemap_RetrievesSitemapByUniqueName()
    {
        // Arrange
        using var ps = CreatePowerShellWithSitemapCmdlets();
        var mockConnection = CreateMockConnection("sitemap");
        
        var sitemapId = Guid.NewGuid();
        var sitemap = new Entity("sitemap")
        {
            Id = sitemapId,
            ["sitemapid"] = sitemapId,
            ["sitemapname"] = "Service App Sitemap",
            ["sitemapnameunique"] = "service_sitemap",
            ["sitemapxml"] = SampleSitemapXml
        };
        Service!.Create(sitemap);

        // Act
        ps.AddCommand("Get-DataverseSitemap")
          .AddParameter("Connection", mockConnection)
          .AddParameter("UniqueName", "service_sitemap");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        var sitemapInfo = results[0].BaseObject as SitemapInfo;
        sitemapInfo!.UniqueName.Should().Be("service_sitemap");
    }

    [Fact]
    public void GetDataverseSitemap_ReturnsAllSitemaps()
    {
        // Arrange
        using var ps = CreatePowerShellWithSitemapCmdlets();
        var mockConnection = CreateMockConnection("sitemap");
        
        var sitemap1 = new Entity("sitemap")
        {
            Id = Guid.NewGuid(),
            ["sitemapname"] = "Sitemap One",
            ["sitemapnameunique"] = "sitemap_one",
            ["sitemapxml"] = SampleSitemapXml
        };
        sitemap1["sitemapid"] = sitemap1.Id;
        
        var sitemap2 = new Entity("sitemap")
        {
            Id = Guid.NewGuid(),
            ["sitemapname"] = "Sitemap Two",
            ["sitemapnameunique"] = "sitemap_two",
            ["sitemapxml"] = SampleSitemapXml
        };
        sitemap2["sitemapid"] = sitemap2.Id;
        
        Service!.Create(sitemap1);
        Service!.Create(sitemap2);

        // Act
        ps.AddCommand("Get-DataverseSitemap")
          .AddParameter("Connection", mockConnection);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(2);
    }

    [Fact]
    public void GetDataverseSitemap_ReturnsSitemapXml()
    {
        // Arrange
        using var ps = CreatePowerShellWithSitemapCmdlets();
        var mockConnection = CreateMockConnection("sitemap");
        
        var sitemapId = Guid.NewGuid();
        var sitemap = new Entity("sitemap")
        {
            Id = sitemapId,
            ["sitemapid"] = sitemapId,
            ["sitemapname"] = "Test Sitemap",
            ["sitemapnameunique"] = "test_sitemap",
            ["sitemapxml"] = SampleSitemapXml
        };
        Service!.Create(sitemap);

        // Act
        ps.AddCommand("Get-DataverseSitemap")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", sitemapId);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        var sitemapInfo = results[0].BaseObject as SitemapInfo;
        sitemapInfo!.SitemapXml.Should().Contain("SiteMap");
        sitemapInfo.SitemapXml.Should().Contain("Area");
    }

    #endregion

    #region Set-DataverseSitemap Tests

    [Fact]
    public void SetDataverseSitemap_UpdatesSitemapXml()
    {
        // Arrange
        using var ps = CreatePowerShellWithSitemapCmdlets();
        var mockConnection = CreateMockConnection("sitemap");
        
        var sitemapId = Guid.NewGuid();
        var sitemap = new Entity("sitemap")
        {
            Id = sitemapId,
            ["sitemapid"] = sitemapId,
            ["sitemapname"] = "Updatable Sitemap",
            ["sitemapnameunique"] = "updatable_sitemap",
            ["sitemapxml"] = SampleSitemapXml
        };
        Service!.Create(sitemap);
        
        var newSitemapXml = @"<SiteMap><Area Id=""NewArea"" Title=""New""/></SiteMap>";

        // Act
        ps.AddCommand("Set-DataverseSitemap")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", sitemapId)
          .AddParameter("SitemapXml", newSitemapXml);
        ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        
        // Verify the sitemap was updated
        ps.Commands.Clear();
        ps.AddCommand("Get-DataverseSitemap")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", sitemapId);
        var checkResults = ps.Invoke();
        
        var sitemapInfo = checkResults[0].BaseObject as SitemapInfo;
        sitemapInfo!.SitemapXml.Should().Contain("NewArea");
    }

    #endregion

    #region Remove-DataverseSitemap Tests

    [Fact]
    public void RemoveDataverseSitemap_RemovesSitemap()
    {
        // Arrange
        using var ps = CreatePowerShellWithSitemapCmdlets();
        var mockConnection = CreateMockConnection("sitemap");
        
        var sitemapId = Guid.NewGuid();
        var sitemap = new Entity("sitemap")
        {
            Id = sitemapId,
            ["sitemapid"] = sitemapId,
            ["sitemapname"] = "Sitemap To Delete",
            ["sitemapnameunique"] = "delete_sitemap",
            ["sitemapxml"] = SampleSitemapXml
        };
        Service!.Create(sitemap);

        // Act
        ps.AddCommand("Remove-DataverseSitemap")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", sitemapId)
          .AddParameter("Confirm", false);
        ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        
        // Verify sitemap was deleted
        ps.Commands.Clear();
        ps.AddCommand("Get-DataverseSitemap")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", sitemapId);
        var checkResults = ps.Invoke();
        
        checkResults.Should().BeEmpty();
    }

    #endregion

    #region Get-DataverseSitemapEntry Tests

    [Fact]
    public void GetDataverseSitemapEntry_RetrievesEntriesFromSitemap()
    {
        // Arrange
        using var ps = CreatePowerShellWithSitemapCmdlets();
        var mockConnection = CreateMockConnection("sitemap");
        
        var sitemapId = Guid.NewGuid();
        var sitemap = new Entity("sitemap")
        {
            Id = sitemapId,
            ["sitemapid"] = sitemapId,
            ["sitemapname"] = "Entry Test Sitemap",
            ["sitemapnameunique"] = "entry_sitemap",
            ["sitemapxml"] = SampleSitemapXml
        };
        Service!.Create(sitemap);

        // Act
        ps.AddCommand("Get-DataverseSitemapEntry")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SitemapId", sitemapId);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        // Should have entries for Areas, Groups, and SubAreas
        results.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public void GetDataverseSitemapEntry_FiltersByArea()
    {
        // Arrange
        using var ps = CreatePowerShellWithSitemapCmdlets();
        var mockConnection = CreateMockConnection("sitemap");
        
        var sitemapId = Guid.NewGuid();
        var sitemap = new Entity("sitemap")
        {
            Id = sitemapId,
            ["sitemapid"] = sitemapId,
            ["sitemapname"] = "Filter Test Sitemap",
            ["sitemapnameunique"] = "filter_sitemap",
            ["sitemapxml"] = SampleSitemapXml
        };
        Service!.Create(sitemap);

        // Act - Get the specific Area entry by ID
        ps.AddCommand("Get-DataverseSitemapEntry")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SitemapId", sitemapId)
          .AddParameter("Area")
          .AddParameter("EntryId", "SFA");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCountGreaterThan(0, "Should have at least one result for area SFA");
        results[0].Should().NotBeNull("Result item should not be null");
        
        // The cmdlet returns PSObjects with NoteProperties, not SitemapEntryInfo objects
        var result = results[0];
        result.Properties["EntryType"].Value.Should().Be(SitemapEntryType.Area);
        result.Properties["Id"].Value.Should().Be("SFA");
    }

    #endregion

    #region Set-DataverseSitemapEntry Tests

    [Fact]
    public void SetDataverseSitemapEntry_AddsEntryToSitemap()
    {
        // Arrange
        using var ps = CreatePowerShellWithSitemapCmdlets();
        var mockConnection = CreateMockConnection("sitemap", "contact");  // Include contact for entity validation
        
        var sitemapId = Guid.NewGuid();
        var sitemap = new Entity("sitemap")
        {
            Id = sitemapId,
            ["sitemapid"] = sitemapId,
            ["sitemapname"] = "Add Entry Sitemap",
            ["sitemapnameunique"] = "add_entry_sitemap",
            ["sitemapxml"] = SampleSitemapXml
        };
        Service!.Create(sitemap);

        // Act - Add a new SubArea using proper switch parameters (use contact entity which is available in mock)
        ps.AddCommand("Set-DataverseSitemapEntry")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SitemapId", sitemapId)
          .AddParameter("SubArea")
          .AddParameter("EntryId", "NewSubArea")
          .AddParameter("ParentAreaId", "SFA")
          .AddParameter("ParentGroupId", "SFA")
          .AddParameter("Entity", "contact");
        ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        
        // Verify the entry was added
        ps.Commands.Clear();
        ps.AddCommand("Get-DataverseSitemap")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", sitemapId);
        var checkResults = ps.Invoke();
        
        var sitemapInfo = checkResults[0].BaseObject as SitemapInfo;
        sitemapInfo!.SitemapXml.Should().Contain("NewSubArea");
    }

    [Fact]
    public void SetDataverseSitemapEntry_SupportsWhatIf()
    {
        // Arrange
        using var ps = CreatePowerShellWithSitemapCmdlets();
        var mockConnection = CreateMockConnection("sitemap");
        
        var sitemapId = Guid.NewGuid();
        var sitemap = new Entity("sitemap")
        {
            Id = sitemapId,
            ["sitemapid"] = sitemapId,
            ["sitemapname"] = "WhatIf Sitemap",
            ["sitemapnameunique"] = "whatif_sitemap",
            ["sitemapxml"] = SampleSitemapXml
        };
        Service!.Create(sitemap);

        // Act - Use WhatIf
        ps.AddScript(@"
            param($connection, $sitemapId)
            Set-DataverseSitemapEntry -Connection $connection -SitemapId $sitemapId -Area 'SFA' -Group 'SFA' -SubArea 'WhatIfSubArea' -Entity 'opportunity' -WhatIf
        ")
        .AddParameter("connection", mockConnection)
        .AddParameter("sitemapId", sitemapId);
        ps.Invoke();

        // Assert - Sitemap should NOT contain the new entry
        ps.Commands.Clear();
        ps.AddCommand("Get-DataverseSitemap")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", sitemapId);
        var checkResults = ps.Invoke();
        
        var sitemapInfo = checkResults[0].BaseObject as SitemapInfo;
        sitemapInfo!.SitemapXml.Should().NotContain("WhatIfSubArea");
    }

    #endregion

    #region Remove-DataverseSitemapEntry Tests

    [Fact]
    public void RemoveDataverseSitemapEntry_RemovesEntryFromSitemap()
    {
        // Arrange
        using var ps = CreatePowerShellWithSitemapCmdlets();
        var mockConnection = CreateMockConnection("sitemap");
        
        var sitemapId = Guid.NewGuid();
        var sitemap = new Entity("sitemap")
        {
            Id = sitemapId,
            ["sitemapid"] = sitemapId,
            ["sitemapname"] = "Remove Entry Sitemap",
            ["sitemapnameunique"] = "remove_entry_sitemap",
            ["sitemapxml"] = SampleSitemapXml
        };
        Service!.Create(sitemap);

        // Act - Remove the nav_conts SubArea using proper switch parameters
        ps.AddCommand("Remove-DataverseSitemapEntry")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SitemapId", sitemapId)
          .AddParameter("SubArea")
          .AddParameter("EntryId", "nav_conts")
          .AddParameter("Confirm", false);
        ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        
        // Verify the entry was removed
        ps.Commands.Clear();
        ps.AddCommand("Get-DataverseSitemap")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", sitemapId);
        var checkResults = ps.Invoke();
        
        var sitemapInfo = checkResults[0].BaseObject as SitemapInfo;
        sitemapInfo!.SitemapXml.Should().NotContain("nav_conts");
    }

    #endregion
}
