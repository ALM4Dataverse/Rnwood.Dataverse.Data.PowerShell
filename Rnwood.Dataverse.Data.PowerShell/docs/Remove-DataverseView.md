---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseView

## SYNOPSIS
Removes Dataverse views (savedquery and userquery entities).

## SYNTAX

```
Remove-DataverseView -Id <Guid> [-ViewType <String>] [-IfExists] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Remove-DataverseView cmdlet deletes Dataverse views from the savedquery (system views) or userquery (personal views) entities. Views define how records are displayed in model-driven apps and other Dataverse interfaces.

The cmdlet supports safe deletion with confirmation prompts and WhatIf support. Use the -IfExists parameter to suppress errors when the view doesn't exist.

## EXAMPLES

### Example 1: Remove a personal view
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseView -Id "12345678-1234-1234-1234-123456789012"
```

Removes a personal view (default) by its ID. Prompts for confirmation before deletion.

### Example 2: Remove a system view
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseView -Id "12345678-1234-1234-1234-123456789012" -ViewType "System"
```

Removes a system view by its ID. ViewType must be specified when deleting system views.

### Example 3: Remove view if it exists
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseView -Id "12345678-1234-1234-1234-123456789012" -IfExists -Confirm:$false
```

Removes a view if it exists, without raising an error if it doesn't exist. Suppresses the confirmation prompt.

### Example 4: Remove multiple views via pipeline
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseView -Name "Test*" |
    Remove-DataverseView -Confirm:$false
```

Finds all views whose names start with "Test" and removes them without confirmation prompts. The ViewType is automatically inferred from the pipeline input.

### Example 5: Remove with confirmation suppressed
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseView -Id $viewId -ViewType "Personal" -Confirm:$false
```

Removes a personal view without prompting for confirmation.

### Example 6: Use WhatIf to preview deletion
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseView -Id $viewId -ViewType "System" -WhatIf
```

Shows what would happen if the view were deleted, without actually deleting it.

### Example 7: Remove views in a safe workflow
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Get test views
PS C:\> $testViews = Get-DataverseView -Name "DEV_*"
PS C:\> 
PS C:\> # Preview what will be deleted
PS C:\> $testViews | Remove-DataverseView -WhatIf
PS C:\> 
PS C:\> # Confirm and delete
PS C:\> $testViews | Remove-DataverseView
```

Demonstrates a safe workflow: first preview with WhatIf, then delete with confirmation prompts.

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
ID of the view to remove.
Required parameter.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -IfExists
If specified, the cmdlet will not raise an error if the view does not exist.
Useful for idempotent scripts that may run multiple times.

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

### -ViewType
Specify "System" to remove a system view (savedquery) or "Personal" to remove a personal view (userquery).
Default is "Personal" if not specified.
When used with pipeline input from Get-DataverseView, this parameter is automatically inferred.

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: System, Personal

Required: False
Position: Named
Default value: Personal
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
Default value: True
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

**Safety Features:**
- Prompts for confirmation by default (use `-Confirm:$false` to suppress)
- Supports WhatIf to preview operations without making changes
- IfExists parameter prevents errors when view doesn't exist

**View Types:**
- Personal views (userquery): Default, user-specific views
- System views (savedquery): Shared views accessible to all users
- Some system views may be managed and cannot be deleted

**Pipeline Support:**
- Accepts view objects from Get-DataverseView
- Automatically extracts Id and ViewType from pipeline input
- Enables scenarios like `Get-DataverseView | Where-Object | Remove-DataverseView`

**Error Handling:**
- Without -IfExists: Throws error if view doesn't exist
- With -IfExists: Silently continues if view doesn't exist
- Writes verbose output when -IfExists suppresses an error

**Best Practices:**
- Always use -WhatIf first to preview deletions
- Use -IfExists in idempotent scripts
- Be careful with system views - they affect all users
- Consider backing up views with Get-DataverseView before deletion

## RELATED LINKS

[View Management Documentation](../../docs/core-concepts/view-management.md)

[Get-DataverseView](Get-DataverseView.md)

[Set-DataverseView](Set-DataverseView.md)

[Querying Records](../../docs/core-concepts/querying.md)
