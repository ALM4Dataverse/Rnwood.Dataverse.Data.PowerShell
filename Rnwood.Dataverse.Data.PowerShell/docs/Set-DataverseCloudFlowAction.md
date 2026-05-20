---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseCloudFlowAction

## SYNOPSIS
Creates or updates an action within a cloud flow in Dataverse.

## SYNTAX

### ById
```
Set-DataverseCloudFlowAction -FlowId <Guid> [-ActionId] <String> [-Type <String>] [-Inputs <Object>]
 [-Description <String>] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf]
 [-Confirm] [<CommonParameters>]
```

### ByName
```
Set-DataverseCloudFlowAction -FlowName <String> [-ActionId] <String> [-Type <String>] [-Inputs <Object>]
 [-Description <String>] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf]
 [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Creates or updates an action within a cloud flow by modifying the flow's clientdata JSON. If the action with the given `ActionId` already exists in the flow, it is updated. If it does not exist, a new action is created (requires the `-Type` parameter).

Can set or update action inputs, description metadata, and type.

## EXAMPLES

### Example 1: Create a new action in a flow
```powershell
PS C:\> Set-DataverseCloudFlowAction -FlowName "My Flow" -ActionId "Initialize_Counter" -Type "InitializeVariable" -Inputs @{variables=@(@{name="counter";type="Integer";value=0})}
```

Creates a new InitializeVariable action named "Initialize_Counter" in the specified flow.

### Example 2: Update action inputs with hashtable
```powershell
PS C:\> $inputs = @{ to = "user@example.com"; subject = "New Subject" }
PS C:\> Set-DataverseCloudFlowAction -FlowName "My Flow" -ActionId "Send_email" -Inputs $inputs
```

Updates the inputs for the "Send_email" action using a hashtable.

### Example 3: Update action inputs with JSON string
```powershell
PS C:\> $json = '{"to":"user@example.com","subject":"New Subject"}'
PS C:\> Set-DataverseCloudFlowAction -FlowId "00000000-0000-0000-0000-000000000000" -ActionId "Send_email" -Inputs $json
```

Updates the inputs for the "Send_email" action using a JSON string.

### Example 4: Update action description
```powershell
PS C:\> Set-DataverseCloudFlowAction -FlowName "My Flow" -ActionId "Send_email" -Description "Send notification email"
```

Updates the description metadata for the action.

### Example 5: Use WhatIf to preview changes
```powershell
PS C:\> Set-DataverseCloudFlowAction -FlowName "My Flow" -ActionId "Send_email" -Inputs $inputs -WhatIf
```

Shows what would be changed without actually updating the flow.

## PARAMETERS

### -ActionId
The ID/name of the action to create or update.

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

### -Description
The description for the action.

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

### -Inputs
The inputs for the action as a hashtable or JSON string.

```yaml
Type: Object
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

### -Type
The type of the action. Required when creating a new action. Common values include: 'Http', 'Response', 'Compose', 'InitializeVariable', 'SetVariable', 'AppendToArrayVariable', 'Condition', 'Scope', 'Foreach', 'Until', 'Switch'.

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

The `-Type` parameter is required when creating a new action. When updating an existing action, `-Type` is optional and only changes the action's type if specified.

Common Power Automate action types: 'Http' (HTTP request), 'Response' (HTTP response), 'Compose' (Data operations - compose), 'InitializeVariable', 'SetVariable', 'AppendToArrayVariable', 'AppendToStringVariable', 'IncrementVariable', 'DecrementVariable', 'Condition', 'Scope', 'Foreach', 'Until', 'Switch', 'Terminate'.

## RELATED LINKS
