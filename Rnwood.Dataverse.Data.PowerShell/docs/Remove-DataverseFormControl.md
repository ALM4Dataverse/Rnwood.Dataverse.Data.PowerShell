---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseFormControl

## SYNOPSIS
Removes a control from a Dataverse form section.

## SYNTAX

### ById
```
Remove-DataverseFormControl -FormId <Guid> -ControlId <String> [-SectionName <String>] [-TabName <String>]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### ByDataField
```
Remove-DataverseFormControl -FormId <Guid> -DataField <String> [-SectionName <String>] [-TabName <String>]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Remove-DataverseFormControl cmdlet permanently removes a control from a Dataverse form section. This operation cannot be undone, so use with caution. The cmdlet supports removing controls by either their unique ID or by their data field name within a specific section.

When a control is removed, any business rules, form scripts, or other customizations that reference the control may be affected, so it's important to verify dependencies before removal.

## EXAMPLES

### Example 1: Remove a control by ID
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $form = Get-DataverseForm -Entity 'contact' -Name 'Information'
PS C:\> Remove-DataverseFormControl -FormId $form.Id -ControlId 'firstname_control_id'
```

Removes the control with the specified ID from the form.

### Example 2: Remove a control by data field name
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseFormControl -FormId $formId -TabName 'General' -SectionName 'ContactInfo' -DataField 'middlename'
```

Removes the control bound to the 'middlename' field from the ContactInfo section in the General tab.

### Example 3: Remove control with immediate publishing
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseFormControl -FormId $formId -TabName 'Details' -SectionName 'CustomSection' -DataField 'custom_field' -Publish
```

Removes the custom field control and publishes the form to make changes visible immediately.

### Example 4: Remove control with confirmation
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseFormControl -FormId $formId -TabName 'General' -SectionName 'MainInfo' -DataField 'telephone2' -Confirm
```

Removes the control with user confirmation prompt before proceeding.

### Example 5: Remove multiple controls safely
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $controlsToRemove = @('fax', 'pager', 'telex')
PS C:\> foreach ($fieldName in $controlsToRemove) {
    try {
        Remove-DataverseFormControl -FormId $formId -TabName 'General' -SectionName 'ContactMethods' -DataField $fieldName -WhatIf
        Write-Host "Would remove control: $fieldName"
    }
    catch {
        Write-Warning "Cannot remove control $fieldName: $($_.Exception.Message)"
    }
}

# After reviewing, execute without -WhatIf
PS C:\> foreach ($fieldName in $controlsToRemove) {
    try {
        Remove-DataverseFormControl -FormId $formId -TabName 'General' -SectionName 'ContactMethods' -DataField $fieldName
        Write-Host "Removed control: $fieldName"
    }
    catch {
        Write-Warning "Failed to remove control $fieldName: $($_.Exception.Message)"
    }
}
```

Safely removes multiple controls by first previewing with -WhatIf, then executing the actual removal.

### Example 6: Remove obsolete controls based on conditions
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $form = Get-DataverseForm -Entity 'account' -Name 'Information' -ParseFormXml
PS C:\> $controlsToRemove = $form.ParsedForm.Tabs | 
    ForEach-Object { $_.Sections } |
    ForEach-Object { $_.Controls } | 
    Where-Object { $_.DataField -like 'legacy_*' -and $_.Hidden -eq $true }

PS C:\> foreach ($control in $controlsToRemove) {
    Write-Host "Removing hidden legacy control: $($control.DataField)"
    Remove-DataverseFormControl -FormId $form.Id -ControlId $control.Id
}
```

Removes controls that match specific criteria (hidden legacy controls).

### Example 7: Backup before removal
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # First, backup the form XML
PS C:\> $form = Get-DataverseForm -Entity 'contact' -Name 'Information' -IncludeFormXml
PS C:\> $backup = @{
    FormId = $form.Id
    FormXml = $form.FormXml
    BackupDate = Get-Date
}
PS C:\> $backup | Export-Clixml -Path "FormBackup_$($form.Id)_$(Get-Date -Format 'yyyyMMdd_HHmmss').xml"

PS C:\> # Now safely remove the control
PS C:\> Remove-DataverseFormControl -FormId $form.Id -TabName 'General' -SectionName 'ContactInfo' -DataField 'assistantname'
PS C:\> Write-Host "Control removed. Backup saved for recovery if needed."
```

Creates a backup of the form before removing the control for safety.

### Example 8: Remove control and handle dependencies
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $controlToRemove = 'creditlimit'

PS C:\> # Check if control exists and get details
PS C:\> $control = Get-DataverseFormControl -FormId $formId -TabName 'Details' -SectionName 'Financial' -DataField $controlToRemove

PS C:\> if ($control) {
    Write-Host "Found control: $($control.DataField) - $($control.Label)"
    Write-Warning "Please verify no business rules or scripts reference this control before removal"
    
    $userChoice = Read-Host "Continue with removal? (y/N)"
    if ($userChoice -eq 'y' -or $userChoice -eq 'Y') {
        Remove-DataverseFormControl -FormId $formId -TabName 'Details' -SectionName 'Financial' -DataField $controlToRemove
        Write-Host "Control removed successfully"
    } else {
        Write-Host "Removal cancelled"
    }
} else {
    Write-Warning "Control not found: $controlToRemove"
}
```

Checks for control existence and handles dependencies before removal.

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

### -ControlId
Unique ID of the control to remove. Use this when you have the specific control ID rather than searching by data field.

```yaml
Type: String
Parameter Sets: ById
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DataField
Data field name (attribute logical name) of the control to remove. The control must exist in the specified section.

```yaml
Type: String
Parameter Sets: ByDataField
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FormId
ID of the form containing the control to remove.

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

### -SectionName
Name of the section containing the control to remove. Required when using the ByDataField parameter set.

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

### -TabName
Name of the tab containing the section with the control to remove. Required when using the ByDataField parameter set.

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

### -Confirm
Prompts you for confirmation before running the cmdlet. Recommended when removing important controls.

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
Shows what would happen if the cmdlet runs. Use this to preview the operation before executing.
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

**Important Warnings:**
- ?? **Permanent Operation**: Removing a control permanently deletes it from the form
- ?? **Dependencies**: Business rules, form scripts, or workflows referencing the control may break
- ?? **No Undo**: This operation cannot be reversed - always backup forms before major changes
- ?? **Data Access**: Removing a control doesn't delete the field data, only the form presentation

**Parameter Sets:**
- **ById**: Remove control using its unique ID (faster, more precise)
- **ByDataField**: Remove control using its data field name within a specific tab/section

**Best Practices:**
1. **Always backup** forms before removing controls
2. **Use -WhatIf** to preview changes before execution
3. **Test in development** environments before production
4. **Check dependencies** - ensure no business rules or scripts reference the control
5. **Consider hiding** instead of removing if the control might be needed later
6. **Document changes** for maintenance and rollback purposes
7. **Verify user permissions** after control removal

**Alternative to Removal:**
Instead of removing controls, consider:
- Setting `Visible="false"` to hide controls temporarily
- Setting `Disabled="true"` to make controls read-only
- Using security roles to control control visibility
- Moving controls to other sections or tabs

**Recovery Options:**
- Restore from form XML backup
- Recreate control manually
- Import solution with previous form version
- Use version control for form definitions

**Common Use Cases:**
- Removing temporary or test controls
- Cleaning up obsolete fields
- Streamlining form layouts
- Removing controls after data migration
- Form optimization and simplification

**Dependencies to Check Before Removal:**
- Business rules targeting the control
- JavaScript form scripts referencing the control
- Power Automate flows using the field
- Custom ribbon commands or buttons
- Security role field permissions
- Workflows using the field data
- Reports or views displaying the field

**Control Types and Considerations:**
- **System controls** (like Owner): Usually shouldn't be removed
- **Required fields**: Ensure required validation is handled elsewhere
- **Lookup controls**: Check relationship dependencies
- **Custom controls**: Verify no external dependencies
- **Subgrids**: Check related entity access requirements

## RELATED LINKS

[Get-DataverseFormControl](Get-DataverseFormControl.md)

[Set-DataverseFormControl](Set-DataverseFormControl.md)

[Remove-DataverseFormSection](Remove-DataverseFormSection.md)

[Remove-DataverseFormTab](Remove-DataverseFormTab.md)

[Get-DataverseForm](Get-DataverseForm.md)

[Set-DataverseForm](Set-DataverseForm.md)
