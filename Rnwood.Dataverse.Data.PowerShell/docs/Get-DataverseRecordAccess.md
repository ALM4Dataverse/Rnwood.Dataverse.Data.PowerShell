---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseRecordAccess

## SYNOPSIS
Retrieves all principals (users or teams) who have shared access to a specific record.

## SYNTAX

```
Get-DataverseRecordAccess [-TableName] <String> [-Id] <Guid> [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet retrieves all security principals (users or teams) who have been granted shared access to a specific Dataverse record using the RetrieveSharedPrincipalsAndAccessRequest message.

The cmdlet returns a list of PrincipalAccess objects, each containing information about a principal and their access rights to the record.

Note: This cmdlet only returns explicitly shared access. It does not return access granted through ownership or security roles.

## EXAMPLES

### Example 1: Get all principals with access to a record
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $accessList = Get-DataverseRecordAccess -TableName account -Id "12345678-1234-1234-1234-123456789012"
PS C:\> $accessList | Format-Table @{Label="Principal ID"; Expression={$_.Principal.Id}}, @{Label="Principal Type"; Expression={$_.Principal.LogicalName}}, AccessMask

Principal ID                         Principal Type AccessMask
------------                         -------------- ----------
87654321-4321-4321-4321-210987654321 systemuser     ReadAccess, WriteAccess
22222222-2222-2222-2222-222222222222 team           ReadAccess, WriteAccess, DeleteAccess
```

Retrieves all principals who have shared access to the account record.

### Example 2: Find users with write access
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $accessList = Get-DataverseRecordAccess -TableName contact -Id "11111111-1111-1111-1111-111111111111"
PS C:\> $writeAccess = $accessList | Where-Object { 
>>     ($_.AccessMask -band [Microsoft.Crm.Sdk.Messages.AccessRights]::WriteAccess) -ne 0 
>> }
PS C:\> $writeAccess | ForEach-Object { 
>>     Write-Host "Principal $($_.Principal.Id) has write access"
>> }
```

Finds all principals who have write access to the contact record.

### Example 3: Pipeline usage with multiple records
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $opportunities = Get-DataverseRecord -TableName opportunity -FilterValues @{statecode=0} -Columns opportunityid
PS C:\> $opportunities | ForEach-Object {
>>     Get-DataverseRecordAccess -TableName opportunity -Id $_.opportunityid
>> } | Group-Object {$_.Principal.Id} | Select-Object Count, Name

Count Name
----- ----
    3 87654321-4321-4321-4321-210987654321
    2 22222222-2222-2222-2222-222222222222
```

Gets shared access information for multiple opportunity records and groups by principal.

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

### Microsoft.Crm.Sdk.Messages.PrincipalAccess
## NOTES
See https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.retrievesharedprincipalsandaccessrequest?view=dataverse-sdk-latest

## RELATED LINKS

[Set-DataverseRecordAccess](Set-DataverseRecordAccess.md)

[Test-DataverseRecordAccess](Test-DataverseRecordAccess.md)

[Remove-DataverseRecordAccess](Remove-DataverseRecordAccess.md)

[Microsoft Learn: Security concepts in Dataverse](https://learn.microsoft.com/power-platform/admin/wp-security-cds)
