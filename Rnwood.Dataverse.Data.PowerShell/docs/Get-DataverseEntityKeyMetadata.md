---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseEntityKeyMetadata

## SYNOPSIS
Retrieves entity key (alternate key) metadata for a Dataverse table.

## SYNTAX

```
Get-DataverseEntityKeyMetadata [-EntityName] <String> [[-KeyName] <String>] [-UseMetadataCache] [-Published]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Retrieves metadata about entity keys (alternate keys) defined on a Dataverse table. 
Entity keys provide a way to uniquely identify records using columns other than the primary key.
You can retrieve all keys for a table or a specific key by name.

By default, unpublished (draft) metadata is retrieved. Use -Published to retrieve only published metadata.

## EXAMPLES

### Example 1: Get all keys for an entity
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseEntityKeyMetadata -EntityName contact
```

Retrieves all entity keys defined on the contact table.

### Example 2: Get a specific entity key
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseEntityKeyMetadata -EntityName contact -KeyName emailaddress1_key
```

Retrieves metadata for a specific entity key.

### Example 3: Get published keys with caching
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseEntityKeyMetadata -EntityName account -Published -UseMetadataCache
```

Retrieves published entity key metadata for the account table using the metadata cache for improved performance.

## PARAMETERS

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

### -EntityName
Logical name of the entity (table) to retrieve key metadata for

```yaml
Type: String
Parameter Sets: (All)
Aliases: TableName

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -KeyName
Logical name of the specific key to retrieve.
If not specified, returns all keys for the entity

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
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

### -Published
Retrieve only published metadata.
By default, unpublished (draft) metadata is retrieved which includes all changes

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

### -UseMetadataCache
Use the shared global metadata cache for improved performance

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String
## OUTPUTS

### Microsoft.Xrm.Sdk.Metadata.EntityKeyMetadata
## NOTES

## RELATED LINKS
