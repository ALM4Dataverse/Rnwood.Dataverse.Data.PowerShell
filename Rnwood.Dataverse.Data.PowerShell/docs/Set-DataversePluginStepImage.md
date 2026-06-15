---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataversePluginStepImage

## SYNOPSIS
Creates or updates a plugin step image in a Dataverse environment.

## SYNTAX

```
Set-DataversePluginStepImage [-Id <Guid>] -SdkMessageProcessingStepId <Guid> -EntityAlias <String>
 -ImageType <PluginStepImageType> [-MessagePropertyName <String>] [-Attributes <String[]>] [-Name <String>]
 [-PassThru] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

## DESCRIPTION
Creates or updates plugin step images (pre-image or post-image) with attribute filtering.

## EXAMPLES

### Example 1: Create a pre-image with specific attributes
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataversePluginStepImage `
    -SdkMessageProcessingStepId $stepId `
    -EntityAlias "PreImage" `
    -ImageType 0 `
    -Attributes "firstname","lastname","emailaddress1"
```

Creates a pre-image with specific attributes.

### Example 2: Create a post-image with all attributes
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataversePluginStepImage `
    -SdkMessageProcessingStepId $stepId `
    -EntityAlias "PostImage" `
    -ImageType 1
```

Creates a post-image with all attributes (no -Attributes parameter means all attributes are included).

## PARAMETERS

### -Attributes
Attributes to include in the image (array of attribute logical names). Leave empty for all attributes. Tab completion is available for attribute names.

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
Entity alias for the image (used to reference the image in plugin code)

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Id
ID of the plugin step image to update. If not specified, a new image is created.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -ImageType
Image type: 0=PreImage, 1=PostImage, 2=Both

```yaml
Type: PluginStepImageType
Parameter Sets: (All)
Aliases:
Accepted values: PreImage, PostImage, Both

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MessagePropertyName
Message property name. Default is 'Target' for most messages.

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

### -Name
Name of the image

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

### -PassThru
If specified, the created/updated image is written to the pipeline as a PSObject

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

### -SdkMessageProcessingStepId
Plugin step ID this image belongs to

```yaml
Type: Guid
Parameter Sets: (All)
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

### System.Nullable`1[[System.Guid, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES

## RELATED LINKS

[Get-DataversePluginStepImage](Get-DataversePluginStepImage.md)
[Remove-DataversePluginStepImage](Remove-DataversePluginStepImage.md)
