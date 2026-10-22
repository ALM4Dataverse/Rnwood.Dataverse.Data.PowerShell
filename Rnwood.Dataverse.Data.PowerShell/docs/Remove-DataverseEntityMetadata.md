---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseEntityMetadata

## SYNOPSIS
Deletes an entity (table) from Dataverse.

## SYNTAX

```
Remove-DataverseEntityMetadata [-EntityName] <String> [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The `Remove-DataverseEntityMetadata` cmdlet permanently deletes an entity (table) and all its data from Dataverse. This operation cannot be undone and will delete:
- The entity definition and all its attributes
- All records in the entity
- All relationships to the entity
- All views, forms, and charts associated with the entity
- All security roles and permissions for the entity

**?? WARNING: This is a destructive operation that permanently deletes data!**

By default, this cmdlet prompts for confirmation (ConfirmImpact = High). Use `-Force` to suppress the confirmation prompt.

## EXAMPLES

### Example 1: Delete an entity with confirmation
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseEntityMetadata -EntityName new_customentity

Confirm
Are you sure you want to perform this action?
Performing the operation "Delete entity" on target "new_customentity".
[Y] Yes  [A] Yes to All  [N] No  [L] No to All  [S] Suspend  [?] Help (default is "Y"): Y
```

Deletes the entity `new_customentity` after prompting for confirmation.

### Example 2: Delete an entity without confirmation
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseEntityMetadata -EntityName new_customentity -Force
```

Deletes the entity `new_customentity` without prompting for confirmation.

### Example 3: Delete multiple entities
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> @("new_entity1", "new_entity2", "new_entity3") | Remove-DataverseEntityMetadata -Force
```

Deletes multiple entities by passing entity names through the pipeline.

### Example 4: Use WhatIf to preview deletion
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseEntityMetadata -EntityName new_testentity -WhatIf

What if: Performing the operation "Delete entity" on target "new_testentity".
```

Shows what would happen without actually deleting the entity.

### Example 5: Delete entity with specific connection
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $conn = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive
PS C:\> Remove-DataverseEntityMetadata -EntityName new_oldentity -Force
```

Deletes an entity using a specific connection.

### Example 6: Delete test entities matching a pattern
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseEntityMetadata | 
    Where-Object { $_.LogicalName -like "new_test*" } | 
    ForEach-Object { 
        Write-Host "Deleting $($_.LogicalName)..."
        Remove-DataverseEntityMetadata -EntityName $_.LogicalName -Force 
    }
```

Finds and deletes all entities whose names start with "new_test".

### Example 7: Conditional deletion based on records
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $recordCount = (Get-DataverseRecord -TableName new_customentity).Count
PS C:\> if ($recordCount -eq 0) {
    Write-Host "Entity is empty, safe to delete"
    Remove-DataverseEntityMetadata -EntityName new_customentity -Force
} else {
    Write-Warning "Entity contains $recordCount records. Deletion cancelled."
}
```

Checks if an entity is empty before deleting it.

### Example 8: Handle deletion errors
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> try {
    Remove-DataverseEntityMetadata -EntityName new_customentity -Force -ErrorAction Stop
    Write-Host "Entity deleted successfully"
} catch {
    Write-Error "Failed to delete entity: $($_.Exception.Message)"
}
```

Handles potential errors during entity deletion with proper error handling.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet.

If not provided, uses the default connection set via `Get-DataverseConnection -SetAsDefault`.

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
Logical name of the entity (table) to delete. This is the internal name of the table (e.g., `new_customentity`, `account`, `contact`).

**?? WARNING:** Deleting an entity is permanent and cannot be undone. All data, relationships, and customizations will be lost.

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

**Note:** This cmdlet has ConfirmImpact = High, so confirmation is required by default unless `-Force` or `-Confirm:$false` is specified.

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

This cmdlet provides programmatic access to Dataverse metadata. For comprehensive documentation and examples, see the metadata concept guide at docs/core-concepts/metadata.md

## RELATED LINKS

[Get-DataverseEntityMetadata](Get-DataverseEntityMetadata.md)

[Set-DataverseEntityMetadata](Set-DataverseEntityMetadata.md)

[Remove-DataverseAttributeMetadata](Remove-DataverseAttributeMetadata.md)

[Microsoft Learn: Delete or remove a table](https://learn.microsoft.com/power-apps/maker/data-platform/data-platform-delete-entity)
