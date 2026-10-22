---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseSolution

## SYNOPSIS
Removes (uninstalls) a solution from Dataverse.

## SYNTAX

```
Remove-DataverseSolution [-UniqueName] <String> [-PollingIntervalSeconds <Int32>] [-TimeoutSeconds <Int32>]
 [-SolutionHistoryWaitSeconds <Int32>] [-SkipSolutionHistoryCheck] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet removes (uninstalls) a solution from a Dataverse environment using the asynchronous uninstall process. The operation is asynchronous and the cmdlet monitors the uninstall progress.

When removing a solution:
- All customizations contained in the solution are removed (for managed solutions)
- Unmanaged solutions only remove the solution container, not the customizations
- Dependencies must be resolved before removal (e.g., remove dependent solutions first)
- The cmdlet monitors the uninstall operation and reports progress
- Before checking solution state, the cmdlet waits briefly for any in-progress solution history operations for the solution or its related holding solution to finish, unless skipped

## EXAMPLES

### Example 1: Remove a solution
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseSolution -UniqueName "MySolution"
Solution 'My Solution' removed successfully.
```

Removes the specified solution from the environment.

### Example 2: Remove with confirmation
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseSolution -UniqueName "MySolution" -Confirm

Confirm
Are you sure you want to perform this action?
Performing the operation "Remove" on target "Solution 'MySolution'".
[Y] Yes  [A] Yes to All  [N] No  [L] No to All  [S] Suspend  [?] Help (default is "Y"): Y
Solution 'My Solution' removed successfully.
```

Removes the solution with explicit confirmation.

### Example 3: Remove with custom timeout
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseSolution -UniqueName "LargeSolution" -TimeoutSeconds 1200 -PollingIntervalSeconds 10
Solution 'Large Solution' removed successfully.
```

Removes a large solution with a 20-minute timeout and checks status every 10 seconds.

### Example 4: Remove without confirmation
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseSolution -UniqueName "TestSolution" -Confirm:$false
Solution 'Test Solution' removed successfully.
```

Removes the solution without prompting for confirmation.

### Example 5: Handle solution not found error
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> try {
>>     Remove-DataverseSolution -UniqueName "NonExistentSolution" -ErrorAction Stop
>> } catch {
>>     Write-Host "Error: $_"
>> }
Error: Solution 'NonExistentSolution' not found.
```

Attempts to remove a solution that doesn't exist and handles the error.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet. If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.

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

### -PollingIntervalSeconds
Polling interval in seconds for checking uninstall status. Default is 5 seconds.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 5
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

### -SkipSolutionHistoryCheck
Skip checking solution history for in-progress operations before checking existing solution state.

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

### -SolutionHistoryWaitSeconds
Maximum time in seconds to wait for existing solution history operations to complete before checking solution state. Default is 30.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 30
Accept pipeline input: False
Accept wildcard characters: False
```

### -TimeoutSeconds
Timeout in seconds for the uninstall operation. Default is 3600 seconds (1 hour).

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 3600
Accept pipeline input: False
Accept wildcard characters: False
```

### -UniqueName
The unique name of the solution to remove.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
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

- This operation is irreversible. Make sure you have a backup if needed.
- Removing a managed solution removes all its customizations.
- Removing an unmanaged solution only removes the solution container, not the customizations.
- Dependencies must be resolved before removal (remove dependent solutions first).
- The cmdlet monitors the uninstall progress and reports via PowerShell's progress bar.
- For large solutions or slow environments, consider increasing the timeout value.

## RELATED LINKS

[Get-DataverseSolution](Get-DataverseSolution.md)

[Import-DataverseSolution](Import-DataverseSolution.md)

[Export-DataverseSolution](Export-DataverseSolution.md)
