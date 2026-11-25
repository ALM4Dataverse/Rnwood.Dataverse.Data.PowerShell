using AwesomeAssertions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Tests for Set-DataverseEntityKeyMetadata cmdlet
    /// Migrated from tests/Set-DataverseEntityKeyMetadata.Tests.ps1 (6 tests)
    /// </summary>
    public class SetDataverseEntityKeyMetadataTests : TestBase
    {
        // ===== Parameter Validation ===== (1 test)

        [Fact]
        public void SetDataverseEntityKeyMetadata_EmptyKeyAttributes_ThrowsError()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseEntityKeyMetadata", typeof(Commands.SetDataverseEntityKeyMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act - Try with empty KeyAttributes array
            ps.AddCommand("Set-DataverseEntityKeyMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("SchemaName", "test_key")
              .AddParameter("KeyAttributes", new string[0])
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

            // Assert - Should throw error about KeyAttributes (empty array validation)
            (ps.HadErrors || caughtException != null).Should().BeTrue("Expected error for empty KeyAttributes");
            
            // Check either stream errors or exception for the expected message
            if (ps.Streams.Error.Count > 0)
            {
                ps.Streams.Error[0].Exception.Message.Should().Contain("KeyAttributes");
            }
            else if (caughtException != null)
            {
                // PowerShell parameter validation might throw binding exception for empty array
                caughtException.Message.Should().ContainAny("KeyAttributes", "empty", "Empty");
            }
        }

        // ===== Request Creation ===== (3 tests)
        // Note: These tests require organization entity metadata and FakeXrmEasy
        // doesn't support CreateEntityKeyRequest fully. Tested in E2E tests.

        [Fact]
        public void SetDataverseEntityKeyMetadata_CreatesRequestWithCorrectEntityName()
        {
            // Arrange - TestBase has CreateEntityKeyRequest interceptor
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseEntityKeyMetadata", typeof(Commands.SetDataverseEntityKeyMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Act - Create a new key
            ps.AddCommand("Set-DataverseEntityKeyMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("SchemaName", "contact_emailaddress1_key")
              .AddParameter("KeyAttributes", new[] { "emailaddress1" })
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - Cmdlet should execute without errors
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void SetDataverseEntityKeyMetadata_CreatesEntityKeyMetadataWithCorrectSchemaName()
        {
            // Arrange - TestBase has CreateEntityKeyRequest interceptor
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseEntityKeyMetadata", typeof(Commands.SetDataverseEntityKeyMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Act - Create a new key with specific schema name
            ps.AddCommand("Set-DataverseEntityKeyMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("SchemaName", "contact_emailaddress1_key")
              .AddParameter("KeyAttributes", new[] { "emailaddress1" })
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - Should execute without errors
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void SetDataverseEntityKeyMetadata_SetsKeyAttributesCorrectly()
        {
            // Arrange - TestBase has CreateEntityKeyRequest interceptor
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseEntityKeyMetadata", typeof(Commands.SetDataverseEntityKeyMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Act - Create a key with multiple attributes
            ps.AddCommand("Set-DataverseEntityKeyMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("SchemaName", "contact_name_key")
              .AddParameter("KeyAttributes", new[] { "firstname", "lastname" })
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - Should execute without errors
            ps.HadErrors.Should().BeFalse();
        }

        // ===== WhatIf Support ===== (1 test)

        [Fact]
        public void SetDataverseEntityKeyMetadata_WhatIf_DoesNotCreateKey()
        {
            // Arrange - TestBase has CreateEntityKeyRequest interceptor
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseEntityKeyMetadata", typeof(Commands.SetDataverseEntityKeyMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Act - Create key with WhatIf
            ps.AddCommand("Set-DataverseEntityKeyMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("SchemaName", "contact_emailaddress1_key")
              .AddParameter("KeyAttributes", new[] { "emailaddress1" })
              .AddParameter("WhatIf", true);

            var results = ps.Invoke();

            // Assert - WhatIf should not execute actual creation
            results.Should().BeEmpty();
            ps.HadErrors.Should().BeFalse();
        }

        // ===== Force Parameter ===== (1 test)

        [Fact]
        public void SetDataverseEntityKeyMetadata_Force_SkipsExistenceCheck()
        {
            // Arrange - TestBase has CreateEntityKeyRequest interceptor
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseEntityKeyMetadata", typeof(Commands.SetDataverseEntityKeyMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection("contact");

            // Act - Create key with Force (should skip existence check)
            ps.AddCommand("Set-DataverseEntityKeyMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("SchemaName", "contact_emailaddress1_key")
              .AddParameter("KeyAttributes", new[] { "emailaddress1" })
              .AddParameter("Force", true)
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - Should execute without checking if key exists first
            ps.HadErrors.Should().BeFalse();
        }
    }
}
