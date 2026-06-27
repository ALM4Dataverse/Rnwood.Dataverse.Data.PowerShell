---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Compare-DataverseSolutionComponents

## SYNOPSIS
Compares a solution file with the state of that solution in the target environment or with another solution file.

## SYNTAX

### FileToEnvironment
```
Compare-DataverseSolutionComponents [-Connection <ServiceClient>] [-SolutionFile] <String> [-FileToEnvironment]
 [-ReverseComparison] [-TestIfAdditive] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### BytesToEnvironment
```
Compare-DataverseSolutionComponents [-Connection <ServiceClient>] -SolutionBytes <Byte[]> [-BytesToEnvironment]
 [-ReverseComparison] [-TestIfAdditive] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### FileToFile
```
Compare-DataverseSolutionComponents [-SolutionFile] <String> [-FileToFile] [-TargetSolutionFile] <String>
 [-TestIfAdditive] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### BytesToFile
```
Compare-DataverseSolutionComponents -SolutionBytes <Byte[]> [-BytesToFile] [-TargetSolutionFile] <String>
 [-TestIfAdditive] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The Compare-DataverseSolutionComponents cmdlet compares solution files and environments to identify differences in solution components. It can:
- Compare a solution file with the same solution in a Dataverse environment
- Compare two solution files directly
- Reverse the comparison direction (environment to file)
- Enumerate and compare subcomponents for components with full inclusion behavior

The cmdlet outputs detailed comparison results showing whether components have been added, removed, modified, or had their behavior changed.

**Subcomponent Support:**
When a component has behavior 0 (Include Subcomponents), the cmdlet automatically enumerates and compares all subcomponents:
- **Entity subcomponents**: Attributes, Relationships, Forms, Views
- Subcomponent changes are tracked with parent component information
- **File-to-file comparisons**: Subcomponents extracted from customizations.xml in solution ZIP
- **File-to-environment comparisons**: Subcomponents queried from environment metadata

Component behavior statuses:
- **Added**: Component exists in source but not in target
- **Removed**: Component exists in target but not in source
- **Modified**: Component exists in both with same behavior
- **BehaviorIncluded**: Component behavior changed to include more data (e.g., Shell → Full)
- **BehaviorExcluded**: Component behavior changed to exclude data (e.g., Full → Shell)

Rootcomponentbehavior values:
- 0 = Include Subcomponents (Full component with all subcomponents enumerated)
- 1 = Do Not Include Subcomponents
- 2 = Include As Shell (Shell only, no subcomponents)

## EXAMPLES

### Example 1: Compare a solution file with the target environment
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive
PS C:\> Compare-DataverseSolutionComponents -SolutionFile "C:\Solutions\MySolution_1_0_0_0.zip"
```

This example compares the MySolution solution file with the state of the solution in the target environment.

### Example 2: Compare two solution files
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Compare-DataverseSolutionComponents -SolutionFile "C:\Solutions\MySolution_v1.zip" -TargetSolutionFile "C:\Solutions\MySolution_v2.zip"
```

This example compares two solution files to identify what changed between versions.

### Example 3: Compare environment to file (reverse direction)
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive
PS C:\> Compare-DataverseSolutionComponents -SolutionFile "C:\Solutions\MySolution.zip" -ReverseComparison
```

This example reverses the comparison to show what's in the environment compared to the file.

### Example 4: Filter results to show only added components
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive
PS C:\> $results = Compare-DataverseSolutionComponents -SolutionFile "C:\Solutions\MySolution.zip"
PS C:\> $results | Where-Object { $_.Status -eq "Added" } | Format-Table
```

This example shows only the components that have been added to the solution file but don't exist in the environment.

### Example 5: Identify behavior changes between solution versions
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $results = Compare-DataverseSolutionComponents -SolutionFile "C:\v1\MySolution.zip" -TargetSolutionFile "C:\v2\MySolution.zip"
PS C:\> $results | Where-Object { $_.Status -like "Behavior*" } | Format-Table
```

This example compares two solution versions and shows only components where the behavior has changed (BehaviorIncluded or BehaviorExcluded).

### Example 6: View subcomponents of entities
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive
PS C:\> $results = Compare-DataverseSolutionComponents -SolutionFile "C:\Solutions\MySolution.zip"
PS C:\> $results | Where-Object IsSubcomponent -eq $true | Format-Table ComponentTypeName, Status, ParentComponentTypeName
```

This example shows only subcomponents (attributes, views, forms, etc.) and their parent component types.

### Example 7: Export comparison results to CSV
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive
PS C:\> $results = Compare-DataverseSolutionComponents -SolutionFile "C:\Solutions\MySolution.zip"
PS C:\> $results | Export-Csv -Path "C:\Reports\solution-comparison.csv" -NoTypeInformation
```

This example exports the comparison results to a CSV file for further analysis.

### Example 8: Test if changes are additive (file to environment)
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $conn = Get-DataverseConnection -Url "https://yourorg.crm.dynamics.com" -Interactive
PS C:\> $isAdditive = Compare-DataverseSolutionComponents -SolutionFile "C:\Solutions\MySolution.zip" -TestIfAdditive -Verbose
```

This example tests whether the solution file has only additive changes compared to the environment (no removed components or less inclusive behavior changes).
Returns $true if additive, $false otherwise. Full comparison results are output to verbose stream.

### Example 9: Test if changes are additive (file to file)
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $isAdditive = Compare-DataverseSolutionComponents -SolutionFile "C:\Solutions\MySolution_v1.zip" -TargetSolutionFile "C:\Solutions\MySolution_v2.zip" -FileToFile -TestIfAdditive -Verbose
PS C:\> if ($isAdditive) { Write-Host "Update can use simple import mode" } else { Write-Host "Update requires stage and upgrade" }
```

This example tests whether changes between two solution files are additive. This is useful for determining whether a solution update can use simple import mode (faster) or requires stage and upgrade mode (slower but handles removals correctly).

## PARAMETERS

### -BytesToEnvironment
Compare solution bytes with the environment.

```yaml
Type: SwitchParameter
Parameter Sets: BytesToEnvironment
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BytesToFile
Compare solution bytes with a target solution file.

```yaml
Type: SwitchParameter
Parameter Sets: BytesToFile
Aliases:

Required: True
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
Parameter Sets: FileToEnvironment, BytesToEnvironment
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FileToEnvironment
Compare a solution file with the environment.

```yaml
Type: SwitchParameter
Parameter Sets: FileToEnvironment
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FileToFile
Compare two solution files.

```yaml
Type: SwitchParameter
Parameter Sets: FileToFile
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

### -ReverseComparison
Compare environment to file instead of file to environment.

```yaml
Type: SwitchParameter
Parameter Sets: FileToEnvironment, BytesToEnvironment
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SolutionBytes
Solution file bytes to compare.

```yaml
Type: Byte[]
Parameter Sets: BytesToEnvironment, BytesToFile
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -SolutionFile
Path to the solution file (.zip) to compare.

```yaml
Type: String
Parameter Sets: FileToEnvironment, FileToFile
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TargetSolutionFile
Path to the target solution file (.zip) to compare.

```yaml
Type: String
Parameter Sets: FileToFile, BytesToFile
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TestIfAdditive
Test if changes are additive (no removed components or less inclusive behavior changes). Returns true/false, outputs full results to verbose.

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

### System.Byte[]
## OUTPUTS

### System.Management.Automation.PSObject
### System.Boolean
## NOTES
- **Status values**:
  - **Added**: Component exists in source but not in target
  - **Removed**: Component exists in target but not in source
  - **Modified**: Component exists in both with same behavior
  - **BehaviorIncluded**: Behavior changed to include more data (e.g., Shell → Full)
  - **BehaviorExcluded**: Behavior changed to exclude data (e.g., Full → Shell)
  
- **Subcomponent enumeration**:
  - When a component has behavior 0 (Include Subcomponents), all subcomponents are automatically enumerated
  - Entity subcomponents include: Attributes (type 2), Relationships (type 10), Forms (type 60), Views (type 26)
  - **File-to-file comparisons**: Subcomponents extracted from customizations.xml in solution ZIP
  - **Environment comparisons**: Subcomponents queried from environment metadata
  - Works for both comparison modes - no Connection required for file-to-file
  - If the solution does not exist in the target environment, all components will be marked as "Added"
  - Components that exist in both locations with the same behavior are marked as "Modified" (deep content inspection is not performed)
  - The cmdlet can compare file-to-environment, file-to-file, or environment-to-file (with -ReverseComparison)
  - Connection parameter is only required for environment comparisons, not for file-to-file comparisons

- **Integration with Import-DataverseSolution**:
  - This cmdlet is used internally by `Import-DataverseSolution -UseUpdateIfAdditive` to determine if a solution update can use the simpler import mode
  - The -UseUpdateIfAdditive switch compares components and uses simple import if no components are removed or have less inclusive behavior changes

- **TestIfAdditive switch**:
  - When specified, returns $true or $false based on whether changes are additive only
  - Returns $true if there are zero components with "InTargetOnly" status (removed) or "InSourceAndTarget_BehaviourLessInclusiveInSource" status (behavior became less inclusive)
  - Full comparison results are output to the verbose stream for troubleshooting
  - Uses the same logic as `Import-DataverseSolution -UseUpdateIfAdditive` to determine if simple import mode can be used
  - Works with all parameter sets: FileToEnvironment, BytesToEnvironment, FileToFile, and BytesToFile

## RELATED LINKS

[Export-DataverseSolution](Export-DataverseSolution.md)
[Import-DataverseSolution](Import-DataverseSolution.md)
[Get-DataverseConnection](Get-DataverseConnection.md)
