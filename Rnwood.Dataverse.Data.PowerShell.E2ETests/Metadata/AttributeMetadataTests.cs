using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using AwesomeAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Metadata
{
    /// <summary>
    /// Attribute metadata manipulation tests against a real Dataverse environment.
    /// Converted from e2e-tests/AttributeMetadata.Tests.ps1
    /// </summary>
    /// <remarks>
    /// Placed in the SchemaAndPublishChanges collection because these tests create custom
    /// entities, modify attributes and execute Publish operations which conflict with other
    /// schema-changing tests when run in parallel.
    /// </remarks>
    [Collection(SchemaChangesCollection.Name)]
    public class AttributeMetadataTests : E2ETestBase
    {
[Fact]
        public void CanCreateReadUpdateAndDeleteAllAttributeTypesComprehensively()
        {


            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

try {
    $timestamp = [DateTime]::UtcNow.ToString('yyyyMMddHHmm')
    $testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
    $entityName = ""new_attrtest_${timestamp}_$testRunId""
    
    Write-Host 'Creating test entity...'
    Invoke-WithRetry {
        Set-DataverseEntityMetadata -Connection $connection `
            -EntityName $entityName `
            -SchemaName ""new_AttrTest_${timestamp}_$testRunId"" `
            -DisplayName 'Attribute Test Entity' `
            -DisplayCollectionName 'Attribute Test Entities' `
            -PrimaryAttributeSchemaName 'new_name' `
            -OwnershipType UserOwned `
            -Confirm:$false
    }
    Write-Host '✓ Test entity created'
    
    Write-Host 'Creating string attribute...'
    Invoke-WithRetry {
        Set-DataverseAttributeMetadata -Connection $connection `
            -EntityName $entityName `
            -AttributeName 'new_teststring' `
            -SchemaName 'new_TestString' `
            -AttributeType String `
            -DisplayName 'Test String' `
            -MaxLength 100 `
            -Confirm:$false
    }
    Write-Host '✓ String attribute created'
    
    Write-Host 'Cleanup - Removing test entity...'
    Invoke-WithRetry {
        Remove-DataverseEntityMetadata -Connection $connection -EntityName $entityName -Confirm:$false
    }
    Write-Host '✓ Test entity deleted'
    
    Write-Host 'SUCCESS: All attribute metadata operations completed'
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

        [Fact]
        public void CanUpdateStatusCodeOptionsWithStateProperty()
        {
            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

try {
    $timestamp = [DateTime]::UtcNow.ToString('yyyyMMddHHmmss')
    $testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
    $entityName = ""new_statustest_${timestamp}_$testRunId""
    
    Write-Host 'Creating test entity with custom statuscode...'
    Invoke-WithRetry {
        Set-DataverseEntityMetadata -Connection $connection `
            -EntityName $entityName `
            -SchemaName ""new_StatusTest_${timestamp}_$testRunId"" `
            -DisplayName 'StatusCode Test Entity' `
            -DisplayCollectionName 'StatusCode Test Entities' `
            -PrimaryAttributeSchemaName 'new_name' `
            -OwnershipType UserOwned `
            -Confirm:$false
        
        # Publish to make statuscode attribute available
        Invoke-DataverseRequest -Connection $connection -Request (New-Object Microsoft.Crm.Sdk.Messages.PublishXmlRequest -Property @{
            ParameterXml = ""<importexportxml><entities><entity>$entityName</entity></entities></importexportxml>""
        })
        
        Start-Sleep -Seconds 5
    }
    Write-Host '✓ Test entity created and published'
    
    Write-Host 'Updating statuscode options with State property...'
    $statusOptions = @(
        @{Value=1; Label='Draft'; State=0},
        @{Value=807290000; Label='Approved'; State=0},
        @{Value=807290001; Label='Filled'; State=0},
        @{Value=2; Label='Closed'; State=1},
        @{Value=807290002; Label='Cancelled'; State=1}
    )
    
    Invoke-WithRetry {
        Set-DataverseAttributeMetadata -Connection $connection `
            -EntityName $entityName `
            -AttributeName 'statuscode' `
            -Options $statusOptions `
            -Confirm:$false
        
        # Publish changes
        Invoke-DataverseRequest -Connection $connection -Request (New-Object Microsoft.Crm.Sdk.Messages.PublishXmlRequest -Property @{
            ParameterXml = ""<importexportxml><entities><entity>$entityName</entity></entities></importexportxml>""
        })
        
        Start-Sleep -Seconds 5
    }
    Write-Host '✓ StatusCode options updated'
    
    Write-Host 'Verifying statuscode options...'
    $statusAttr = Get-DataverseAttributeMetadata -Connection $connection `
        -EntityName $entityName `
        -AttributeName 'statuscode'
    
    $statusAttr | Should -Not -BeNullOrEmpty
    $statusAttr.OptionSet.Options.Count | Should -BeGreaterOrEqual 5
    
    # Verify that all our custom options exist
    $draftOption = $statusAttr.OptionSet.Options | Where-Object { $_.Value -eq 1 }
    $draftOption | Should -Not -BeNullOrEmpty
    $draftOption.Label.UserLocalizedLabel.Label | Should -Be 'Draft'
    $draftOption.State | Should -Be 0
    
    $closedOption = $statusAttr.OptionSet.Options | Where-Object { $_.Value -eq 2 }
    $closedOption | Should -Not -BeNullOrEmpty
    $closedOption.Label.UserLocalizedLabel.Label | Should -Be 'Closed'
    $closedOption.State | Should -Be 1
    
    Write-Host '✓ StatusCode options verified'
    
    Write-Host 'Cleanup - Removing test entity...'
    Invoke-WithRetry {
        Remove-DataverseEntityMetadata -Connection $connection -EntityName $entityName -Confirm:$false
    }
    Write-Host '✓ Test entity deleted'
    
    Write-Host 'SUCCESS: StatusCode update with State property completed'
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
