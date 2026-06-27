---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseEntityKeyMetadata

## SYNOPSIS
Deletes an alternate key from an entity (table) in Dataverse.

## SYNTAX

```
Remove-DataverseEntityKeyMetadata [-EntityName] <String> [-KeyName] <String> [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Remove-DataverseEntityKeyMetadata cmdlet deletes an alternate key from an entity in Dataverse.
This operation permanently removes the key definition.

## EXAMPLES

### Example 1: Delete an alternate key
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $connection = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive
PS C:\> Remove-DataverseEntityKeyMetadata -EntityName contact -KeyName "contact_emailaddress1_key" -Confirm:$false
```

This command deletes the alternate key named "contact_emailaddress1_key" from the contact entity without prompting for confirmation.

### Example 2: Delete a key with confirmation
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseEntityKeyMetadata -EntityName account -KeyName "account_number_key"
```

This command deletes the alternate key but prompts for confirmation first (ConfirmImpact is High).

### Example 3: Preview deletion with WhatIf
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseEntityKeyMetadata -EntityName contact -KeyName "contact_name_key" -WhatIf
```

This command shows what would happen if the key were deleted, but doesn't actually perform the deletion.

### Example 4: Delete key from pipeline
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> "contact" | Remove-DataverseEntityKeyMetadata -KeyName "contact_phone_key" -Confirm:$false
```

This command demonstrates piping the entity name to the cmdlet.

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

### -EntityName
The logical name of the entity (table) to delete the key from.

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
The logical name of the alternate key to delete.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
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

### System.String
## OUTPUTS

### System.Object
## NOTES
- This operation permanently deletes the alternate key.
- Confirmation is required by default due to the high impact of this operation.
- Use -Confirm:$false to skip the confirmation prompt.
- You may need to publish the entity after deleting the key for changes to take effect.

## RELATED LINKS

[Get-DataverseEntityKeyMetadata](Get-DataverseEntityKeyMetadata.md)
[Set-DataverseEntityKeyMetadata](Set-DataverseEntityKeyMetadata.md)
[Define alternate keys for an entity](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/define-alternate-keys-entity)
