---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseSolutionComponent

## SYNOPSIS
Adds or updates a solution component in an unmanaged solution, with automatic handling of behavior changes.

## SYNTAX

```
Set-DataverseSolutionComponent [-SolutionName] <String> [-ComponentId] <Guid> [-ComponentType] <Int32>
 [-Behavior <Int32>] [-AddRequiredComponents] [-DoNotIncludeSubcomponents] [-PassThru]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet adds or updates a solution component in an unmanaged Dataverse solution. 

**Key behavior:** Dataverse does not allow updating the "root component behavior" directly. When you need to change the behavior (e.g., from "Include Subcomponents" to "Do Not Include Subcomponents"), this cmdlet automatically:
1. Removes the component from the solution
2. Re-adds it with the new behavior

If the component doesn't exist in the solution, it is simply added. If it exists with the same behavior, no action is taken (idempotent operation).

**Behavior values:**
- 0 = Include Subcomponents (default) - Includes all subcomponents like attributes, forms, views
- 1 = Do Not Include Subcomponents - Includes only the root component
- 2 = Include As Shell - Includes the component shell without subcomponents

## EXAMPLES

### Example 1: Add a table (entity) to a solution
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $connection = Get-DataverseConnection -Url "https://contoso.crm.dynamics.com" -Interactive
PS C:\> Set-DataverseSolutionComponent -SolutionName "MySolution" `
    -ComponentId "dfe12c85-55b3-4c77-9c04-7d5d06d2e9e2" -ComponentType 1 -Behavior 0
```

Adds an entity (ComponentType 1) to "MySolution" with behavior "Include Subcomponents" (0).

### Example 2: Change component behavior from Include to Do Not Include Subcomponents
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseSolutionComponent -SolutionName "MySolution" `
    -ComponentId "dfe12c85-55b3-4c77-9c04-7d5d06d2e9e2" -ComponentType 1 -Behavior 1 -PassThru

SolutionName  : MySolution
SolutionId    : a1b2c3d4-5678-90ab-cdef-1234567890ab
ComponentId   : dfe12c85-55b3-4c77-9c04-7d5d06d2e9e2
ComponentType : 1
Behavior      : Do Not Include Subcomponents
BehaviorValue : 1
WasUpdated    : True
```

Changes the behavior of an existing component. The cmdlet removes and re-adds the component with the new behavior, returning details via PassThru.

### Example 3: Add a component and include required dependencies
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseSolutionComponent -SolutionName "MySolution" `
    -ComponentId "8a9b7c6d-5e4f-3a2b-1c0d-9e8f7a6b5c4d" -ComponentType 26 `
    -AddRequiredComponents -Behavior 0
```

Adds a view (ComponentType 26) and automatically includes any required components (like the parent entity).

### Example 4: Preview changes with WhatIf
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseSolutionComponent -SolutionName "MySolution" `
    -ComponentId "dfe12c85-55b3-4c77-9c04-7d5d06d2e9e2" -ComponentType 1 -Behavior 2 -WhatIf

What if: Performing the operation "Add to solution 'MySolution' with behavior: Include As Shell" 
on target "Component dfe12c85-55b3-4c77-9c04-7d5d06d2e9e2 (type 1)".
```

Shows what would happen without making changes.

### Example 5: Add multiple components from pipeline
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $components = @(
    @{ ComponentId = "guid1"; ComponentType = 1 }
    @{ ComponentId = "guid2"; ComponentType = 26 }
    @{ ComponentId = "guid3"; ComponentType = 60 }
)
PS C:\> $components | ForEach-Object { 
    Set-DataverseSolutionComponent -SolutionName "MySolution" `
        -ComponentId $_.ComponentId -ComponentType $_.ComponentType -Behavior 0
}
```

Adds multiple components to a solution using pipeline input.

## PARAMETERS

### -AddRequiredComponents
Indicates whether other solution components that are required by the component being added should also be added to the solution.

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

### -Behavior
The root component behavior:
- 0 = Include Subcomponents (default) - Includes all subcomponents
- 1 = Do Not Include Subcomponents - Includes only the root component
- 2 = Include As Shell - **Note:** This behavior is not fully supported by the underlying Dataverse API. The component will be added with "Do Not Include Subcomponents" behavior instead, and a warning will be displayed.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -ComponentId
The ID (GUID) of the solution component to add or update.

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
- 25: Organization
- 26: View
- 29: WebResource
- 31: Report
- 60: Chart
- 80: Process (Workflow)

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

### -DoNotIncludeSubcomponents
Indicates whether the subcomponents should be excluded when adding the component.

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

### -PassThru
If specified, outputs information about the component operation to the pipeline.

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
The unique name of the solution to add or update the component in.

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

### System.Management.Automation.PSObject
## NOTES

**Important Behavior Details:**
- This cmdlet only works with unmanaged solutions
- When changing behavior, the component is removed and re-added automatically
- If the component doesn't exist, it is added with the specified behavior
- If the component exists with the same behavior, no action is taken (idempotent)
- The cmdlet supports -WhatIf to preview changes before making them
- **Behavior 2 (Include As Shell) Limitation:** The Dataverse AddSolutionComponent API does not directly support setting behavior to "Include As Shell" (value 2). When this value is specified, the cmdlet will add the component with "Do Not Include Subcomponents" behavior and display a warning. To set "Include As Shell" behavior, you may need to use the Dataverse UI or make direct API calls.

**Component Types Reference:**
See Microsoft documentation for a complete list of solution component types:
https://learn.microsoft.com/en-us/power-apps/developer/data-platform/solution-component-file

## RELATED LINKS

[Get-DataverseSolutionComponent](Get-DataverseSolutionComponent.md)
[Invoke-DataverseAddSolutionComponent](Invoke-DataverseAddSolutionComponent.md)
[Invoke-DataverseRemoveSolutionComponent](Invoke-DataverseRemoveSolutionComponent.md)
