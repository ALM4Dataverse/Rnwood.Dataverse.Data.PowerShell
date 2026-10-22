---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseOptionSetMetadata

## SYNOPSIS
Deletes a global option set from Dataverse.

## SYNTAX

```
Remove-DataverseOptionSetMetadata [-Name] <String> [-IfExists] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The `Remove-DataverseOptionSetMetadata` cmdlet permanently deletes a global option set from Dataverse. This operation cannot be undone and will delete:
- The global option set definition
- Any references to this option set (fields using this global option set)

**⚠️ WARNING: This is a destructive operation that permanently deletes metadata!**

By default, this cmdlet prompts for confirmation (ConfirmImpact = High). Use `-Confirm:$false` to suppress the confirmation prompt.

## EXAMPLES

### Example 1: Delete a global option set with confirmation
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseOptionSetMetadata -Name new_customstatus

Confirm
Are you sure you want to perform this action?
Performing the operation "Delete" on target "Global option set 'new_customstatus'".
[Y] Yes  [A] Yes to All  [N] No  [L] No to All  [S] Suspend  [?] Help (default is "Y"): Y
```

Deletes the global option set `new_customstatus` after prompting for confirmation.

### Example 2: Delete a global option set without confirmation
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseOptionSetMetadata -Name new_obsoletestatus -Confirm:$false
```

Deletes the global option set `new_obsoletestatus` without prompting.

### Example 3: Delete with IfExists flag
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseOptionSetMetadata -Name new_maybemissing -IfExists -Confirm:$false
```

Attempts to delete the global option set but won't error if it doesn't exist.

### Example 4: Use WhatIf to preview deletion
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseOptionSetMetadata -Name new_testoptionset -WhatIf

What if: Performing the operation "Delete" on target "Global option set 'new_testoptionset'".
```

Shows what would happen without actually deleting the option set.

### Example 5: Delete multiple global option sets
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> @("new_status1", "new_status2", "new_status3") | 
    Remove-DataverseOptionSetMetadata -Confirm:$false
```

Deletes multiple global option sets.

### Example 6: Delete with error handling
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> try {
    Remove-DataverseOptionSetMetadata -Name new_deprecated -Confirm:$false -ErrorAction Stop
    Write-Host "Global option set deleted successfully"
} catch {
    Write-Error "Failed to delete global option set: $($_.Exception.Message)"
}
```

Deletes a global option set with proper error handling.

### Example 7: Cleanup test option sets
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseOptionSetMetadata | 
    Where-Object { $_.Name -like "new_test*" } | 
    ForEach-Object {
        Write-Host "Deleting: $($_.Name)"
        Remove-DataverseOptionSetMetadata -Name $_.Name -Confirm:$false
    }
```

Finds and deletes all test global option sets (starting with "new_test").

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet.

If not provided, uses the default connection set via `Get-DataverseConnection -SetAsDefault`.

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

### -IfExists
If specified, the cmdlet will not raise an error if the global option set does not exist.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name
Name of the global option set to delete.

**⚠️ WARNING:** Deleting a global option set is permanent and cannot be undone.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
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

**Note:** This cmdlet has ConfirmImpact = High, so confirmation is required by default unless `-Confirm:$false` is specified.

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
Shows what would happen if the cmdlet runs. The cmdlet is not run.

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

### System.String
## OUTPUTS

### System.Object
## NOTES

This cmdlet provides programmatic access to Dataverse metadata. For comprehensive documentation and examples, see the metadata concept guide at docs/core-concepts/metadata.md

## RELATED LINKS

[Get-DataverseOptionSetMetadata](Get-DataverseOptionSetMetadata.md)

[Set-DataverseOptionSetMetadata](Set-DataverseOptionSetMetadata.md)

[Microsoft Learn: Delete a global choice](https://learn.microsoft.com/power-apps/maker/data-platform/custom-picklists)
