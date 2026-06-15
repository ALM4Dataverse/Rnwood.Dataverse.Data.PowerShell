---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseFormEventHandler

## SYNOPSIS
Retrieves event handlers from a Dataverse form (form-level, attribute-level, tab-level, or control-level events).

## SYNTAX

### FormEvent (Default)
```
Get-DataverseFormEventHandler -FormId <Guid> [-EventName <String>] [-HandlerUniqueId <Guid>] [-Published]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### AttributeEvent
```
Get-DataverseFormEventHandler -FormId <Guid> [-EventName <String>] -AttributeName <String>
 [-HandlerUniqueId <Guid>] [-Published] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### TabEvent
```
Get-DataverseFormEventHandler -FormId <Guid> [-EventName <String>] -TabName <String> [-HandlerUniqueId <Guid>]
 [-Published] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### ControlEvent
```
Get-DataverseFormEventHandler -FormId <Guid> [-EventName <String>] -TabName <String> -ControlId <String>
 -SectionName <String> [-HandlerUniqueId <Guid>] [-Published] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### AllEvents
```
Get-DataverseFormEventHandler -FormId <Guid> [-EventName <String>] [-All] [-HandlerUniqueId <Guid>]
 [-Published] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The Get-DataverseFormEventHandler cmdlet retrieves JavaScript event handlers from a Dataverse form. It supports four event location types:
- **Form-level events**: Events at the form root (e.g., onload, onsave)
- **Attribute-level events**: Events with an attribute property (e.g., onchange for specific fields)
- **Tab-level events**: Events within a tab element (e.g., tabstatechange)
- **Control-level events**: Events within a control element (e.g., onchange)

The cmdlet returns event handlers with their location, function name, library name, and other properties. Handlers can be filtered by event name, handler unique ID, or location.

## EXAMPLES

### Example 1: List ALL event handlers from all locations
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseFormEventHandler -FormId $formId
```

Returns all event handlers from the form, including form-level, attribute-level, tab-level, and control-level events. This is the default behavior when called with only the FormId parameter.

### Example 2: Get all form-level event handlers
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseFormEventHandler -FormId $formId -EventName "onload"
```

Retrieves all form-level handlers for the onload event. Note that this excludes attribute-level events even though they are at the form root.

### Example 3: Get attribute-level event handlers
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseFormEventHandler -FormId $formId -AttributeName "department"
```

Retrieves all event handlers for the "department" attribute. These are typically onchange events.

### Example 4: Get tab-level event handlers
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseFormEventHandler -FormId $formId -TabName "General"
```

Retrieves all event handlers for the "General" tab. These are typically tabstatechange events.

### Example 5: Get control-level event handlers
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseFormEventHandler -FormId $formId -ControlId "firstname" -TabName "general" -SectionName "name"
```

Retrieves event handlers for a specific control.

### Example 6: Get a specific handler by unique ID
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $handlerId = 'a1b2c3d4-e5f6-4789-abcd-ef0123456789'
PS C:\> Get-DataverseFormEventHandler -FormId $formId -HandlerUniqueId $handlerId
```

Retrieves a specific handler by its unique identifier.

### Example 7: List all event handlers across all locations
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Get form-level events
PS C:\> $formEvents = Get-DataverseFormEventHandler -FormId $formId
PS C:\> Write-Host "Form events: $($formEvents.Count)"
PS C:\>
PS C:\> # Get all controls and their events
PS C:\> $controls = Get-DataverseFormControl -FormId $formId
PS C:\> foreach ($control in $controls) {
PS C:\>     $handlers = Get-DataverseFormEventHandler -FormId $formId `
PS C:\>         -ControlId $control.Id -TabName $control.TabName -SectionName $control.SectionName `
PS C:\>         -ErrorAction SilentlyContinue
PS C:\>     if ($handlers) {
PS C:\>         Write-Host "Control $($control.Id): $($handlers.Count) handlers"
PS C:\>     }
PS C:\> }
```

Lists all event handlers across all controls on a form.

### Example 8: Verify Attribute property in output
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $handlers = Get-DataverseFormEventHandler -FormId $formId -AttributeName "emailaddress1"
PS C:\> foreach ($handler in $handlers) {
PS C:\>     Write-Host "Event: $($handler.EventName), Attribute: $($handler.Attribute), Function: $($handler.FunctionName)"
PS C:\> }
```

Displays the Attribute property which identifies attribute-level events.

### Example 9: Get event handlers from published form only
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseFormEventHandler -FormId $formId -EventName "onload" -Published
```

Retrieves event handlers from the published version of the form only. By default, the cmdlet retrieves from the unpublished (draft) version which includes all recent changes.

## PARAMETERS

### -All
List all event handlers from all locations

```yaml
Type: SwitchParameter
Parameter Sets: AllEvents
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -AttributeName
The attribute name for retrieving attribute-level events. Used for events that have an attribute property in the FormXml.

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
The control ID for retrieving control-level events. Requires TabName and SectionName.

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

### -EventName
Filter handlers by event name (e.g., onload, onsave, onchange, tabstatechange).

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
The ID of the form to retrieve handlers from.

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

### -HandlerUniqueId
Filter by a specific handler's unique ID.

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

### -Published
Retrieve only the published version of the form (default is unpublished)

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

### -SectionName
The section name containing the control (required when ControlId is specified).

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Guid
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES

**Event Location Types:**
1. **Form-level**: Events at form root without an attribute property (e.g., `<event name="onsave">`)
2. **Attribute-level**: Events at form root with an attribute property (e.g., `<event name="onchange" attribute="department">`)
3. **Tab-level**: Events within a tab element (e.g., `<event name="tabstatechange">` inside `<tab>`)
4. **Control-level**: Events within a control element (e.g., `<event name="onchange">` inside `<control>`)

Form-level queries automatically exclude attribute-level events even though both are at the form root level.

## RELATED LINKS

[Set-DataverseFormEventHandler](Set-DataverseFormEventHandler.md)
[Remove-DataverseFormEventHandler](Remove-DataverseFormEventHandler.md)
[Get-DataverseFormLibrary](Get-DataverseFormLibrary.md)
