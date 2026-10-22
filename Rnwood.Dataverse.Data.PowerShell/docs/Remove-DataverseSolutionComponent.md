---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseSolutionComponent

## SYNOPSIS
Removes a solution component from an unmanaged solution.

## SYNTAX

```
Remove-DataverseSolutionComponent [-SolutionName] <String> [-ComponentId] <Guid> [-ComponentType] <Int32>
 [-IfExists] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

## DESCRIPTION

This cmdlet removes a solution component from an unmanaged Dataverse solution.

**Important Notes:**
- This cmdlet only works with unmanaged solutions
- The component itself is not deleted from the environment, only its association with the solution is removed
- If you need to delete the component entirely (e.g., delete an entity or attribute), use the appropriate Remove-Dataverse*Metadata cmdlet
- Use with caution as removing components may affect solution dependencies

## EXAMPLES

### Example 1: Remove an entity component from a solution
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $connection = Get-DataverseConnection -Url "https://contoso.crm.dynamics.com" -Interactive
PS C:\> $entityMetadata = Get-DataverseEntityMetadata -EntityName "new_customtable"
PS C:\> Remove-DataverseSolutionComponent -SolutionName "MySolution" `
    -ComponentId $entityMetadata.MetadataId -ComponentType 1
```

Removes an entity component from the solution. The entity itself remains in the environment.

### Example 2: Remove an attribute component
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $attributeMetadata = Get-DataverseAttributeMetadata `
    -EntityName "account" -AttributeName "new_customfield"
PS C:\> Remove-DataverseSolutionComponent -SolutionName "MySolution" `
    -ComponentId $attributeMetadata.MetadataId -ComponentType 2 -Confirm:$false
```

Removes an attribute component from the solution without confirmation prompt.

### Example 3: Remove multiple components from pipeline
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $components = Get-DataverseSolutionComponent -SolutionName "MySolution"
PS C:\> $components | Where-Object { $_.ComponentType -eq 26 } | ForEach-Object {
    Remove-DataverseSolutionComponent -SolutionName "MySolution" `
        -ComponentId $_.ObjectId -ComponentType $_.ComponentType -Confirm:$false
}
```

Removes all view components (type 26) from a solution.

### Example 4: Use IfExists to avoid errors
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseSolutionComponent -SolutionName "MySolution" `
    -ComponentId $componentId -ComponentType 1 -IfExists
```

Removes the component if it exists in the solution, or does nothing if it doesn't exist (no error thrown).

### Example 5: Preview with WhatIf
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseSolutionComponent -SolutionName "MySolution" `
    -ComponentId $componentId -ComponentType 1 -WhatIf

What if: Performing the operation "Remove from solution 'MySolution'" 
on target "Component dfe12c85-55b3-4c77-9c04-7d5d06d2e9e2 (type 1)".
```

Shows what would happen without actually removing the component.

## PARAMETERS

### -ComponentId
The ID (GUID) of the solution component to remove. This is typically the MetadataId for metadata components.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases: ObjectId

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -ComponentType
The type of the solution component. Common values:
- 1: Entity
- 2: Attribute
- 9: OptionSet
- 10: EntityRelationship
- 24: Form
- 26: View
- 29: WebResource
- 60: Chart
- 80: Process (Workflow)

See [Microsoft's documentation](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/solution-component-file) for a complete list.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: True
Position: 2
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Connection
The Dataverse connection to use. If not specified, uses the default connection set by Set-DataverseConnectionAsDefault.

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

### -IfExists
If specified, the cmdlet will not raise an error if the component does not exist in the solution.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
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

### -SolutionName
The unique name of the solution from which to remove the component.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
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
### System.Int32
## OUTPUTS

### System.Object
## NOTES

This cmdlet only removes the component from the solution, it does not delete the component from the environment. Only works with unmanaged solutions (managed solutions are read-only). The component must exist in the solution for the cmdlet to succeed (unless -IfExists is specified). Removing components may affect solution dependencies - ensure you understand the impact before removing. After removing components, consider publishing customizations with Publish-DataverseCustomizations.

Component removal vs deletion: Remove-DataverseSolutionComponent removes component from solution (component remains in environment); Remove-DataverseEntityMetadata deletes the entity from the environment entirely; Remove-DataverseAttributeMetadata deletes the attribute from the environment entirely.

## RELATED LINKS

[Set-DataverseSolutionComponent](Set-DataverseSolutionComponent.md)
[Get-DataverseSolutionComponent](Get-DataverseSolutionComponent.md)
[Remove-DataverseEntityMetadata](Remove-DataverseEntityMetadata.md)
[Remove-DataverseAttributeMetadata](Remove-DataverseAttributeMetadata.md)
