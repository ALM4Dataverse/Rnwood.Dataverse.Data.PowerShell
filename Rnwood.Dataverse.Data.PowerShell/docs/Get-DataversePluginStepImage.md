---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataversePluginStepImage

## SYNOPSIS
Retrieves plugin step image records from a Dataverse environment.

## SYNTAX

### All (Default)
```
Get-DataversePluginStepImage [-SdkMessageProcessingStepId <Guid>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### ById
```
Get-DataversePluginStepImage -Id <Guid> [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### ByAlias
```
Get-DataversePluginStepImage -EntityAlias <String> [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### ByStep
```
Get-DataversePluginStepImage -SdkMessageProcessingStepId <Guid> [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Retrieves plugin step images by ID, alias, step ID, or all images.

## EXAMPLES

### Example 1
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataversePluginStepImage -SdkMessageProcessingStepId 12345678-1234-1234-1234-123456789012
```

Retrieves all step images for a specific plugin step.

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

### -EntityAlias
Entity alias of the plugin step image to retrieve

```yaml
Type: String
Parameter Sets: ByAlias
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Id
ID of the plugin step image to retrieve

```yaml
Type: Guid
Parameter Sets: ById
Aliases:

Required: True
Position: Named
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

### -SdkMessageProcessingStepId
Plugin step ID to filter images by

```yaml
Type: Guid
Parameter Sets: All
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

```yaml
Type: Guid
Parameter Sets: ByStep
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Guid
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES

## RELATED LINKS

[Set-DataversePluginStepImage](Set-DataversePluginStepImage.md)
[Remove-DataversePluginStepImage](Remove-DataversePluginStepImage.md)
