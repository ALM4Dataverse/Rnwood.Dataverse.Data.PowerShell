---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseFormControl

## SYNOPSIS
Retrieves control information from a Dataverse form including controls from tabs, sections, and the form header.

## SYNTAX

```
Get-DataverseFormControl -FormId <Guid> [-TabName <String>] [-SectionName <String>] [-ControlId <String>]
 [-DataField <String>] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The Get-DataverseFormControl cmdlet retrieves control information from a Dataverse form. It parses the FormXML to extract control details including ID, data field, class ID, labels, events, parameters, and visibility settings. You can retrieve all controls from a form or filter by tab, section, control ID, or data field name.

The cmdlet supports retrieving controls from:
- Form tabs and sections (standard controls)
- Form header (use TabName='[Header]')
- Hidden controls section

## EXAMPLES

### Example 1: Get all controls from a form
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $form = Get-DataverseForm -Entity 'contact' -Name 'Information'
PS C:\> Get-DataverseFormControl -FormId $form.FormId
```

Retrieves all controls from all tabs, sections, and the header in the contact Information form.

### Example 2: Get controls from a specific section (requires TabName)
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $formId = '12345678-1234-1234-1234-123456789012'
PS C:\> Get-DataverseFormControl -FormId $formId -TabName 'General' -SectionName 'ContactDetails'
```

Retrieves all controls from the 'ContactDetails' section in the 'General' tab. Note: TabName is required when SectionName is specified since section names are only unique within a tab.

### Example 3: Get a specific control by ID
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseFormControl -FormId $formId -ControlId 'firstname_ctrl'
```

Retrieves the control with ID 'firstname_ctrl'.

### Example 4: Get controls by data field
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseFormControl -FormId $formId -DataField 'emailaddress1'
```

Retrieves all controls bound to the 'emailaddress1' field.

### Example 5: Get controls from specific tab and section
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseFormControl -FormId $formId -TabName 'Details' -SectionName 'ContactInfo'
```

Retrieves controls from the 'ContactInfo' section within the 'Details' tab.

### Example 6: Explore control and cell properties
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $control = Get-DataverseFormControl -FormId $formId -DataField 'firstname'
PS C:\> $control.Id              # Control ID
PS C:\> $control.DataField       # Bound data field name
PS C:\> $control.ClassId         # Control class identifier
PS C:\> $control.Labels          # Localized labels
PS C:\> $control.Events          # Event handlers
PS C:\> $control.Parameters      # Custom parameters
PS C:\> $control.ColSpan         # Cell column span
PS C:\> $control.RowSpan         # Cell row span
PS C:\> $control.CellId          # Cell identifier
PS C:\> $control.Auto            # Cell auto-sizing
PS C:\> $control.LockLevel       # Cell lock level
PS C:\> $control.CellLabels      # Cell-level labels (if different from control)
PS C:\> $control.CellEvents      # Cell-level events
```

Explores the structure and properties of a specific control including cell-level attributes.

### Example 7: Find controls with specific properties
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $controls = Get-DataverseFormControl -FormId $formId
PS C:\> $controls | Where-Object { $_.Hidden -eq $true }                    # Hidden controls
PS C:\> $controls | Where-Object { $_.IsRequired -eq $true }               # Required controls
PS C:\> $controls | Where-Object { $_.Disabled -eq $true }                 # Disabled controls
PS C:\> $controls | Where-Object { $_.ShowLabel -eq $false }               # Controls without labels
```

Filters controls based on specific properties.

### Example 8: Get controls and their locations
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseFormControl -FormId $formId | 
    Select-Object TabName, SectionName, Id, DataField | 
    Sort-Object TabName, SectionName, Id
```

Gets controls with their location information and sorts by hierarchy.

### Example 9: Get all header controls
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseFormControl -FormId $formId -TabName '[Header]'
```

Retrieves all controls from the form header. Header controls are commonly used for displaying key information at the top of forms.

### Example 10: Get specific header control by data field
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseFormControl -FormId $formId -TabName '[Header]' -DataField 'emailaddress1'
```

Retrieves the email address control from the form header.

### Example 11: Find all email controls across the entire form
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $controls = Get-DataverseFormControl -FormId $formId
PS C:\> $emailControls = $controls | Where-Object { $_.ClassId -match 'ADA2203E-B4CD' }
PS C:\> $emailControls | Select-Object TabName, SectionName, DataField
```

Finds all email-type controls in the form, including those in the header and regular sections.

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
ID of a specific control to retrieve. If not specified, all controls are returned.

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

### -DataField
Data field name to filter controls. Returns all controls bound to the specified field.

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

### -FormId
ID of the form to retrieve controls from.

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
Name of the section containing the controls. TabName is required when using SectionName since section names are only unique within a tab. If not specified, controls from all sections are returned.

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
Name of the tab containing the sections. If not specified, controls from all tabs are returned. Use '[Header]' to retrieve controls from the form header.

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
Form -> Header -> Tabs -> Tab Columns -> Sections -> Rows -> Cells -> Controls

The form header is a special section at the top of forms that typically displays key fields in a condensed format. Header controls are displayed on all form tabs and cannot be hidden by tab navigation.

Controls are the finest level of detail in a form structure. Each control is contained in a cell within a row, which can be either:
- In a section within a tab column (regular controls)
- In the form header (header controls)

**Working with Header Controls:**
- Use TabName='[Header]' to get controls from the form header
- Header controls are returned with TabName and SectionName both set to "[Header]"
- When no filters are applied, the cmdlet returns controls from tabs, sections, header, and hidden controls

The cmdlet also searches the form's `hiddencontrols` section when no tab/section filters are applied. Hidden controls are returned with TabName and SectionName set to "[HiddenControls]".

**Control Types:**
Controls are identified by their ClassId attribute which determines behavior:
- Standard text/numeric input controls
- Lookup controls for entity references
- Option set controls for choice fields
- Date/time pickers
- Boolean (checkbox) controls
- Subgrids for related records
- Web resources for custom content
- Quick forms for embedded forms
- Spacer controls for layout
- Timer controls for time tracking

**Control Properties:**
- **DataField**: The logical name of the entity attribute the control is bound to
- **ClassId**: GUID that identifies the control type and behavior
- **Disabled**: Whether the control accepts input
- **Hidden**: Whether the control is visible on the form
- **ShowLabel**: Whether to display the field label
- **IsRequired**: Whether the control enforces required field validation

**Cell Properties (New):**
Controls are contained within cells, which have their own properties:
- **CellId**: Unique identifier for the cell
- **ColSpan**: Number of columns the cell spans (1-4 typically)
- **RowSpan**: Number of rows the cell spans
- **Auto**: Whether the cell auto-sizes to fit content
- **LockLevel**: Cell-level edit restrictions (0=None, 1=Form, 2=System)
- **CellLabels**: Labels defined at cell level (can override control labels)
- **CellEvents**: Events attached to the cell (separate from control events)
- **CellShowLabel**: Cell-level override for label display

**Common Use Cases:**
- Form control auditing and analysis
- Control-level customization validation
- Automated form configuration documentation
- Cross-environment form comparison
- Finding controls by data field across forms

**Performance Notes:**
- Retrieves unpublished form version first, falls back to published
- Parses FormXML efficiently with minimal performance impact
- Use filter parameters for better performance on forms with many controls

## RELATED LINKS

[Get-DataverseForm](Get-DataverseForm.md)

[Get-DataverseFormTab](Get-DataverseFormTab.md)

[Get-DataverseFormSection](Get-DataverseFormSection.md)

[Set-DataverseFormControl](Set-DataverseFormControl.md)

[Remove-DataverseFormControl](Remove-DataverseFormControl.md)
