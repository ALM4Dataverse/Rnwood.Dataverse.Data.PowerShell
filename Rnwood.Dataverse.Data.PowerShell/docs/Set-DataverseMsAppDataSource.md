---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseMsAppDataSource

## SYNOPSIS
Adds or updates a data source in a Canvas app .msapp file.

## SYNTAX

### Dataverse-FromPath
```
Set-DataverseMsAppDataSource [-MsAppPath] <String> [-TableLogicalName] <String> [-Connection <ServiceClient>]
 [-DisplayName <String>] [-DatasetName <String>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

### Collection-FromPath
```
Set-DataverseMsAppDataSource [-MsAppPath] <String> [-DataSourceName] <String> [-JsonExamplePath] <String>
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### Dataverse-FromObject
```
Set-DataverseMsAppDataSource [-CanvasApp] <PSObject> [-TableLogicalName] <String> [-Connection <ServiceClient>]
 [-DisplayName <String>] [-DatasetName <String>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

### Collection-FromObject
```
Set-DataverseMsAppDataSource [-CanvasApp] <PSObject> [-DataSourceName] <String> [-JsonExamplePath] <String>
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Adds or updates a data source in a Canvas app .msapp file. Two types of data sources are supported:

- **Dataverse table data source**: Fetches table metadata from Dataverse using the provided connection and registers it as a data source. Uses the connection's authentication.
- **Static collection data source**: Creates a collection data source from a JSON example file, inferring the schema from the data.

> **EXPERIMENTAL:** YAML-first Canvas app modification is experimental. The Power Apps MSAPP format it not fully understood/supported and the results may need to be validated in Power Apps Studio.

## EXAMPLES

### Example 1: Add a Dataverse table data source
```powershell
$conn = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -ClientId $id -ClientSecret $secret
Set-DataverseMsAppDataSource -MsAppPath "myapp.msapp" -TableLogicalName "account" -Connection $conn
```

### Example 2: Add a collection data source from JSON
```powershell
Set-DataverseMsAppDataSource -MsAppPath "myapp.msapp" -DataSourceName "Products" -JsonExamplePath "products.json"
```

### Example 3: Add a Dataverse data source using default connection
```powershell
Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive -SetAsDefault
Set-DataverseMsAppDataSource -MsAppPath "myapp.msapp" -TableLogicalName "contact"
```

## PARAMETERS

### -CanvasApp
Canvas app PSObject from Get-DataverseCanvasApp -IncludeDocument.

```yaml
Type: PSObject
Parameter Sets: Dataverse-FromObject, Collection-FromObject
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet. If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.

```yaml
Type: ServiceClient
Parameter Sets: Dataverse-FromPath, Dataverse-FromObject
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DatasetName
Optional dataset name. Defaults to 'default.cds'.

```yaml
Type: String
Parameter Sets: Dataverse-FromPath, Dataverse-FromObject
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DataSourceName
Name of the static collection data source.

```yaml
Type: String
Parameter Sets: Collection-FromPath, Collection-FromObject
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DisplayName
Optional display name to use for the data source. Defaults to the table display name from Dataverse.

```yaml
Type: String
Parameter Sets: Dataverse-FromPath, Dataverse-FromObject
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -JsonExamplePath
Path to a JSON file containing example data for the collection. The schema is inferred from the data.

```yaml
Type: String
Parameter Sets: Collection-FromPath, Collection-FromObject
Aliases:

Required: True
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MsAppPath
Path to the .msapp file.

```yaml
Type: String
Parameter Sets: Dataverse-FromPath, Collection-FromPath
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
Determines how PowerShell responds to progress updates generated by a script, cmdlet, or provider.

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

### -TableLogicalName
Logical name of the Dataverse table to add as a data source.

```yaml
Type: String
Parameter Sets: Dataverse-FromPath, Dataverse-FromObject
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

### System.Management.Automation.PSObject
## OUTPUTS

### System.Object
## NOTES

> **EXPERIMENTAL:** YAML-first Canvas app modification is experimental. The Power Apps MSAPP format it not fully understood/supported and the results may need to be validated in Power Apps Studio.

A warning is emitted at runtime each time this cmdlet runs as a reminder of the experimental status. To suppress the warning, use the common `-WarningAction SilentlyContinue` parameter.

## RELATED LINKS

[Get-DataverseMsAppDataSource](Get-DataverseMsAppDataSource.md)
[Remove-DataverseMsAppDataSource](Remove-DataverseMsAppDataSource.md)
[Get-DataverseConnection](Get-DataverseConnection.md)
