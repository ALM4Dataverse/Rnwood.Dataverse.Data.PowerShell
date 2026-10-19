using System;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using AwesomeAssertions;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets;

public class WebResourcesTests : TestBase
{
    [Fact]
    public void WebResourceCmdlets_CanCreateRetrieveUpdateAndDeleteWebResource()
    {
        var webResourceName = $"test_{Guid.NewGuid():N}";
        var tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}.js");

        try
        {
            File.WriteAllText(tempFile, "console.log('standard test');", Encoding.UTF8);

            using var createPs = CreatePowerShellWithCmdlets();
            var mockConnection = CreateWebResourceConnection();

            createPs.AddCommand("Set-DataverseWebResource")
                .AddParameter("Connection", mockConnection)
                .AddParameter("Name", webResourceName)
                .AddParameter("Path", tempFile)
                .AddParameter("DisplayName", "Standard Test Resource")
                .AddParameter("PublisherPrefix", "test");
            createPs.Invoke();

            createPs.HadErrors.Should().BeFalse(string.Join(System.Environment.NewLine, createPs.Streams.Error.Select(e => e.ToString())));

            File.WriteAllText(tempFile, "console.log('standard test updated');", Encoding.UTF8);

            using var updatePs = CreatePowerShellWithCmdlets();
            updatePs.AddCommand("Set-DataverseWebResource")
                .AddParameter("Connection", mockConnection)
                .AddParameter("Name", webResourceName)
                .AddParameter("Path", tempFile);
            updatePs.Invoke();

            updatePs.HadErrors.Should().BeFalse(string.Join(System.Environment.NewLine, updatePs.Streams.Error.Select(e => e.ToString())));

            using var getPs = CreatePowerShellWithCmdlets();
            getPs.AddCommand("Get-DataverseWebResource")
                .AddParameter("Connection", mockConnection)
                .AddParameter("Name", webResourceName)
                .AddParameter("DecodeContent", true);
            var getResults = getPs.Invoke();

            getPs.HadErrors.Should().BeFalse(string.Join(System.Environment.NewLine, getPs.Streams.Error.Select(e => e.ToString())));
            getResults.Should().HaveCount(1);
            getResults[0].Properties["name"]?.Value.Should().Be(webResourceName);
            var contentBytes = getResults[0].Properties["content"]?.Value.Should().BeOfType<byte[]>().Subject;
            Encoding.UTF8.GetString(contentBytes!).Should().Contain("standard test updated");

            using var removePs = CreatePowerShellWithCmdlets();
            removePs.AddCommand("Remove-DataverseWebResource")
                .AddParameter("Connection", mockConnection)
                .AddParameter("Name", webResourceName)
                .AddParameter("Confirm", false);
            removePs.Invoke();

            removePs.HadErrors.Should().BeFalse(string.Join(System.Environment.NewLine, removePs.Streams.Error.Select(e => e.ToString())));
            FindStoredWebResources(webResourceName).Should().BeEmpty();
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public void WebResourceCmdlets_CanBatchUploadAndRemoveWebResourcesFromFolder()
    {
        var prefix = $"testbatch_{Guid.NewGuid():N}";
        var tempFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(tempFolder);

        try
        {
            File.WriteAllText(System.IO.Path.Combine(tempFolder, "test1.js"), "console.log('test1');", Encoding.UTF8);
            File.WriteAllText(System.IO.Path.Combine(tempFolder, "test2.js"), "console.log('test2');", Encoding.UTF8);
            File.WriteAllText(System.IO.Path.Combine(tempFolder, "test.css"), "body { color: red; }", Encoding.UTF8);

            var mockConnection = CreateWebResourceConnection();

            using var setPs = CreatePowerShellWithCmdlets();
            setPs.AddCommand("Set-DataverseWebResource")
                .AddParameter("Connection", mockConnection)
                .AddParameter("Folder", tempFolder)
                .AddParameter("PublisherPrefix", prefix)
                .AddParameter("FileFilter", "*.*");
            setPs.Invoke();

            setPs.HadErrors.Should().BeFalse(string.Join(System.Environment.NewLine, setPs.Streams.Error.Select(e => e.ToString())));

            using var getPs = CreatePowerShellWithCmdlets();
            getPs.AddCommand("Get-DataverseWebResource")
                .AddParameter("Connection", mockConnection)
                .AddParameter("Name", $"{prefix}_*");
            var getResults = getPs.Invoke();

            getPs.HadErrors.Should().BeFalse(string.Join(System.Environment.NewLine, getPs.Streams.Error.Select(e => e.ToString())));
            getResults.Should().HaveCount(3);

            using var removePs = CreatePowerShellWithCmdlets();
            removePs.AddCommand("Remove-DataverseWebResource")
                .AddParameter("Connection", mockConnection)
                .AddParameter("Confirm", false);
            removePs.Invoke(getResults);

            removePs.HadErrors.Should().BeFalse(string.Join(System.Environment.NewLine, removePs.Streams.Error.Select(e => e.ToString())));
            FindStoredWebResources($"{prefix}_*").Should().BeEmpty();
        }
        finally
        {
            if (Directory.Exists(tempFolder))
            {
                Directory.Delete(tempFolder, true);
            }
        }
    }

    private ServiceClient CreateWebResourceConnection()
    {
        return CreateMockConnectionWithCustomMetadata(null, BuildWebResourceMetadata());
    }

    private Entity[] FindStoredWebResources(string namePattern)
    {
        var query = new QueryExpression("webresource")
        {
            ColumnSet = new ColumnSet("webresourceid", "name", "content"),
            Criteria = new FilterExpression
            {
                Conditions =
                {
                    new ConditionExpression(
                        "name",
                        namePattern.Contains('*') || namePattern.Contains('?') ? ConditionOperator.Like : ConditionOperator.Equal,
                        namePattern.Replace("*", "%").Replace("?", "_"))
                }
            }
        };

        return Service!.RetrieveMultiple(query).Entities.ToArray();
    }

    private static System.Collections.Generic.List<EntityMetadata> BuildWebResourceMetadata()
    {
        return new System.Collections.Generic.List<EntityMetadata>
        {
            BuildEntityMetadata(
                logicalName: "solution",
                schemaName: "Solution",
                primaryIdAttribute: "solutionid",
                primaryNameAttribute: "friendlyname",
                objectTypeCode: 7101,
                BuildGuidAttribute("solutionid", "SolutionId"),
                BuildStringAttribute("friendlyname", "FriendlyName")),
            BuildEntityMetadata(
                logicalName: "webresource",
                schemaName: "WebResource",
                primaryIdAttribute: "webresourceid",
                primaryNameAttribute: "name",
                objectTypeCode: 61,
                BuildGuidAttribute("webresourceid", "WebResourceId"),
                BuildStringAttribute("name", "Name"),
                BuildStringAttribute("displayname", "DisplayName"),
                BuildStringAttribute("description", "Description"),
                BuildStringAttribute("content", "Content"),
                BuildWebResourceTypeAttribute("webresourcetype", "WebResourceType"),
                BuildBooleanAttribute("ismanaged", "IsManaged"),
                BuildBooleanAttribute("iscustomizable", "IsCustomizable"),
                BuildBooleanAttribute("canbedeleted", "CanBeDeleted"),
                BuildIntegerAttribute("languagecode", "LanguageCode"),
                BuildStringAttribute("dependencyxml", "DependencyXml"),
                BuildStringAttribute("introducedversion", "IntroducedVersion"),
                BuildBooleanAttribute("isenabledformobileclient", "IsEnabledForMobileClient"),
                BuildBooleanAttribute("ishidden", "IsHidden"),
                BuildDateTimeAttribute("createdon", "CreatedOn"),
                BuildDateTimeAttribute("modifiedon", "ModifiedOn"),
                BuildLookupAttribute("createdby", "CreatedBy", "systemuser"),
                BuildLookupAttribute("modifiedby", "ModifiedBy", "systemuser"),
                BuildLookupAttribute("organizationid", "OrganizationId", "organization"),
                BuildBigIntAttribute("versionnumber", "VersionNumber"),
                BuildLookupAttribute("solutionid", "SolutionId", "solution"),
                BuildComponentStateAttribute("componentstate", "ComponentState"),
                BuildDateTimeAttribute("overwritetime", "OverwriteTime"))
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
        var attribute = new StringAttributeMetadata { MaxLength = 4000 };
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

    private static BigIntAttributeMetadata BuildBigIntAttribute(string logicalName, string schemaName)
    {
        var attribute = new BigIntAttributeMetadata();
        SetAttributeDefaults(attribute, logicalName, schemaName, AttributeTypeCode.BigInt);
        return attribute;
    }

    private static PicklistAttributeMetadata BuildPicklistAttribute(string logicalName, string schemaName, params OptionMetadata[] options)
    {
        var attribute = new PicklistAttributeMetadata();
        SetAttributeDefaults(attribute, logicalName, schemaName, AttributeTypeCode.Picklist);
        if (options.Length > 0)
        {
            var optionSet = new OptionSetMetadata();
            foreach (var option in options)
            {
                optionSet.Options.Add(option);
            }

            SetMetadataProperty(attribute, nameof(PicklistAttributeMetadata.OptionSet), optionSet);
        }
        return attribute;
    }

    private static PicklistAttributeMetadata BuildWebResourceTypeAttribute(string logicalName, string schemaName)
    {
        return BuildPicklistAttribute(
            logicalName,
            schemaName,
            BuildOption(1, "HTML"),
            BuildOption(2, "CSS"),
            BuildOption(3, "Script"),
            BuildOption(4, "Data"),
            BuildOption(5, "PNG"),
            BuildOption(6, "JPG"),
            BuildOption(7, "GIF"),
            BuildOption(8, "Silverlight"),
            BuildOption(9, "XSL"),
            BuildOption(10, "ICO"),
            BuildOption(11, "SVG"),
            BuildOption(12, "RESX"));
    }

    private static PicklistAttributeMetadata BuildComponentStateAttribute(string logicalName, string schemaName)
    {
        return BuildPicklistAttribute(
            logicalName,
            schemaName,
            BuildOption(0, "Published"),
            BuildOption(1, "Unpublished"),
            BuildOption(2, "Deleted"),
            BuildOption(3, "Deleted Unpublished"));
    }

    private static OptionMetadata BuildOption(int value, string label)
    {
        var localizedLabel = new LocalizedLabel(label, 1033);
        var metadataLabel = new Label();
        metadataLabel.LocalizedLabels.Add(localizedLabel);
        metadataLabel.UserLocalizedLabel = localizedLabel;
        return new OptionMetadata(metadataLabel, value);
    }

    private static DateTimeAttributeMetadata BuildDateTimeAttribute(string logicalName, string schemaName)
    {
        var attribute = new DateTimeAttributeMetadata();
        SetAttributeDefaults(attribute, logicalName, schemaName, AttributeTypeCode.DateTime);
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