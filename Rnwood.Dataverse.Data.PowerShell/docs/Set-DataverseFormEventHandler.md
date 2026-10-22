---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseFormEventHandler

## SYNOPSIS
Adds or updates an event handler in a Dataverse form (form-level, attribute-level, tab-level, or control-level).

## SYNTAX

### FormEvent (Default)
```
Set-DataverseFormEventHandler -FormId <Guid> -EventName <String> -FunctionName <String> -LibraryName <String>
 [-HandlerUniqueId <Guid>] [-Enabled <Boolean>] [-Parameters <String>] [-PassExecutionContext <Boolean>]
 [-Application <Boolean>] [-Active <Boolean>] [-AllowCustomEventNames] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### AttributeEvent
```
Set-DataverseFormEventHandler -FormId <Guid> -EventName <String> -FunctionName <String> -LibraryName <String>
 -AttributeName <String> [-HandlerUniqueId <Guid>] [-Enabled <Boolean>] [-Parameters <String>]
 [-PassExecutionContext <Boolean>] [-Application <Boolean>] [-Active <Boolean>] [-AllowCustomEventNames]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### TabEvent
```
Set-DataverseFormEventHandler -FormId <Guid> -EventName <String> -FunctionName <String> -LibraryName <String>
 -TabName <String> [-HandlerUniqueId <Guid>] [-Enabled <Boolean>] [-Parameters <String>]
 [-PassExecutionContext <Boolean>] [-Application <Boolean>] [-Active <Boolean>] [-AllowCustomEventNames]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### ControlEvent
```
Set-DataverseFormEventHandler -FormId <Guid> -EventName <String> -FunctionName <String> -LibraryName <String>
 -TabName <String> -ControlId <String> -SectionName <String> [-HandlerUniqueId <Guid>] [-Enabled <Boolean>]
 [-Parameters <String>] [-PassExecutionContext <Boolean>] [-Application <Boolean>] [-Active <Boolean>]
 [-AllowCustomEventNames] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf]
 [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Set-DataverseFormEventHandler cmdlet adds or updates JavaScript event handlers on Dataverse forms. It supports four event location types:
- **Form-level events**: Events at the form root (e.g., onload, onsave)
- **Attribute-level events**: Events with an attribute property (e.g., onchange for specific fields)
- **Tab-level events**: Events within a tab element (e.g., tabstatechange)
- **Control-level events**: Events within a control element (e.g., onchange)

**Validations performed:**
- Event names are validated against known types (onload, onsave, onchange, tabstatechange, etc.)
- Event names are automatically converted to lowercase (FormXML requirement)
- Web resource must exist (including unpublished versions) and be a JavaScript file (webresourcetype=3)
- Library must already be added to the form using Set-DataverseFormLibrary

Use the `-AllowCustomEventNames` switch to bypass event name validation for custom event types.

## EXAMPLES

### Example 1: Add a form onload handler
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormEventHandler -FormId $formId -EventName "onload" -FunctionName "OnFormLoad" -LibraryName "new_/scripts/main.js"
```

Adds an onload event handler to the form.

### Example 2: Add an attribute-level onchange handler
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormEventHandler -FormId $formId -AttributeName "department" -EventName "onchange" -FunctionName "OnDepartmentChange" -LibraryName "new_/scripts/validation.js"
```

Adds an onchange event handler for the "department" attribute. This creates an event with the attribute property at the form root level.

### Example 3: Add a tab-level tabstatechange handler
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormEventHandler -FormId $formId -TabName "General" -EventName "tabstatechange" -FunctionName "OnTabChange" -LibraryName "new_/scripts/tabs.js"
```

Adds a tabstatechange event handler to the "General" tab. This creates an event within the tab element.

### Example 4: Add a control onchange handler
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormEventHandler -FormId $formId -EventName "onchange" -FunctionName "OnFieldChange" -LibraryName "new_/scripts/validation.js" -ControlId "emailaddress1" -TabName "general" -SectionName "contact_info"
```

Adds an onchange event handler to a specific control.

### Example 5: Add a handler with parameters
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormEventHandler -FormId $formId -EventName "onload" -FunctionName "InitializeForm" -LibraryName "new_/scripts/init.js" -Parameters "{'mode':'create','showWarnings':true}"
```

Adds a handler with JSON parameters that will be passed to the function.

### Example 6: Add a disabled handler
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormEventHandler -FormId $formId -EventName "onsave" -FunctionName "ValidateData" -LibraryName "new_/scripts/validation.js" -Enabled $false
```

Adds a handler but leaves it disabled for later activation.

### Example 7: Update an existing handler
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormEventHandler -FormId $formId -EventName "onload" -FunctionName "ExistingFunction" -LibraryName "new_/scripts/main.js" -PassExecutionContext $false
```

Updates an existing handler's PassExecutionContext setting.

### Example 8: Add multiple attribute handlers
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $attributes = @("firstname", "lastname", "emailaddress1")
PS C:\> foreach ($attr in $attributes) {
PS C:\>     Set-DataverseFormEventHandler -FormId $formId `
PS C:\>         -AttributeName $attr -EventName "onchange" `
PS C:\>         -FunctionName "ValidateField" -LibraryName "new_/scripts/validation.js"
PS C:\> }
```

Adds onchange handlers for multiple attributes using the same validation function.

### Example 9: Add a custom event handler
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormEventHandler -FormId $formId -EventName "mycustomevent" -FunctionName "CustomHandler" -LibraryName "new_/scripts/custom.js" -AllowCustomEventNames
```

Adds a handler for a custom event type using the `-AllowCustomEventNames` switch to bypass validation. Event names are automatically converted to lowercase.

## PARAMETERS

### -Active
Whether the event is active. Default is true.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: True
Accept pipeline input: False
Accept wildcard characters: False
```

### -AllowCustomEventNames
Allow custom event names without validation

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

### -Application
Whether the event is application-managed. Default is false.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -AttributeName
The attribute name for attribute-level events. Creates an event with the attribute property at the form root.

```yaml
Type: String
Parameter Sets: AttributeEvent
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Connection
The Dataverse connection to use.

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
The control ID for control-level events.

```yaml
Type: String
Parameter Sets: ControlEvent
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Enabled
Whether the handler is enabled. Default is true.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: True
Accept pipeline input: False
Accept wildcard characters: False
```

### -EventName
The name of the event (e.g., onload, onsave, onchange, tabstatechange).

Event names are validated against known types and automatically converted to lowercase. Valid event names include: onload, onsave, onchange, tabstatechange, onprestagechange, onpreprocessstatuschange, onprocessstatuschange, onstagechange, onstageselected, onreadystatechange, onresourcemodelload.

Use `-AllowCustomEventNames` to bypass validation for custom event types.

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

### -FormId
The ID of the form.

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

### -FunctionName
The name of the JavaScript function to call.

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

### -HandlerUniqueId
Optional unique ID for the handler. If not specified, a new GUID is generated.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LibraryName
The name of the web resource library containing the function.

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

### -Parameters
Parameters to pass to the function (as a string, typically JSON).

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

### -PassExecutionContext
Whether to pass the execution context to the function. Default is true.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: True
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
{{ Fill ProgressAction Description }}

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
The section name containing the control (required for control events).

```yaml
Type: String
Parameter Sets: ControlEvent
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TabName
The tab name for tab-level events (standalone) or containing the control (for control-level events).

```yaml
Type: String
Parameter Sets: TabEvent, ControlEvent
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

### System.Management.Automation.PSObject
## NOTES
- The web resource must exist (published or unpublished) before adding a handler.
- If a handler with the same function name and library already exists, it will be updated.
- **Event Location Types:**
  1. **Form-level**: Events at form root without an attribute property
  2. **Attribute-level**: Events at form root with an attribute property (adds `attribute="name"` to the event element)
  3. **Tab-level**: Events within a tab element (adds event inside the tab's events collection)
  4. **Control-level**: Events within a control element

## RELATED LINKS

[Get-DataverseFormEventHandler](Get-DataverseFormEventHandler.md)
[Remove-DataverseFormEventHandler](Remove-DataverseFormEventHandler.md)
[Set-DataverseFormLibrary](Set-DataverseFormLibrary.md)
