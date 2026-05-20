using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Solution
{
    /// <summary>
    /// Solution component manipulation tests against a real Dataverse environment.
    /// Converted from e2e-tests/SolutionComponent.Tests.ps1
    /// </summary>
    [Collection(SchemaChangesCollection.Name)]
    public class SolutionComponentTests : E2ETestBase
    {
        [Fact]
        public void GetDataverseSolutionComponent_ReadsComponentsFromEnvironmentSummary()
        {


            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

try {
    $testStartTime = Get-Date
    Write-Host '[TIMING] Test started'
    
    # Wait for any existing operations to complete before starting test
    $stepStartTime = Get-Date
    Write-Host 'Pre-check: Ensuring no pending operations...'
    Start-Sleep -Seconds 5
    $stepDuration = ((Get-Date) - $stepStartTime).TotalSeconds
    Write-Host ""[TIMING] Pre-check completed in $stepDuration seconds""

    $timestamp = [DateTime]::UtcNow.ToString('yyyyMMddHHmm')
    $testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
    $solutionName = ""e2esolcomp_${timestamp}_$testRunId""
    $solutionDisplayName = ""E2E Solution Component Test $testRunId""
    $publisherPrefix = 'e2e'
    # Use standard 'account' entity instead of creating a custom entity
    $entityName = 'account'
    
    Write-Host ""Test solution: $solutionName""
    Write-Host ""Using standard entity: $entityName""
    
    $stepStartTime = Get-Date
    Write-Host 'Step 1: Creating test solution...'
    Invoke-WithRetry {
        
        $existingSolution = Get-DataverseSolution -Connection $connection -UniqueName $solutionName -ErrorAction SilentlyContinue
        if ($existingSolution) {
            Write-Host '  Removing existing test solution...'
            Remove-DataverseSolution -Connection $connection -UniqueName $solutionName -Confirm:$false
        }
        
        $publisher = Get-DataverseRecord -Connection $connection -TableName publisher -FilterValues @{ 'customizationprefix' = $publisherPrefix } | Select-Object -First 1
        if (-not $publisher) {
            Write-Host '  Creating test publisher...'
            $publisher = @{
                'uniquename' = ""e2etestpublisher_$testRunId""
                'friendlyname' = 'E2E Test Publisher'
                'customizationprefix' = $publisherPrefix
            } | Set-DataverseRecord -Connection $connection -TableName publisher -PassThru
        }
        
        Set-DataverseSolution -Connection $connection `
            -UniqueName $solutionName `
            -Name $solutionDisplayName `
            -Version '1.0.0.0' `
            -PublisherUniqueName $publisher.uniquename `
            -Confirm:$false
        
        $solution = Get-DataverseSolution -Connection $connection -UniqueName $solutionName
        $script:solutionId = $solution.Id
    }
    $stepDuration = ((Get-Date) - $stepStartTime).TotalSeconds
    Write-Host ""✓ Test solution created (ID: $($script:solutionId))""
    Write-Host ""[TIMING] Step 1 completed in $stepDuration seconds""
    
    $stepStartTime = Get-Date
    Write-Host 'Step 2: Getting entity metadata...'
    Invoke-WithRetry {
        $entityMetadata = Get-DataverseEntityMetadata -Connection $connection -EntityName $entityName
        $script:entityObjectId = $entityMetadata.MetadataId
    }
    $stepDuration = ((Get-Date) - $stepStartTime).TotalSeconds
    Write-Host ""✓ Entity ObjectId retrieved: $($script:entityObjectId)""
    Write-Host ""[TIMING] Step 2 completed in $stepDuration seconds""
    
    $stepStartTime = Get-Date
    Write-Host 'Step 3: Adding entity to solution for summary retrieval...'
    Invoke-WithRetry {
        Set-DataverseSolutionComponent -Connection $connection `
            -SolutionName $solutionName `
            -ComponentId $script:entityObjectId `
            -ComponentType 1 `
            -Behavior 0 `
            -Confirm:$false
    }
    $stepDuration = ((Get-Date) - $stepStartTime).TotalSeconds
    Write-Host '✓ Entity added to solution'
    Write-Host ""[TIMING] Step 3 completed in $stepDuration seconds""
    
    $stepStartTime = Get-Date
    Write-Host 'Step 4: Verifying component exists in solution...'
    Invoke-WithRetry {
        $components = Get-DataverseSolutionComponent -Connection $connection -SolutionName $solutionName
        
        $entityComponent = $components | Where-Object { $_.ObjectId -eq $entityName }
        if (-not $entityComponent) {
            throw 'Entity component not found in solution'
        }
        if ($entityComponent.ComponentType -ne 1) {
            throw ""Expected component type 1 (Entity), got $($entityComponent.ComponentType)""
        }
        Write-Host ""  Found component: Type=$($entityComponent.ComponentType), Behavior=$($entityComponent.Behavior)""
        
        if ($entityComponent.Behavior -ne 'IncludeSubcomponents') {
            throw ""Expected behavior 'IncludeSubcomponents' for Behavior 0, got '$($entityComponent.Behavior)'""
        }
    }
    $stepDuration = ((Get-Date) - $stepStartTime).TotalSeconds
    Write-Host '✓ Component verified via solution component summary'
    Write-Host ""[TIMING] Step 4 completed in $stepDuration seconds""
    
    $stepStartTime = Get-Date
    Write-Host 'Step 5: Cleanup - Removing test solution...'
    Invoke-WithRetry {
        Remove-DataverseSolution -Connection $connection -UniqueName $solutionName -Confirm:$false
    }
    $stepDuration = ((Get-Date) - $stepStartTime).TotalSeconds
    Write-Host '✓ Test solution deleted'
    Write-Host ""[TIMING] Step 5 completed in $stepDuration seconds""
    
    $testDuration = ((Get-Date) - $testStartTime).TotalSeconds
    Write-Host ""[TIMING] Total test duration: $testDuration seconds""
    Write-Host 'SUCCESS: All solution component operations completed successfully'
}
catch {
    $testDuration = ((Get-Date) - $testStartTime).TotalSeconds
    Write-Host ""[TIMING] Test failed after $testDuration seconds""
    Write-Host ""ERROR: $($_ | Out-String)""
    throw
}
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("SUCCESS", because: result.GetFullOutput());
        }

        [Fact]
        public void SolutionCmdlets_AcceptSolutionHistoryPrecheckParameters()
        {
            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

$timestamp = [DateTime]::UtcNow.ToString('yyyyMMddHHmmss')
$testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
$solutionName = ""e2esolhist_${timestamp}_$testRunId""
$solutionDisplayName = ""E2E Solution History Test $testRunId""
$publisherPrefix = 'e2h'
$solutionCreated = $false

try {
    Invoke-WithRetry {
        $script:publisher = Get-DataverseRecord -Connection $connection -TableName publisher -FilterValues @{ 'customizationprefix' = $publisherPrefix } | Select-Object -First 1
        if (-not $script:publisher) {
            $script:publisher = @{
                'uniquename' = ""e2ehistorypublisher_$testRunId""
                'friendlyname' = 'E2E Solution History Publisher'
                'customizationprefix' = $publisherPrefix
            } | Set-DataverseRecord -Connection $connection -TableName publisher -PassThru
        }
    }

    Invoke-WithRetry {
        Set-DataverseSolution -Connection $connection `
            -UniqueName $solutionName `
            -Name $solutionDisplayName `
            -Version '1.0.0.0' `
            -PublisherUniqueName $script:publisher.uniquename `
            -Confirm:$false
        $script:solutionCreated = $true
    }

    Invoke-WithRetry {
        Remove-DataverseSolution -Connection $connection `
            -UniqueName $solutionName `
            -SolutionHistoryWaitSeconds 10 `
            -Confirm:$false `
            -Verbose
    }

    Invoke-WithRetry {
        Invoke-DataverseSolutionUpgrade -Connection $connection `
            -SolutionName $solutionName `
            -IfExists `
            -SkipSolutionHistoryCheck `
            -Verbose
    }

    Write-Host 'SUCCESS: Solution history parameters executed successfully'
}
catch {
    Write-Host ""ERROR: $($_ | Out-String)""
    throw
}
finally {
    if ($solutionCreated) {
        try {
            Invoke-WithRetry {
                $existingSolution = Get-DataverseSolution -Connection $connection -UniqueName $solutionName -ErrorAction SilentlyContinue
                if ($existingSolution) {
                    Remove-DataverseSolution -Connection $connection `
                        -UniqueName $solutionName `
                        -SkipSolutionHistoryCheck `
                        -Confirm:$false `
                        -Verbose:$false
                }
            }
        }
        catch {
            Write-Host ""CLEANUP WARNING: $($_ | Out-String)""
        }
    }
}
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("SUCCESS", because: result.GetFullOutput());
        }
    }
}
