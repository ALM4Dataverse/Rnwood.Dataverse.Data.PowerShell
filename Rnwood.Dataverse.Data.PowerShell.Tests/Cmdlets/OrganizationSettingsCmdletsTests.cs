using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using AwesomeAssertions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets;

public class OrganizationSettingsCmdletsTests : TestBase
{
    private PS CreatePowerShellWithOrganizationSettingsCmdlets()
    {
        var initialSessionState = InitialSessionState.CreateDefault2();
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Get-DataverseOrganizationSettings", typeof(GetDataverseOrganizationSettingsCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Set-DataverseOrganizationSettings", typeof(SetDataverseOrganizationSettingsCmdlet), null));

        var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
        runspace.Open();

        var ps = PS.Create();
        ps.Runspace = runspace;
        return ps;
    }

    [Fact]
    public void GetDataverseOrganizationSettings_RetrievesOrganizationRecord()
    {
        using var ps = CreatePowerShellWithOrganizationSettingsCmdlets();
        var mockConnection = CreateMockConnectionWithCustomMetadata(null, BuildOrganizationMetadata());

        var organization = GetExistingOrganization();
        organization["name"] = "Test Org";
        organization["maximumtrackingnumber"] = 41;
        organization["orgdborgsettings"] = "<OrgSettings><AllowSaveAsDraftAppointment>true</AllowSaveAsDraftAppointment></OrgSettings>";
        Service!.Update(organization);

        ps.AddCommand("Get-DataverseOrganizationSettings")
          .AddParameter("Connection", mockConnection);
        var results = ps.Invoke();

        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error));
        results.Should().HaveCount(1);
        results[0].Properties["name"].Value.Should().Be("Test Org");
        results[0].Properties["maximumtrackingnumber"].Value.Should().Be(41);
        results[0].Properties.Select(property => property.Name).Should().NotContain("orgdborgsettings");
    }

    [Fact]
    public void GetDataverseOrganizationSettings_OrgDbOrgSettings_ParsesSettings()
    {
        using var ps = CreatePowerShellWithOrganizationSettingsCmdlets();
        var mockConnection = CreateMockConnectionWithCustomMetadata(null, BuildOrganizationMetadata());

        var organization = GetExistingOrganization();
        organization["orgdborgsettings"] = "<OrgSettings><AllowSaveAsDraftAppointment>true</AllowSaveAsDraftAppointment><MaximumTrackingNumber>5</MaximumTrackingNumber><SomeText>abc</SomeText></OrgSettings>";
        Service!.Update(organization);

        ps.AddCommand("Get-DataverseOrganizationSettings")
          .AddParameter("Connection", mockConnection)
          .AddParameter("OrgDbOrgSettings", true);
        var results = ps.Invoke();

        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error));
        results.Should().HaveCount(1);
        var properties = results[0].Properties.ToArray();
        properties.Select(property => property.Name).Should().Contain(new[]
        {
            "AllowSaveAsDraftAppointment",
            "MaximumTrackingNumber",
            "SomeText"
        });
        properties.First(property => property.Name == "AllowSaveAsDraftAppointment").Value.Should().Be(true);
        properties.First(property => property.Name == "MaximumTrackingNumber").Value.Should().Be(5);
        properties.First(property => property.Name == "SomeText").Value.Should().Be("abc");
    }

    [Fact]
    public void SetDataverseOrganizationSettings_UpdatesOrganizationColumn()
    {
        using var ps = CreatePowerShellWithOrganizationSettingsCmdlets();
        var mockConnection = CreateMockConnectionWithCustomMetadata(null, BuildOrganizationMetadata());

        var organization = GetExistingOrganization();
        organization["name"] = "Test Org";
        organization["maximumtrackingnumber"] = 41;
        Service!.Update(organization);

        ps.AddCommand("Set-DataverseOrganizationSettings")
          .AddParameter("Connection", mockConnection)
          .AddParameter("InputObject", new System.Collections.Hashtable
          {
              ["maximumtrackingnumber"] = 42,
          })
          .AddParameter("Confirm", false)
          .AddParameter("PassThru", true);
        var results = ps.Invoke();

        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error));
        results.Should().HaveCount(1);
        results[0].Properties["maximumtrackingnumber"].Value.Should().Be(42);

        var updated = Service!.Retrieve("organization", organization.Id, new ColumnSet(true));
        updated.GetAttributeValue<int>("maximumtrackingnumber").Should().Be(42);
    }

    [Fact]
    public void SetDataverseOrganizationSettings_OrgDbOrgSettings_UpdatesAndRemovesSettings()
    {
        using var ps = CreatePowerShellWithOrganizationSettingsCmdlets();
        var mockConnection = CreateMockConnectionWithCustomMetadata(null, BuildOrganizationMetadata());

        var organization = GetExistingOrganization();
        organization["orgdborgsettings"] = "<OrgSettings><AllowSaveAsDraftAppointment>false</AllowSaveAsDraftAppointment><LegacySetting>keep</LegacySetting></OrgSettings>";
        Service!.Update(organization);

        ps.AddCommand("Set-DataverseOrganizationSettings")
          .AddParameter("Connection", mockConnection)
          .AddParameter("InputObject", new System.Collections.Hashtable
          {
              ["AllowSaveAsDraftAppointment"] = true,
              ["LegacySetting"] = null,
              ["NewSetting"] = 7,
          })
          .AddParameter("OrgDbOrgSettings", true)
          .AddParameter("Confirm", false);
        ps.Invoke();

        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error));

        var updated = Service!.Retrieve("organization", organization.Id, new ColumnSet(true));
        var xml = updated.GetAttributeValue<string>("orgdborgsettings");
        xml.Should().Contain("AllowSaveAsDraftAppointment");
        xml.Should().Contain("<NewSetting>7</NewSetting>");
        xml.Should().NotContain("LegacySetting");
    }

    private static System.Collections.Generic.List<EntityMetadata> BuildOrganizationMetadata()
    {
        return new System.Collections.Generic.List<EntityMetadata>
        {
            BuildEntityMetadata(
                logicalName: "organization",
                schemaName: "Organization",
                primaryIdAttribute: "organizationid",
                primaryNameAttribute: "name",
                objectTypeCode: 1,
                BuildGuidAttribute("organizationid", "OrganizationId"),
                BuildStringAttribute("name", "Name"),
                BuildIntegerAttribute("maximumtrackingnumber", "MaximumTrackingNumber"),
                BuildStringAttribute("orgdborgsettings", "OrgDbOrgSettings"))
        };
    }

    private Entity GetExistingOrganization()
    {
        return Service!.RetrieveMultiple(new QueryExpression("organization")
        {
            ColumnSet = new ColumnSet(true),
            TopCount = 1
        }).Entities.Single();
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
        SetMetadataProperty(attribute, nameof(AttributeMetadata.LogicalName), logicalName);
        SetMetadataProperty(attribute, nameof(AttributeMetadata.SchemaName), schemaName);
        SetMetadataProperty(attribute, nameof(AttributeMetadata.AttributeType), AttributeTypeCode.Uniqueidentifier);
        return attribute;
    }

    private static StringAttributeMetadata BuildStringAttribute(string logicalName, string schemaName)
    {
        var attribute = new StringAttributeMetadata();
        SetMetadataProperty(attribute, nameof(AttributeMetadata.LogicalName), logicalName);
        SetMetadataProperty(attribute, nameof(AttributeMetadata.SchemaName), schemaName);
        SetMetadataProperty(attribute, nameof(AttributeMetadata.AttributeType), AttributeTypeCode.String);
        attribute.MaxLength = 1073741823;
        return attribute;
    }

    private static IntegerAttributeMetadata BuildIntegerAttribute(string logicalName, string schemaName)
    {
        var attribute = new IntegerAttributeMetadata();
        SetMetadataProperty(attribute, nameof(AttributeMetadata.LogicalName), logicalName);
        SetMetadataProperty(attribute, nameof(AttributeMetadata.SchemaName), schemaName);
        SetMetadataProperty(attribute, nameof(AttributeMetadata.AttributeType), AttributeTypeCode.Integer);
        return attribute;
    }

    private static void SetMetadataProperty(object target, string propertyName, object value)
    {
        var property = target.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        property?.SetValue(target, value);
    }
}