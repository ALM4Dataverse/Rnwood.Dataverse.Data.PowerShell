---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseSolution

## SYNOPSIS
Retrieves solution information from a Dataverse environment.

## SYNTAX

```
Get-DataverseSolution [[-UniqueName] <String>] [-Managed] [-Unmanaged] [-ExcludeSystemSolutions]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet retrieves solution information from a Dataverse environment. You can retrieve all solutions or filter by unique name, managed status, or exclude system solutions.

The cmdlet returns SolutionInfo objects with metadata about each solution including name, version, publisher information, and more.

## EXAMPLES

### Example 1: Retrieve all solutions
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseSolution

UniqueName            Name                  Version    IsManaged PublisherName
----------            ----                  -------    --------- -------------
MySolution            My Solution           1.0.0.0    False     Contoso
Active                Active Solution       1.0.0.0    True      Microsoft
Default               Default Solution      1.0.0.0    True      Microsoft
```

Retrieves all solutions from the Dataverse environment.

### Example 2: Retrieve a specific solution by unique name
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseSolution -UniqueName "MySolution"

UniqueName            : MySolution
Name                  : My Solution
Version               : 1.0.0.0
IsManaged             : False
Description           : My custom solution
PublisherName         : Contoso
PublisherUniqueName   : contoso
PublisherPrefix       : con
```

Retrieves a specific solution by its unique name.

### Example 3: Get only managed solutions
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseSolution -Managed

UniqueName            Name                  Version    IsManaged
----------            ----                  -------    ---------
UpgradeSolution       Upgrade Solution      2.0.0.0    True
PartnerSolution       Partner Solution      1.5.0.0    True
```

Retrieves only managed solutions.

### Example 4: Get only unmanaged solutions, excluding system solutions
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseSolution -Unmanaged -ExcludeSystemSolutions

UniqueName            Name                  Version    IsManaged
----------            ----                  -------    ---------
MySolution            My Solution           1.0.0.0    False
CustomSolution        Custom Solution       1.2.0.0    False
```

Retrieves only unmanaged solutions, excluding the Default, Active, and Basic system solutions.

### Example 5: Get solutions using wildcard patterns
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseSolution -UniqueName "Contoso*"

UniqueName            Name                  Version    IsManaged
----------            ----                  -------    ---------
ContosoSales          Contoso Sales         1.0.0.0    False
ContosoMarketing      Contoso Marketing     2.1.0.0    False
```

Retrieves all solutions whose unique name starts with "Contoso".

### Example 6: Filter solutions with multiple wildcards
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseSolution -UniqueName "*Custom*" -Unmanaged

UniqueName            Name                  Version    IsManaged
----------            ----                  -------    ---------
MyCustomSolution      My Custom Solution    1.0.0.0    False
TeamCustomization     Team Customization    1.5.0.0    False
```

Retrieves all unmanaged solutions whose unique name contains "Custom".

### Example 7: Check solution version before upgrade
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $solution = Get-DataverseSolution -UniqueName "MySolution"
PS C:\> if ($solution.Version -lt [Version]"2.0.0.0") {
>>     Write-Host "Solution needs upgrade from $($solution.Version) to 2.0.0.0"
>> }
```

Retrieves a solution and checks if it needs to be upgraded.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet. If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.

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

### -ExcludeSystemSolutions
Exclude default system solutions (Default, Active, and Basic).

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

### -Managed
Filter to return only managed solutions.

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

### -UniqueName
The unique name of the solution to retrieve. Supports wildcards (* and ?). If not specified, all solutions are returned.

Use `*` to match zero or more characters and `?` to match a single character.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Unmanaged
Filter to return only unmanaged solutions.

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

### None
## OUTPUTS

### Rnwood.Dataverse.Data.PowerShell.SolutionInfo
## NOTES

This cmdlet requires an active connection to a Dataverse environment.

## RELATED LINKS

[Get-DataverseSolutionFile](Get-DataverseSolutionFile.md)

[Import-DataverseSolution](Import-DataverseSolution.md)

[Export-DataverseSolution](Export-DataverseSolution.md)

[Set-DataverseSolution](Set-DataverseSolution.md)

[Remove-DataverseSolution](Remove-DataverseSolution.md)
