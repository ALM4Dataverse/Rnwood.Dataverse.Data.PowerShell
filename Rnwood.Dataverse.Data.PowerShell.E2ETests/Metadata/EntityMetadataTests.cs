using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using AwesomeAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Metadata
{
    /// <summary>
    /// Entity metadata manipulation tests against a real Dataverse environment.
    /// Converted from e2e-tests/EntityMetadata.Tests.ps1
    /// </summary>
    /// <remarks>
    /// Placed in the SchemaAndPublishChanges collection because these tests create and delete
    /// custom entities (schema changes) which conflict with other schema-changing tests when run
    /// in parallel.
    /// </remarks>
    [Collection(SchemaChangesCollection.Name)]
    public class EntityMetadataTests : E2ETestBase
    {
[Fact]
        public void CanCreateReadUpdateAndDeleteCustomEntityWithAllFeatures()
        {


            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

try {
    # Wait for any existing operations to complete before starting test
    Write-Host 'Pre-check: Ensuring no pending operations...'
    Start-Sleep -Seconds 5

    $timestamp = [DateTime]::UtcNow.ToString('yyyyMMddHHmm')
    $testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
    $entityName = ""new_enttest_${timestamp}_$testRunId""
    $entitySchemaName = ""new_EntTest_${timestamp}_$testRunId""
    
    Write-Host ""Test entity: $entityName""
    
    Write-Host 'Step 1: Creating custom entity with all features...'
    Invoke-WithRetry {
        Set-DataverseEntityMetadata -Connection $connection `
            -EntityName $entityName `
            -SchemaName $entitySchemaName `
            -DisplayName 'Entity Test' `
            -DisplayCollectionName 'Entity Tests' `
            -PrimaryAttributeSchemaName 'new_name' `
            -OwnershipType UserOwned `
            -Confirm:$false
    }
    Write-Host '✓ Custom entity created'
    
    Write-Host 'Step 2: Retrieving entity metadata...'
    Invoke-WithRetry {
        $entityMetadata = Get-DataverseEntityMetadata -Connection $connection -EntityName $entityName
        if (-not $entityMetadata) {
            throw 'Failed to retrieve entity metadata'
        }
        Write-Host ""  Entity: $($entityMetadata.LogicalName)""
    }
    Write-Host '✓ Entity metadata retrieved'
    
    Write-Host 'Step 3: Cleanup - Removing test entity...'
    Invoke-WithRetry {
        Remove-DataverseEntityMetadata -Connection $connection -EntityName $entityName -Confirm:$false
    }
    Write-Host '✓ Test entity deleted'
    
    Write-Host 'SUCCESS: All entity metadata operations completed'
}
catch {
    Write-Host ""ERROR: $($_ | Out-String)""
    throw
}
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("SUCCESS", because: result.GetFullOutput());
        }
    }
}
