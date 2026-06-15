---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseSolutionFileComponent

## SYNOPSIS
Retrieves the components from a Dataverse solution file (.zip).

## SYNTAX

### FromFile
```
Get-DataverseSolutionFileComponent [-SolutionFile] <String> [-IncludeSubcomponents]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### FromBytes
```
Get-DataverseSolutionFileComponent -SolutionBytes <Byte[]> [-IncludeSubcomponents]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Extracts and lists all components from a Dataverse solution file, including root components from the solution manifest, as well as environment variables (componenttype=380) and connection references (componenttype=635) that are stored as separate files within the solution package.

This cmdlet is useful for analyzing solution contents without importing the solution into an environment. It discovers components that may not be listed in the RootComponents section of solution.xml, such as environment variables which are stored in the environmentvariabledefinitions/ folder.

## EXAMPLES

### Example 1: List all components from a solution file
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $conn = Get-DataverseConnection -Url "https://contoso.crm.dynamics.com" -Interactive
PS C:\> Get-DataverseSolutionFileComponent -SolutionFile "C:\Solutions\MySolution_1_0_0_0.zip"
```

Lists all root components from the specified solution file, including environment variables and connection references.

### Example 2: List components including subcomponents
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $conn = Get-DataverseConnection -Url "https://contoso.crm.dynamics.com" -Interactive
PS C:\> Get-DataverseSolutionFileComponent -SolutionFile "C:\Solutions\MySolution_1_0_0_0.zip" -IncludeSubcomponents
```

Lists all components including subcomponents (attributes, relationships, forms, views, etc.) from the specified solution file.

### Example 3: Filter for environment variables
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $conn = Get-DataverseConnection -Url "https://contoso.crm.dynamics.com" -Interactive
PS C:\> Get-DataverseSolutionFileComponent -SolutionFile "C:\Solutions\MySolution_1_0_0_0.zip" | Where-Object { $_.ComponentType -eq 380 }
```

Lists only environment variable components (componenttype=380) from the solution file.

### Example 4: Filter for connection references
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $conn = Get-DataverseConnection -Url "https://contoso.crm.dynamics.com" -Interactive
PS C:\> Get-DataverseSolutionFileComponent -SolutionFile "C:\Solutions\MySolution_1_0_0_0.zip" | Where-Object { $_.ComponentType -eq 635 }
```

Lists only connection reference components (componenttype=635) from the solution file.

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

### -IncludeSubcomponents
Include subcomponents (attributes, relationships, forms, views, etc.) from the solution file.

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

### -SolutionBytes
Solution file bytes to analyze.

```yaml
Type: Byte[]
Parameter Sets: FromBytes
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -SolutionFile
Path to the solution file (.zip) to analyze.

```yaml
Type: String
Parameter Sets: FromFile
Aliases:

Required: True
Position: 0
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
## NOTES

This cmdlet discovers components beyond those listed in the solution's RootComponents section:
- Environment Variable Definitions (componenttype=380) are discovered by reading XML files from the environmentvariabledefinitions/ folder within the solution package
- Connection References (componenttype=635) are discovered by parsing the customizations.xml file

This ensures all components are visible even if they are not explicitly listed as root components in the solution manifest.

## RELATED LINKS

[Get-DataverseSolution](Get-DataverseSolution.md)
[Compare-DataverseSolutionComponents](Compare-DataverseSolutionComponents.md)
[Import-DataverseSolution](Import-DataverseSolution.md)
