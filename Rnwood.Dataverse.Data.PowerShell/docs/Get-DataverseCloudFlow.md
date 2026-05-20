---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseCloudFlow

## SYNOPSIS
Retrieves cloud flow information from a Dataverse environment.

## SYNTAX

```
Get-DataverseCloudFlow [[-Name] <String>] [-Id <Guid>] [-Activated] [-Draft] [-IncludeClientData]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Gets information about cloud flows (modern flows) stored in Dataverse. Cloud flows are stored in the workflow entity with category=5.
You can filter flows by name, ID, or state (activated/draft).

## EXAMPLES

### Example 1: Get all cloud flows
```powershell
PS C:\> Get-DataverseCloudFlow
```

Returns all cloud flows in the Dataverse environment.

### Example 2: Get a specific flow by name
```powershell
PS C:\> Get-DataverseCloudFlow -Name "My Flow"
```

Returns the cloud flow with the exact name "My Flow".

### Example 3: Get flows with wildcard name filter
```powershell
PS C:\> Get-DataverseCloudFlow -Name "Sales*"
```

Returns all cloud flows whose names start with "Sales".

### Example 4: Get only activated flows
```powershell
PS C:\> Get-DataverseCloudFlow -Activated
```

Returns only cloud flows that are currently activated (running).

### Example 5: Get flow definition with actions
```powershell
PS C:\> Get-DataverseCloudFlow -Name "My Flow" -IncludeClientData
```

Returns the cloud flow including its clientdata JSON which contains the flow definition and actions.

## PARAMETERS

### -Activated
Filter to return only activated cloud flows.

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

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet, or string specifying Dataverse organization URL (e.g.
http://server.com/MyOrg/).
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

### -Draft
Filter to return only draft cloud flows.

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

### -Id
The ID of the cloud flow to retrieve.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IncludeClientData
Include the client data JSON containing the flow definition in the output.

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

### -Name
The name of the cloud flow to retrieve.
Supports wildcards (* and ?).
If not specified, all cloud flows are returned.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
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

### None
## OUTPUTS

### Rnwood.Dataverse.Data.PowerShell.Commands.CloudFlowInfo
## NOTES

## RELATED LINKS
