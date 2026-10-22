---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseFormEventHandler

## SYNOPSIS
Removes an event handler from a Dataverse form (form-level, attribute-level, tab-level, or control-level).

## SYNTAX

### FormEventByUniqueId (Default)
```
Remove-DataverseFormEventHandler -FormId <Guid> -EventName <String> -HandlerUniqueId <Guid>
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### AttributeEventByUniqueId
```
Remove-DataverseFormEventHandler -FormId <Guid> -EventName <String> -HandlerUniqueId <Guid>
 -AttributeName <String> [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf]
 [-Confirm] [<CommonParameters>]
```

### TabEventByUniqueId
```
Remove-DataverseFormEventHandler -FormId <Guid> -EventName <String> -HandlerUniqueId <Guid> -TabName <String>
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### ControlEventByUniqueId
```
Remove-DataverseFormEventHandler -FormId <Guid> -EventName <String> -HandlerUniqueId <Guid> -TabName <String>
 -ControlId <String> -SectionName <String> [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [-WhatIf] [-Confirm] [<CommonParameters>]
```

### FormEventByFunction
```
Remove-DataverseFormEventHandler -FormId <Guid> -EventName <String> -FunctionName <String>
 -LibraryName <String> [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

### AttributeEventByFunction
```
Remove-DataverseFormEventHandler -FormId <Guid> -EventName <String> -FunctionName <String>
 -LibraryName <String> -AttributeName <String> [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### TabEventByFunction
```
Remove-DataverseFormEventHandler -FormId <Guid> -EventName <String> -FunctionName <String>
 -LibraryName <String> -TabName <String> [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [-WhatIf] [-Confirm] [<CommonParameters>]
```

### ControlEventByFunction
```
Remove-DataverseFormEventHandler -FormId <Guid> -EventName <String> -FunctionName <String>
 -LibraryName <String> -TabName <String> -ControlId <String> -SectionName <String>
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Remove-DataverseFormEventHandler cmdlet removes JavaScript event handlers from Dataverse forms. It supports four event location types:
- **Form-level events**: Events at the form root
- **Attribute-level events**: Events with an attribute property
- **Tab-level events**: Events within a tab element
- **Control-level events**: Events within a control element

Handlers can be identified by their unique ID or by function name and library name combination.

## EXAMPLES

### Example 1: Remove a form-level handler by unique ID
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $handlerId = 'a1b2c3d4-e5f6-4789-abcd-ef0123456789'
PS C:\> Remove-DataverseFormEventHandler -FormId $formId -EventName "onload" -HandlerUniqueId $handlerId
```

Removes a specific onload handler from the form.

### Example 2: Remove a form-level handler by function and library name
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseFormEventHandler -FormId $formId -EventName "onsave" -FunctionName "ValidateData" -LibraryName "new_/scripts/validation.js"
```

Removes a handler identified by its function and library.

### Example 3: Remove an attribute-level handler
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseFormEventHandler -FormId $formId -AttributeName "department" -EventName "onchange" -FunctionName "OnDepartmentChange" -LibraryName "new_/scripts/validation.js"
```

Removes an onchange handler from the "department" attribute.

### Example 4: Remove a tab-level handler
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseFormEventHandler -FormId $formId -TabName "General" -EventName "tabstatechange" -FunctionName "OnTabChange" -LibraryName "new_/scripts/tabs.js"
```

Removes a tabstatechange handler from the "General" tab.

### Example 5: Remove a control-level handler
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseFormEventHandler -FormId $formId -EventName "onchange" -FunctionName "OnFieldChange" -LibraryName "new_/scripts/main.js" -ControlId "emailaddress1" -TabName "general" -SectionName "contact_info"
```

Removes an onchange handler from a specific control.

### Example 6: Remove all handlers for an event
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $handlers = Get-DataverseFormEventHandler -FormId $formId -EventName "onload"
PS C:\> foreach ($handler in $handlers) {
PS C:\>     Remove-DataverseFormEventHandler -FormId $formId -EventName "onload" -HandlerUniqueId $handler.HandlerUniqueId -Confirm:$false
PS C:\> }
```

Removes all handlers for a specific event.

### Example 7: Clean up attribute handlers for multiple fields
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $attributes = @("firstname", "lastname", "emailaddress1")
PS C:\> foreach ($attr in $attributes) {
PS C:\>     $handlers = Get-DataverseFormEventHandler -FormId $formId -AttributeName $attr -ErrorAction SilentlyContinue
PS C:\>     foreach ($handler in $handlers) {
PS C:\>         Remove-DataverseFormEventHandler -FormId $formId -AttributeName $attr -EventName $handler.EventName -HandlerUniqueId $handler.HandlerUniqueId -Confirm:$false
PS C:\>     }
PS C:\> }
```

Removes all handlers from multiple attributes.

### Example 8: Clean up deprecated handlers
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $allHandlers = Get-DataverseFormEventHandler -FormId $formId
PS C:\> $deprecated = $allHandlers | Where-Object { $_.LibraryName -like "*deprecated*" }
PS C:\> foreach ($handler in $deprecated) {
PS C:\>     if ($handler.Attribute) {
PS C:\>         Remove-DataverseFormEventHandler -FormId $formId -AttributeName $handler.Attribute -EventName $handler.EventName -HandlerUniqueId $handler.HandlerUniqueId -Confirm:$false
PS C:\>     } elseif ($handler.TabName -and -not $handler.ControlId) {
PS C:\>         Remove-DataverseFormEventHandler -FormId $formId -TabName $handler.TabName -EventName $handler.EventName -HandlerUniqueId $handler.HandlerUniqueId -Confirm:$false
PS C:\>     } else {
PS C:\>         Remove-DataverseFormEventHandler -FormId $formId -EventName $handler.EventName -HandlerUniqueId $handler.HandlerUniqueId -Confirm:$false
PS C:\>     }
PS C:\> }
```

Removes all handlers using deprecated libraries across all event types.

## PARAMETERS

### -AttributeName
The attribute name for attribute-level events.

```yaml
Type: String
Parameter Sets: AttributeEventByUniqueId, AttributeEventByFunction
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
Parameter Sets: ControlEventByUniqueId, ControlEventByFunction
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -EventName
The name of the event containing the handler to remove.

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
The function name to identify the handler to remove.

```yaml
Type: String
Parameter Sets: FormEventByFunction, AttributeEventByFunction, TabEventByFunction, ControlEventByFunction
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -HandlerUniqueId
The unique ID of the handler to remove.

```yaml
Type: Guid
Parameter Sets: FormEventByUniqueId, AttributeEventByUniqueId, TabEventByUniqueId, ControlEventByUniqueId
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LibraryName
The library name to identify the handler to remove (required with FunctionName).

```yaml
Type: String
Parameter Sets: FormEventByFunction, AttributeEventByFunction, TabEventByFunction, ControlEventByFunction
Aliases:

Required: True
Position: Named
Default value: None
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
The section name containing the control.

```yaml
Type: String
Parameter Sets: ControlEventByUniqueId, ControlEventByFunction
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
Parameter Sets: TabEventByUniqueId, ControlEventByUniqueId, TabEventByFunction, ControlEventByFunction
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

### System.Object
## NOTES
- After removing the last handler from an event, the empty event element is automatically cleaned up.
- **Event Location Types:**
  1. **Form-level**: Events at form root without an attribute property
  2. **Attribute-level**: Events at form root with an attribute property (requires AttributeName parameter)
  3. **Tab-level**: Events within a tab element (requires TabName parameter without ControlId/SectionName)
  4. **Control-level**: Events within a control element (requires ControlId, TabName, and SectionName parameters)

## RELATED LINKS

[Get-DataverseFormEventHandler](Get-DataverseFormEventHandler.md)
[Set-DataverseFormEventHandler](Set-DataverseFormEventHandler.md)
[Remove-DataverseFormLibrary](Remove-DataverseFormLibrary.md)
