---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseRelationshipMetadata

## SYNOPSIS
Retrieves relationship metadata from Dataverse.

## SYNTAX

```
Get-DataverseRelationshipMetadata [[-EntityName] <String>] [[-RelationshipName] <String>]
 [-RelationshipType <String>] [-Published] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
The `Get-DataverseRelationshipMetadata` cmdlet retrieves metadata information about relationships in Dataverse. This includes OneToMany (1:N), ManyToOne (N:1), and ManyToMany (N:N) relationships.

You can retrieve:
- All relationships in the organization
- Relationships for a specific entity
- A specific relationship by schema name
- Relationships filtered by type (OneToMany, ManyToOne, ManyToMany)

The cmdlet returns relationship metadata objects that contain comprehensive information about relationship structure, cascade behaviors, and configuration.

## EXAMPLES

### Example 1: Get all relationships for an entity
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $relationships = Get-DataverseRelationshipMetadata -EntityName account
PS C:\> $relationships.Count
45

PS C:\> $relationships | Select-Object -First 5 SchemaName, RelationshipType, ReferencedEntity, ReferencingEntity

SchemaName                    RelationshipType ReferencedEntity ReferencingEntity
----------                    ---------------- ----------------- ------------------
account_primary_contact       OneToMany        account          contact
account_customer_accounts     OneToMany        account          account
account_parent_account        OneToMany        account          account
account_master_account        OneToMany        account          account
account_Account_Email_Email   OneToMany        account          email
```

Retrieves all relationships involving the `account` entity.

### Example 2: Get a specific relationship by name
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $rel = Get-DataverseRelationshipMetadata -RelationshipName account_primary_contact
PS C:\> [PSCustomObject]@{
    SchemaName = $rel.SchemaName
    RelationshipType = $rel.RelationshipType
    ReferencedEntity = $rel.ReferencedEntity
    ReferencingEntity = $rel.ReferencingEntity
    CascadeDelete = $rel.CascadeDelete.Value
    IsCustomRelationship = $rel.IsCustomRelationship
}

SchemaName            : account_primary_contact
RelationshipType      : OneToMany
ReferencedEntity      : account
ReferencingEntity     : contact
CascadeDelete         : RemoveLink
IsCustomRelationship  : False
```

Retrieves metadata for a specific relationship by schema name.

### Example 3: Get all OneToMany relationships
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $oneToMany = Get-DataverseRelationshipMetadata -RelationshipType OneToMany
PS C:\> $oneToMany.Count
234

PS C:\> $oneToMany | Where-Object { $_.ReferencedEntity -eq "account" } | 
    Select-Object SchemaName, ReferencingEntity, CascadeDelete

SchemaName                    ReferencingEntity CascadeDelete
----------                    ----------------- -------------
account_primary_contact       contact          RemoveLink
account_customer_accounts     account          RemoveLink
account_parent_account        account          RemoveLink
account_master_account        account          RemoveLink
account_Account_Email_Email   email            Cascade
```

Retrieves all OneToMany relationships in the organization.

### Example 4: Get ManyToMany relationships for an entity
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $manyToMany = Get-DataverseRelationshipMetadata -EntityName account -RelationshipType ManyToMany
PS C:\> $manyToMany | Select-Object SchemaName, ReferencedEntity, ReferencingEntity, IntersectEntityName

SchemaName                    ReferencedEntity ReferencingEntity IntersectEntityName
----------                    ----------------- ----------------- -------------------
accountleads_association      account          lead             accountleads
accountopportunities_association account        opportunity      accountopportunities
accountcompetitors_association account          competitor       accountcompetitors
```

Retrieves ManyToMany relationships for the account entity.

### Example 5: Analyze cascade behaviors
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $relationships = Get-DataverseRelationshipMetadata -EntityName account -RelationshipType OneToMany
PS C:\> $relationships | Select-Object SchemaName, ReferencingEntity, 
    @{Name="CascadeDelete"; Expression={$_.CascadeDelete.Value}},
    @{Name="CascadeAssign"; Expression={$_.CascadeAssign.Value}},
    @{Name="CascadeShare"; Expression={$_.CascadeShare.Value}}

SchemaName                    ReferencingEntity CascadeDelete CascadeAssign CascadeShare
----------                    ----------------- ------------- ------------- ------------
account_primary_contact       contact          RemoveLink    NoCascade     NoCascade
account_customer_accounts     account          RemoveLink    NoCascade     NoCascade
account_parent_account        account          RemoveLink    NoCascade     NoCascade
account_master_account        account          RemoveLink    NoCascade     NoCascade
account_Account_Email_Email   email            Cascade       Cascade       Cascade
```

Analyzes cascade behaviors for OneToMany relationships.

### Example 6: Find custom relationships
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $customRelationships = Get-DataverseRelationshipMetadata | 
    Where-Object { $_.IsCustomRelationship -eq $true }

PS C:\> $customRelationships | Select-Object SchemaName, RelationshipType, ReferencedEntity, ReferencingEntity

SchemaName                    RelationshipType ReferencedEntity ReferencingEntity
----------                    ---------------- ----------------- ------------------
new_project_contact           OneToMany        new_project      contact
new_project_task              OneToMany        new_project      new_task
new_project_resource          ManyToMany       new_project      new_resource
```

Finds all custom relationships in the organization.

### Example 7: Get relationships with cascade delete
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $cascadeDeleteRels = Get-DataverseRelationshipMetadata -RelationshipType OneToMany | 
    Where-Object { $_.CascadeDelete.Value -eq "Cascade" }

PS C:\> $cascadeDeleteRels | Select-Object SchemaName, ReferencedEntity, ReferencingEntity, CascadeDelete

SchemaName                    ReferencedEntity ReferencingEntity CascadeDelete
----------                    ----------------- ----------------- -------------
account_Account_Email_Email   account          email            Cascade
contact_Contact_Email_Email   contact          email            Cascade
lead_Lead_Email_Email         lead             email            Cascade
```

Finds relationships that will delete related records when the parent is deleted.

### Example 8: Export relationship metadata to CSV
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $relationships = Get-DataverseRelationshipMetadata -EntityName account
PS C:\> $relationships | Select-Object SchemaName, RelationshipType, ReferencedEntity, ReferencingEntity,
    @{Name="CascadeDelete"; Expression={$_.CascadeDelete.Value}},
    @{Name="IsCustom"; Expression={$_.IsCustomRelationship}} | 
    Export-Csv -Path "account_relationships.csv" -NoTypeInformation
```

Exports relationship metadata to CSV for documentation or analysis.

### Example 9: Find self-referencing relationships
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $selfReferencing = Get-DataverseRelationshipMetadata | 
    Where-Object { $_.ReferencedEntity -eq $_.ReferencingEntity -and $_.RelationshipType -eq "OneToMany" }

PS C:\> $selfReferencing | Select-Object SchemaName, ReferencedEntity, 
    @{Name="IsHierarchical"; Expression={$_.IsHierarchical}}

SchemaName                    ReferencedEntity IsHierarchical
----------                    ----------------- --------------
account_parent_account        account          True
contact_parent_contact        contact          True
systemuser_parent_systemuser  systemuser       True
```

Finds hierarchical (self-referencing) relationships.

### Example 10: Get relationship statistics
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $allRelationships = Get-DataverseRelationshipMetadata
PS C:\> $stats = [PSCustomObject]@{
    TotalRelationships = $allRelationships.Count
    OneToMany          = ($allRelationships | Where-Object { $_.RelationshipType -eq "OneToMany" }).Count
    ManyToOne          = ($allRelationships | Where-Object { $_.RelationshipType -eq "ManyToOne" }).Count
    ManyToMany         = ($allRelationships | Where-Object { $_.RelationshipType -eq "ManyToMany" }).Count
    CustomRelationships = ($allRelationships | Where-Object { $_.IsCustomRelationship }).Count
    SystemRelationships = ($allRelationships | Where-Object { -not $_.IsCustomRelationship }).Count
}

PS C:\> $stats

TotalRelationships  : 456
OneToMany          : 234
ManyToOne          : 189
ManyToMany         : 33
CustomRelationships : 12
SystemRelationships : 444
```

Generates statistics about relationships in the organization.

### Example 11: Find relationships by entity pattern
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $projectRelationships = Get-DataverseRelationshipMetadata | 
    Where-Object { $_.SchemaName -like "*project*" }

PS C:\> $projectRelationships | Select-Object SchemaName, RelationshipType, ReferencedEntity, ReferencingEntity

SchemaName                    RelationshipType ReferencedEntity ReferencingEntity
----------                    ---------------- ----------------- ------------------
new_project_contact           OneToMany        new_project      contact
new_project_task              OneToMany        new_project      new_task
new_project_resource          ManyToMany       new_project      new_resource
new_project_milestone         OneToMany        new_project      new_milestone
```

Finds all relationships related to project entities.

### Example 12: Analyze lookup attributes from relationships
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $oneToMany = Get-DataverseRelationshipMetadata -RelationshipType OneToMany -EntityName account
PS C:\> foreach ($rel in $oneToMany) {
    # Get the lookup attribute metadata
    $lookupAttr = Get-DataverseAttributeMetadata -EntityName $rel.ReferencingEntity -AttributeName $rel.ReferencingAttribute
    
    [PSCustomObject]@{
        Relationship = $rel.SchemaName
        LookupAttribute = $lookupAttr.LogicalName
        DisplayName = $lookupAttr.DisplayName.UserLocalizedLabel.Label
        RequiredLevel = $lookupAttr.RequiredLevel.Value
        IsSearchable = $lookupAttr.IsValidForAdvancedFind.Value
    }
}
```

Analyzes lookup attributes created by OneToMany relationships.

### Example 13: Compare relationships between environments
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $conn1 = Get-DataverseConnection -Url "https://dev.crm.dynamics.com" -Interactive
PS C:\> $conn2 = Get-DataverseConnection -Url "https://prod.crm.dynamics.com" -Interactive

PS C:\> $devRels = Get-DataverseRelationshipMetadata -EntityName account
PS C:\> $prodRels = Get-DataverseRelationshipMetadata -EntityName account

PS C:\> $devSchemas = $devRels | Select-Object -ExpandProperty SchemaName
PS C:\> $prodSchemas = $prodRels | Select-Object -ExpandProperty SchemaName

PS C:\> $onlyInDev = $devSchemas | Where-Object { $_ -notin $prodSchemas }
PS C:\> $onlyInProd = $prodSchemas | Where-Object { $_ -notin $devSchemas }

PS C:\> Write-Host "Relationships only in Dev: $($onlyInDev -join ', ')"
PS C:\> Write-Host "Relationships only in Prod: $($onlyInProd -join ', ')"
```

Compares relationships between development and production environments.

### Example 14: Find relationships with specific cascade behavior
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $restrictDelete = Get-DataverseRelationshipMetadata -RelationshipType OneToMany | 
    Where-Object { $_.CascadeDelete.Value -eq "Restrict" }

PS C:\> $restrictDelete | Select-Object SchemaName, ReferencedEntity, ReferencingEntity

SchemaName                    ReferencedEntity ReferencingEntity
----------                    ----------------- ------------------
account_account_master_account account          account
contact_contact_master_contact contact          contact
```

Finds relationships that prevent deletion of parent records when child records exist.

### Example 15: Pipeline relationship names
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> @("account_primary_contact", "contact_parent_contact", "lead_qualifying_lead") | 
    Get-DataverseRelationshipMetadata | 
    Select-Object SchemaName, RelationshipType, ReferencedEntity, ReferencingEntity

SchemaName                    RelationshipType ReferencedEntity ReferencingEntity
----------                    ---------------- ----------------- ------------------
account_primary_contact       OneToMany        account          contact
contact_parent_contact        OneToMany        contact          contact
lead_qualifying_lead          OneToMany        lead             lead
```

Processes multiple relationship names through the pipeline.

### Example 16: Query only published metadata
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Get only published relationships
PS C:\> $publishedRels = Get-DataverseRelationshipMetadata -EntityName account -Published
PS C:\> $publishedRels.Count
42

PS C:\> # Default behavior includes unpublished (draft) relationships
PS C:\> $unpublishedRels = Get-DataverseRelationshipMetadata -EntityName account
PS C:\> $unpublishedRels.Count
45
```

Demonstrates retrieving only published relationship metadata vs unpublished (draft) metadata. Use -Published to query only relationships that have been published.

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

### -EntityName
Logical name of the entity (table) to retrieve relationships for.

If specified, returns all relationships where this entity is either the referenced entity (parent) or referencing entity (child).

If not specified, returns all relationships in the organization.

```yaml
Type: String
Parameter Sets: (All)
Aliases: TableName

Required: False
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

### -RelationshipName
Schema name of the specific relationship to retrieve.

If specified, returns only that relationship. If not specified, returns all relationships matching other criteria.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RelationshipType
Filter relationships by type.

Valid values:
- **OneToMany**: Parent-child relationships (1:N)
- **ManyToOne**: Child-parent relationships (N:1) - inverse of OneToMany
- **ManyToMany**: Many-to-many relationships (N:N)

If not specified, returns all relationship types.

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: OneToMany, ManyToOne, ManyToMany

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

### Microsoft.Xrm.Sdk.Metadata.RelationshipMetadataBase
## NOTES

This cmdlet provides programmatic access to Dataverse metadata. For comprehensive documentation and examples, see the metadata concept guide at docs/core-concepts/metadata.md

## RELATED LINKS

[Get-DataverseEntityMetadata](Get-DataverseEntityMetadata.md)

[Set-DataverseRelationshipMetadata](Set-DataverseRelationshipMetadata.md)

[Remove-DataverseRelationshipMetadata](Remove-DataverseRelationshipMetadata.md)

[Microsoft Learn: Table relationships](https://learn.microsoft.com/power-apps/maker/data-platform/create-edit-entity-relationships)
