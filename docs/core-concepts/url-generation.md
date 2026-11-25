# URL Generation

Generate URLs for accessing Dataverse resources in web browsers. These cmdlets help you create deep links to records, navigate to the Power Apps Maker Portal, or open the Power Platform Admin Center.

## Overview

The URL generation cmdlets provide a convenient way to:
- Share links to specific records with team members
- Create bookmarks for frequently accessed resources
- Integrate Dataverse URLs into custom applications or workflows
- Quickly navigate to administrative interfaces

## Available Cmdlets

### Get-DataverseRecordUrl

Generates URLs to open specific records or create new records in the Dataverse web interface.

**Basic Usage:**

```powershell
# Get URL for a specific contact record
$url = Get-DataverseRecordUrl -Connection $c -TableName "contact" -Id $contactId

# Get URL to create a new account
$url = Get-DataverseRecordUrl -Connection $c -TableName "account"

# Open record in a specific app by unique name
$url = Get-DataverseRecordUrl -Connection $c -TableName "contact" -Id $contactId -AppUniqueName "sales_app"

# Open record in a specific app by ID
$url = Get-DataverseRecordUrl -Connection $c -TableName "contact" -Id $contactId -AppId $appId

# Open record with a specific form
$url = Get-DataverseRecordUrl -Connection $c -TableName "account" -FormId $formId
```

**Key Features:**
- Supports both existing records (with ID) and new record creation (without ID)
- Can specify app context using either:
  - **AppUniqueName**: Looks up the app ID automatically (including unpublished apps)
  - **AppId**: Uses the app ID directly
- Optional form ID parameter to open specific forms
- Works with pipeline input for batch URL generation

### Get-DataverseMakerPortalUrl

Generates URLs to open the Power Apps Maker Portal for the current environment.

**Basic Usage:**

```powershell
# Get URL for Maker Portal home page
$url = Get-DataverseMakerPortalUrl -Connection $c

# Get URL for a specific table in the Maker Portal
$url = Get-DataverseMakerPortalUrl -Connection $c -TableName "contact"

# Open Maker Portal directly in browser
Start-Process (Get-DataverseMakerPortalUrl -Connection $c -TableName "account")
```

**Key Features:**
- Opens the Maker Portal home page by default
- Optional table context to open a specific table's detail page
- Automatically determines the environment ID from the connection

### Get-DataverseAdminPortalUrl

Generates URLs to open the Power Platform Admin Center for the current environment.

**Basic Usage:**

```powershell
# Get URL for Admin Center
$url = Get-DataverseAdminPortalUrl -Connection $c

# Open Admin Center directly in browser
Start-Process (Get-DataverseAdminPortalUrl -Connection $c)
```

**Key Features:**
- Opens the environment hub page in the Admin Center
- Automatically determines the environment ID from the connection

## Common Scenarios

### Sharing Record Links

Generate and share URLs for specific records:

```powershell
# Get URLs for all high-priority cases
$cases = Get-DataverseRecord -Connection $c -TableName "incident" -FilterValues @{ prioritycode = 1 }
$caseUrls = $cases | ForEach-Object {
    [PSCustomObject]@{
        Title = $_.title
        CaseNumber = $_.ticketnumber
        URL = Get-DataverseRecordUrl -Connection $c -TableName "incident" -Id $_.incidentid
    }
}
$caseUrls | Export-Csv -Path "high-priority-cases.csv" -NoTypeInformation
```

### Opening Records in Specific Apps

Open records in different app contexts:

```powershell
# Open contact in Sales Hub app
$salesUrl = Get-DataverseRecordUrl -Connection $c -TableName "contact" -Id $contactId -AppUniqueName "SalesHub"

# Open same contact in Customer Service Hub
$serviceUrl = Get-DataverseRecordUrl -Connection $c -TableName "contact" -Id $contactId -AppUniqueName "CustomerServiceHub"
```

### Bulk URL Generation

Generate URLs for multiple records efficiently:

```powershell
# Generate URLs for all active accounts
Get-DataverseRecord -Connection $c -TableName "account" -FilterValues @{ statecode = 0 } |
    Select-Object name, accountid, 
        @{Name='URL'; Expression={ Get-DataverseRecordUrl -Connection $c -TableName "account" -Id $_.accountid }} |
    Export-Csv -Path "active-accounts-with-urls.csv" -NoTypeInformation
```

### Navigation Helper Functions

Create helper functions for quick navigation:

```powershell
function Open-DataverseRecord {
    param(
        [Parameter(Mandatory)]
        [string]$TableName,
        [Parameter(Mandatory)]
        [guid]$Id,
        [string]$AppUniqueName
    )
    
    $url = if ($AppUniqueName) {
        Get-DataverseRecordUrl -TableName $TableName -Id $Id -AppUniqueName $AppUniqueName
    } else {
        Get-DataverseRecordUrl -TableName $TableName -Id $Id
    }
    
    Start-Process $url
}

# Usage
Open-DataverseRecord -TableName "contact" -Id $contactId -AppUniqueName "sales_app"
```

### Admin Portal Quick Access

Quickly access the admin portal for your environment:

```powershell
# Create a shortcut function
function Open-AdminPortal {
    Start-Process (Get-DataverseAdminPortalUrl)
}

# Usage
Open-AdminPortal
```

## URL Format Reference

### Record URLs
```
https://{org}.crm.dynamics.com/main.aspx?etn={table}&id={recordid}&pagetype=entityrecord
https://{org}.crm.dynamics.com/main.aspx?etn={table}&id={recordid}&pagetype=entityrecord&appid={appid}
https://{org}.crm.dynamics.com/main.aspx?etn={table}&pagetype=entityrecord (new record)
```

### Maker Portal URLs
```
https://make.powerapps.com/environments/{envid}/home
https://make.powerapps.com/environments/{envid}/entities/entity/{tablename}
```

### Admin Portal URLs
```
https://admin.powerplatform.microsoft.com/environments/{envid}/hub
```

## Best Practices

1. **Use AppUniqueName when possible**: It's more maintainable than app IDs and works with unpublished apps
2. **Cache generated URLs**: If you're generating many URLs for the same records, consider caching them
3. **Validate app existence**: When using AppUniqueName, the cmdlet will error if the app isn't found
4. **Consider permissions**: Generated URLs still require the user to have appropriate permissions
5. **Use default connections**: Set a default connection to avoid repeating the `-Connection` parameter

## Related Cmdlets

- [`Get-DataverseConnection`](../../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseConnection.md) — Create or retrieve connections
- [`Get-DataverseRecord`](../../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseRecord.md) — Query records
- [`Get-DataverseAppModule`](../../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseAppModule.md) — Query app modules

## See Also

- [Cmdlet Documentation](../../../Rnwood.Dataverse.Data.PowerShell/docs/)
- [Connection Management](connections.md)
- [App Module Management](app-module-management.md)
