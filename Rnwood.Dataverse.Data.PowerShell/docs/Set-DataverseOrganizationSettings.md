---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseOrganizationSettings

## SYNOPSIS
Updates organization settings in the single organization record in a Dataverse environment.

## SYNTAX

```
Set-DataverseOrganizationSettings -InputObject <Object> [-OrgDbOrgSettings] [-PassThru]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Set-DataverseOrganizationSettings cmdlet updates settings in the single organization record in a Dataverse environment. The record is automatically discovered.

When -OrgDbOrgSettings is NOT specified: Updates organization table columns. Property names in InputObject must match column names.
When -OrgDbOrgSettings IS specified: Updates OrgDbOrgSettings XML. Property names are setting names. Use $null to remove a setting.

The cmdlet compares existing values with new values and only updates changed values. Verbose output shows what changed.

InputObject can be either a PSCustomObject or a Hashtable. Hashtables provide a more concise syntax.

## EXAMPLES

### Example 1: Update organization table columns
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $connection = Get-DataverseConnection -Url "https://contoso.crm.dynamics.com" -Interactive
PS C:\> $updates = [PSCustomObject]@{ name = "Contoso Corporation" }
PS C:\> Set-DataverseOrganizationSettings -InputObject $updates -Confirm:$false -Verbose
VERBOSE: Column 'name': Changing from '"Contoso"' to '"Contoso Corporation"'
VERBOSE: Updated 1 attribute(s) in organization record ...
```

Updates the organization name using a PSCustomObject. Only changed values are updated.

### Example 1B: Update organization table columns using Hashtable
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $updates = @{ name = "Contoso Corporation" }
PS C:\> Set-DataverseOrganizationSettings -InputObject $updates -Confirm:$false -Verbose
VERBOSE: Column 'name': Changing from '"Contoso"' to '"Contoso Corporation"'
VERBOSE: Updated 1 attribute(s) in organization record ...
```

Updates the organization name using a Hashtable. Hashtables provide a more concise syntax.

### Example 2: Update OrgDbOrgSettings
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $settings = @{
    MaxUploadFileSize = 10485760
    EnableBingMapsIntegration = $true
}
PS C:\> Set-DataverseOrganizationSettings -InputObject $settings -OrgDbOrgSettings -Confirm:$false -Verbose
VERBOSE: Setting 'MaxUploadFileSize': Changing from '5242880' to '10485760'
VERBOSE: Setting 'EnableBingMapsIntegration': No change (value is 'true')
VERBOSE: Updated 1 attribute(s) in organization record ...
```

Updates OrgDbOrgSettings using a Hashtable. Only changed settings are updated.

### Example 3: Remove an OrgDbOrgSettings setting
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $updates = @{
    ObsoleteGetting = $null
}
PS C:\> Set-DataverseOrganizationSettings -InputObject $updates -OrgDbOrgSettings -Confirm:$false -Verbose
VERBOSE: Setting 'ObsoleteGetting': Removing (was 'oldvalue')
VERBOSE: Updated 1 attribute(s) in organization record ...
```

Removes a setting from OrgDbOrgSettings by passing $null using a Hashtable.

### Example 4: Update with PassThru
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $updates = [PSCustomObject]@{ name = "New Name" }
PS C:\> $result = Set-DataverseOrganizationSettings -InputObject $updates -PassThru -Confirm:$false
PS C:\> $result.name
New Name
```

Updates and returns the updated record.

### Example 5: Use WhatIf to preview changes
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $updates = [PSCustomObject]@{ MaxUploadFileSize = 10485760 }
PS C:\> Set-DataverseOrganizationSettings -InputObject $updates -OrgDbOrgSettings -WhatIf
What if: Performing the operation "Update organization settings" on target "OrgDbOrgSettings in organization record ..."
```

Previews what would be updated without making actual changes.

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

### -InputObject
Object containing values to update.

When -OrgDbOrgSettings is NOT specified: Property names must match organization table column names.
When -OrgDbOrgSettings IS specified: Property names are OrgDbOrgSettings setting names. Use $null values to remove settings.

Accepts either PSCustomObject or Hashtable. Hashtables provide a more concise syntax: `@{ name = "value" }` instead of `[PSCustomObject]@{ name = "value" }`.

```yaml
Type: Object
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -OrgDbOrgSettings
If specified, updates settings within the OrgDbOrgSettings XML column instead of organization table columns.
Property names in InputObject are treated as setting names. Use $null values to remove settings.
Existing values are compared and only changed settings are updated.

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

### -PassThru
If specified, returns the updated organization record

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

### System.Object
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES
This cmdlet has a high confirm impact and will prompt for confirmation by default. Use -Confirm:$false to suppress the prompt.

## RELATED LINKS

[Get-DataverseOrganizationSettings](Get-DataverseOrganizationSettings.md)
[Get-DataverseConnection](Get-DataverseConnection.md)
