---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseFormTab

## SYNOPSIS
Removes a tab from a Dataverse form.

## SYNTAX

```
Remove-DataverseFormTab -FormId <Guid> [-TabName <String>] [-TabId <String>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Remove-DataverseFormTab cmdlet permanently removes a tab from a Dataverse form. When a tab is removed, all sections and controls within that tab are also deleted. This operation cannot be undone, so use with caution.

The cmdlet supports removing tabs by either name or ID, and can optionally publish the form immediately after the removal to make changes visible to users.

## EXAMPLES

### Example 1: Remove a tab by name
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $form = Get-DataverseForm -Entity 'contact' -Name 'Information'
PS C:\> Remove-DataverseFormTab -FormId $form.Id -TabName 'CustomTab'
```

Removes the tab named 'CustomTab' from the contact Information form.

### Example 2: Remove a tab by ID with immediate publishing
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseFormTab -FormId $formId -TabId 'tab-guid-12345' -Publish
```

Removes the tab with the specified ID and publishes the form to make changes visible immediately.

### Example 3: Remove tab with confirmation
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseFormTab -FormId $formId -TabName 'ObsoleteTab' -Confirm
```

Removes the tab with user confirmation prompt before proceeding.

### Example 4: Remove multiple tabs safely
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $tabsToRemove = @('TempTab1', 'TempTab2', 'TempTab3')
PS C:\> foreach ($tabName in $tabsToRemove) {
    try {
        Remove-DataverseFormTab -FormId $formId -TabName $tabName -WhatIf
        Write-Host "Would remove tab: $tabName"
    }
    catch {
        Write-Warning "Cannot remove tab $tabName: $($_.Exception.Message)"
    }
}

# After reviewing, execute without -WhatIf
PS C:\> foreach ($tabName in $tabsToRemove) {
    try {
        Remove-DataverseFormTab -FormId $formId -TabName $tabName
        Write-Host "Removed tab: $tabName"
    }
    catch {
        Write-Warning "Failed to remove tab $tabName: $($_.Exception.Message)"
    }
}
```

Safely removes multiple tabs by first previewing with -WhatIf, then executing the actual removal.

### Example 5: Remove tabs based on conditions
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $form = Get-DataverseForm -Entity 'account' -Name 'Information' -ParseFormXml
PS C:\> $tabsToRemove = $form.ParsedForm.Tabs | Where-Object { 
    $_.Name -like 'Legacy*' -and $_.Sections.Count -eq 0 
}

PS C:\> foreach ($tab in $tabsToRemove) {
    Write-Host "Removing empty legacy tab: $($tab.Name)"
    Remove-DataverseFormTab -FormId $form.Id -TabName $tab.Name
}
```

Removes tabs that match specific criteria (legacy tabs with no sections).

### Example 6: Backup before removal
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

PS C:\> # Now safely remove the tab
PS C:\> Remove-DataverseFormTab -FormId $form.Id -TabName 'OldTab'
PS C:\> Write-Host "Tab removed. Backup saved for recovery if needed."
```

Creates a backup of the form before removing the tab for safety.

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

### -FormId
ID of the form containing the tab to remove.

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

### -TabId
ID of the tab to remove. Use this when you have the specific tab ID rather than the name.

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
Name of the tab to remove. The tab name must exist in the form.

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
Prompts you for confirmation before running the cmdlet. Recommended when removing important tabs.

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
- ?? **Permanent Operation**: Removing a tab permanently deletes all sections and controls within it
- ?? **Data Loss**: Any custom controls, business rules, or form scripts targeting removed elements may break
- ?? **No Undo**: This operation cannot be reversed - always backup forms before major changes
- ?? **Form Validation**: Ensure removing the tab doesn't break form functionality or required fields

**Parameter Sets:**
- **ByName**: Remove tab using its name (most common)
- **ById**: Remove tab using its unique ID (useful when names might not be unique)

**Best Practices:**
1. **Always backup** forms before removing tabs
2. **Use -WhatIf** to preview changes before execution
3. **Test in development** environments before production
4. **Check dependencies** - ensure no business rules or scripts reference the tab
5. **Verify required fields** - ensure no required fields are in the tab being removed
6. **Consider hiding** instead of removing if the tab might be needed later
7. **Document changes** for maintenance and rollback purposes

**Alternative to Removal:**
Instead of removing tabs, consider:
- Setting `Visible="false"` to hide tabs temporarily
- Moving controls to other tabs before removal
- Using security roles to control tab visibility

**Recovery Options:**
- Restore from form XML backup
- Recreate tab structure manually
- Import solution with previous form version
- Use version control for form definitions

**Common Use Cases:**
- Removing temporary or test tabs
- Cleaning up legacy form elements
- Streamlining form layouts
- Removing tabs after data migration
- Form reorganization and simplification

**Dependencies to Check Before Removal:**
- Business rules targeting controls in the tab
- JavaScript form scripts referencing tab elements
- Security role permissions for specific tabs
- Workflows or Power Automate flows using form data
- Custom controls or PCF components in the tab

## RELATED LINKS

[Get-DataverseFormTab](Get-DataverseFormTab.md)

[Set-DataverseFormTab](Set-DataverseFormTab.md)

[Remove-DataverseFormSection](Remove-DataverseFormSection.md)

[Remove-DataverseFormControl](Remove-DataverseFormControl.md)

[Get-DataverseForm](Get-DataverseForm.md)

[Set-DataverseForm](Set-DataverseForm.md)
