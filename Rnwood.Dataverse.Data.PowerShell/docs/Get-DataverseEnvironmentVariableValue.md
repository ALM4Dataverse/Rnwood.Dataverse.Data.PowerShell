---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseEnvironmentVariableValue

## SYNOPSIS
Gets environment variable values from Dataverse.

## SYNTAX

```
Get-DataverseEnvironmentVariableValue [[-SchemaName] <String>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Gets environment variable values from Dataverse. This cmdlet returns only the value records, not the definitions.
Use Get-DataverseEnvironmentVariableDefinition to retrieve both definitions and their associated values.

## EXAMPLES

### Example 1: Get all environment variable values
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseEnvironmentVariableValue
```

Retrieves all environment variable values in the environment.

### Example 2: Get a specific environment variable value
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseEnvironmentVariableValue -SchemaName "new_apiurl"
```

Retrieves the value for the environment variable with schema name "new_apiurl".

### Example 3: Get environment variable values with wildcard
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseEnvironmentVariableValue -SchemaName "new_api*"
```

Retrieves all environment variable values where the schema name starts with "new_api".

### Example 4: Get values from pipeline
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> "new_apiurl", "new_apikey" | Get-DataverseEnvironmentVariableValue
```

Retrieves values for multiple environment variables via pipeline.

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
Schema name of the environment variable to retrieve values for. Supports wildcards (* and ?).

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES
- This cmdlet retrieves only value records
- Use Get-DataverseEnvironmentVariableDefinition to get both definitions and values together
- Values without a corresponding definition will not be returned

## RELATED LINKS

[Get-DataverseEnvironmentVariableDefinition](Get-DataverseEnvironmentVariableDefinition.md)
[Set-DataverseEnvironmentVariableValue](Set-DataverseEnvironmentVariableValue.md)
[Remove-DataverseEnvironmentVariableValue](Remove-DataverseEnvironmentVariableValue.md)
