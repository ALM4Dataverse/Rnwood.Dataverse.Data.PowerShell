#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Sorts stable release tags by their numeric version.

.DESCRIPTION
    Accepts tags in either vX.Y.Z or X.Y.Z format and returns stable tags
    sorted from highest to lowest version.

.PARAMETER Tags
    Tag names to filter and sort.

.EXAMPLE
    ./scripts/Get-StableReleaseTags.ps1 -Tags @("v3.0.1", "3.2.0", "3.1.0")
    Returns the tags in numeric version order, with 3.2.0 first.
#>

param(
    [Parameter(Mandatory = $true)]
    [AllowEmptyCollection()]
    [string[]]$Tags
)

$stableTags = foreach ($tag in $Tags) {
    if ($tag -match '^v?(\d+)\.(\d+)\.(\d+)$') {
        [pscustomobject]@{
            Tag = $tag
            Version = [version]::new([int]$matches[1], [int]$matches[2], [int]$matches[3])
        }
    }
}

$stableTags |
    Sort-Object -Property Version -Descending |
    ForEach-Object { $_.Tag }
