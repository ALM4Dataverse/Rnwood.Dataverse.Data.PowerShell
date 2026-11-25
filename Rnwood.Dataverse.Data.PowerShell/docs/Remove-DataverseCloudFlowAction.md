---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseCloudFlowAction

## SYNOPSIS
Removes an action from a cloud flow in Dataverse.

## SYNTAX

### ById
```
Remove-DataverseCloudFlowAction -FlowId <Guid> [-ActionId] <String> [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### ByName
```
Remove-DataverseCloudFlowAction -FlowName <String> [-ActionId] <String> [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Removes an action from a cloud flow's definition by modifying the flow's clientdata JSON. Use with caution as this modifies the flow structure.

## EXAMPLES

### Example 1: Remove an action by flow name
```powershell
PS C:\> Remove-DataverseCloudFlowAction -FlowName "My Flow" -ActionId "Send_email"
```

Removes the "Send_email" action from the flow. Will prompt for confirmation.

### Example 2: Remove an action by flow ID
```powershell
PS C:\> Remove-DataverseCloudFlowAction -FlowId "00000000-0000-0000-0000-000000000000" -ActionId "Old_action"
```

Removes the "Old_action" from the specified flow.

### Example 3: Remove without confirmation
```powershell
PS C:\> Remove-DataverseCloudFlowAction -FlowName "My Flow" -ActionId "Send_email" -Confirm:$false
```

Removes the action without prompting for confirmation.

### Example 4: Preview removal with WhatIf
```powershell
PS C:\> Remove-DataverseCloudFlowAction -FlowName "My Flow" -ActionId "Send_email" -WhatIf
```

Shows what would be removed without actually modifying the flow.

## PARAMETERS

### -ActionId
The ID/name of the action to remove.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
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
The ID of the cloud flow containing the action.

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

### -FlowName
The name of the cloud flow containing the action.

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
Shows what would happen if the cmdlet runs.
The cmdlet is not run.

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

### System.Guid
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
