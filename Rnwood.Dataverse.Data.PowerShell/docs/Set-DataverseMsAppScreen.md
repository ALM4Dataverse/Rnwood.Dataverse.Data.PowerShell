---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseMsAppScreen

## SYNOPSIS
Adds or updates a screen in a .msapp file.

## SYNTAX

### YamlContent-FromPath
```
Set-DataverseMsAppScreen [-MsAppPath] <String> [-ScreenName] <String> [-YamlContent] <String>
 [-TemplateSourceMsAppPath <String[]>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

### YamlFile-FromPath
```
Set-DataverseMsAppScreen [-MsAppPath] <String> [-ScreenName] <String> -YamlFilePath <String>
 [-TemplateSourceMsAppPath <String[]>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

### YamlContent-FromObject
```
Set-DataverseMsAppScreen [-CanvasApp] <PSObject> [-ScreenName] <String> [-YamlContent] <String>
 [-TemplateSourceMsAppPath <String[]>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

### YamlFile-FromObject
```
Set-DataverseMsAppScreen [-CanvasApp] <PSObject> [-ScreenName] <String> -YamlFilePath <String>
 [-TemplateSourceMsAppPath <String[]>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

## DESCRIPTION
Adds or updates a screen in a Canvas app .msapp file. The screen is defined using Power Apps YAML format. The .msapp file is modified in place and the Controls/*.json files are automatically regenerated from the YAML.

> **EXPERIMENTAL:** YAML-first Canvas app modification is experimental. The Power Apps MSAPP format it not fully understood/supported and the results may need to be validated in Power Apps Studio.

## EXAMPLES

### Example 1: Add a new screen
```powershell
PS C:\> $yaml = @"
Screens:
  NewScreen:
    Properties:
      LoadingSpinnerColor: =RGBA(56, 96, 178, 1)
"@
PS C:\> Set-DataverseMsAppScreen -MsAppPath "myapp.msapp" -ScreenName "NewScreen" -YamlContent $yaml
```

Adds a new screen to the .msapp file.

### Example 2: Update from YAML file
```powershell
PS C:\> Set-DataverseMsAppScreen -MsAppPath "myapp.msapp" -ScreenName "MainScreen" -YamlFilePath "MainScreen.pa.yaml"
```

Updates a screen using YAML content from a file.

## PARAMETERS

### -CanvasApp
Canvas app PSObject from Get-DataverseCanvasApp -IncludeDocument

```yaml
Type: PSObject
Parameter Sets: YamlContent-FromObject, YamlFile-FromObject
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
Parameter Sets: YamlContent-FromPath, YamlFile-FromPath
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
Name of the screen

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

### -TemplateSourceMsAppPath
Optional .msapp file(s) used as template sources for control prototypes during pack

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -YamlContent
YAML content for the screen

```yaml
Type: String
Parameter Sets: YamlContent-FromPath, YamlContent-FromObject
Aliases:

Required: True
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -YamlFilePath
Path to a YAML file containing the screen definition

```yaml
Type: String
Parameter Sets: YamlFile-FromPath, YamlFile-FromObject
Aliases:

Required: True
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

> **EXPERIMENTAL:** YAML-first Canvas app modification is experimental. The Power Apps MSAPP format it not fully understood/supported and the results may need to be validated in Power Apps Studio.

A warning is emitted at runtime each time this cmdlet runs as a reminder of the experimental status. To suppress the warning, use the common `-WarningAction SilentlyContinue` parameter.

## RELATED LINKS
