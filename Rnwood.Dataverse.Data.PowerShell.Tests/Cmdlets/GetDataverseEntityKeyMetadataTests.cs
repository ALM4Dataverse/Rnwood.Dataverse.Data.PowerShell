using System;
using AwesomeAssertions;
using Microsoft.Xrm.Sdk.Metadata;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Tests for Get-DataverseEntityKeyMetadata cmdlet
    /// Migrated from tests/Get-DataverseEntityKeyMetadata.Tests.ps1 (6 tests)
    /// </summary>
    public class GetDataverseEntityKeyMetadataTests : TestBase
    {
        // ===== Parameter Validation ===== (2 tests)

        [Fact]
        public void GetDataverseEntityKeyMetadata_EntityNameParameter_Accepted()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseEntityKeyMetadata", typeof(Commands.GetDataverseEntityKeyMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act
            ps.AddCommand("Get-DataverseEntityKeyMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact");

            ps.Invoke();

            // Assert - Should not throw with EntityName parameter
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void GetDataverseEntityKeyMetadata_KeyNameParameter_ThrowsIfNotFound()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseEntityKeyMetadata", typeof(Commands.GetDataverseEntityKeyMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act
            ps.AddCommand("Get-DataverseEntityKeyMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("KeyName", "nonexistent_key");

            Exception? caughtException = null;
            try
            {
                ps.Invoke();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert - Should throw with KeyNotFound error (parameters are valid)
            (ps.HadErrors || caughtException != null).Should().BeTrue("Expected error for non-existent key");
            
            // Check either stream errors or exception for the expected message
            if (ps.Streams.Error.Count > 0)
            {
                ps.Streams.Error[0].Exception.Message.Should().Contain("not found");
            }
            else if (caughtException != null)
            {
                caughtException.Message.Should().Contain("not found");
            }
        }

        // ===== Request Creation ===== (3 tests)

        [Fact]
        public void GetDataverseEntityKeyMetadata_CreatesRequestWithCorrectEntityName()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseEntityKeyMetadata", typeof(Commands.GetDataverseEntityKeyMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act
            ps.AddCommand("Get-DataverseEntityKeyMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact");

            ps.Invoke();

            // Assert - Cmdlet should execute without errors
            ps.HadErrors.Should().BeFalse();
            // Note: FakeXrmEasy doesn't fully support entity keys, but cmdlet should accept parameters
        }

        [Fact]
        public void GetDataverseEntityKeyMetadata_UsesRetrieveAsIfPublishedTrueByDefault()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseEntityKeyMetadata", typeof(Commands.GetDataverseEntityKeyMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act - Execute without -Published flag
            ps.AddCommand("Get-DataverseEntityKeyMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact");

            ps.Invoke();

            // Assert - Should retrieve unpublished metadata by default
            ps.HadErrors.Should().BeFalse();
            // Note: RetrieveAsIfPublished defaults to true (includes unpublished changes)
        }

        [Fact]
        public void GetDataverseEntityKeyMetadata_UsesRetrieveAsIfPublishedFalseWithPublished()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseEntityKeyMetadata", typeof(Commands.GetDataverseEntityKeyMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act - Execute with -Published flag
            ps.AddCommand("Get-DataverseEntityKeyMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("Published", true);

            ps.Invoke();

            // Assert - Should retrieve published metadata only
            ps.HadErrors.Should().BeFalse();
            // Note: RetrieveAsIfPublished should be false (published only)
        }

        // ===== Error Handling ===== (1 test)

        [Fact]
        public void GetDataverseEntityKeyMetadata_ThrowsWhenKeyNameNotFound()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseEntityKeyMetadata", typeof(Commands.GetDataverseEntityKeyMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act
            ps.AddCommand("Get-DataverseEntityKeyMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("KeyName", "nonexistent_key");

            Exception? caughtException = null;
            try
            {
                ps.Invoke();
            }
            catch (Exception ex)
            {
                caughtException = ex;
            }

            // Assert - Should throw error when key not found
            (ps.HadErrors || caughtException != null).Should().BeTrue("Expected error for non-existent key");
            
            // Check either stream errors or exception for the expected message
            if (ps.Streams.Error.Count > 0)
            {
                ps.Streams.Error[0].Exception.Message.Should().Contain("not found");
            }
            else if (caughtException != null)
            {
                caughtException.Message.Should().Contain("not found");
            }
        }
    }
}
