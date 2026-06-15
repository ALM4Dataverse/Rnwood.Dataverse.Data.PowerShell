---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseAttributeMetadata

## SYNOPSIS
Creates or updates an attribute (column) in Dataverse.

## SYNTAX

```
Set-DataverseAttributeMetadata [-EntityName] <String> [-AttributeName] <String> [-SchemaName <String>]
 [-DisplayName <String>] [-Description <String>] [-AttributeType <String>] [-RequiredLevel <String>]
 [-IsSearchable] [-IsSecured] [-IsAuditEnabled] [-MaxLength <Int32>] [-StringFormat <String>]
 [-MinValue <Object>] [-MaxValue <Object>] [-Precision <Int32>] [-DateTimeFormat <String>]
 [-DateTimeBehavior <String>] [-TrueLabel <String>] [-FalseLabel <String>] [-DefaultValue <Boolean>]
 [-OptionSetName <String>] [-Options <Hashtable[]>] [-Targets <String[]>] [-RelationshipSchemaName <String>]
 [-CascadeAssign <String>] [-CascadeShare <String>] [-CascadeUnshare <String>] [-CascadeReparent <String>]
 [-CascadeDelete <String>] [-CascadeMerge <String>] [-MaxSizeInKB <Int32>] [-PassThru] [-Publish]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The `Set-DataverseAttributeMetadata` cmdlet creates new attributes (columns) or updates existing ones in a Dataverse table. It supports all major attribute types including text, numbers, dates, choices, lookups, files, and images.

When creating a new attribute, the `-AttributeType` and `-SchemaName` parameters are required. When updating an existing attribute, only the properties you specify will be changed.

**Important Notes:**
- Some attribute properties are immutable after creation (e.g., AttributeType, SchemaName, StringFormat, DateTimeFormat, DateTimeBehavior)
- The cmdlet will throw an error if you attempt to change an immutable property
- After making changes, you may need to publish customizations for them to be visible
- The metadata cache for the entity is automatically invalidated after changes

## EXAMPLES

### Example 1: Create a simple text attribute
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAttributeMetadata -EntityName account -AttributeName new_customfield `
    -SchemaName new_CustomField -DisplayName "Custom Field" -AttributeType String `
    -MaxLength 200 -Description "A custom text field"
```

Creates a new single-line text attribute named `new_customfield` on the `account` table with a maximum length of 200 characters.

### Example 2: Create a multiline text attribute
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAttributeMetadata -EntityName contact -AttributeName new_notes `
    -SchemaName new_Notes -DisplayName "Notes" -AttributeType Memo `
    -MaxLength 4000 -RequiredLevel Recommended
```

Creates a multiline text (memo) attribute on the `contact` table with 4000 character limit and recommended requirement level.

### Example 3: Create an integer attribute with constraints
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAttributeMetadata -EntityName product -AttributeName new_quantity `
    -SchemaName new_Quantity -DisplayName "Quantity" -AttributeType Integer `
    -MinValue 0 -MaxValue 10000 -RequiredLevel ApplicationRequired
```

Creates an integer attribute with minimum value of 0 and maximum of 10000, making it required by the application.

### Example 4: Create a decimal attribute with precision
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAttributeMetadata -EntityName invoice -AttributeName new_discount `
    -SchemaName new_Discount -DisplayName "Discount Percentage" -AttributeType Decimal `
    -MinValue 0 -MaxValue 100 -Precision 2
```

Creates a decimal attribute for discount percentages with 2 decimal places of precision.

### Example 5: Create a money attribute
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAttributeMetadata -EntityName opportunity -AttributeName new_bonus `
    -SchemaName new_Bonus -DisplayName "Bonus Amount" -AttributeType Money `
    -MinValue 0 -MaxValue 1000000 -Precision 2
```

Creates a money attribute for bonus amounts with currency formatting.

### Example 6: Create a date-only attribute
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAttributeMetadata -EntityName contact -AttributeName new_hiredate `
    -SchemaName new_HireDate -DisplayName "Hire Date" -AttributeType DateTime `
    -DateTimeFormat DateOnly -DateTimeBehavior UserLocal
```

Creates a date-only attribute (no time component) that displays in the user's local timezone.

### Example 7: Create a date and time attribute with timezone independence
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAttributeMetadata -EntityName appointment -AttributeName new_eventtime `
    -SchemaName new_EventTime -DisplayName "Event Time" -AttributeType DateTime `
    -DateTimeFormat DateAndTime -DateTimeBehavior TimeZoneIndependent
```

Creates a date and time attribute that stores values independent of timezone.

### Example 8: Create a boolean (Yes/No) attribute
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAttributeMetadata -EntityName account -AttributeName new_ispremium `
    -SchemaName new_IsPremium -DisplayName "Is Premium" -AttributeType Boolean `
    -TrueLabel "Premium" -FalseLabel "Standard" -DefaultValue $true
```

Creates a Yes/No attribute with custom labels and a default value of true.

### Example 9: Create a choice (picklist) attribute with local options
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $options = @(
    @{ Value = 1; Label = "Small" }
    @{ Value = 2; Label = "Medium" }
    @{ Value = 3; Label = "Large" }
    @{ Value = 4; Label = "Extra Large" }
)

PS C:\> Set-DataverseAttributeMetadata -EntityName product -AttributeName new_size `
    -SchemaName new_Size -DisplayName "Product Size" -AttributeType Picklist `
    -Options $options
```

Creates a choice attribute with local (entity-specific) options.

### Example 10: Create a choice attribute using a global option set
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAttributeMetadata -EntityName contact -AttributeName new_status `
    -SchemaName new_Status -DisplayName "Customer Status" -AttributeType Picklist `
    -OptionSetName new_customerstatus
```

Creates a choice attribute that uses an existing global option set named `new_customerstatus`.

### Example 11: Create a multi-select choice attribute
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $interests = @(
    @{ Value = 1; Label = "Technology" }
    @{ Value = 2; Label = "Sports" }
    @{ Value = 3; Label = "Music" }
    @{ Value = 4; Label = "Travel" }
)

PS C:\> Set-DataverseAttributeMetadata -EntityName contact -AttributeName new_interests `
    -SchemaName new_Interests -DisplayName "Interests" -AttributeType MultiSelectPicklist `
    -Options $interests
```

Creates a multi-select choice attribute allowing multiple values to be selected.

### Example 12: Create a file attribute
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAttributeMetadata -EntityName account -AttributeName new_contract `
    -SchemaName new_Contract -DisplayName "Contract Document" -AttributeType File `
    -MaxSizeInKB 10240
```

Creates a file attribute that can store documents up to 10MB (10240 KB).

### Example 13: Create an image attribute
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAttributeMetadata -EntityName contact -AttributeName new_photo `
    -SchemaName new_Photo -DisplayName "Profile Photo" -AttributeType Image `
    -MaxSizeInKB 5120
```

Creates an image attribute for storing profile photos up to 5MB.

### Example 14: Update attribute display name and description
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAttributeMetadata -EntityName account -AttributeName new_customfield `
    -DisplayName "Updated Field Name" -Description "Updated description for this field"
```

Updates the display name and description of an existing attribute. No other properties are changed.

### Example 15: Update attribute required level
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAttributeMetadata -EntityName contact -AttributeName emailaddress1 `
    -RequiredLevel ApplicationRequired
```

Changes the required level of the email address field to make it application-required.

### Example 16: Enable audit on an attribute
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAttributeMetadata -EntityName account -AttributeName revenue `
    -IsAuditEnabled
```

Enables auditing for the revenue field to track changes.

### Example 17: Update string attribute maximum length
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAttributeMetadata -EntityName account -AttributeName accountnumber `
    -MaxLength 50
```

Increases the maximum length of the account number field to 50 characters.

### Example 18: Update numeric attribute constraints
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAttributeMetadata -EntityName product -AttributeName new_quantity `
    -MinValue 10 -MaxValue 5000
```

Updates the minimum and maximum values for an existing integer attribute.

### Example 19: Create attribute with -PassThru to see result
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $result = Set-DataverseAttributeMetadata -EntityName account -AttributeName new_score `
    -SchemaName new_Score -DisplayName "Score" -AttributeType Integer `
    -PassThru

PS C:\> $result

LogicalName : new_score
SchemaName  : new_Score
DisplayName : Score
AttributeType : Integer
MetadataId  : a1234567-89ab-cdef-0123-456789abcdef
```

Creates an attribute and returns the metadata of the created attribute using `-PassThru`.

### Example 20: Create email format string attribute
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAttributeMetadata -EntityName contact -AttributeName new_secondaryemail `
    -SchemaName new_SecondaryEmail -DisplayName "Secondary Email" -AttributeType String `
    -MaxLength 100 -StringFormat Email
```

Creates a text attribute with email format validation.

### Example 21: Create URL format string attribute
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAttributeMetadata -EntityName account -AttributeName new_website2 `
    -SchemaName new_Website2 -DisplayName "Secondary Website" -AttributeType String `
    -MaxLength 200 -StringFormat Url
```

Creates a text attribute with URL format, displayed as a clickable link.

### Example 22: Create phone format string attribute
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAttributeMetadata -EntityName contact -AttributeName new_mobilephone2 `
    -SchemaName new_MobilePhone2 -DisplayName "Secondary Mobile" -AttributeType String `
    -MaxLength 20 -StringFormat Phone
```

Creates a text attribute with phone number formatting.

### Example 23: Attempt to update immutable property (will fail)
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAttributeMetadata -EntityName account -AttributeName new_customfield `
    -StringFormat Email

Set-DataverseAttributeMetadata : Cannot change StringFormat from 'Text' to 'Email'. This property is immutable after creation.
```

Demonstrates that attempting to change an immutable property will result in an error.

### Example 24: Create BigInt attribute
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAttributeMetadata -EntityName account -AttributeName new_largeid `
    -SchemaName new_LargeId -DisplayName "Large ID" -AttributeType BigInt `
    -Description "Stores very large integer values"
```

Creates a BigInt attribute for storing large integer values beyond the range of regular integers.

### Example 25: Create Double attribute
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAttributeMetadata -EntityName measurement -AttributeName new_temperature `
    -SchemaName new_Temperature -DisplayName "Temperature" -AttributeType Double `
    -MinValue -273.15 -MaxValue 1000 -Precision 4
```

Creates a double-precision floating point attribute with 4 decimal places.

### Example 26: Make attribute searchable
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAttributeMetadata -EntityName account -AttributeName new_keywords `
    -SchemaName new_Keywords -DisplayName "Keywords" -AttributeType String `
    -MaxLength 500 -IsSearchable
```

Creates a searchable text attribute that can be used in Advanced Find queries.

### Example 27: Batch create multiple attributes
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $attributes = @(
    @{ AttributeName = "new_field1"; SchemaName = "new_Field1"; DisplayName = "Field 1"; AttributeType = "String"; MaxLength = 100 }
    @{ AttributeName = "new_field2"; SchemaName = "new_Field2"; DisplayName = "Field 2"; AttributeType = "Integer"; MinValue = 0; MaxValue = 100 }
    @{ AttributeName = "new_field3"; SchemaName = "new_Field3"; DisplayName = "Field 3"; AttributeType = "Boolean"; TrueLabel = "Yes"; FalseLabel = "No" }
)

PS C:\> foreach ($attr in $attributes) {
    Set-DataverseAttributeMetadata -EntityName account @attr
}
```

Creates multiple attributes by iterating through a collection of attribute definitions.

### Example 28: Create UniqueIdentifier attribute
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAttributeMetadata -EntityName account -AttributeName new_externalid `
    -SchemaName new_ExternalId -DisplayName "External ID" -AttributeType UniqueIdentifier `
    -Description "Unique identifier from external system"
```

Creates a GUID attribute for storing unique identifiers from external systems.

### Example 29: Use -WhatIf to preview changes
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAttributeMetadata -EntityName account -AttributeName new_testfield `
    -SchemaName new_TestField -DisplayName "Test Field" -AttributeType String `
    -MaxLength 100 -WhatIf

What if: Performing the operation "Create attribute 'new_TestField' of type 'String'" on target "Entity 'account'".
```

Uses -WhatIf to see what would happen without actually creating the attribute.

### Example 30: Update with specific connection
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $conn = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive

PS C:\> Set-DataverseAttributeMetadata -EntityName account `
    -AttributeName new_customfield -DisplayName "Updated Name"
```

Updates an attribute using a specific connection instead of the default connection.

### Example 31: Update statuscode options with State property
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $statusOptions = @(
    @{Value=1; Label='Draft'; State=0},
    @{Value=807290000; Label='Approved'; State=0},
    @{Value=807290001; Label='Filled'; State=0},
    @{Value=2; Label='Closed'; State=1},
    @{Value=807290002; Label='Cancelled'; State=1}
)

PS C:\> Set-DataverseAttributeMetadata -EntityName abc_position `
    -AttributeName statuscode -Options $statusOptions
```

Updates the statuscode attribute options for a custom entity. The State property links each status value to a state (0=Active, 1=Inactive). This allows you to define which status values are valid for each state.

## PARAMETERS

### -AttributeName
Logical name of the attribute (column). This is the internal name used to reference the field programmatically.

For existing attributes, this identifies which attribute to update. For new attributes combined with `-SchemaName`, this becomes the logical name after the publisher prefix is added.

```yaml
Type: String
Parameter Sets: (All)
Aliases: ColumnName

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -AttributeType
Type of attribute to create. Required when creating a new attribute, ignored when updating.

Valid values:
- **String**: Single-line text (default max: 100 characters)
- **Memo**: Multi-line text (default max: 2000 characters)
- **Integer**: Whole number (-2,147,483,648 to 2,147,483,647)
- **BigInt**: Large whole number
- **Decimal**: Decimal number with fixed precision
- **Double**: Double-precision floating point number
- **Money**: Currency value
- **DateTime**: Date and/or time value
- **Boolean**: Yes/No (true/false) value
- **Picklist**: Single-select choice list
- **MultiSelectPicklist**: Multi-select choice list
- **Lookup**: Reference to another record (not fully supported - use SDK cmdlets)
- **File**: File attachment
- **Image**: Image attachment
- **UniqueIdentifier**: GUID value

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: String, Memo, Integer, BigInt, Decimal, Double, Money, DateTime, Boolean, Picklist, MultiSelectPicklist, Lookup, File, Image, UniqueIdentifier

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CascadeAssign
Cascade behavior for Assign: NoCascade, Cascade, Active, UserOwned, RemoveLink (Lookup only)

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: NoCascade, Cascade, Active, UserOwned, RemoveLink

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CascadeDelete
Cascade behavior for Delete: NoCascade, RemoveLink, Restrict, Cascade (Lookup only)

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: NoCascade, RemoveLink, Restrict, Cascade

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CascadeMerge
Cascade behavior for Merge: NoCascade, Cascade (Lookup only)

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: NoCascade, Cascade

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CascadeReparent
Cascade behavior for Reparent: NoCascade, Cascade, Active, UserOwned, RemoveLink (Lookup only)

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: NoCascade, Cascade, Active, UserOwned, RemoveLink

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CascadeShare
Cascade behavior for Share: NoCascade, Cascade, Active, UserOwned (Lookup only)

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: NoCascade, Cascade, Active, UserOwned

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CascadeUnshare
Cascade behavior for Unshare: NoCascade, Cascade, Active, UserOwned (Lookup only)

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: NoCascade, Cascade, Active, UserOwned

Required: False
Position: Named
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

### -DateTimeBehavior
Behavior for DateTime attributes that controls how dates are stored and displayed.

Valid values:
- **UserLocal** (default): Dates are stored in UTC and displayed in user's timezone
- **DateOnly**: Only the date is stored, no time component
- **TimeZoneIndependent**: Dates are stored and displayed exactly as entered, ignoring timezone

**Note**: This property is immutable after creation.

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: UserLocal, DateOnly, TimeZoneIndependent

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DateTimeFormat
Format for DateTime attributes that controls whether time component is included.

Valid values:
- **DateOnly**: Only date is shown (no time)
- **DateAndTime**: Both date and time are shown

**Note**: This property is immutable after creation.

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: DateOnly, DateAndTime

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DefaultValue
Default value for Boolean attributes. When a new record is created, this value is automatically set.

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Description
Description of the attribute that appears as tooltip text in the UI. This helps users understand the purpose of the field.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DisplayName
Display name of the attribute shown in the UI. This is the user-friendly name that appears in forms and views.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -EntityName
Logical name of the entity (table) where the attribute will be created or updated.

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

### -FalseLabel
Label for the false option in Boolean attributes. If not specified, defaults to "No".

**Note**: This property is immutable after creation.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IsAuditEnabled
Whether audit is enabled for this attribute. When enabled, changes to this field are tracked in the audit history.

**Note**: Auditing must also be enabled at the organization and entity level.

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

### -IsSearchable
Whether the attribute is searchable in Advanced Find and quick find queries. Making fields searchable allows users to find records based on this field's value.

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

### -IsSecured
Whether the attribute is secured (requires field-level security). When true, access to this field is controlled by field security profiles.

**Note**: This property cannot be changed after creation. Field-level security must be configured separately through security profiles.

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

### -MaxLength
Maximum length for String or Memo attributes, in characters.

For **String** attributes: Default is 100, maximum is 4000.
For **Memo** attributes: Default is 2000, maximum is 1,048,576.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MaxSizeInKB
Maximum size in kilobytes for File or Image attributes.

For **File** attributes: Default is 32,768 KB (32 MB), maximum is 131,072 KB (128 MB).
For **Image** attributes: Default is 10,240 KB (10 MB), maximum is 30,720 KB (30 MB).

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MaxValue
Maximum value for Integer, Decimal, Double, or Money attributes.

The type of value provided should match the attribute type (integer for Integer, decimal for Decimal, etc.).

```yaml
Type: Object
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MinValue
Minimum value for Integer, Decimal, Double, or Money attributes.

The type of value provided should match the attribute type (integer for Integer, decimal for Decimal, etc.).

```yaml
Type: Object
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Options
Array of hashtables defining options for Picklist or MultiSelectPicklist attributes with local option sets.

Each hashtable should have:
- `Value`: Integer value for the option (optional - will be auto-assigned if not provided)
- `Label`: String label displayed to users
- `State`: Integer state value (required for statuscode attributes only)

Example: `@(@{Value=1; Label='Small'}, @{Value=2; Label='Large'})`

For statuscode attributes, the State property is required to link each status to a state:
`@(@{Value=1; Label='Draft'; State=0}, @{Value=2; Label='Approved'; State=0}, @{Value=3; Label='Closed'; State=1})`

**Note**: This property is immutable after creation for regular picklist attributes. To update option values for regular picklists, use `Set-DataverseOptionSetMetadata`. For statuscode attributes, Options can be updated to define valid status values with their associated states.

```yaml
Type: Hashtable[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OptionSetName
Name of an existing global option set to use for Picklist or MultiSelectPicklist attributes.

If specified, the attribute will use the global option set. If not specified, you must provide `-Options` to create a local option set.

**Note**: This property is immutable after creation. To update option values, use `Set-DataverseOptionSetMetadata`.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
Return the created or updated attribute metadata as a PSObject.

The returned object includes properties like LogicalName, SchemaName, DisplayName, AttributeType, and MetadataId.

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

### -Precision
Precision for Decimal, Double, or Money attributes.

Specifies the number of decimal places to display. Valid range is 0-10.

For **Money** attributes: Precision is typically 2 for currency values.
For **Decimal/Double** attributes: Choose precision based on your data requirements.

```yaml
Type: Int32
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

### -Publish
If specified, publishes the attribute after creating or updating

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

### -RelationshipSchemaName
Schema name for the relationship created with the Lookup attribute (e.g., 'new_project_contact'). If not specified, generates from entity names.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RequiredLevel
Required level for the attribute that controls whether the field is required.

Valid values:
- **None**: Field is optional
- **SystemRequired**: Field is required by the system (cannot be null)
- **ApplicationRequired**: Field is required by application logic
- **Recommended**: Field is recommended but not required

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: None, SystemRequired, ApplicationRequired, Recommended

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SchemaName
Schema name of the attribute, used for creating new attributes. This is the unique name with publisher prefix (e.g., 'new_CustomField').

Required when creating a new attribute. Ignored when updating an existing attribute.

**Note**: This property is immutable after creation.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -StringFormat
Format for String attributes that controls validation and display.

Valid values:
- **Text** (default): Plain text
- **TextArea**: Multi-line text area display
- **Email**: Email address with validation
- **Url**: URL with clickable link
- **TickerSymbol**: Stock ticker symbol
- **Phone**: Phone number
- **PhoneticGuide**: Phonetic guide for pronunciation
- **VersionNumber**: Version number
- **Json**: JSON data

**Note**: This property is immutable after creation.

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: Text, TextArea, Email, Url, TickerSymbol, Phone, PhoneticGuide, VersionNumber, Json

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Targets
Array of target entity logical names for Lookup attributes. Specifies which entity types this lookup can reference.

**Note**: Creating Lookup attributes requires additional relationship setup and is not fully supported by this cmdlet. Use `Invoke-DataverseCreateAttribute` with `CreateOneToManyRequest` for full control.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TrueLabel
Label for the true option in Boolean attributes. If not specified, defaults to "Yes".

**Note**: This property is immutable after creation.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

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

### System.Management.Automation.PSObject
## NOTES

This cmdlet provides programmatic access to Dataverse metadata. For comprehensive documentation and examples, see the metadata concept guide at docs/core-concepts/metadata.md

## RELATED LINKS

[Get-DataverseEntityMetadata](Get-DataverseEntityMetadata.md)

[Set-DataverseOptionSetMetadata](Set-DataverseOptionSetMetadata.md)

[Set-DataverseRelationshipMetadata](Set-DataverseRelationshipMetadata.md)

[Microsoft Learn: Dataverse Column Types](https://learn.microsoft.com/power-apps/maker/data-platform/types-of-fields)
