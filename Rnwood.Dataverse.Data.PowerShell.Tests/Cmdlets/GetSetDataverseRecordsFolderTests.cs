using Xunit;
using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using AwesomeAssertions;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets;

/// <summary>
/// Tests for Get-DataverseRecordsFolder and Set-DataverseRecordsFolder script module cmdlets.
/// These are PowerShell script modules (.psm1), so tests must execute in a child PowerShell process.
/// </summary>
public class GetSetDataverseRecordsFolderTests : TestBase, IDisposable
{
    private string? _tempFolder;
    
    private string CreateTempFolder()
    {
        _tempFolder = Path.Combine(Path.GetTempPath(), $"DataverseRecordsFolderTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempFolder);
        return _tempFolder;
    }

    public void Dispose()
    {
        if (_tempFolder != null && Directory.Exists(_tempFolder))
        {
            try { Directory.Delete(_tempFolder, true); } catch { /* Ignore cleanup errors */ }
        }
    }

    [Fact]
    public void DataverseRecordsFolder_RoundTripSerialization_WritesAndReadsCorrectly()
    {
        // Arrange
        var folder = CreateTempFolder();
        var script = $@"
$record = [PSCustomObject]@{{
    Id = [Guid]'00000000-0000-0000-0000-000000000001'
    TableName = 'contact'
    firstname = 'John'
    lastname = 'Doe'
}}

# Write record to folder
$record | Set-DataverseRecordsFolder -OutputPath '{folder.Replace("'", "''")}'

# Read records back
$results = @(Get-DataverseRecordsFolder -InputPath '{folder.Replace("'", "''")}')

if ($results.Count -ne 1) {{ throw 'Expected 1 record, got ' + $results.Count }}
if ($results[0].firstname -ne 'John') {{ throw 'firstname mismatch: ' + $results[0].firstname }}
if ($results[0].lastname -ne 'Doe') {{ throw 'lastname mismatch: ' + $results[0].lastname }}

Write-Output 'PASS'
";

        // Act
        var result = PowerShellProcessRunner.Run(script);
        
        // Assert
        result.Success.Should().BeTrue($"Script failed: {result.GetFullOutput()}");
    }

    [Fact]
    public void DataverseRecordsFolder_CreatesOneJsonFilePerRecord()
    {
        // Arrange
        var folder = CreateTempFolder();
        var script = $@"
$records = @(
    [PSCustomObject]@{{ Id = [Guid]'00000000-0000-0000-0000-000000000001'; TableName = 'contact'; name = 'Record1' }},
    [PSCustomObject]@{{ Id = [Guid]'00000000-0000-0000-0000-000000000002'; TableName = 'contact'; name = 'Record2' }},
    [PSCustomObject]@{{ Id = [Guid]'00000000-0000-0000-0000-000000000003'; TableName = 'contact'; name = 'Record3' }}
)

$records | Set-DataverseRecordsFolder -OutputPath '{folder.Replace("'", "''")}'

# Count JSON files
$count = (Get-ChildItem -Path '{folder.Replace("'", "''")}' -Filter '*.json').Count
if ($count -ne 3) {{ throw 'Expected 3 files, got ' + $count }}
Write-Output 'PASS'
";

        // Act
        var result = PowerShellProcessRunner.Run(script);
        
        // Assert
        result.Success.Should().BeTrue($"Script failed: {result.GetFullOutput()}");
    }

    [Fact]
    public void DataverseRecordsFolder_HandlesEmptyFolderGracefully()
    {
        // Arrange
        var folder = CreateTempFolder();
        var script = $@"
$results = @(Get-DataverseRecordsFolder -InputPath '{folder.Replace("'", "''")}')
$count = $results.Count
if ($count -ne 0) {{ throw 'Expected 0 records, got ' + $count }}
Write-Output 'PASS'
";

        // Act
        var result = PowerShellProcessRunner.Run(script);
        
        // Assert
        result.Success.Should().BeTrue($"Script failed: {result.GetFullOutput()}");
    }

    [Fact]
    public void DataverseRecordsFolder_PreservesComplexDataTypes()
    {
        // Arrange
        var folder = CreateTempFolder();
        var script = $@"
$record = [PSCustomObject]@{{
    Id = [Guid]'00000000-0000-0000-0000-000000000001'
    TableName = 'contact'
    birthdate = '1990-05-15T00:00:00'
    revenue = 12345.67
    isactive = $true
        primarycontactid = [PSCustomObject]@{{ Id = [Guid]'00000000-0000-0000-0000-000000000099'; LogicalName = 'account' }}
}}

$record | Set-DataverseRecordsFolder -OutputPath '{folder.Replace("'", "''")}'
$results = @(Get-DataverseRecordsFolder -InputPath '{folder.Replace("'", "''")}')

if ($results.Count -ne 1) {{ throw 'Expected 1 record, got ' + $results.Count }}
$birthdate = [datetime]$results[0].birthdate
if ($birthdate.ToString('yyyy-MM-dd') -ne '1990-05-15') {{ throw 'birthdate mismatch: ' + $birthdate.ToString('o') }}
if ([decimal]$results[0].revenue -ne 12345.67) {{ throw 'revenue mismatch: ' + $results[0].revenue }}
if ($results[0].isactive -ne $true) {{ throw 'isactive mismatch: ' + $results[0].isactive }}
if ($results[0].primarycontactid.Id -ne [Guid]'00000000-0000-0000-0000-000000000099') {{ throw 'lookup Id mismatch' }}

Write-Output 'PASS'
";

        // Act
        var result = PowerShellProcessRunner.Run(script);
        
        // Assert
        result.Success.Should().BeTrue($"Script failed: {result.GetFullOutput()}");
    }

    [Fact]
    public void DataverseRecordsFolder_SyncsBehavior_RemovesFilesNotInCurrentBatch()
    {
        // Arrange - Set-DataverseRecordsFolder syncs the folder to match the current batch,
        // removing files that aren't in the current batch (unless -withdeletions is used)
        var folder = CreateTempFolder();
        var script = $@"
# First write - creates file for record 1
[PSCustomObject]@{{ Id = [Guid]'00000000-0000-0000-0000-000000000001'; TableName = 'contact'; name = 'First' }} | 
    Set-DataverseRecordsFolder -OutputPath '{folder.Replace("'", "''")}'

$afterFirst = (Get-ChildItem -Path '{folder.Replace("'", "''")}' -Filter '*.json').Count

# Second write with different record - removes record 1, creates record 2
[PSCustomObject]@{{ Id = [Guid]'00000000-0000-0000-0000-000000000002'; TableName = 'contact'; name = 'Second' }} | 
    Set-DataverseRecordsFolder -OutputPath '{folder.Replace("'", "''")}'

$afterSecond = (Get-ChildItem -Path '{folder.Replace("'", "''")}' -Filter '*.json').Count

# Verify: first call creates 1 file, second call also results in 1 file (previous was removed)
if ($afterFirst -ne 1) {{ throw 'Expected 1 file after first write, got ' + $afterFirst }}
if ($afterSecond -ne 1) {{ throw 'Expected 1 file after second write, got ' + $afterSecond }}
Write-Output 'PASS'
";

        // Act
        var result = PowerShellProcessRunner.Run(script);
        
        // Assert
        result.Success.Should().BeTrue($"Script failed: {result.GetFullOutput()}");
    }

    [Fact]
    public void DataverseRecordsFolder_HandlesMultipleTableTypes()
    {
        // Arrange
        var folder = CreateTempFolder();
        var script = $@"
$records = @(
    [PSCustomObject]@{{ Id = [Guid]'00000000-0000-0000-0000-000000000001'; TableName = 'contact'; name = 'Contact1' }},
    [PSCustomObject]@{{ Id = [Guid]'00000000-0000-0000-0000-000000000002'; TableName = 'account'; name = 'Account1' }}
)

$records | Set-DataverseRecordsFolder -OutputPath '{folder.Replace("'", "''")}'

# Read all records
$all = @(Get-DataverseRecordsFolder -InputPath '{folder.Replace("'", "''")}')
$contacts = @($all | Where-Object {{ $_.TableName -eq 'contact' }})
$accounts = @($all | Where-Object {{ $_.TableName -eq 'account' }})

if ($contacts.Count -ne 1) {{ throw 'Expected 1 contact, got ' + $contacts.Count }}
if ($accounts.Count -ne 1) {{ throw 'Expected 1 account, got ' + $accounts.Count }}
Write-Output 'PASS'
";

        // Act
        var result = PowerShellProcessRunner.Run(script);
        
        // Assert
        result.Success.Should().BeTrue($"Script failed: {result.GetFullOutput()}");
    }

    [Fact]
    public void DataverseRecordsFolder_UsesRecordIdInFilename()
    {
        // Arrange
        var folder = CreateTempFolder();
        var testId = "11111111-1111-1111-1111-111111111111";
        var script = $@"
[PSCustomObject]@{{ Id = [Guid]'{testId}'; TableName = 'contact'; name = 'Test' }} | 
    Set-DataverseRecordsFolder -OutputPath '{folder.Replace("'", "''")}'

# Check filename contains ID
$files = Get-ChildItem -Path '{folder.Replace("'", "''")}' -Filter '*.json'
if (-not ($files.Name -match '{testId}')) {{ throw 'Expected filename to contain id {testId}' }}
Write-Output 'PASS'
";

        // Act
        var result = PowerShellProcessRunner.Run(script);
        
        // Assert
        result.Success.Should().BeTrue($"Script failed: {result.GetFullOutput()}");
    }

    [Fact]
    public void DataverseRecordsFolder_PreservesRecordMetadata()
    {
        // Arrange
        var folder = CreateTempFolder();
        var script = $@"
$record = [PSCustomObject]@{{
    Id = [Guid]'00000000-0000-0000-0000-000000000001'
    TableName = 'contact'
    customfield = 'value'
}}

$record | Set-DataverseRecordsFolder -OutputPath '{folder.Replace("'", "''")}'
$results = @(Get-DataverseRecordsFolder -InputPath '{folder.Replace("'", "''")}')

# Check metadata is preserved
if ($results.Count -ne 1) {{ throw 'Expected 1 record, got ' + $results.Count }}
if ($results[0].Id -ne [Guid]'00000000-0000-0000-0000-000000000001') {{ throw 'Id mismatch: ' + $results[0].Id }}
if ($results[0].TableName -ne 'contact') {{ throw 'TableName mismatch: ' + $results[0].TableName }}
Write-Output 'PASS'
";

        // Act
        var result = PowerShellProcessRunner.Run(script);
        
        // Assert
        result.Success.Should().BeTrue($"Script failed: {result.GetFullOutput()}");
    }
}
