using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using AwesomeAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Metadata
{
    /// <summary>
    /// OptionSet metadata manipulation tests against a real Dataverse environment.
    /// Converted from e2e-tests/OptionSetMetadata.Tests.ps1
    /// </summary>
    /// <remarks>
    /// Placed in the SchemaAndPublishChanges collection because these tests create global option
    /// sets and also modify the shared contact.preferredcontactmethodcode option set, which
    /// conflicts with other tests if run in parallel.
    /// </remarks>
    [Collection(SchemaChangesCollection.Name)]
    public class OptionSetMetadataTests : E2ETestBase
    {
        [Fact]
        public void CanCreateReadUpdateAndDeleteGlobalOptionSetsComprehensively()
        {


            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

try {
    $timestamp = [DateTime]::UtcNow.ToString('yyyyMMddHHmm')
    $testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
    $optionSetName = ""new_opttest_${timestamp}_$testRunId""
    
    Write-Host 'Creating global option set...'
    Invoke-WithRetry {
        Set-DataverseOptionSetMetadata -Connection $connection `
            -Name $optionSetName `
            -DisplayName 'OptionSet Test' `
            -Options @(
                @{ Value = 1; Label = 'Option 1' },
                @{ Value = 2; Label = 'Option 2' }
            ) `
            -Confirm:$false
    }
    Write-Host '✓ Global option set created'
    
    Write-Host 'Cleanup - Removing global option set...'
    Invoke-WithRetry {
        $existingOptionSet = Get-DataverseOptionSetMetadata -Connection $connection | Where-Object { $_.Name -eq $optionSetName }
        if ($null -ne $existingOptionSet) {
            Remove-DataverseOptionSetMetadata -Connection $connection -Name $optionSetName -Confirm:$false
        }
    }
    Write-Host '✓ Global option set deleted'
    
    Write-Host 'SUCCESS: All option set operations completed'
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
        public void CanUpdateLocalOptionSetUsingEntityNameAndAttributeName()
        {
            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

try {
    Write-Host 'Getting current option set for contact.preferredcontactmethodcode...'
    $currentOptions = Invoke-WithRetry {
        Get-DataverseOptionSetMetadata -Connection $connection -EntityName contact -AttributeName preferredcontactmethodcode
    }
    Write-Host ""Current IsGlobal: $($currentOptions.IsGlobal)""
    Write-Host ""Current Options Count: $($currentOptions.Options.Count)""
    
    # Store original labels for restoration
    $originalOptions = @()
    foreach ($opt in $currentOptions.Options) {
        $originalOptions += @{
            Value = $opt.Value
            Label = $opt.Label.UserLocalizedLabel.Label
        }
    }
    
    Write-Host 'Updating local option set with new labels...'
    $result = Invoke-WithRetry {
        Set-DataverseOptionSetMetadata -Connection $connection `
            -EntityName contact `
            -AttributeName preferredcontactmethodcode `
            -Options @(
                @{Value=1; Label='Any (Test)'}
                @{Value=2; Label='Email (Test)'}
                @{Value=3; Label='Phone (Test)'}
                @{Value=4; Label='Fax (Test)'}
                @{Value=5; Label='Mail (Test)'}
            ) `
            -NoRemoveMissingOptions `
            -PassThru `
            -Confirm:$false
    }
    
    Write-Host ""Updated option set returned: $($result -ne $null)""
    Write-Host ""Updated Options Count: $($result.Options.Count)""
    
    # Verify the update
    $verify = Invoke-WithRetry {
        Get-DataverseOptionSetMetadata -Connection $connection -EntityName contact -AttributeName preferredcontactmethodcode
    }
    $testLabel = ($verify.Options | Where-Object { $_.Value -eq 1 }).Label.UserLocalizedLabel.Label
    Write-Host ""Verified label for value 1: $testLabel""
    
    if ($testLabel -ne 'Any (Test)') {
        throw ""Update verification failed. Expected 'Any (Test)' but got '$testLabel'""
    }
    
    Write-Host 'Restoring original labels...'
    Invoke-WithRetry {
        Set-DataverseOptionSetMetadata -Connection $connection `
            -EntityName contact `
            -AttributeName preferredcontactmethodcode `
            -Options $originalOptions `
            -NoRemoveMissingOptions `
            -Confirm:$false
    }
    
    Write-Host '✓ Local option set updated and restored successfully'
    Write-Host 'SUCCESS: Local option set operations completed'
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
