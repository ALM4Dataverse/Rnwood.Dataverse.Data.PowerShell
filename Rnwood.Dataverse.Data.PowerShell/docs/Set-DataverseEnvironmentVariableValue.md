---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseEnvironmentVariableValue

## SYNOPSIS
Sets environment variable values in Dataverse.

## SYNTAX

### Single (Default)
```
Set-DataverseEnvironmentVariableValue [-SchemaName] <String> [-Value] <String> [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### Multiple
```
Set-DataverseEnvironmentVariableValue -EnvironmentVariableValues <Hashtable> [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Sets environment variable values in Dataverse. This cmdlet only manages value records and requires that the environment variable definition already exists.
Use Set-DataverseEnvironmentVariableDefinition to create both the definition and set the value in one operation.

## EXAMPLES

### Example 1: Set a single environment variable value
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseEnvironmentVariableValue -SchemaName "new_apiurl" -Value "https://api.production.example.com"
```

Sets the value for the environment variable "new_apiurl". The definition must already exist.

### Example 2: Set multiple environment variable values
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseEnvironmentVariableValue -EnvironmentVariableValues @{
    'new_apiurl' = 'https://api.production.example.com'
    'new_apikey' = 'prod-key-12345'
    'new_timeout' = '30'
}
```

Sets values for multiple environment variables at once using a hashtable.

### Example 3: Update value from pipeline
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseEnvironmentVariableValue -SchemaName "new_apiurl" | 
    Set-DataverseEnvironmentVariableValue -Value "https://api.staging.example.com"
```

Updates the value for an existing environment variable.

### Example 4: Set value with WhatIf
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseEnvironmentVariableValue -SchemaName "new_apiurl" -Value "https://api.test.example.com" -WhatIf
```

Shows what would happen if the cmdlet runs without actually making changes.

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

### -EnvironmentVariableValues
Hashtable of environment variable schema names to values (e.g., @{'new_apiurl' = 'https://api.example.com'}).

```yaml
Type: Hashtable
Parameter Sets: Multiple
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

### -SchemaName
Schema name of the environment variable to set the value for (for single parameter set).

```yaml
Type: String
Parameter Sets: Single
Aliases: DefinitionSchemaName

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Value
Value to set for the environment variable (for single parameter set). Can be an empty string.

```yaml
Type: String
Parameter Sets: Single
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
- The environment variable definition must already exist before using this cmdlet
- If the definition doesn't exist, the cmdlet will return an error
- Use Set-DataverseEnvironmentVariableDefinition to create both definition and value
- If a value record doesn't exist, it will be created
- If a value record already exists, it will be updated

## RELATED LINKS

[Get-DataverseEnvironmentVariableValue](Get-DataverseEnvironmentVariableValue.md)
[Set-DataverseEnvironmentVariableDefinition](Set-DataverseEnvironmentVariableDefinition.md)
[Remove-DataverseEnvironmentVariableValue](Remove-DataverseEnvironmentVariableValue.md)
