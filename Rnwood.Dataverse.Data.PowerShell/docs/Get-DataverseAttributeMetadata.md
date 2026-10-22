---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseAttributeMetadata

## SYNOPSIS
Retrieves attribute (column) metadata from Dataverse.

## SYNTAX

```
Get-DataverseAttributeMetadata [-EntityName] <String> [[-AttributeName] <String>] [-UseMetadataCache]
 [-Published] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The `Get-DataverseAttributeMetadata` cmdlet retrieves metadata information about attributes (columns) in Dataverse entities. You can retrieve metadata for a specific attribute or all attributes for an entity.

The cmdlet returns comprehensive attribute information including:
- Logical name and schema name
- Display name and description
- Attribute type (String, Integer, Boolean, Picklist, etc.)
- Type-specific properties (MaxLength, MinValue, MaxValue, Precision, etc.)
- Required level (None, Recommended, ApplicationRequired, SystemRequired)
- Validation settings (IsSearchable, IsAuditEnabled, IsSecured)
- For choice fields: Option set name and options
- For lookup fields: Target entity information

Attribute metadata is essential for:
- Understanding field types and constraints
- Building dynamic forms and validation
- Data type conversion and formatting
- Schema documentation and analysis
- Migration and deployment planning

**Performance Tip:** Use `-UseMetadataCache` to enable caching for improved performance when repeatedly accessing metadata.

## EXAMPLES

### Example 1: Get metadata for a specific attribute
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $attr = Get-DataverseAttributeMetadata -EntityName contact -AttributeName firstname
PS C:\> $attr

LogicalName      : firstname
SchemaName       : FirstName
DisplayName      : First Name
AttributeType    : String
MaxLength        : 50
RequiredLevel    : None
IsSearchable     : True
IsAuditEnabled   : True
```

Retrieves metadata for the `firstname` attribute on the `contact` entity.

### Example 2: Get all attributes for an entity
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $attributes = Get-DataverseAttributeMetadata -EntityName account
PS C:\> $attributes.Count
150

PS C:\> $attributes | Select-Object -First 10 LogicalName, AttributeType

LogicalName       AttributeType
-----------       -------------
accountid         Uniqueidentifier
accountname       String
accountnumber     String
address1_city     String
emailaddress1     String
revenue           Money
numberofemployees Integer
createdon         DateTime
```

Retrieves all attributes for the `account` entity.

### Example 3: Filter attributes by type
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseAttributeMetadata -EntityName contact | 
    Where-Object { $_.AttributeType -eq 'String' } |
    Select-Object LogicalName, MaxLength

LogicalName       MaxLength
-----------       ---------
firstname         50
lastname          50
emailaddress1     100
jobtitle          100
department        100
```

Finds all string (text) attributes on the `contact` entity.

### Example 4: Find required attributes
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseAttributeMetadata -EntityName account | 
    Where-Object { $_.RequiredLevel.Value -in @('ApplicationRequired', 'SystemRequired') } |
    Select-Object LogicalName, DisplayName, RequiredLevel

LogicalName     DisplayName     RequiredLevel
-----------     -----------     -------------
accountname     Account Name    ApplicationRequired
```

Finds all required attributes on the `account` entity.

### Example 5: Get choice field options
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $attr = Get-DataverseAttributeMetadata -EntityName contact -AttributeName gendercode
PS C:\> $attr.OptionSet.Options | Select-Object Value, Label

Value Label
----- -----
1     Male
2     Female
```

Retrieves the options for a choice (picklist) field.

### Example 6: Find searchable attributes
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseAttributeMetadata -EntityName contact | 
    Where-Object { $_.IsValidForAdvancedFind.Value -eq $true } |
    Select-Object LogicalName, DisplayName

LogicalName     DisplayName
-----------     -----------
firstname       First Name
lastname        Last Name
emailaddress1   Email
jobtitle        Job Title
```

Finds all attributes that can be used in Advanced Find.

### Example 7: Get attributes with max length
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseAttributeMetadata -EntityName contact | 
    Where-Object { $_.MaxLength -ne $null } |
    Select-Object LogicalName, AttributeType, MaxLength | 
    Sort-Object MaxLength -Descending

LogicalName       AttributeType MaxLength
-----------       ------------- ---------
description       Memo          2000
address1_composite String       1000
fullname          String       160
emailaddress1     String       100
```

Finds string and memo attributes with their maximum lengths.

### Example 8: Find numeric attributes with constraints
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseAttributeMetadata -EntityName account | 
    Where-Object { $_.AttributeType -in @('Integer', 'Decimal', 'Money') } |
    Select-Object LogicalName, AttributeType, MinValue, MaxValue, Precision

LogicalName         AttributeType MinValue MaxValue Precision
-----------         ------------- -------- -------- ---------
numberofemployees   Integer       0        1000000000
revenue             Money         0        100000000000 2
```

Finds numeric attributes with their constraints.

### Example 9: Export attribute list to CSV
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseAttributeMetadata -EntityName account | 
    Select-Object LogicalName, DisplayName, AttributeType, RequiredLevel, MaxLength | 
    Export-Csv -Path "account_attributes.csv" -NoTypeInformation
```

Exports attribute metadata to CSV for documentation.

### Example 10: Use metadata cache for performance
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # First call - fetches from server
PS C:\> Measure-Command { 
    $attrs1 = Get-DataverseAttributeMetadata -EntityName contact -UseMetadataCache 
}

Milliseconds : 380

PS C:\> # Second call - uses cache
PS C:\> Measure-Command { 
    $attrs2 = Get-DataverseAttributeMetadata -EntityName contact -UseMetadataCache 
}

Milliseconds : 1
```

Demonstrates the performance improvement when using the metadata cache.

### Example 11: Find date/time attributes
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseAttributeMetadata -EntityName contact | 
    Where-Object { $_.AttributeType -eq 'DateTime' } |
    Select-Object LogicalName, DisplayName, DateTimeBehavior, Format

LogicalName     DisplayName     DateTimeBehavior Format
-----------     -----------     ---------------- ------
createdon       Created On      UserLocal        DateAndTime
modifiedon      Modified On     UserLocal        DateAndTime
birthdate       Birthday        DateOnly         DateOnly
```

Finds all date/time attributes with their format settings.

### Example 12: Find lookup attributes
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseAttributeMetadata -EntityName contact | 
    Where-Object { $_.AttributeType -eq 'Lookup' } |
    Select-Object LogicalName, DisplayName, Targets

LogicalName     DisplayName     Targets
-----------     -----------     -------
parentcustomerid Parent Customer {account, contact}
ownerid         Owner           {systemuser, team}
```

Finds all lookup attributes and their target entities.

### Example 13: Find custom attributes only
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseAttributeMetadata -EntityName account | 
    Where-Object { $_.IsCustomAttribute -eq $true } |
    Select-Object LogicalName, DisplayName, AttributeType

LogicalName         DisplayName         AttributeType
-----------         -----------         -------------
new_customfield     Custom Field        String
new_priority        Priority            Picklist
new_projectid       Project             Lookup
```

Finds only custom (user-created) attributes.

### Example 14: Find attributes with field-level security
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseAttributeMetadata -EntityName contact | 
    Where-Object { $_.IsSecured -eq $true } |
    Select-Object LogicalName, DisplayName

LogicalName     DisplayName
-----------     -----------
socialprofile   Social Profile
```

Finds attributes with field-level security enabled.

### Example 15: Pipeline attribute name
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> @("firstname", "lastname", "emailaddress1") | ForEach-Object {
    Get-DataverseAttributeMetadata -EntityName contact -AttributeName $_
} | Select-Object LogicalName, AttributeType, MaxLength

LogicalName     AttributeType MaxLength
-----------     ------------- ---------
firstname       String        50
lastname        String        50
emailaddress1   String        100
```

Retrieves metadata for multiple attributes using pipeline.

### Example 16: Compare attribute metadata between entities
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $accountAttrs = Get-DataverseAttributeMetadata -EntityName account
PS C:\> $contactAttrs = Get-DataverseAttributeMetadata -EntityName contact

PS C:\> $accountNames = $accountAttrs.LogicalName
PS C:\> $contactNames = $contactAttrs.LogicalName

PS C:\> $commonAttrs = $accountNames | Where-Object { $_ -in $contactNames }
PS C:\> Write-Host "Common attributes: $($commonAttrs.Count)"
Common attributes: 45
```

Compares attributes between two entities.

### Example 17: Generate field documentation
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $attrs = Get-DataverseAttributeMetadata -EntityName account
PS C:\> foreach ($attr in $attrs) {
    [PSCustomObject]@{
        Field = $attr.LogicalName
        Label = $attr.DisplayName.UserLocalizedLabel.Label
        Type = $attr.AttributeType
        Required = $attr.RequiredLevel.Value
        Description = $attr.Description.UserLocalizedLabel.Label
    }
} | Export-Csv -Path "account_fields_doc.csv" -NoTypeInformation
```

Generates comprehensive field documentation for an entity.

### Example 18: Query only published metadata
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Get only published attribute metadata
PS C:\> $publishedAttrs = Get-DataverseAttributeMetadata -EntityName account -Published
PS C:\> $publishedAttrs.Count
145

PS C:\> # Default behavior includes unpublished (draft) attributes
PS C:\> $unpublishedAttrs = Get-DataverseAttributeMetadata -EntityName account
PS C:\> $unpublishedAttrs.Count
150
```

Demonstrates retrieving only published metadata vs unpublished (draft) metadata. By default, the cmdlet retrieves unpublished metadata which may include attributes that have been created or modified but not yet published.

## PARAMETERS

### -AttributeName
Logical name of the attribute (column).
If not specified, returns all attributes for the entity.

```yaml
Type: String
Parameter Sets: (All)
Aliases: ColumnName

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

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

### -EntityName
Logical name of the entity (table)

```yaml
Type: String
Parameter Sets: (All)
Aliases: TableName

Required: True
Position: 0
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

### -Published
Retrieve only published metadata. By default (when this switch is not specified), unpublished (draft) metadata is retrieved which includes all changes that have not yet been published.

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

### -UseMetadataCache
Use the shared global metadata cache for improved performance

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### Microsoft.Xrm.Sdk.Metadata.AttributeMetadata
## NOTES

This cmdlet provides programmatic access to Dataverse metadata. For comprehensive documentation and examples, see the metadata concept guide at docs/core-concepts/metadata.md

## RELATED LINKS
