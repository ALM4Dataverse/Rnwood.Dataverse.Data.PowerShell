---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseComponentDependency

## SYNOPSIS
Retrieves component dependencies in Dataverse.

## SYNTAX

### RequiredBy
```
Get-DataverseComponentDependency [-ObjectId] <Guid> [-ComponentType] <Int32> [-RequiredBy]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Dependent
```
Get-DataverseComponentDependency [-ObjectId] <Guid> [-ComponentType] <Int32> [-Dependent]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet retrieves component dependencies in Dataverse. It supports two modes:

**-RequiredBy**: Retrieves dependencies that would prevent a component from being deleted (uses RetrieveDependenciesForDeleteRequest). This shows what other components require the specified component to exist.

**-Dependent**: Retrieves components that depend on a specified component (uses RetrieveDependentComponentsRequest). This shows what components use or reference the specified component.

The cmdlet returns a collection of dependency entities, each describing a dependency relationship between components.

## EXAMPLES

### Example 1: Check dependencies before deleting an entity
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $entityMetadata = Get-DataverseEntityMetadata -EntityName "new_customtable"
PS C:\> $dependencies = Get-DataverseComponentDependency -ObjectId $entityMetadata.MetadataId -ComponentType 1 -RequiredBy

PS C:\> if ($dependencies) {
>>     Write-Host "Cannot delete - found $($dependencies.Count) dependencies"
>>     $dependencies | Format-Table dependentcomponentobjectid, dependentcomponenttype
>> } else {
>>     Write-Host "Entity can be safely deleted"
>> }
```

Checks if a custom entity has any dependencies before attempting to delete it.

### Example 2: Find all components that use a specific attribute
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $attrMetadata = Get-DataverseAttributeMetadata -EntityName "contact" -AttributeName "new_customfield"
PS C:\> $dependents = Get-DataverseComponentDependency -ObjectId $attrMetadata.MetadataId -ComponentType 2 -Dependent

PS C:\> $dependents | Group-Object dependentcomponenttype | ForEach-Object {
>>     $typeName = switch ($_.Name) {
>>         "24" { "Forms" }
>>         "26" { "Views" }
>>         "29" { "Web Resources" }
>>         "80" { "Workflows" }
>>         default { "Type $($_.Name)" }
>>     }
>>     Write-Host "$typeName: $($_.Count)"
>> }
```

Groups dependent components by type to understand how an attribute is being used.

### Example 3: Pipeline processing for multiple components
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $attributes = Get-DataverseAttributeMetadata -EntityName "account" | Where-Object { $_.IsCustomAttribute }
PS C:\> $attributes | ForEach-Object {
>>     $deps = Get-DataverseComponentDependency -ObjectId $_.MetadataId -ComponentType 2 -RequiredBy
>>     if ($deps) {
>>         [PSCustomObject]@{
>>             AttributeName = $_.LogicalName
>>             DependencyCount = $deps.Count
>>         }
>>     }
>> } | Format-Table
```

Reports dependency counts for all custom attributes on the account entity.

### Example 4: Analyze what would be affected by removing an option set
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $optionSetMetadata = Get-DataverseOptionSetMetadata -Name "new_customchoice"
PS C:\> $dependents = Get-DataverseComponentDependency -ObjectId $optionSetMetadata.MetadataId -ComponentType 9 -Dependent

PS C:\> if ($dependents.Count -gt 0) {
>>     Write-Host "Option set is used by $($dependents.Count) components"
>> } else {
>>     Write-Host "Option set is not currently in use"
>> }
```

Checks if a custom option set is in use by any components.

## PARAMETERS

### -ComponentType
Component type of the object to check. Common types:
- 1 = Entity (Table)
- 2 = Attribute (Column)
- 9 = Option Set (Choice)
- 10 = Relationship
- 24 = Form
- 26 = View
- 29 = Web Resource
- 60 = Chart
- 80 = Process (Workflow)

For a complete list, see [Microsoft's documentation](https://learn.microsoft.com/en-us/power-apps/developer/data-platform/solution-component-file).

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet.

If not provided, uses the default connection set via `Get-DataverseConnection -SetAsDefault`.

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

### -Dependent
Retrieves components that depend on the specified component. This shows what would be affected if the component were modified or removed.

```yaml
Type: SwitchParameter
Parameter Sets: Dependent
Aliases:

Required: True
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -ObjectId
Unique identifier (MetadataId) of the component to check for dependencies.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases: ComponentId, MetadataId

Required: True
Position: 0
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

### -RequiredBy
Retrieves dependencies that would prevent deletion of the specified component. This shows what requires the component to exist.

```yaml
Type: SwitchParameter
Parameter Sets: RequiredBy
Aliases:

Required: True
Position: Named
Default value: False
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
- This cmdlet uses the RetrieveDependenciesForDeleteRequest SDK message (with -RequiredBy) or RetrieveDependentComponentsRequest SDK message (with -Dependent).
- The returned entities contain properties like dependentcomponentobjectid, dependentcomponenttype, requiredcomponentobjectid, and requiredcomponenttype.
- Empty result means no dependencies were found.
- Use -RequiredBy before deleting components to avoid errors.
- Use -Dependent to understand impact of changes to a component.

See https://learn.microsoft.com/en-us/power-apps/developer/data-platform/dependency-tracking-solution-components

## RELATED LINKS

[Get-DataverseSolutionDependency](Get-DataverseSolutionDependency.md)
