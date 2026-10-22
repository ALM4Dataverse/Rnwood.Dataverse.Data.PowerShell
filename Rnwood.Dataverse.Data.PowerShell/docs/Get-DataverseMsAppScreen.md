---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseMsAppScreen

## SYNOPSIS
Retrieves screens from a .msapp file.

## SYNTAX

### FromPath
```
Get-DataverseMsAppScreen [-MsAppPath] <String> [-ScreenName <String>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### FromObject
```
Get-DataverseMsAppScreen [-CanvasApp] <PSObject> [-ScreenName <String>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
Retrieves screens from a Canvas app .msapp file. Screens are returned as PowerShell objects containing the screen name, file path, and YAML content.

## EXAMPLES

### Example 1: Get all screens from a .msapp file
```powershell
PS C:\> $screens = Get-DataverseMsAppScreen -MsAppPath "C:\apps\myapp.msapp"
PS C:\> $screens | Format-Table ScreenName, Size
```

Retrieves all screens from a .msapp file.

### Example 2: Filter screens by name pattern
```powershell
PS C:\> Get-DataverseMsAppScreen -MsAppPath "myapp.msapp" -ScreenName "Main*"
```

Retrieves only screens whose names start with "Main".

## PARAMETERS

### -CanvasApp
Canvas app PSObject from Get-DataverseCanvasApp -IncludeDocument

```yaml
Type: PSObject
Parameter Sets: FromObject
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -MsAppPath
Path to the .msapp file

```yaml
Type: String
Parameter Sets: FromPath
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

### -ScreenName
Name pattern of screens to retrieve.
Supports wildcards (* and ?)

```yaml
Type: String
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

### System.Management.Automation.PSObject
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES

## RELATED LINKS
