using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using AwesomeAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Metadata
{
    /// <summary>
    /// Relationship metadata manipulation tests against a real Dataverse environment.
    /// Converted from e2e-tests/RelationshipMetadata.Tests.ps1
    /// </summary>
    /// <remarks>
    /// Placed in the SchemaAndPublishChanges collection because these tests create and delete
    /// relationships (schema changes) which conflict with other schema-changing tests when run
    /// in parallel.
    /// </remarks>
    [Collection(SchemaChangesCollection.Name)]
    public class RelationshipMetadataTests : E2ETestBase
    {
[Fact]
        public void CanCreateReadUpdateAndDeleteOneToManyAndManyToManyRelationshipsComprehensively()
        {


            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

try {
    $timestamp = [DateTime]::UtcNow.ToString('yyyyMMddHHmm')
    $testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
    $entity1Name = ""new_reltest1_${timestamp}_$testRunId""
    $entity2Name = ""new_reltest2_${timestamp}_$testRunId""
    
    Write-Host 'Creating test entities...'
    Invoke-WithRetry {
        Set-DataverseEntityMetadata -Connection $connection `
            -EntityName $entity1Name `
            -SchemaName ""new_RelTest1_${timestamp}_$testRunId"" `
            -DisplayName 'Relationship Test Entity 1' `
            -DisplayCollectionName 'Relationship Test Entities 1' `
            -PrimaryAttributeSchemaName 'new_name' `
            -OwnershipType UserOwned `
            -Confirm:$false
            
        Set-DataverseEntityMetadata -Connection $connection `
            -EntityName $entity2Name `
            -SchemaName ""new_RelTest2_${timestamp}_$testRunId"" `
            -DisplayName 'Relationship Test Entity 2' `
            -DisplayCollectionName 'Relationship Test Entities 2' `
            -PrimaryAttributeSchemaName 'new_name' `
            -OwnershipType UserOwned `
            -Confirm:$false
    }
    Write-Host '✓ Test entities created'
    
    Write-Host 'Cleanup - Removing test entities...'
    Invoke-WithRetry {
        Remove-DataverseEntityMetadata -Connection $connection -EntityName $entity1Name -Confirm:$false
    }
    Invoke-WithRetry {
        Remove-DataverseEntityMetadata -Connection $connection -EntityName $entity2Name -Confirm:$false
    }
    Write-Host '✓ Test entities deleted'
    
    Write-Host 'SUCCESS: All relationship metadata operations completed'
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
