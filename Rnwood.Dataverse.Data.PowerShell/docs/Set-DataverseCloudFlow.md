---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseCloudFlow

## SYNOPSIS
Creates or updates a cloud flow in Dataverse.

## SYNTAX

### ById
```
Set-DataverseCloudFlow [-Id] <Guid> [-NewName <String>] [-Description <String>] [-ClientData <String>]
 [-Activate] [-Deactivate] [-PassThru] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [-WhatIf] [-Confirm] [<CommonParameters>]
```

### ByName
```
Set-DataverseCloudFlow [-Name] <String> [-NewName <String>] [-Description <String>] [-ClientData <String>]
 [-Activate] [-Deactivate] [-PassThru] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Creates or updates a cloud flow's properties (name, description, definition) or changes its state (activate/deactivate).

When the `ByName` parameter set is used and no cloud flow with that name exists, a new flow is created. If a flow with that name already exists, it is updated. The `ById` parameter set always updates an existing flow.

When creating a new flow without specifying `-ClientData`, a minimal instant (manual button) flow definition is used as the default.

Supports WhatIf and Confirm for safe execution. Use `-PassThru` to return the ID of the created or updated flow.

## EXAMPLES

### Example 1: Create a new flow (or update if it already exists)
```powershell
PS C:\> $flowId = Set-DataverseCloudFlow -Name "My Flow" -Description "My flow description" -PassThru
```

Creates a new cloud flow named "My Flow" if it does not already exist, or updates the description if it does. Returns the flow ID.

### Example 2: Create a flow with a custom definition
```powershell
PS C:\> $clientData = Get-Content -Raw 'myflow.json'
PS C:\> Set-DataverseCloudFlow -Name "My Custom Flow" -ClientData $clientData
```

Creates a new cloud flow with a custom flow definition JSON, or updates the definition of an existing flow with that name.

### Example 3: Rename a flow
```powershell
PS C:\> Set-DataverseCloudFlow -Name "My Flow" -NewName "My Renamed Flow"
```

Renames the existing cloud flow from "My Flow" to "My Renamed Flow".

### Example 4: Activate a flow
```powershell
PS C:\> Set-DataverseCloudFlow -Id "00000000-0000-0000-0000-000000000000" -Activate
```

Activates (turns on) the cloud flow with the specified ID.

### Example 5: Deactivate a flow
```powershell
PS C:\> Set-DataverseCloudFlow -Name "My Flow" -Deactivate
```

Deactivates (turns off) the cloud flow named "My Flow".

### Example 6: Update description
```powershell
PS C:\> Set-DataverseCloudFlow -Name "My Flow" -Description "Updated description"
```

Updates the description of the existing cloud flow.

### Example 7: Use WhatIf to preview changes
```powershell
PS C:\> Set-DataverseCloudFlow -Name "My Flow" -Activate -WhatIf
```

Shows what would happen if the flow were activated without actually activating it.

## PARAMETERS

### -Activate
Activate the cloud flow.

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

### -ClientData
The flow definition JSON (clientdata). When creating a new flow and this is not specified, a minimal instant (manual button) flow definition is used as the default. Use this parameter to provide a custom flow definition or to update an existing flow's definition.

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

### -Deactivate
Deactivate the cloud flow (set to draft).

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

### -Description
The description for the cloud flow.

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

### -Id
The ID of the cloud flow to update. Use this parameter set to update an existing flow by its ID.

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

### -Name
The name of the cloud flow. If a flow with this name exists it will be updated; otherwise a new flow is created.

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

### -NewName
The new display name for the cloud flow (only applies when updating an existing flow).

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

### -PassThru
Return the ID of the created or updated cloud flow.

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

### System.Guid
## NOTES

When creating a new flow using `-Name`, if no `-ClientData` is provided, the flow is created with a minimal instant (manual button trigger) definition. This ensures the flow is properly registered in the Power Automate service and can be subsequently updated with actions using `Set-DataverseCloudFlowAction`.

Cloud flow updates may require appropriate Flow platform permissions. Activating/deactivating flows requires that the calling user owns the flow or has proper delegated access.

## RELATED LINKS
