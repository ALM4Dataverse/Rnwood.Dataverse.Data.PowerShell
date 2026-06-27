---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseEntityMetadata

## SYNOPSIS
Retrieves entity (table) metadata from Dataverse.

## SYNTAX

```
Get-DataverseEntityMetadata [[-EntityName] <String>] [-ExcludeAttributes] [-ExcludeRelationships]
 [-ExcludePrivileges] [-UseMetadataCache] [-Published] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The `Get-DataverseEntityMetadata` cmdlet retrieves metadata information about entities (tables) in Dataverse. You can retrieve metadata for a specific entity or all entities in the organization.

The cmdlet returns comprehensive entity information including:
- Logical name and schema name
- Display names (singular and plural)
- Primary key and primary name attributes
- Ownership type (User, Team, Organization)
- Entity capabilities (activities, notes, audit, change tracking)
- Custom vs system entity status
- By default: attributes, relationships, and privileges (use -Exclude* switches to omit)

Metadata is essential for:
- Understanding entity structure and capabilities
- Building dynamic forms and queries
- Validating entity existence and properties
- Schema documentation and analysis
- Migration and deployment planning

**Performance Tip:** Use `-UseMetadataCache` to enable caching for improved performance when repeatedly accessing metadata.

## EXAMPLES

### Example 1: Get metadata for a specific entity
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $metadata = Get-DataverseEntityMetadata -EntityName contact
PS C:\> $metadata

LogicalName          : contact
SchemaName           : Contact
DisplayName          : Contact
DisplayCollectionName: Contacts
PrimaryIdAttribute   : contactid
PrimaryNameAttribute : fullname
OwnershipType        : UserOwned
IsCustomEntity       : False
```

Retrieves basic metadata for the `contact` entity.

### Example 2: Get metadata with attributes excluded
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $metadata = Get-DataverseEntityMetadata -EntityName account -ExcludeAttributes
PS C:\> $metadata.Attributes
# Attributes will be null or minimal when excluded

PS C:\> # By default, attributes are included
PS C:\> $metadata = Get-DataverseEntityMetadata -EntityName account
PS C:\> $metadata.Attributes.Count
150
```

Gets entity metadata without attributes, then shows the default behavior includes them.

### Example 3: Get metadata with relationships excluded
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $metadata = Get-DataverseEntityMetadata -EntityName account -ExcludeRelationships
PS C:\> $metadata.OneToManyRelationships
# Relationships will be null or minimal when excluded

PS C:\> # By default, relationships are included
PS C:\> $metadata = Get-DataverseEntityMetadata -EntityName account
PS C:\> $metadata.OneToManyRelationships.Count
45
```

Gets entity metadata without relationships, then shows the default behavior includes them.

### Example 4: Get comprehensive metadata (all options by default)
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # By default, all metadata is included
PS C:\> $metadata = Get-DataverseEntityMetadata -EntityName contact

PS C:\> [PSCustomObject]@{
    Entity = $metadata.LogicalName
    Attributes = $metadata.Attributes.Count
    OneToMany = $metadata.OneToManyRelationships.Count
    ManyToOne = $metadata.ManyToOneRelationships.Count
    ManyToMany = $metadata.ManyToManyRelationships.Count
    Privileges = $metadata.Privileges.Count
}

Entity     : contact
Attributes : 120
OneToMany  : 38
ManyToOne  : 15
ManyToMany : 5
Privileges : 8
```

Retrieves complete entity metadata - all details are included by default.

### Example 5: List all entities in the organization
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $allEntities = Get-DataverseEntityMetadata
PS C:\> $allEntities.Count
450

PS C:\> $allEntities | Select-Object -First 10 LogicalName, DisplayName, IsCustomEntity

LogicalName          DisplayName          IsCustomEntity
-----------          -----------          --------------
account              Account              False
contact              Contact              False
lead                 Lead                 False
opportunity          Opportunity          False
new_project          Project              True
new_task             Task                 True
```

Retrieves basic metadata for all entities in the organization.

### Example 6: Filter to custom entities only
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $customEntities = Get-DataverseEntityMetadata | Where-Object { $_.IsCustomEntity -eq $true }
PS C:\> $customEntities | Select-Object LogicalName, DisplayName, SchemaName

LogicalName     DisplayName     SchemaName
-----------     -----------     ----------
new_project     Project         new_Project
new_task        Task            new_Task
new_resource    Resource        new_Resource
```

Retrieves only custom (user-created) entities.

### Example 7: Find entities with audit enabled
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $auditEntities = Get-DataverseEntityMetadata | 
    Where-Object { $_.IsAuditEnabled.Value -eq $true }

PS C:\> $auditEntities | Select-Object LogicalName, DisplayName

LogicalName     DisplayName
-----------     -----------
account         Account
contact         Contact
opportunity     Opportunity
```

Finds all entities with auditing enabled.

### Example 8: Find entities that support activities
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $activityEntities = Get-DataverseEntityMetadata | 
    Where-Object { $_.IsActivityParty.Value -eq $true }

PS C:\> $activityEntities.Count
25
```

Finds all entities that can be associated with activities.

### Example 9: Export entity list to CSV
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseEntityMetadata | 
    Select-Object LogicalName, DisplayName, IsCustomEntity, OwnershipType, IsAuditEnabled | 
    Export-Csv -Path "entities.csv" -NoTypeInformation
```

Exports a list of all entities with key properties to CSV for documentation.

### Example 10: Use metadata cache for performance
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # First call - fetches from server
PS C:\> Measure-Command { $metadata1 = Get-DataverseEntityMetadata -EntityName contact -UseMetadataCache }

Milliseconds : 450

PS C:\> # Second call - uses cache
PS C:\> Measure-Command { $metadata2 = Get-DataverseEntityMetadata -EntityName contact -UseMetadataCache }

Milliseconds : 2
```

Demonstrates the performance improvement when using the metadata cache.

### Example 11: Compare entity metadata between environments
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $dev = Get-DataverseConnection -Url "https://dev.crm.dynamics.com" -Interactive
PS C:\> $prod = Get-DataverseConnection -Url "https://prod.crm.dynamics.com" -Interactive

PS C:\> $devEntities = Get-DataverseEntityMetadata | Select-Object -ExpandProperty LogicalName
PS C:\> $prodEntities = Get-DataverseEntityMetadata | Select-Object -ExpandProperty LogicalName

PS C:\> $onlyInDev = $devEntities | Where-Object { $_ -notin $prodEntities }
PS C:\> $onlyInProd = $prodEntities | Where-Object { $_ -notin $devEntities }

PS C:\> Write-Host "Entities only in Dev: $($onlyInDev -join ', ')"
PS C:\> Write-Host "Entities only in Prod: $($onlyInProd -join ', ')"
```

Compares entities between development and production environments.

### Example 12: Find entities by display name pattern
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseEntityMetadata | 
    Where-Object { $_.DisplayName.UserLocalizedLabel.Label -like "*Project*" } |
    Select-Object LogicalName, DisplayName

LogicalName     DisplayName
-----------     -----------
new_project     Project
new_projecttask Project Task
```

Finds entities with "Project" in their display name.

### Example 13: Pipeline entity name to get metadata
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> @("account", "contact", "lead") | Get-DataverseEntityMetadata | 
    Select-Object LogicalName, DisplayName, PrimaryNameAttribute

LogicalName DisplayName PrimaryNameAttribute
----------- ----------- --------------------
account     Account     name
contact     Contact     fullname
lead        Lead        fullname
```

Pipelines multiple entity names to retrieve their metadata.

### Example 14: Get entities with change tracking enabled
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseEntityMetadata | 
    Where-Object { $_.ChangeTrackingEnabled -eq $true } |
    Select-Object LogicalName, DisplayName

LogicalName     DisplayName
-----------     -----------
account         Account
contact         Contact
```

Finds entities with change tracking enabled for data synchronization.

### Example 15: Access icon properties from entity metadata
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $metadata = Get-DataverseEntityMetadata -EntityName account
PS C:\> $metadata | Select-Object LogicalName, IconVectorName, IconLargeName, IconMediumName, IconSmallName

LogicalName IconVectorName    IconLargeName      IconMediumName     IconSmallName
----------- --------------    -------------      --------------     -------------
account     svg_account       Entity/account.png Entity/account.png Entity/account.png
```

Retrieves and displays icon properties for an entity. Icon properties specify the visual representation of the entity in the UI.

### Example 16: Work with default connection
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Set a default connection
PS C:\> $conn = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive
PS C:\> Set-DataverseConnectionAsDefault

PS C:\> # Now get metadata without specifying connection
PS C:\> $metadata = Get-DataverseEntityMetadata -EntityName account
PS C:\> $metadata.LogicalName
account
```

Demonstrates using the default connection for simplified commands.

### Example 17: Query only published metadata
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Get only published metadata (excludes unpublished changes)
PS C:\> $publishedMetadata = Get-DataverseEntityMetadata -EntityName account -Published
PS C:\> $publishedMetadata.LogicalName
account

PS C:\> # Default behavior retrieves unpublished metadata (includes draft changes)
PS C:\> $unpublishedMetadata = Get-DataverseEntityMetadata -EntityName account
```

Demonstrates the difference between querying published vs unpublished (draft) metadata. By default, the cmdlet retrieves unpublished metadata which includes all changes. Use the -Published switch to retrieve only published metadata.

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

### -EntityName
Logical name of the entity to retrieve metadata for.
If not specified, returns all entities.

```yaml
Type: String
Parameter Sets: (All)
Aliases: TableName

Required: False
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -ExcludeAttributes
Exclude attribute (column) metadata from the output. By default, attributes are included.

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

### -ExcludePrivileges
Exclude privilege metadata from the output. By default, privileges are included.

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

### -ExcludeRelationships
Exclude relationship metadata from the output. By default, relationships are included.

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

### System.String
## OUTPUTS

### Microsoft.Xrm.Sdk.Metadata.EntityMetadata
## NOTES

This cmdlet provides programmatic access to Dataverse metadata. For comprehensive documentation and examples, see the metadata concept guide at docs/core-concepts/metadata.md

## RELATED LINKS
