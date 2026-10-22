---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseSolutionUpgrade

## SYNOPSIS
Applies a staged solution upgrade by deleting the original solution and promoting the holding solution.

## SYNTAX

```
Invoke-DataverseSolutionUpgrade [-SolutionName] <String> [-IfExists] [-PollingIntervalSeconds <Int32>]
 [-TimeoutSeconds <Int32>] [-SolutionHistoryWaitSeconds <Int32>] [-SkipSolutionHistoryCheck]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet completes a solution upgrade that was previously staged using Import-DataverseSolution with -Mode HoldingSolution or -Mode StageAndUpgrade. It deletes the original solution and promotes the holding solution (named SolutionName_Upgrade) to become the active solution using an asynchronous job with progress reporting.

The cmdlet uses the Microsoft.Crm.Sdk.Messages.DeleteAndPromoteRequest wrapped in Microsoft.Xrm.Sdk.Messages.ExecuteAsyncRequest to perform the upgrade asynchronously. The cmdlet monitors the async operation and reports progress using PowerShell's progress bar.

Before checking holding-solution state, the cmdlet waits briefly for any in-progress solution history operations for the solution or its holding solution to finish, unless skipped.

**Typical upgrade workflow:**
1. Import a new version of the solution using `Import-DataverseSolution -Mode HoldingSolution` or `-Mode StageAndUpgrade`. This creates a holding solution named `SolutionName_Upgrade`.
2. Test the holding solution to verify it works correctly.
3. Run `Invoke-DataverseSolutionUpgrade -SolutionName "SolutionName"` to complete the upgrade by deleting the old solution and promoting the holding solution.

## EXAMPLES

### Example 1: Apply a solution upgrade
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Invoke-DataverseSolutionUpgrade -SolutionName "MySolution"

SolutionName      : MySolution
HoldingSolutionName : MySolution_Upgrade
Status           : Success
```

Applies the staged upgrade for MySolution, promoting MySolution_Upgrade to MySolution.

### Example 2: Apply upgrade with existence check
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Invoke-DataverseSolutionUpgrade -SolutionName "MySolution" -IfExists

WARNING: Holding solution 'MySolution_Upgrade' does not exist. Skipping upgrade operation.
```

Checks if the holding solution exists before attempting to apply the upgrade. If it doesn't exist, skips the operation with a warning.

### Example 3: Apply upgrade with confirmation
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Invoke-DataverseSolutionUpgrade -SolutionName "MySolution" -Confirm

Confirm
Are you sure you want to perform this action?
Performing the operation "Apply upgrade" on target "Solution 'MySolution'".
[Y] Yes  [A] Yes to All  [N] No  [L] No to All  [S] Suspend  [?] Help (default is "Y"): Y

SolutionName        : MySolution
HoldingSolutionName : MySolution_Upgrade
AsyncOperationId    : {87654321-4321-4321-4321-210987654321}
Status              : Succeeded
```

Applies the upgrade with explicit confirmation.

### Example 4: Complete upgrade workflow
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Step 1: Import solution as holding solution for upgrade
PS C:\> Import-DataverseSolution -InFile "C:\Solutions\MySolution_v2.zip" -Mode HoldingSolution

ImportJobId      : {12345678-1234-1234-1234-123456789012}
AsyncOperationId : {87654321-4321-4321-4321-210987654321}
Status          : Succeeded

PS C:\> # Step 2: Test the holding solution (MySolution_Upgrade)
PS C:\> # ... perform testing ...

PS C:\> # Step 3: Apply the upgrade to promote the holding solution
PS C:\> Invoke-DataverseSolutionUpgrade -SolutionName "MySolution"

SolutionName        : MySolution
HoldingSolutionName : MySolution_Upgrade
AsyncOperationId    : {11111111-2222-3333-4444-555555555555}
Status              : Succeeded
```

Complete workflow showing how to stage a solution upgrade and then apply it.

### Example 5: Apply upgrade without confirmation
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Invoke-DataverseSolutionUpgrade -SolutionName "MySolution" -Confirm:$false

SolutionName        : MySolution
HoldingSolutionName : MySolution_Upgrade
AsyncOperationId    : {abcdef12-3456-7890-abcd-ef1234567890}
Status              : Succeeded
```

Applies the upgrade without prompting for confirmation.

### Example 6: Conditional upgrade in deployment script
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Import new version as holding solution
PS C:\> Import-DataverseSolution -InFile "C:\Solutions\MySolution.zip" -Mode HoldingSolution

PS C:\> # Apply upgrade only if holding solution exists
PS C:\> Invoke-DataverseSolutionUpgrade -SolutionName "MySolution" -IfExists
```

Useful in deployment scripts where you want to conditionally apply an upgrade only if a holding solution exists.

### Example 7: Apply upgrade with custom timeout
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Invoke-DataverseSolutionUpgrade -SolutionName "LargeSolution" -TimeoutSeconds 7200 -PollingIntervalSeconds 10

SolutionName        : LargeSolution
HoldingSolutionName : LargeSolution_Upgrade
AsyncOperationId    : {99999999-8888-7777-6666-555555555555}
Status              : Succeeded
```

Applies the upgrade for a large solution with a 2-hour timeout and checks status every 10 seconds.

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

### -IfExists
Check if the holding solution (SolutionName_Upgrade) exists before attempting to apply the upgrade. If it doesn't exist, skip the operation with a warning message instead of throwing an error.

This is useful in deployment scripts where you want to conditionally apply an upgrade only if a holding solution was staged.

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

### -PollingIntervalSeconds
Polling interval in seconds for checking upgrade status. Default is 5 seconds.

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

### -SolutionName
The unique name of the solution to upgrade (e.g., 'MySolution'). The holding solution must exist with the name 'MySolution_Upgrade'.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TimeoutSeconds
Timeout in seconds for the upgrade operation. Default is 3600 seconds (1 hour).

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

### None
## OUTPUTS

### System.Object
## NOTES

This cmdlet uses the DeleteAndPromoteRequest API wrapped in ExecuteAsyncRequest to asynchronously and atomically delete the original solution and promote the holding solution. The cmdlet monitors the async operation and reports progress using PowerShell's progress bar.

**Important considerations:**
- The holding solution (SolutionName_Upgrade) must exist before running this cmdlet
- The operation is atomic - both the delete and promote happen together
- This operation cannot be undone - ensure you've tested the holding solution before applying the upgrade
- If the -IfExists switch is not used and the holding solution doesn't exist, an error will occur
- The operation runs asynchronously and the cmdlet monitors progress with configurable timeout and polling intervals
- Progress is reported using PowerShell's progress API showing current status
- The async operation can be canceled by pressing Ctrl+C

**Upgrade workflow:**
1. Stage the upgrade using `Import-DataverseSolution -Mode HoldingSolution` or `-Mode StageAndUpgrade`
2. Test the holding solution to ensure it works correctly
3. Run this cmdlet to complete the upgrade

See also:
- https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.deleteandpromoterequest
- https://learn.microsoft.com/en-us/dotnet/api/microsoft.xrm.sdk.messages.executeasyncrequest

## RELATED LINKS

[Import-DataverseSolution]()
[Remove-DataverseSolution]()
[Get-DataverseSolution]()
