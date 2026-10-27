param(
	[string]$ProjectDir = ".",
	[string]$OutDir = "."
)

$ErrorActionPreference="Stop"
$projectdir=$ProjectDir.Trim('""')
$outdir=$OutDir.Trim('""')


if (-not(get-installedmodule Platyps -MinimumVersion 0.14.1 -ErrorAction silentlycontinue)) {
	$installAttempts = 5
	for ($attempt = 1; $attempt -le $installAttempts; $attempt++) {
		try {
			install-module -scope CurrentUser PlatyPs -force -ErrorAction Stop
			break
		} catch {
			if ($attempt -eq $installAttempts) { throw }
			Write-Warning "Failed to install PlatyPS on attempt $attempt; retrying after a short delay."
			Start-Sleep -Seconds (5 * $attempt)
		}
	}
}

$modulepath = "$outdir/Rnwood.Dataverse.Data.PowerShell.psd1"
write-host "Using module: $modulepath"

# Check if module exists before trying to import
if (-not (Test-Path $modulepath)) {
	write-host "Module not found at $modulepath - skipping help update (module not built yet)"
	exit 0
}

# Try to import the module, exit gracefully if it fails
try {
	import-module $modulepath -ErrorAction Stop
} catch {
	write-host "Could not import module from $modulepath - skipping help update: $_"
	exit 0
}

# --- Incremental-run guard: skip work if inputs haven't changed ---
$stampFile = Join-Path $OutDir 'updatehelp.stamp'

# Build list of inputs that should trigger an update when changed
$helpInputs = @()
$docsDir = Join-Path $ProjectDir 'docs'
if (Test-Path $docsDir) {
	$helpInputs += Get-ChildItem -Path $docsDir -Filter '*.md' -Recurse -File | Select-Object -ExpandProperty FullName
}
$helpInputs += (Join-Path $ProjectDir 'updatehelp.ps1')
$helpInputs += (Join-Path $ProjectDir 'buildhelp.ps1')
$cmdletDll = Join-Path $OutDir 'cmdlets\Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll'
if (Test-Path $cmdletDll) {
	$helpInputs += $cmdletDll
}

# Determine latest input timestamp
$maxInputTime = [datetime]::MinValue
foreach ($f in $helpInputs) {
	try {
		$item = Get-Item -LiteralPath $f -ErrorAction Stop
		if ($item.LastWriteTimeUtc -gt $maxInputTime) { $maxInputTime = $item.LastWriteTimeUtc }
	} catch {
		# Ignore missing files; absence means they can't trigger an update
	}
}

# If stamp exists and is newer-or-equal to all inputs, skip actual update work
if (Test-Path $stampFile) {
	$stampTime = (Get-Item $stampFile).LastWriteTimeUtc
	if ($stampTime -ge $maxInputTime) {
		Write-Verbose "updatehelp.ps1: no inputs changed since $stampTime; skipping help update."
		Exit 0
	}
}

# Ensure output directory exists
if (-not (Test-Path $OutDir)) { New-Item -ItemType Directory -Path $OutDir -Force | Out-Null }

update-markdownhelpmodule -path $projectdir/docs -UpdateInputOutput -AlphabeticParamsOrder -RefreshModulePage

# After successful update, write/update the stamp so future runs can be skipped
$now = (Get-Date).ToUniversalTime().ToString("o")
$stampContent = "updatehelp ran at $now"
Set-Content -Path $stampFile -Value $stampContent -Encoding UTF8
# --- End incremental-run guard ---
