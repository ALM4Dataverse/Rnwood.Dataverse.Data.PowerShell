#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Tests the complete workflow version calculation logic including prerelease handling

.DESCRIPTION
    Simulates the workflow's version calculation with various scenarios involving
    stable releases and prereleases to ensure correct version determination
#>

$ErrorActionPreference = "Stop"

Write-Host "=== Testing Complete Workflow Version Calculation ===" -ForegroundColor Cyan
Write-Host ""

# Test scenarios simulating different repository states
$testScenarios = @(
    @{
        Name = "First PR with breaking change after stable release"
        StableRelease = "1.0.0"
        ExistingPrereleases = @()
        PRTitle = "feat!: add breaking change"
        ExpectedVersion = "2.0.0"
    },
    @{
        Name = "Second PR with breaking change (after first PR created prerelease)"
        StableRelease = "1.0.0"
        ExistingPrereleases = @("v2.0.0-ci20241101001")
        PRTitle = "feat!: another breaking change"
        ExpectedVersion = "2.0.0"
        Description = "Should NOT bump to 3.0.0 - first prerelease already did the major bump"
    },
    @{
        Name = "Third PR with feature (after breaking change prereleases)"
        StableRelease = "1.0.0"
        ExistingPrereleases = @("v2.0.0-ci20241101001", "v2.0.0-ci20241102001")
        PRTitle = "feat: add feature"
        ExpectedVersion = "2.0.0"
        Description = "Should stay at 2.0.0 - major bump is higher than minor"
    },
    @{
        Name = "First PR with feature after stable release"
        StableRelease = "1.4.0"
        ExistingPrereleases = @()
        PRTitle = "feat: add new feature"
        ExpectedVersion = "1.5.0"
    },
    @{
        Name = "Second PR with feature (after first PR created prerelease)"
        StableRelease = "1.4.0"
        ExistingPrereleases = @("v1.5.0-ci20241101001")
        PRTitle = "feat: another feature"
        ExpectedVersion = "1.5.0"
        Description = "Should NOT bump to 1.6.0 - first prerelease already did the minor bump"
    },
    @{
        Name = "PR with fix after feature prerelease"
        StableRelease = "1.4.0"
        ExistingPrereleases = @("v1.5.0-ci20241101001")
        PRTitle = "fix: bug fix"
        ExpectedVersion = "1.5.0"
        Description = "Should stay at 1.5.0 - feature bump is higher than patch"
    },
    @{
        Name = "PR with breaking change after feature prerelease"
        StableRelease = "1.4.0"
        ExistingPrereleases = @("v1.5.0-ci20241101001")
        PRTitle = "feat!: breaking change"
        ExpectedVersion = "2.0.0"
        Description = "Should bump to 2.0.0 - major bump is higher than existing minor"
    },
    @{
        Name = "Multiple prereleases with various bumps"
        StableRelease = "1.4.0"
        ExistingPrereleases = @("v1.4.1-ci20241101001", "v1.5.0-ci20241102001", "v1.4.2-ci20241103001")
        PRTitle = "feat: new feature"
        ExpectedVersion = "1.5.0"
        Description = "Should use highest prerelease (1.5.0) since it matches the feature bump"
    }
)

$passed = 0
$failed = 0

$stableTags = @(& ./scripts/Get-StableReleaseTags.ps1 -Tags @(
    "v3.0.1",
    "3.2.0",
    "3.1.0",
    "v3.1.0-ci202610193682"
))
$expectedStableTags = @("3.2.0", "3.1.0", "v3.0.1")
if (($stableTags -join ",") -eq ($expectedStableTags -join ",")) {
    Write-Host "✓ Stable release tags are sorted numerically across v-prefixed and bare tags" -ForegroundColor Green
    $passed++
} else {
    Write-Host "✗ Stable release tags were not sorted numerically: $($stableTags -join ', ')" -ForegroundColor Red
    $failed++
}

foreach ($scenario in $testScenarios) {
    Write-Host "Scenario: $($scenario.Name)" -ForegroundColor Yellow
    Write-Host "  Stable Release: $($scenario.StableRelease)"
    Write-Host "  Existing Prereleases: $(if ($scenario.ExistingPrereleases.Count -gt 0) { $scenario.ExistingPrereleases -join ', ' } else { 'None' })"
    Write-Host "  PR Title: $($scenario.PRTitle)"
    if ($scenario.Description) {
        Write-Host "  Expected Behavior: $($scenario.Description)" -ForegroundColor Gray
    }
    
    # Simulate the workflow logic
    $baseVersion = $scenario.StableRelease
    $existingPrereleases = $scenario.ExistingPrereleases
    
    # Call Get-NextVersion with the PR title
    $params = @{
        BaseVersion = $baseVersion
        CommitMessages = @($scenario.PRTitle)
    }
    if ($existingPrereleases.Count -gt 0) {
        $params.ExistingPrereleases = $existingPrereleases
    }
    
    $calculatedVersion = & ./scripts/Get-NextVersion.ps1 @params
    
    Write-Host "  Calculated Version: $calculatedVersion"
    Write-Host "  Expected Version: $($scenario.ExpectedVersion)"
    
    if ($calculatedVersion -eq $scenario.ExpectedVersion) {
        Write-Host "  ✓ PASSED" -ForegroundColor Green
        $passed++
    } else {
        Write-Host "  ✗ FAILED" -ForegroundColor Red
        $failed++
    }
    
    Write-Host ""
}

Write-Host "=== Test Summary ===" -ForegroundColor Cyan
Write-Host "  Passed: $passed" -ForegroundColor Green
Write-Host "  Failed: $failed" -ForegroundColor $(if ($failed -gt 0) { "Red" } else { "Green" })

if ($failed -gt 0) {
    exit 1
}

Write-Host ""
Write-Host "All workflow version calculation tests passed!" -ForegroundColor Green
