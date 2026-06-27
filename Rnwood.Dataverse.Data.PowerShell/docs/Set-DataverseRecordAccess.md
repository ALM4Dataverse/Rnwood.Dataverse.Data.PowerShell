---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseRecordAccess

## SYNOPSIS
Grants or modifies access rights for a security principal (user or team) on a specific record.

## SYNTAX

```
Set-DataverseRecordAccess [-TableName] <String> [-Id] <Guid> [-Principal] <Guid> [-AccessRights] <AccessRights>
 [-IsTeam] [-Replace] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

## DESCRIPTION

This cmdlet grants or modifies access rights for a security principal (user or team) on a specific Dataverse record using the GrantAccessRequest or ModifyAccessRequest messages.

The cmdlet has two modes of operation:

**Additive (default)**: Adds the specified rights to any existing rights the principal has.

**Replace (-Replace switch)**: Replaces all existing rights with only the specified rights.

Access rights can be combined using bitwise OR to grant multiple permissions at once.

## EXAMPLES

### Example 1: Grant read and write access to a user (additive)
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $accessRights = [Microsoft.Crm.Sdk.Messages.AccessRights]::ReadAccess -bor [Microsoft.Crm.Sdk.Messages.AccessRights]::WriteAccess
PS C:\> Set-DataverseRecordAccess -TableName account -Id "12345678-1234-1234-1234-123456789012" -Principal "87654321-4321-4321-4321-210987654321" -AccessRights $accessRights
```

Grants read and write access to the specified user for the account record. If the user already has other rights, they will be retained.

### Example 2: Add delete access to existing permissions
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # User already has read/write access, now add delete
PS C:\> Set-DataverseRecordAccess -TableName contact -Id "11111111-1111-1111-1111-111111111111" -Principal "22222222-2222-2222-2222-222222222222" -AccessRights DeleteAccess
```

Adds delete access to the user's existing permissions. The user will now have read, write, and delete access.

### Example 3: Replace all access with only read access
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # User has read, write, delete access - replace with only read
PS C:\> Set-DataverseRecordAccess -TableName opportunity -Id "33333333-3333-3333-3333-333333333333" -Principal "44444444-4444-4444-4444-444444444444" -AccessRights ReadAccess -Replace
```

Replaces all existing access rights with only read access. Any write or delete permissions are removed.

### Example 4: Grant full access to a team
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $fullAccess = [Microsoft.Crm.Sdk.Messages.AccessRights]::ReadAccess -bor [Microsoft.Crm.Sdk.Messages.AccessRights]::WriteAccess -bor [Microsoft.Crm.Sdk.Messages.AccessRights]::DeleteAccess -bor [Microsoft.Crm.Sdk.Messages.AccessRights]::ShareAccess -bor [Microsoft.Crm.Sdk.Messages.AccessRights]::AssignAccess
PS C:\> Set-DataverseRecordAccess -TableName account -Id "55555555-5555-5555-5555-555555555555" -Principal "66666666-6666-6666-6666-666666666666" -AccessRights $fullAccess -IsTeam
```

Grants full access to a team for the account record.

### Example 5: Use with pipeline
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $records = Get-DataverseRecord -TableName contact -FilterValues @{statecode=0} -Columns contactid
PS C:\> $userId = "77777777-7777-7777-7777-777777777777"
PS C:\> $records | ForEach-Object {
>>     Set-DataverseRecordAccess -TableName contact -Id $_.contactid -Principal $userId -AccessRights ReadAccess
>> }
```

Grants read access to a user for all active contact records.

## PARAMETERS

### -AccessRights
The access rights to grant. Can be combined using bitwise OR.

Valid values include:
- ReadAccess - Read access
- WriteAccess - Write access  
- AppendAccess - Append access
- AppendToAccess - Append to access
- CreateAccess - Create access
- DeleteAccess - Delete access
- ShareAccess - Share access
- AssignAccess - Assign access

```yaml
Type: AccessRights
Parameter Sets: (All)
Aliases:
Accepted values: None, ReadAccess, WriteAccess, AppendAccess, AppendToAccess, CreateAccess, DeleteAccess, ShareAccess, AssignAccess

Required: True
Position: 3
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

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
The security principal (user or team) for which to set access rights. Must be a Guid representing the systemuser or team record ID.

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

### -Replace
If specified, replaces all existing access rights with the specified rights. Otherwise, adds the specified rights to existing access.

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
### System.Guid
## OUTPUTS

### System.Object
## NOTES
See https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.grantaccessrequest?view=dataverse-sdk-latest

## RELATED LINKS

[Get-DataverseRecordAccess](Get-DataverseRecordAccess.md)

[Test-DataverseRecordAccess](Test-DataverseRecordAccess.md)

[Remove-DataverseRecordAccess](Remove-DataverseRecordAccess.md)

[Microsoft Learn: Security concepts in Dataverse](https://learn.microsoft.com/power-platform/admin/wp-security-cds)
