using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation.Runspaces;
using System.Reflection;
using AwesomeAssertions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets;

/// <summary>
/// Tests for plugin management cmdlets:
/// - Get-DataversePluginAssembly
/// - Get-DataversePluginType
/// - Get-DataversePluginStep
/// </summary>
public class PluginManagementTests : TestBase
{
    private PS CreatePowerShellWithPluginCmdlets()
    {
        var initialSessionState = InitialSessionState.CreateDefault();
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataversePluginAssembly", typeof(GetDataversePluginAssemblyCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataversePluginType", typeof(GetDataversePluginTypeCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataversePluginStep", typeof(GetDataversePluginStepCmdlet), null));

        var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
        runspace.Open();

        var ps = PS.Create();
        ps.Runspace = runspace;
        return ps;
    }

    [Fact]
    public void GetDataversePluginAssembly_FiltersByName_AndPopulatesLinkedNames()
    {
        using var ps = CreatePowerShellWithPluginCmdlets();
        var mockConnection = CreateMockConnectionWithCustomMetadata(null, BuildPluginMetadata());

        var organizationId = Environment!.OrganizationId;
        var managedIdentityId = Guid.NewGuid();
        var targetAssemblyId = Guid.NewGuid();

        Environment.Seed(
            new Entity("managedidentity")
            {
                Id = managedIdentityId,
                ["managedidentityid"] = managedIdentityId,
                ["name"] = "Managed Identity Alpha"
            },
            new Entity("pluginassembly")
            {
                Id = targetAssemblyId,
                ["pluginassemblyid"] = targetAssemblyId,
                ["name"] = "Rnwood.Target.Plugins",
                ["organizationid"] = new EntityReference("organization", organizationId),
                ["managedidentityid"] = new EntityReference("managedidentity", managedIdentityId)
            },
            new Entity("pluginassembly")
            {
                Id = Guid.NewGuid(),
                ["pluginassemblyid"] = Guid.NewGuid(),
                ["name"] = "Rnwood.Other.Plugins",
                ["organizationid"] = new EntityReference("organization", organizationId)
            });

        ps.AddCommand("Get-DataversePluginAssembly")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Name", "Rnwood.Target.Plugins");
        var results = ps.Invoke();

        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["Id"].Value.Should().Be(targetAssemblyId);
        results[0].Properties["name"].Value.Should().Be("Rnwood.Target.Plugins");
        results[0].Properties["organizationid"].Value.Should().Be("MockOrganization");
        results[0].Properties["managedidentityid"].Value.Should().BeOfType<DataverseEntityReference>();
        var managedIdentityReference = (DataverseEntityReference)results[0].Properties["managedidentityid"].Value;
        managedIdentityReference.TableName.Should().Be("managedidentity");
        managedIdentityReference.Id.Should().Be(managedIdentityId);
    }

    [Fact]
    public void GetDataversePluginType_FiltersByPluginAssemblyId()
    {
        using var ps = CreatePowerShellWithPluginCmdlets();
        var mockConnection = CreateMockConnectionWithCustomMetadata(null, BuildPluginMetadata());

        var organizationId = Environment!.OrganizationId;
        var targetAssemblyId = Guid.NewGuid();
        var otherAssemblyId = Guid.NewGuid();
        var targetTypeId = Guid.NewGuid();

        Environment.Seed(
            new Entity("pluginassembly")
            {
                Id = targetAssemblyId,
                ["pluginassemblyid"] = targetAssemblyId,
                ["name"] = "Rnwood.Target.Plugins",
                ["organizationid"] = new EntityReference("organization", organizationId)
            },
            new Entity("pluginassembly")
            {
                Id = otherAssemblyId,
                ["pluginassemblyid"] = otherAssemblyId,
                ["name"] = "Rnwood.Other.Plugins",
                ["organizationid"] = new EntityReference("organization", organizationId)
            },
            new Entity("plugintype")
            {
                Id = targetTypeId,
                ["plugintypeid"] = targetTypeId,
                ["typename"] = "Rnwood.Target.Plugins.ContactPlugin",
                ["pluginassemblyid"] = new EntityReference("pluginassembly", targetAssemblyId),
                ["organizationid"] = new EntityReference("organization", organizationId)
            },
            new Entity("plugintype")
            {
                Id = Guid.NewGuid(),
                ["plugintypeid"] = Guid.NewGuid(),
                ["typename"] = "Rnwood.Other.Plugins.AccountPlugin",
                ["pluginassemblyid"] = new EntityReference("pluginassembly", otherAssemblyId),
                ["organizationid"] = new EntityReference("organization", organizationId)
            });

        ps.AddCommand("Get-DataversePluginType")
          .AddParameter("Connection", mockConnection)
          .AddParameter("PluginAssemblyId", targetAssemblyId);
        var results = ps.Invoke();

        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["Id"].Value.Should().Be(targetTypeId);
        results[0].Properties["typename"].Value.Should().Be("Rnwood.Target.Plugins.ContactPlugin");
        results[0].Properties["pluginassemblyid"].Value.Should().Be("Rnwood.Target.Plugins");
        results[0].Properties["organizationid"].Value.Should().Be("MockOrganization");
    }

    [Fact]
    public void GetDataversePluginStep_FiltersByPluginTypeId()
    {
        using var ps = CreatePowerShellWithPluginCmdlets();
        var mockConnection = CreateMockConnectionWithCustomMetadata(null, BuildPluginMetadata());

        var organizationId = Environment!.OrganizationId;
        var assemblyId = Guid.NewGuid();
        var targetTypeId = Guid.NewGuid();
        var otherTypeId = Guid.NewGuid();
        var messageId = Guid.NewGuid();
        var filterId = Guid.NewGuid();
        var targetStepId = Guid.NewGuid();

        Environment.Seed(
            new Entity("pluginassembly")
            {
                Id = assemblyId,
                ["pluginassemblyid"] = assemblyId,
                ["name"] = "Rnwood.Target.Plugins",
                ["organizationid"] = new EntityReference("organization", organizationId)
            },
            new Entity("plugintype")
            {
                Id = targetTypeId,
                ["plugintypeid"] = targetTypeId,
                ["typename"] = "Rnwood.Target.Plugins.ContactPlugin",
                ["pluginassemblyid"] = new EntityReference("pluginassembly", assemblyId),
                ["organizationid"] = new EntityReference("organization", organizationId)
            },
            new Entity("plugintype")
            {
                Id = otherTypeId,
                ["plugintypeid"] = otherTypeId,
                ["typename"] = "Rnwood.Target.Plugins.OtherPlugin",
                ["pluginassemblyid"] = new EntityReference("pluginassembly", assemblyId),
                ["organizationid"] = new EntityReference("organization", organizationId)
            },
            new Entity("sdkmessage")
            {
                Id = messageId,
                ["sdkmessageid"] = messageId,
                ["name"] = "Create"
            },
            new Entity("sdkmessagefilter")
            {
                Id = filterId,
                ["sdkmessagefilterid"] = filterId,
                ["primaryobjecttypecode"] = "contact"
            },
            new Entity("sdkmessageprocessingstep")
            {
                Id = targetStepId,
                ["sdkmessageprocessingstepid"] = targetStepId,
                ["name"] = "Contact Create Step",
                ["plugintypeid"] = new EntityReference("plugintype", targetTypeId),
                ["sdkmessageid"] = new EntityReference("sdkmessage", messageId),
                ["sdkmessagefilterid"] = new EntityReference("sdkmessagefilter", filterId),
                ["organizationid"] = new EntityReference("organization", organizationId)
            },
            new Entity("sdkmessageprocessingstep")
            {
                Id = Guid.NewGuid(),
                ["sdkmessageprocessingstepid"] = Guid.NewGuid(),
                ["name"] = "Other Step",
                ["plugintypeid"] = new EntityReference("plugintype", otherTypeId),
                ["sdkmessageid"] = new EntityReference("sdkmessage", messageId),
                ["organizationid"] = new EntityReference("organization", organizationId)
            });

        ps.AddCommand("Get-DataversePluginStep")
          .AddParameter("Connection", mockConnection)
          .AddParameter("PluginTypeId", targetTypeId);
        var results = ps.Invoke();

        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["Id"].Value.Should().Be(targetStepId);
        results[0].Properties["name"].Value.Should().Be("Contact Create Step");
        results[0].Properties["plugintypeid"].Value.Should().Be("Rnwood.Target.Plugins.ContactPlugin");
        results[0].Properties["sdkmessageid"].Value.Should().Be("Create");
        results[0].Properties["organizationid"].Value.Should().Be("MockOrganization");
    }

    [Fact]
    public void PluginEnums_ExposeExpectedValues()
    {
        ((int)PluginAssemblyIsolationMode.Sandbox).Should().Be(2);
        ((int)PluginAssemblySourceType.Database).Should().Be(0);
        ((int)PluginStepStage.PreOperation).Should().Be(20);
        ((int)PluginStepMode.Synchronous).Should().Be(0);
        ((int)PluginStepImageType.PreImage).Should().Be(0);
        ((int)PluginStepDeployment.ServerOnly).Should().Be(0);
    }

    private static List<EntityMetadata> BuildPluginMetadata()
    {
        return new List<EntityMetadata>
        {
            BuildEntityMetadata(
                logicalName: "organization",
                schemaName: "Organization",
                primaryIdAttribute: "organizationid",
                primaryNameAttribute: "name",
                objectTypeCode: 1,
                BuildGuidAttribute("organizationid", "OrganizationId"),
                BuildStringAttribute("name", "Name")),
            BuildEntityMetadata(
                logicalName: "managedidentity",
                schemaName: "ManagedIdentity",
                primaryIdAttribute: "managedidentityid",
                primaryNameAttribute: "name",
                objectTypeCode: 10001,
                BuildGuidAttribute("managedidentityid", "ManagedIdentityId"),
                BuildStringAttribute("name", "Name")),
            BuildEntityMetadata(
                logicalName: "pluginassembly",
                schemaName: "PluginAssembly",
                primaryIdAttribute: "pluginassemblyid",
                primaryNameAttribute: "name",
                objectTypeCode: 4605,
                BuildGuidAttribute("pluginassemblyid", "PluginAssemblyId"),
                BuildStringAttribute("name", "Name"),
                BuildLookupAttribute("organizationid", "OrganizationId", "organization"),
                BuildLookupAttribute("managedidentityid", "ManagedIdentityId", "managedidentity")),
            BuildEntityMetadata(
                logicalName: "plugintype",
                schemaName: "PluginType",
                primaryIdAttribute: "plugintypeid",
                primaryNameAttribute: "typename",
                objectTypeCode: 4606,
                BuildGuidAttribute("plugintypeid", "PluginTypeId"),
                BuildStringAttribute("typename", "TypeName"),
                BuildLookupAttribute("pluginassemblyid", "PluginAssemblyId", "pluginassembly"),
                BuildLookupAttribute("organizationid", "OrganizationId", "organization")),
            BuildEntityMetadata(
                logicalName: "sdkmessage",
                schemaName: "SdkMessage",
                primaryIdAttribute: "sdkmessageid",
                primaryNameAttribute: "name",
                objectTypeCode: 4608,
                BuildGuidAttribute("sdkmessageid", "SdkMessageId"),
                BuildStringAttribute("name", "Name")),
            BuildEntityMetadata(
                logicalName: "sdkmessagefilter",
                schemaName: "SdkMessageFilter",
                primaryIdAttribute: "sdkmessagefilterid",
                primaryNameAttribute: "primaryobjecttypecode",
                objectTypeCode: 4607,
                BuildGuidAttribute("sdkmessagefilterid", "SdkMessageFilterId"),
                BuildStringAttribute("primaryobjecttypecode", "PrimaryObjectTypeCode")),
            BuildEntityMetadata(
                logicalName: "sdkmessageprocessingstep",
                schemaName: "SdkMessageProcessingStep",
                primaryIdAttribute: "sdkmessageprocessingstepid",
                primaryNameAttribute: "name",
                objectTypeCode: 4609,
                BuildGuidAttribute("sdkmessageprocessingstepid", "SdkMessageProcessingStepId"),
                BuildStringAttribute("name", "Name"),
                BuildLookupAttribute("plugintypeid", "PluginTypeId", "plugintype"),
                BuildLookupAttribute("sdkmessageid", "SdkMessageId", "sdkmessage"),
                BuildLookupAttribute("sdkmessagefilterid", "SdkMessageFilterId", "sdkmessagefilter"),
                BuildLookupAttribute("organizationid", "OrganizationId", "organization"))
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
}