---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataversePluginStep

## SYNOPSIS
Creates or updates a plugin step (SDK message processing step) in a Dataverse environment.

## SYNTAX

```
Set-DataversePluginStep [-Id <Guid>] -Name <String> -PluginTypeId <Guid> -SdkMessageId <Guid>
 [-SdkMessageFilterId <Guid>] -Stage <PluginStepStage> -Mode <PluginStepMode> [-Rank <Int32>]
 [-Description <String>] [-FilteringAttributes <String[]>] [-Configuration <String>]
 [-SecureConfigurationId <Guid>] [-ImpersonatingUserId <Guid>] [-StateCode <Int32>] [-StatusCode <Int32>]
 [-SupportedDeployment <PluginStepDeployment>] [-PassThru] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Creates or updates plugin steps with comprehensive configuration including stage, mode, rank, and filtering attributes.

## EXAMPLES

### Example 1: Create a plugin step with filtering attributes
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataversePluginStep -Name "MyStep" -PluginTypeId $typeId `
    -SdkMessageId $messageId -Stage 20 -Mode 0 `
    -FilteringAttributes "name","revenue","primarycontactid"
```

Creates a synchronous pre-operation plugin step with filtering attributes.

### Example 2: Create a plugin step without filtering
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataversePluginStep -Name "MyStep" -PluginTypeId $typeId `
    -SdkMessageId $messageId -Stage 20 -Mode 0
```

Creates a synchronous pre-operation plugin step without filtering attributes.

## PARAMETERS

### -Configuration
Configuration for the plugin step (unsecure configuration)

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

### -Description
Description of the step

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

### -FilteringAttributes
Filtering attributes (comma-separated list of attribute logical names)

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

### -Id
ID of the plugin step to update. If not specified, a new step is created.

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

### -ImpersonatingUserId
Impersonating user ID (runs as this user)

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Mode
Execution mode: 0=Synchronous, 1=Asynchronous

```yaml
Type: PluginStepMode
Parameter Sets: (All)
Aliases:
Accepted values: Synchronous, Asynchronous

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name
Name of the plugin step

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

### -PassThru
If specified, the created/updated step is written to the pipeline as a PSObject

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

### -PluginTypeId
Plugin type ID this step executes

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

### -Rank
Execution order (rank). Default is 1.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SdkMessageFilterId
SDK message filter ID (for entity-specific messages)

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SdkMessageId
SDK message ID this step is registered for

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

### -SecureConfigurationId
Secure configuration for the plugin step

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Stage
Stage of execution: 10=PreValidation, 20=PreOperation, 40=PostOperation, 50=PostOperationDeprecated

```yaml
Type: PluginStepStage
Parameter Sets: (All)
Aliases:
Accepted values: PreValidation, PreOperation, PostOperation, PostOperationDeprecated

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -StateCode
State of the step: 0=Enabled, 1=Disabled

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -StatusCode
Status of the step: 1=Enabled, 2=Disabled

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SupportedDeployment
Supported deployment: 0=ServerOnly, 1=MicrosoftDynamics365Client, 2=Both. Default is 0.

```yaml
Type: PluginStepDeployment
Parameter Sets: (All)
Aliases:
Accepted values: ServerOnly, MicrosoftDynamics365Client, Both

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

### System.Nullable`1[[System.Guid, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES

## RELATED LINKS

[Get-DataversePluginStep](Get-DataversePluginStep.md)
[Remove-DataversePluginStep](Remove-DataversePluginStep.md)
