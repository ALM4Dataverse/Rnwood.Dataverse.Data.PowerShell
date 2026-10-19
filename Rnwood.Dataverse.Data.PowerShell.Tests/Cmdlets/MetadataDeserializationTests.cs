using System.Reflection;
using System.Runtime.Serialization;
using AwesomeAssertions;
using Microsoft.Xrm.Sdk.Metadata;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Tests that verify metadata deserialization works correctly for all entities.
    /// These tests validate that the embedded XML metadata files can be loaded.
    /// </summary>
    public class MetadataDeserializationTests : TestBase
    {
        [Fact]
        public void ContactMetadata_CanBeDeserialized()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Rnwood.Dataverse.Data.PowerShell.Tests.Metadata.contact.xml";
            
            using var stream = assembly.GetManifestResourceStream(resourceName);
            stream.Should().NotBeNull($"Resource {resourceName} should exist");
            
            var serializer = new DataContractSerializer(typeof(EntityMetadata));
            var metadata = serializer.ReadObject(stream) as EntityMetadata;
            
            metadata.Should().NotBeNull();
            metadata!.LogicalName.Should().Be("contact");
        }

        [Fact]
        public void AccountMetadata_CanBeDeserialized()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "Rnwood.Dataverse.Data.PowerShell.Tests.Metadata.account.xml";
            
            using var stream = assembly.GetManifestResourceStream(resourceName);
            stream.Should().NotBeNull($"Resource {resourceName} should exist");
            
            var serializer = new DataContractSerializer(typeof(EntityMetadata));
            var metadata = serializer.ReadObject(stream) as EntityMetadata;
            
            metadata.Should().NotBeNull();
            metadata!.LogicalName.Should().Be("account");
        }

        [Fact]
        public void CreateMockConnection_WithContactAndAccount_LoadsBothMetadata()
        {
            // Arrange & Act - Test that CreateMockConnection works with both entities
            var mockConnection = CreateMockConnection("contact", "account");
            
            // Assert
            mockConnection.Should().NotBeNull();
            LoadedMetadata.Should().HaveCount(2);
            LoadedMetadata.Should().Contain(m => m.LogicalName == "contact");
            LoadedMetadata.Should().Contain(m => m.LogicalName == "account");
        }
    }
}
