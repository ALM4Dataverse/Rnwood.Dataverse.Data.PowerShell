---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseFormTab

## SYNOPSIS
Creates or updates a tab on a Dataverse form with support for column layouts.

## SYNTAX

```
Set-DataverseFormTab -FormId <Guid> [-TabId <String>] [-Name <String>] [-Label <String>]
 [-LanguageCode <Int32>] [-Expanded] [-Hidden] [-ShowLabel] [-VerticalLayout] [-Layout <String>]
 [-Column1Width <Int32>] [-Column2Width <Int32>] [-Column3Width <Int32>] [-Index <Int32>]
 [-InsertBefore <String>] [-InsertAfter <String>] [-PassThru] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Set-DataverseFormTab cmdlet creates new tabs or updates existing ones on Dataverse forms. It supports comprehensive column layout management, allowing you to configure one, two, or three column layouts with custom width percentages.

When creating a new tab, it will be added to the end of the tab list unless positioning parameters (Index, InsertBefore, or InsertAfter) are specified. When updating an existing tab (by providing TabId), the specified properties will be modified while preserving other existing settings.

When changing column layouts, existing sections are automatically redistributed across the new column structure.

## EXAMPLES

### Example 1: Create a simple tab
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormTab -FormId $formId -Name "Details" -Label "Details Tab"
```

Creates a new tab with default single-column layout.

### Example 2: Create a two-column tab
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormTab -FormId $formId -Name "ContactInfo" -Label "Contact Information" `
    -Layout TwoColumns -Column1Width 60 -Column2Width 40
```

Creates a tab with two columns: 60% and 40% width.

### Example 3: Create a three-column tab
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormTab -FormId $formId -Name "Overview" -Label "Overview" `
    -Layout ThreeColumns -Column1Width 40 -Column2Width 30 -Column3Width 30
```

Creates a tab with three columns with custom widths.

### Example 4: Update existing tab layout
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $tab = Get-DataverseFormTab -FormId $formId -TabName "general"
PS C:\> Set-DataverseFormTab -FormId $formId -TabId $tab.Id -Name "general" `
    -Layout TwoColumns -Column1Width 50 -Column2Width 50
```

Updates an existing tab to use a two-column layout with equal widths.

### Example 5: Create tab with positioning
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormTab -FormId $formId -Name "Summary" -Label "Summary" `
    -InsertBefore "general" -Layout OneColumn -PassThru
```

Creates a new tab and positions it before the "general" tab, returning the new tab ID.

### Example 6: Create collapsed hidden tab
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormTab -FormId $formId -Name "Advanced" -Label "Advanced Options" `
    -Hidden -Expanded:$false -ShowLabel
```

Creates a new tab that is hidden and collapsed by default but shows the label.

### Example 7: Update tab visibility and layout
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $tab = Get-DataverseFormTab -FormId $formId -TabName "details"
PS C:\> Set-DataverseFormTab -FormId $formId -TabId $tab.Id -Name "details" `
    -Hidden:$false -Layout ThreeColumns -Column1Width 33 -Column2Width 33 -Column3Width 34
```

Updates an existing tab to make it visible and change to a three-column layout.

### Example 8: Create tab at specific position
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormTab -FormId $formId -Name "FirstTab" -Label "First Tab" `
    -Index 0 -Layout OneColumn
```

Creates a new tab at the beginning (first position) of the tab list.

## PARAMETERS

### -Column1Width
Width of the first column as percentage (0-100)

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

### -Column2Width
Width of the second column as percentage (0-100)

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

### -Column3Width
Width of the third column as percentage (0-100)

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

### -Expanded
Whether the tab is expanded by default

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

### -FormId
ID of the form to create or update the tab on

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
Whether the tab is hidden

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
Zero-based index position to insert the tab. Used only when creating new tabs.

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
Name or ID of the tab after which to insert this tab. Used only when creating new tabs.

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
Name or ID of the tab before which to insert this tab. Used only when creating new tabs.

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
Display label text for the tab

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

### -Layout
Column layout for the tab

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: OneColumn, TwoColumns, ThreeColumns

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name
Logical name of the tab. Required for all operations.

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

### -PassThru
Return the tab ID after creation/update

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

### -ShowLabel
Whether to show the tab label

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

### -TabId
ID of the existing tab to update. If not specified, a new tab will be created.

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

### -VerticalLayout
Whether to use vertical layout for the tab

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

**Column Layout Management:**
- **OneColumn**: Creates a single column layout (100% width)
- **TwoColumns**: Creates a two-column layout (default 50%/50%)
- **ThreeColumns**: Creates a three-column layout (default 33%/33%/34%)
- Column widths are specified as percentages (0-100)
- Total width should add up to 100% for best results

**Tab Creation vs Update:**
- If TabId is not specified, a new tab is created
- If TabId is specified, the existing tab is updated
- New tabs are added at the end unless positioning parameters are used

**Positioning Options (New Tabs Only):**
- **Index**: Zero-based position in the tab list
- **InsertBefore**: Insert before the specified tab (by name or ID)
- **InsertAfter**: Insert after the specified tab (by name or ID)
- Index takes precedence over Insert parameters if multiple are specified

**Section Redistribution:**
When changing column layouts on existing tabs, sections are automatically redistributed:
- Sections are collected from the old layout
- Distributed evenly across the new columns
- Original section order is preserved as much as possible

**Best Practices:**
- Always specify column widths when using TwoColumns or ThreeColumns layouts
- Use descriptive tab names for better form organization
- Consider user experience when positioning tabs
- Test layout changes thoroughly before publishing
- Use -WhatIf to preview changes before applying

## RELATED LINKS

[Get-DataverseFormTab](Get-DataverseFormTab.md)

[Remove-DataverseFormTab](Remove-DataverseFormTab.md)

[Set-DataverseFormSection](Set-DataverseFormSection.md)

[Get-DataverseForm](Get-DataverseForm.md)
