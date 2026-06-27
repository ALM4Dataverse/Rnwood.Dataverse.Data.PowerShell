---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseAppModuleComponent

## SYNOPSIS
Removes an app module component from Dataverse.

## SYNTAX

### ById
```
Remove-DataverseAppModuleComponent -Id <Guid> [-IfExists] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### ByAppModuleUniqueName
```
Remove-DataverseAppModuleComponent -AppModuleUniqueName <String> -ObjectId <Guid> [-IfExists]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### ByAppModuleId
```
Remove-DataverseAppModuleComponent -AppModuleId <Guid> -ObjectId <Guid> [-IfExists]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Remove-DataverseAppModuleComponent cmdlet removes a component from a model-driven app. This removes the relationship between the app and the component (entity, form, view, etc.) but does not delete the underlying component itself.

The cmdlet supports three ways to identify the component to remove:
1. By component ID directly (fastest)
2. By app module unique name and object ID (cross-environment friendly)
3. By app module ID and object ID

Use the IfExists parameter to suppress errors when attempting to remove components that don't exist.

## EXAMPLES

### Example 1: Remove a component by ID
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $component = Get-DataverseAppModuleComponent -AppModuleUniqueName "myapp" | Where-Object { $_.ObjectId -eq $entityId }
PS C:\> Remove-DataverseAppModuleComponent -Id $component.Id -Confirm:$false
```

Finds and removes a specific component from an app module using the component ID.

### Example 2: Remove component by app module unique name
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $entityMetadata = Get-DataverseEntityMetadata -EntityName "contact"
PS C:\> Remove-DataverseAppModuleComponent -AppModuleUniqueName "myapp" -ObjectId $entityMetadata.MetadataId -Confirm:$false
```

Removes the contact entity from an app module by specifying the app's unique name and entity metadata ID.

### Example 3: Remove component by app module ID
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $app = Get-DataverseAppModule -UniqueName "myapp"
PS C:\> $form = Get-DataverseRecord -TableName systemform -FilterValues @{ name = "Contact Form" }
PS C:\> Remove-DataverseAppModuleComponent -AppModuleId $app.Id -ObjectId $form.systemformid -Confirm:$false
```

Removes a form from an app module by specifying the app module ID and form ID.

### Example 4: Remove with IfExists flag
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseAppModuleComponent -Id $componentId -IfExists -Confirm:$false
```

Attempts to remove a component but doesn't error if it doesn't exist.

### Example 5: Remove all components of a type
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseAppModuleComponent -AppModuleUniqueName "myapp" -ComponentType Chart |
    Remove-DataverseAppModuleComponent -Confirm:$false
```

Removes all chart components from an app module using pipeline input with the ById parameter set.

### Example 6: Use WhatIf to preview
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseAppModuleComponent -AppModuleUniqueName "myapp" -ObjectId $entityId -WhatIf
```

Shows what would happen without actually removing the component.

### Example 7: Remove multiple components safely
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $componentsToRemove = @(
    @{ AppModuleUniqueName = "myapp"; ObjectId = $entity1Id },
    @{ AppModuleUniqueName = "myapp"; ObjectId = $entity2Id },
    @{ AppModuleUniqueName = "myapp"; ObjectId = $formId }
)
PS C:\> $componentsToRemove | ForEach-Object {
    Remove-DataverseAppModuleComponent -AppModuleUniqueName $_.AppModuleUniqueName -ObjectId $_.ObjectId -IfExists -Confirm:$false
}
```

Removes multiple components safely with error suppression.

## PARAMETERS

### -AppModuleId
ID of the app module containing the component to remove.

```yaml
Type: Guid
Parameter Sets: ByAppModuleId
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -AppModuleUniqueName
Unique name of the app module containing the component to remove.

```yaml
Type: String
Parameter Sets: ByAppModuleUniqueName
Aliases:

Required: True
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
ID of the app module component to remove

```yaml
Type: Guid
Parameter Sets: ById
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -IfExists
If specified, the cmdlet will not raise an error if the component does not exist

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
Object ID of the component (entity metadata ID, form ID, view ID, etc.)

```yaml
Type: Guid
Parameter Sets: ByAppModuleUniqueName, ByAppModuleId
Aliases:

Required: True
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
## OUTPUTS

### System.Object
## NOTES

**Parameter Sets:**
1. **ById** (Default): Remove component by its direct ID - fastest method when you have the component ID
2. **ByAppModuleUniqueName**: Remove component by app unique name and object ID - best for cross-environment scripts
3. **ByAppModuleId**: Remove component by app module ID and object ID - useful when you have the app ID

**Behavior:**
- Removes the component relationship from the app using RemoveAppComponentsRequest
- Does NOT delete the underlying component (entity, form, view, etc.)
- Component can be added back to the app later
- Uses both published and unpublished queries to find app modules

**IfExists Flag:**
- Suppresses errors when component doesn't exist
- Useful for cleanup scripts and idempotent operations
- Reports via WriteVerbose when components are not found

**Confirmation:**
- Prompts for confirmation by default (ConfirmImpact = Medium)
- Use -Confirm:$false to skip confirmation
- Use -WhatIf to preview without executing

**Cross-Environment Considerations:**
- Use AppModuleUniqueName parameter set for scripts that run across environments
- Object IDs (entity metadata IDs, form IDs) may differ between environments
- App module unique names are consistent across environments

**Use Cases:**
- Removing entities from an app
- Cleaning up unused forms or views
- Reorganizing app structure
- Automated app configuration management
- Environment-specific component cleanup

## RELATED LINKS

[Get-DataverseAppModuleComponent](Get-DataverseAppModuleComponent.md)

[Set-DataverseAppModuleComponent](Set-DataverseAppModuleComponent.md)

[Get-DataverseAppModule](Get-DataverseAppModule.md)

[Remove-DataverseAppModule](Remove-DataverseAppModule.md)