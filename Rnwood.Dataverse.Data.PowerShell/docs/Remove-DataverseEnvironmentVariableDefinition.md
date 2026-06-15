---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseEnvironmentVariableDefinition

## SYNOPSIS
Removes environment variable definitions from Dataverse.

## SYNTAX

```
Remove-DataverseEnvironmentVariableDefinition [-SchemaName] <String> [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Removes environment variable definitions from Dataverse. This will also remove any associated values for the definition.

## EXAMPLES

### Example 1: Remove an environment variable definition
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseEnvironmentVariableDefinition -SchemaName "new_apiurl"
```

Removes the environment variable definition "new_apiurl" and any associated value.

### Example 2: Remove with confirmation
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseEnvironmentVariableDefinition -SchemaName "new_apiurl" -Confirm
```

Prompts for confirmation before removing the definition.

### Example 3: Remove with WhatIf
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseEnvironmentVariableDefinition -SchemaName "new_apiurl" -WhatIf
```

Shows what would happen if the cmdlet runs without actually removing anything.

### Example 4: Remove from pipeline
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseEnvironmentVariableDefinition -SchemaName "new_test*" | 
    Remove-DataverseEnvironmentVariableDefinition
```

Removes all environment variable definitions with schema names starting with "new_test".

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
Schema name of the environment variable definition to remove.

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
- Removes both the definition and any associated values
- If the definition doesn't exist, an error will be returned
- Consider the impact on solutions that reference this environment variable before removing

## RELATED LINKS

[Get-DataverseEnvironmentVariableDefinition](Get-DataverseEnvironmentVariableDefinition.md)
[Set-DataverseEnvironmentVariableDefinition](Set-DataverseEnvironmentVariableDefinition.md)
