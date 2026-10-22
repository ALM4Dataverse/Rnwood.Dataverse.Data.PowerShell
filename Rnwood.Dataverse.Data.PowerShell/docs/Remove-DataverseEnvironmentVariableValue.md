---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseEnvironmentVariableValue

## SYNOPSIS
Removes environment variable values from Dataverse.

## SYNTAX

```
Remove-DataverseEnvironmentVariableValue [-SchemaName] <String> [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Removes environment variable values from Dataverse. This cmdlet only removes the value record and preserves the definition.
Use Remove-DataverseEnvironmentVariableDefinition to remove both the definition and value.

## EXAMPLES

### Example 1: Remove an environment variable value
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseEnvironmentVariableValue -SchemaName "new_apiurl"
```

Removes the value for the environment variable "new_apiurl", but preserves the definition.

### Example 2: Remove with confirmation
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseEnvironmentVariableValue -SchemaName "new_apiurl" -Confirm
```

Prompts for confirmation before removing the value.

### Example 3: Remove value with WhatIf
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseEnvironmentVariableValue -SchemaName "new_apiurl" -WhatIf
```

Shows what would happen if the cmdlet runs without actually removing the value.

### Example 4: Remove from pipeline
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseEnvironmentVariableValue -SchemaName "new_test*" | 
    Remove-DataverseEnvironmentVariableValue
```

Removes values for all environment variables with schema names starting with "new_test".

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
Schema name of the environment variable to remove the value for.

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
- This cmdlet only removes the value record, not the definition
- The definition will remain in the environment after the value is removed
- Use Remove-DataverseEnvironmentVariableDefinition to remove both definition and value
- If the value doesn't exist, an error will be returned

## RELATED LINKS

[Get-DataverseEnvironmentVariableValue](Get-DataverseEnvironmentVariableValue.md)
[Set-DataverseEnvironmentVariableValue](Set-DataverseEnvironmentVariableValue.md)
[Remove-DataverseEnvironmentVariableDefinition](Remove-DataverseEnvironmentVariableDefinition.md)
