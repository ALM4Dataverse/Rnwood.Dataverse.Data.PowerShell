---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseRecordUrl

## SYNOPSIS
Generates a URL to open a record in the Dataverse web interface.

## SYNTAX

### ByAppUniqueName (Default)
```
Get-DataverseRecordUrl [-TableName] <String> [[-Id] <Guid>] [-AppUniqueName <String>] [-FormId <Guid>]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### ByAppId
```
Get-DataverseRecordUrl [-TableName] <String> [[-Id] <Guid>] [-AppId <Guid>] [-FormId <Guid>]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet generates a URL that can be used to open a record in the Dataverse web interface. If an ID is provided, the URL will open that specific record. If no ID is provided, the URL will open a form to create a new record.

The generated URL can be:
- Opened directly in a web browser
- Shared with users via email or other communication
- Embedded in custom applications or workflows
- Used for deep-linking into specific records or forms

The cmdlet can optionally include parameters to open the record in a specific app (by app ID or unique name) or with a specific form.

## EXAMPLES

### Example 1: Get URL for a specific record
```powershell
PS C:\> Get-DataverseRecordUrl -Connection $c -TableName "contact" -Id "12345678-1234-1234-1234-123456789012"
```

Returns a URL to open the contact record with the specified ID.

### Example 2: Get URL to create a new record
```powershell
PS C:\> Get-DataverseRecordUrl -Connection $c -TableName "account"
```

Returns a URL to open a form for creating a new account record.

### Example 3: Get URL for a record in a specific app by unique name
```powershell
PS C:\> Get-DataverseRecordUrl -Connection $c -TableName "contact" -Id "12345678-1234-1234-1234-123456789012" -AppUniqueName "myapp_12345"
```

Returns a URL to open the contact record in the app with unique name "myapp_12345". The app ID is automatically looked up (including unpublished apps).

### Example 4: Get URL for a record in a specific app by ID
```powershell
PS C:\> Get-DataverseRecordUrl -Connection $c -TableName "contact" -Id "12345678-1234-1234-1234-123456789012" -AppId "87654321-4321-4321-4321-210987654321"
```

Returns a URL to open the contact record in the specified model-driven app by app ID.

### Example 5: Get URL with a specific form
```powershell
PS C:\> Get-DataverseRecordUrl -Connection $c -TableName "account" -FormId "abcdefgh-abcd-abcd-abcd-abcdefghijkl"
```

Returns a URL to create a new account using the specified form.

### Example 6: Generate URLs from pipeline
```powershell
PS C:\> Get-DataverseRecord -Connection $c -TableName "contact" | Get-DataverseRecordUrl -Connection $c -TableName "contact"
```

Generates URLs for all contact records returned by Get-DataverseRecord.

## PARAMETERS

### -AppId
The App ID to open the record in a specific model-driven app.

```yaml
Type: Guid
Parameter Sets: ByAppId
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -AppUniqueName
The unique name of the app to open the record in a specific model-driven app. The app ID will be looked up automatically (including unpublished apps).

```yaml
Type: String
Parameter Sets: ByAppUniqueName
Aliases: UniqueName

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

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

### -FormId
The Form ID to open a specific form for the record.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Id
The ID of the record. If not provided, generates a URL for creating a new record.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases: RecordId

Required: False
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
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
The logical name of the table (e.g., 'account', 'contact').

```yaml
Type: String
Parameter Sets: (All)
Aliases: EntityName, LogicalName

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String
### System.Nullable`1[[System.Guid, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]
## OUTPUTS

### System.String
## NOTES

When using AppUniqueName, the cmdlet will query the appmodule entity to find the app ID, including unpublished apps.

## RELATED LINKS
