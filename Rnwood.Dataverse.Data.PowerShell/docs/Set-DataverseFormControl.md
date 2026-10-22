---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseFormControl

## SYNOPSIS
Creates or updates a control in a Dataverse form section or header.

## SYNTAX

### Standard (Default)
```
Set-DataverseFormControl -FormId <Guid> [-SectionName <String>] -TabName <String> [-ControlId <String>]
 [-DataField <String>] [-ControlType <String>] [-Labels <Hashtable>] [-Disabled] [-Rows <Int32>]
 [-ColSpan <Int32>] [-RowSpan <Int32>] [-ShowLabel] [-IsRequired] [-Parameters <Hashtable>] [-PassThru]
 [-Row <Int32>] [-Column <Int32>] [-CellId <String>] [-Auto] [-LockLevel <Int32>] [-Hidden]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### RawXml
```
Set-DataverseFormControl -FormId <Guid> [-SectionName <String>] -TabName <String> [-ControlId <String>]
 -ControlXml <String> [-ControlType <String>] [-Labels <Hashtable>] [-Disabled] [-Rows <Int32>]
 [-ColSpan <Int32>] [-RowSpan <Int32>] [-ShowLabel] [-IsRequired] [-Parameters <Hashtable>] [-PassThru]
 [-Row <Int32>] [-Column <Int32>] [-CellId <String>] [-Auto] [-LockLevel <Int32>] [-Hidden]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Set-DataverseFormControl cmdlet creates new controls or updates existing controls in Dataverse form sections and headers. Controls are the individual form elements (text boxes, dropdowns, lookups, etc.) that users interact with. The cmdlet supports all standard control types and allows raw XML for advanced scenarios.

Header controls appear at the top of forms and are visible across all tabs. To work with header controls, use TabName='[Header]' and omit the SectionName parameter.

You can specify control properties like data binding, positioning, visibility, labels, and behavior. When creating controls, you can specify their position using Index, InsertBefore, or InsertAfter parameters. For advanced scenarios, use the RawXml parameter set to provide complete control XML.

## EXAMPLES

### Example 1: Add a simple text field control
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $form = Get-DataverseForm -Entity 'contact' -Name 'Information'
PS C:\> Set-DataverseFormControl -FormId $form.Id -SectionName 'GeneralSection' `
    -DataField 'firstname' -Labels @{1033 = 'First Name'} -ShowLabel -PassThru
```

Adds a text field control for the firstname attribute with a custom label.

### Example 2: Create a lookup control with custom properties
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormControl -FormId $formId -SectionName 'ContactInfo' `
    -DataField 'parentcustomerid' -ControlType 'Lookup' `
    -Labels @{1033 = 'Parent Account'} -ColSpan 2 -IsRequired -PassThru
```

Creates a lookup control spanning 2 columns and marked as required.

### Example 3: Add a multiline text control
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormControl -FormId $formId -SectionName 'Details' `
    -DataField 'description' -ControlType 'Standard' `
    -Labels @{1033 = 'Description'} -Rows 4 -ColSpan 2 -PassThru
```

Creates a multiline text area with 4 rows spanning 2 columns.

### Example 4: Hide a control on the form
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormControl -FormId $formId -SectionName 'Details' `
    -DataField 'internalfield' -Labels @{1033 = 'Internal Field'} `
    -Hidden -PassThru
```

Creates a hidden control that won't be visible to users on the form.

### Example 5: Insert control at specific position
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormControl -FormId $formId -SectionName 'GeneralSection' `
    -DataField 'emailaddress1' -Labels @{1033 = 'Primary Email'} `
    -InsertAfter 'lastname' -ColSpan 2 -PassThru
```

Inserts an email control after the 'lastname' control.

### Example 6: Create a subgrid control without DataField
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $parameters = @{
    'targetEntityType' = 'opportunity'
    'viewId' = '12345678-1234-1234-1234-123456789012'
    'RelationshipName' = 'contact_customer_opportunity'
}
PS C:\> Set-DataverseFormControl -FormId $formId -SectionName 'RelatedData' `
    -TabName 'General' -ControlType 'Subgrid' -ControlId 'opportunities_subgrid' `
    -Labels @{1033 = 'Related Opportunities'} -Parameters $parameters -ColSpan 2 -PassThru
```

Creates a subgrid control showing related opportunities. Note: Subgrids don't require a DataField parameter since they're not bound to a single attribute.

### Example 7: Create a web resource control without DataField
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $webResourceParams = @{
    'src' = 'WebResources/custom_chart.html'
    'height' = '300'
    'scrolling' = 'no'
}
PS C:\> Set-DataverseFormControl -FormId $formId -SectionName 'Analytics' `
    -TabName 'General' -ControlType 'WebResource' -ControlId 'chart_webresource' `
    -Labels @{1033 = 'Sales Chart'} -Parameters $webResourceParams -ColSpan 2 -PassThru
```

Adds a web resource control displaying a custom HTML chart. Web resources don't require a DataField parameter.

### Example 7: Auto-create subgrid using relationship name
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormControl -FormId $formId -SectionName 'RelatedData' `
    -TabName 'General' -DataField 'contact_customer_accounts' `
    -Labels @{1033 = 'Related Accounts'} -ColSpan 2 -PassThru
```

Creates a subgrid control by specifying a relationship name in DataField. When DataField contains a one-to-many or many-to-many relationship name, the cmdlet automatically determines the control type as Subgrid. No need to explicitly specify ControlType.

### Example 8: Update existing control properties
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormControl -FormId $formId -SectionName 'ContactInfo' `
    -ControlId 'existing-control-id' -DataField 'telephone1' `
    -Labels @{1033 = 'Primary Phone'} -IsRequired -ColSpan 1
```

Updates an existing control's label, requirement status, and layout.

### Example 9: Create control using raw XML for advanced configuration
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $controlXml = @"
<control id="custom_datetime" classid="{5B773807-9FB2-42DB-97C3-7A91EFF8ADFF}" 
         datafieldname="custom_datetime" disabled="false">
  <labels>
    <label description="Custom Date/Time" languagecode="1033" />
  </labels>
  <parameters>
    <DateAndTimeStyle>DateAndTime</DateAndTimeStyle>
    <HideTimeForDate>false</HideTimeForDate>
  </parameters>
</control>
"@
PS C:\> Set-DataverseFormControl -FormId $formId -SectionName 'CustomSection' `
    -ControlXml $controlXml -PassThru
```

Creates a datetime control using raw XML for full control over configuration.

### Example 11: Create control with cell-level attributes
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormControl -FormId $formId `
    -TabName 'General' -SectionName 'Details' `
    -DataField 'description' -ControlType 'Memo' `
    -Labels @{1033 = 'Notes'} -ColSpan 2 -RowSpan 2 `
    -Auto -LockLevel 0 -CellId 'cell_description' -PassThru
```

Creates a memo control with cell-level attributes including auto-sizing and custom cell ID.

### Example 11: Update existing control to make it required
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormControl -FormId $formId `
    -TabName 'General' -SectionName 'ContactInfo' `
    -DataField 'telephone1' -IsRequired -ShowLabel
```

Updates an existing control to make it required. The cmdlet now supports updating existing controls without recreating them.

### Example 13: Create Email control with validation
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormControl -FormId $formId `
    -TabName 'General' -SectionName 'ContactInfo' `
    -DataField 'emailaddress1' -ControlType 'Email' `
    -Labels @{1033 = 'Primary Email'} -IsRequired -ColSpan 2 -PassThru
```

Creates an email control with built-in email validation.

### Example 14: Create Money control for currency field
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormControl -FormId $formId `
    -TabName 'Financial' -SectionName 'Pricing' `
    -DataField 'revenue' -ControlType 'Money' `
    -Labels @{1033 = 'Annual Revenue'} -ColSpan 1 -PassThru
```

Creates a currency control with proper formatting.

### Example 15: Bulk control creation with error handling
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $controls = @(
    @{ DataField = 'firstname'; Label = 'First Name'; ColSpan = 1 }
    @{ DataField = 'lastname'; Label = 'Last Name'; ColSpan = 1; IsRequired = $true }
    @{ DataField = 'emailaddress1'; Label = 'Email'; ColSpan = 2; ControlType = 'Standard' }
)

PS C:\> foreach ($ctrl in $controls) {
    try {
        $params = @{
            Connection = $c
            FormId = $formId
            SectionName = 'ContactDetails'
            DataField = $ctrl.DataField
            Label = $ctrl.Label
            ColSpan = $ctrl.ColSpan
            ShowLabel = $true
            Visible = $true
            PassThru = $true
        }
        if ($ctrl.IsRequired) { $params.IsRequired = $true }
        if ($ctrl.ControlType) { $params.ControlType = $ctrl.ControlType }
        
        $controlId = Set-DataverseFormControl @params
        Write-Host "Created control: $($ctrl.DataField) with ID: $controlId"
    }
    catch {
        Write-Warning "Failed to create control $($ctrl.DataField): $($_.Exception.Message)"
    }
}
```

Creates multiple controls with error handling and conditional parameters.

### Example 15: Create header control (form header)
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormControl -FormId $formId `
    -TabName '[Header]' -DataField 'emailaddress1' `
    -ControlType 'Email' -Labels @{1033 = 'Email'} -Disabled -PassThru
```

Creates a read-only email control in the form header. Header controls are displayed at the top of the form across all tabs. The header section is automatically created if it doesn't exist.

### Example 17: Create multiple header controls
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Create email control in header
PS C:\> Set-DataverseFormControl -FormId $formId `
    -TabName '[Header]' -DataField 'emailaddress1' `
    -ControlType 'Email' -Labels @{1033 = 'Email'} -ColSpan 1 -Disabled

PS C:\> # Create owner control in header
PS C:\> Set-DataverseFormControl -FormId $formId `
    -TabName '[Header]' -DataField 'ownerid' `
    -ControlType 'Lookup' -Labels @{1033 = 'Owner'} -ColSpan 1 -Disabled

PS C:\> # Get all header controls
PS C:\> Get-DataverseFormControl -FormId $formId -TabName '[Header]'
```

Creates multiple controls in the form header with specific positioning.

### Example 17: Update existing header control
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormControl -FormId $formId `
    -TabName '[Header]' -DataField 'emailaddress1' `
    -Labels @{1033 = 'Primary Email'} -ColSpan 2
```

Updates an existing header control's label and cell span without recreating it.

## PARAMETERS

### -Auto
Whether the cell should be auto-sized

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

### -CellId
ID of the cell that will contain the control. Used for advanced form customization scenarios.

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

### -ColSpan
Number of columns the control spans in the section layout. Typical values are 1-4 depending on section column configuration.

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

### -Column
Zero-based column index where the control should be placed. Must not exceed section column count.

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

### -ControlId
ID of the control to update. When provided, the cmdlet updates an existing control instead of creating a new one.

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

### -ControlType
Control type for specialized behaviors. Valid values: Standard, Lookup, OptionSet, DateTime, Boolean, Subgrid, WebResource, QuickForm, Spacer, IFrame, Timer, KBSearch, Notes, Email, Memo, Money, Data.

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: Standard, Lookup, OptionSet, DateTime, Boolean, Subgrid, WebResource, QuickForm, Spacer, IFrame, Timer, KBSearch, Notes, Email, Memo, Money, Data

Required: False
Position: Named
Default value: Standard
Accept pipeline input: False
Accept wildcard characters: False
```

### -ControlXml
Raw XML for the control element. Use this for advanced control configurations that require specific XML structure.

```yaml
Type: String
Parameter Sets: RawXml
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DataField
Data field name (attribute logical name) for the control. This binds the control to a specific entity attribute.

**Can also accept relationship names**: When DataField contains a one-to-many or many-to-many relationship name, the cmdlet automatically determines the control type as Subgrid.

**Not required** for special controls that are not bound to attributes: Subgrid, WebResource, QuickForm, Spacer, IFrame, Timer, KBSearch (when creating them without a relationship binding).

```yaml
Type: String
Parameter Sets: Standard
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Disabled
Whether the control is disabled (read-only) for users. Disabled controls display data but cannot be edited.

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
ID of the form containing the section where the control will be created or updated.

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
Whether the control is hidden

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

### -IsRequired
Whether the control is required (must have a value before saving). This affects field validation and visual indicators.

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

### -Labels
The labels for the control as a hashtable keyed by LCID. Example: @{1033 = 'English Label'; 1031 = 'German Label'}

```yaml
Type: Hashtable
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LockLevel
Lock level for the cell

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

### -Parameters
Custom parameters as a hashtable for special controls (Subgrid, WebResource, QuickForm, etc.). Parameters vary by control type.

```yaml
Type: Hashtable
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
Return the control ID after creation or update. Useful for chaining operations or tracking created controls.

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

### -Row
Zero-based row index where the control should be placed. New rows are added if needed.

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

### -Rows
Number of rows for multiline text controls. Controls the height of text areas and memo fields.

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

### -RowSpan
Number of rows the control spans in the section layout. Useful for controls that need more vertical space.

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

### -SectionName
Name of the section containing the control. The section must exist in the form. Not required when working with header controls (TabName='[Header]').

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

### -ShowLabel
Whether to show the control label. Default behavior varies by control type and form configuration.

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
Name of the tab containing the section. Use '[Header]' to create or update controls in the form header.

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

### System.String
## NOTES

**Control Types and Usage:**

| ControlType | Purpose | DataField Required | Common Parameters |
|-------------|---------|-------------------|-------------------|
| **Standard** | Basic text/numeric input | ✓ Yes | Rows (for multiline) |
| **Email** | Email address fields | ✓ Yes | None (validates email format) |
| **Memo** | Multiline text areas | ✓ Yes | Rows (height in lines) |
| **Money** | Currency fields | ✓ Yes | None (inherits from attribute) |
| **Lookup** | Entity references | ✓ Yes | None (uses default lookup behavior) |
| **OptionSet** | Choice/Picklist fields | ✓ Yes | None (inherits from attribute) |
| **DateTime** | Date/time fields | ✓ Yes | DateAndTimeStyle, HideTimeForDate |
| **Boolean** | Yes/No fields | ✓ Yes | None (checkbox behavior) |
| **Notes** | Attachments | ✓ Yes | None |
| **Data** | Generic data fields | ✓ Yes | None |
| **Subgrid** | Related records | ✗ No | targetEntityType, viewId, RelationshipName |
| **WebResource** | Custom HTML/images | ✗ No | src, height, width, scrolling |
| **QuickForm** | Embedded forms | ✗ No | EntityName, FormId |
| **Spacer** | Layout spacing | ✗ No | Height |
| **IFrame** | External content | ✗ No | src, height, width, scrolling |
| **Timer** | Date/time tracking | ✗ No | None |
| **KBSearch** | Knowledge base | ✗ No | None |

**Parameter Sets:**
- **Create**: For creating new controls (no ControlId required)
- **Update**: For updating existing controls (ControlId required)
- **RawXml**: For advanced scenarios with custom XML

**Positioning Logic:**
- **Index**: Absolute position (0-based) within the section
- **InsertBefore**: Relative position before another control
- **InsertAfter**: Relative position after another control
- If none specified: Appends to the end of the section

**Layout Guidelines:**
- **ColSpan 1**: Single column width (typical for short fields)
- **ColSpan 2**: Double width (good for longer text, emails)
- **ColSpan 3-4**: Wide controls (descriptions, subgrids)
- **RowSpan**: Rarely used, mainly for custom controls

**Common Parameters by Control Type:**

**Subgrid Parameters:**  
- targetEntityType = 'opportunity'
- viewId = 'view-guid'
- RelationshipName = 'contact_customer_opportunity'
- EnableQuickFind = 'true'
- EnableViewPicker = 'true'
- RecordsPerPage = '5'

**WebResource Parameters:**  
- src = 'WebResources/custom_control.html'
- height = '300'
- width = '100%'
- scrolling = 'no'
- border = '0'

**DateTime Parameters:**  
- DateAndTimeStyle = 'DateAndTime' (or 'DateOnly')
- HideTimeForDate = 'false'

**Best Practices:**
- Use meaningful control IDs for easier maintenance
- Test control layouts on different screen sizes
- Consider user workflow when positioning controls
- Use appropriate ColSpan values for content
- Validate required field combinations
- Test with different user roles and permissions
- Header controls should be read-only (Disabled) in most cases
- Keep header simple with only key fields (3-5 controls recommended)
- Use ColSpan to balance header control widths
- For special controls (Subgrid, WebResource, etc.), omit DataField and use ControlType to specify the control type
- For relationship-based subgrids, provide the relationship name in DataField and the control type will be auto-determined

**Control Types Requiring DataField vs. Not:**

**Attribute-Bound Controls (DataField Required):**
- Standard, Lookup, OptionSet, DateTime, Boolean, Email, Memo, Money, Notes, Data

**Special Controls (DataField Not Required):**
- Subgrid, WebResource, QuickForm, Spacer, IFrame, Timer, KBSearch

**Relationship-Based Controls:**
- When DataField contains a one-to-many or many-to-many relationship name (e.g., "contact_customer_accounts"), the cmdlet automatically determines the control type as Subgrid
- You can override the auto-determined type by specifying the ControlType parameter

When creating special controls without a relationship binding, you must specify the ControlType parameter but should NOT provide a DataField parameter.

**Working with Header Controls:**
- Use TabName='[Header]' to create or update header controls
- SectionName parameter is not required for header controls
- Header is automatically created if it doesn't exist
- Header controls are visible across all form tabs
- Header controls typically should be Disabled to prevent editing
- Common header controls: Owner, Modified By, Modified On, Status, Email
- Position header controls using Index, InsertBefore, or InsertAfter parameters

## RELATED LINKS

[Get-DataverseFormControl](Get-DataverseFormControl.md)

[Remove-DataverseFormControl](Remove-DataverseFormControl.md)

[Get-DataverseFormSection](Get-DataverseFormSection.md)

[Set-DataverseFormSection](Set-DataverseFormSection.md)

[Get-DataverseForm](Get-DataverseForm.md)

[Set-DataverseForm](Set-DataverseForm.md)
