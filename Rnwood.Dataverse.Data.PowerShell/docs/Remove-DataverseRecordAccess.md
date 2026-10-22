---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseRecordAccess

## SYNOPSIS
Revokes access rights for a security principal (user or team) on a specific record.

## SYNTAX

```
Remove-DataverseRecordAccess [-TableName] <String> [-Id] <Guid> [-Principal] <Guid> [-IsTeam]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet revokes all access rights for a security principal (user or team) on a specific Dataverse record using the RevokeAccessRequest message.

After revocation, the principal will no longer have explicit shared access to the record. However, they may still have access through:
- Record ownership
- Security roles
- Team membership
- Hierarchy security

This cmdlet has ConfirmImpact.Medium and prompts for confirmation by default.

## EXAMPLES

### Example 1: Revoke access from a user
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseRecordAccess -TableName account -Id "12345678-1234-1234-1234-123456789012" -Principal "87654321-4321-4321-4321-210987654321"
```

Revokes all access from the specified user for the account record. Prompts for confirmation.

### Example 2: Revoke access from a team without confirmation
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseRecordAccess -TableName contact -Id "11111111-1111-1111-1111-111111111111" -Principal "22222222-2222-2222-2222-222222222222" -IsTeam -Confirm:$false
```

Revokes all access from the specified team for the contact record without prompting for confirmation.

### Example 3: Remove access from multiple principals
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $accessList = Get-DataverseRecordAccess -TableName opportunity -Id "33333333-3333-3333-3333-333333333333"
PS C:\> $whoAmI = Get-DataverseWhoAmI
PS C:\> $accessList | Where-Object { $_.Principal.Id -ne $whoAmI.UserId } | ForEach-Object {
>>     Remove-DataverseRecordAccess -TableName opportunity -Id "33333333-3333-3333-3333-333333333333" -Principal $_.Principal.Id -Confirm:$false
>> }
```

Removes shared access from all principals except the current user.

### Example 4: Verify access was removed
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseRecordAccess -TableName contact -Id "11111111-1111-1111-1111-111111111111" -Principal "44444444-4444-4444-4444-444444444444" -Confirm:$false
PS C:\> $access = Test-DataverseRecordAccess -TableName contact -Id "11111111-1111-1111-1111-111111111111" -Principal "44444444-4444-4444-4444-444444444444"
PS C:\> Write-Host "Access after removal: $access"
Access after removal: None
```

Removes access and verifies it was removed successfully.

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

### -Id
Id of the record.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -IsTeam
Specify if the principal is a team (default is systemuser).

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

### -Principal
The security principal (user or team) for which to revoke access rights. Must be a Guid representing the systemuser or team record ID.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: True
Position: 2
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

### -TableName
Logical name of the table.

```yaml
Type: String
Parameter Sets: (All)
Aliases: EntityName

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Confirm
Prompts you for confirmation before running the cmdlet. This is enabled by default (ConfirmImpact.Medium).

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
### System.Guid
## OUTPUTS

### System.Object
## NOTES
See https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.revokeaccessrequest?view=dataverse-sdk-latest

## RELATED LINKS

[Set-DataverseRecordAccess](Set-DataverseRecordAccess.md)

[Get-DataverseRecordAccess](Get-DataverseRecordAccess.md)

[Test-DataverseRecordAccess](Test-DataverseRecordAccess.md)

[Microsoft Learn: Security concepts in Dataverse](https://learn.microsoft.com/power-platform/admin/wp-security-cds)
