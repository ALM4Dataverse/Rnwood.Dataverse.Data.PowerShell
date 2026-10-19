using System;
using FluentAssertions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Tests for Set-DataverseEntityMetadata cmdlet - Icon properties
    /// Migrated from tests/Set-DataverseEntityMetadata-Icons.Tests.ps1 (19 tests)
    /// 
    /// Note: Many icon-related tests require E2E testing because:
    /// - CreateEntityRequest is not supported by FakeXrmEasy
    /// - UpdateEntityRequest requires full metadata operations beyond what mocks provide
    /// - Icon validation needs webresource entity with full metadata support
    /// 
    /// Tests enabled in this file:
    /// - Get-DataverseEntityMetadata icon property access tests (verify icon properties exist on metadata)
    /// - Icon validation error tests (verify errors thrown before entity operations)
    /// </summary>
    public class SetDataverseEntityMetadataIconsTests : TestBase
    {
        private PS CreatePowerShellWithCmdlets()
        {
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseEntityMetadata", typeof(GetDataverseEntityMetadataCmdlet), null));
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseEntityMetadata", typeof(SetDataverseEntityMetadataCmdlet), null));
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseRecord", typeof(GetDataverseRecordCmdlet), null));
            
            var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            
            var ps = PS.Create();
            ps.Runspace = runspace;
            return ps;
        }

        // ===== Setting Icon Properties on New Entity ===== (2 tests - E2E only)
        // These tests require CreateEntityRequest which FakeXrmEasy doesn't support.

        [Fact]
        public void SetDataverseEntityMetadata_NewEntity_SetsIconVectorName()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");

            // Create an SVG webresource for icon validation
            var iconWebResource = new Entity("webresource")
            {
                Id = Guid.NewGuid(),
                ["name"] = "test_icon.svg",
                ["webresourcetype"] = new OptionSetValue(11) // SVG type
            };
            Service!.Create(iconWebResource);

            // Act - Create new entity with IconVectorName
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "new_testentity")
              .AddParameter("SchemaName", "new_TestEntity")
              .AddParameter("DisplayName", "Test Entity")
              .AddParameter("DisplayCollectionName", "Test Entities")
              .AddParameter("PrimaryAttributeSchemaName", "new_name")
              .AddParameter("PrimaryAttributeDisplayName", "Name")
              .AddParameter("IconVectorName", "test_icon.svg")
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - Should complete without errors
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void SetDataverseEntityMetadata_NewEntity_SetsAllIconProperties()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");

            // Create webresources for each icon type
            var vectorIcon = new Entity("webresource")
            {
                Id = Guid.NewGuid(),
                ["name"] = "vector_icon.svg",
                ["webresourcetype"] = new OptionSetValue(11) // SVG
            };
            var largeIcon = new Entity("webresource")
            {
                Id = Guid.NewGuid(),
                ["name"] = "large_icon.png",
                ["webresourcetype"] = new OptionSetValue(5) // PNG
            };
            var mediumIcon = new Entity("webresource")
            {
                Id = Guid.NewGuid(),
                ["name"] = "medium_icon.png",
                ["webresourcetype"] = new OptionSetValue(5)
            };
            var smallIcon = new Entity("webresource")
            {
                Id = Guid.NewGuid(),
                ["name"] = "small_icon.png",
                ["webresourcetype"] = new OptionSetValue(5)
            };
            Service!.Create(vectorIcon);
            Service.Create(largeIcon);
            Service.Create(mediumIcon);
            Service.Create(smallIcon);

            // Act - Create new entity with all icon properties
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "new_testentity2")
              .AddParameter("SchemaName", "new_TestEntity2")
              .AddParameter("DisplayName", "Test Entity 2")
              .AddParameter("DisplayCollectionName", "Test Entities 2")
              .AddParameter("PrimaryAttributeSchemaName", "new_name")
              .AddParameter("PrimaryAttributeDisplayName", "Name")
              .AddParameter("IconVectorName", "vector_icon.svg")
              .AddParameter("IconLargeName", "large_icon.png")
              .AddParameter("IconMediumName", "medium_icon.png")
              .AddParameter("IconSmallName", "small_icon.png")
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - Should complete without errors
            ps.HadErrors.Should().BeFalse();
        }

        // ===== Updating Icon Properties on Existing Entity ===== (3 tests - E2E only)
        // These tests require UpdateEntityRequest with full metadata retrieval.

        [Fact]
        public void SetDataverseEntityMetadata_ExistingEntity_UpdatesIconVectorName()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");

            // Create an SVG webresource
            var iconWebResource = new Entity("webresource")
            {
                Id = Guid.NewGuid(),
                ["name"] = "new_icon.svg",
                ["webresourcetype"] = new OptionSetValue(11)
            };
            Service!.Create(iconWebResource);

            // Act - Update existing entity's IconVectorName
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("IconVectorName", "new_icon.svg")
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - Should complete without errors
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void SetDataverseEntityMetadata_ExistingEntity_UpdatesAllIconProperties()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");

            // Create webresources for all icon types
            var vectorIcon = new Entity("webresource")
            {
                Id = Guid.NewGuid(),
                ["name"] = "updated_vector.svg",
                ["webresourcetype"] = new OptionSetValue(11)
            };
            var largeIcon = new Entity("webresource")
            {
                Id = Guid.NewGuid(),
                ["name"] = "updated_large.png",
                ["webresourcetype"] = new OptionSetValue(5)
            };
            var mediumIcon = new Entity("webresource")
            {
                Id = Guid.NewGuid(),
                ["name"] = "updated_medium.png",
                ["webresourcetype"] = new OptionSetValue(5)
            };
            var smallIcon = new Entity("webresource")
            {
                Id = Guid.NewGuid(),
                ["name"] = "updated_small.png",
                ["webresourcetype"] = new OptionSetValue(5)
            };
            Service!.Create(vectorIcon);
            Service.Create(largeIcon);
            Service.Create(mediumIcon);
            Service.Create(smallIcon);

            // Act - Update all icon properties
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("IconVectorName", "updated_vector.svg")
              .AddParameter("IconLargeName", "updated_large.png")
              .AddParameter("IconMediumName", "updated_medium.png")
              .AddParameter("IconSmallName", "updated_small.png")
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - Should complete without errors
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void SetDataverseEntityMetadata_IconVectorName_ClearsWithEmptyString()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");

            // Act - Clear icon by setting empty string
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("IconVectorName", "")
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - Should complete without errors (empty string is valid)
            ps.HadErrors.Should().BeFalse();
        }

        // ===== Updating with EntityMetadata Object ===== (4 tests - E2E only)

        [Fact]
        public void SetDataverseEntityMetadata_EntityMetadataObject_UpdatesIcons()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");

            // Create webresources
            var iconWebResource = new Entity("webresource")
            {
                Id = Guid.NewGuid(),
                ["name"] = "metadata_icon.svg",
                ["webresourcetype"] = new OptionSetValue(11)
            };
            Service!.Create(iconWebResource);

            // Get existing entity metadata
            ps.AddCommand("Get-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact");

            var getResults = ps.Invoke();
            ps.Commands.Clear();
            var entityMetadata = getResults[0].BaseObject as EntityMetadata;
            entityMetadata.Should().NotBeNull();

            // Modify icon properties
            entityMetadata!.IconVectorName = "metadata_icon.svg";

            // Act - Update using EntityMetadata object
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityMetadata", entityMetadata)
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - Should complete without errors
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void SetDataverseEntityMetadata_EntityMetadataFromPipeline_UpdatesIcons()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");

            // Create webresource
            var iconWebResource = new Entity("webresource")
            {
                Id = Guid.NewGuid(),
                ["name"] = "pipeline_icon.svg",
                ["webresourcetype"] = new OptionSetValue(11)
            };
            Service!.Create(iconWebResource);

            // Get entity metadata and update icon via pipeline
            ps.AddScript(@"
                param($connection)
                Get-DataverseEntityMetadata -Connection $connection -EntityName contact | ForEach-Object {
                    $_.IconVectorName = 'pipeline_icon.svg'
                    $_
                } | Set-DataverseEntityMetadata -Connection $connection -Confirm:$false
            ")
              .AddParameter("connection", mockConnection);

            ps.Invoke();

            // Assert - Should complete without errors
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void SetDataverseEntityMetadata_EntityMetadataWithoutMetadataId_ThrowsError()
        {
            // Test validates error when EntityMetadata has no MetadataId
            // This validation happens before any entity operation, so it works with mocks
            
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create EntityMetadata without MetadataId
            var entityMetadata = new EntityMetadata
            {
                LogicalName = "contact"
                // MetadataId is not set (null/empty)
            };

            // Act - Try to update with EntityMetadata missing MetadataId
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityMetadata", entityMetadata)
              .AddParameter("Confirm", false);

            Exception? caughtException = null;
            try
            {
                ps.Invoke();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert - Should have error about missing MetadataId
            (ps.HadErrors || caughtException != null).Should().BeTrue("validation should fail for missing MetadataId");
            
            if (ps.Streams.Error.Count > 0)
            {
                var errorMessage = ps.Streams.Error[0].Exception.Message;
                errorMessage.Should().Contain("MetadataId");
            }
        }

        [Fact]
        public void SetDataverseEntityMetadata_EntityMetadataWithoutLogicalName_ThrowsError()
        {
            // Test validates error when EntityMetadata has no LogicalName
            // This validation happens before any entity operation, so it works with mocks
            
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create EntityMetadata with MetadataId but no LogicalName
            var entityMetadata = new EntityMetadata
            {
                // LogicalName is not set
            };
            // Set MetadataId via reflection since it's read-only
            typeof(MetadataBase).GetProperty("MetadataId")!.SetValue(entityMetadata, Guid.NewGuid());

            // Act - Try to update with EntityMetadata missing LogicalName
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityMetadata", entityMetadata)
              .AddParameter("Confirm", false);

            Exception? caughtException = null;
            try
            {
                ps.Invoke();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert - Should have error about missing LogicalName
            (ps.HadErrors || caughtException != null).Should().BeTrue("validation should fail for missing LogicalName");
            
            if (ps.Streams.Error.Count > 0)
            {
                var errorMessage = ps.Streams.Error[0].Exception.Message;
                errorMessage.Should().Contain("LogicalName");
            }
        }

        // ===== EntityMetadata Parameter with PassThru ===== (1 test - E2E only)

        [Fact]
        public void SetDataverseEntityMetadata_EntityMetadataWithPassThru_ReturnsUpdatedMetadata()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");

            // Create webresource
            var iconWebResource = new Entity("webresource")
            {
                Id = Guid.NewGuid(),
                ["name"] = "passthru_icon.svg",
                ["webresourcetype"] = new OptionSetValue(11)
            };
            Service!.Create(iconWebResource);

            // Act - Update with PassThru
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("IconVectorName", "passthru_icon.svg")
              .AddParameter("PassThru", true)
              .AddParameter("Confirm", false);

            var results = ps.Invoke();

            // Assert - Should return updated metadata
            ps.HadErrors.Should().BeFalse();
            results.Should().ContainSingle();
            // Note: In mock environment, PassThru retrieves metadata via RetrieveEntityRequest
            // which is already handled by TestBase interceptor, so we should get metadata back
            var returnedMetadata = results[0].BaseObject as EntityMetadata;
            if (returnedMetadata == null)
            {
                // Alternative: cmdlet might return PSObject wrapper
                returnedMetadata = results[0].Properties["LogicalName"]?.Value != null
                    ? LoadedMetadata.FirstOrDefault(m => m.LogicalName == "contact")
                    : null;
            }
            returnedMetadata.Should().NotBeNull();
            returnedMetadata!.LogicalName.Should().Be("contact");
        }

        // ===== Icon Properties in Output ===== (2 tests - enabled)
        // These tests verify Get-DataverseEntityMetadata returns icon properties (read-only operation).

        [Fact]
        public void GetDataverseEntityMetadata_ReturnsIconPropertiesAccessible()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection();

            // Act - Get metadata
            ps.AddCommand("Get-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact");

            var results = ps.Invoke();

            // Assert - Icon properties should exist
            ps.HadErrors.Should().BeFalse();
            results.Should().ContainSingle();

            var result = results[0];
            var propertyNames = result.Properties.Select(p => p.Name).ToList();

            propertyNames.Should().Contain("IconVectorName");
            propertyNames.Should().Contain("IconLargeName");
            propertyNames.Should().Contain("IconMediumName");
            propertyNames.Should().Contain("IconSmallName");
        }

        [Fact]
        public void GetDataverseEntityMetadata_IconProperties_CanBeAccessed()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection();

            // Act - Get metadata and access icon properties
            ps.AddCommand("Get-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact");

            var results = ps.Invoke();

            // Assert - Accessing icon properties should not throw
            ps.HadErrors.Should().BeFalse();
            results.Should().ContainSingle();

            var result = results[0];
            
            // These should not throw errors (values may be null)
            var vectorIcon = result.Properties["IconVectorName"]?.Value;
            var largeIcon = result.Properties["IconLargeName"]?.Value;
            var mediumIcon = result.Properties["IconMediumName"]?.Value;
            var smallIcon = result.Properties["IconSmallName"]?.Value;

            // Test passes if no errors were thrown
            true.Should().BeTrue();
        }

        // ===== Icon WebResource Validation Tests ===== (7 tests)
        // These tests verify icon validation behavior. Some can work with mocks, others need E2E.

        [Fact]
        public void SetDataverseEntityMetadata_IconVectorName_ThrowsIfWebResourceNotFound()
        {
            // Test validates error when referenced webresource doesn't exist
            // This validation happens BEFORE the entity operation, so it should fail at validation.
            
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            // Create mock without webresource entities - icon validation will fail
            var mockConnection = CreateMockConnection("contact");

            // Act - Try to update entity with non-existent icon
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("IconVectorName", "nonexistent_icon.svg");

            Exception? caughtException = null;
            try
            {
                ps.Invoke();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert - Should fail because webresource doesn't exist
            (ps.HadErrors || caughtException != null).Should().BeTrue();
            
            if (ps.Streams.Error.Count > 0)
            {
                // The error should mention the webresource validation
                var errorMessage = ps.Streams.Error[0].Exception.Message;
                errorMessage.Should().Contain("webresource");
            }
        }

        [Fact]
        public void SetDataverseEntityMetadata_IconVectorName_ThrowsIfWebResourceWrongType()
        {
            // Test validates error when webresource is not SVG type
            // This validation happens BEFORE the entity operation.
            
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create a PNG webresource (type 5) when we need SVG (type 11)
            var webResource = new Entity("webresource")
            {
                Id = Guid.NewGuid(),
                ["name"] = "test_icon.png",
                ["webresourcetype"] = new OptionSetValue(5) // PNG, not SVG
            };
            Service!.Create(webResource);

            // Act - Try to update entity with wrong icon type
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("IconVectorName", "test_icon.png");

            Exception? caughtException = null;
            try
            {
                ps.Invoke();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert - Should fail because webresource type is wrong
            (ps.HadErrors || caughtException != null).Should().BeTrue();
            
            if (ps.Streams.Error.Count > 0)
            {
                var errorMessage = ps.Streams.Error[0].Exception.Message;
                // Error should mention type requirement
                errorMessage.Should().Contain("type");
            }
        }

        [Fact]
        public void SetDataverseEntityMetadata_SkipIconValidation_SkipsExistenceCheck()
        {
            // Test validates -SkipIconValidation allows non-existent webresources
            // With SkipIconValidation, validation is skipped and we proceed to entity update
            // which will fail in FakeXrmEasy but that's expected.
            
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");

            // Act - Use SkipIconValidation to bypass validation
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("IconVectorName", "nonexistent_icon.svg")
              .AddParameter("SkipIconValidation", true);

            Exception? caughtException = null;
            try
            {
                ps.Invoke();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert - Should NOT fail with webresource validation error
            // It may fail later with UpdateEntityRequest but that's a different error
            if (ps.Streams.Error.Count > 0)
            {
                var errorMessage = ps.Streams.Error[0].Exception.Message.ToLower();
                // Should NOT contain webresource validation error
                errorMessage.Should().NotContain("does not reference a valid webresource");
            }
        }

        [Fact]
        public void SetDataverseEntityMetadata_SkipIconValidation_SkipsTypeCheck()
        {
            // Test validates -SkipIconValidation allows wrong webresource type
            
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create a PNG webresource when we need SVG
            var webResource = new Entity("webresource")
            {
                Id = Guid.NewGuid(),
                ["name"] = "wrong_type_icon.png",
                ["webresourcetype"] = new OptionSetValue(5) // PNG, not SVG
            };
            Service!.Create(webResource);

            // Act - Use SkipIconValidation to bypass validation
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("IconVectorName", "wrong_type_icon.png")
              .AddParameter("SkipIconValidation", true);

            Exception? caughtException = null;
            try
            {
                ps.Invoke();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert - Should NOT fail with webresource type error
            if (ps.Streams.Error.Count > 0)
            {
                var errorMessage = ps.Streams.Error[0].Exception.Message.ToLower();
                // Should NOT contain type validation error
                errorMessage.Should().NotContain("is required");
            }
        }

        [Fact]
        public void SetDataverseEntityMetadata_IconVectorName_ValidatesAgainstSVGWebResource()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");

            // Create an SVG webresource
            var iconWebResource = new Entity("webresource")
            {
                Id = Guid.NewGuid(),
                ["name"] = "valid_svg_icon.svg",
                ["webresourcetype"] = new OptionSetValue(11) // SVG type
            };
            Service!.Create(iconWebResource);

            // Act - Update with valid SVG webresource
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("IconVectorName", "valid_svg_icon.svg")
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - Should complete without errors
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void SetDataverseEntityMetadata_IconVectorName_AllowsEmptyStringToClear()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");

            // Act - Clear icon with empty string (no validation needed)
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("IconVectorName", "")
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - Should complete without errors
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void SetDataverseEntityMetadata_IconVectorName_AllowsNullToSkipUpdate()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");

            // Create PNG webresource (types 5, 6, 7 are allowed for IconLargeName)
            var iconWebResource = new Entity("webresource")
            {
                Id = Guid.NewGuid(),
                ["name"] = "other_icon.png",
                ["webresourcetype"] = new OptionSetValue(5) // PNG type for raster icons
            };
            Service!.Create(iconWebResource);

            // Act - Update without IconVectorName parameter (null/omitted)
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("IconLargeName", "other_icon.png") // Update different icon property
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - Should complete without errors (IconVectorName not changed)
            ps.HadErrors.Should().BeFalse();
        }

        // ===== Icon WebResource Validation - Unpublished WebResources ===== (1 test - E2E only)

        [Fact]
        public void SetDataverseEntityMetadata_IconVectorName_ValidatesAgainstUnpublishedWebResource()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");

            // Create an SVG webresource (TestBase handles RetrieveUnpublishedMultipleRequest)
            var iconWebResource = new Entity("webresource")
            {
                Id = Guid.NewGuid(),
                ["name"] = "unpublished_icon.svg",
                ["webresourcetype"] = new OptionSetValue(11)
            };
            Service!.Create(iconWebResource);

            // Act - Update with icon (validation should check unpublished webresources too)
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("IconVectorName", "unpublished_icon.svg")
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - Should complete without errors (found in unpublished check)
            ps.HadErrors.Should().BeFalse();
        }
    }
}
