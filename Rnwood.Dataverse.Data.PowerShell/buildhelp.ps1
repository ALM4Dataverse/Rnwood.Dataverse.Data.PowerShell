param([Parameter(Mandatory)][string]$projectdir, [Parameter(Mandatory)][string]$outdir)
$ErrorActionPreference = "Stop"
$projectdir = $projectdir.Trim('""')
$outdir = $outdir.Trim('""')

function Compute-FolderDocsHash {
    param([string]$path)
    if (-not (Test-Path $path)) { return $null }
    $files = Get-ChildItem -Path $path -Recurse -File -ErrorAction Stop | Sort-Object -Property FullName
    if ($files.Count -eq 0) { return $null }

    $sb = New-Object System.Text.StringBuilder
    foreach ($f in $files) {
        $h = Get-FileHash -Path $f.FullName -Algorithm SHA256
        # include full name and file hash so renames/moves also change the combined hash
        [void]$sb.Append($f.FullName + ':' + $h.Hash + ';')
    }

    $bytes = [System.Text.Encoding]::UTF8.GetBytes($sb.ToString())
    $sha = [System.Security.Cryptography.SHA256]::Create()
    $hashBytes = $sha.ComputeHash($bytes)
    $hash = -join ($hashBytes | ForEach-Object { $_.ToString('x2') })
    return $hash
}

$docsPath = Join-Path $projectdir 'docs'
$hashFile = Join-Path $outdir 'buildhelp.hash'

$currentHash = Compute-FolderDocsHash -path $docsPath
if ($null -eq $currentHash) {
    Write-Verbose "No docs found at '$docsPath' - nothing to do."
    exit 0
}

$previousHash = $null
if (Test-Path $hashFile) {
    try { $previousHash = Get-Content $hashFile -ErrorAction Stop -Raw } catch { $previousHash = $null }
}

if ($previousHash -eq $currentHash) {
    Write-Verbose "Docs have not changed since last run. Skipping help generation."
    exit 0
}

# Ensure PlatyPs is available
if (-not (Get-InstalledModule PlatyPs -MinimumVersion 0.14.1 -ErrorAction SilentlyContinue)) {
    Install-Module -Scope CurrentUser PlatyPs -Force
}

# Remove existing generated help only when we are going to regenerate
if (Test-Path (Join-Path $outdir 'en-GB')) {
    Remove-Item -Verbose -Recurse -Force (Join-Path $outdir 'en-GB')
}

# Ensure output dir exists
if (-not (Test-Path $outdir)) { New-Item -ItemType Directory -Path $outdir -Force | Out-Null }

New-ExternalHelp -Verbose -Path $docsPath -OutputPath (Join-Path $outdir 'en-GB')

# Also copy to en-US so PowerShell on en-US Windows locales can find help
$enUS = Join-Path $outdir 'en-US'
if (Test-Path $enUS) {
    Remove-Item -Recurse -Force $enUS
}
Copy-Item -Recurse (Join-Path $outdir 'en-GB') $enUS

# Save the new hash for incremental checks next time
try {
    $currentHash | Out-File -FilePath $hashFile -Encoding UTF8 -Force
} catch {
    Write-Warning "Failed to write hash file to '$hashFile': $_"
}