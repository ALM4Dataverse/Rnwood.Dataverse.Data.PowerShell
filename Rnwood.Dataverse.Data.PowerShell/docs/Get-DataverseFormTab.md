---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseFormTab

## SYNOPSIS
Retrieves tab information from a Dataverse form.

## SYNTAX

```
Get-DataverseFormTab -FormId <Guid> [-TabName <String>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The Get-DataverseFormTab cmdlet retrieves tab information from a Dataverse form. It parses the FormXML to extract tab details including name, ID, visibility, layout, labels, and sections. You can retrieve all tabs from a form or filter by a specific tab name.

## EXAMPLES

### Example 1: Get all tabs from a form
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $form = Get-DataverseForm -Entity 'contact' -Name 'Information'
PS C:\> Get-DataverseFormTab -FormId $form.Id
```

Retrieves all tabs from the contact Information form.

### Example 2: Get a specific tab by name
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $formId = '12345678-1234-1234-1234-123456789012'
PS C:\> Get-DataverseFormTab -FormId $formId -TabName 'General'
```

Retrieves only the 'General' tab from the specified form.

### Example 3: Pipeline usage with form objects
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseForm -Entity 'account' | 
    ForEach-Object { Get-DataverseFormTab -FormId $_.Id }
```

Gets all tabs from all account forms using pipeline.

### Example 4: Explore tab structure
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $tab = Get-DataverseFormTab -FormId $formId -TabName 'Details'
PS C:\> $tab.Layout          # Shows OneColumn, TwoColumns, ThreeColumns, or Custom
PS C:\> $tab.Sections        # Array of sections in the tab
PS C:\> $tab.Labels          # Localized labels for the tab
```

Explores the structure and properties of a specific tab.

### Example 5: Check tab visibility settings
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $tabs = Get-DataverseFormTab -FormId $formId
PS C:\> $tabs | Where-Object { $_.Hidden -eq $true }
```

Finds all hidden tabs in the form.

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
ID of the form to retrieve tabs from.

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

### -TabName
Name of a specific tab to retrieve. If not specified, all tabs are returned.

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
Form -> Header -> Tabs -> Tab (with columns) -> Sections -> Controls

Tabs typically contain 1-3 columns, each column contains sections, and sections contain controls arranged in rows and cells.

**Tab Layout Types:**
- **OneColumn**: Single column layout (100% width)
- **TwoColumns**: Two-column layout (typically 50%/50% or custom)
- **ThreeColumns**: Three-column layout (typically 33%/33%/33% or custom)
- **Custom**: More than 3 columns or non-standard configuration
- **None**: No columns element found

**Common Use Cases:**
- Form analysis and documentation
- Programmatic form structure validation
- Automated form configuration auditing
- Cross-environment form comparison

**Performance Notes:**
- Retrieves unpublished form version first, falls back to published
- Parses FormXML on-demand, minimal performance impact
- Use -TabName to filter for better performance on large forms

## RELATED LINKS

[Get-DataverseForm](Get-DataverseForm.md)

[Set-DataverseFormTab](Set-DataverseFormTab.md)

[Remove-DataverseFormTab](Remove-DataverseFormTab.md)

[Get-DataverseFormSection](Get-DataverseFormSection.md)

[Get-DataverseFormControl](Get-DataverseFormControl.md)
