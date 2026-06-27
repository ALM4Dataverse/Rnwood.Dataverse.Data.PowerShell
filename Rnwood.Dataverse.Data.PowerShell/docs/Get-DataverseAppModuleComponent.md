---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseAppModuleComponent

## SYNOPSIS
Retrieves app module component information from a Dataverse environment.

## SYNTAX

```
Get-DataverseAppModuleComponent [[-Id] <Guid>] [-AppModuleId <Guid>] [-AppModuleUniqueName <String>]
 [-ObjectId <Guid>] [-ComponentType <AppModuleComponentType>] [-Raw] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The Get-DataverseAppModuleComponent cmdlet retrieves components that belong to a model-driven app. Components define what entities, dashboards, business process flows, and other elements are included in an app.

By default, the cmdlet returns parsed component information with key properties. Use the -Raw parameter to return all attribute values from the appmodulecomponent record.

Components can be filtered by ID, app module ID, app module unique name, object ID, or component type.

## EXAMPLES

### Example 1: Get all components for an app module by ID
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $app = Get-DataverseAppModule -UniqueName "myapp"
PS C:\> Get-DataverseAppModuleComponent -AppModuleId $app.Id
```

Retrieves all components associated with a specific app module using its ID.

---

### Example 2: Get all components for an app module by unique name
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseAppModuleComponent -AppModuleUniqueName "myapp"
```

Retrieves all components associated with a specific app module using its unique name.

---

### Example 3: Get a specific component by ID
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseAppModuleComponent -Id "12345678-1234-1234-1234-123456789012"
```

Retrieves a specific component by its ID.

---

### Example 4: Get components by component type
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseAppModuleComponent -AppModuleUniqueName "myapp" -ComponentType Entity
```

Retrieves all entity components for an app module.

---

### Example 5: Get components for a specific entity
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $entityMetadata = Get-DataverseEntityMetadata -EntityName "contact"
PS C:\> Get-DataverseAppModuleComponent -ObjectId $entityMetadata.MetadataId
```

Finds which app modules include a specific entity.

---

### Example 6: Get raw component data
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseAppModuleComponent -AppModuleId $appId -Raw
```

Retrieves components with all raw attribute values instead of parsed properties.

## PARAMETERS

### -AppModuleId
Filter components by app module ID.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -AppModuleUniqueName
Filter components by app module unique name.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -ComponentType
Filter components by component type (Entity, View, BusinessProcessFlow, RibbonCommand, Chart, Form, SiteMap)

```yaml
Type: AppModuleComponentType
Parameter Sets: (All)
Aliases:
Accepted values: Entity, View, BusinessProcessFlow, RibbonCommand, Chart, Form, SiteMap

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

### -Id
The ID of the app module component to retrieve.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -ObjectId
Filter components by object ID (the ID of the component entity record).

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
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

### -Raw
Return raw values instead of display values

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Guid
### System.String
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES

**Component Types:**
- Entity (1): Table/entity components
- View (26): View components  
- BusinessProcessFlow (29): Business process flow components
- RibbonCommand (48): Ribbon command components
- Chart (59): Chart/visualization components
- Form (60): Form components
- SiteMap (62): Site map components

**Default Behavior:**
- Returns parsed component information with key properties
- Automatically pages through results if multiple components match criteria
- Supports filtering by app module ID, unique name, object ID, or component type

**Filtering:**
- Filter by AppModuleId or AppModuleUniqueName to get all components for an app
- Filter by ObjectId to find which apps use a specific component
- Filter by ComponentType to get components of a specific type
- Combine filters for more precise queries

**Common Properties:**
- Id: The unique identifier of the component
- AppModuleId: The ID of the app module this component belongs to
- ObjectId: The ID of the underlying component (entity, form, etc.)
- ComponentType: The type of component as an enum value
- RootComponentBehavior: How subcomponents are included
- IsDefault: Whether this is the default component of its type
- IsMetadata: Whether this is a metadata component

**Common Use Cases:**
- Auditing what components are included in an app
- Finding which apps use a specific entity or form
- Validating app configuration
- Cloning app structures across environments

## RELATED LINKS

[Set-DataverseAppModuleComponent](Set-DataverseAppModuleComponent.md)

[Remove-DataverseAppModuleComponent](Remove-DataverseAppModuleComponent.md)

[Get-DataverseAppModule](Get-DataverseAppModule.md)