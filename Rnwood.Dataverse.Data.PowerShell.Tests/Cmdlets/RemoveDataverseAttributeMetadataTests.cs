using FluentAssertions;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Tests for Remove-DataverseAttributeMetadata cmdlet
    /// Migrated from tests/Set-Remove-DataverseMetadata.Tests.ps1 (Remove-DataverseAttributeMetadata tests)
    /// </summary>
    public class RemoveDataverseAttributeMetadataTests : TestBase
    {
        [Fact]
        public void RemoveDataverseAttributeMetadata_WhatIf_DoesNotThrow()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Remove-DataverseAttributeMetadata", typeof(Commands.RemoveDataverseAttributeMetadataCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            var mockConnection = CreateMockConnection();

            // Act - WhatIf should not throw even with mock connection
            ps.AddCommand("Remove-DataverseAttributeMetadata")
              .AddParameter("Connection", mockConnection)
              .AddParameter("EntityName", "contact")
              .AddParameter("AttributeName", "new_testfield")
              .AddParameter("WhatIf", true);

            // Assert
            var action = () => ps.Invoke();
            action.Should().NotThrow();
        }
    }
}
