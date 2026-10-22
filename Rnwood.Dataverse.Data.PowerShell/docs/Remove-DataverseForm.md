---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseForm

## SYNOPSIS
Removes/deletes a form from a Dataverse environment.

## SYNTAX

### ById
```
Remove-DataverseForm -Id <Guid> [-Publish] [-IfExists] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### ByName
```
Remove-DataverseForm -Entity <String> -Name <String> [-Publish] [-IfExists] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Remove-DataverseForm cmdlet deletes form definitions from a Dataverse environment. Forms can be deleted by ID or by entity name and form name. The cmdlet supports -WhatIf and -Confirm for safe deletion operations and includes built-in confirmation prompts for destructive operations.

?? **Warning**: Deleting forms is irreversible. Always use -WhatIf first and ensure you have backups of important forms.

## EXAMPLES

### Example 1: Delete a form by ID with confirmation
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $formId = 'a1234567-89ab-cdef-0123-456789abcdef'
PS C:\> Remove-DataverseForm -Id $formId
```

Deletes a form by its ID with built-in confirmation prompt.

### Example 2: Delete a form by entity and name
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseForm -Entity 'contact' -Name 'Old Contact Form' -Confirm:$false
```

Deletes a form by entity name and form name without confirmation.

### Example 3: Delete form and publish immediately
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseForm -Id $formId -Publish -Confirm:$false
```

Deletes a form and immediately publishes the entity to apply changes.

### Example 4: Safe deletion with IfExists
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseForm -Entity 'account' -Name 'Test Form' -IfExists -Confirm:$false
```

Attempts to delete a form but doesn't raise an error if the form doesn't exist.

### Example 5: Preview deletion with WhatIf
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseForm -Id $formId -WhatIf
```

Shows what would happen if the form were deleted without actually deleting it.

### Example 6: Batch delete forms with pipeline
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseForm -Entity 'contact' | 
    Where-Object { $_.Name -like 'Test*' } | 
    Remove-DataverseForm -IfExists -Confirm:$false
```

Finds and deletes all contact forms whose names start with 'Test'.

### Example 7: Delete form and handle errors gracefully
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> try {
    Remove-DataverseForm -Entity 'contact' -Name 'NonexistentForm' -IfExists
    Write-Host "Form deletion completed successfully"
} catch {
    Write-Warning "Failed to delete form: $($_.Exception.Message)"
}
```

Demonstrates error handling when deleting forms.

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

### -Entity
Logical name of the entity/table containing the form to delete

```yaml
Type: String
Parameter Sets: ByName
Aliases: EntityName, TableName

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Id
ID of the form to delete

```yaml
Type: Guid
Parameter Sets: ById
Aliases: formid

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -IfExists
Don't raise an error if the form doesn't exist. Useful for idempotent scripts where the form may have already been deleted.

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

### -Name
Name of the form to delete

```yaml
Type: String
Parameter Sets: ByName
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
Controls how progress information is displayed during cmdlet execution.

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
Publish the entity after deleting the form. This applies the changes immediately and makes them visible to users.

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

### System.Guid
## OUTPUTS

### System.Object
## NOTES

**Safety Features:**
- Built-in confirmation prompts for destructive operations
- Support for -WhatIf to preview changes
- -IfExists parameter to avoid errors in idempotent scripts

**Publishing Behavior:**
- Form deletion takes effect immediately in the unpublished layer
- Use -Publish to immediately apply changes to the published layer
- Changes are visible to users only after publishing

**Error Handling:**
- Throws exceptions if form is not found (unless -IfExists is used)
- Supports both published and unpublished form deletion
- Validates form existence before attempting deletion

**Best Practices:**
- Always test with -WhatIf first
- Use -IfExists in automated scripts for idempotency
- Consider backing up forms before deletion
- Use -Publish when immediate effect is needed

**Related Operations:**
- Use Get-DataverseForm to find forms before deletion
- Use Set-DataverseForm to modify forms instead of deleting/recreating
- Consider deactivating forms instead of deleting them when possible

## RELATED LINKS
