---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseFileData

## SYNOPSIS
Uploads file data to a Dataverse file column.

## SYNTAX

### FilePath
```
Set-DataverseFileData -TableName <String> -Id <Guid> -ColumnName <String> -FilePath <String>
 [-MimeType <String>] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

### Bytes
```
Set-DataverseFileData -TableName <String> -Id <Guid> -ColumnName <String> -FileContent <Byte[]>
 [-FileName <String>] [-MimeType <String>] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Set-DataverseFileData cmdlet uploads file data to a Dataverse file column. You can upload from a file path or from a byte array in memory. The cmdlet uses block-based uploading (4MB blocks) for efficient transfer of large files. MIME types are automatically detected from file extensions using the MimeTypesMap package, but can be manually overridden if needed.

## EXAMPLES

### Example 1: Upload file from path
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFileData -TableName "account" -Id $accountId -ColumnName "documentfile" -FilePath "C:\Documents\contract.pdf"
```

Uploads a file from the specified path to the file column. MIME type is automatically detected as "application/pdf".

### Example 2: Upload from byte array
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $bytes = [System.IO.File]::ReadAllBytes("C:\Documents\contract.pdf")
PS C:\> Set-DataverseFileData -TableName "account" -Id $accountId -ColumnName "documentfile" -FileContent $bytes -FileName "contract.pdf"
```

Uploads file content from a byte array with a specified filename.

### Example 3: Upload with manual MIME type
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFileData -TableName "account" -Id $accountId -ColumnName "documentfile" -FilePath "C:\Documents\data.bin" -MimeType "application/octet-stream"
```

Uploads a file with a manually specified MIME type, overriding auto-detection.

### Example 4: Pipe from Get-DataverseRecord
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseRecord -TableName "account" -Id $accountId | Set-DataverseFileData -ColumnName "documentfile" -FilePath "C:\Documents\contract.pdf"
```

Pipes a record and uploads a file to it.

### Example 5: Use WhatIf to preview changes
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFileData -TableName "account" -Id $accountId -ColumnName "documentfile" -FilePath "C:\Documents\contract.pdf" -WhatIf
```

Shows what would happen if the cmdlet runs without actually uploading the file.

## PARAMETERS

### -ColumnName
Logical name of the file column

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet.
If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.

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

### -FileContent
File content as a byte array

```yaml
Type: Byte[]
Parameter Sets: Bytes
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -FileName
Filename to use when uploading from byte array

```yaml
Type: String
Parameter Sets: Bytes
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FilePath
File path of the file to upload

```yaml
Type: String
Parameter Sets: FilePath
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Id
ID of the record to update with the file

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -MimeType
MIME type for the file (e.g., 'application/pdf', 'image/png'). If not specified, automatically determined from file extension.

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

### -ProgressAction
Determines how PowerShell responds to progress updates generated by the cmdlet.

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
Logical name of table containing the file column

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

### System.String
### System.Guid
### System.Byte[]
## OUTPUTS

### System.Void
## NOTES

## RELATED LINKS
