---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataversePluginAssembly

## SYNOPSIS
Retrieves plugin assembly records from a Dataverse environment.

## SYNTAX

### All (Default)
```
Get-DataversePluginAssembly [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### ById
```
Get-DataversePluginAssembly -Id <Guid> [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### ByName
```
Get-DataversePluginAssembly -Name <String> [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
The Get-DataversePluginAssembly cmdlet retrieves plugin assembly records from a Dataverse environment. You can retrieve assemblies by ID, name, or get all assemblies.

## EXAMPLES

### Example 1: Get all plugin assemblies
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataversePluginAssembly -All
```

Retrieves all plugin assemblies in the environment.

### Example 2: Get plugin assembly by ID
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataversePluginAssembly -Id 12345678-1234-1234-1234-123456789012
```

Retrieves a specific plugin assembly by its ID.

### Example 3: Get plugin assembly by name
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataversePluginAssembly -Name "MyPluginAssembly"
```

Retrieves a plugin assembly by its name.

## PARAMETERS

### -Connection
The Dataverse ServiceClient connection to use. If not specified, the default connection is used.

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

### -Id
The ID of the plugin assembly to retrieve.

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

### -Name
The name of the plugin assembly to retrieve.

```yaml
Type: String
Parameter Sets: ByName
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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Guid
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES

## RELATED LINKS

[Set-DataversePluginAssembly](Set-DataversePluginAssembly.md)
[Remove-DataversePluginAssembly](Remove-DataversePluginAssembly.md)
