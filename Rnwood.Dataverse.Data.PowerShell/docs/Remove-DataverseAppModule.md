---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseAppModule

## SYNOPSIS
Removes an app module (model-driven app) from Dataverse.

## SYNTAX

### ById
```
Remove-DataverseAppModule -Id <Guid> [-IfExists] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### ByUniqueName
```
Remove-DataverseAppModule -UniqueName <String> [-IfExists] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Remove-DataverseAppModule cmdlet deletes model-driven apps from Dataverse. Apps can be removed by ID or UniqueName.

Use the IfExists parameter to suppress errors when attempting to remove apps that don't exist.

## EXAMPLES

### Example 1: Remove an app module by ID
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseAppModule -Id "12345678-1234-1234-1234-123456789012" -Confirm:$false
```

Removes the app module with the specified ID without confirmation.

### Example 2: Remove an app module by UniqueName
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseAppModule -UniqueName "myapp_unique" -Confirm:$false
```

Removes the app module with the specified unique name.

### Example 3: Safe removal with IfExists
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseAppModule -UniqueName "maybe_exists" -IfExists -Confirm:$false
```

Attempts to remove the app but doesn't error if it doesn't exist.

### Example 4: Use WhatIf to preview
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseAppModule -UniqueName "myapp" -WhatIf
```

Shows what would happen without actually removing the app.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet.
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
ID of the app module to remove

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

### -IfExists
If specified, the cmdlet will not raise an error if the app module does not exist

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

### -UniqueName
Unique name of the app module to remove

```yaml
Type: String
Parameter Sets: ByUniqueName
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
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
### System.String
## OUTPUTS

### System.Object
## NOTES

**IfExists Behavior:**
- When specified, suppresses errors if the app doesn't exist
- Useful in cleanup scripts or idempotent deployment scenarios
- Without IfExists, throws an error if app not found

**Identification:**
- Apps can be identified by ID (Guid) or UniqueName (string)
- UniqueName is more portable across environments
- UniqueName parameter resolves to ID internally

**Confirmation:**
- By default, prompts for confirmation (ConfirmImpact = Medium)
- Use -Confirm:$false to skip confirmation
- Use -WhatIf to preview without executing

**Cascade Behavior:**
- Removing an app module may also remove related app module components
- Check dependent components before removal

## RELATED LINKS

[Get-DataverseAppModule](Get-DataverseAppModule.md)

[Set-DataverseAppModule](Set-DataverseAppModule.md)

[Remove-DataverseAppModuleComponent](Remove-DataverseAppModuleComponent.md)
