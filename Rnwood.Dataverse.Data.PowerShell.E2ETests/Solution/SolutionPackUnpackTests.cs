using FluentAssertions;
using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Solution
{
    /// <summary>
    /// E2E tests for Export-DataverseSolution (ToFolder), Expand-DataverseSolutionFile,
    /// and Compress-DataverseSolutionFile, including the new -SourceFormat and -MapFile parameters.
    /// </summary>
    public class SolutionPackUnpackTests : E2ETestBase
    {
        /// <summary>
        /// PowerShell fragment that creates a minimal test solution and stores its name in $testSolutionName.
        /// Includes cleanup on exit.
        /// </summary>
        private const string SetupTestSolution = @"
$ts      = [DateTime]::UtcNow.ToString('yyyyMMddHHmm')
$runId   = [guid]::NewGuid().ToString('N').Substring(0,8)
$testSolutionName = ""e2epkunpk_${ts}_${runId}""
$pubPrefix = 'e2pk'

$pub = Get-DataverseRecord -Connection $connection -TableName publisher -FilterValues @{ 'customizationprefix' = $pubPrefix } | Select-Object -First 1
if (-not $pub) {
    $pub = @{
        'uniquename'         = ""e2pkpub_${runId}""
        'friendlyname'       = 'E2E Pack Test Publisher'
        'customizationprefix'= $pubPrefix
    } | Set-DataverseRecord -Connection $connection -TableName publisher -PassThru
}

Set-DataverseSolution -Connection $connection `
    -UniqueName $testSolutionName `
    -Name ""E2E Pack Test $runId"" `
    -Version '1.0.0.0' `
    -PublisherUniqueName $pub.uniquename `
    -Confirm:$false
Write-Host ""Created test solution: $testSolutionName""
";

        [Fact]
        public void ExportDataverseSolution_ToFolder_Basic_Succeeds()
        {
            var script = GetConnectionScript(SetupTestSolution + @"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

$outFolder = Join-Path ([System.IO.Path]::GetTempPath()) ('e2e_export_basic_' + $runId)

try {
    Write-Host 'Exporting test solution to folder (basic, no SourceFormat)...'
    Export-DataverseSolution -Connection $connection -SolutionName $testSolutionName -OutFolder $outFolder -Confirm:$false -Verbose

    $files = Get-ChildItem $outFolder -Recurse | Where-Object { -not $_.PSIsContainer }
    if ($files.Count -eq 0) { throw 'No files were written to output folder' }

    Write-Host ""SUCCESS: Exported $($files.Count) file(s) to $outFolder""
}
finally {
    Remove-DataverseSolution -Connection $connection -UniqueName $testSolutionName -Confirm:$false -ErrorAction SilentlyContinue
    if (Test-Path $outFolder) { Remove-Item $outFolder -Recurse -Force -ErrorAction SilentlyContinue }
}
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("SUCCESS", because: result.GetFullOutput());
        }

        [Fact]
        public void ExportDataverseSolution_ToFolder_WithSourceFormatXml_Succeeds()
        {
            var script = GetConnectionScript(SetupTestSolution + @"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

$outFolder = Join-Path ([System.IO.Path]::GetTempPath()) ('e2e_export_xml_' + $runId)

try {
    Write-Host 'Exporting test solution to folder with -SourceFormat Xml...'
    Export-DataverseSolution -Connection $connection -SolutionName $testSolutionName -OutFolder $outFolder -SourceFormat Xml -Confirm:$false -Verbose

    $files = Get-ChildItem $outFolder -Recurse | Where-Object { -not $_.PSIsContainer }
    if ($files.Count -eq 0) { throw 'No files were written to output folder' }

    Write-Host ""SUCCESS: Exported $($files.Count) file(s) with SourceFormat Xml""
}
finally {
    Remove-DataverseSolution -Connection $connection -UniqueName $testSolutionName -Confirm:$false -ErrorAction SilentlyContinue
    if (Test-Path $outFolder) { Remove-Item $outFolder -Recurse -Force -ErrorAction SilentlyContinue }
}
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("SUCCESS", because: result.GetFullOutput());
        }

        [Fact]
        public void ExportDataverseSolution_ToFolder_WithSourceFormatYaml_Succeeds()
        {
            var script = GetConnectionScript(SetupTestSolution + @"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

$outFolder = Join-Path ([System.IO.Path]::GetTempPath()) ('e2e_export_yaml_' + $runId)

try {
    Write-Host 'Exporting test solution to folder with -SourceFormat Yaml...'
    Export-DataverseSolution -Connection $connection -SolutionName $testSolutionName -OutFolder $outFolder -SourceFormat Yaml -Confirm:$false -Verbose

    $files = Get-ChildItem $outFolder -Recurse | Where-Object { -not $_.PSIsContainer }
    if ($files.Count -eq 0) { throw 'No files were written to output folder' }

    Write-Host ""SUCCESS: Exported $($files.Count) file(s) with SourceFormat Yaml""
}
finally {
    Remove-DataverseSolution -Connection $connection -UniqueName $testSolutionName -Confirm:$false -ErrorAction SilentlyContinue
    if (Test-Path $outFolder) { Remove-Item $outFolder -Recurse -Force -ErrorAction SilentlyContinue }
}
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("SUCCESS", because: result.GetFullOutput());
        }

        [Fact]
        public void ExportDataverseSolution_ToFolder_WithMapFile_Succeeds()
        {
            var script = GetConnectionScript(SetupTestSolution + @"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

$outFolder = Join-Path ([System.IO.Path]::GetTempPath()) ('e2e_export_map_' + $runId)
$mapFile   = Join-Path ([System.IO.Path]::GetTempPath()) ('e2e_map_' + $runId + '.xml')

try {
    @'
<?xml version=""1.0"" encoding=""utf-8""?>
<Mapping>
</Mapping>
'@ | Set-Content -Path $mapFile -Encoding UTF8

    Write-Host 'Exporting test solution to folder with -MapFile...'
    Export-DataverseSolution -Connection $connection -SolutionName $testSolutionName -OutFolder $outFolder -MapFile $mapFile -Confirm:$false -Verbose

    $files = Get-ChildItem $outFolder -Recurse | Where-Object { -not $_.PSIsContainer }
    if ($files.Count -eq 0) { throw 'No files were written to output folder' }

    Write-Host ""SUCCESS: Exported $($files.Count) file(s) with MapFile""
}
finally {
    Remove-DataverseSolution -Connection $connection -UniqueName $testSolutionName -Confirm:$false -ErrorAction SilentlyContinue
    if (Test-Path $outFolder) { Remove-Item $outFolder -Recurse -Force -ErrorAction SilentlyContinue }
    if (Test-Path $mapFile)   { Remove-Item $mapFile -Force -ErrorAction SilentlyContinue }
}
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("SUCCESS", because: result.GetFullOutput());
        }

        [Fact]
        public void ExpandDataverseSolutionFile_Basic_Succeeds()
        {
            var script = GetConnectionScript(SetupTestSolution + @"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

$tempDir   = Join-Path ([System.IO.Path]::GetTempPath()) ('e2e_expand_' + $runId)
$zipFile   = Join-Path $tempDir 'solution.zip'
$outFolder = Join-Path $tempDir 'unpacked'

try {
    New-Item -ItemType Directory -Path $tempDir | Out-Null

    Write-Host 'Exporting solution to zip...'
    Export-DataverseSolution -Connection $connection -SolutionName $testSolutionName -OutFile $zipFile -Confirm:$false

    Write-Host 'Expanding solution zip (basic)...'
    Expand-DataverseSolutionFile -Path $zipFile -OutputPath $outFolder -Confirm:$false -Verbose

    $files = Get-ChildItem $outFolder -Recurse | Where-Object { -not $_.PSIsContainer }
    if ($files.Count -eq 0) { throw 'No files were written to output folder' }

    Write-Host ""SUCCESS: Expanded to $($files.Count) file(s) in $outFolder""
}
finally {
    Remove-DataverseSolution -Connection $connection -UniqueName $testSolutionName -Confirm:$false -ErrorAction SilentlyContinue
    if (Test-Path $tempDir) { Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue }
}
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("SUCCESS", because: result.GetFullOutput());
        }

        [Fact]
        public void ExpandDataverseSolutionFile_WithSourceFormatXml_Succeeds()
        {
            var script = GetConnectionScript(SetupTestSolution + @"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

$tempDir   = Join-Path ([System.IO.Path]::GetTempPath()) ('e2e_expand_xml_' + $runId)
$zipFile   = Join-Path $tempDir 'solution.zip'
$outFolder = Join-Path $tempDir 'unpacked'

try {
    New-Item -ItemType Directory -Path $tempDir | Out-Null

    Export-DataverseSolution -Connection $connection -SolutionName $testSolutionName -OutFile $zipFile -Confirm:$false

    Write-Host 'Expanding solution zip with -SourceFormat Xml...'
    Expand-DataverseSolutionFile -Path $zipFile -OutputPath $outFolder -SourceFormat Xml -Confirm:$false -Verbose

    $files = Get-ChildItem $outFolder -Recurse | Where-Object { -not $_.PSIsContainer }
    if ($files.Count -eq 0) { throw 'No files were written to output folder' }

    Write-Host ""SUCCESS: Expanded (SourceFormat Xml) to $($files.Count) file(s)""
}
finally {
    Remove-DataverseSolution -Connection $connection -UniqueName $testSolutionName -Confirm:$false -ErrorAction SilentlyContinue
    if (Test-Path $tempDir) { Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue }
}
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("SUCCESS", because: result.GetFullOutput());
        }

        [Fact]
        public void ExpandDataverseSolutionFile_WithSourceFormatYaml_Succeeds()
        {
            var script = GetConnectionScript(SetupTestSolution + @"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

$tempDir   = Join-Path ([System.IO.Path]::GetTempPath()) ('e2e_expand_yaml_' + $runId)
$zipFile   = Join-Path $tempDir 'solution.zip'
$outFolder = Join-Path $tempDir 'unpacked'

try {
    New-Item -ItemType Directory -Path $tempDir | Out-Null

    Export-DataverseSolution -Connection $connection -SolutionName $testSolutionName -OutFile $zipFile -Confirm:$false

    Write-Host 'Expanding solution zip with -SourceFormat Yaml...'
    Expand-DataverseSolutionFile -Path $zipFile -OutputPath $outFolder -SourceFormat Yaml -Confirm:$false -Verbose

    $files = Get-ChildItem $outFolder -Recurse | Where-Object { -not $_.PSIsContainer }
    if ($files.Count -eq 0) { throw 'No files were written to output folder' }

    Write-Host ""SUCCESS: Expanded (SourceFormat Yaml) to $($files.Count) file(s)""
}
finally {
    Remove-DataverseSolution -Connection $connection -UniqueName $testSolutionName -Confirm:$false -ErrorAction SilentlyContinue
    if (Test-Path $tempDir) { Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue }
}
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("SUCCESS", because: result.GetFullOutput());
        }

        [Fact]
        public void ExpandDataverseSolutionFile_WithMapFile_Succeeds()
        {
            var script = GetConnectionScript(SetupTestSolution + @"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

$tempDir   = Join-Path ([System.IO.Path]::GetTempPath()) ('e2e_expand_map_' + $runId)
$zipFile   = Join-Path $tempDir 'solution.zip'
$outFolder = Join-Path $tempDir 'unpacked'
$mapFile   = Join-Path $tempDir 'mapping.xml'

try {
    New-Item -ItemType Directory -Path $tempDir | Out-Null

    @'
<?xml version=""1.0"" encoding=""utf-8""?>
<Mapping>
</Mapping>
'@ | Set-Content -Path $mapFile -Encoding UTF8

    Export-DataverseSolution -Connection $connection -SolutionName $testSolutionName -OutFile $zipFile -Confirm:$false

    Write-Host 'Expanding solution zip with -MapFile...'
    Expand-DataverseSolutionFile -Path $zipFile -OutputPath $outFolder -MapFile $mapFile -Confirm:$false -Verbose

    $files = Get-ChildItem $outFolder -Recurse | Where-Object { -not $_.PSIsContainer }
    if ($files.Count -eq 0) { throw 'No files were written to output folder' }

    Write-Host ""SUCCESS: Expanded with MapFile to $($files.Count) file(s)""
}
finally {
    Remove-DataverseSolution -Connection $connection -UniqueName $testSolutionName -Confirm:$false -ErrorAction SilentlyContinue
    if (Test-Path $tempDir) { Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue }
}
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("SUCCESS", because: result.GetFullOutput());
        }

        [Fact]
        public void CompressDataverseSolutionFile_Basic_RoundTrip_Succeeds()
        {
            var script = GetConnectionScript(SetupTestSolution + @"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

$tempDir      = Join-Path ([System.IO.Path]::GetTempPath()) ('e2e_compress_' + $runId)
$zipFile      = Join-Path $tempDir 'solution.zip'
$outFolder    = Join-Path $tempDir 'unpacked'
$repackedZip  = Join-Path $tempDir 'repacked.zip'

try {
    New-Item -ItemType Directory -Path $tempDir | Out-Null

    Write-Host 'Exporting solution to zip...'
    Export-DataverseSolution -Connection $connection -SolutionName $testSolutionName -OutFile $zipFile -Confirm:$false

    Write-Host 'Expanding solution...'
    Expand-DataverseSolutionFile -Path $zipFile -OutputPath $outFolder -Confirm:$false

    Write-Host 'Compressing solution folder (basic)...'
    Compress-DataverseSolutionFile -Path $outFolder -OutputPath $repackedZip -Confirm:$false -Verbose

    if (-not (Test-Path $repackedZip)) { throw ""Repacked zip not found: $repackedZip"" }
    $size = (Get-Item $repackedZip).Length
    if ($size -eq 0) { throw 'Repacked zip is empty' }

    Write-Host ""SUCCESS: Compress-DataverseSolutionFile produced $size byte zip""
}
finally {
    Remove-DataverseSolution -Connection $connection -UniqueName $testSolutionName -Confirm:$false -ErrorAction SilentlyContinue
    if (Test-Path $tempDir) { Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue }
}
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("SUCCESS", because: result.GetFullOutput());
        }

        [Fact]
        public void CompressDataverseSolutionFile_WithMapFile_Succeeds()
        {
            var script = GetConnectionScript(SetupTestSolution + @"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

$tempDir      = Join-Path ([System.IO.Path]::GetTempPath()) ('e2e_compress_map_' + $runId)
$zipFile      = Join-Path $tempDir 'solution.zip'
$outFolder    = Join-Path $tempDir 'unpacked'
$repackedZip  = Join-Path $tempDir 'repacked_map.zip'
$mapFile      = Join-Path $tempDir 'mapping.xml'

try {
    New-Item -ItemType Directory -Path $tempDir | Out-Null

    @'
<?xml version=""1.0"" encoding=""utf-8""?>
<Mapping>
</Mapping>
'@ | Set-Content -Path $mapFile -Encoding UTF8

    Write-Host 'Exporting solution to zip...'
    Export-DataverseSolution -Connection $connection -SolutionName $testSolutionName -OutFile $zipFile -Confirm:$false

    Write-Host 'Expanding solution...'
    Expand-DataverseSolutionFile -Path $zipFile -OutputPath $outFolder -Confirm:$false

    Write-Host 'Compressing solution folder with -MapFile...'
    Compress-DataverseSolutionFile -Path $outFolder -OutputPath $repackedZip -MapFile $mapFile -Confirm:$false -Verbose

    if (-not (Test-Path $repackedZip)) { throw ""Repacked zip not found: $repackedZip"" }
    $size = (Get-Item $repackedZip).Length
    if ($size -eq 0) { throw 'Repacked zip is empty' }

    Write-Host ""SUCCESS: Compress-DataverseSolutionFile with MapFile produced $size byte zip""
}
finally {
    Remove-DataverseSolution -Connection $connection -UniqueName $testSolutionName -Confirm:$false -ErrorAction SilentlyContinue
    if (Test-Path $tempDir) { Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue }
}
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("SUCCESS", because: result.GetFullOutput());
        }
    }
}
