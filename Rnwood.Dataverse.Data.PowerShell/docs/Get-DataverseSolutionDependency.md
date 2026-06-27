---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseSolutionDependency

## SYNOPSIS
Retrieves solution dependencies in Dataverse.

## SYNTAX

### Missing-ByName
```
Get-DataverseSolutionDependency [-SolutionUniqueName] <String> [-Missing] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Uninstall-ByName
```
Get-DataverseSolutionDependency [-SolutionUniqueName] <String> [-Uninstall] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Missing-ById
```
Get-DataverseSolutionDependency [-SolutionId] <Guid> [-Missing] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Uninstall-ById
```
Get-DataverseSolutionDependency [-SolutionId] <Guid> [-Uninstall] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet retrieves solution dependencies in Dataverse. It supports two modes:

**-Missing**: Retrieves missing dependencies for a solution (uses RetrieveMissingDependenciesRequest). This is useful for validating a solution before import to identify components that are required but not present in the target environment.

**-Uninstall**: Retrieves dependencies that would prevent a solution from being uninstalled (uses RetrieveDependenciesForUninstallRequest). This shows external components that depend on components within the solution.

The cmdlet returns a collection of dependency entities, each describing a dependency relationship.

## EXAMPLES

### Example 1: Check for missing dependencies before import
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $missingDeps = Get-DataverseSolutionDependency -SolutionUniqueName "MyCustomSolution" -Missing

PS C:\> if ($missingDeps) {
>>     Write-Host "Cannot import solution - found $($missingDeps.Count) missing dependencies"
>>     $missingDeps | Format-Table missingcomponentid, missingcomponenttype
>> } else {
>>     Write-Host "All dependencies satisfied - ready to import"
>>     Import-DataverseSolution -SolutionFile "MyCustomSolution.zip"
>> }
```

Checks for missing dependencies before importing a solution.

### Example 2: Verify solution uninstall readiness
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $dependencies = Get-DataverseSolutionDependency -SolutionUniqueName "MyOldSolution" -Uninstall

PS C:\> if ($dependencies) {
>>     Write-Host "Cannot uninstall - found $($dependencies.Count) dependencies"
>>     $dependencies | Format-Table dependentcomponentobjectid, dependentcomponenttype
>> } else {
>>     Write-Host "Solution can be safely uninstalled"
>>     Remove-DataverseSolution -SolutionUniqueName "MyOldSolution"
>> }
```

Checks if a solution has any dependencies before attempting to uninstall it.

### Example 3: Generate missing dependencies report
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $missingDeps = Get-DataverseSolutionDependency -SolutionUniqueName "TestSolution" -Missing
PS C:\> $missingDeps | ForEach-Object {
>>     [PSCustomObject]@{
>>         MissingComponentId = $_.missingcomponentid
>>         MissingComponentType = $_.missingcomponenttype
>>         RequiredComponentId = $_.requiredcomponentid
>>         RequiredComponentType = $_.requiredcomponenttype
>>     }
>> } | Format-Table -AutoSize
```

Creates a detailed report of all missing dependencies.

### Example 4: Check multiple solutions for uninstall readiness
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $solutions = "Solution1", "Solution2", "Solution3"
PS C:\> $solutions | ForEach-Object {
>>     $deps = Get-DataverseSolutionDependency -SolutionUniqueName $_ -Uninstall
>>     [PSCustomObject]@{
>>         SolutionName = $_
>>         DependencyCount = $deps.Count
>>         CanUninstall = ($deps.Count -eq 0)
>>     }
>> } | Format-Table
```

Checks multiple solutions for uninstall readiness and reports status.

### Example 5: Pipeline usage with solution list
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $solutions = Get-DataverseSolution | Where-Object { $_.ismanaged -eq $false }
PS C:\> $solutions | ForEach-Object {
>>     $deps = Get-DataverseSolutionDependency -SolutionUniqueName $_.uniquename -Uninstall
>>     if ($deps) {
>>         [PSCustomObject]@{
>>             SolutionName = $_.friendlyname
>>             UniqueName = $_.uniquename
>>             DependencyCount = $deps.Count
>>             CanUninstall = $false
>>         }
>>     } else {
>>         [PSCustomObject]@{
>>             SolutionName = $_.friendlyname
>>             UniqueName = $_.uniquename
>>             DependencyCount = 0
>>             CanUninstall = $true
>>         }
>>     }
>> } | Format-Table
```

Checks all unmanaged solutions to identify which ones can be safely uninstalled.

## PARAMETERS

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

### -Missing
Retrieves missing dependencies for the solution. This validates what required components are not present in the target environment.

```yaml
Type: SwitchParameter
Parameter Sets: Missing-ByName, Missing-ById
Aliases:

Required: True
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

### -SolutionId
ID of the solution

```yaml
Type: Guid
Parameter Sets: Missing-ById, Uninstall-ById
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -SolutionUniqueName
Unique name of the solution to check for dependencies.

```yaml
Type: String
Parameter Sets: Missing-ByName, Uninstall-ByName
Aliases: UniqueName

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Uninstall
Retrieves dependencies that would prevent solution uninstall. This shows external components that depend on components within the solution.

```yaml
Type: SwitchParameter
Parameter Sets: Uninstall-ByName, Uninstall-ById
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

### System.String
### System.Guid
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES
- This cmdlet uses the RetrieveMissingDependenciesRequest SDK message (with -Missing) or RetrieveDependenciesForUninstallRequest SDK message (with -Uninstall).
- For -Missing mode: Returns entities with properties like missingcomponentid, missingcomponenttype, requiredcomponentid, and requiredcomponenttype.
- For -Uninstall mode: Returns entities with properties like dependentcomponentobjectid, dependentcomponenttype, requiredcomponentobjectid, and requiredcomponenttype.
- An empty result means no dependencies were found.
- Use -Missing before solution import to avoid import failures.
- Use -Uninstall before solution removal to understand what must be resolved first.

See https://learn.microsoft.com/en-us/power-apps/developer/data-platform/dependency-tracking-solution-components

## RELATED LINKS

[Get-DataverseComponentDependency](Get-DataverseComponentDependency.md)
[Import-DataverseSolution](Import-DataverseSolution.md)
[Remove-DataverseSolution](Remove-DataverseSolution.md)
