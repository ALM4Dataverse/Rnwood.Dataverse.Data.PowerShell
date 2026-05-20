---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseCloudFlowAction

## SYNOPSIS
Retrieves actions from a cloud flow in Dataverse.

## SYNTAX

### ById
```
Get-DataverseCloudFlowAction [-FlowId] <Guid> [[-ActionName] <String>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### ByName
```
Get-DataverseCloudFlowAction [-FlowName] <String> [[-ActionName] <String>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Extracts and displays individual actions from a cloud flow's definition. Actions are parsed from the flow's clientdata JSON field.

## EXAMPLES

### Example 1: Get all actions in a flow
```powershell
PS C:\> Get-DataverseCloudFlowAction -FlowName "My Flow"
```

Returns all actions in the cloud flow named "My Flow".

### Example 2: Get a specific action
```powershell
PS C:\> Get-DataverseCloudFlowAction -FlowId "00000000-0000-0000-0000-000000000000" -ActionName "Send_email"
```

Returns the "Send_email" action from the specified flow.

### Example 3: Get actions matching a pattern
```powershell
PS C:\> Get-DataverseCloudFlowAction -FlowName "My Flow" -ActionName "Send*"
```

Returns all actions whose names start with "Send" in the flow.

### Example 4: View action details
```powershell
PS C:\> Get-DataverseCloudFlowAction -FlowName "My Flow" | Select-Object Name, Type, Description
```

Gets all actions and displays their name, type, and description.

## PARAMETERS

### -ActionName
The name of a specific action to retrieve.
Supports wildcards (* and ?).
If not specified, all actions are returned.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
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

### -FlowId
The ID of the cloud flow to retrieve actions from.

```yaml
Type: Guid
Parameter Sets: ById
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -FlowName
The name of the cloud flow to retrieve actions from.

```yaml
Type: String
Parameter Sets: ByName
Aliases:

Required: True
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

### System.Guid
## OUTPUTS

### Rnwood.Dataverse.Data.PowerShell.Commands.CloudFlowActionInfo
## NOTES

## RELATED LINKS
