---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseFormSection

## SYNOPSIS
Creates or updates a section in a Dataverse form tab.

## SYNTAX

```
Set-DataverseFormSection -FormId <Guid> -TabName <String> [-SectionId <String>] -Name <String>
 [-Label <String>] [-LanguageCode <Int32>] [-ShowLabel] [-ShowBar] [-Hidden] [-Columns <Int32>]
 [-LabelWidth <Int32>] [-Index <Int32>] [-InsertBefore <String>] [-InsertAfter <String>] [-PassThru]
 [-ColumnIndex <Int32>] [-CellLabelAlignment <CellLabelAlignment>] [-CellLabelPosition <CellLabelPosition>]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Set-DataverseFormSection cmdlet creates new sections or updates existing sections in Dataverse form tabs. Sections are containers within form tabs that hold controls and define layout structure. You can specify positioning, layout properties, labels, and visibility settings.

When creating a new section, it will be added to the first available column unless ColumnIndex is specified. When updating an existing section (by providing SectionId), the specified properties will be modified while preserving other existing settings.

Sections can be positioned using Index, InsertBefore, or InsertAfter parameters, and can be placed in specific tab columns using ColumnIndex.

## EXAMPLES

### Example 1: Create a new section in a tab
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $form = Get-DataverseForm -Entity 'contact' -Name 'Information'
PS C:\> Set-DataverseFormSection -FormId $form.FormId -TabName 'General' `
    -Name 'CustomSection' -Label 'Custom Information' -Columns 2 -PassThru
```

Creates a new section named 'CustomSection' with 2 columns in the General tab.

### Example 2: Update an existing section's properties
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $section = Get-DataverseFormSection -FormId $formId -TabName 'General' -SectionName 'ContactDetails'
PS C:\> Set-DataverseFormSection -FormId $formId -TabName 'General' `
    -SectionId $section.Id -Name 'ContactDetails' `
    -Label 'Updated Contact Information' -ShowLabel -ShowBar -Columns 1
```

Updates an existing section's label and layout properties.

### Example 3: Create a section at a specific position
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormSection -FormId $formId -TabName 'Details' `
    -Name 'AddressSection' -Label 'Address Information' `
    -InsertAfter 'ContactSection' -Columns 2 -PassThru
```

Creates a new section positioned after the 'ContactSection' with 2-column layout.

### Example 4: Create a section in specific column
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormSection -FormId $formId -TabName 'General' `
    -Name 'RightPanelSection' -Label 'Additional Details' `
    -ColumnIndex 1 -Columns 1 -LabelWidth 120 -PassThru
```

Creates a section in the second column (index 1) of the tab with custom label width.

### Example 5: Create section with cell formatting
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormSection -FormId $formId -TabName 'Advanced' `
    -Name 'FormattedSection' -Label 'Formatted Information' `
    -CellLabelAlignment Center -CellLabelPosition Top -Columns 2 -PassThru
```

Creates a section with centered labels positioned above controls.

### Example 6: Create hidden section for conditional display
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormSection -FormId $formId -TabName 'Advanced' `
    -Name 'ConditionalSection' -Label 'Advanced Settings' `
    -Hidden -Columns 1 -ShowLabel:$false -PassThru
```

Creates a hidden section that can be shown conditionally via business rules or JavaScript.

### Example 7: Move existing section to different position
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $section = Get-DataverseFormSection -FormId $formId -TabName 'General' -SectionName 'Summary'
PS C:\> Set-DataverseFormSection -FormId $formId -TabName 'General' `
    -SectionId $section.Id -Name 'Summary' -Index 0
```

Moves an existing section to the first position (index 0) within its column.

### Example 8: Bulk section creation
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $sections = @(
    @{ Name = 'BasicInfo'; Label = 'Basic Information'; Columns = 2; ColumnIndex = 0 }
    @{ Name = 'ContactDetails'; Label = 'Contact Details'; Columns = 1; ColumnIndex = 1 }
    @{ Name = 'Preferences'; Label = 'User Preferences'; Columns = 2; ColumnIndex = 0 }
)

PS C:\> foreach ($section in $sections) {
    try {
        $sectionId = Set-DataverseFormSection -FormId $formId -TabName 'General' `
            -Name $section.Name -Label $section.Label -Columns $section.Columns `
            -ColumnIndex $section.ColumnIndex -PassThru
        Write-Host "Created section: $($section.Name) with ID: $sectionId"
    }
    catch {
        Write-Warning "Failed to create section $($section.Name): $($_.Exception.Message)"
    }
}
```

Creates multiple sections in specific columns with error handling.

## PARAMETERS

### -CellLabelAlignment
Cell label alignment for controls within the section

```yaml
Type: CellLabelAlignment
Parameter Sets: (All)
Aliases:
Accepted values: Center, Left, Right

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CellLabelPosition
Cell label position for controls within the section

```yaml
Type: CellLabelPosition
Parameter Sets: (All)
Aliases:
Accepted values: Top, Left

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ColumnIndex
Zero-based index of the column within the tab where the section should be placed

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Columns
Number of columns in the section (1-4). Controls how many columns of controls can be placed side-by-side in the section.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
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

### -FormId
ID of the form containing the tab where the section will be created or updated

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

### -Hidden
Whether the section is hidden

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

### -Index
Zero-based index position to insert the section within its target column. If not specified, the section is added at the end.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InsertAfter
Name or ID of the section after which to insert the new section. Cannot be used with Index or InsertBefore.

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

### -InsertBefore
Name or ID of the section before which to insert the new section. Cannot be used with Index or InsertAfter.

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

### -Label
Display label text for the section

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

### -LabelWidth
Width of labels in pixels for controls within the section

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LanguageCode
Language code for the label (default: 1033 for English)

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 1033
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name
Logical name of the section. Required for all operations.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
Return the section ID after creation/update

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
ID of the existing section to update. If not specified, a new section will be created.

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

### -ShowBar
Whether to show the section bar (border/divider)

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

### -ShowLabel
Whether to show the section label

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

### -TabName
Name of the tab containing the section

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
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

### System.Guid
## OUTPUTS

### System.String
## NOTES

**Section Layout:**
- **Columns**: Number of columns within the section (1-4)
- Controls within a section are arranged in the specified number of columns
- Different from tab columns - sections have their own internal column layout

**Section Placement:**
- **ColumnIndex**: Determines which tab column the section is placed in (0-based)
- If not specified, section is placed in the first available column
- Tab must have the specified column index or an error will occur

**Positioning Options:**
- **Index**: Zero-based position within the target column
- **InsertBefore**: Insert before the specified section (by name or ID)
- **InsertAfter**: Insert after the specified section (by name or ID)
- Index takes precedence over Insert parameters if multiple are specified

**Cell Label Formatting:**
- **CellLabelAlignment**: How labels are aligned (Center, Left, Right)
- **CellLabelPosition**: Where labels are positioned relative to controls (Top, Left)
- **LabelWidth**: Fixed width in pixels for control labels

**Section Creation vs Update:**
- If SectionId is not specified, a new section is created
- If SectionId is specified, the existing section is updated
- New sections are added at the end of the target column unless positioning is specified

**Best Practices:**
- Use descriptive section names for better form organization
- Group related controls within the same section
- Consider using multiple columns for better space utilization
- Test layout changes thoroughly before publishing
- Use -WhatIf to preview changes before applying

**Common Use Cases:**
- Grouping related form controls
- Creating responsive form layouts
- Organizing information hierarchically
- Supporting conditional visibility scenarios

## RELATED LINKS

[Get-DataverseFormSection](Get-DataverseFormSection.md)

[Remove-DataverseFormSection](Remove-DataverseFormSection.md)

[Set-DataverseFormControl](Set-DataverseFormControl.md)

[Get-DataverseFormTab](Get-DataverseFormTab.md)

[Set-DataverseFormTab](Set-DataverseFormTab.md)
