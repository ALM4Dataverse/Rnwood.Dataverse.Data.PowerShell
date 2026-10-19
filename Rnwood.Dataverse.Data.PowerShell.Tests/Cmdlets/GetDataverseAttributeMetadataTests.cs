using System;
using FluentAssertions;
using Microsoft.Xrm.Sdk.Metadata;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using System.Linq;
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
    /// Tests for Get-DataverseAttributeMetadata cmdlet
    /// Migrated from tests/Get-DataverseMetadata.Tests.ps1 (attribute-related tests)
    /// </summary>
    public class GetDataverseAttributeMetadataTests : TestBase, IDisposable
    {
        public GetDataverseAttributeMetadataTests()
        {
            SetDataverseConnectionAsDefaultCmdlet.ClearDefault();
        }

        public override void Dispose()
        {
            SetDataverseConnectionAsDefaultCmdlet.ClearDefault();
            base.Dispose();
        }

        // ===== Single Attribute Retrieval ===== (3 tests)

        [Fact]
        public void GetDataverseAttributeMetadata_SingleAttribute_ReturnsMetadata()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseAttributeMetadata", typeof(Commands.GetDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act
            ps.AddCommand("Get-DataverseAttributeMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("AttributeName", "firstname");

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);

            var attributeMetadata = results[0].BaseObject as AttributeMetadata;
            attributeMetadata.Should().NotBeNull();
            attributeMetadata!.LogicalName.Should().Be("firstname");
            attributeMetadata.SchemaName.Should().NotBeNullOrEmpty();
            attributeMetadata.AttributeType.Should().NotBeNull();
            attributeMetadata.EntityLogicalName.Should().Be("contact");
        }

        [Fact]
        public void GetDataverseAttributeMetadata_StringAttribute_ReturnsTypeSpecificProperties()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseAttributeMetadata", typeof(Commands.GetDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act - get string attribute
            ps.AddCommand("Get-DataverseAttributeMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("AttributeName", "firstname");

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);

            var attributeMetadata = results[0].BaseObject as StringAttributeMetadata;
            attributeMetadata.Should().NotBeNull();
            attributeMetadata!.MaxLength.Should().NotBeNull();
            attributeMetadata.MaxLength.Should().BeGreaterThan(0);
        }

        [Fact]
        public void GetDataverseAttributeMetadata_DefaultConnection_UsesDefaultConnection()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseAttributeMetadata", typeof(Commands.GetDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            try
            {
                SetDataverseConnectionAsDefaultCmdlet.SetDefault(mockConnection);

                // Act - call without -Connection
                ps.AddCommand("Get-DataverseAttributeMetadata")
                  .AddParameter("EntityName", "contact")
                  .AddParameter("AttributeName", "firstname");

                var results = ps.Invoke();

                // Assert
                ps.HadErrors.Should().BeFalse();
                results.Should().HaveCount(1);

                var attributeMetadata = results[0].BaseObject as AttributeMetadata;
                attributeMetadata.Should().NotBeNull();
                attributeMetadata!.LogicalName.Should().Be("firstname");
            }
            finally
            {
                SetDataverseConnectionAsDefaultCmdlet.ClearDefault();
            }
        }



        // ===== All Attributes Retrieval ===== (2 tests)

        [Fact]
        public void GetDataverseAttributeMetadata_AllAttributes_ReturnsAllAttributesForEntity()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseAttributeMetadata", typeof(Commands.GetDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act - get all attributes by not specifying AttributeName
            ps.AddCommand("Get-DataverseAttributeMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact");

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().NotBeEmpty();

            // Verify firstname is in the list
            var attributes = results.Select(r => r.BaseObject as AttributeMetadata).ToList();
            var firstnameAttr = attributes.FirstOrDefault(a => a?.LogicalName == "firstname");
            firstnameAttr.Should().NotBeNull();
        }

        [Fact]
        public void GetDataverseAttributeMetadata_AllAttributes_ReturnsSortedByLogicalName()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseAttributeMetadata", typeof(Commands.GetDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act
            ps.AddCommand("Get-DataverseAttributeMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact");

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().NotBeEmpty();

            // Verify sorting
            var logicalNames = results
                .Select(r => (r.BaseObject as AttributeMetadata)?.LogicalName)
                .Where(name => name != null)
                .ToList();

            var sortedNames = logicalNames.OrderBy(n => n).ToList();

            // Compare arrays - should already be sorted
            logicalNames.Should().BeEquivalentTo(sortedNames, options => options.WithStrictOrdering());
        }

        // ===== Published Metadata Retrieval ===== (3 tests)

        [Fact]
        public void GetDataverseAttributeMetadata_DefaultBehavior_RetrievesUnpublishedMetadata()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseAttributeMetadata", typeof(Commands.GetDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act - get metadata without -Published flag
            ps.AddCommand("Get-DataverseAttributeMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("AttributeName", "firstname");

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);

            var attributeMetadata = results[0].BaseObject as AttributeMetadata;
            attributeMetadata.Should().NotBeNull();
            attributeMetadata!.LogicalName.Should().Be("firstname");
        }

        [Fact]
        public void GetDataverseAttributeMetadata_PublishedFlag_RetrievesPublishedMetadata()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseAttributeMetadata", typeof(Commands.GetDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act - get only published metadata
            ps.AddCommand("Get-DataverseAttributeMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("AttributeName", "firstname")
              .AddParameter("Published", true);

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);

            var attributeMetadata = results[0].BaseObject as AttributeMetadata;
            attributeMetadata.Should().NotBeNull();
            attributeMetadata!.LogicalName.Should().Be("firstname");
        }

        [Fact]
        public void GetDataverseAttributeMetadata_PublishedFlagWithAllAttributes_RetrievesAllPublishedAttributes()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseAttributeMetadata", typeof(Commands.GetDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act - get all published attributes
            ps.AddCommand("Get-DataverseAttributeMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("Published", true);

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().NotBeEmpty();
        }
    }
}
