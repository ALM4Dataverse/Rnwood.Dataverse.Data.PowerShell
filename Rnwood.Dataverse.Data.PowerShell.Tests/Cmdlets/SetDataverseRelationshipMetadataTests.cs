using AwesomeAssertions;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Tests for Set-DataverseRelationshipMetadata cmdlet
    /// Migrated from tests/Set-DataverseRelationshipMetadata.Tests.ps1 (1 test)
    /// </summary>
    public class SetDataverseRelationshipMetadataTests : TestBase
    {
        // ===== Create ManyToMany Relationship ===== (1 test)

        [Fact]
        public void SetDataverseRelationshipMetadata_CreateManyToMany_UsesIntersectEntitySchemaName()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseRelationshipMetadata", typeof(Commands.SetDataverseRelationshipMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection(new[] { "contact", "account" });

            // Act - Create ManyToMany relationship using IntersectEntitySchemaName parameter
            ps.AddCommand("Set-DataverseRelationshipMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("SchemaName", "new_account_contact")
              .AddParameter("RelationshipType", "ManyToMany")
              .AddParameter("ReferencedEntity", "account")
              .AddParameter("ReferencingEntity", "contact")
              .AddParameter("IntersectEntitySchemaName", "new_accountcontact")
              .AddParameter("Confirm", false);

            ps.Invoke();

            // Assert - Should execute without errors
            // Note: FakeXrmEasy doesn't support CreateManyToManyRequest
            // This test validates compilation and parameter acceptance only
            // Full validation requires E2E test with real Dataverse
            ps.HadErrors.Should().BeFalse();
        }
    }
}
