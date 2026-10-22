---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseFormSection

## SYNOPSIS
Retrieves section information from a Dataverse form.

## SYNTAX

```
Get-DataverseFormSection -FormId <Guid> [-TabName <String>] [-SectionName <String>]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The Get-DataverseFormSection cmdlet retrieves section information from a Dataverse form. It parses the FormXML to extract section details including name, ID, visibility, column layout, labels, and contained controls. You can retrieve all sections from a form, filter by tab name, or get a specific section by name.

## EXAMPLES

### Example 1: Get all sections from a form
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $form = Get-DataverseForm -Entity 'contact' -Name 'Information'
PS C:\> Get-DataverseFormSection -FormId $form.FormId
```

Retrieves all sections from all tabs in the contact Information form.

### Example 2: Get sections from a specific tab
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $formId = '12345678-1234-1234-1234-123456789012'
PS C:\> Get-DataverseFormSection -FormId $formId -TabName 'General'
```

Retrieves all sections from the 'General' tab of the specified form.

### Example 3: Get a specific section by name
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseFormSection -FormId $formId -SectionName 'ContactDetails'
```

Retrieves only the section named 'ContactDetails' from the form.

### Example 4: Get sections from specific tab and section
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseFormSection -FormId $formId -TabName 'Details' -SectionName 'AdditionalInfo'
```

Retrieves the 'AdditionalInfo' section specifically from the 'Details' tab.

### Example 5: Explore section properties
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $section = Get-DataverseFormSection -FormId $formId -TabName 'General' -SectionName 'Summary'
PS C:\> $section.ColumnIndex    # Which column in the tab this section belongs to
PS C:\> $section.Columns        # Number of columns in the section
PS C:\> $section.Controls       # Controls contained in this section
PS C:\> $section.Labels         # Localized labels for the section
```

Explores the structure and properties of a specific section.

### Example 6: Find sections with specific properties
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $sections = Get-DataverseFormSection -FormId $formId
PS C:\> $sections | Where-Object { $_.Hidden -eq $true }                    # Hidden sections
PS C:\> $sections | Where-Object { $_.Columns -gt 1 }                      # Multi-column sections
PS C:\> $sections | Where-Object { $_.ShowLabel -eq $false }               # Sections without labels
```

Filters sections based on specific properties.

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
ID of the form to retrieve sections from.

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
Name of a specific section to retrieve. If not specified, all sections are returned.

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
Name of the tab containing the sections. If not specified, sections from all tabs are returned.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Guid
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES

**Form Structure Hierarchy:**  
Form -> Header -> Tabs -> Tab Columns -> Sections -> Controls

Tabs contain 1-3 columns (typically), each column contains multiple sections. Sections contain controls arranged in rows and cells.

**Section Column Layout:**
- **Columns**: Number of columns within the section (1-4 typical)
- Different from tab columns - sections have their own internal column layout
- Controls within a section are arranged in the specified number of columns

**Cell Label Properties:**
- **CellLabelAlignment**: Center, Left, Right
- **CellLabelPosition**: Top, Left
- Controls how labels appear for controls within the section

**Common Use Cases:**
- Form analysis and documentation
- Section-level customization and validation
- Automated form configuration auditing
- Cross-environment form comparison

**Performance Notes:**
- Retrieves unpublished form version first, falls back to published
- Parses FormXML on-demand with minimal performance impact
- Use -TabName and -SectionName filters for better performance on large forms

## RELATED LINKS

[Get-DataverseForm](Get-DataverseForm.md)

[Get-DataverseFormTab](Get-DataverseFormTab.md)

[Set-DataverseFormSection](Set-DataverseFormSection.md)

[Remove-DataverseFormSection](Remove-DataverseFormSection.md)

[Get-DataverseFormControl](Get-DataverseFormControl.md)
