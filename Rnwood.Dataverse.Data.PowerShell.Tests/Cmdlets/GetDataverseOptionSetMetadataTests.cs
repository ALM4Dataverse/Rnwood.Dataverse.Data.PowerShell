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
    /// Tests for Get-DataverseOptionSetMetadata cmdlet
    /// Migrated from tests/Get-DataverseMetadata.Tests.ps1 (option set-related tests)
    /// </summary>
    public class GetDataverseOptionSetMetadataTests : TestBase
    {
        // ===== Entity Attribute Option Set ===== (3 tests)

        [Fact]
        public void GetDataverseOptionSetMetadata_ChoiceAttribute_ReturnsOptionSetWithOptions()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseOptionSetMetadata", typeof(Commands.GetDataverseOptionSetMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act - get option set for a choice field (gendercode is a standard choice field on contact)
            ps.AddCommand("Get-DataverseOptionSetMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("AttributeName", "gendercode");

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);

            var optionSetMetadata = results[0].BaseObject as OptionSetMetadata;
            optionSetMetadata.Should().NotBeNull();
            optionSetMetadata!.Options.Should().NotBeNull();
            optionSetMetadata.Options.Should().NotBeEmpty();

            // Verify option structure
            var firstOption = optionSetMetadata.Options.First();
            firstOption.Value.Should().NotBeNull();
            firstOption.Label.Should().NotBeNull();
            firstOption.Label.UserLocalizedLabel.Should().NotBeNull();
            firstOption.Label.UserLocalizedLabel.Label.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void GetDataverseOptionSetMetadata_DefaultConnection_UsesDefaultConnection()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseOptionSetMetadata", typeof(Commands.GetDataverseOptionSetMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

                        try
                        {
                                SetDataverseConnectionAsDefaultCmdlet.SetDefault(mockConnection);

                                ps.AddCommand("Get-DataverseOptionSetMetadata")
                                    .AddParameter("EntityName", "contact")
                                    .AddParameter("AttributeName", "gendercode");

                                var results = ps.Invoke();

                                ps.HadErrors.Should().BeFalse();
                                results.Should().HaveCount(1);

                                var optionSetMetadata = results[0].BaseObject as OptionSetMetadata;
                                optionSetMetadata.Should().NotBeNull();
                                optionSetMetadata!.Options.Should().NotBeEmpty();
                        }
                        finally
                        {
                                SetDataverseConnectionAsDefaultCmdlet.ClearDefault();
                        }
        }



        [Fact]
        public void GetDataverseOptionSetMetadata_NonChoiceAttribute_ThrowsError()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseOptionSetMetadata", typeof(Commands.GetDataverseOptionSetMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act - try to get option set for a string field
            ps.AddCommand("Get-DataverseOptionSetMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("AttributeName", "firstname");

            // Assert - expecting a terminating error
            var action = () => ps.Invoke();
            action.Should().Throw<System.Management.Automation.CmdletInvocationException>()
                .WithMessage("*firstname*")
                .WithMessage("*not a choice field*");
        }

        // ===== Published Metadata Retrieval ===== (2 tests)

        [Fact]
        public void GetDataverseOptionSetMetadata_DefaultBehavior_RetrievesUnpublishedMetadata()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseOptionSetMetadata", typeof(Commands.GetDataverseOptionSetMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act - get option set without -Published flag
            ps.AddCommand("Get-DataverseOptionSetMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("AttributeName", "gendercode");

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);

            var optionSetMetadata = results[0].BaseObject as OptionSetMetadata;
            optionSetMetadata.Should().NotBeNull();
            optionSetMetadata!.Options.Should().NotBeEmpty();
        }

        [Fact]
        public void GetDataverseOptionSetMetadata_PublishedFlag_RetrievesPublishedMetadata()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseOptionSetMetadata", typeof(Commands.GetDataverseOptionSetMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act - get only published option set
            ps.AddCommand("Get-DataverseOptionSetMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("AttributeName", "gendercode")
              .AddParameter("Published", true);

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);

            var optionSetMetadata = results[0].BaseObject as OptionSetMetadata;
            optionSetMetadata.Should().NotBeNull();
            optionSetMetadata!.Options.Should().NotBeEmpty();
        }
    }
}
