---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseAppModuleComponent

## SYNOPSIS
Creates or updates an app module component in Dataverse.

## SYNTAX

```
Set-DataverseAppModuleComponent [-Id <Guid>] [-AppModuleId <Guid>] [-AppModuleUniqueName <String>]
 [-ObjectId <Guid>] [-ComponentType <AppModuleComponentType>] [-RootComponentBehavior <RootComponentBehavior>]
 [-IsDefault <Boolean>] [-IsMetadata <Boolean>] [-NoUpdate] [-NoCreate] [-PassThru]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Set-DataverseAppModuleComponent cmdlet adds or updates components in a model-driven app. Components can be entities, views, business process flows, charts, forms, sitemaps, and other app elements.

If a component with the specified ID exists, it will be updated; otherwise, a new component is created using the AddAppComponentsRequest API.

## EXAMPLES

### Example 1: Add an entity to an app module by ID
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $app = Get-DataverseAppModule -UniqueName "myapp"
PS C:\> $entityMetadata = Get-DataverseEntityMetadata -EntityName "contact"
PS C:\> Set-DataverseAppModuleComponent -PassThru `
    -AppModuleId $app.Id `
    -ObjectId $entityMetadata.MetadataId `
    -ComponentType Entity
```

Adds the contact entity to an app module using the app module ID. (Parameter corrected: uses -AppModuleId)

### Example 2: Add an entity to an app module by unique name
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $entityMetadata = Get-DataverseEntityMetadata -EntityName "account"
PS C:\> Set-DataverseAppModuleComponent -PassThru `
    -AppModuleUniqueName "myapp" `
    -ObjectId $entityMetadata.MetadataId `
    -ComponentType Entity
```

Adds the account entity to an app module using the app module's unique name.

### Example 3: Add a form component
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $form = Get-DataverseRecord -TableName systemform -FilterValues @{ name = "Contact Main Form" }
PS C:\> Set-DataverseAppModuleComponent -PassThru `
    -AppModuleUniqueName "myapp" `
    -ObjectId $form.systemformid `
    -ComponentType Form `
    -IsDefault $true
```

Adds a form as the default form for the app.

### Example 4: Add a view component
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $view = Get-DataverseView -TableName contact -Name "Active Contacts"
PS C:\> Set-DataverseAppModuleComponent -PassThru `
    -AppModuleId $appId `
    -ObjectId $view.Id `
    -ComponentType View `
    -RootComponentBehavior IncludeSubcomponents
```

Adds a view with subcomponents included.

### Example 5: Update component properties
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAppModuleComponent `
    -Id $componentId `
    -IsDefault $true `
    -RootComponentBehavior DoNotIncludeSubcomponents
```

Updates an existing component to be the default with DoNotIncludeSubcomponents behavior.

### Example 6: Add multiple components via pipeline
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $entities = @("contact", "account", "lead") | ForEach-Object {
    $metadata = Get-DataverseEntityMetadata -EntityName $_
    [PSCustomObject]@{
        AppModuleUniqueName = "salesapp"
        ObjectId = $metadata.MetadataId
        ComponentType = [Rnwood.Dataverse.Data.PowerShell.Commands.Model.AppModuleComponentType]::Entity
    }
}
PS C:\> $entities | Set-DataverseAppModuleComponent
```

Adds multiple entities to an app module via pipeline input.

### Example 7: Add component to app by unique name and then update
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $entity = Get-DataverseEntityMetadata -EntityName "contact"
PS C:\> $compId = Set-DataverseAppModuleComponent -PassThru -AppModuleUniqueName "myapp" -ObjectId $entity.MetadataId -ComponentType Entity
PS C:\> Set-DataverseAppModuleComponent -Id $compId -IsDefault $true
```

Creates a component referencing the app by unique name, then sets it as default.

## PARAMETERS

### -AppModuleId
App module ID that this component belongs to.
Required when creating a new component if AppModuleUniqueName is not specified.

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
Unique name of the app module that this component belongs to. If specified, takes precedence over AppModuleId.

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
Component type (Entity, View, BusinessProcessFlow, RibbonCommand, Chart, Form, SiteMap).
Required when creating a new component.

```yaml
Type: AppModuleComponentType
Parameter Sets: (All)
Aliases:
Accepted values: Entity, View, BusinessProcessFlow, RibbonCommand, Chart, Form, SiteMap

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
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
ID of the app module component to update.
If not specified or if the component doesn't exist, a new component is created.

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

### -IsDefault
Whether this is the default component

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -IsMetadata
Whether this is a metadata component

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -NoCreate
If specified, then no component will be created even if no existing component matching the ID is found

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

### -NoUpdate
If specified, existing components matching the ID will not be updated

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

### -ObjectId
Object ID (the ID of the component entity record).
Required when creating a new component.

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

### -PassThru
If specified, returns the ID of the created or updated component

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

### -RootComponentBehavior
Root component behavior (IncludeSubcomponents, DoNotIncludeSubcomponents, IncludeAsShell)

```yaml
Type: RootComponentBehavior
Parameter Sets: (All)
Aliases:
Accepted values: IncludeSubcomponents, DoNotIncludeSubcomponents, IncludeAsShell

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
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
### System.String
### System.Nullable`1[[Rnwood.Dataverse.Data.PowerShell.Commands.Model.AppModuleComponentType, Rnwood.Dataverse.Data.PowerShell.Cmdlets, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]
### System.Nullable`1[[Rnwood.Dataverse.Data.PowerShell.Commands.Model.RootComponentBehavior, Rnwood.Dataverse.Data.PowerShell.Cmdlets, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]
### System.Nullable`1[[System.Boolean, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]
## OUTPUTS

### System.Guid
## NOTES

**Required Parameters for Creation:**
- AppModuleId or AppModuleUniqueName: Identifies the app module to add the component to
- ObjectId: The ID of the component entity (entity metadata ID, form ID, etc.)
- ComponentType: The type of component (see values below)

**Component Types:**
- Entity (1): Table/entity components
- View (26): View components  
- BusinessProcessFlow (29): Business process flow components
- RibbonCommand (48): Ribbon command components
- Chart (59): Chart/visualization components
- Form (60): Form components
- SiteMap (62): Site map components

**Root Component Behavior:**
- IncludeSubcomponents (0): Include all subcomponents (default)
- DoNotIncludeSubcomponents (1): Include only the main component
- IncludeAsShell (2): Include as shell component only

**Upsert Behavior:**
- If ID is provided and exists: updates that component
- If ID not found or not provided: creates new component using AddAppComponentsRequest
- AppModuleUniqueName takes precedence over AppModuleId when both are provided

**Control Flags:**
- NoUpdate: Prevents updating existing components (returns ID if found, does nothing if not found)
- NoCreate: Prevents creating new components (updates if found, does nothing if not found)
- PassThru: Returns the ID of the created or updated component

**Best Practices:**
- Use Get-DataverseEntityMetadata to get entity MetadataId for ObjectId when adding entities
- Use Get-DataverseView, Get-DataverseRecord for forms, etc. to get IDs for other components
- Set IsDefault for the primary component of each type in an app
- Use RootComponentBehavior to control sub-component inclusion
- Use AppModuleUniqueName for cross-environment compatibility

## RELATED LINKS

[Get-DataverseAppModuleComponent](Get-DataverseAppModuleComponent.md)

[Remove-DataverseAppModuleComponent](Remove-DataverseAppModuleComponent.md)

[Get-DataverseAppModule](Get-DataverseAppModule.md)

[Set-DataverseAppModule](Set-DataverseAppModule.md)