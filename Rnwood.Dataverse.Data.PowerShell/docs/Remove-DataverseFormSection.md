---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseFormSection

## SYNOPSIS
Removes a section from a Dataverse form tab.

## SYNTAX

```
Remove-DataverseFormSection -FormId <Guid> [-TabName <String>] [-SectionName <String>] [-SectionId <String>]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Remove-DataverseFormSection cmdlet permanently removes a section from a Dataverse form tab. When a section is removed, all controls within that section are also deleted. This operation cannot be undone, so use with caution.

The cmdlet supports removing sections by either name or ID within a specific tab, and can optionally publish the form immediately after the removal to make changes visible to users.

## EXAMPLES

### Example 1: Remove a section by name
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $form = Get-DataverseForm -Entity 'contact' -Name 'Information'
PS C:\> Remove-DataverseFormSection -FormId $form.Id -TabName 'General' -SectionName 'CustomSection'
```

Removes the section named 'CustomSection' from the General tab of the contact Information form.

### Example 2: Remove a section by ID with immediate publishing
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseFormSection -FormId $formId -TabName 'Details' -SectionId 'section-guid-12345' -Publish
```

Removes the section with the specified ID from the Details tab and publishes the form to make changes visible immediately.

### Example 3: Remove section with confirmation
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseFormSection -FormId $formId -TabName 'General' -SectionName 'ObsoleteSection' -Confirm
```

Removes the section with user confirmation prompt before proceeding.

### Example 4: Remove multiple sections safely
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $sectionsToRemove = @('TempSection1', 'TempSection2', 'TempSection3')
PS C:\> foreach ($sectionName in $sectionsToRemove) {
    try {
        Remove-DataverseFormSection -FormId $formId -TabName 'Advanced' -SectionName $sectionName -WhatIf
        Write-Host "Would remove section: $sectionName"
    }
    catch {
        Write-Warning "Cannot remove section $sectionName: $($_.Exception.Message)"
    }
}

# After reviewing, execute without -WhatIf
PS C:\> foreach ($sectionName in $sectionsToRemove) {
    try {
        Remove-DataverseFormSection -FormId $formId -TabName 'Advanced' -SectionName $sectionName
        Write-Host "Removed section: $sectionName"
    }
    catch {
        Write-Warning "Failed to remove section $sectionName: $($_.Exception.Message)"
    }
}
```

Safely removes multiple sections by first previewing with -WhatIf, then executing the actual removal.

### Example 5: Remove sections based on conditions
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $form = Get-DataverseForm -Entity 'account' -Name 'Information' -ParseFormXml
PS C:\> $sectionsToRemove = $form.ParsedForm.Tabs | Where-Object { $_.Name -eq 'Details' } | 
    ForEach-Object { $_.Sections } | Where-Object { 
    $_.Name -like 'Legacy*' -and $_.Controls.Count -eq 0 
}

PS C:\> foreach ($section in $sectionsToRemove) {
    Write-Host "Removing empty legacy section: $($section.Name)"
    Remove-DataverseFormSection -FormId $form.Id -TabName 'Details' -SectionName $section.Name
}
```

Removes sections that match specific criteria (legacy sections with no controls).

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

PS C:\> # Now safely remove the section
PS C:\> Remove-DataverseFormSection -FormId $form.Id -TabName 'General' -SectionName 'OldSection'
PS C:\> Write-Host "Section removed. Backup saved for recovery if needed."
```

Creates a backup of the form before removing the section for safety.

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
ID of the form containing the section to remove.

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

### -SectionId
ID of the section to remove. Use this when you have the specific section ID rather than the name.

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

### -SectionName
Name of the section to remove. The section name must exist in the specified tab.

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
Name of the tab containing the section (required when using SectionName)

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
Prompts you for confirmation before running the cmdlet. Recommended when removing important sections.

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
- ?? **Permanent Operation**: Removing a section permanently deletes all controls within it
- ?? **Data Loss**: Any custom controls, business rules, or form scripts targeting removed elements may break
- ?? **No Undo**: This operation cannot be reversed - always backup forms before major changes
- ?? **Form Validation**: Ensure removing the section doesn't break form functionality or required fields

**Parameter Sets:**
- **ByName**: Remove section using its name (most common)
- **ById**: Remove section using its unique ID (useful when names might not be unique)

**Best Practices:**
1. **Always backup** forms before removing sections
2. **Use -WhatIf** to preview changes before execution
3. **Test in development** environments before production
4. **Check dependencies** - ensure no business rules or scripts reference the section
5. **Verify required fields** - ensure no required fields are in the section being removed
6. **Consider hiding** instead of removing if the section might be needed later
7. **Document changes** for maintenance and rollback purposes

**Alternative to Removal:**
Instead of removing sections, consider:
- Setting `Visible="false"` to hide sections temporarily
- Moving controls to other sections before removal
- Using security roles to control section visibility

**Recovery Options:**
- Restore from form XML backup
- Recreate section structure manually
- Import solution with previous form version
- Use version control for form definitions

**Common Use Cases:**
- Removing temporary or test sections
- Cleaning up legacy form elements
- Streamlining form layouts
- Removing sections after data migration
- Form reorganization and simplification

**Dependencies to Check Before Removal:**
- Business rules targeting controls in the section
- JavaScript form scripts referencing section elements
- Security role permissions for specific sections
- Workflows or Power Automate flows using form data
- Custom controls or PCF components in the section

## RELATED LINKS

[Get-DataverseFormSection](Get-DataverseFormSection.md)

[Set-DataverseFormSection](Set-DataverseFormSection.md)

[Remove-DataverseFormTab](Remove-DataverseFormTab.md)

[Remove-DataverseFormControl](Remove-DataverseFormControl.md)

[Get-DataverseForm](Get-DataverseForm.md)

[Set-DataverseForm](Set-DataverseForm.md)
