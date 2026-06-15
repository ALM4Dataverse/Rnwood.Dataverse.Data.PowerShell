---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseAttributeMetadata

## SYNOPSIS
Deletes an attribute (column) from a Dataverse entity.

## SYNTAX

```
Remove-DataverseAttributeMetadata [-EntityName] <String> [-AttributeName] <String>
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The `Remove-DataverseAttributeMetadata` cmdlet permanently deletes an attribute (column) from a Dataverse entity. This operation cannot be undone and will delete:
- The attribute definition
- All data stored in that attribute for all records
- Any dependencies on the attribute (views, forms, business rules referencing it)

**?? WARNING: This is a destructive operation that permanently deletes data!**

By default, this cmdlet prompts for confirmation (ConfirmImpact = High). Use `-Force` to suppress the confirmation prompt.

## EXAMPLES

### Example 1: Delete an attribute with confirmation
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseAttributeMetadata -EntityName account -AttributeName new_customfield

Confirm
Are you sure you want to perform this action?
Performing the operation "Delete attribute 'new_customfield'" on target "Entity 'account'".
[Y] Yes  [A] Yes to All  [N] No  [L] No to All  [S] Suspend  [?] Help (default is "Y"): Y
```

Deletes the attribute `new_customfield` from the `account` entity after prompting for confirmation.

### Example 2: Delete an attribute without confirmation
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseAttributeMetadata -EntityName contact -AttributeName new_oldfield -Force
```

Deletes the attribute `new_oldfield` from the `contact` entity without prompting.

### Example 3: Delete multiple attributes from an entity
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> @("new_field1", "new_field2", "new_field3") | 
    Remove-DataverseAttributeMetadata -EntityName account -Force
```

Deletes multiple attributes from the `account` entity.

### Example 4: Use WhatIf to preview deletion
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseAttributeMetadata -EntityName account -AttributeName new_testfield -WhatIf

What if: Performing the operation "Delete attribute 'new_testfield'" on target "Entity 'account'".
```

Shows what would happen without actually deleting the attribute.

### Example 5: Delete all custom attributes from an entity
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $entity = Get-DataverseEntityMetadata -EntityName new_customentity
PS C:\> $entity.Attributes | 
    Where-Object { $_.LogicalName -like "new_*" } |
    ForEach-Object {
        Write-Host "Deleting attribute: $($_.LogicalName)"
        Remove-DataverseAttributeMetadata -EntityName new_customentity `
            -AttributeName $_.LogicalName -Force
    }
```

Finds and deletes all custom attributes (starting with "new_") from an entity.

### Example 6: Conditional deletion based on data
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $records = Get-DataverseRecord -TableName account -Columns new_customfield
PS C:\> $hasData = $records | Where-Object { $null -ne $_.new_customfield }

PS C:\> if (-not $hasData) {
    Write-Host "Field is empty in all records, safe to delete"
    Remove-DataverseAttributeMetadata -EntityName account -AttributeName new_customfield -Force
} else {
    Write-Warning "Field contains data in $($hasData.Count) records. Review before deletion."
}
```

Checks if an attribute contains data before deleting it.

### Example 7: Delete with error handling
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> try {
    Remove-DataverseAttributeMetadata -EntityName account -AttributeName new_obsolete `
        -Force -ErrorAction Stop
    Write-Host "Attribute deleted successfully"
    
    # Publish customizations after deletion
    Invoke-DataversePublishAllXml
    Write-Host "Customizations published"
} catch {
    Write-Error "Failed to delete attribute: $($_.Exception.Message)"
}
```

Deletes an attribute with proper error handling and publishes customizations afterward.

### Example 8: Cleanup unused test fields
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $testFields = @("new_test1", "new_test2", "new_debug1", "new_temp")
PS C:\> foreach ($field in $testFields) {
    Write-Host "Checking $field..."
    try {
        Remove-DataverseAttributeMetadata -EntityName account -AttributeName $field `
            -Force -ErrorAction Stop
        Write-Host "  Deleted $field" -ForegroundColor Green
    } catch {
        Write-Host "  Could not delete $field (may not exist or has dependencies)" `
            -ForegroundColor Yellow
    }
}
```

Attempts to delete multiple test fields, handling cases where they may not exist.

### Example 9: Delete lookup attribute
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Note: Deleting a lookup attribute also deletes the relationship
PS C:\> Remove-DataverseAttributeMetadata -EntityName contact -AttributeName new_projectid -Force
```

Deletes a lookup attribute. Note that this also removes the associated relationship.

### Example 10: Use specific connection
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $conn = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive
PS C:\> Remove-DataverseAttributeMetadata `
    -EntityName account -AttributeName new_deprecated -Force
```

Deletes an attribute using a specific connection.

## PARAMETERS

### -AttributeName
Logical name of the attribute (column) to delete. This is the internal name of the column (e.g., `new_customfield`, `accountnumber`).

**?? WARNING:** Deleting an attribute is permanent and cannot be undone. All data in this column will be lost.

```yaml
Type: String
Parameter Sets: (All)
Aliases: ColumnName

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

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
Logical name of the entity (table) that contains the attribute to delete.

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

[Set-DataverseAttributeMetadata](Set-DataverseAttributeMetadata.md)

[Remove-DataverseEntityMetadata](Remove-DataverseEntityMetadata.md)

[Microsoft Learn: Delete or remove a column](https://learn.microsoft.com/power-apps/maker/data-platform/data-platform-delete-field)
