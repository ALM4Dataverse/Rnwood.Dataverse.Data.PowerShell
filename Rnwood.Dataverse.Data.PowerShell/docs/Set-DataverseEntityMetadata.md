---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseEntityMetadata

## SYNOPSIS
Creates or updates an entity (table) in Dataverse.

## SYNTAX

### ByProperties (Default)
```
Set-DataverseEntityMetadata [-EntityName] <String> [-SchemaName <String>] [-DisplayName <String>]
 [-DisplayCollectionName <String>] [-Description <String>] [-OwnershipType <String>] [-IsActivity]
 [-HasActivities] [-HasNotes] [-IsAuditEnabled] [-ChangeTrackingEnabled] [-IconVectorName <String>]
 [-IconLargeName <String>] [-IconMediumName <String>] [-IconSmallName <String>]
 [-PrimaryAttributeSchemaName <String>] [-PrimaryAttributeDisplayName <String>]
 [-PrimaryAttributeMaxLength <Int32>] [-PassThru] [-Publish] [-SkipIconValidation]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### ByEntityMetadata
```
Set-DataverseEntityMetadata [-EntityMetadata] <EntityMetadata> [-PassThru] [-Publish] [-SkipIconValidation]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The `Set-DataverseEntityMetadata` cmdlet creates new entities (tables) or updates existing ones in Dataverse. It allows you to configure entity properties such as display names, ownership type, features like activities and notes support, and icon properties.

The cmdlet supports two parameter sets:
- **ByProperties**: Specify individual properties to create or update an entity
- **ByEntityMetadata**: Pass a complete EntityMetadata object (retrieved from Get-DataverseEntityMetadata) to update an entity

Icon properties (IconVectorName, IconLargeName, IconMediumName, IconSmallName) control the visual representation of the entity in the Dataverse UI. By default, the cmdlet validates that icon properties reference valid webresources:
- **IconVectorName** must reference an SVG webresource (type 11)
- **IconLargeName, IconMediumName, IconSmallName** must reference PNG (type 5), JPG (type 6), or GIF (type 7) webresources
- The webresource must exist (including unpublished webresources)
- Use `-SkipIconValidation` to bypass these checks

When creating a new entity, the `-SchemaName` and `-PrimaryAttributeSchemaName` parameters are required. The cmdlet automatically creates a primary name attribute for the entity.

When updating an existing entity, only the properties you specify will be changed. Some entity properties are immutable after creation (e.g., OwnershipType, SchemaName).

**Important Notes:**
- After creating or updating entities, you must publish customizations for changes to be visible
- Creating entities requires System Administrator or System Customizer security role
- The metadata cache is automatically invalidated after changes
- Some properties cannot be changed after entity creation
- Icon validation is performed by default but can be skipped with `-SkipIconValidation`

## EXAMPLES

### Example 1: Create a simple custom entity
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseEntityMetadata -EntityName new_product `
    -SchemaName new_Product `
    -DisplayName "Product" `
    -DisplayCollectionName "Products" `
    -Description "Custom product catalog" `
    -OwnershipType UserOwned `
    -PrimaryAttributeSchemaName new_name `
    -PrimaryAttributeDisplayName "Product Name"
```

Creates a new user-owned entity named `new_product` with a primary name attribute.

### Example 2: Create entity with activities and notes support
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseEntityMetadata -EntityName new_project `
    -SchemaName new_Project `
    -DisplayName "Project" `
    -DisplayCollectionName "Projects" `
    -OwnershipType UserOwned `
    -HasActivities `
    -HasNotes `
    -PrimaryAttributeSchemaName new_name `
    -PrimaryAttributeDisplayName "Project Name"
```

Creates an entity that supports activities (tasks, appointments) and notes (annotations).

### Example 3: Create organization-owned entity
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseEntityMetadata -EntityName new_setting `
    -SchemaName new_Setting `
    -DisplayName "Application Setting" `
    -DisplayCollectionName "Application Settings" `
    -OwnershipType OrganizationOwned `
    -PrimaryAttributeSchemaName new_name `
    -PrimaryAttributeDisplayName "Setting Name"
```

Creates an organization-owned entity (not owned by specific users or teams).

### Example 4: Create entity with audit and change tracking enabled
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseEntityMetadata -EntityName new_transaction `
    -SchemaName new_Transaction `
    -DisplayName "Transaction" `
    -DisplayCollectionName "Transactions" `
    -OwnershipType UserOwned `
    -IsAuditEnabled `
    -ChangeTrackingEnabled `
    -PrimaryAttributeSchemaName new_name `
    -PrimaryAttributeDisplayName "Transaction ID"
```

Creates an entity with auditing and change tracking enabled for compliance requirements.

### Example 5: Create entity with custom primary name length
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseEntityMetadata -EntityName new_code `
    -SchemaName new_Code `
    -DisplayName "Code" `
    -DisplayCollectionName "Codes" `
    -OwnershipType UserOwned `
    -PrimaryAttributeSchemaName new_code `
    -PrimaryAttributeDisplayName "Code" `
    -PrimaryAttributeMaxLength 50
```

Creates an entity with a primary name attribute limited to 50 characters.

### Example 6: Update entity display names
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseEntityMetadata -EntityName new_product `
    -DisplayName "Product Catalog" `
    -DisplayCollectionName "Product Catalogs" `
    -Description "Updated product catalog description"
```

Updates the display names and description of an existing entity.

### Example 7: Enable audit on existing entity
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseEntityMetadata -EntityName account `
    -IsAuditEnabled
```

Enables auditing for the standard `account` entity.

### Example 8: Enable change tracking on existing entity
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseEntityMetadata -EntityName contact `
    -ChangeTrackingEnabled
```

Enables change tracking for the `contact` entity to support data synchronization scenarios.

### Example 9: Enable activities and notes on existing custom entity
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseEntityMetadata -EntityName new_project `
    -HasActivities `
    -HasNotes
```

Enables both activities and notes support on an existing custom entity named `new_project`.

### Example 10: Create entity and return metadata
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $result = Set-DataverseEntityMetadata -EntityName new_item `
    -SchemaName new_Item `
    -DisplayName "Item" `
    -DisplayCollectionName "Items" `
    -OwnershipType UserOwned `
    -PrimaryAttributeSchemaName new_name `
    -PrimaryAttributeDisplayName "Item Name" `
    -PassThru

PS C:\> $result

LogicalName      : new_item
SchemaName       : new_Item
DisplayName      : Item
EntitySetName    : new_items
MetadataId       : a1234567-89ab-cdef-0123-456789abcdef
OwnershipType    : UserOwned
IsCustomEntity   : True
IsAuditEnabled   : False
```

Creates an entity and returns its metadata using `-PassThru`.

### Example 11: Use -WhatIf to preview changes
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseEntityMetadata -EntityName new_test `
    -SchemaName new_Test `
    -DisplayName "Test Entity" `
    -DisplayCollectionName "Test Entities" `
    -OwnershipType UserOwned `
    -PrimaryAttributeSchemaName new_name `
    -PrimaryAttributeDisplayName "Name" `
    -WhatIf

What if: Performing the operation "Create entity 'new_Test'" on target "Dataverse organization".
```

Uses `-WhatIf` to see what would happen without actually creating the entity.

### Example 12: Create entity with activities, notes, and audit
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseEntityMetadata -EntityName new_case `
    -SchemaName new_Case `
    -DisplayName "Support Case" `
    -DisplayCollectionName "Support Cases" `
    -OwnershipType UserOwned `
    -HasActivities `
    -HasNotes `
    -IsAuditEnabled `
    -ChangeTrackingEnabled `
    -PrimaryAttributeSchemaName new_casenumber `
    -PrimaryAttributeDisplayName "Case Number" `
    -PrimaryAttributeMaxLength 20
```

Creates a full-featured entity with all available options enabled.

### Example 13: Update with Force parameter
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseEntityMetadata -EntityName new_product `
    -DisplayName "Updated Product" `
    -Force
```

Updates an entity without confirmation prompt (Force parameter bypasses confirmation).

### Example 14: Create team-owned entity
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseEntityMetadata -EntityName new_resource `
    -SchemaName new_Resource `
    -DisplayName "Shared Resource" `
    -DisplayCollectionName "Shared Resources" `
    -OwnershipType TeamOwned `
    -PrimaryAttributeSchemaName new_name `
    -PrimaryAttributeDisplayName "Resource Name"
```

Creates a team-owned entity where records are owned by teams rather than individual users.

### Example 15: Batch create multiple entities
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $entities = @(
    @{ SchemaName = "new_Category"; DisplayName = "Category"; PrimaryAttribute = "new_name" }
    @{ SchemaName = "new_Tag"; DisplayName = "Tag"; PrimaryAttribute = "new_name" }
    @{ SchemaName = "new_Comment"; DisplayName = "Comment"; PrimaryAttribute = "new_text" }
)

PS C:\> foreach ($entity in $entities) {
    $name = $entity.SchemaName.ToLower()
    Set-DataverseEntityMetadata -EntityName $name `
        -SchemaName $entity.SchemaName `
        -DisplayName $entity.DisplayName `
        -DisplayCollectionName "$($entity.DisplayName)s" `
        -OwnershipType UserOwned `
        -PrimaryAttributeSchemaName $entity.PrimaryAttribute `
        -PrimaryAttributeDisplayName "Name"
}
```

Creates multiple entities by iterating through a collection.

### Example 16: Create with specific connection
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $conn = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive

PS C:\> Set-DataverseEntityMetadata `
    -EntityName new_inventory `
    -SchemaName new_Inventory `
    -DisplayName "Inventory Item" `
    -DisplayCollectionName "Inventory Items" `
    -OwnershipType UserOwned `
    -PrimaryAttributeSchemaName new_name `
    -PrimaryAttributeDisplayName "Item Name"
```

Creates an entity using a specific connection instead of the default connection.

### Example 17: Create entity with icon properties
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseEntityMetadata -EntityName new_product `
    -SchemaName new_Product `
    -DisplayName "Product" `
    -DisplayCollectionName "Products" `
    -OwnershipType UserOwned `
    -PrimaryAttributeSchemaName new_name `
    -PrimaryAttributeDisplayName "Product Name" `
    -IconVectorName "svg_product" `
    -IconLargeName "Entity/product_large.png" `
    -IconMediumName "Entity/product_medium.png" `
    -IconSmallName "Entity/product_small.png"
```

Creates an entity with custom icon properties. Icons control the visual appearance of the entity in the Dataverse UI.

### Example 18: Update icon properties on existing entity
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseEntityMetadata -EntityName account `
    -IconVectorName "svg_custom_account" `
    -IconLargeName "Custom/account_large.png"
```

Updates only the icon properties of an existing entity without changing other properties.

### Example 19: Update entity using EntityMetadata object
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Get existing metadata
PS C:\> $metadata = Get-DataverseEntityMetadata -EntityName account

PS C:\> # Modify properties
PS C:\> $metadata.IconVectorName = "svg_updated_account"
PS C:\> $metadata.ChangeTrackingEnabled = $true

PS C:\> # Update the entity with modified metadata
PS C:\> Set-DataverseEntityMetadata -EntityMetadata $metadata
```

Retrieves an EntityMetadata object, modifies properties, and updates the entity using the modified object. This is useful for complex updates or bulk modifications.

### Example 20: Pipeline EntityMetadata for batch updates
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Update icons for multiple custom entities
PS C:\> Get-DataverseEntityMetadata | 
    Where-Object { $_.IsCustomEntity -eq $true } |
    ForEach-Object {
        $_.IconVectorName = "svg_custom_icon"
        $_
    } |
    Set-DataverseEntityMetadata -Publish
```

Retrieves all custom entities, updates their icon properties, and publishes the changes using the pipeline.

### Example 21: Copy entity metadata properties to another entity
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Get source entity metadata
PS C:\> $sourceMetadata = Get-DataverseEntityMetadata -EntityName account

PS C:\> # Copy icon properties to target entity
PS C:\> Set-DataverseEntityMetadata -EntityName new_customer `
    -IconVectorName $sourceMetadata.IconVectorName `
    -IconLargeName $sourceMetadata.IconLargeName `
    -IconMediumName $sourceMetadata.IconMediumName `
    -IconSmallName $sourceMetadata.IconSmallName
```

Copies icon properties from one entity to another entity.

### Example 22: Create entity with validated icon webresources
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Create an entity with all icon properties that reference valid webresources
PS C:\> Set-DataverseEntityMetadata -EntityName new_product `
    -SchemaName new_Product `
    -DisplayName "Product" `
    -DisplayCollectionName "Products" `
    -OwnershipType UserOwned `
    -PrimaryAttributeSchemaName new_name `
    -PrimaryAttributeDisplayName "Product Name" `
    -IconVectorName "new_product_svg" `
    -IconLargeName "new_product_large.png" `
    -IconMediumName "new_product_medium.png" `
    -IconSmallName "new_product_small.png"
```

Creates an entity with icon validation enabled (default). The cmdlet validates that:
- "new_product_svg" references an SVG webresource (type 11)
- "new_product_large.png" references a PNG, JPG, or GIF webresource (type 5, 6, or 7)
- "new_product_medium.png" references a PNG, JPG, or GIF webresource (type 5, 6, or 7)
- "new_product_small.png" references a PNG, JPG, or GIF webresource (type 5, 6, or 7)

If any webresource doesn't exist or has the wrong type, an error is thrown.

### Example 23: Skip icon webresource validation
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Create an entity without validating icon webresource existence
PS C:\> Set-DataverseEntityMetadata -EntityName new_product `
    -SchemaName new_Product `
    -DisplayName "Product" `
    -DisplayCollectionName "Products" `
    -OwnershipType UserOwned `
    -PrimaryAttributeSchemaName new_name `
    -PrimaryAttributeDisplayName "Product Name" `
    -IconVectorName "new_product_icon" `
    -SkipIconValidation
```

Creates an entity with icon validation disabled. The cmdlet will not check if "new_product_icon" webresource exists or has the correct type. Use this when you plan to create the webresource later or want to bypass validation for development/testing.

### Example 24: Create an activity entity
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseEntityMetadata -EntityName new_customactivity `
    -SchemaName new_CustomActivity `
    -DisplayName "Custom Activity" `
    -DisplayCollectionName "Custom Activities" `
    -Description "A custom activity entity for tracking interactions" `
    -OwnershipType UserOwned `
    -IsActivity `
    -PrimaryAttributeSchemaName new_subject `
    -PrimaryAttributeDisplayName "Subject" `
    -PrimaryAttributeMaxLength 200
```

Creates a custom activity entity that derives from activitypointer. Activity entities are used to track interactions like appointments, emails, phone calls, and custom activity types. Activity entities automatically support certain features like regarding objects and activity parties.

**Important Notes:**
- Activity entities derive from the activitypointer entity and inherit standard activity fields
- The `-IsActivity` switch can only be set during entity creation and cannot be changed afterwards
- Activity entities automatically appear in timeline controls and activity-related views
- The primary attribute is typically used as the "Subject" field for the activity

## PARAMETERS

### -ChangeTrackingEnabled
Whether change tracking is enabled for the entity. Change tracking enables efficient data synchronization by tracking changes at the table level.

When enabled, Dataverse maintains a log of changes (creates, updates, deletes) that external systems can query to stay synchronized.

```yaml
Type: SwitchParameter
Parameter Sets: ByProperties
Aliases:

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

### -Description
Description of the entity that appears as tooltip text in the UI. This helps users understand the purpose of the entity.

```yaml
Type: String
Parameter Sets: ByProperties
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DisplayCollectionName
Display collection name (plural) of the entity used in the UI when showing multiple records.

For example, if DisplayName is "Product", DisplayCollectionName should be "Products".

```yaml
Type: String
Parameter Sets: ByProperties
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DisplayName
Display name of the entity shown in the UI. This is the user-friendly name that appears in forms, views, and navigation.

```yaml
Type: String
Parameter Sets: ByProperties
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -EntityMetadata
EntityMetadata object to update. When provided, all property values from the metadata object are used to update the entity.

This parameter is useful for:
- Making complex updates by modifying a retrieved EntityMetadata object
- Batch updates using pipeline processing
- Copying properties between entities

The EntityMetadata object must have a valid MetadataId and LogicalName.

**Tip**: Retrieve EntityMetadata using `Get-DataverseEntityMetadata`, modify properties, then pass to this cmdlet.

```yaml
Type: EntityMetadata
Parameter Sets: ByEntityMetadata
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -EntityName
Logical name of the entity (table). This is the internal name used to reference the entity programmatically.

For new entities combined with `-SchemaName`, this becomes the logical name after the publisher prefix is added (e.g., `new_product`).

For existing entities, this identifies which entity to update.

```yaml
Type: String
Parameter Sets: ByProperties
Aliases: TableName

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -HasActivities
Whether the entity supports activities (tasks, phone calls, appointments, emails, etc.).

When enabled, users can:
- Associate activities with records from this entity
- See activity timelines on forms
- Create activities related to these records

```yaml
Type: SwitchParameter
Parameter Sets: ByProperties
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -HasNotes
Whether the entity supports notes (annotations). Notes allow users to add text notes and file attachments to records.

When enabled, users can:
- Add notes with rich text
- Attach files to records
- View notes history

```yaml
Type: SwitchParameter
Parameter Sets: ByProperties
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IconLargeName
Large icon name for the entity. Specifies the filename or identifier for the large icon image used in the Dataverse UI.

Icon files are typically stored in web resources. The value should reference the web resource name (e.g., "Entity/account_large.png").

```yaml
Type: String
Parameter Sets: ByProperties
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IconMediumName
Medium icon name for the entity. Specifies the filename or identifier for the medium icon image used in the Dataverse UI.

```yaml
Type: String
Parameter Sets: ByProperties
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IconSmallName
Small icon name for the entity. Specifies the filename or identifier for the small icon image used in the Dataverse UI.

```yaml
Type: String
Parameter Sets: ByProperties
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IconVectorName
Vector icon name (SVG icon identifier) for the entity. Modern Dataverse UI uses vector icons for scalable, resolution-independent display.

The value should reference an SVG web resource (e.g., "new_/icons/user.svg"). 

**Tip**: Use the `Set-DataverseTableIconFromSet` cmdlet to easily download and set icons from online icon sets (FluentUI, Tabler, Iconoir) instead of manually creating web resources. See `Get-DataverseIconSetIcon` to browse available icons.

Vector icons provide the best visual quality across different display resolutions and zoom levels.

```yaml
Type: String
Parameter Sets: ByProperties
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IsActivity
Whether this entity is an activity entity (derives from activitypointer). Activity entities are used to track interactions like appointments, emails, phone calls, and custom activity types.

When enabled, the entity:
- Inherits standard activity fields (regardingobjectid, scheduledstart, scheduledend, etc.)
- Appears in timeline controls on related records
- Can be associated with regarding objects (accounts, contacts, opportunities, etc.)
- Supports activity parties for participants

**Note**: This property can only be set during entity creation and cannot be changed afterwards. Activity entities cannot be converted to standard entities and vice versa.

```yaml
Type: SwitchParameter
Parameter Sets: ByProperties
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IsAuditEnabled
Whether audit is enabled for this entity. When enabled, changes to records are tracked in the audit history.

**Note**: Auditing must also be enabled at the organization level and for specific fields.

```yaml
Type: SwitchParameter
Parameter Sets: ByProperties
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OwnershipType
Ownership type of the entity that determines who can own records.

Valid values:
- **UserOwned**: Records are owned by individual users (default for most custom entities)
- **TeamOwned**: Records are owned by teams
- **OrganizationOwned**: Records are owned by the organization (no specific owner)

**Note**: This property is immutable after entity creation.

```yaml
Type: String
Parameter Sets: ByProperties
Aliases:
Accepted values: UserOwned, TeamOwned, OrganizationOwned

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
Return the created or updated entity metadata as a PSObject.

The returned object includes properties like LogicalName, SchemaName, DisplayName, EntitySetName, MetadataId, and OwnershipType.

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

### -PrimaryAttributeDisplayName
Display name for the primary name attribute. This is the user-friendly label shown in the UI for the name field.

Required when creating a new entity. For example: "Product Name", "Customer Name", "Case Number".

```yaml
Type: String
Parameter Sets: ByProperties
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PrimaryAttributeMaxLength
Maximum length for the primary name attribute in characters. Default is 100 if not specified.

Valid range is 1-4000 characters. Consider your naming conventions when setting this value.

```yaml
Type: Int32
Parameter Sets: ByProperties
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PrimaryAttributeSchemaName
Schema name for the primary name attribute. Required when creating a new entity.

Must follow naming conventions with publisher prefix (e.g., `new_name`, `new_productname`).

**Note**: Cannot be changed after entity creation.

```yaml
Type: String
Parameter Sets: ByProperties
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
If specified, publishes the entity after creating or updating

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

### -SchemaName
Schema name of the entity with publisher prefix (e.g., `new_CustomEntity`). Required when creating a new entity.

This is the unique name that includes your solution publisher's prefix. It becomes the basis for the logical name.

**Note**: This property is immutable after creation.

```yaml
Type: String
Parameter Sets: ByProperties
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SkipIconValidation
If specified, skips validation that icon properties reference valid webresources.

By default, when setting icon properties (IconVectorName, IconLargeName, IconMediumName, IconSmallName), the cmdlet validates that the specified webresource exists and has the correct type:
- IconVectorName must reference an SVG webresource (type 11)
- IconLargeName, IconMediumName, IconSmallName must reference PNG (type 5), JPG (type 6), or GIF (type 7) webresources

Use this switch to:
- Skip validation during development/testing
- Set icon names before creating the corresponding webresources
- Override validation in automated scripts where webresources may not exist yet

**Note**: If validation is skipped and the webresource doesn't exist or has the wrong type, the entity may not display correctly in the Dataverse UI.

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

### Microsoft.Xrm.Sdk.Metadata.EntityMetadata
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES

This cmdlet provides programmatic access to Dataverse metadata. For comprehensive documentation and examples, see the metadata concept guide at docs/core-concepts/metadata.md

## RELATED LINKS

[Get-DataverseEntityMetadata](Get-DataverseEntityMetadata.md)

[Set-DataverseAttributeMetadata](Set-DataverseAttributeMetadata.md)

[Remove-DataverseEntityMetadata](Remove-DataverseEntityMetadata.md)

[Microsoft Learn: Create custom tables](https://learn.microsoft.com/power-apps/maker/data-platform/data-platform-create-entity)
