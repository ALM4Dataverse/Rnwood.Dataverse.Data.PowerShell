---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Compress-DataverseSolutionFile

## SYNOPSIS
Packs a Dataverse solution folder using the Power Apps CLI.

## SYNTAX

```
Compress-DataverseSolutionFile [-Path] <String> [-OutputPath] <String> [-PackageType <SolutionPackageType>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The `Compress-DataverseSolutionFile` cmdlet packs an unpacked Dataverse solution folder into a solution ZIP file using the Power Apps CLI (`pac solution pack` command). This is the reverse operation of `Expand-DataverseSolutionFile` and is typically used after editing solution components.

The cmdlet requires the Power Apps CLI to be installed and available in your system PATH. Install from https://aka.ms/PowerAppsCLI

## EXAMPLES

### Example 1: Pack a solution
```powershell
Compress-DataverseSolutionFile -Path "C:\Solutions\MySolution_Src" -OutputPath "C:\Solutions\MySolution.zip"
```

Packs the solution folder into a ZIP file.

### Example 2: Pack a managed solution
```powershell
Compress-DataverseSolutionFile -Path "C:\Solutions\MySolution_Src" -OutputPath "C:\Solutions\MySolution_Managed.zip" -PackageType Managed
```

Packs a managed solution from a folder that was previously unpacked with `-PackageType Both`. Any folders with `.msapp` extension are automatically zipped into .msapp files before packing.

## PARAMETERS

### -OutputPath
Output path for the packed solution file (.zip).

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PackageType
Package type: 'Unmanaged' (default), 'Managed' (from a previous unpack 'Both'), or 'Both'.

```yaml
Type: SolutionPackageType
Parameter Sets: (All)
Aliases:
Accepted values: Unmanaged, Managed, Both

Required: False
Position: Named
Default value: Unmanaged
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path
Path to the solution folder to pack.

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

### None
## OUTPUTS

### System.Object
## NOTES
- Requires the Power Apps CLI (pac) to be installed and available in your PATH.
- Install from https://aka.ms/PowerAppsCLI
- Canvas App folders (with `.msapp` extension) are automatically detected and packed into .msapp files.
- A temporary copy of the solution folder is created when .msapp folders are found, so the original folder is not modified.
- Use `-PackageType` to specify 'Unmanaged' (default), 'Managed' (from a previous unpack 'Both'), or 'Both'.

## RELATED LINKS

[Expand-DataverseSolutionFile](Expand-DataverseSolutionFile.md)

[Export-DataverseSolution](Export-DataverseSolution.md)

[Import-DataverseSolution](Import-DataverseSolution.md)

[Power Apps CLI documentation](https://learn.microsoft.com/power-platform/developer/cli/introduction)
