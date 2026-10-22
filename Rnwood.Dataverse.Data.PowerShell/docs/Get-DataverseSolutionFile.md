---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseSolutionFile

## SYNOPSIS
Parses a Dataverse solution file and returns metadata information.

## SYNTAX

### FromFile
```
Get-DataverseSolutionFile [-Path] <String> [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### FromBytes
```
Get-DataverseSolutionFile -SolutionFile <Byte[]> [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet parses a Dataverse solution ZIP file (either from a file path or from bytes) and extracts metadata information including:
- Solution unique name and friendly name
- Version
- Managed/unmanaged status
- Description
- Publisher information (name, unique name, prefix)

This is useful for inspecting solution files before importing them or for automation scripts that need to read solution metadata.

## EXAMPLES

### Example 1: Parse a solution file from disk
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseSolutionFile -Path "C:\Solutions\MySolution_1_0_0_0.zip"

UniqueName            : MySolution
Name                  : My Solution
Version               : 1.0.0.0
IsManaged             : False
Description           : My custom solution for business processes
PublisherName         : Contoso
PublisherUniqueName   : contoso
PublisherPrefix       : con
```

Parses the solution file and displays all metadata.

### Example 2: Parse a solution file from bytes in pipeline
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $bytes = [System.IO.File]::ReadAllBytes("C:\Solutions\MySolution.zip")
PS C:\> $bytes | Get-DataverseSolutionFile

UniqueName            : MySolution
Name                  : My Solution
Version               : 1.0.0.0
IsManaged             : False
```

Reads solution bytes and pipes them to the cmdlet for parsing.

### Example 3: Check if a solution is managed
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $info = Get-DataverseSolutionFile -Path "C:\Solutions\MySolution.zip"
PS C:\> if ($info.IsManaged) {
>>     Write-Host "Solution is managed"
>> } else {
>>     Write-Host "Solution is unmanaged"
>> }
```

Parses the solution and checks the managed status.

## PARAMETERS

### -Path
Path to the solution file (.zip) to parse.

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

### -SolutionFile
Solution file bytes to parse.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Byte[]
## OUTPUTS

### Rnwood.Dataverse.Data.PowerShell.SolutionInfo
## NOTES

This cmdlet does not require a connection to Dataverse as it only parses the local solution file.

## RELATED LINKS

[Import-DataverseSolution](Import-DataverseSolution.md)

[Export-DataverseSolution](Export-DataverseSolution.md)

[Get-DataverseSolution](Get-DataverseSolution.md)
