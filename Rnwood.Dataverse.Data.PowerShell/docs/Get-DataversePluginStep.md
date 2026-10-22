---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataversePluginStep

## SYNOPSIS
Retrieves plugin step (SDK message processing step) records from a Dataverse environment.

## SYNTAX

### All (Default)
```
Get-DataversePluginStep [-PluginTypeId <Guid>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### ById
```
Get-DataversePluginStep -Id <Guid> [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### ByName
```
Get-DataversePluginStep -Name <String> [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### ByPluginType
```
Get-DataversePluginStep -PluginTypeId <Guid> [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
Retrieves plugin steps by ID, name, plugin type ID, or all steps.

## EXAMPLES

### Example 1
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataversePluginStep -PluginTypeId 12345678-1234-1234-1234-123456789012
```

Retrieves all plugin steps for a specific plugin type.

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

### -Id
ID of the plugin step to retrieve

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
Name of the plugin step to retrieve

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

### -PluginTypeId
Plugin type ID to filter steps by

```yaml
Type: Guid
Parameter Sets: All
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

```yaml
Type: Guid
Parameter Sets: ByPluginType
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

[Set-DataversePluginStep](Set-DataversePluginStep.md)
[Remove-DataversePluginStep](Remove-DataversePluginStep.md)
