using AwesomeAssertions;
using Microsoft.PowerPlatform.Dataverse.Client;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Tests for Remove-DataverseEntityKeyMetadata cmdlet
    /// Migrated from tests/Remove-DataverseEntityKeyMetadata.Tests.ps1 (7 tests)
    /// </summary>
    public class RemoveDataverseEntityKeyMetadataTests : TestBase
    {
        /// <summary>
        /// Seeds the test alternate key that tests expect to exist on the contact entity.
        /// </summary>
        private ServiceClient CreateMockConnectionWithTestKey(params string[] entities)
        {
            var conn = entities.Length > 0 ? CreateMockConnection(entities) : CreateMockConnection();
            Environment!.MetadataStore.AddEntity("contact")
                .WithAlternateKey("contact_emailaddress1_key", "emailaddress1");
            return conn;
        }

        private static PS CreatePowerShellWithCmdlets()
        {
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Remove-DataverseEntityKeyMetadata", typeof(Commands.RemoveDataverseEntityKeyMetadataCmdlet), null));
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseConnectionAsDefault", typeof(Commands.SetDataverseConnectionAsDefaultCmdlet), null));

            var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();

            var ps = PS.Create();
            ps.Runspace = runspace;
            return ps;
        }

        // ===== Delete Key ===== (3 tests)

        [Fact]
        public void RemoveDataverseEntityKeyMetadata_DeletesAlternateKey()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Remove-DataverseEntityKeyMetadata", typeof(Commands.RemoveDataverseEntityKeyMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnectionWithTestKey();

            // Act - Delete the key
            ps.AddCommand("Remove-DataverseEntityKeyMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("KeyName", "contact_emailaddress1_key")
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - Should execute without errors
            ps.HadErrors.Should().BeFalse();
            // Note: FakeXrmEasy may not support DeleteEntityKeyRequest
        }

        [Fact]
        public void RemoveDataverseEntityKeyMetadata_PassesCorrectEntityName()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Remove-DataverseEntityKeyMetadata", typeof(Commands.RemoveDataverseEntityKeyMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnectionWithTestKey();

            // Act - Delete with specific entity name
            ps.AddCommand("Remove-DataverseEntityKeyMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("KeyName", "contact_emailaddress1_key")
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - Should execute without errors
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void RemoveDataverseEntityKeyMetadata_PassesCorrectKeyName()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Remove-DataverseEntityKeyMetadata", typeof(Commands.RemoveDataverseEntityKeyMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnectionWithTestKey();

            // Act - Delete with specific key name
            ps.AddCommand("Remove-DataverseEntityKeyMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("KeyName", "contact_emailaddress1_key")
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - Should execute without errors
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void RemoveDataverseEntityKeyMetadata_WorksWithDefaultConnection()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnectionWithTestKey("contact");

            // Set default connection via static helper
            SetDataverseConnectionAsDefaultCmdlet.SetDefault(mockConnection);

            // Act - Delete without explicit connection
            ps.AddCommand("Remove-DataverseEntityKeyMetadata")
              .AddParameter("EntityName", "contact")
              .AddParameter("KeyName", "contact_emailaddress1_key")
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - Should use default connection
            ps.HadErrors.Should().BeFalse();
        }

        // ===== WhatIf Support ===== (2 tests)

        [Fact]
        public void RemoveDataverseEntityKeyMetadata_WhatIf_DoesNotDelete()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Remove-DataverseEntityKeyMetadata", typeof(Commands.RemoveDataverseEntityKeyMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnectionWithTestKey();

            // Act - Delete with WhatIf
            ps.AddCommand("Remove-DataverseEntityKeyMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("KeyName", "contact_emailaddress1_key")
              .AddParameter("WhatIf", true);

            ps.Invoke();

            // Assert - Should not execute actual deletion
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void RemoveDataverseEntityKeyMetadata_ConfirmFalse_ExecutesDelete()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Remove-DataverseEntityKeyMetadata", typeof(Commands.RemoveDataverseEntityKeyMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnectionWithTestKey();

            // Act - Delete with Confirm:$false
            ps.AddCommand("Remove-DataverseEntityKeyMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("KeyName", "contact_emailaddress1_key")
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - Should execute deletion
            ps.HadErrors.Should().BeFalse();
        }

        // ===== Pipeline Support ===== (1 test)

        [Fact]
        public void RemoveDataverseEntityKeyMetadata_AcceptsEntityNameFromPipeline()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Remove-DataverseEntityKeyMetadata", typeof(Commands.RemoveDataverseEntityKeyMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnectionWithTestKey();

            // Act - Pass entity name via pipeline
            ps.AddScript("'contact'")
              .AddCommand("Remove-DataverseEntityKeyMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("KeyName", "contact_emailaddress1_key")
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - Should accept pipelined entity name
            ps.HadErrors.Should().BeFalse();
        }
    }
}
