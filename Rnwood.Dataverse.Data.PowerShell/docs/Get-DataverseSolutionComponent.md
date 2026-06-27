---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseSolutionComponent

## SYNOPSIS
Retrieves the components of a solution from a Dataverse environment.

## SYNTAX

### ByUniqueName
```
Get-DataverseSolutionComponent [-SolutionName] <String> [-IncludeImpliedSubcomponents]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### BySolutionId
```
Get-DataverseSolutionComponent [-SolutionId] <Guid> [-IncludeImpliedSubcomponents]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The Get-DataverseSolutionComponent cmdlet retrieves the root components of a solution from a Dataverse environment. It can retrieve components by solution unique name or solution ID.

When the IncludeImpliedSubcomponents parameter is specified, it also retrieves subcomponents for entity components (type 1), such as attributes, relationships, forms, and views that are implied if the whole entity has been added to the solution.

The cmdlet outputs PowerShell objects representing each component with properties including ObjectId, ComponentType, ComponentTypeName, Behavior, MetadataId, and IsSubcomponent.

## EXAMPLES

### Example 1: Get components by solution name
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive
PS C:\> Get-DataverseSolutionComponent -SolutionName "MySolution"
```

This example retrieves all root components from the solution named "MySolution".

### Example 2: Get components by solution ID
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive
PS C:\> Get-DataverseSolutionComponent -SolutionId "12345678-1234-1234-1234-123456789012"
```

This example retrieves all root components from the solution with the specified ID.

### Example 3: Get components including subcomponents
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive
PS C:\> Get-DataverseSolutionComponent -SolutionName "MySolution" -IncludeImpliedSubcomponents
```

This example retrieves all root components and their subcomponents from the solution named "MySolution".

### Example 4: Filter components by type
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive
PS C:\> $components = Get-DataverseSolutionComponent -SolutionName "MySolution"
PS C:\> $components | Where-Object { $_.ComponentType -eq 1 } | Format-Table
```

This example retrieves all components from the solution and filters to show only entities (ComponentType 1).

### Example 5: Count components by type
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive
PS C:\> $components = Get-DataverseSolutionComponent -SolutionName "MySolution"
PS C:\> $components | Group-Object ComponentTypeName | Select-Object Name, Count | Sort-Object Count -Descending
```

This example retrieves all components and groups them by component type to show a count of each type.

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

### -IncludeImpliedSubcomponents
Include subcomponents (attributes, relationships, forms, views, etc.) implied each root table component if the whole table is included in solution.

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

### -SolutionId
The ID of the solution.

```yaml
Type: Guid
Parameter Sets: BySolutionId
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -SolutionName
The unique name of the solution.

```yaml
Type: String
Parameter Sets: ByUniqueName
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String
### System.Guid
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES
- **Component Types**: The cmdlet returns components with numeric ComponentType values. Common types include:
  - 1: Entity
  - 2: Attribute
  - 10: Entity Relationship
  - 24: Form
  - 26: Saved Query (View)
  - 60: System Form
  - 61: Web Resource
  - 62: Site Map
  - Full list available at: https://learn.microsoft.com/en-us/power-apps/developer/data-platform/reference/entities/solutioncomponent

- **Behavior Values**: The Behavior property indicates how the component is included in the solution:
  - Include Subcomponents (0): Full component with all subcomponents
  - Do Not Include Subcomponents (1): Component without subcomponents
  - Include As Shell (2): Shell only, no subcomponents

- **Subcomponents**: When IncludeImpliedSubcomponents is used, only entity subcomponents are retrieved. Subcomponents have IsSubcomponent set to true and include ParentComponentType and ParentObjectId properties.

- **Performance**: Retrieving subcomponents can significantly increase the number of results and processing time for solutions with many entities.

## RELATED LINKS
