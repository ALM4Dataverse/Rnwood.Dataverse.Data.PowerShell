using AwesomeAssertions;
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
    /// Tests for Set-DataverseEntityMetadata cmdlet - HasActivities and HasNotes features
    /// Migrated from tests/Set-DataverseEntityMetadata-HasActivitiesHasNotes.Tests.ps1 (5 tests)
    /// </summary>
    public class SetDataverseEntityMetadataFeaturesTests : TestBase
    {
        // ===== HasActivities Parameter ===== (2 tests)

        [Fact]
        public void SetDataverseEntityMetadata_HasActivitiesTrue_DetectsChanges()
        {
            // Arrange - TestBase has UpdateEntityRequest interceptor
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseEntityMetadata", typeof(Commands.SetDataverseEntityMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Act - Set HasActivities to true
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("HasActivities", true)
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - Should not have "No changes specified" warning
            ps.HadErrors.Should().BeFalse();
            ps.Streams.Warning.Should().NotContain(w => w.Message.Contains("No changes specified"));
        }

        [Fact]
        public void SetDataverseEntityMetadata_HasActivitiesFalse_DetectsChanges()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");

            // Act - Set HasActivities to false using PowerShell script syntax
            // Note: -HasActivities:$false sets SwitchParameter with value=false
            // This cannot be done via .AddParameter() API, requires PowerShell script execution
            ps.AddScript("Set-DataverseEntityMetadata -Connection $args[0] -EntityName contact -HasActivities:$false -Confirm:$false");
            ps.AddArgument(mockConnection);
            ps.Invoke();

            // Assert - Should not have "No changes specified" warning
            ps.HadErrors.Should().BeFalse();
            ps.Streams. Warning.Should().NotContain(w => w.Message.Contains("No changes specified"));
        }

        // ===== HasNotes Parameter ===== (2 tests)

        [Fact]
        public void SetDataverseEntityMetadata_HasNotesTrue_DetectsChanges()
        {
            // Arrange - TestBase has UpdateEntityRequest interceptor
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseEntityMetadata", typeof(Commands.SetDataverseEntityMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Act - Set HasNotes to true
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("HasNotes", true)
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - Should not have "No changes specified" warning
            ps.HadErrors.Should().BeFalse();
            ps.Streams.Warning.Should().NotContain(w => w.Message.Contains("No changes specified"));
        }

        [ Fact]
        public void SetDataverseEntityMetadata_HasNotesFalse_DetectsChanges()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");

            // Act - Set HasNotes to false using PowerShell script syntax
            // Note: -HasNotes:$false sets SwitchParameter with value=false
            // This cannot be done via .AddParameter() API, requires PowerShell script execution
            ps.AddScript("Set-DataverseEntityMetadata -Connection $args[0] -EntityName contact -HasNotes:$false -Confirm:$false");
            ps.AddArgument(mockConnection);
            ps.Invoke();

            // Assert - Should not have "No changes specified" warning
            ps.HadErrors.Should().BeFalse();
            ps.Streams.Warning.Should().NotContain(w => w.Message.Contains("No changes specified"));
        }

        // ===== Combined HasActivities and HasNotes ===== (1 test - skipped due to FakeXrmEasy limitations)

        [Fact]
        public void SetDataverseEntityMetadata_BothHasActivitiesAndHasNotes_DetectsChanges()
        {
            // Arrange - TestBase has UpdateEntityRequest interceptor
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseEntityMetadata", typeof(Commands.SetDataverseEntityMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Act - Set both HasActivities and HasNotes
            ps.AddCommand("Set-DataverseEntityMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("HasActivities", true)
              .AddParameter("HasNotes", true)
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - Should not have "No changes specified" warning
            ps.HadErrors.Should().BeFalse();
            ps.Streams.Warning.Should().NotContain(w => w.Message.Contains("No changes specified"));
        }
    }
}
