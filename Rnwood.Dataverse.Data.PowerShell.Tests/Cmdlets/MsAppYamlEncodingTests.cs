using AwesomeAssertions;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets;

public class MsAppYamlEncodingTests : TestBase
{
    [Fact]
    public void SetDataverseMsAppScreen_WritesYamlWithoutUtf8Bom()
    {
        var script = """
$tmp = Join-Path ([System.IO.Path]::GetTempPath()) ("msapp-encoding-test-" + [guid]::NewGuid().ToString() + ".msapp")

try {
    $msapp = New-DataverseMsApp -Path $tmp -Force

    $yaml = @"
Properties:
    LoadingSpinnerColor: =RGBA(56, 96, 178, 1)
Children:
    - Label2:
        Control: Label@2.5.1
        Properties:
            BorderColor: =RGBA(0, 0, 0, 0)
            BorderStyle: =BorderStyle.None
            BorderThickness: =2
            Color: =RGBA(50, 49, 48, 1)
            DisabledBorderColor: =RGBA(0, 0, 0, 0)
            DisabledColor: =RGBA(161, 159, 157, 1)
            FocusedBorderThickness: =4
            Font: =Font.'Segoe UI'
            Text: =`"Screen 2`"
            X: =327
            Y: =223
"@

    Set-DataverseMsAppScreen -MsAppPath $msapp.FullName -ScreenName 'Screen2' -YamlContent $yaml -Confirm:$false | Out-Null

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::OpenRead($msapp.FullName)
    try {
        $entry = $zip.Entries | Where-Object { $_.FullName -eq 'Src/Screen2.pa.yaml' } | Select-Object -First 1
        if ($null -eq $entry) { throw 'Screen2 YAML entry not found' }

        $stream = $entry.Open()
        try {
            $bytes = New-Object byte[] 3
            $read = $stream.Read($bytes, 0, 3)
            if ($read -lt 3) { throw 'Screen2 YAML too short to validate BOM' }

            $hasBom = ($bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF)
            if ($hasBom) { throw 'Screen2 YAML was written with UTF-8 BOM' }
        }
        finally {
            $stream.Dispose()
        }
    }
    finally {
        $zip.Dispose()
    }

    # Ensure Get cmdlet can parse the updated screen
    $screen = Get-DataverseMsAppScreen -MsAppPath $msapp.FullName -ScreenName 'Screen2'
    if ($null -eq $screen -or [string]::IsNullOrWhiteSpace($screen.YamlContent)) {
        throw 'Get-DataverseMsAppScreen failed to parse Screen2 YAML content'
    }

    Write-Output 'PASS'
}
finally {
    if (Test-Path $tmp) {
        Remove-Item $tmp -Force
    }
}
""";

        var result = PowerShellProcessRunner.Run(script);

        result.Success.Should().BeTrue($"Script failed: {result.GetFullOutput()}");
    }
}
