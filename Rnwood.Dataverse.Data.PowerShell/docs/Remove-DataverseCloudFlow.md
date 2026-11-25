---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseCloudFlow

## SYNOPSIS
Removes (deletes) a cloud flow from Dataverse.

## SYNTAX

### ById
```
Remove-DataverseCloudFlow [-Id] <Guid> [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [-WhatIf] [-Confirm] [<CommonParameters>]
```

### ByName
```
Remove-DataverseCloudFlow [-Name] <String> [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Permanently deletes a cloud flow from Dataverse. Use with caution as this operation cannot be undone. Supports WhatIf and Confirm for safe execution.

## EXAMPLES

### Example 1: Remove a flow by ID
```powershell
PS C:\> Remove-DataverseCloudFlow -Id "00000000-0000-0000-0000-000000000000"
```

Deletes the cloud flow with the specified ID. Will prompt for confirmation.

### Example 2: Remove a flow by name
```powershell
PS C:\> Remove-DataverseCloudFlow -Name "My Flow"
```

Deletes the cloud flow named "My Flow". Will prompt for confirmation.

### Example 3: Remove without confirmation
```powershell
PS C:\> Remove-DataverseCloudFlow -Name "My Flow" -Confirm:$false
```

Deletes the flow without prompting for confirmation.

### Example 4: Preview deletion with WhatIf
```powershell
PS C:\> Remove-DataverseCloudFlow -Name "My Flow" -WhatIf
```

Shows what would be deleted without actually deleting it.

## PARAMETERS

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

### -Id
The ID of the cloud flow to remove.

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
The name of the cloud flow to remove.

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
