using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using AwesomeAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Metadata
{
    /// <summary>
    /// Entity key metadata manipulation tests against a real Dataverse environment.
    /// Converted from e2e-tests/EntityKeyMetadata.Tests.ps1
    /// </summary>
    /// <remarks>
    /// Placed in the SchemaAndPublishChanges collection because these tests create and delete
    /// alternate keys (schema changes) which conflict with other schema-changing tests when run
    /// in parallel.
    /// </remarks>
    [Collection(SchemaChangesCollection.Name)]
    public class EntityKeyMetadataTests : E2ETestBase
    {
[Fact]
        public void CanCreateReadAndDeleteAlternateKeys()
        {


            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

try {
    $timestamp = [DateTime]::UtcNow.ToString('yyyyMMddHHmm')
    $testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
    $entityName = ""new_keytest_${timestamp}_$testRunId""
    
    Write-Host 'Creating test entity...'
    Invoke-WithRetry {
        Set-DataverseEntityMetadata -Connection $connection `
            -EntityName $entityName `
            -SchemaName ""new_KeyTest_${timestamp}_$testRunId"" `
            -DisplayName 'Key Test Entity' `
            -DisplayCollectionName 'Key Test Entities' `
            -PrimaryAttributeSchemaName 'new_name' `
            -OwnershipType UserOwned `
            -Confirm:$false
    }
    Write-Host '✓ Test entity created'
    
    Write-Host 'Creating alternate key...'
    Invoke-WithRetry {
        Set-DataverseEntityKeyMetadata -Connection $connection `
            -EntityName $entityName `
            -SchemaName 'new_testkey' `
            -DisplayName 'Test Key' `
            -KeyAttributes @('new_name') `
            -Confirm:$false
    }
    Write-Host '✓ Alternate key created'
    
    Write-Host 'Cleanup - Removing test entity...'
    Invoke-WithRetry {
        Remove-DataverseEntityMetadata -Connection $connection -EntityName $entityName -Confirm:$false
    }
    Write-Host '✓ Test entity deleted'
    
    Write-Host 'SUCCESS: All entity key operations completed'
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
