using AwesomeAssertions;
using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Records
{
    [Collection(SchemaChangesCollection.Name)]
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
            result.StandardOutput.Should().Contain("SUCCESS: bulk create/update", because: result.GetFullOutput());
        }

        [Fact]
        public void BulkUpsertUsesRealUpsertMultipleResponseCollection()
        {
            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
$timestamp = [DateTime]::UtcNow.ToString('yyyyMMddHHmm')
$entityName = ""new_bulkupsert_${timestamp}_$testRunId""
$entityCreated = $false
$recordIds = [System.Collections.Generic.List[guid]]::new()

try {
    Invoke-WithRetry {
        Set-DataverseEntityMetadata -Connection $connection `
            -EntityName $entityName `
            -SchemaName ""new_BulkUpsert_${timestamp}_$testRunId"" `
            -DisplayName 'Bulk Upsert Test' `
            -DisplayCollectionName 'Bulk Upsert Tests' `
            -PrimaryAttributeSchemaName 'new_name' `
            -OwnershipType UserOwned `
            -Confirm:$false
    }
    $entityCreated = $true

    $capabilityQuery = [Microsoft.Xrm.Sdk.Query.QueryExpression]::new('sdkmessagefilter')
    $capabilityQuery.ColumnSet = [Microsoft.Xrm.Sdk.Query.ColumnSet]::new('sdkmessagefilterid')
    $capabilityQuery.TopCount = 1
    $capabilityQuery.Criteria.AddCondition(
        'primaryobjecttypecode',
        [Microsoft.Xrm.Sdk.Query.ConditionOperator]::Equal,
        $entityName)
    $messageLink = $capabilityQuery.AddLink('sdkmessage', 'sdkmessageid', 'sdkmessageid')
    $messageLink.LinkCriteria.AddCondition(
        'name',
        [Microsoft.Xrm.Sdk.Query.ConditionOperator]::Equal,
        'UpsertMultiple')
    if ($connection.RetrieveMultiple($capabilityQuery).Entities.Count -eq 0) {
        throw ""The created custom table '$entityName' does not expose UpsertMultiple.""
    }

    $id1 = [guid]::NewGuid()
    $id2 = [guid]::NewGuid()
    $recordIds.Add($id1)
    $recordIds.Add($id2)
    $inputs = @(
        @{ Id = $id1; new_name = ""Upsert-$testRunId-1"" }
        @{ Id = $id2; new_name = ""Upsert-$testRunId-2"" }
    )
    $upserted = @($inputs | Set-DataverseRecord -Connection $connection -TableName $entityName -Upsert -PassThru -BatchSize 10)
    if ($upserted.Count -ne 2 -or [guid]$upserted[0].Id -ne $id1 -or [guid]$upserted[1].Id -ne $id2) {
        throw 'UpsertMultiple did not return the expected response IDs.'
    }

    $updates = @(
        @{ Id = $id1; new_name = ""UpdatedUpsert-$testRunId-1"" }
        @{ Id = $id2; new_name = ""UpdatedUpsert-$testRunId-2"" }
    )
    $upsertedUpdates = @($updates | Set-DataverseRecord -Connection $connection -TableName $entityName -Upsert -PassThru -BatchSize 10)
    if ($upsertedUpdates.Count -ne 2) {
        throw 'UpsertMultiple did not return one response per updated record.'
    }
    foreach ($record in $upsertedUpdates) {
        $stored = Get-DataverseRecord -Connection $connection -TableName $entityName -Id $record.Id
        if ($stored.new_name -notlike ""UpdatedUpsert-$testRunId-*"") {
            throw ""UpsertMultiple did not update record $($record.Id).""
        }
    }

    Write-Host 'SUCCESS: UpsertMultiple response collection processed'
}
finally {
    if ($entityCreated) {
        foreach ($recordId in $recordIds) {
            Remove-DataverseRecord -Connection $connection -TableName $entityName -Id $recordId -Confirm:$false -ErrorAction Continue
        }
        Invoke-WithRetry {
            Remove-DataverseEntityMetadata -Connection $connection -EntityName $entityName -Confirm:$false
        }
    }
}
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("SUCCESS: UpsertMultiple response collection processed", because: result.GetFullOutput());
        }
    }
}
