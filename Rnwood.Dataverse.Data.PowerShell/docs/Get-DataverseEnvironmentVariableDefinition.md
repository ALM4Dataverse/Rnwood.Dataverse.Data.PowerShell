---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseEnvironmentVariableDefinition

## SYNOPSIS
Gets environment variable definitions from Dataverse.

## SYNTAX

```
Get-DataverseEnvironmentVariableDefinition [[-SchemaName] <String>] [-DisplayName <String>]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Gets environment variable definitions from Dataverse. This cmdlet retrieves the definition metadata (schema name, display name, description, type, default value). The Type property in the output shows the human-readable label (e.g., "String", "Number") instead of the numeric value.

## EXAMPLES

### Example 1: Get all environment variable definitions
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseEnvironmentVariableDefinition
```

Retrieves all environment variable definitions in the environment.

### Example 2: Get a specific environment variable definition
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseEnvironmentVariableDefinition -SchemaName "new_apiurl"
```

Retrieves the definition for the environment variable with schema name "new_apiurl".

### Example 3: Get environment variables by display name
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseEnvironmentVariableDefinition -DisplayName "API*"
```

Retrieves all environment variable definitions where the display name starts with "API".

### Example 4: Get definitions from pipeline
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> "new_apiurl", "new_apikey" | Get-DataverseEnvironmentVariableDefinition
```

Retrieves definitions for multiple environment variables via pipeline.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet.
If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.

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

### -DisplayName
Display name filter for environment variables (supports wildcards).

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
Schema name of the environment variable to retrieve.

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

## RELATED LINKS
