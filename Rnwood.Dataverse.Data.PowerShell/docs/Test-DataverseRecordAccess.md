---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Test-DataverseRecordAccess

## SYNOPSIS
Tests the access rights a security principal (user or team) has for a specific record.

## SYNTAX

```
Test-DataverseRecordAccess [-TableName] <String> [-Id] <Guid> [-Principal] <Guid> [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet tests the access rights that a security principal (user or team) has for a specific Dataverse record using the RetrievePrincipalAccessRequest message.

The cmdlet returns an AccessRights enumeration value, which can include:
- None - No access
- ReadAccess - Read access
- WriteAccess - Write access
- AppendAccess - Append access
- AppendToAccess - Append to access
- CreateAccess - Create access
- DeleteAccess - Delete access
- ShareAccess - Share access
- AssignAccess - Assign access

This is useful for verifying permissions, debugging security issues, and implementing custom security logic.

## EXAMPLES

### Example 1: Check user access to a specific account record
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $userId = "87654321-4321-4321-4321-210987654321"
PS C:\> $access = Test-DataverseRecordAccess -TableName account -Id "12345678-1234-1234-1234-123456789012" -Principal $userId
PS C:\> $access
ReadAccess, WriteAccess, DeleteAccess, ShareAccess, AssignAccess
```

Tests what access rights the specified user has for the account record.

### Example 2: Check if a user has write access
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $userId = "22222222-2222-2222-2222-222222222222"
PS C:\> $access = Test-DataverseRecordAccess -TableName contact -Id "11111111-1111-1111-1111-111111111111" -Principal $userId
PS C:\> if ($access -band [Microsoft.Crm.Sdk.Messages.AccessRights]::WriteAccess) {
>>     Write-Host "User has write access"
>> } else {
>>     Write-Host "User does not have write access"
>> }
```

Checks if a user has write access to a contact record.

### Example 3: Check access for current user
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $whoAmI = Get-DataverseWhoAmI
PS C:\> $access = Test-DataverseRecordAccess -TableName opportunity -Id "33333333-3333-3333-3333-333333333333" -Principal $whoAmI.UserId
PS C:\> $access
ReadAccess, WriteAccess, AppendAccess, AppendToAccess, DeleteAccess, ShareAccess, AssignAccess
```

Checks what access the current authenticated user has to an opportunity record.

### Example 4: Pipeline usage
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $contacts = Get-DataverseRecord -TableName contact -FilterValues @{statecode=0} -Columns contactid
PS C:\> $userId = "44444444-4444-4444-4444-444444444444"
PS C:\> $contacts | ForEach-Object {
>>     $access = Test-DataverseRecordAccess -TableName contact -Id $_.contactid -Principal $userId
>>     [PSCustomObject]@{
>>         ContactId = $_.contactid
>>         HasWriteAccess = ($access -band [Microsoft.Crm.Sdk.Messages.AccessRights]::WriteAccess) -ne 0
>>     }
>> }
```

Checks write access for a user across multiple contact records.

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

### -Principal
The security principal (user or team) for which to check access rights. Must be a Guid representing the systemuser or team record ID.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String
### System.Guid
## OUTPUTS

### Microsoft.Crm.Sdk.Messages.AccessRights
## NOTES
See https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.retrieveprincipalaccessrequest?view=dataverse-sdk-latest

## RELATED LINKS

[Set-DataverseRecordAccess](Set-DataverseRecordAccess.md)

[Get-DataverseRecordAccess](Get-DataverseRecordAccess.md)

[Remove-DataverseRecordAccess](Remove-DataverseRecordAccess.md)

[Microsoft Learn: Security concepts in Dataverse](https://learn.microsoft.com/power-platform/admin/wp-security-cds)
