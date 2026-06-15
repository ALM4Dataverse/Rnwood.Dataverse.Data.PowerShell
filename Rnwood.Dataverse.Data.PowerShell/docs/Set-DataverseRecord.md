---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseRecord

## SYNOPSIS
Creates or updates Dataverse records including M:M association/disassociation, status and assignment changes.

## SYNTAX

```
Set-DataverseRecord -InputObject <PSObject> -TableName <String> [-BatchSize <UInt32>]
 [-RetrievalBatchSize <UInt32>] [-IgnoreProperties <String[]>] [-Id <Guid>] [-MatchOn <String[][]>]
 [-AllowMultipleMatches] [-PassThru] [-NoUpdate] [-NoCreate] [-NoUpdateColumns <String[]>] [-CallerId <Guid>]
 [-UpdateAllColumns] [-CreateOnly] [-Upsert] [-LookupColumns <Hashtable>]
 [-BypassBusinessLogicExecution <BusinessLogicTypes[]>] [-BypassBusinessLogicExecutionStepIds <Guid[]>]
 [-Retries <Int32>] [-InitialRetryDelay <Int32>] [-MaxDegreeOfParallelism <Int32>] [-EnableDuplicateDetection]
 [-FileDirectory <String>] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf]
 [-Confirm] [<CommonParameters>]
```

## DESCRIPTION

By default, this command will check for existing record matching the input object's primary key(s) and will update if there is a match and there are changes, or create if not. 

The `TableName` and `Id` properties will normally be read from the pipeine if present, but can be overriden as separate arguments.

See the various options below which can vary if/how the record is matched and if new records will be created or not.

**Data Type Conversion:**

Primitive types (text, number, yes/no) can be specified directly using the PowerShell numeric/string/bool types etc. A conversion will be attempted from other types.

Date and date/time columns can be specified directly using the PowerShell DateTime type. A conversion will be attempted from string and other types, but take care to include TZ offset.

Choice (option set), State and Status columns will accept any or:
- Numeric value of the choice
- Label
- $null or empty string

Lookup and UniqueIdentifier columns will accept any of:
 - the name of the target record (as long as it is unique)
 - the Id of a record in one of the tables the lookup targets
 - an object which has `TableName`/`LogicalName`/`EntityName` and `Id` properties. This includes `EntityReference` instances from the SDK and values as returned from `Get-DataverseRecord`.
 - $null or empty string.

Party list columns accept a collection of objects each of which need to be convertible to a `activityparty` table record using the rules above.

**Batch Operations:**

When multiple records are passed through the pipeline or provided in a single invocation, this cmdlet automatically uses `ExecuteMultipleRequest` to batch operations for improved performance. 

Key Batch Behavior:
- Default batch size is 100 records per request (configurable via `-BatchSize` parameter)
- Set `-BatchSize 1` to disable batching and send one request at a time
- Batching continues on error - all operations are attempted and errors are reported at the end
- Each error includes the input record for correlation
- For best performance with `-CallerId`, sort records by caller ID since a new batch is needed when the caller changes

**Retry Logic:**

Individual batch items can be automatically retried on failure using exponential backoff:
- Use `-Retries` parameter to specify the number of retry attempts (default is 0 - no retries)
- Use `-InitialRetryDelay` to set the initial delay in seconds before the first retry (default is 5s)
- Each subsequent retry doubles the delay time (exponential backoff)
- Records are added to the next batch after the delay period has elapsed
- If only retries are pending at the end of processing, the cmdlet will wait for the appropriate delay before retrying
- After all retries are exhausted, failures are written as errors as usual
- Retries are particularly useful for transient errors like throttling or temporary connectivity issues

**Parallel Processing:**

For maximum throughput when processing large numbers of records, you can enable parallel processing:
- Use `-MaxDegreeOfParallelism` parameter to specify the number of parallel worker threads (default is 1 - parallel processing disabled)
- Each worker processes records concurrently using its own cloned connection to Dataverse
- Parallel processing can be combined with batching for even better performance
- When set to a value greater than 1, records are distributed across multiple workers
- Each worker maintains its own batch queue when `-BatchSize` > 1
- Progress is tracked and reported across all workers
- All output types (Verbose, Error, Progress, PassThru) work correctly in parallel mode
- Note: With mock connections that don't support cloning, workers share the same connection (may have reduced parallelism)

See Example 20 and Example 21 in the EXAMPLES section for usage demonstrations.

**How Create vs Update is Determined:**

The cmdlet follows this decision logic for each record:

1. **With `-Upsert` switch:**
   - Uses `UpsertRequest` which lets Dataverse decide based on alternate keys
   - No check for existing record is performed
   - Dataverse creates if no match, updates if match found
   - `-MatchOn` must specify an alternate key defined on the table

2. **With `-CreateOnly` switch:**
   - Always attempts to create new records
   - No check for existing record is performed
   - Use when you know records don't exist (best performance)

3. **With `-NoUpdate` switch:**
   - Checks for existing records but will not update them
   - Only creates new records when no match found

4. **With `-NoCreate` switch:**
   - Checks for existing records but will not create them
   - Only updates records when match found

5. **Default behavior (no switches):**
   - Checks for existing record by:
     - Primary ID if provided via `-Id` parameter or `Id` property
     - `-MatchOn` columns if specified (tries each match set in order)
   - If existing record found:
     - Retrieves current values
     - Removes unchanged columns (unless `-UpdateAllColumns` specified)
     - Sends `UpdateRequest` if changes detected
     - Skips update if nothing changed
   - If no existing record found:
     - Sends `CreateRequest` for new record

After create/update, additional operations are batched if needed:
- Assignment (`AssignRequest`) if `ownerid` column is set
- Status change (`SetStateRequest`) if `statecode` or `statuscode` columns are set

**Assigning records**

If the input object contains an `ownerid` property the cmdlet will perform an assignment after the main create/update. `ownerid` accepts the same forms as other lookup columns: a GUID, a PSObject with `Id` and `TableName`/`LogicalName` properties (or an `EntityReference`), or a name string which the cmdlet will try to resolve. Assignments are executed with `AssignRequest` and will be batched when `-BatchSize` &gt; 1.

**Setting state/status**

To change a record's state/status include `statuscode` (and optionally `statecode`) on the input object. `statuscode` and `statecode` accept either the numeric value or the display label. If only `statuscode` is provided the cmdlet will infer the matching `statecode` from the table metadata and then issue a `SetStateRequest` after the main create/update. State/status requests are executed separately and are batched when `-BatchSize` &gt; 1.

## EXAMPLES

### Example 1: Create a single record
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> [PSCustomObject] @{
	TableName = "contact"
	lastname = "Simpson"
	firstname = "Homer"
} | Set-DataverseRecord
```

Creates a new contact record with a last name and first name.

### Example 2: Update existing records
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseRecord -TableName contact -Columns statuscode | 
    ForEach-Object { $_.statuscode = "Inactive" } | 
    Set-DataverseRecord
```

Retrieves all existing contacts and sets their status reason to `Inactive`.

### Example 3: Batch create multiple records
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $contacts = @(
    @{ firstname = "John"; lastname = "Doe"; emailaddress1 = "john@example.com" }
    @{ firstname = "Jane"; lastname = "Smith"; emailaddress1 = "jane@example.com" }
    @{ firstname = "Bob"; lastname = "Johnson"; emailaddress1 = "bob@example.com" }
)

PS C:\> $contacts | Set-DataverseRecord -TableName contact -CreateOnly
```

Creates multiple contact records in a single batch. The `-CreateOnly` switch improves performance by skipping the existence check since we know these are new records. By default, all 3 records will be sent in a single ExecuteMultipleRequest.

### Example 4: Batch update with choice columns
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $accounts = Get-DataverseRecord -TableName account -Top 50

PS C:\> $accounts | ForEach-Object { 
    $_.industrycode = "Retail"  # Can use label name
    $_.accountratingcode = 1     # Or numeric value
} | Set-DataverseRecord
```

Retrieves 50 accounts and updates their industry (using label) and rating (using numeric value). Choice/option set columns accept either the numeric value or the display label.

### Example 5: Batch operations with state/status changes
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $cases = Get-DataverseRecord -TableName incident -FilterValues @{ statecode = 0 }

PS C:\> $cases | ForEach-Object {
    $_.statuscode = "Resolved"  # Can use status label
    $_.statecode = 1             # Or numeric state value
} | Set-DataverseRecord
```

Closes all active cases by setting their state and status. The cmdlet automatically handles state/status changes as separate SetStateRequest operations after the main update.

### Example 6: Upsert with alternate key
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> @(
    @{ emailaddress1 = "user1@example.com"; firstname = "Alice"; lastname = "Anderson" }
    @{ emailaddress1 = "user2@example.com"; firstname = "Bob"; lastname = "Brown" }
) | Set-DataverseRecord -TableName contact -Upsert -MatchOn emailaddress1
```

Uses upsert operation with alternate key on emailaddress1. If a contact with the email exists, it will be updated; otherwise a new one is created. Requires that `emailaddress1` is defined as an alternate key on the contact table.

### Example 7: Control batch size
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $records = 1..500 | ForEach-Object {
    @{ name = "Account $_"; telephone1 = "555-$_" }
}

PS C:\> $records | Set-DataverseRecord -TableName account -BatchSize 50 -CreateOnly
```

Creates 500 records in batches of 50. This will result in 10 ExecuteMultipleRequest calls, each containing 50 CreateRequest operations.

### Example 8: Disable batching for detailed error handling
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $records | Set-DataverseRecord -TableName contact -BatchSize 1
```

Disables batching by setting BatchSize to 1. Each record is sent in a separate request, which stops immediately on first error rather than continuing through the batch.

### Example 9: MatchOn with multiple columns
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> @(
    @{ firstname = "John"; lastname = "Doe"; telephone1 = "555-0001" }
    @{ firstname = "Jane"; lastname = "Smith"; telephone1 = "555-0002" }
) | Set-DataverseRecord -TableName contact -MatchOn ("firstname", "lastname")
```

Looks for existing contacts matching BOTH firstname AND lastname. If found, updates them; otherwise creates new records.

### Example 10: Assignment with batching
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $newOwnerId = "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"

PS C:\> Get-DataverseRecord -TableName account -Top 100 | 
    ForEach-Object { $_.ownerid = $newOwnerId } | 
    Set-DataverseRecord
```

Reassigns 100 accounts to a new owner. The cmdlet handles this as update operations followed by AssignRequest operations, all batched together.

### Example 11: Using lookup columns with names
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> @{
    firstname = "John"
    lastname = "Doe"
    parentcustomerid = "Contoso Ltd"  # Lookup by account name
} | Set-DataverseRecord -TableName contact
```

Creates a contact with a lookup to an account by name. The module will automatically resolve "Contoso Ltd" to the account's GUID if the name is unique.

### Example 12: Handle errors in batch operations
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $records = @(
    @{ firstname = "Alice"; lastname = "Valid"; emailaddress1 = "alice@example.com" }
    @{ firstname = "Bob"; lastname = ""; emailaddress1 = "bob@example.com" }  # Invalid - required field missing
    @{ firstname = "Charlie"; lastname = "Valid"; emailaddress1 = "charlie@example.com" }
)

PS C:\> $errors = @()
PS C:\> $records | Set-DataverseRecord -TableName contact -CreateOnly -ErrorVariable +errors -ErrorAction SilentlyContinue

PS C:\> # Process errors - each error's TargetObject contains the input record that failed
PS C:\> foreach ($err in $errors) {
    Write-Host "Failed to create contact: $($err.TargetObject.firstname) $($err.TargetObject.lastname)"
    Write-Host "Error: $($err.Exception.Message)"
}
```

Demonstrates batch error handling. When using batching (default BatchSize of 100), the cmdlet continues processing all records even if some fail. Each error written to the error stream includes the original input object as the `TargetObject`, allowing you to correlate which input caused the error. Use `-ErrorVariable` to collect errors and `-ErrorAction SilentlyContinue` to prevent them from stopping execution.

### Example 13: Access full error details from server
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $records = @(
    @{ firstname = "Test"; lastname = "User" }
)

PS C:\> $errors = @()
PS C:\> $records | Set-DataverseRecord -TableName contact -CreateOnly -ErrorVariable +errors -ErrorAction SilentlyContinue

PS C:\> # Access detailed error information from the server
PS C:\> foreach ($err in $errors) {
    # TargetObject contains the input record that failed
    Write-Host "Failed record: $($err.TargetObject.firstname) $($err.TargetObject.lastname)"
    
    # Exception.Message contains full server response including:
    # - OrganizationServiceFault ErrorCode and Message
    # - TraceText with server-side trace details
    # - InnerFault details (if any)
    Write-Host "Full error details from server:"
    Write-Host $err.Exception.Message
    
    # You can also access individual components:
    Write-Host "Error category: $($err.CategoryInfo.Category)"
    Write-Host "Exception type: $($err.Exception.GetType().Name)"
}
```

Demonstrates how to access comprehensive error details from the Dataverse server. The `Exception.Message` property contains the full server response formatted by the cmdlet, including the OrganizationServiceFault error code, message, server-side trace text, and any nested inner fault details. The `TargetObject` identifies which input record caused the error, while the full error message provides all diagnostic information needed for troubleshooting server-side issues.

### Example 14: Stop on first error with BatchSize 1
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $records = @(
    @{ firstname = "Alice"; lastname = "Valid" }
    @{ firstname = "Bob"; lastname = "" }  # Invalid - will cause error
    @{ firstname = "Charlie"; lastname = "Valid" }  # Won't be processed
)

PS C:\> try {
    $records | Set-DataverseRecord -TableName contact -CreateOnly -BatchSize 1 -ErrorAction Stop
} catch {
    Write-Host "Error creating record: $($_.TargetObject.firstname)"
    Write-Host "Remaining records were not processed"
}
```

Disables batching by setting `-BatchSize 1`. With this setting, each record is sent in a separate request, and execution stops immediately on the first error (when using `-ErrorAction Stop`). This is useful when you need to stop processing on the first failure rather than attempting all records.

### Example 15: Get IDs of created records using PassThru
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $contacts = @(
    @{ firstname = "Alice"; lastname = "Anderson" }
    @{ firstname = "Bob"; lastname = "Brown" }
    @{ firstname = "Charlie"; lastname = "Clark" }
)

PS C:\> $createdRecords = $contacts | Set-DataverseRecord -TableName contact -CreateOnly -PassThru

PS C:\> # Access the IDs of created records
PS C:\> foreach ($record in $createdRecords) {
    Write-Host "Created contact $($record.firstname) $($record.lastname) with ID: $($record.Id)"
}
```

Creates multiple contact records and returns the input objects with their `Id` property set. The `-PassThru` parameter causes the cmdlet to write each input object to the pipeline after the record is created, with the `Id` property populated with the GUID of the newly created record. This allows you to capture and use the IDs in subsequent operations.

### Example 16: Use PassThru to create and link records
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Create parent account and get its ID
PS C:\> $account = @{ name = "Contoso Ltd" } | 
    Set-DataverseRecord -TableName account -CreateOnly -PassThru

PS C:\> # Create child contact linked to the account
PS C:\> $contact = @{ 
    firstname = "John"
    lastname = "Doe"
    parentcustomerid = $account.Id
} | Set-DataverseRecord -TableName contact -CreateOnly -PassThru

PS C:\> Write-Host "Created contact $($contact.Id) linked to account $($account.Id)"
```

Demonstrates using `-PassThru` to chain record creation operations. First creates an account and captures its ID using `-PassThru`, then uses that ID to create a related contact record. This pattern is useful when you need to establish relationships between newly created records.

### Example 17: Associate records in many-to-many relationships
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Create two accounts to associate
PS C:\> $account1 = @{ name = "Contoso Ltd" } | Set-DataverseRecord -TableName account -CreateOnly -PassThru
PS C:\> $account2 = @{ name = "Fabrikam Inc" } | Set-DataverseRecord -TableName account -CreateOnly -PassThru

PS C:\> # Associate the accounts using the intersect table
PS C:\> # The intersect table name is typically in the format: <entity1>_<entity2> or similar
PS C:\> # For account-to-account relationships, it might be "account_accounts" or similar
PS C:\> # Check your Dataverse metadata for the exact intersect table name and column names
PS C:\> @{
    # Entity 1 in the relationship - use the exact column name from the intersect table
    accountid = $account1.Id
    
    # Entity 2 in the relationship - use the exact column name from the intersect table
    accountid2 = $account2.Id
} | Set-DataverseRecord -TableName "account_accounts" -CreateOnly
```

Creates a many-to-many association between two account records. Many-to-many relationships in Dataverse are implemented using intersect tables that contain references to both related entities. The property names must match the exact column names in the intersect table (typically the primary key column names of the related entities). Use metadata queries or the Dataverse UI to find the correct table name and column names for your M:M relationship.

### Example 18: Batch associate multiple M:M relationships
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Create multiple contacts and marketing lists
PS C:\> $contacts = @(
    @{ firstname = "Alice"; lastname = "Smith" }
    @{ firstname = "Bob"; lastname = "Jones" }
    @{ firstname = "Carol"; lastname = "Williams" }
) | Set-DataverseRecord -TableName contact -CreateOnly -PassThru

PS C:\> $marketingList = @{ listname = "Q1 Newsletter Subscribers"; type = $false } | 
    Set-DataverseRecord -TableName list -CreateOnly -PassThru

PS C:\> # Associate all contacts with the marketing list using the intersect table
PS C:\> # Property names must match the exact column names in the intersect table
PS C:\> $associations = $contacts | ForEach-Object {
    @{
        contactid = $_.Id
        listid = $marketingList.Id
    }
}

PS C:\> $associations | Set-DataverseRecord -TableName "listcontact" -CreateOnly
```

Creates multiple many-to-many associations in a batch operation. This example associates multiple contacts with a marketing list using the `listcontact` intersect table. The property names (`contactid`, `listid`) must match the exact column names in the intersect table. The cmdlet automatically batches these operations for improved performance.

### Example 19: Using retry logic for transient failures
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $records = @(
    @{ firstname = "Alice"; lastname = "Smith" }
    @{ firstname = "Bob"; lastname = "Jones" }
    @{ firstname = "Carol"; lastname = "Williams" }
)

PS C:\> # Retry each failed record up to 3 times with exponential backoff
PS C:\> $records | Set-DataverseRecord -TableName contact -CreateOnly -Retries 3 -InitialRetryDelay 500 -Verbose
```

Creates multiple contacts with automatic retry on transient failures. Failed requests will be retried up to 3 times with delays of 500ms, 1000ms, and 2000ms respectively. The `-Verbose` flag shows retry attempts and wait times.

### Example 20: Process records in parallel for maximum throughput
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Create 10,000 records with 4 parallel workers and batches of 100
PS C:\> $records = 1..10000 | ForEach-Object {
    @{ 
        firstname = "User$_"
        lastname = "Parallel"
        emailaddress1 = "user$_@example.com" 
    }
}

PS C:\> $records | Set-DataverseRecord -TableName contact -CreateOnly -MaxDegreeOfParallelism 4 -BatchSize 100 -Verbose
```

Processes 10,000 records using 4 parallel worker threads. Each worker maintains its own batch of 100 records. This results in multiple ExecuteMultipleRequest operations running concurrently for maximum throughput. The `-Verbose` flag shows worker activity and progress across all threads.

### Example 21: Parallel updates with MatchOn
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Update records in parallel by email address
PS C:\> $updates = 1..1000 | ForEach-Object {
    @{ 
        emailaddress1 = "user$_@example.com"
        telephone1 = "555-$_"
    }
}

PS C:\> $updates | Set-DataverseRecord -TableName contact -MatchOn emailaddress1 -MaxDegreeOfParallelism 3 -BatchSize 50
```

Updates 1000 existing contacts in parallel using email address as the match key. Uses 3 workers with batch size of 50 for optimal performance.

### Example 22: Update multiple matching records with AllowMultipleMatches
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Update all contacts with a specific last name
PS C:\> @{ 
    lastname = "TestUser"
    emailaddress1 = "updated@example.com" 
} | Set-DataverseRecord -TableName contact -MatchOn lastname -AllowMultipleMatches
```

Updates ALL contacts with lastname "TestUser" by setting their email address. The -AllowMultipleMatches switch allows updating all matching records when multiple records match the MatchOn criteria. Without this switch, an error would be raised if multiple matches are found.

### Example 23: Import records with file attachments from Set-DataverseRecordsFolder
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Import records from a folder created by Set-DataverseRecordsFolder
PS C:\> # The folder structure should look like:
PS C:\> #   data/documents/
PS C:\> #     ??? record1.json  (contains documentbody = "abc123-...")
PS C:\> #     ??? record2.json
PS C:\> #     ??? files/
PS C:\> #         ??? abc123-...  (GUID folder)
PS C:\> #             ??? contract.pdf (the actual file)

PS C:\> Get-DataverseRecordsFolder -InputPath data/documents | 
    Set-DataverseRecord -TableName document -FileDirectory data/documents/files -MatchOn name
```

Imports records from a folder previously exported using `Set-DataverseRecordsFolder`. The `-FileDirectory` parameter points to the `files` subfolder where file attachments are stored. When the cmdlet detects that a file column value (GUID) is being set, it looks for a subfolder with that GUID name and uploads the file found there.

### Example 24: Create records with file uploads
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Prepare a file directory structure
PS C:\> $fileId = [Guid]::NewGuid()
PS C:\> New-Item -ItemType Directory -Path "temp/files/$fileId" -Force
PS C:\> Copy-Item "C:\Documents\report.pdf" "temp/files/$fileId/report.pdf"

PS C:\> # Create a record with a file column
PS C:\> @{
    name = "Monthly Report"
    new_document = $fileId  # File column set to the GUID
} | Set-DataverseRecord -TableName new_report -FileDirectory temp/files -CreateOnly
```

Creates a new record with a file attachment. The file column (`new_document`) is set to a GUID that corresponds to a subfolder in the FileDirectory. The cmdlet creates the record first, then uploads the file from the subfolder.

## PARAMETERS

### -AllowMultipleMatches
If specified, allows updating of ALL matching records when MatchOn criteria matches more than one record. Without this switch, an error is raised if MatchOn finds multiple matches.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -BatchSize
Controls the maximum number of requests sent to Dataverse in one batch (where possible) to improve throughput. Specify 1 to disable.

When value is 1, requests are sent to Dataverse one request at a time. When > 1, batching is used. 

Note that the batch will continue on error and any errors will be returned once the batch has completed. The error contains the input record to allow correlation.

```yaml
Type: UInt32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BypassBusinessLogicExecution
Specifies the types of business logic (for example plugins) to bypass

```yaml
Type: BusinessLogicTypes[]
Parameter Sets: (All)
Aliases:
Accepted values: CustomSync, CustomAsync

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BypassBusinessLogicExecutionStepIds
Specifies the IDs of plugin steps to bypass

```yaml
Type: Guid[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CallerId
If specified, the creation/updates will be done on behalf of the user with the specified ID. For best performance, sort the records using this value since a new batch request is needed each time this value changes.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet,

```yaml
Type: ServiceClient
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CreateOnly
If specified, no check for existing record is made and records will always be attempted to be created.
Use this option when it's known that no existing matching records will exist to improve performance.
See the -noupdate option for an alternative.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -EnableDuplicateDetection
If specified, duplicate detection will be enabled by setting SuppressDuplicateDetection to false in create, update, and upsert requests.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FileDirectory
Path to a directory containing file attachments to upload. When a file column value (GUID) changes, the cmdlet looks for a subfolder with the matching GUID inside this directory and uploads the single file found there.

This parameter is designed to work with folders created by `Set-DataverseRecordsFolder`. When records with file columns are exported using that cmdlet, files are saved to a `files` subfolder with each file in its own GUID-named subfolder. When importing records back using this parameter, the cmdlet automatically uploads files when their GUID values change.

**Requirements:**
- The FileDirectory must contain subfolders named with the file ID (GUID)
- Each subfolder must contain exactly one file
- The file column value in the input object must be a valid GUID

**Behavior:**
- On create: If a file column has a non-empty GUID, the file is uploaded after record creation
- On update: If the file column GUID has changed, the file is uploaded after record update
- File uploads are executed after the main record operation completes

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Id
ID of record to be created or updated.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -IgnoreProperties
List of properties on the input object which are ignored and not attemted to be mapped to the record.
Default is none.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InitialRetryDelay
Initial delay in seconds before first retry. Subsequent retries use exponential backoff (delay doubles each time). Default is 5s.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 5
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputObject
Object containing values to be used.
Property names must match the logical names of Dataverse fields in the specified entity and the property values are used to set the values of the Dataverse record being created/updated.
The properties may include ownerid, statecode and statuscode which will assign and change the record state/status.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases: Values

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -LookupColumns
Hashset of lookup column name in the target entity to column name in the referred to entity with which to find the records.

```yaml
Type: Hashtable
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MatchOn
List of list of field names that identify an existing record to update based on the values of those fields in the InputObject.
For create/update these are used if a record with and Id matching the value of the Id cannot be found.
The first list that returns a match is used.
e.g.
("firstname", "lastname"), "fullname" will try to find an existing record based on the firstname AND listname from the InputObject and if not found it will try by fullname.
For upsert only a single list is allowed and it must match the properties of an alternate key defined on the table.

When multiple records match the MatchOn criteria, an error is raised unless -AllowMultipleMatches is specified.

```yaml
Type: String[][]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MaxDegreeOfParallelism
Maximum number of parallel set operations. Default is 1 (parallel processing disabled).
When set to a value greater than 1, records are processed in parallel using multiple connections.

Each worker thread processes records concurrently with its own cloned connection to Dataverse.
Parallel processing can be combined with batching (`-BatchSize` > 1) for even better performance.
Progress, errors, and verbose output are all handled thread-safely.

Example: Process 1000 records with 4 workers and batches of 100:


$records | Set-DataverseRecord -Connection $conn -TableName contact -CreateOnly -MaxDegreeOfParallelism 4 -BatchSize 100

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 1
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoCreate
If specified then no records will be created even if no existing records matching the ID and or MatchOn fields is found.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoUpdate
If specified existing records matching the ID and or MatchOn fields will not be updated.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoUpdateColumns
List of column names which will not be included when updating existing records.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
If specified, the InputObject is written to the pipeline as a PSObject with an Id property set indicating the primary key of the affected record (even if nothing was updated). The output is always converted to a PSObject format regardless of the input object type (hashtable, Entity, or PSObject) for uniformity.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
{{ Fill ProgressAction Description }}

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Retries
Number of times to retry each batch item on failure. Default is 0 (no retries). Each retry uses exponential backoff based on the `-InitialRetryDelay` parameter.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -RetrievalBatchSize
Controls the maximum number of records to retrieve in a single query when checking for existing records. Default is 500. Specify 1 to retrieve one record at a time (original behavior).

When processing multiple records through the pipeline, this parameter batches retrieval queries to significantly improve performance:
- Records are queued until the batch size is reached or processing completes
- All queued records are retrieved in a single optimized query using `In` operator (for ID and single-column MatchOn) or `Or` conditions (for multi-column MatchOn and intersect entities)
- After retrieval, records are processed sequentially for create/update operations

Performance impact:
- Default (500): Retrieves up to 500 records in one query, reducing N queries to ceil(N/500)
- Value of 1: Disables batching, retrieves one record at a time (original behavior)
- Higher values: Better performance for large batches, but may hit Dataverse query limits

This parameter only affects retrieval of existing records for comparison. To skip retrieval entirely and maximize performance, use `-CreateOnly` or `-UpdateAllColumns`.

```yaml
Type: UInt32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 500
Accept pipeline input: False
Accept wildcard characters: False
```

### -TableName
Logical name of entity. For many-to-many relationships, specify the intersect table name (e.g., "listcontact", "account_accounts").

```yaml
Type: String
Parameter Sets: (All)
Aliases: EntityName, LogicalName

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -UpdateAllColumns
If specified an update containing all supplied columns will be issued without retrieving the existing record for comparison (default is to remove unchanged columns). Id must be provided

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Upsert
If specified, upsert request will be used to create/update existing records as appropriate.
-MatchOn is not supported with this option

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WhatIf
Shows what would happen if the cmdlet runs.
The cmdlet is not run.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Management.Automation.PSObject
### System.String
### System.Guid
### System.Nullable`1[[System.Guid, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
