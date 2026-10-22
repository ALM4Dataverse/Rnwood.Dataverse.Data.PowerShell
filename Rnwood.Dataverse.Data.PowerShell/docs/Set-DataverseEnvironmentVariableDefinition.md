---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseEnvironmentVariableDefinition

## SYNOPSIS
Creates or updates environment variable definitions in Dataverse.

## SYNTAX

```
Set-DataverseEnvironmentVariableDefinition [-SchemaName] <String> [-DisplayName <String>]
 [-Description <String>] [-Type <String>] [-DefaultValue <String>] [-PassThru] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Creates or updates environment variable definitions in Dataverse. If the definition doesn't exist, it will be created. If it exists, it will be updated with the provided properties.

## EXAMPLES

### Example 1: Create a new environment variable definition
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseEnvironmentVariableDefinition -SchemaName "new_apiurl" -DisplayName "API URL" -Description "The URL for the external API"
```

Creates a new environment variable definition with the specified schema name, display name, and description.

### Example 2: Update an existing environment variable definition
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseEnvironmentVariableDefinition -SchemaName "new_apiurl" -Description "Updated description for the API URL"
```

Updates the description of an existing environment variable definition.

### Example 3: Create definition with specific type
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseEnvironmentVariableDefinition -SchemaName "new_timeout" -DisplayName "Timeout" -Type "Number"
```

Creates an environment variable definition with a specific type (in this case, Number type).

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

### -DefaultValue
Default value for the environment variable definition.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Description
Description for the environment variable definition.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -DisplayName
Display name for the environment variable definition.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -PassThru
If specified, returns the environment variable definition record as a PSObject after creation/update.

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

### -SchemaName
Schema name of the environment variable definition to create or update.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -Type
Type of the environment variable definition. Valid values are: String, Number, Boolean, JSON, Data Source, Secret. Default is String.

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: String, Number, Boolean, JSON, Data Source, Secret

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
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

### System.String
## OUTPUTS

### System.Object
## NOTES
- If the environment variable definition doesn't exist, it will be created with default type (String) if not specified.
- If the environment variable definition exists, only the provided properties will be updated.

## RELATED LINKS

[Get-DataverseEnvironmentVariableDefinition](Get-DataverseEnvironmentVariableDefinition.md)
[Set-DataverseEnvironmentVariableValue](Set-DataverseEnvironmentVariableValue.md)
[Remove-DataverseEnvironmentVariableDefinition](Remove-DataverseEnvironmentVariableDefinition.md)
