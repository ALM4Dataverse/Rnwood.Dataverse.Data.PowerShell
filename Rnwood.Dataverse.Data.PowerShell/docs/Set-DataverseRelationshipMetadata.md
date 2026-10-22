---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseRelationshipMetadata

## SYNOPSIS
Creates or updates a relationship in Dataverse.

## SYNTAX

```
Set-DataverseRelationshipMetadata [-SchemaName] <String> -RelationshipType <String> -ReferencedEntity <String>
 -ReferencingEntity <String> [-LookupAttributeSchemaName <String>] [-LookupAttributeDisplayName <String>]
 [-LookupAttributeDescription <String>] [-LookupAttributeRequiredLevel <String>]
 [-IntersectEntitySchemaName <String>] [-CascadeAssign <String>] [-CascadeShare <String>]
 [-CascadeUnshare <String>] [-CascadeReparent <String>] [-CascadeDelete <String>] [-CascadeMerge <String>]
 [-IsHierarchical] [-IsSearchable] [-PassThru] [-Publish] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Creates a new relationship or updates an existing relationship in Dataverse. Supports both OneToMany and ManyToMany relationship types.

When creating a OneToMany relationship, a lookup attribute is automatically created on the referencing entity.

When updating an existing relationship, only certain properties can be modified:
- For OneToMany relationships: Cascade behaviors (Assign, Share, Unshare, Reparent, Delete, Merge) and IsHierarchical
- For ManyToMany relationships: Very limited updateable properties (most are immutable after creation)

## EXAMPLES

### Example 1: Create a OneToMany relationship
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseRelationshipMetadata -SchemaName "new_project_contact" `
    -RelationshipType "OneToMany" `
    -ReferencedEntity "new_project" `
    -ReferencingEntity "contact" `
    -LookupAttributeSchemaName "new_ProjectId" `
    -LookupAttributeDisplayName "Project" `
    -CascadeDelete "RemoveLink"
```

Creates a OneToMany relationship from new_project to contact with a lookup field called new_ProjectId.

### Example 2: Create a ManyToMany relationship
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseRelationshipMetadata -SchemaName "new_project_contact" `
    -RelationshipType "ManyToMany" `
    -ReferencedEntity "new_project" `
    -ReferencingEntity "contact" `
    -IntersectEntitySchemaName "new_project_contact"
```

Creates a ManyToMany relationship between new_project and contact tables.

### Example 3: Update cascade behavior on existing relationship
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseRelationshipMetadata -SchemaName "new_project_contact" `
    -RelationshipType "OneToMany" `
    -ReferencedEntity "new_project" `
    -ReferencingEntity "contact" `
    -CascadeDelete "Cascade" `
    -CascadeAssign "Cascade"
```

Updates an existing OneToMany relationship to change its cascade behaviors.

### Example 4: Create relationship with full cascade configuration
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseRelationshipMetadata -SchemaName "new_task_project" `
    -RelationshipType "OneToMany" `
    -ReferencedEntity "new_project" `
    -ReferencingEntity "new_task" `
    -LookupAttributeSchemaName "new_ProjectId" `
    -LookupAttributeDisplayName "Project" `
    -LookupAttributeRequiredLevel "ApplicationRequired" `
    -CascadeAssign "Cascade" `
    -CascadeShare "Cascade" `
    -CascadeUnshare "Cascade" `
    -CascadeReparent "Cascade" `
    -CascadeDelete "Cascade" `
    -CascadeMerge "Cascade" `
    -PassThru
```

Creates a fully cascading parent-child relationship with all cascade operations set to Cascade.

## PARAMETERS

### -CascadeAssign
Cascade behavior for Assign operations. Determines what happens to related records when the parent record is assigned to a different user.
- NoCascade: No automatic action
- Cascade: Related records are also assigned
- Active: Only active related records are assigned
- UserOwned: Only user-owned related records are assigned
- RemoveLink: Removes the relationship link

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: NoCascade, Cascade, Active, UserOwned, RemoveLink

Required: False
Position: Named
Default value: NoCascade
Accept pipeline input: False
Accept wildcard characters: False
```

### -CascadeDelete
Cascade behavior for Delete operations. Determines what happens to related records when the parent record is deleted.
- NoCascade: No automatic action
- RemoveLink: Removes the relationship link (default)
- Restrict: Prevents deletion if related records exist
- Cascade: Related records are also deleted

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: NoCascade, RemoveLink, Restrict, Cascade

Required: False
Position: Named
Default value: RemoveLink
Accept pipeline input: False
Accept wildcard characters: False
```

### -CascadeMerge
Cascade behavior for Merge operations. Determines what happens to related records when parent records are merged.
- NoCascade: No automatic action
- Cascade: Related records are also re-parented to the merged record

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: NoCascade, Cascade

Required: False
Position: Named
Default value: NoCascade
Accept pipeline input: False
Accept wildcard characters: False
```

### -CascadeReparent
Cascade behavior for Reparent operations. Determines what happens to related records when the lookup value is changed.
- NoCascade: No automatic action
- Cascade: Related records maintain their relationship
- Active: Only active related records maintain relationship
- UserOwned: Only user-owned related records maintain relationship
- RemoveLink: Removes the relationship link

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: NoCascade, Cascade, Active, UserOwned, RemoveLink

Required: False
Position: Named
Default value: NoCascade
Accept pipeline input: False
Accept wildcard characters: False
```

### -CascadeShare
Cascade behavior for Share operations. Determines what happens to related records when the parent record is shared.
- NoCascade: No automatic action
- Cascade: Related records are also shared
- Active: Only active related records are shared
- UserOwned: Only user-owned related records are shared

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: NoCascade, Cascade, Active, UserOwned

Required: False
Position: Named
Default value: NoCascade
Accept pipeline input: False
Accept wildcard characters: False
```

### -CascadeUnshare
Cascade behavior for Unshare operations. Determines what happens to related records when sharing is removed from the parent record.
- NoCascade: No automatic action
- Cascade: Related records also have sharing removed
- Active: Only active related records have sharing removed
- UserOwned: Only user-owned related records have sharing removed

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: NoCascade, Cascade, Active, UserOwned

Required: False
Position: Named
Default value: NoCascade
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

### -IntersectEntitySchemaName
Schema name of the intersect entity for ManyToMany relationships (e.g., 'new_project_contact').
If not specified, uses the relationship SchemaName.

```yaml
Type: String
Parameter Sets: (All)
Aliases: IntersectEntityName

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IsHierarchical
Whether the relationship supports hierarchical relationships. Used for self-referencing relationships that form a hierarchy.

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

### -IsSearchable
Whether the lookup attribute is searchable in advanced find (OneToMany only). Only applicable when creating a new relationship.

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

### -LookupAttributeDescription
Description of the lookup attribute (OneToMany only). Only used when creating a new relationship.

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

### -LookupAttributeDisplayName
Display name of the lookup attribute (OneToMany only). If not specified, the LookupAttributeSchemaName is used. Only used when creating a new relationship.

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

### -LookupAttributeRequiredLevel
Required level of the lookup attribute (OneToMany only):
- None: Optional field
- SystemRequired: Required by the system
- ApplicationRequired: Required by the application
- Recommended: Recommended but not required

Only used when creating a new relationship.

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

### -LookupAttributeSchemaName
Schema name of the lookup attribute to create on the referencing entity (OneToMany only, e.g., 'new_ProjectId').
Required when creating a new OneToMany relationship.

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
Return the created or updated relationship metadata.

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

### -Publish
If specified, publishes the relationship after creating or updating

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

### -ReferencedEntity
Primary/referenced entity name (OneToMany) or first entity name (ManyToMany).
For OneToMany relationships, this is the "parent" or "one" side of the relationship.

```yaml
Type: String
Parameter Sets: (All)
Aliases: PrimaryEntity, Entity1

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ReferencingEntity
Related/referencing entity name (OneToMany) or second entity name (ManyToMany).
For OneToMany relationships, this is the "child" or "many" side of the relationship where the lookup field is created.

```yaml
Type: String
Parameter Sets: (All)
Aliases: RelatedEntity, Entity2

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -RelationshipType
Type of relationship to create:
- OneToMany: Parent-child relationship with a lookup field on the child entity
- ManyToMany: Many-to-many relationship with an intersect table

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: OneToMany, ManyToMany

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SchemaName
Schema name of the relationship (e.g., 'new_project_contact'). This must be unique within the organization.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
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

### None
## OUTPUTS

### Microsoft.Xrm.Sdk.Metadata.RelationshipMetadataBase
## NOTES

This cmdlet provides programmatic access to Dataverse metadata. For comprehensive documentation and examples, see the metadata concept guide at docs/core-concepts/metadata.md

## RELATED LINKS

[Get-DataverseRelationshipMetadata](Get-DataverseRelationshipMetadata.md)
[Remove-DataverseRelationshipMetadata](Remove-DataverseRelationshipMetadata.md)
