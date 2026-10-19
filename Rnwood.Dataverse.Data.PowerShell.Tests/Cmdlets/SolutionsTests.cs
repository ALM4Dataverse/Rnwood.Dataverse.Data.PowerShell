using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Text;
using System.Linq;
using AwesomeAssertions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets;

/// <summary>
/// Tests for Solution-related cmdlets:
/// - Get-DataverseSolution
/// - Import-DataverseSolution
/// - Export-DataverseSolution
/// - Get-DataverseSolutionFile
/// - Get-DataverseSolutionDependency
/// </summary>
public class SolutionsTests : TestBase
{
    private new PS CreatePowerShellWithCmdlets()
    {
        var initialSessionState = InitialSessionState.CreateDefault();
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataverseSolution", typeof(GetDataverseSolutionCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataverseSolutionFile", typeof(GetDataverseSolutionFileCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataverseSolutionDependency", typeof(GetDataverseSolutionDependencyCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Import-DataverseSolution", typeof(ImportDataverseSolutionCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Remove-DataverseSolution", typeof(RemoveDataverseSolutionCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Invoke-DataverseSolutionUpgrade", typeof(InvokeDataverseSolutionUpgradeCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Set-DataverseSolutionComponent", typeof(SetDataverseSolutionComponentCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Remove-DataverseSolutionComponent", typeof(RemoveDataverseSolutionComponentCmdlet), null));

        var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
        runspace.Open();

        var ps = PS.Create();
        ps.Runspace = runspace;
        return ps;
    }

    /// <summary>
    /// Creates a minimal solution zip file with a valid solution.xml structure.
    /// </summary>
    private static byte[] CreateSolutionZipBytes(string uniqueName, string friendlyName, string version, bool isManaged = false)
    {
        var solutionXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<ImportExportXml xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <SolutionManifest>
    <UniqueName>{uniqueName}</UniqueName>
    <LocalizedNames>
      <LocalizedName description=""{friendlyName}"" languagecode=""1033"" />
    </LocalizedNames>
    <Descriptions>
      <Description description=""Test solution description"" languagecode=""1033"" />
    </Descriptions>
    <Version>{version}</Version>
    <Managed>{(isManaged ? "1" : "0")}</Managed>
    <Publisher>
      <UniqueName>publisher</UniqueName>
      <LocalizedNames>
        <LocalizedName description=""Test Publisher"" languagecode=""1033"" />
      </LocalizedNames>
      <CustomizationPrefix>test</CustomizationPrefix>
    </Publisher>
    <RootComponents />
    <MissingDependencies />
  </SolutionManifest>
</ImportExportXml>";

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            var entry = archive.CreateEntry("solution.xml");
            using (var entryStream = entry.Open())
            {
                var bytes = Encoding.UTF8.GetBytes(solutionXml);
                entryStream.Write(bytes, 0, bytes.Length);
            }
        }
        memoryStream.Position = 0;
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Creates solution and publisher records in the mock context.
    /// </summary>
    private Entity CreateSolutionEntity(string uniqueName, string friendlyName, string version, bool isManaged)
    {
        // Create publisher first
        var publisherId = Guid.NewGuid();
        var publisher = new Entity("publisher", publisherId)
        {
            ["friendlyname"] = "Test Publisher",
            ["uniquename"] = "testpublisher",
            ["customizationprefix"] = "test"
        };
        Service!.Create(publisher);

        // Create solution
        var solution = new Entity("solution")
        {
            Id = Guid.NewGuid(),
            ["uniquename"] = uniqueName,
            ["friendlyname"] = friendlyName,
            ["version"] = version,
            ["ismanaged"] = isManaged,
            ["description"] = "Test description",
            ["publisherid"] = new EntityReference("publisher", publisherId)
        };
        Service!.Create(solution);
        return solution;
    }

    // Get-DataverseSolution Wildcard Tests (Get-DataverseSolution-Wildcards.Tests.ps1 - 5 tests)

    [Fact]
    public void GetDataverseSolution_UsesEqualOperator_ForExactNameNoWildcards()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("contact");
        CreateSolutionEntity("MySolution", "My Solution", "1.0.0.0", false);
        CreateSolutionEntity("OtherSolution", "Other Solution", "1.0.0.0", false);

        // Act
        ps.AddCommand("Get-DataverseSolution")
          .AddParameter("Connection", mockConnection)
          .AddParameter("UniqueName", "MySolution");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        var result = results[0].BaseObject as SolutionInfo;
        result.Should().NotBeNull();
        result!.UniqueName.Should().Be("MySolution");
    }

    [Fact]
    public void GetDataverseSolution_UsesLikeOperatorWithPercent_ForWildcardPatternWithAsterisk()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("contact");
        CreateSolutionEntity("MySolution1", "My Solution 1", "1.0.0.0", false);
        CreateSolutionEntity("MySolution2", "My Solution 2", "1.0.0.0", false);
        CreateSolutionEntity("OtherSolution", "Other Solution", "1.0.0.0", false);

        // Act - * should translate to % for SQL Like
        ps.AddCommand("Get-DataverseSolution")
          .AddParameter("Connection", mockConnection)
          .AddParameter("UniqueName", "My*");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(2);
        var solutions = results.Select(r => (r.BaseObject as SolutionInfo)!.UniqueName).ToList();
        solutions.Should().Contain("MySolution1");
        solutions.Should().Contain("MySolution2");
        solutions.Should().NotContain("OtherSolution");
    }

    [Fact]
    public void GetDataverseSolution_UsesLikeOperatorWithUnderscore_ForWildcardPatternWithQuestionMark()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("contact");
        CreateSolutionEntity("MySolution1", "My Solution 1", "1.0.0.0", false);
        CreateSolutionEntity("MySolution2", "My Solution 2", "1.0.0.0", false);
        CreateSolutionEntity("MySolution12", "My Solution 12", "1.0.0.0", false);

        // Act - ? should translate to _ for SQL Like (matches exactly one character)
        ps.AddCommand("Get-DataverseSolution")
          .AddParameter("Connection", mockConnection)
          .AddParameter("UniqueName", "MySolution?");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(2);
        var solutions = results.Select(r => (r.BaseObject as SolutionInfo)!.UniqueName).ToList();
        solutions.Should().Contain("MySolution1");
        solutions.Should().Contain("MySolution2");
        solutions.Should().NotContain("MySolution12");
    }

    [Fact]
    public void GetDataverseSolution_UsesLikeOperatorWithBothPercentAndUnderscore_ForCombinedWildcards()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("contact");
        CreateSolutionEntity("MySolution1", "My Solution 1", "1.0.0.0", false);
        CreateSolutionEntity("MySolution2", "My Solution 2", "1.0.0.0", false);
        CreateSolutionEntity("MyOtherSolution3", "My Other Solution 3", "1.0.0.0", false);
        CreateSolutionEntity("TheirSolution4", "Their Solution 4", "1.0.0.0", false);

        // Act - combine * and ? wildcards
        ps.AddCommand("Get-DataverseSolution")
          .AddParameter("Connection", mockConnection)
          .AddParameter("UniqueName", "My*Solution?");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        var solutions = results.Select(r => (r.BaseObject as SolutionInfo)!.UniqueName).ToList();
        solutions.Should().Contain("MySolution1");
        solutions.Should().Contain("MySolution2");
        solutions.Should().Contain("MyOtherSolution3");
        solutions.Should().NotContain("TheirSolution4");
    }

    [Fact]
    public void GetDataverseSolution_CombinesWildcardFilterWithManagedFilter()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("contact");
        CreateSolutionEntity("MySolution1", "My Solution 1", "1.0.0.0", false); // unmanaged
        CreateSolutionEntity("MySolution2", "My Solution 2", "1.0.0.0", true);  // managed
        CreateSolutionEntity("OtherSolution", "Other Solution", "1.0.0.0", true); // managed

        // Act - filter by wildcard name AND managed
        ps.AddCommand("Get-DataverseSolution")
          .AddParameter("Connection", mockConnection)
          .AddParameter("UniqueName", "My*")
          .AddParameter("Managed", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        var result = results[0].BaseObject as SolutionInfo;
        result.Should().NotBeNull();
        result!.UniqueName.Should().Be("MySolution2");
        result.IsManaged.Should().BeTrue();
    }

    // Get-DataverseSolutionFile Tests (Get-DataverseSolutionFile.Tests.ps1 - 2 tests)

    [Fact]
    public void GetDataverseSolutionFile_ParsesSolutionFileAndReturnsMetadata()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var solutionBytes = CreateSolutionZipBytes("TestSolution", "Test Solution", "1.2.3.4", false);
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempFile, solutionBytes);

            // Act
            ps.AddCommand("Get-DataverseSolutionFile")
              .AddParameter("Path", tempFile);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            var solutionInfo = results[0].BaseObject as SolutionFileInfo;
            solutionInfo.Should().NotBeNull();
            solutionInfo!.UniqueName.Should().Be("TestSolution");
            solutionInfo.Name.Should().Be("Test Solution");
            solutionInfo.Version.Should().Be(new Version(1, 2, 3, 4));
            solutionInfo.IsManaged.Should().BeFalse();
            solutionInfo.PublisherUniqueName.Should().Be("publisher");
            solutionInfo.PublisherName.Should().Be("Test Publisher");
            solutionInfo.PublisherPrefix.Should().Be("test");
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void GetDataverseSolutionFile_ReturnsError_ForMissingFile()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".zip");

        // Act
        ps.AddCommand("Get-DataverseSolutionFile")
          .AddParameter("Path", nonExistentPath);
        
        // The cmdlet throws a terminating error
        var exception = Assert.Throws<CmdletInvocationException>(() => ps.Invoke());
        
        // Assert
        exception.InnerException.Should().BeOfType<FileNotFoundException>();
    }

    // Get-DataverseSolutionDependency Tests (Get-DataverseSolutionDependency.Tests.ps1 - 10 tests)

    [Fact]
    public void GetDataverseSolutionDependency_RetrievesMissingDependencies_WithMissingSwitch()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var dependencyEntities = new EntityCollection();
        dependencyEntities.Entities.Add(new Entity("dependency")
        {
            Id = Guid.NewGuid(),
            ["dependentcomponenttype"] = new OptionSetValue(1),
            ["requiredcomponenttype"] = new OptionSetValue(2)
        });

        var mockConnection = CreateMockConnection(
            request =>
            {
                if (request is RetrieveMissingDependenciesRequest)
                {
                    var response = new RetrieveMissingDependenciesResponse();
                    response.Results["EntityCollection"] = dependencyEntities;
                    return response;
                }
                return null;
            },
            "contact");

        CreateSolutionEntity("TestSolution", "Test Solution", "1.0.0.0", false);

        // Act
        ps.AddCommand("Get-DataverseSolutionDependency")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionUniqueName", "TestSolution")
          .AddParameter("Missing", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
    }

    [Fact]
    public void GetDataverseSolutionDependency_AcceptsSolutionUniqueNameFromPipeline()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("contact");
        CreateSolutionEntity("TestSolution", "Test Solution", "1.0.0.0", false);

        // Create a PSObject with UniqueName property to simulate pipeline input
        var pipelineInput = new PSObject();
        pipelineInput.Properties.Add(new PSNoteProperty("SolutionUniqueName", "TestSolution"));

        // Act - Accept input from pipeline via property name
        ps.AddCommand("Get-DataverseSolutionDependency")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Missing", true);
        var results = ps.Invoke(new[] { pipelineInput });

        // Assert
        ps.HadErrors.Should().BeFalse();
        // Default interceptor returns empty collection
        results.Should().BeEmpty();
    }

    [Fact]
    public void GetDataverseSolutionDependency_SupportsUniqueNameAlias()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("contact");
        CreateSolutionEntity("TestSolution", "Test Solution", "1.0.0.0", false);

        // Act - use UniqueName alias instead of SolutionUniqueName
        ps.AddCommand("Get-DataverseSolutionDependency")
          .AddParameter("Connection", mockConnection)
          .AddParameter("UniqueName", "TestSolution")
          .AddParameter("Missing", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        // Default interceptor returns empty collection
        results.Should().BeEmpty();
    }

    [Fact]
    public void GetDataverseSolutionDependency_ReturnsEmptyCollection_WhenNoMissingDependencies()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("contact");
        CreateSolutionEntity("TestSolution", "Test Solution", "1.0.0.0", false);

        // Act
        ps.AddCommand("Get-DataverseSolutionDependency")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionUniqueName", "TestSolution")
          .AddParameter("Missing", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().BeEmpty();
    }

    [Fact]
    public void GetDataverseSolutionDependency_RetrievesUninstallDependencies_WithUninstallSwitch()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var dependencyEntities = new EntityCollection();
        dependencyEntities.Entities.Add(new Entity("dependency")
        {
            Id = Guid.NewGuid(),
            ["dependentcomponenttype"] = new OptionSetValue(1),
            ["requiredcomponenttype"] = new OptionSetValue(2)
        });

        var mockConnection = CreateMockConnection(
            request =>
            {
                if (request is RetrieveDependenciesForUninstallRequest)
                {
                    var response = new RetrieveDependenciesForUninstallResponse();
                    response.Results["EntityCollection"] = dependencyEntities;
                    return response;
                }
                return null;
            },
            "contact");

        CreateSolutionEntity("TestSolution", "Test Solution", "1.0.0.0", false);

        // Act
        ps.AddCommand("Get-DataverseSolutionDependency")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionUniqueName", "TestSolution")
          .AddParameter("Uninstall", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
    }

    [Fact]
    public void GetDataverseSolutionDependency_ReturnsEmptyCollection_WhenNoUninstallDependencies()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("contact");
        CreateSolutionEntity("TestSolution", "Test Solution", "1.0.0.0", false);

        // Act
        ps.AddCommand("Get-DataverseSolutionDependency")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionUniqueName", "TestSolution")
          .AddParameter("Uninstall", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().BeEmpty();
    }

    [Fact]
    public void SetDataverseSolutionComponent_AddsNewComponent_AndReturnsPassThruInfo()
    {
        using var ps = CreatePowerShellWithCmdlets();

        var componentId = Guid.NewGuid();
        AddSolutionComponentRequest capturedAddRequest = null;
        var mockConnection = CreateMockConnectionWithCustomMetadata(
            request =>
            {
                if (request is AddSolutionComponentRequest addRequest)
                {
                    capturedAddRequest = addRequest;
                    var solutionId = GetSolutionIdByUniqueName(addRequest.SolutionUniqueName);
                    Service!.Create(CreateSolutionComponentEntity(Guid.NewGuid(), solutionId, addRequest.ComponentId, addRequest.ComponentType, 0));
                    return new OrganizationResponse();
                }

                return null;
            },
            BuildSolutionComponentMetadata());

        CreateSolutionEntity("TestSolution", "Test Solution", "1.0.0.0", false);

        ps.AddCommand("Set-DataverseSolutionComponent")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionName", "TestSolution")
          .AddParameter("ComponentId", componentId)
          .AddParameter("ComponentType", 1)
          .AddParameter("Behavior", 0)
          .AddParameter("PassThru", true)
          .AddParameter("Confirm", false);
        var results = ps.Invoke();

        ps.HadErrors.Should().BeFalse();
        capturedAddRequest.Should().NotBeNull();
        capturedAddRequest!.DoNotIncludeSubcomponents.Should().BeFalse();
        capturedAddRequest.IncludedComponentSettingsValues.Should().BeNull();
        results.Should().HaveCount(1);
        results[0].Properties["BehaviorValue"]?.Value.Should().Be(0);
        results[0].Properties["Behavior"]?.Value.Should().Be("Include Subcomponents");
        results[0].Properties["WasUpdated"]?.Value.Should().Be(false);

        var storedComponent = GetSingleSolutionComponent(componentId, 1);
        storedComponent.Should().NotBeNull();
        storedComponent!.GetAttributeValue<OptionSetValue>("rootcomponentbehavior")!.Value.Should().Be(0);
    }

    [Fact]
    public void SetDataverseSolutionComponent_ReaddsComponent_WhenBehaviorChanges()
    {
        using var ps = CreatePowerShellWithCmdlets();

        var componentId = Guid.NewGuid();
        int addCount = 0;
        int removeCount = 0;
        AddSolutionComponentRequest capturedAddRequest = null;
        RemoveSolutionComponentRequest capturedRemoveRequest = null;
        var mockConnection = CreateMockConnectionWithCustomMetadata(
            request =>
            {
                if (request is AddSolutionComponentRequest addRequest)
                {
                    addCount++;
                    capturedAddRequest = addRequest;
                    var solutionId = GetSolutionIdByUniqueName(addRequest.SolutionUniqueName);
                    var behavior = addRequest.IncludedComponentSettingsValues != null && addRequest.IncludedComponentSettingsValues.Length == 0
                        ? 2
                        : addRequest.DoNotIncludeSubcomponents ? 1 : 0;

                    var existing = GetSingleSolutionComponent(addRequest.ComponentId, addRequest.ComponentType);
                    if (existing != null)
                    {
                        Service!.Delete("solutioncomponent", existing.Id);
                    }

                    Service!.Create(CreateSolutionComponentEntity(Guid.NewGuid(), solutionId, addRequest.ComponentId, addRequest.ComponentType, behavior));
                    return new OrganizationResponse();
                }

                if (request is RemoveSolutionComponentRequest removeRequest)
                {
                    removeCount++;
                    capturedRemoveRequest = removeRequest;
                    var existing = GetSingleSolutionComponent(removeRequest.ComponentId, removeRequest.ComponentType);
                    if (existing != null)
                    {
                        Service!.Delete("solutioncomponent", existing.Id);
                    }
                    return new OrganizationResponse();
                }

                return null;
            },
            BuildSolutionComponentMetadata());

        var solution = CreateSolutionEntity("TestSolution", "Test Solution", "1.0.0.0", false);
        Service!.Create(CreateSolutionComponentEntity(Guid.NewGuid(), solution.Id, componentId, 1, 0));

        ps.AddCommand("Set-DataverseSolutionComponent")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionName", "TestSolution")
          .AddParameter("ComponentId", componentId)
          .AddParameter("ComponentType", 1)
          .AddParameter("Behavior", 2)
          .AddParameter("PassThru", true)
          .AddParameter("Confirm", false);
        var results = ps.Invoke();

        ps.HadErrors.Should().BeFalse();
        removeCount.Should().Be(1);
        addCount.Should().Be(1);
        capturedRemoveRequest.Should().NotBeNull();
        capturedAddRequest.Should().NotBeNull();
        capturedAddRequest!.DoNotIncludeSubcomponents.Should().BeTrue();
        capturedAddRequest.IncludedComponentSettingsValues.Should().NotBeNull();
        capturedAddRequest.IncludedComponentSettingsValues.Should().BeEmpty();
        results.Should().HaveCount(1);
        results[0].Properties["BehaviorValue"]?.Value.Should().Be(2);
        results[0].Properties["Behavior"]?.Value.Should().Be("Include As Shell");
        results[0].Properties["WasUpdated"]?.Value.Should().Be(true);

        var storedComponent = GetSingleSolutionComponent(componentId, 1);
        storedComponent.Should().NotBeNull();
        storedComponent!.GetAttributeValue<OptionSetValue>("rootcomponentbehavior")!.Value.Should().Be(2);
    }

    [Fact]
    public void RemoveDataverseSolutionComponent_RemovesExistingComponent()
    {
        using var ps = CreatePowerShellWithCmdlets();

        var componentId = Guid.NewGuid();
        RemoveSolutionComponentRequest capturedRemoveRequest = null;
        var mockConnection = CreateMockConnectionWithCustomMetadata(
            request =>
            {
                if (request is RemoveSolutionComponentRequest removeRequest)
                {
                    capturedRemoveRequest = removeRequest;
                    var existing = GetSingleSolutionComponent(removeRequest.ComponentId, removeRequest.ComponentType);
                    if (existing != null)
                    {
                        Service!.Delete("solutioncomponent", existing.Id);
                    }
                    return new OrganizationResponse();
                }

                return null;
            },
            BuildSolutionComponentMetadata());

        var solution = CreateSolutionEntity("TestSolution", "Test Solution", "1.0.0.0", false);
        Service!.Create(CreateSolutionComponentEntity(Guid.NewGuid(), solution.Id, componentId, 1, 0));

        ps.AddCommand("Remove-DataverseSolutionComponent")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionName", "TestSolution")
          .AddParameter("ComponentId", componentId)
          .AddParameter("ComponentType", 1)
          .AddParameter("Confirm", false);
        ps.Invoke();

        ps.HadErrors.Should().BeFalse();
        capturedRemoveRequest.Should().NotBeNull();
        GetSingleSolutionComponent(componentId, 1).Should().BeNull();
    }

    [Fact]
    public void RemoveDataverseSolutionComponent_IfExists_DoesNotError_WhenComponentIsMissing()
    {
        using var ps = CreatePowerShellWithCmdlets();

        var mockConnection = CreateMockConnectionWithCustomMetadata(null, BuildSolutionComponentMetadata());
        CreateSolutionEntity("TestSolution", "Test Solution", "1.0.0.0", false);

        ps.AddCommand("Remove-DataverseSolutionComponent")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionName", "TestSolution")
          .AddParameter("ComponentId", Guid.NewGuid())
          .AddParameter("ComponentType", 1)
          .AddParameter("IfExists", true)
          .AddParameter("Confirm", false);
        var results = ps.Invoke();

        ps.HadErrors.Should().BeFalse();
        results.Should().BeEmpty();
    }

    // Import-DataverseSolution Version Check Tests (Import-DataverseSolution-VersionChecks.Tests.ps1 - 7 tests)

    [Fact]
    public void ImportDataverseSolution_ExtractsVersionFromSolutionFileCorrectly()
    {
        // This test verifies that Get-DataverseSolutionFile correctly extracts version
        // The Import-DataverseSolution uses the same extraction logic
        using var ps = CreatePowerShellWithCmdlets();
        var solutionBytes = CreateSolutionZipBytes("TestSolution", "Test Solution", "2.5.1.100", false);
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(tempFile, solutionBytes);

            ps.AddCommand("Get-DataverseSolutionFile")
              .AddParameter("Path", tempFile);
            var results = ps.Invoke();

            ps.HadErrors.Should().BeFalse();
            var solutionInfo = results[0].BaseObject as SolutionFileInfo;
            solutionInfo!.Version.Should().Be(new Version(2, 5, 1, 100));
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ImportDataverseSolution_DetectsSameVersionAndSkips_WithWarning()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var solutionBytes = CreateSolutionZipBytes("TestSolution", "Test Solution", "1.0.0.0", true);
        var mockConnection = CreateMockConnectionWithAsyncImport();
        
        // Create existing solution with same version
        CreateSolutionEntity("TestSolution", "Test Solution", "1.0.0.0", true);

        // Act
        ps.AddCommand("Import-DataverseSolution")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionFile", solutionBytes)
          .AddParameter("SkipIfSameVersion", true)
          .AddParameter("PollingIntervalSeconds", 1)
          .AddParameter("TimeoutSeconds", 30);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().BeEmpty(); // No output when skipped
        ps.Streams.Warning.Should().Contain(w => w.Message.Contains("Skipping import") && w.Message.Contains("same version"));
    }

    [Fact]
    public void ImportDataverseSolution_DetectsLowerVersionAndSkips_WithWarning()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var solutionBytes = CreateSolutionZipBytes("TestSolution", "Test Solution", "1.0.0.0", true);
        var mockConnection = CreateMockConnectionWithAsyncImport();
        
        // Create existing solution with higher version
        CreateSolutionEntity("TestSolution", "Test Solution", "2.0.0.0", true);

        // Act
        ps.AddCommand("Import-DataverseSolution")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionFile", solutionBytes)
          .AddParameter("SkipIfLowerVersion", true)
          .AddParameter("PollingIntervalSeconds", 1)
          .AddParameter("TimeoutSeconds", 30);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().BeEmpty(); // No output when skipped
        ps.Streams.Warning.Should().Contain(w => w.Message.Contains("Skipping import") && w.Message.Contains("lower than"));
    }

    // Import-DataverseSolution Component Update Tests (Import-DataverseSolution-SkipWithComponentUpdate.Tests.ps1 - 6 tests)

    [Fact]
    public void ImportDataverseSolution_UpdatesConnectionReferences_WhenProvided()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var solutionBytes = CreateSolutionZipWithConnectionReference("TestSolution", "Test Solution", "1.0.0.0", 
            "new_sharedconnectionref", "shared_sharepointonline");
        var mockConnection = CreateMockConnectionWithAsyncImport();
        
        var connectionId = Guid.NewGuid().ToString();
        var connectionRefs = new System.Collections.Hashtable
        {
            { "new_sharedconnectionref", connectionId }
        };

        // Act
        ps.AddCommand("Import-DataverseSolution")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionFile", solutionBytes)
          .AddParameter("ConnectionReferences", connectionRefs)
          .AddParameter("PollingIntervalSeconds", 1)
          .AddParameter("TimeoutSeconds", 30);
        var results = ps.Invoke();

        // Assert
        if (ps.HadErrors)
        {
            var errors = string.Join("\n", ps.Streams.Error.Select(e => e.ToString()));
            throw new InvalidOperationException($"PowerShell errors occurred: {errors}");
        }
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        var result = results[0];
        result.Should().NotBeNull();
        result.Properties["ImportJobId"]?.Value.Should().NotBeNull();
    }

    [Fact]
    public void ImportDataverseSolution_UpdatesEnvironmentVariables_WhenProvided()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var solutionBytes = CreateSolutionZipWithEnvironmentVariable("TestSolution", "Test Solution", "1.0.0.0", "new_apiurl");
        var mockConnection = CreateMockConnectionWithAsyncImport();
        
        var envVars = new System.Collections.Hashtable
        {
            { "new_apiurl", "https://api.example.com" }
        };

        // Act
        ps.AddCommand("Import-DataverseSolution")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionFile", solutionBytes)
          .AddParameter("EnvironmentVariables", envVars)
          .AddParameter("PollingIntervalSeconds", 1)
          .AddParameter("TimeoutSeconds", 30);
        var results = ps.Invoke();

        // Assert
        if (ps.HadErrors)
        {
            var errors = string.Join("\n", ps.Streams.Error.Select(e => e.ToString()));
            throw new InvalidOperationException($"PowerShell errors occurred: {errors}");
        }
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        var result = results[0];
        result.Should().NotBeNull();
        result.Properties["ImportJobId"]?.Value.Should().NotBeNull();
    }

    // Import-DataverseSolution Null Reference Tests (Import-DataverseSolution-NullReferenceException.Tests.ps1 - 2 tests)

    [Fact]
    public void ImportDataverseSolution_DoesNotThrowNullReferenceException_ForNewSolutionWithNoComponents()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var solutionBytes = CreateSolutionZipBytes("EmptySolution", "Empty Solution", "1.0.0.0", false);
        var mockConnection = CreateMockConnectionWithAsyncImport();

        // Act
        ps.AddCommand("Import-DataverseSolution")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionFile", solutionBytes)
          .AddParameter("PollingIntervalSeconds", 1)
          .AddParameter("TimeoutSeconds", 30);
        var results = ps.Invoke();

        // Assert
        if (ps.HadErrors)
        {
            var errors = string.Join("\n", ps.Streams.Error.Select(e => e.ToString()));
            throw new InvalidOperationException($"PowerShell errors occurred: {errors}");
        }
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        var result = results[0];
        result.Should().NotBeNull();
        result.Properties["ImportJobId"]?.Value.Should().NotBeNull();
    }

    [Fact]
    public void ImportDataverseSolution_DoesNotThrowNullReferenceException_WithUseUpdateIfVersionMajorMinorMatches()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var solutionBytes = CreateSolutionZipBytes("TestSolution", "Test Solution", "1.0.1.0", true);
        var mockConnection = CreateMockConnectionWithAsyncImport();
        
        // Create existing solution with matching major.minor version
        CreateSolutionEntity("TestSolution", "Test Solution", "1.0.0.0", true);

        // Act
        ps.AddCommand("Import-DataverseSolution")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionFile", solutionBytes)
          .AddParameter("UseUpdateIfVersionMajorMinorMatches", true)
          .AddParameter("PollingIntervalSeconds", 1)
          .AddParameter("TimeoutSeconds", 30);
        var results = ps.Invoke();

        // Assert
        if (ps.HadErrors)
        {
            var errors = string.Join("\n", ps.Streams.Error.Select(e => e.ToString()));
            throw new InvalidOperationException($"PowerShell errors occurred: {errors}");
        }
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        var result = results[0];
        result.Should().NotBeNull();
        result.Properties["ImportJobId"]?.Value.Should().NotBeNull();
    }

    // Import-DataverseSolution Holding Solution Tests (Import-DataverseSolution-HoldingSolutionVersionCheck.Tests.ps1 - 3 tests)

    [Fact]
    public void ImportDataverseSolution_SkipsImport_WhenHoldingSolutionExistsWithSameVersion()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var solutionBytes = CreateSolutionZipBytes("TestSolution", "Test Solution", "1.0.0.0", true);
        var mockConnection = CreateMockConnectionWithAsyncImport();
        
        // Create base solution
        CreateSolutionEntity("TestSolution", "Test Solution", "0.9.0.0", true);
        // Create holding solution with same version as import
        CreateSolutionEntity("TestSolution_Upgrade", "Test Solution Upgrade", "1.0.0.0", true);

        // Act
        ps.AddCommand("Import-DataverseSolution")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionFile", solutionBytes)
          .AddParameter("Mode", ImportMode.HoldingSolution)
          .AddParameter("PollingIntervalSeconds", 1)
          .AddParameter("TimeoutSeconds", 30);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().BeEmpty(); // No output when skipped
        ps.Streams.Warning.Should().Contain(w => w.Message.Contains("Skipping import", StringComparison.OrdinalIgnoreCase) && w.Message.Contains("Holding solution", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ImportDataverseSolution_FailsWithClearError_WhenHoldingSolutionExistsWithDifferentVersion()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var solutionBytes = CreateSolutionZipBytes("TestSolution", "Test Solution", "2.0.0.0", true);
        var mockConnection = CreateMockConnectionWithAsyncImport();
        
        // Create base solution
        CreateSolutionEntity("TestSolution", "Test Solution", "0.9.0.0", true);
        // Create holding solution with different version
        CreateSolutionEntity("TestSolution_Upgrade", "Test Solution Upgrade", "1.0.0.0", true);

        // Act & Assert
        ps.AddCommand("Import-DataverseSolution")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionFile", solutionBytes)
          .AddParameter("Mode", ImportMode.HoldingSolution)
          .AddParameter("PollingIntervalSeconds", 1)
          .AddParameter("TimeoutSeconds", 30);
        
        var exception = Assert.Throws<CmdletInvocationException>(() => ps.Invoke());
        exception.InnerException.Should().BeOfType<InvalidOperationException>();
        exception.InnerException!.Message.Should().Contain("Cannot import holding solution");
        exception.InnerException!.Message.Should().Contain("already exists with version");
    }

    [Fact]
    public void ImportDataverseSolution_ProceedsWithHoldingSolutionImport_WhenNoHoldingSolutionExists()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var solutionBytes = CreateSolutionZipBytes("TestSolution", "Test Solution", "2.0.0.0", true);
        var mockConnection = CreateMockConnectionWithAsyncImport();
        
        // Create base solution (but no holding solution)
        CreateSolutionEntity("TestSolution", "Test Solution", "1.0.0.0", true);

        // Act
        ps.AddCommand("Import-DataverseSolution")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionFile", solutionBytes)
          .AddParameter("Mode", ImportMode.HoldingSolution)
          .AddParameter("PollingIntervalSeconds", 1)
          .AddParameter("TimeoutSeconds", 30);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        var result = results[0];
        result.Should().NotBeNull();
        result.Properties["ImportJobId"]?.Value.Should().NotBeNull();
    }

    [Fact]
    public void ImportDataverseSolution_WaitsForSolutionHistoryToClear_BeforeContinuing()
    {
        using var ps = CreatePowerShellWithCmdlets();
        var solutionBytes = CreateSolutionZipBytes("TestSolution", "Test Solution", "1.0.0.0", true);
        var statusesBySolution = new Dictionary<string, Queue<int?>>(StringComparer.OrdinalIgnoreCase)
        {
            ["TestSolution"] = new Queue<int?>(new int?[] { 0, 1 })
        };
        var mockConnection = CreateMockConnectionWithAsyncImport(solutionName =>
        {
            if (!statusesBySolution.TryGetValue(solutionName, out var statuses))
            {
                return null;
            }

            var status = statuses.Peek();
            if (statuses.Count > 1)
            {
                statuses.Dequeue();
            }

            return status;
        });

        ps.AddCommand("Import-DataverseSolution")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionFile", solutionBytes)
          .AddParameter("PollingIntervalSeconds", 1)
          .AddParameter("SolutionHistoryWaitSeconds", 3)
          .AddParameter("TimeoutSeconds", 30)
          .AddParameter("Verbose", true);
        var results = ps.Invoke();

        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        ps.Streams.Verbose.Should().Contain(v => v.Message.Contains("Solution history operation still in progress"));
    }

    [Fact]
    public void ImportDataverseSolution_FailsWhenSolutionHistoryDoesNotClear()
    {
        using var ps = CreatePowerShellWithCmdlets();
        var solutionBytes = CreateSolutionZipBytes("TestSolution", "Test Solution", "1.0.0.0", true);
        var mockConnection = CreateMockConnectionWithAsyncImport(solutionName =>
            string.Equals(solutionName, "TestSolution", StringComparison.OrdinalIgnoreCase) ? 0 : null);

        ps.AddCommand("Import-DataverseSolution")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionFile", solutionBytes)
          .AddParameter("PollingIntervalSeconds", 1)
          .AddParameter("SolutionHistoryWaitSeconds", 1)
          .AddParameter("TimeoutSeconds", 30);

        var exception = Assert.Throws<CmdletInvocationException>(() => ps.Invoke());
        exception.InnerException.Should().BeOfType<TimeoutException>();
        exception.InnerException!.Message.Should().Contain("Timed out after 1 seconds");
    }

    [Fact]
    public void ImportDataverseSolution_CanSkipSolutionHistoryCheck()
    {
        using var ps = CreatePowerShellWithCmdlets();
        var solutionBytes = CreateSolutionZipBytes("TestSolution", "Test Solution", "1.0.0.0", true);
        var solutionHistoryQueryCount = 0;
        var mockConnection = CreateMockConnectionWithAsyncImport(solutionName =>
        {
            solutionHistoryQueryCount++;
            return string.Equals(solutionName, "TestSolution", StringComparison.OrdinalIgnoreCase) ? 0 : null;
        });

        ps.AddCommand("Import-DataverseSolution")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionFile", solutionBytes)
          .AddParameter("SkipSolutionHistoryCheck", true)
          .AddParameter("PollingIntervalSeconds", 1)
          .AddParameter("TimeoutSeconds", 30);
        var results = ps.Invoke();

        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        solutionHistoryQueryCount.Should().Be(0);
    }

    [Fact]
    public void RemoveDataverseSolution_ChecksRelatedUpgradeSolutionHistory()
    {
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnectionWithAsyncUninstall(solutionName =>
            string.Equals(solutionName, "TestSolution_Upgrade", StringComparison.OrdinalIgnoreCase) ? 0 : null);
        CreateSolutionEntity("TestSolution", "Test Solution", "1.0.0.0", true);

        ps.AddCommand("Remove-DataverseSolution")
          .AddParameter("Connection", mockConnection)
          .AddParameter("UniqueName", "TestSolution")
          .AddParameter("SolutionHistoryWaitSeconds", 1)
          .AddParameter("PollingIntervalSeconds", 1)
          .AddParameter("TimeoutSeconds", 30)
          .AddParameter("Confirm", false);

        var exception = Assert.Throws<CmdletInvocationException>(() => ps.Invoke());
        exception.InnerException.Should().BeOfType<TimeoutException>();
        exception.InnerException!.Message.Should().Contain("TestSolution_Upgrade");
    }

    [Fact]
    public void InvokeDataverseSolutionUpgrade_ChecksHoldingSolutionHistory()
    {
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnectionWithAsyncUpgrade(solutionName =>
            string.Equals(solutionName, "TestSolution_Upgrade", StringComparison.OrdinalIgnoreCase) ? 0 : null);

        ps.AddCommand("Invoke-DataverseSolutionUpgrade")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionName", "TestSolution")
          .AddParameter("SolutionHistoryWaitSeconds", 1)
          .AddParameter("PollingIntervalSeconds", 1)
          .AddParameter("TimeoutSeconds", 30)
          .AddParameter("Confirm", false);

        var exception = Assert.Throws<CmdletInvocationException>(() => ps.Invoke());
        exception.InnerException.Should().BeOfType<TimeoutException>();
        exception.InnerException!.Message.Should().Contain("TestSolution_Upgrade");
    }

    // Import-DataverseSolution Component Parameter Filtering Tests (Import-DataverseSolution-ComponentParameterFiltering.Tests.ps1 - 3 tests)

    [Fact]
    public void ImportDataverseSolution_DoesNotThrow_WhenExtraConnectionReferencesProvided()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var solutionBytes = CreateSolutionZipWithConnectionReference("TestSolution", "Test Solution", "1.0.0.0", 
            "new_existingconnref", "shared_sharepointonline");
        var mockConnection = CreateMockConnectionWithAsyncImport();
        
        // Provide extra connection references that don't exist in the solution
        var connectionRefs = new System.Collections.Hashtable
        {
            { "new_existingconnref", Guid.NewGuid().ToString() },
            { "new_nonexistentconnref1", Guid.NewGuid().ToString() },
            { "new_nonexistentconnref2", Guid.NewGuid().ToString() }
        };

        // Act
        ps.AddCommand("Import-DataverseSolution")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionFile", solutionBytes)
          .AddParameter("ConnectionReferences", connectionRefs)
          .AddParameter("PollingIntervalSeconds", 1)
          .AddParameter("TimeoutSeconds", 30);
        var results = ps.Invoke();

        // Assert - should not throw, extra parameters are ignored
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        var result = results[0];
        result.Should().NotBeNull();
        result.Properties["ImportJobId"]?.Value.Should().NotBeNull();
    }

    [Fact]
    public void ImportDataverseSolution_DoesNotThrow_WhenExtraEnvironmentVariablesProvided()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var solutionBytes = CreateSolutionZipWithEnvironmentVariable("TestSolution", "Test Solution", "1.0.0.0", "new_apiurl");
        var mockConnection = CreateMockConnectionWithAsyncImport();
        
        // Provide extra environment variables that don't exist in the solution
        var envVars = new System.Collections.Hashtable
        {
            { "new_apiurl", "https://api.example.com" },
            { "new_nonexistentvar1", "value1" },
            { "new_nonexistentvar2", "value2" }
        };

        // Act
        ps.AddCommand("Import-DataverseSolution")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionFile", solutionBytes)
          .AddParameter("EnvironmentVariables", envVars)
          .AddParameter("PollingIntervalSeconds", 1)
          .AddParameter("TimeoutSeconds", 30);
        var results = ps.Invoke();

        // Assert - should not throw, extra parameters are ignored
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        var result = results[0];
        result.Should().NotBeNull();
        result.Properties["ImportJobId"]?.Value.Should().NotBeNull();
    }

    [Fact]
    public void ImportDataverseSolution_Handles_AllProvidedParametersNotInSolution()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var solutionBytes = CreateSolutionZipBytes("TestSolution", "Test Solution", "1.0.0.0", false);
        var mockConnection = CreateMockConnectionWithAsyncImport();
        
        // Provide connection references and environment variables that don't exist in the solution
        var connectionRefs = new System.Collections.Hashtable
        {
            { "new_nonexistentconnref", Guid.NewGuid().ToString() }
        };
        var envVars = new System.Collections.Hashtable
        {
            { "new_nonexistentvar", "value" }
        };

        // Act
        ps.AddCommand("Import-DataverseSolution")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionFile", solutionBytes)
          .AddParameter("ConnectionReferences", connectionRefs)
          .AddParameter("EnvironmentVariables", envVars)
          .AddParameter("PollingIntervalSeconds", 1)
          .AddParameter("TimeoutSeconds", 30);
        var results = ps.Invoke();

        // Assert - should not throw, all parameters are ignored
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        var result = results[0];
        result.Should().NotBeNull();
        result.Properties["ImportJobId"]?.Value.Should().NotBeNull();
    }

    // Import-DataverseSolution Case Insensitive Tests (Import-DataverseSolution-CaseInsensitive.Tests.ps1 - ~1 test)

    [Fact]
    public void ImportDataverseSolution_ConnectionReferencesAndEnvVars_AreCaseInsensitive()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var solutionBytes = CreateSolutionZipWithConnectionReferenceAndEnvironmentVariable(
            "TestSolution", "Test Solution", "1.0.0.0", 
            "new_SharedConnectionRef", "shared_sharepointonline",
            "new_ApiUrl");
        var mockConnection = CreateMockConnectionWithAsyncImport();
        
        // Provide parameters with different casing
        var connectionRefs = new System.Collections.Hashtable
        {
            { "NEW_sharedconnectionref", Guid.NewGuid().ToString() } // Different casing
        };
        var envVars = new System.Collections.Hashtable
        {
            { "NEW_apiurl", "https://api.example.com" } // Different casing
        };

        // Act
        ps.AddCommand("Import-DataverseSolution")
          .AddParameter("Connection", mockConnection)
          .AddParameter("SolutionFile", solutionBytes)
          .AddParameter("ConnectionReferences", connectionRefs)
          .AddParameter("EnvironmentVariables", envVars)
          .AddParameter("PollingIntervalSeconds", 1)
          .AddParameter("TimeoutSeconds", 30);
        var results = ps.Invoke();

        // Assert - should match case-insensitively
        ps.HadErrors.Should().BeFalse();
        results.Should().HaveCount(1);
        var result = results[0];
        result.Should().NotBeNull();
        result.Properties["ImportJobId"]?.Value.Should().NotBeNull();
    }

    /// <summary>
    /// Creates a mock connection that simulates async solution import with job tracking.
    /// The mock intercepts ImportSolutionAsyncRequest and StageAndUpgradeAsyncRequest,
    /// creates mock asyncoperation and importjob entities, and simulates job progression.
    /// </summary>
    private ServiceClient CreateMockConnectionWithAsyncImport(Func<string, int?>? solutionHistoryStatusProvider = null)
    {
        // Track async operations and import jobs for multiple polling cycles
        var asyncOperations = new Dictionary<Guid, AsyncOperationState>();
        var importJobs = new Dictionary<string, ImportJobState>();
        int pollCount = 0;

        ServiceClient mockConnection = CreateMockConnection(
            request =>
            {
                // Intercept ImportSolutionAsyncRequest
                if (request is ImportSolutionAsyncRequest importRequest)
                {
                    var asyncOperationId = Guid.NewGuid();
                    var importJobId = Guid.NewGuid().ToString("N");

                    // Create initial async operation state
                    asyncOperations[asyncOperationId] = new AsyncOperationState
                    {
                        StatusCode = 0, // Waiting
                        Message = "Import queued",
                        FriendlyMessage = "Waiting to import solution"
                    };

                    // Create initial import job state
                    importJobs[importJobId] = new ImportJobState
                    {
                        Progress = 0
                    };

                    var response = new ImportSolutionAsyncResponse();
                    response.Results["ImportJobKey"] = importJobId;
                    response.Results["AsyncOperationId"] = asyncOperationId;
                    return response;
                }

                // Intercept StageAndUpgradeAsyncRequest
                if (request is StageAndUpgradeAsyncRequest stageRequest)
                {
                    var asyncOperationId = Guid.NewGuid();
                    var importJobId = Guid.NewGuid().ToString("N");

                    // Create initial async operation state
                    asyncOperations[asyncOperationId] = new AsyncOperationState
                    {
                        StatusCode = 0, // Waiting
                        Message = "Upgrade queued",
                        FriendlyMessage = "Waiting to stage and upgrade solution"
                    };

                    // Create initial import job state
                    importJobs[importJobId] = new ImportJobState
                    {
                        Progress = 0
                    };

                    var response = new StageAndUpgradeAsyncResponse();
                    response.Results["ImportJobKey"] = importJobId;
                    response.Results["AsyncOperationId"] = asyncOperationId;
                    return response;
                }

                // Intercept queries for solution history table
                if (request is RetrieveMultipleRequest solutionHistoryRequest &&
                    solutionHistoryRequest.Query is QueryExpression solutionHistoryQuery &&
                    solutionHistoryQuery.EntityName == "msdyn_solutionhistory")
                {
                    return CreateSolutionHistoryResponse(solutionHistoryQuery, solutionHistoryStatusProvider);
                }

                // Intercept queries for asyncoperation table
                if (request is RetrieveMultipleRequest rmRequest &&
                    rmRequest.Query is QueryExpression qe &&
                    qe.EntityName == "asyncoperation")
                {
                    // Extract the asyncoperationid from the query
                    var asyncOperationId = Guid.Empty;
                    foreach (var condition in qe.Criteria.Conditions)
                    {
                        if (condition.AttributeName == "asyncoperationid" &&
                            condition.Operator == ConditionOperator.Equal &&
                            condition.Values.Count > 0)
                        {
                            asyncOperationId = (Guid)condition.Values[0];
                            break;
                        }
                    }

                    if (asyncOperationId != Guid.Empty && asyncOperations.ContainsKey(asyncOperationId))
                    {
                        var state = asyncOperations[asyncOperationId];

                        // Simulate job progression: Waiting -> InProgress -> Succeeded
                        // Progress through states based on poll count
                        pollCount++;
                        if (pollCount == 1)
                        {
                            // First poll: Waiting
                            state.StatusCode = 0;
                            state.Message = "Import queued";
                            state.FriendlyMessage = "Waiting to import solution";
                        }
                        else if (pollCount == 2)
                        {
                            // Second poll: InProgress
                            state.StatusCode = 20;
                            state.Message = "Import in progress";
                            state.FriendlyMessage = "Importing solution components";
                        }
                        else
                        {
                            // Third+ poll: Succeeded
                            state.StatusCode = 30;
                            state.Message = "Import completed";
                            state.FriendlyMessage = "Solution imported successfully";
                        }

                        var asyncOperation = new Entity("asyncoperation", asyncOperationId);
                        asyncOperation["statuscode"] = new OptionSetValue(state.StatusCode);
                        asyncOperation["message"] = state.Message;
                        asyncOperation["friendlymessage"] = state.FriendlyMessage;

                        var response = new RetrieveMultipleResponse();
                        response.Results["EntityCollection"] = new EntityCollection(new List<Entity> { asyncOperation });
                        return response;
                    }
                }

                // Intercept queries for importjob table
                if (request is RetrieveMultipleRequest rmRequest2 &&
                    rmRequest2.Query is QueryExpression qe2 &&
                    qe2.EntityName == "importjob")
                {
                    // Extract the importjobid from the query
                    string? importJobId = null;
                    foreach (var condition in qe2.Criteria.Conditions)
                    {
                        if (condition.AttributeName == "importjobid" &&
                            condition.Operator == ConditionOperator.Equal &&
                            condition.Values.Count > 0)
                        {
                            importJobId = condition.Values[0].ToString();
                            break;
                        }
                    }

                    if (importJobId != null && importJobs.ContainsKey(importJobId))
                    {
                        var state = importJobs[importJobId];

                        // Update progress based on poll count
                        if (pollCount == 1)
                        {
                            state.Progress = 10;
                        }
                        else if (pollCount == 2)
                        {
                            state.Progress = 50;
                        }
                        else
                        {
                            state.Progress = 100;
                        }

                        var importJob = new Entity("importjob", Guid.Parse(importJobId));
                        importJob["progress"] = (double)state.Progress;

                        var response = new RetrieveMultipleResponse();
                        response.Results["EntityCollection"] = new EntityCollection(new List<Entity> { importJob });
                        return response;
                    }
                }

                // Intercept queries for solution table (to support DoesSolutionExist and GetInstalledSolutionVersion)
                if (request is RetrieveMultipleRequest rmRequest3 &&
                    rmRequest3.Query is QueryExpression qe3 &&
                    qe3.EntityName == "solution")
                {
                    // Let FakeXrmEasy handle solution queries - solutions were created via CreateSolutionEntity
                    // which uses the base service
                    return null;
                }

                return null; // Let default interceptor handle other requests
            },
            "contact");

        return mockConnection;
    }

    private ServiceClient CreateMockConnectionWithAsyncUninstall(Func<string, int?>? solutionHistoryStatusProvider = null)
    {
        var asyncOperations = new Dictionary<Guid, AsyncOperationState>();

        return CreateMockConnection(
            request =>
            {
                if (request is RetrieveMultipleRequest solutionHistoryRequest &&
                    solutionHistoryRequest.Query is QueryExpression solutionHistoryQuery &&
                    solutionHistoryQuery.EntityName == "msdyn_solutionhistory")
                {
                    return CreateSolutionHistoryResponse(solutionHistoryQuery, solutionHistoryStatusProvider);
                }

                if (request is UninstallSolutionAsyncRequest)
                {
                    var asyncOperationId = Guid.NewGuid();
                    asyncOperations[asyncOperationId] = new AsyncOperationState
                    {
                        StateCode = 3,
                        StatusCode = 30,
                        Message = "Uninstall completed"
                    };

                    var response = new UninstallSolutionAsyncResponse();
                    response.Results["AsyncOperationId"] = asyncOperationId;
                    return response;
                }

                if (request is RetrieveRequest retrieveRequest &&
                    retrieveRequest.Target.LogicalName == "asyncoperation" &&
                    asyncOperations.TryGetValue(retrieveRequest.Target.Id, out var state))
                {
                    var asyncOperation = new Entity("asyncoperation", retrieveRequest.Target.Id);
                    asyncOperation["statecode"] = new OptionSetValue(state.StateCode);
                    asyncOperation["statuscode"] = new OptionSetValue(state.StatusCode);
                    asyncOperation["message"] = state.Message;

                    var response = new RetrieveResponse();
                    response.Results["Entity"] = asyncOperation;
                    return response;
                }

                return null;
            },
            "contact");
    }

    private ServiceClient CreateMockConnectionWithAsyncUpgrade(Func<string, int?>? solutionHistoryStatusProvider = null)
    {
        var asyncOperations = new Dictionary<Guid, AsyncOperationState>();

        return CreateMockConnection(
            request =>
            {
                if (request is RetrieveMultipleRequest solutionHistoryRequest &&
                    solutionHistoryRequest.Query is QueryExpression solutionHistoryQuery &&
                    solutionHistoryQuery.EntityName == "msdyn_solutionhistory")
                {
                    return CreateSolutionHistoryResponse(solutionHistoryQuery, solutionHistoryStatusProvider);
                }

                if (request is ExecuteAsyncRequest executeAsyncRequest &&
                    executeAsyncRequest.Request is DeleteAndPromoteRequest)
                {
                    var asyncOperationId = Guid.NewGuid();
                    asyncOperations[asyncOperationId] = new AsyncOperationState
                    {
                        StateCode = 3,
                        StatusCode = 30,
                        Message = "Upgrade completed"
                    };

                    var response = new ExecuteAsyncResponse();
                    response.Results["AsyncJobId"] = asyncOperationId;
                    return response;
                }

                if (request is RetrieveRequest retrieveRequest &&
                    retrieveRequest.Target.LogicalName == "asyncoperation" &&
                    asyncOperations.TryGetValue(retrieveRequest.Target.Id, out var state))
                {
                    var asyncOperation = new Entity("asyncoperation", retrieveRequest.Target.Id);
                    asyncOperation["statecode"] = new OptionSetValue(state.StateCode);
                    asyncOperation["statuscode"] = new OptionSetValue(state.StatusCode);
                    asyncOperation["message"] = state.Message;

                    var response = new RetrieveResponse();
                    response.Results["Entity"] = asyncOperation;
                    return response;
                }

                return null;
            },
            "contact");
    }

    private static OrganizationResponse? CreateSolutionHistoryResponse(
        QueryExpression query,
        Func<string, int?>? solutionHistoryStatusProvider)
    {
        if (solutionHistoryStatusProvider == null)
        {
            return null;
        }

        var namesToQuery = query.Criteria.Conditions
            .Where(condition => condition.AttributeName == "msdyn_name")
            .SelectMany(condition => condition.Values.Cast<object>())
            .Select(value => value?.ToString())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Cast<string>()
            .ToArray();

        var entities = namesToQuery
            .Select(name => new
            {
                Name = name,
                Status = solutionHistoryStatusProvider(name)
            })
            .Where(result => result.Status.HasValue)
            .Select(result =>
            {
                var entity = new Entity("msdyn_solutionhistory", Guid.NewGuid());
                entity["msdyn_name"] = result.Name;
                entity["msdyn_status"] = new OptionSetValue(result.Status.Value);
                entity["msdyn_starttime"] = DateTime.UtcNow;
                return entity;
            })
            .ToList();

        var response = new RetrieveMultipleResponse();
        response.Results["EntityCollection"] = new EntityCollection(entities);
        return response;
    }

    /// <summary>
    /// State tracker for async operation entities during mock import.
    /// </summary>
    private class AsyncOperationState
    {
        public int StateCode { get; set; } = 3;
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string FriendlyMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// State tracker for import job entities during mock import.
    /// </summary>
    private class ImportJobState
    {
        public int Progress { get; set; }
    }

    private Guid GetSolutionIdByUniqueName(string uniqueName)
    {
        var query = new QueryExpression("solution")
        {
            ColumnSet = new ColumnSet("solutionid"),
            Criteria = new FilterExpression
            {
                Conditions =
                {
                    new ConditionExpression("uniquename", ConditionOperator.Equal, uniqueName)
                }
            },
            TopCount = 1
        };

        var result = Service!.RetrieveMultiple(query);
        return result.Entities.Single().Id;
    }

    private Entity GetSingleSolutionComponent(Guid componentId, int componentType)
    {
        var query = new QueryExpression("solutioncomponent")
        {
            ColumnSet = new ColumnSet("solutioncomponentid", "objectid", "componenttype", "rootcomponentbehavior"),
            Criteria = new FilterExpression
            {
                Conditions =
                {
                    new ConditionExpression("objectid", ConditionOperator.Equal, componentId),
                    new ConditionExpression("componenttype", ConditionOperator.Equal, componentType)
                }
            },
            TopCount = 1
        };

        return Service!.RetrieveMultiple(query).Entities.SingleOrDefault();
    }

    private static Entity CreateSolutionComponentEntity(Guid id, Guid solutionId, Guid componentId, int componentType, int behavior)
    {
        return new Entity("solutioncomponent", id)
        {
            ["solutionid"] = new EntityReference("solution", solutionId),
            ["objectid"] = componentId,
            ["componenttype"] = componentType,
            ["rootcomponentbehavior"] = new OptionSetValue(behavior)
        };
    }

    private static List<EntityMetadata> BuildSolutionComponentMetadata()
    {
        return new List<EntityMetadata>
        {
            BuildEntityMetadata(
                logicalName: "publisher",
                schemaName: "Publisher",
                primaryIdAttribute: "publisherid",
                primaryNameAttribute: "friendlyname",
                objectTypeCode: 7100,
                BuildGuidAttribute("publisherid", "PublisherId"),
                BuildStringAttribute("friendlyname", "FriendlyName"),
                BuildStringAttribute("uniquename", "UniqueName"),
                BuildStringAttribute("customizationprefix", "CustomizationPrefix")),
            BuildEntityMetadata(
                logicalName: "solution",
                schemaName: "Solution",
                primaryIdAttribute: "solutionid",
                primaryNameAttribute: "friendlyname",
                objectTypeCode: 7101,
                BuildGuidAttribute("solutionid", "SolutionId"),
                BuildStringAttribute("friendlyname", "FriendlyName"),
                BuildStringAttribute("uniquename", "UniqueName"),
                BuildStringAttribute("version", "Version"),
                BuildBooleanAttribute("ismanaged", "IsManaged"),
                BuildStringAttribute("description", "Description"),
                BuildLookupAttribute("publisherid", "PublisherId", "publisher")),
            BuildEntityMetadata(
                logicalName: "solutioncomponent",
                schemaName: "SolutionComponent",
                primaryIdAttribute: "solutioncomponentid",
                primaryNameAttribute: "solutioncomponentid",
                objectTypeCode: 7102,
                BuildGuidAttribute("solutioncomponentid", "SolutionComponentId"),
                BuildLookupAttribute("solutionid", "SolutionId", "solution"),
                BuildGuidAttribute("objectid", "ObjectId"),
                BuildIntegerAttribute("componenttype", "ComponentType"),
                BuildPicklistAttribute("rootcomponentbehavior", "RootComponentBehavior"))
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
        SetAttributeDefaults(attribute, logicalName, schemaName, AttributeTypeCode.Uniqueidentifier);
        return attribute;
    }

    private static StringAttributeMetadata BuildStringAttribute(string logicalName, string schemaName)
    {
        var attribute = new StringAttributeMetadata { MaxLength = 500 };
        SetAttributeDefaults(attribute, logicalName, schemaName, AttributeTypeCode.String);
        return attribute;
    }

    private static BooleanAttributeMetadata BuildBooleanAttribute(string logicalName, string schemaName)
    {
        var attribute = new BooleanAttributeMetadata();
        SetAttributeDefaults(attribute, logicalName, schemaName, AttributeTypeCode.Boolean);
        return attribute;
    }

    private static IntegerAttributeMetadata BuildIntegerAttribute(string logicalName, string schemaName)
    {
        var attribute = new IntegerAttributeMetadata();
        SetAttributeDefaults(attribute, logicalName, schemaName, AttributeTypeCode.Integer);
        return attribute;
    }

    private static PicklistAttributeMetadata BuildPicklistAttribute(string logicalName, string schemaName)
    {
        var attribute = new PicklistAttributeMetadata();
        SetAttributeDefaults(attribute, logicalName, schemaName, AttributeTypeCode.Picklist);
        return attribute;
    }

    private static LookupAttributeMetadata BuildLookupAttribute(string logicalName, string schemaName, params string[] targets)
    {
        var attribute = new LookupAttributeMetadata();
        SetAttributeDefaults(attribute, logicalName, schemaName, AttributeTypeCode.Lookup);
        SetMetadataProperty(attribute, nameof(LookupAttributeMetadata.Targets), targets);
        return attribute;
    }

    private static void SetAttributeDefaults(AttributeMetadata attribute, string logicalName, string schemaName, AttributeTypeCode type)
    {
        SetMetadataProperty(attribute, nameof(AttributeMetadata.LogicalName), logicalName);
        SetMetadataProperty(attribute, nameof(AttributeMetadata.SchemaName), schemaName);
        SetMetadataProperty(attribute, nameof(AttributeMetadata.AttributeType), type);
        SetMetadataProperty(attribute, nameof(AttributeMetadata.IsValidForRead), true);
    }

    private static void SetMetadataProperty(object target, string propertyName, object? value)
    {
        var property = target.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        property?.SetValue(target, value);
    }

    /// <summary>
    /// Creates a solution zip file with a connection reference component.
    /// </summary>
    private static byte[] CreateSolutionZipWithConnectionReference(
        string uniqueName, string friendlyName, string version,
        string connectionRefLogicalName, string connectorId)
    {
        var solutionXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<ImportExportXml xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <SolutionManifest>
    <UniqueName>{uniqueName}</UniqueName>
    <LocalizedNames>
      <LocalizedName description=""{friendlyName}"" languagecode=""1033"" />
    </LocalizedNames>
    <Descriptions>
      <Description description=""Test solution with connection reference"" languagecode=""1033"" />
    </Descriptions>
    <Version>{version}</Version>
    <Managed>0</Managed>
    <Publisher>
      <UniqueName>publisher</UniqueName>
      <LocalizedNames>
        <LocalizedName description=""Test Publisher"" languagecode=""1033"" />
      </LocalizedNames>
      <CustomizationPrefix>test</CustomizationPrefix>
    </Publisher>
    <RootComponents />
    <MissingDependencies />
  </SolutionManifest>
</ImportExportXml>";

        var connectionRefXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<connectionreference connectionreferencelogicalname=""{connectionRefLogicalName}"">
  <connectionreferenceconnectorid>/providers/Microsoft.PowerApps/apis/{connectorId}</connectionreferenceconnectorid>
  <connectionreferencedisplayname>SharePoint Connection</connectionreferencedisplayname>
</connectionreference>";

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            // Add solution.xml
            var solutionEntry = archive.CreateEntry("solution.xml");
            using (var entryStream = solutionEntry.Open())
            {
                var bytes = Encoding.UTF8.GetBytes(solutionXml);
                entryStream.Write(bytes, 0, bytes.Length);
            }

            // Add connection reference
            var connRefEntry = archive.CreateEntry($"connectionreferences/{connectionRefLogicalName}.xml");
            using (var entryStream = connRefEntry.Open())
            {
                var bytes = Encoding.UTF8.GetBytes(connectionRefXml);
                entryStream.Write(bytes, 0, bytes.Length);
            }
        }
        memoryStream.Position = 0;
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Creates a solution zip file with an environment variable component.
    /// </summary>
    private static byte[] CreateSolutionZipWithEnvironmentVariable(
        string uniqueName, string friendlyName, string version, string envVarSchemaName)
    {
        var solutionXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<ImportExportXml xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <SolutionManifest>
    <UniqueName>{uniqueName}</UniqueName>
    <LocalizedNames>
      <LocalizedName description=""{friendlyName}"" languagecode=""1033"" />
    </LocalizedNames>
    <Descriptions>
      <Description description=""Test solution with environment variable"" languagecode=""1033"" />
    </Descriptions>
    <Version>{version}</Version>
    <Managed>0</Managed>
    <Publisher>
      <UniqueName>publisher</UniqueName>
      <LocalizedNames>
        <LocalizedName description=""Test Publisher"" languagecode=""1033"" />
      </LocalizedNames>
      <CustomizationPrefix>test</CustomizationPrefix>
    </Publisher>
    <RootComponents />
    <MissingDependencies />
  </SolutionManifest>
</ImportExportXml>";

        var envVarXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<environmentvariabledefinition environmentvariabledefinitionname=""{envVarSchemaName}"">
  <displayname>API URL</displayname>
  <type>100000000</type>
  <schemaname>{envVarSchemaName}</schemaname>
</environmentvariabledefinition>";

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            // Add solution.xml
            var solutionEntry = archive.CreateEntry("solution.xml");
            using (var entryStream = solutionEntry.Open())
            {
                var bytes = Encoding.UTF8.GetBytes(solutionXml);
                entryStream.Write(bytes, 0, bytes.Length);
            }

            // Add environment variable
            var envVarEntry = archive.CreateEntry($"environmentvariabledefinitions/{envVarSchemaName}.xml");
            using (var entryStream = envVarEntry.Open())
            {
                var bytes = Encoding.UTF8.GetBytes(envVarXml);
                entryStream.Write(bytes, 0, bytes.Length);
            }
        }
        memoryStream.Position = 0;
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Creates a solution zip file with both connection reference and environment variable components.
    /// </summary>
    private static byte[] CreateSolutionZipWithConnectionReferenceAndEnvironmentVariable(
        string uniqueName, string friendlyName, string version,
        string connectionRefLogicalName, string connectorId, string envVarSchemaName)
    {
        var solutionXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<ImportExportXml xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <SolutionManifest>
    <UniqueName>{uniqueName}</UniqueName>
    <LocalizedNames>
      <LocalizedName description=""{friendlyName}"" languagecode=""1033"" />
    </LocalizedNames>
    <Descriptions>
      <Description description=""Test solution with components"" languagecode=""1033"" />
    </Descriptions>
    <Version>{version}</Version>
    <Managed>0</Managed>
    <Publisher>
      <UniqueName>publisher</UniqueName>
      <LocalizedNames>
        <LocalizedName description=""Test Publisher"" languagecode=""1033"" />
      </LocalizedNames>
      <CustomizationPrefix>test</CustomizationPrefix>
    </Publisher>
    <RootComponents />
    <MissingDependencies />
  </SolutionManifest>
</ImportExportXml>";

        var connectionRefXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<connectionreference connectionreferencelogicalname=""{connectionRefLogicalName}"">
  <connectionreferenceconnectorid>/providers/Microsoft.PowerApps/apis/{connectorId}</connectionreferenceconnectorid>
  <connectionreferencedisplayname>SharePoint Connection</connectionreferencedisplayname>
</connectionreference>";

        var envVarXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<environmentvariabledefinition environmentvariabledefinitionname=""{envVarSchemaName}"">
  <displayname>API URL</displayname>
  <type>100000000</type>
  <schemaname>{envVarSchemaName}</schemaname>
</environmentvariabledefinition>";

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            // Add solution.xml
            var solutionEntry = archive.CreateEntry("solution.xml");
            using (var entryStream = solutionEntry.Open())
            {
                var bytes = Encoding.UTF8.GetBytes(solutionXml);
                entryStream.Write(bytes, 0, bytes.Length);
            }

            // Add connection reference
            var connRefEntry = archive.CreateEntry($"connectionreferences/{connectionRefLogicalName}.xml");
            using (var entryStream = connRefEntry.Open())
            {
                var bytes = Encoding.UTF8.GetBytes(connectionRefXml);
                entryStream.Write(bytes, 0, bytes.Length);
            }

            // Add environment variable
            var envVarEntry = archive.CreateEntry($"environmentvariabledefinitions/{envVarSchemaName}.xml");
            using (var entryStream = envVarEntry.Open())
            {
                var bytes = Encoding.UTF8.GetBytes(envVarXml);
                entryStream.Write(bytes, 0, bytes.Length);
            }
        }
        memoryStream.Position = 0;
        return memoryStream.ToArray();
    }
}
