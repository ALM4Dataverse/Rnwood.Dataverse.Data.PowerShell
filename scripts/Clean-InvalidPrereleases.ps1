#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Identifies and optionally deletes incorrectly-versioned prerelease tags

.DESCRIPTION
    This script helps identify prerelease tags that are based on commits before
    the stable release tag. These tags can cause the CI version calculation to
    produce incorrect versions.
    
    The script:
    1. Finds the latest stable release tag
    2. Identifies all prerelease tags
    3. Checks if each prerelease is on a commit AFTER the stable tag
    4. Lists any prereleases that are BEFORE the stable tag
    5. Optionally deletes them (with -Delete flag)

.PARAMETER Delete
    If specified, deletes the invalid prerelease tags after confirmation

.PARAMETER Force
    If specified with -Delete, skips confirmation prompt

.EXAMPLE
    ./scripts/Clean-InvalidPrereleases.ps1
    Lists invalid prerelease tags without deleting them

.EXAMPLE
    ./scripts/Clean-InvalidPrereleases.ps1 -Delete
    Lists and deletes invalid prerelease tags after confirmation

.EXAMPLE
    ./scripts/Clean-InvalidPrereleases.ps1 -Delete -Force
    Lists and deletes invalid prerelease tags without confirmation
#>

param(
    [Parameter(Mandatory = $false)]
    [switch]$Delete,
    
    [Parameter(Mandatory = $false)]
    [switch]$Force
)

$ErrorActionPreference = "Stop"

Write-Host "=== Prerelease Tag Validation ===" -ForegroundColor Cyan
Write-Host ""

# Get all tags
$allTags = git tag --list 2>$null

# Find the latest stable release (no prerelease suffix)
$latestStableTag = & ./scripts/Get-StableReleaseTags.ps1 -Tags $allTags | Select-Object -First 1

if (-not $latestStableTag) {
    Write-Host "No stable release tag found. Nothing to check." -ForegroundColor Yellow
    exit 0
}

Write-Host "Latest stable release: $latestStableTag" -ForegroundColor Green

# Get stable tag commit
$stableCommit = git rev-list -n 1 $latestStableTag 2>$null

# Get all prerelease tags
$prereleaseTags = $allTags | Where-Object { $_ -match '-' }

Write-Host "Found $($prereleaseTags.Count) prerelease tag(s)" -ForegroundColor Gray
Write-Host ""

# Check each prerelease tag
$invalidTags = @()

Write-Host "Checking prerelease tags..." -ForegroundColor Cyan

foreach ($tag in $prereleaseTags) {
    $tagCommit = git rev-list -n 1 $tag 2>$null
    
    if ($tagCommit -and $stableCommit) {
        # Check if stable tag is an ancestor of this prerelease tag
        git merge-base --is-ancestor $stableCommit $tagCommit 2>$null
        $isAfterStable = $LASTEXITCODE -eq 0
        
        # Also check it's not the same commit
        $isSameCommit = $tagCommit -eq $stableCommit
        
        if (-not $isAfterStable -or $isSameCommit) {
            # This prerelease is BEFORE or AT the stable tag
            $shortCommit = if ($tagCommit.Length -ge 7) { $tagCommit.Substring(0, 7) } else { $tagCommit }
            $invalidTags += @{
                Tag = $tag
                Commit = $shortCommit
                Reason = if ($isSameCommit) { "Same commit as stable tag" } else { "Created before stable tag" }
            }
        }
    }
}

# Display results
Write-Host ""
if ($invalidTags.Count -eq 0) {
    Write-Host "✓ No invalid prerelease tags found!" -ForegroundColor Green
    exit 0
}

Write-Host "Found $($invalidTags.Count) invalid prerelease tag(s):" -ForegroundColor Yellow
Write-Host ""

foreach ($invalid in $invalidTags) {
    Write-Host "  • $($invalid.Tag)" -ForegroundColor Red
    Write-Host "    Commit: $($invalid.Commit)" -ForegroundColor Gray
    Write-Host "    Reason: $($invalid.Reason)" -ForegroundColor Gray
    Write-Host ""
}

Write-Host "These tags can cause incorrect version calculation in CI builds." -ForegroundColor Yellow
Write-Host ""

# Handle deletion
if ($Delete) {
    # Build git push command string for display
    $gitPushCommand = "git push origin " + (($invalidTags | ForEach-Object { ":refs/tags/$($_.Tag)" }) -join " ")
    
    if (-not $Force) {
        Write-Host "This will DELETE these tags from the local repository." -ForegroundColor Yellow
        Write-Host "You will need to push the deletions separately with:" -ForegroundColor Yellow
        Write-Host "  $gitPushCommand" -ForegroundColor Gray
        Write-Host ""
        
        $confirm = Read-Host "Do you want to delete these tags? (yes/no)"
        
        if ($confirm -ne "yes") {
            Write-Host "Deletion cancelled." -ForegroundColor Gray
            exit 0
        }
    }
    
    Write-Host ""
    Write-Host "Deleting invalid tags..." -ForegroundColor Cyan
    
    foreach ($invalid in $invalidTags) {
        Write-Host "  Deleting $($invalid.Tag)..." -ForegroundColor Gray
        git tag -d $invalid.Tag 2>$null
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "    ✓ Deleted locally" -ForegroundColor Green
        } else {
            Write-Host "    ✗ Failed to delete" -ForegroundColor Red
        }
    }
    
    Write-Host ""
    Write-Host "Local tags deleted. To delete from remote, run:" -ForegroundColor Yellow
    Write-Host "  $gitPushCommand" -ForegroundColor Gray
} else {
    Write-Host "To delete these tags, run:" -ForegroundColor Yellow
    Write-Host "  ./scripts/Clean-InvalidPrereleases.ps1 -Delete" -ForegroundColor Gray
    Write-Host ""
}
