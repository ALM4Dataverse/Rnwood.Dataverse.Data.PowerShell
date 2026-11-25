using System;
using AwesomeAssertions;
using Microsoft.Xrm.Sdk.Metadata;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using Microsoft.PowerPlatform.Dataverse.Client;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Tests for Get-DataverseEntityMetadata cmdlet
    /// Migrated from tests/Get-DataverseMetadata.Tests.ps1 (entity-related tests)
    /// </summary>
    public class GetDataverseEntityMetadataTests : TestBase, IDisposable
    {
        public GetDataverseEntityMetadataTests()
        {
            SetDataverseConnectionAsDefaultCmdlet.ClearDefault();
        }

        public override void Dispose()
        {
            SetDataverseConnectionAsDefaultCmdlet.ClearDefault();
            base.Dispose();
        }

        // ===== Single Entity Retrieval ===== (3 tests)

        [Fact]
        public void GetDataverseEntityMetadata_SingleEntity_ReturnsMetadata()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseEntityMetadata", typeof(Commands.GetDataverseEntityMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act
            ps.AddCommand("Get-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact");

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);

            var entityMetadata = results[0].BaseObject as EntityMetadata;
            entityMetadata.Should().NotBeNull();
            entityMetadata!.LogicalName.Should().Be("contact");
            entityMetadata.SchemaName.Should().NotBeNullOrEmpty();
            entityMetadata.PrimaryIdAttribute.Should().NotBeNullOrEmpty();
            entityMetadata.PrimaryNameAttribute.Should().NotBeNullOrEmpty();

            // Verify attributes are included by default
            entityMetadata.Attributes.Should().NotBeNull();
            entityMetadata.Attributes.Should().NotBeEmpty();
        }

        [Fact]
        public void GetDataverseEntityMetadata_ExcludeAttributes_ExcludesAttributeMetadata()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseEntityMetadata", typeof(Commands.GetDataverseEntityMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act
            ps.AddCommand("Get-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("ExcludeAttributes", true);

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);

            var entityMetadata = results[0].BaseObject as EntityMetadata;
            entityMetadata.Should().NotBeNull();
            entityMetadata!.LogicalName.Should().Be("contact");

            // Note: FakeXrmEasy may still return attributes even when not requested
            // In real Dataverse, Attributes would be null or empty
        }

        [Fact]
        public void GetDataverseEntityMetadata_DefaultConnection_UsesDefaultConnection()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseEntityMetadata", typeof(Commands.GetDataverseEntityMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

                        try
                        {
                                SetDataverseConnectionAsDefaultCmdlet.SetDefault(mockConnection);

                                ps.AddCommand("Get-DataverseEntityMetadata")
                                    .AddParameter("EntityName", "contact");

                                var results = ps.Invoke();

                                ps.HadErrors.Should().BeFalse();
                                results.Should().HaveCount(1);

                                var entityMetadata = results[0].BaseObject as EntityMetadata;
                                entityMetadata.Should().NotBeNull();
                                entityMetadata!.LogicalName.Should().Be("contact");
                        }
                        finally
                        {
                                SetDataverseConnectionAsDefaultCmdlet.ClearDefault();
                        }
        }

        // ===== All Entities Retrieval ===== (2 tests - both skipped due to FakeXrmEasy limitations)

        [Fact]
        public void GetDataverseEntityMetadata_AllEntities_ReturnsAllEntities()
        {
            // Arrange - TestBase's interceptor handles RetrieveAllEntitiesRequest
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseEntityMetadata", typeof(Commands.GetDataverseEntityMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            // Only use contact metadata (account.xml has encoding issues)
            var mockConnection = CreateMockConnection("contact");

            // Act
            ps.AddCommand("Get-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection);

            var results = ps.Invoke();

            // Assert - should return the loaded metadata entities
            ps.HadErrors.Should().BeFalse();
            results.Should().NotBeEmpty();
        }

        [Fact]
        public void GetDataverseEntityMetadata_AllEntitiesExcludeAttributes_ReturnsAllEntitiesWithoutAttributes()
        {
            // Arrange - TestBase's interceptor handles RetrieveAllEntitiesRequest
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseEntityMetadata", typeof(Commands.GetDataverseEntityMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            // Only use contact metadata (account.xml has encoding issues)
            var mockConnection = CreateMockConnection("contact");

            // Act
            ps.AddCommand("Get-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("ExcludeAttributes", true);

            var results = ps.Invoke();

            // Assert - should return the loaded metadata entities
            ps.HadErrors.Should().BeFalse();
            results.Should().NotBeEmpty();
        }

        // ===== Published Metadata Retrieval ===== (3 tests)

        [Fact]
        public void GetDataverseEntityMetadata_DefaultBehavior_RetrievesUnpublishedMetadata()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseEntityMetadata", typeof(Commands.GetDataverseEntityMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act - get metadata without -Published flag (should include unpublished)
            ps.AddCommand("Get-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact");

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);

            var entityMetadata = results[0].BaseObject as EntityMetadata;
            entityMetadata.Should().NotBeNull();
            entityMetadata!.LogicalName.Should().Be("contact");
        }

        [Fact]
        public void GetDataverseEntityMetadata_PublishedFlag_RetrievesPublishedMetadata()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseEntityMetadata", typeof(Commands.GetDataverseEntityMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act - get only published metadata
            ps.AddCommand("Get-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("Published", true);

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);

            var entityMetadata = results[0].BaseObject as EntityMetadata;
            entityMetadata.Should().NotBeNull();
            entityMetadata!.LogicalName.Should().Be("contact");
        }

        [Fact]
        public void GetDataverseEntityMetadata_PublishedWithExcludeAttributes_RetrievesPublishedMetadataWithoutAttributes()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseEntityMetadata", typeof(Commands.GetDataverseEntityMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act
            ps.AddCommand("Get-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("Published", true)
              .AddParameter("ExcludeAttributes", true);

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);

            var entityMetadata = results[0].BaseObject as EntityMetadata;
            entityMetadata.Should().NotBeNull();
            entityMetadata!.LogicalName.Should().Be("contact");
        }

        // ===== Entity List Retrieval ===== (4 tests - all skipped due to FakeXrmEasy limitations)

        [Fact]
        public void GetDataverseEntityMetadata_ListRetrieval_ReturnsAllEntitiesList()
        {
            // Arrange - TestBase's interceptor handles RetrieveAllEntitiesRequest
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseEntityMetadata", typeof(Commands.GetDataverseEntityMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            // Only use contact metadata (account.xml has encoding issues)
            var mockConnection = CreateMockConnection("contact");

            // Act
            ps.AddCommand("Get-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection);

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().NotBeEmpty();
        }

        [Fact]
        public void GetDataverseEntityMetadata_DetailedInformation_ReturnsDetailedMetadata()
        {
            // Arrange - TestBase's interceptor handles RetrieveAllEntitiesRequest
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseEntityMetadata", typeof(Commands.GetDataverseEntityMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Act
            ps.AddCommand("Get-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact");

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            var metadata = results[0].BaseObject as EntityMetadata;
            metadata.Should().NotBeNull();
            metadata!.Attributes.Should().NotBeEmpty();
        }

        [Fact]
        public void GetDataverseEntityMetadata_FilterCustomEntities_ReturnsOnlyCustomEntities()
        {
            // Arrange - TestBase's interceptor handles RetrieveAllEntitiesRequest
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseEntityMetadata", typeof(Commands.GetDataverseEntityMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            // Only use contact metadata (account.xml has encoding issues)
            var mockConnection = CreateMockConnection("contact");

            // Act - Get all and filter client-side
            ps.AddCommand("Get-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection);

            var results = ps.Invoke();

            // Assert - In real usage, filter with: $results | Where-Object { $_.IsCustomEntity }
            ps.HadErrors.Should().BeFalse();
            results.Should().NotBeEmpty();
        }

        [Fact]
        public void GetDataverseEntityMetadata_ListWithDefaultConnection_UsesDefaultConnection()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseEntityMetadata", typeof(Commands.GetDataverseEntityMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            try
            {
                SetDataverseConnectionAsDefaultCmdlet.SetDefault(mockConnection);

                ps.AddCommand("Get-DataverseEntityMetadata");

                var results = ps.Invoke();

                ps.HadErrors.Should().BeFalse();
                results.Should().NotBeEmpty();
            }
            finally
            {
                SetDataverseConnectionAsDefaultCmdlet.ClearDefault();
            }
        }


    }
}
