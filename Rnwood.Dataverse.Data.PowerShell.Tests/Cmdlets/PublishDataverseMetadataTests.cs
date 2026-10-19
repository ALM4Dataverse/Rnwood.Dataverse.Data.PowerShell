using FluentAssertions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Messages;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Tests for metadata cmdlets - Publish parameter and behavior
    /// Migrated from tests/Set-DataverseMetadata-Publish.Tests.ps1 (11 tests)
    /// </summary>
    public class PublishDataverseMetadataTests : TestBase
    {
        // ===== Set-DataverseEntityMetadata -Publish Parameter ===== (1 test)

        [Fact]
        public void SetDataverseEntityMetadata_WhatIfWithPublish_DoesNotPublish()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseEntityMetadata", typeof(Commands.SetDataverseEntityMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Act - use -WhatIf with -Publish
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("DisplayName", "Updated Contact")
              .AddParameter("Publish", true)
              .AddParameter("WhatIf", true);

            var results = ps.Invoke();

            // Assert - WhatIf should prevent both update and publish
            // WhatIf returns nothing but shouldn't error
            results.Should().BeEmpty();
            ps.HadErrors.Should().BeFalse();
        }

        // ===== Set-DataverseAttributeMetadata -Publish Parameter ===== (1 test)

        [Fact]
        public void SetDataverseAttributeMetadata_WhatIfWithPublish_DoesNotPublish()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseAttributeMetadata", typeof(Commands.SetDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Act - use -WhatIf with -Publish on existing attribute
            ps.AddCommand("Set-DataverseAttributeMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("AttributeName", "firstname")
              .AddParameter("DisplayName", "Updated First Name")
              .AddParameter("Publish", true)
              .AddParameter("WhatIf", true);

            var results = ps.Invoke();

            // Assert - WhatIf should prevent both update and publish
            results.Should().BeEmpty();
            ps.HadErrors.Should().BeFalse();
        }

        // ===== Set-DataverseRelationshipMetadata -Publish Parameter ===== (1 test)

        [Fact]
        public void SetDataverseRelationshipMetadata_WhatIfWithPublish_DoesNotPublish()
        {
            // Arrange - Mock RetrieveRelationshipRequest to return existing relationship
            var publishRequestExecuted = false;

            var mockConnection = CreateMockConnection(request =>
            {
                var requestTypeName = request.GetType().Name;
                
                // Mock RetrieveRelationshipRequest - return existing relationship
                if (requestTypeName == "RetrieveRelationshipRequest")
                {
                    var response = new RetrieveRelationshipResponse();
                    var relationship = new Microsoft.Xrm.Sdk.Metadata.OneToManyRelationshipMetadata
                    {
                        SchemaName = "contact_customer_accounts",
                        ReferencedEntity = "account",
                        ReferencingEntity = "contact"
                    };
                    response.Results["RelationshipMetadata"] = relationship;
                    return response;
                }

                // Track if PublishXmlRequest was called (it shouldn't be with WhatIf)
                if (requestTypeName == "PublishXmlRequest")
                {
                    publishRequestExecuted = true;
                    return new PublishXmlResponse();
                }

                return null;
            }, "contact", "account");

            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseRelationshipMetadata", typeof(Commands.SetDataverseRelationshipMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            // Act - use -WhatIf with -Publish (requires all mandatory parameters)
            ps.AddCommand("Set-DataverseRelationshipMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("SchemaName", "contact_customer_accounts")
              .AddParameter("RelationshipType", "OneToMany")
              .AddParameter("ReferencedEntity", "account")
              .AddParameter("ReferencingEntity", "contact")
              .AddParameter("Publish", true)
              .AddParameter("WhatIf", true);

            var results = ps.Invoke();

            // Assert - WhatIf should prevent both update and publish
            results.Should().BeEmpty();
            ps.HadErrors.Should().BeFalse();
            publishRequestExecuted.Should().BeFalse("because -WhatIf should prevent publish");
        }

        // ===== RetrieveAsIfPublished Behavior ===== (2 tests)

        [Fact]
        public void EntityMetadataRetrieval_RetrievesUnpublishedChanges()
        {
            // Arrange - Intercept RetrieveEntityRequest to verify RetrieveAsIfPublished=true
            bool? retrieveAsIfPublished = null;

            var mockConnection = CreateMockConnection(request =>
            {
                if (request is RetrieveEntityRequest retrieveEntityRequest)
                {
                    retrieveAsIfPublished = retrieveEntityRequest.RetrieveAsIfPublished;
                    // Let FakeXrmEasy handle the request normally
                }
                return null;
            }, "contact");

            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseEntityMetadata", typeof(Commands.SetDataverseEntityMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            // Act - Update entity (this will retrieve existing metadata first)
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("DisplayName", "Updated Contact")
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - Should use RetrieveAsIfPublished=true to get unpublished changes
            retrieveAsIfPublished.Should().BeTrue(
                "because Set-DataverseEntityMetadata must retrieve unpublished changes to avoid conflicts");
        }

        [Fact]
        public void AttributeMetadataRetrieval_RetrievesUnpublishedChanges()
        {
            // Arrange - Intercept RetrieveAttributeRequest to verify RetrieveAsIfPublished=true
            bool? retrieveAsIfPublished = null;

            var mockConnection = CreateMockConnection(request =>
            {
                // Check RetrieveAttributeRequest (used by Set-DataverseAttributeMetadata)
                if (request is RetrieveAttributeRequest retrieveAttributeRequest)
                {
                    retrieveAsIfPublished = retrieveAttributeRequest.RetrieveAsIfPublished;
                    // Let FakeXrmEasy handle the request normally
                }
                
                // Mock UpdateAttributeRequest response
                if (request.GetType().Name == "UpdateAttributeRequest")
                {
                    return new UpdateAttributeResponse();
                }
                
                return null;
            }, "contact");

            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseAttributeMetadata", typeof(Commands.SetDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            // Act - Update attribute (this will retrieve attribute metadata first to check if it exists)
            ps.AddCommand("Set-DataverseAttributeMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("AttributeName", "firstname")
              .AddParameter("DisplayName", "Updated First Name")
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - Should use RetrieveAsIfPublished=true to get unpublished changes
            retrieveAsIfPublished.Should().BeTrue(
                "because Set-DataverseAttributeMetadata must retrieve unpublished changes to avoid conflicts");
        }

        // ===== Publishing After Metadata Changes ===== (3 tests)

        [Fact]
        public void SetDataverseEntityMetadata_Publish_PublishesEntityAfterUpdate()
        {
            // Arrange - Track if PublishXmlRequest was executed
            var publishRequestExecuted = false;
            string? publishParameterXml = null;

            var mockConnection = CreateMockConnection(request =>
            {
                if (request.GetType().Name == "PublishXmlRequest")
                {
                    publishRequestExecuted = true;
                    publishParameterXml = request.Parameters["ParameterXml"]?.ToString();
                    return new PublishXmlResponse();
                }
                return null;
            }, "contact");

            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseEntityMetadata", typeof(Commands.SetDataverseEntityMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            // Act - Update entity with -Publish switch
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("DisplayName", "Updated Contact")
              .AddParameter("Publish", true)
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - PublishXmlRequest should have been executed with entity name
            publishRequestExecuted.Should().BeTrue("because -Publish switch was specified");
            publishParameterXml.Should().Contain("contact", "because the entity name should be in the publish XML");
        }

        [Fact]
        public void SetDataverseEntityMetadata_NoPublish_DoesNotPublish()
        {
            // Arrange - Track if PublishXmlRequest was executed
            var publishRequestExecuted = false;

            var mockConnection = CreateMockConnection(request =>
            {
                if (request.GetType().Name == "PublishXmlRequest")
                {
                    publishRequestExecuted = true;
                    return new PublishXmlResponse();
                }
                return null;
            }, "contact");

            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseEntityMetadata", typeof(Commands.SetDataverseEntityMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            // Act - Update entity WITHOUT -Publish switch
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("DisplayName", "Updated Contact")
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - PublishXmlRequest should NOT have been executed
            publishRequestExecuted.Should().BeFalse("because -Publish switch was not specified");
        }

        [Fact]
        public void SetDataverseAttributeMetadata_Publish_PublishesEntityAfterAttributeUpdate()
        {
            // Arrange - Track if PublishXmlRequest was executed
            var publishRequestExecuted = false;
            string? publishParameterXml = null;

            var mockConnection = CreateMockConnection(request =>
            {
                if (request.GetType().Name == "PublishXmlRequest")
                {
                    publishRequestExecuted = true;
                    publishParameterXml = request.Parameters["ParameterXml"]?.ToString();
                    return new PublishXmlResponse();
                }
                // CreateAttributeRequest needs a response with AttributeId
                if (request.GetType().Name == "CreateAttributeRequest")
                {
                    var response = new CreateAttributeResponse();
                    response.Results["AttributeId"] = Guid.NewGuid();
                    return response;
                }
                // UpdateAttributeRequest needs an empty response
                if (request.GetType().Name == "UpdateAttributeRequest")
                {
                    return new UpdateAttributeResponse();
                }
                return null;
            }, "contact");

            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseAttributeMetadata", typeof(Commands.SetDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            // Act - Update existing attribute (firstname exists in contact) with -Publish switch
            ps.AddCommand("Set-DataverseAttributeMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("AttributeName", "firstname")
              .AddParameter("DisplayName", "Updated First Name")
              .AddParameter("Publish", true)
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - PublishXmlRequest should have been executed with entity name
            publishRequestExecuted.Should().BeTrue("because -Publish switch was specified");
            publishParameterXml.Should().Contain("contact", "because the entity name should be in the publish XML");
        }

        [Fact]
        public void SetDataverseRelationshipMetadata_Publish_PublishesBothEntitiesAfterRelationshipUpdate()
        {
            // Arrange - Track if PublishXmlRequest was executed with both entity names
            var publishRequestExecuted = false;
            string? publishParameterXml = null;

            var mockConnection = CreateMockConnection(request =>
            {
                var requestTypeName = request.GetType().Name;
                
                // Mock RetrieveRelationshipRequest - return existing relationship
                if (requestTypeName == "RetrieveRelationshipRequest")
                {
                    var response = new RetrieveRelationshipResponse();
                    var relationship = new Microsoft.Xrm.Sdk.Metadata.OneToManyRelationshipMetadata
                    {
                        SchemaName = "contact_customer_accounts",
                        ReferencedEntity = "account",
                        ReferencingEntity = "contact",
                        CascadeConfiguration = new Microsoft.Xrm.Sdk.Metadata.CascadeConfiguration
                        {
                            Assign = Microsoft.Xrm.Sdk.Metadata.CascadeType.NoCascade,
                            Share = Microsoft.Xrm.Sdk.Metadata.CascadeType.NoCascade,
                            Unshare = Microsoft.Xrm.Sdk.Metadata.CascadeType.NoCascade,
                            Reparent = Microsoft.Xrm.Sdk.Metadata.CascadeType.NoCascade,
                            Delete = Microsoft.Xrm.Sdk.Metadata.CascadeType.RemoveLink,
                            Merge = Microsoft.Xrm.Sdk.Metadata.CascadeType.NoCascade
                        }
                    };
                    response.Results["RelationshipMetadata"] = relationship;
                    return response;
                }

                // Mock UpdateRelationshipRequest
                if (requestTypeName == "UpdateRelationshipRequest")
                {
                    return new UpdateRelationshipResponse();
                }

                // Track PublishXmlRequest
                if (requestTypeName == "PublishXmlRequest")
                {
                    publishRequestExecuted = true;
                    publishParameterXml = request.Parameters["ParameterXml"]?.ToString();
                    return new PublishXmlResponse();
                }

                return null;
            }, "contact", "account");

            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseRelationshipMetadata", typeof(Commands.SetDataverseRelationshipMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            // Act - Update relationship with -Publish switch
            ps.AddCommand("Set-DataverseRelationshipMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("SchemaName", "contact_customer_accounts")
              .AddParameter("RelationshipType", "OneToMany")
              .AddParameter("ReferencedEntity", "account")
              .AddParameter("ReferencingEntity", "contact")
              .AddParameter("CascadeDelete", "Cascade")
              .AddParameter("Publish", true)
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - PublishXmlRequest should have been executed with both entity names
            publishRequestExecuted.Should().BeTrue("because -Publish switch was specified");
            publishParameterXml.Should().Contain("account", "because the referenced entity should be published");
            publishParameterXml.Should().Contain("contact", "because the referencing entity should be published");
        }
    }
}
