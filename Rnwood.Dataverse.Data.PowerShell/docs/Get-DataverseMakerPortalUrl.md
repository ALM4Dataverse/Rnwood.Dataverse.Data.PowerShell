---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseMakerPortalUrl

## SYNOPSIS
Generates a URL to open the Power Apps Maker Portal for the current environment.

## SYNTAX

```
Get-DataverseMakerPortalUrl [-TableName <String>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet generates a URL that opens the Power Apps Maker Portal for the Dataverse environment associated with the current connection. 

The Maker Portal is where you can:
- Build and configure Power Apps
- Manage solutions
- View and manage tables (entities)
- Create and manage flows
- Work with connections and data sources

Optionally, you can specify a table name to open that specific table's detail page in the Maker Portal.

## EXAMPLES

### Example 1: Get URL for Maker Portal home page
```powershell
PS C:\> Get-DataverseMakerPortalUrl -Connection $c
```

Returns a URL to open the Maker Portal home page for the connected environment.

### Example 2: Get URL for a specific table in Maker Portal
```powershell
PS C:\> Get-DataverseMakerPortalUrl -Connection $c -TableName "contact"
```

Returns a URL to open the contact table's detail page in the Maker Portal.

### Example 3: Open table from pipeline
```powershell
PS C:\> Get-DataverseEntityMetadata -Connection $c -TableName "account" | Get-DataverseMakerPortalUrl -Connection $c
```

Gets the account table metadata and generates a URL to open it in the Maker Portal.

### Example 4: Open Maker Portal directly in browser
```powershell
PS C:\> Start-Process (Get-DataverseMakerPortalUrl -Connection $c -TableName "contact")
```

Opens the contact table in the Maker Portal directly in the default web browser.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet, or string specifying Dataverse organization URL (e.g. http://server.com/MyOrg/). If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.

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

### -TableName
The logical name of the table to open in the maker portal (e.g., 'account', 'contact'). If not provided, opens the Maker Portal home page.

```yaml
Type: String
Parameter Sets: (All)
Aliases: EntityName, LogicalName

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String
## OUTPUTS

### System.String
## NOTES

The Maker Portal URL format is:
- Home page: https://make.powerapps.com/environments/{environmentId}/home
- Table page: https://make.powerapps.com/environments/{environmentId}/entities/entity/{tableName}

## RELATED LINKS

[Power Apps Maker Portal](https://make.powerapps.com)
