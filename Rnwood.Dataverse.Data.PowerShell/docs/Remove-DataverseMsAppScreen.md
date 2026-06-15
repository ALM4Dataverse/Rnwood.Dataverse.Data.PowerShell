---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseMsAppScreen

## SYNOPSIS
Removes a screen from a .msapp file.

## SYNTAX

### FromPath
```
Remove-DataverseMsAppScreen [-MsAppPath] <String> [-ScreenName] <String> [-IfExists]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### FromObject
```
Remove-DataverseMsAppScreen [-CanvasApp] <PSObject> [-ScreenName] <String> [-IfExists]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Removes a screen from a Canvas app .msapp file. The .msapp file is modified in place.

## EXAMPLES

### Example 1: Remove a screen
```powershell
PS C:\> Remove-DataverseMsAppScreen -MsAppPath "myapp.msapp" -ScreenName "OldScreen"
```

Removes a screen from the .msapp file.

### Example 2: Remove with IfExists flag
```powershell
PS C:\> Remove-DataverseMsAppScreen -MsAppPath "myapp.msapp" -ScreenName "MaybeExists" -IfExists
```

Attempts to remove a screen, but doesn't throw an error if it doesn't exist.

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

### -IfExists
If set, no error is thrown if the screen doesn't exist

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
Name of the screen to remove

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
Shows what would happen if the cmdlet runs.
The cmdlet is not run.

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

### System.Management.Automation.PSObject
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
