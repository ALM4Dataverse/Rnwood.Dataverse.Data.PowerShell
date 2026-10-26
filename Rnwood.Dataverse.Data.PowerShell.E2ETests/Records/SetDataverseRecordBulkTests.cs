using AwesomeAssertions;
using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Records
{
    public class SetDataverseRecordBulkTests : E2ETestBase
    {
        [Fact]
        public void BulkCreateAndUpdateAndLegacyExecuteMultiple_WorkAgainstDataverse()
        {
            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'
$testRunId = [guid]::NewGuid().ToString('N')
$recordIds = [System.Collections.Generic.List[guid]]::new()

try {
    $bulkId1 = [guid]::NewGuid()
    $bulkId2 = [guid]::NewGuid()
    $recordIds.Add($bulkId1)
    $recordIds.Add($bulkId2)
    $records = @(
        @{ Id = $bulkId1; firstname = ""Bulk-$testRunId-1""; lastname = 'Set test'; statuscode = 2 }
        @{ Id = $bulkId2; firstname = ""Bulk-$testRunId-2""; lastname = 'Set test' }
    )

    $created = @($records | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru -BatchSize 10)
    if ($created.Count -ne 2) {
        throw ""Expected two bulk-created records, got $($created.Count).""
    }
    $updates = @(
        @{ Id = $created[0].Id; firstname = ""Updated-$testRunId-1"" }
        @{ Id = $created[1].Id; firstname = ""Updated-$testRunId-2"" }
    )
    $updates | Set-DataverseRecord -Connection $connection -TableName contact -UpdateAllColumns -BatchSize 10

    foreach ($record in $created) {
        $updated = Get-DataverseRecord -Connection $connection -TableName contact -Id $record.Id
        if ($updated.firstname -notlike ""Updated-$testRunId-*"") {
            throw ""Bulk update did not persist for record $($record.Id).""
        }
    }

    $legacyId = [guid]::NewGuid()
    $recordIds.Add($legacyId)
    $legacy = @{ Id = $legacyId; firstname = ""Legacy-$testRunId""; lastname = 'Set test' } |
        Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru -BatchSize 10 -UseExecuteMultiple
    if (-not $legacy.Id) {
        throw 'Legacy ExecuteMultiple mode did not return a created record ID.'
    }
    if ([guid]$legacy.Id -ne $legacyId) {
        throw 'Legacy ExecuteMultiple mode returned an unexpected record ID.'
    }

    Write-Host 'SUCCESS: bulk create/update and legacy ExecuteMultiple mode completed'
}
finally {
    if ($recordIds.Count -gt 0) {
        foreach ($recordId in $recordIds) {
            Remove-DataverseRecord -Connection $connection -TableName contact -Id $recordId -Confirm:$false -ErrorAction Continue
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
