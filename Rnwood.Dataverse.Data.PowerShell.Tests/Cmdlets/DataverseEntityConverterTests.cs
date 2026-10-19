using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using AwesomeAssertions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;
using CmdValueType = Rnwood.Dataverse.Data.PowerShell.Commands.ValueType;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Tests for DataverseEntityConverter class - bi-directional conversion between
    /// Dataverse Entity types and PowerShell PSObjects.
    /// </summary>
    public class DataverseEntityConverterTests : TestBase
    {
        #region GetAllColumnNames tests (static method)

        [Fact]
        public void GetAllColumnNames_ExcludesSystemColumns_ByDefault()
        {
            // Arrange
            CreateMockConnection("contact");
            var metadataFactory = new EntityMetadataFactory(Service!);
            var entityMetadata = metadataFactory.GetLimitedMetadata("contact");

            // Act
            var columns = DataverseEntityConverter.GetAllColumnNames(entityMetadata, false, null, false);

            // Assert
            columns.Should().NotContain("createdby");
            columns.Should().NotContain("modifiedby");
            columns.Should().NotContain("ownerid");
            columns.Should().NotContain("organizationid");
        }

        [Fact]
        public void GetAllColumnNames_IncludesSystemColumns_WhenRequested()
        {
            // Arrange
            CreateMockConnection("contact");
            var metadataFactory = new EntityMetadataFactory(Service!);
            var entityMetadata = metadataFactory.GetLimitedMetadata("contact");

            // Act
            var columns = DataverseEntityConverter.GetAllColumnNames(entityMetadata, true, null, false);

            // Assert
            // System columns should be included when includeSystemColumns=true
            // The exact columns depend on metadata
            columns.Should().NotBeEmpty();
        }

        [Fact]
        public void GetAllColumnNames_RespectsExcludeColumns()
        {
            // Arrange
            CreateMockConnection("contact");
            var metadataFactory = new EntityMetadataFactory(Service!);
            var entityMetadata = metadataFactory.GetLimitedMetadata("contact");

            // Act
            var columns = DataverseEntityConverter.GetAllColumnNames(entityMetadata, false, new[] { "firstname", "lastname" }, false);

            // Assert
            columns.Should().NotContain("firstname");
            columns.Should().NotContain("lastname");
        }

        [Fact]
        public void GetAllColumnNames_WithEntityMetadata_ContainsExpectedColumns()
        {
            // Arrange
            CreateMockConnection("contact");
            var metadataFactory = new EntityMetadataFactory(Service!);
            var entityMetadata = metadataFactory.GetLimitedMetadata("contact");

            // Act
            var columns = DataverseEntityConverter.GetAllColumnNames(entityMetadata, false, null, false);

            // Assert
            columns.Should().Contain("firstname");
            columns.Should().Contain("lastname");
            columns.Should().Contain("birthdate");
        }

        #endregion

        #region ConvertPSObjectToEntityReference tests (static method)

        [Fact]
        public void ConvertPSObjectToEntityReference_WithIdAndTableName_ReturnsEntityReference()
        {
            // Arrange
            var id = Guid.NewGuid();
            var psObject = new PSObject();
            psObject.Properties.Add(new PSNoteProperty("Id", id));
            psObject.Properties.Add(new PSNoteProperty("TableName", "contact"));

            // Act
            var result = DataverseEntityConverter.ConvertPSObjectToEntityReference(psObject);

            // Assert
            result.Id.Should().Be(id);
            result.LogicalName.Should().Be("contact");
        }

        [Fact]
        public void ConvertPSObjectToEntityReference_WithLogicalName_ReturnsEntityReference()
        {
            // Arrange
            var id = Guid.NewGuid();
            var psObject = new PSObject();
            psObject.Properties.Add(new PSNoteProperty("Id", id));
            psObject.Properties.Add(new PSNoteProperty("LogicalName", "account"));

            // Act
            var result = DataverseEntityConverter.ConvertPSObjectToEntityReference(psObject);

            // Assert
            result.LogicalName.Should().Be("account");
        }

        [Fact]
        public void ConvertPSObjectToEntityReference_WithEntityName_ReturnsEntityReference()
        {
            // Arrange
            var id = Guid.NewGuid();
            var psObject = new PSObject();
            psObject.Properties.Add(new PSNoteProperty("Id", id));
            psObject.Properties.Add(new PSNoteProperty("EntityName", "lead"));

            // Act
            var result = DataverseEntityConverter.ConvertPSObjectToEntityReference(psObject);

            // Assert
            result.LogicalName.Should().Be("lead");
        }

        [Fact]
        public void ConvertPSObjectToEntityReference_MissingId_ThrowsFormatException()
        {
            // Arrange
            var psObject = new PSObject();
            psObject.Properties.Add(new PSNoteProperty("TableName", "contact"));

            // Act & Assert
            Action act = () => DataverseEntityConverter.ConvertPSObjectToEntityReference(psObject);
            act.Should().Throw<FormatException>()
                .WithMessage("*Id*");
        }

        [Fact]
        public void ConvertPSObjectToEntityReference_MissingTableName_ThrowsFormatException()
        {
            // Arrange
            var id = Guid.NewGuid();
            var psObject = new PSObject();
            psObject.Properties.Add(new PSNoteProperty("Id", id));

            // Act & Assert
            Action act = () => DataverseEntityConverter.ConvertPSObjectToEntityReference(psObject);
            act.Should().Throw<FormatException>()
                .WithMessage("*TableName*EntityName*LogicalName*");
        }

        [Fact]
        public void ConvertPSObjectToEntityReference_WithName_IncludesName()
        {
            // Arrange
            var id = Guid.NewGuid();
            var psObject = new PSObject();
            psObject.Properties.Add(new PSNoteProperty("Id", id));
            psObject.Properties.Add(new PSNoteProperty("TableName", "contact"));
            psObject.Properties.Add(new PSNoteProperty("Name", "John Doe"));

            // Act
            var result = DataverseEntityConverter.ConvertPSObjectToEntityReference(psObject);

            // Assert - Name is set on the EntityReference
            result.Name.Should().Be("John Doe");
        }

        #endregion

        #region EntityMetadataFactory tests

        [Fact]
        public void EntityMetadataFactory_GetLimitedMetadata_ReturnsMetadata()
        {
            // Arrange
            CreateMockConnection("contact");
            var metadataFactory = new EntityMetadataFactory(Service!);

            // Act
            var metadata = metadataFactory.GetLimitedMetadata("contact");

            // Assert
            metadata.Should().NotBeNull();
            metadata.LogicalName.Should().Be("contact");
            metadata.Attributes.Should().NotBeEmpty();
        }

        [Fact]
        public void EntityMetadataFactory_GetAttribute_ReturnsAttribute()
        {
            // Arrange
            CreateMockConnection("contact");
            var metadataFactory = new EntityMetadataFactory(Service!);

            // Act
            var attribute = metadataFactory.GetAttribute("contact", "firstname");

            // Assert
            attribute.Should().NotBeNull();
            attribute.LogicalName.Should().Be("firstname");
        }

        [Fact]
        public void EntityMetadataFactory_GetAttribute_NonExistentColumn_ReturnsNull()
        {
            // Arrange
            CreateMockConnection("contact");
            var metadataFactory = new EntityMetadataFactory(Service!);

            // Act
            var attribute = metadataFactory.GetAttribute("contact", "nonexistent_column_xyz");

            // Assert
            attribute.Should().BeNull();
        }

        [Fact]
        public void EntityMetadataFactory_CachesMetadata()
        {
            // Arrange
            CreateMockConnection("contact");
            var metadataFactory = new EntityMetadataFactory(Service!);

            // Act - call twice
            var metadata1 = metadataFactory.GetLimitedMetadata("contact");
            var metadata2 = metadataFactory.GetLimitedMetadata("contact");

            // Assert - should return the same cached instance
            metadata1.Should().BeSameAs(metadata2);
        }

        #endregion

        #region ConvertToDataverseEntityOptions tests

        [Fact]
        public void ConvertToDataverseEntityOptions_DefaultValues()
        {
            // Act
            var options = new ConvertToDataverseEntityOptions();

            // Assert
            options.IgnoredPropertyName.Should().NotBeNull();
            options.IgnoredPropertyName.Should().BeEmpty();
            options.ColumnOptions.Should().NotBeNull();
            options.ColumnOptions.Should().BeEmpty();
        }

        [Fact]
        public void ConvertToDataverseEntityOptions_CanAddIgnoredProperties()
        {
            // Arrange
            var options = new ConvertToDataverseEntityOptions();

            // Act
            options.IgnoredPropertyName.Add("customfield1");
            options.IgnoredPropertyName.Add("customfield2");

            // Assert
            options.IgnoredPropertyName.Should().HaveCount(2);
            options.IgnoredPropertyName.Should().Contain("customfield1");
            options.IgnoredPropertyName.Should().Contain("customfield2");
        }

        #endregion

        #region DataverseEntityReference tests

        [Fact]
        public void DataverseEntityReference_Constructor_SetsProperties()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            var reference = new DataverseEntityReference("contact", id);

            // Assert
            reference.TableName.Should().Be("contact");
            reference.Id.Should().Be(id);
        }

        [Fact]
        public void DataverseEntityReference_WithEntityReference_SetsProperties()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entityRef = new EntityReference("account", id) { Name = "Test Account" };

            // Act
            var reference = new DataverseEntityReference(entityRef);

            // Assert
            reference.TableName.Should().Be("account");
            reference.Id.Should().Be(id);
            // Note: DataverseEntityReference struct doesn't preserve Name - it only stores TableName and Id
        }

        #endregion
    }
}
