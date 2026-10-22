---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseConnectionAsDefault

## SYNOPSIS
Sets the specified Dataverse connection as the default connection for cmdlets that don't specify a connection.

## SYNTAX

### Set
```
Set-DataverseConnectionAsDefault -Connection <ServiceClient> [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### Clear
```
Set-DataverseConnectionAsDefault [-Clear] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Sets the specified Dataverse connection as the default connection for cmdlets that don't specify a connection. This allows other cmdlets to use this connection without explicitly passing the -Connection parameter.

## EXAMPLES

### Example 1
```powershell
PS C:\> $connection = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive
PS C:\> Set-DataverseConnectionAsDefault -Connection $connection
```

Sets the interactively authenticated connection as the default.

## PARAMETERS

### -Clear
Clear the default Dataverse connection.

```yaml
Type: SwitchParameter
Parameter Sets: Clear
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet.

```yaml
Type: ServiceClient
Parameter Sets: Set
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
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

### Microsoft.PowerPlatform.Dataverse.Client.ServiceClient
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
