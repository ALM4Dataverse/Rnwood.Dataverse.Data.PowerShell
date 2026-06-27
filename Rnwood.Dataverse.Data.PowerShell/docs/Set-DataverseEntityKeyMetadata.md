---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseEntityKeyMetadata

## SYNOPSIS
Creates an alternate key on an entity (table) in Dataverse.

## SYNTAX

```
Set-DataverseEntityKeyMetadata [-EntityName] <String> [-SchemaName] <String> [-DisplayName <String>]
 -KeyAttributes <String[]> [-PassThru] [-Publish] [-Force] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Set-DataverseEntityKeyMetadata cmdlet creates a new alternate key on an entity in Dataverse.
Alternate keys provide a way to uniquely identify records using combinations of attributes other than the primary key.

NOTE: Alternate keys cannot be updated after creation due to Dataverse limitations. To modify a key, you must delete it using Remove-DataverseEntityKeyMetadata and then create a new one.

## EXAMPLES

### Example 1: Create a simple alternate key
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $connection = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive
PS C:\> Set-DataverseEntityKeyMetadata -EntityName contact -SchemaName "contact_emailaddress1_key" -KeyAttributes @("emailaddress1")
```

This command creates an alternate key on the contact entity using the emailaddress1 attribute.

### Example 2: Create a composite key with multiple attributes
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseEntityKeyMetadata -EntityName contact -SchemaName "contact_name_key" -KeyAttributes @("firstname", "lastname") -DisplayName "Full Name Key"
```

This command creates an alternate key using both firstname and lastname attributes with a custom display name.

### Example 3: Create a key and publish the entity
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseEntityKeyMetadata -EntityName account -SchemaName "account_number_key" -KeyAttributes @("accountnumber") -Publish
```

This command creates an alternate key and immediately publishes the entity to make the key active.

### Example 4: Create a key and return the metadata
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $key = Set-DataverseEntityKeyMetadata -EntityName contact -SchemaName "contact_email_key" -KeyAttributes @("emailaddress1") -PassThru
PS C:\> $key | Format-List
```

This command creates an alternate key and returns the key metadata object for further inspection.

### Example 5: Force creation without existence check
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseEntityKeyMetadata -EntityName contact -SchemaName "contact_phone_key" -KeyAttributes @("telephone1") -Force
```

This command creates an alternate key while skipping the existence check. Use this with caution as it may cause errors if the key already exists.

## PARAMETERS

### -Connection
The Dataverse connection to use. If not specified, uses the default connection.

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

### -DisplayName
The display name of the alternate key. If not provided, defaults to the SchemaName.

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

### -EntityName
The logical name of the entity (table) to create the key on.

```yaml
Type: String
Parameter Sets: (All)
Aliases: TableName

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Force
If specified, skips checking if the key already exists. Use with caution as it may cause errors if the key exists.

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

### -KeyAttributes
An array of attribute (column) logical names that make up the alternate key. At least one attribute is required.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
If specified, returns the created key metadata.

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

### -Publish
If specified, publishes the entity after creating the key.

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

### -SchemaName
The schema name of the alternate key (e.g., 'new_customkey' or 'contact_emailaddress1_key').

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
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
Shows what would happen if the cmdlet runs. The cmdlet is not run.

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

### None
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES
- Alternate keys cannot be updated after creation. To modify a key, delete it first and then create a new one.
- Publishing is required for the key to become active, but can be done separately if needed.
- The key attributes must already exist on the entity before creating the alternate key.
- Key creation is an asynchronous process that may take some time to complete.

## RELATED LINKS

[Get-DataverseEntityKeyMetadata](Get-DataverseEntityKeyMetadata.md)
[Remove-DataverseEntityKeyMetadata](Remove-DataverseEntityKeyMetadata.md)
[Define alternate keys for an entity](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/define-alternate-keys-entity)
