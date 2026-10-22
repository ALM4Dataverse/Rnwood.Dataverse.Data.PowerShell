---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseForm

## SYNOPSIS
Retrieves forms from a Dataverse environment.

## SYNTAX

### ById
```
Get-DataverseForm -Id <Guid> [-IncludeFormXml] [-Published] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### ByEntity
```
Get-DataverseForm -Entity <String> [-FormType <FormType>] [-UniqueNameFilter <String>] [-IncludeFormXml]
 [-Published] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### ByName
```
Get-DataverseForm -Entity <String> -Name <String> [-IncludeFormXml] [-Published] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### ByUniqueName
```
Get-DataverseForm -Entity <String> -UniqueName <String> [-IncludeFormXml] [-Published]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The Get-DataverseForm cmdlet retrieves form definitions from a Dataverse environment. Forms can be retrieved by ID, by entity name (optionally filtered by form type), or by entity name and form name. The cmdlet returns simplified PSObject output with form properties, and can optionally include the raw FormXml content.

Use the specialized form management cmdlets (Get-DataverseFormTab, Get-DataverseFormSection, Get-DataverseFormControl) to work with specific form components.

## EXAMPLES

### Example 1: Get all forms for an entity
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseForm -Entity 'contact'
```

Retrieves all forms for the contact entity.

### Example 2: Get a specific form by ID
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $formId = 'a1234567-89ab-cdef-0123-456789abcdef'
PS C:\> Get-DataverseForm -Id $formId
```

Retrieves a specific form by its ID.

### Example 3: Get forms of a specific type
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseForm -Entity 'account' -FormType 'Main'
```

Retrieves all main forms for the account entity.

### Example 4: Get a form by entity and name
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseForm -Entity 'contact' -Name 'Information'
```

Retrieves the contact form named "Information".

### Example 5: Get form with FormXml
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $form = Get-DataverseForm -Entity 'contact' -Name 'Information' -IncludeFormXml
PS C:\> $form.FormXml
```

Retrieves a form and includes the raw FormXml content.

### Example 6: Work with form structure using specialized cmdlets
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $form = Get-DataverseForm -Entity 'contact' -Name 'Information'
PS C:\> # Get all tabs from the form
PS C:\> $tabs = Get-DataverseFormTab -FormId $form.FormId
PS C:\> # Get all sections from a specific tab
PS C:\> $sections = Get-DataverseFormSection -FormId $form.FormId -TabName 'General'
PS C:\> # Get all controls from a specific section
PS C:\> $controls = Get-DataverseFormControl -FormId $form.FormId -TabName 'General' -SectionName 'Details'
```

Demonstrates how to explore form structure using the specialized form cmdlets.

### Example 7: Get published forms
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseForm -Entity 'contact' -Published
```

Retrieves published forms for the contact entity.

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
Logical name of the entity/table for which to retrieve forms

```yaml
Type: String
Parameter Sets: ByEntity, ByName, ByUniqueName
Aliases: EntityName, TableName

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FormType
Form type filter

```yaml
Type: FormType
Parameter Sets: ByEntity
Aliases:
Accepted values: Dashboard, AppointmentBook, Main, MiniCampaignBO, Preview, MobileExpress, QuickViewForm, QuickCreate, Dialog, TaskFlowForm, InteractionCentricDashboard, Card, MainInteractiveExperience, ContextualDashboard, Other, MainBackup, AppointmentBookBackup, PowerBIDashboard

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Id
ID of the form to retrieve

```yaml
Type: Guid
Parameter Sets: ById
Aliases: formid

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -IncludeFormXml
Include the FormXml in the output (default: false for performance)

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
Name of the form to retrieve

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

### -Published
Include only published forms in the results

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

### -UniqueName
Unique name of the form to retrieve

```yaml
Type: String
Parameter Sets: ByUniqueName
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UniqueNameFilter
Unique name filter for forms

```yaml
Type: String
Parameter Sets: ByEntity
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

**Form Types Available:**
- Dashboard (0) - Dashboard forms
- AppointmentBook (1) - Appointment book forms  
- Main (2) - Main entity forms
- MiniCampaignBO (3) - Mini campaign forms
- Preview (4) - Preview forms
- MobileExpress (5) - Mobile express forms
- QuickViewForm (6) - Quick view forms
- QuickCreate (7) - Quick create forms
- Dialog (8) - Dialog forms
- TaskFlowForm (9) - Task flow forms
- InteractionCentricDashboard (10) - Interaction centric dashboards
- Card (11) - Card forms
- MainInteractiveExperience (12) - Main interactive experience forms
- ContextualDashboard (13) - Contextual dashboards
- Other (100) - Other form types
- MainBackup (101) - Main backup forms
- AppointmentBookBackup (102) - Appointment book backup forms
- PowerBIDashboard (103) - Power BI dashboard forms

**Form Presentation Types:**
- ClassicForm (0) - Classic form presentation
- AirForm (1) - Air form presentation
- ConvertedICForm (2) - Converted interaction centric form presentation

**Performance:**
- Use -IncludeFormXml only when needed as FormXml can be large
- Query by specific ID when possible for best performance
- Use -FormType to filter results when querying by entity

**Related Cmdlets:**
- Use Get-DataverseFormTab to retrieve tab information
- Use Get-DataverseFormSection to retrieve section information  
- Use Get-DataverseFormControl to retrieve control information
- Use Set-DataverseForm to create or update forms
- Use Remove-DataverseForm to delete forms

## RELATED LINKS

[Set-DataverseForm](Set-DataverseForm.md)

[Remove-DataverseForm](Remove-DataverseForm.md)

[Get-DataverseFormTab](Get-DataverseFormTab.md)

[Get-DataverseFormSection](Get-DataverseFormSection.md)

[Get-DataverseFormControl](Get-DataverseFormControl.md)
