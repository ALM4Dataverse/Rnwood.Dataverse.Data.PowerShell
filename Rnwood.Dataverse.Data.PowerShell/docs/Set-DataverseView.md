---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseView

## SYNOPSIS
Creates or updates Dataverse views (savedquery and userquery entities) with flexible column and filter configuration.

## SYNTAX

```
Set-DataverseView [-Id <Guid>] [-Name <String>] [-TableName <String>] [-ViewType <String>]
 [-Description <String>] [-Columns <Object[]>] [-AddColumns <Object[]>] [-InsertColumnsBefore <String>]
 [-InsertColumnsAfter <String>] [-RemoveColumns <String[]>] [-UpdateColumns <Hashtable[]>]
 [-FilterValues <Hashtable[]>] [-FetchXml <String>] [-Links <DataverseLinkEntity[]>] [-OrderBy <String[]>]
 [-LayoutXml <String>] [-IsDefault] [-QueryType <QueryType>] [-NoUpdate] [-NoCreate] [-PassThru]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Set-DataverseView cmdlet creates new views or updates existing ones using an upsert pattern. Views define how records are displayed in model-driven apps and other Dataverse interfaces.

The cmdlet supports both simplified syntax (using Columns and FilterValues parameters) and advanced FetchXML-based configuration. Column configurations can be specified as simple strings or detailed hashtables with width and other properties.

When the Id parameter is provided, the cmdlet updates the existing view with that ID. When the Id parameter is not provided but Name and TableName are specified, the cmdlet searches for an existing view with that Name and TableName combination. If found, it updates that view; otherwise, it creates a new view. This prevents creating duplicate views when the same Name and TableName are used.

When updating existing views, you can add, remove, or update specific columns without affecting other view properties.

## EXAMPLES

### Example 1: Create a basic personal view
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseView -PassThru `
    -Name "My Active Contacts" `
    -TableName contact `
 -Columns @("firstname", "lastname", "emailaddress1", "telephone1") `
    -FilterValues @{ statecode = 0 }
```

Creates a personal view (default) showing active contacts with specified columns.

### Example 2: Create a system view with column widths
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseView -PassThru `
  -Name "All Active Contacts" `
    -TableName contact `
    -ViewType "System" `
  -Columns @(
 @{ name = "firstname"; width = 100 },
        @{ name = "lastname"; width = 150 },
      @{ name = "emailaddress1"; width = 250 },
  @{ name = "telephone1"; width = 120 }
    ) `
    -FilterValues @{ statecode = 0 }
```

Creates a system view accessible to all users with specific column widths.

### Example 3: Update an existing view
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseView -Id $viewId `
    -ViewType "Personal" `
    -Name "Updated Contact View" `
    -Description "Shows active contacts with email"
```

Updates the name and description of an existing view. ViewType must be specified when updating.

### Example 4: Update or create a view by Name (upsert pattern)
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # First call creates the view
PS C:\> Set-DataverseView -PassThru `
    -Name "Active Contacts" `
    -TableName contact `
    -ViewType "System" `
    -Columns @("firstname", "lastname", "emailaddress1") `
    -Description "Shows active contacts"

PS C:\> # Second call with same Name and TableName updates the existing view
PS C:\> Set-DataverseView -PassThru `
    -Name "Active Contacts" `
    -TableName contact `
    -ViewType "System" `
    -Columns @("firstname", "lastname", "emailaddress1", "telephone1") `
    -Description "Updated to include telephone"
```

Demonstrates the upsert behavior when Name and TableName are provided without Id. The first call creates a new view. The second call finds the existing view by Name and TableName, then updates it instead of creating a duplicate. This is useful for configuration scripts that can be run multiple times.

### Example 5: Add columns to a view
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseView -Id $viewId `
    -ViewType "Personal" `
    -AddColumns @(
        @{ name = "address1_city"; width = 150 },
        @{ name = "birthdate"; width = 100 }
    )
```

Adds new columns to an existing view without affecting existing columns.

### Example 6: Add columns at specific positions
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseView -Id $viewId `
    -ViewType "Personal" `
    -AddColumns @("mobilephone", "fax") `
    -InsertColumnsAfter "telephone1"
```

Adds mobile phone and fax columns after the telephone1 column.

### Example 7: Insert columns before a specific column
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseView -Id $viewId `
    -ViewType "Personal" `
    -AddColumns @("jobtitle") `
    -InsertColumnsBefore "emailaddress1"
```

Inserts the job title column before the email address column in the layout.

### Example 8: Clone a view using Get-DataverseView output
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $originalView = Get-DataverseView -Id $originalViewId
PS C:\> Set-DataverseView -PassThru `
    -Name "$($originalView.Name) (Copy)" `
    -TableName $originalView.TableName `
    -ViewType $originalView.ViewType `
    -Columns $originalView.Columns `
    -FilterValues $originalView.Filters `
    -Links $originalView.Links `
    -OrderBy $originalView.OrderBy
```

Retrieves a view and creates a copy. The properties returned by Get-DataverseView (Columns, Filters, Links, OrderBy) are in the format expected by Set-DataverseView.

### Example 9: Create view with complex filters
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseView -PassThru `
    -Name "High Value Opportunities" `
    -TableName opportunity `
    -ViewType "System" `
    -Columns @("name", "estimatedvalue", "closeprobability", "actualclosedate") `
    -FilterValues @{
        and = @(
     @{ statecode = 0 },
      @{ or = @(
      @{ estimatedvalue = @{ value = 100000; operator = 'GreaterThan' } },
 @{ closeprobability = @{ value = 80; operator = 'GreaterThan' } }
    )}
        )
  }
```

Creates a system view with nested logical filter expressions using AND/OR operators.

### Example 10: Use FetchXML for advanced queries
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $fetchXml = @"
<fetch>
  <entity name="contact">
    <attribute name="firstname" />
    <attribute name="lastname" />
    <attribute name="emailaddress1" />
    <filter type="and">
    <condition attribute="statecode" operator="eq" value="0" />
      <condition attribute="createdon" operator="last-x-days" value="30" />
    </filter>
    <order attribute="createdon" descending="true" />
  </entity>
</fetch>
"@

PS C:\> Set-DataverseView -PassThru `
    -Name "Recent Contacts" `
    -TableName contact `
  -FetchXml $fetchXml
```

Creates a view using FetchXML for complex query definitions with date-based filters and sorting.

## PARAMETERS

### -AddColumns
Columns to add to the view.
Can be an array of column names or hashtables with column configuration (name, width, etc.)
Use InsertColumnsBefore or InsertColumnsAfter parameters to control the position where columns are inserted.

```yaml
Type: Object[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Columns
Columns to include in the view.
Can be an array of column names (strings) or hashtables with column configuration.
When using hashtables, supported properties include:
- `name` (required): The logical name of the column
- `width`: Display width in pixels (optional, default is 100)

```yaml
Type: Object[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
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

### -Description
Description of the view

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -FetchXml
FetchXml query to use for the view.
When provided, overrides Columns, FilterValues, Links, and OrderBy parameters.

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

### -FilterValues
One or more hashtables to filter records.
Each hashtable's entries are combined with AND; multiple hashtables are combined with OR.
Keys may be 'column' or 'column:Operator' (Operator is a ConditionOperator name).
Values may be a literal, an array (treated as IN), $null (treated as ISNULL), or a nested hashtable with keys 'value' and 'operator'.
Supports grouped filters using 'and', 'or', 'not', or 'xor' keys with nested hashtables for complex logical expressions.

```yaml
Type: Hashtable[]
Parameter Sets: (All)
Aliases: Filters

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Id
ID of the view to update.
If not specified or if the view doesn't exist, a new view is created.
When updating, ViewType must also be specified.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -InsertColumnsAfter
Column name to insert new columns after in the layout.
Used with AddColumns parameter to specify the position where new columns should be inserted.
Cannot be used together with InsertColumnsBefore.

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

### -InsertColumnsBefore
Column name to insert new columns before in the layout.
Used with AddColumns parameter to specify the position where new columns should be inserted.
Cannot be used together with InsertColumnsAfter.

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

### -IsDefault
Set this view as the default view for the table.
Only applicable to system views.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -LayoutXml
Layout XML for the view.
If not specified when creating, a default layout will be generated from Columns parameter.

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

### -Links
Link entities to apply to the view query.
Accepts DataverseLinkEntity objects or hashtables with link configuration.

```yaml
Type: DataverseLinkEntity[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Name
Name of the view.
Required when creating a new view.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -NoCreate
If specified, then no view will be created even if no existing view matching the ID is found.
Use this to ensure only existing views are updated.

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

### -NoUpdate
If specified, existing views matching the ID will not be updated.
Use this to ensure only new views are created.

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

### -OrderBy
List of columns to order records by.
Suffix column name with - to sort descending (e.g., "createdon-", "name").

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -PassThru
If specified, returns the ID of the created or updated view.

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

### -QueryType
View type for different display contexts.
Valid values: MainApplicationView, AdvancedSearch, SubGrid, QuickFindSearch, Reporting, OfflineFilters, LookupView, SMAppointmentBookView, MainApplicationViewWithoutSubject, SavedQueryTypeOther, InteractiveWorkflowView, OfflineTemplate, CustomDefinedView, ExportFieldTranslationsView, OutlookTemplate, AddressBookFilters, OutlookFilters, CopilotView.
Default is AdvancedSearch.

```yaml
Type: QueryType
Parameter Sets: (All)
Aliases:
Accepted values: MainApplicationView, AdvancedSearch, SubGrid, QuickFindSearch, Reporting, OfflineFilters, LookupView, SMAppointmentBookView, OutlookFilters, AddressBookFilters, MainApplicationViewWithoutSubject, SavedQueryTypeOther, InteractiveWorkflowView, OfflineTemplate, CustomDefinedView, CopilotView, ExportFieldTranslationsView, OutlookTemplate

Required: False
Position: Named
Default value: AdvancedSearch
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -RemoveColumns
Columns to remove from the view.
Specify the logical names of columns to remove.

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

### -TableName
Logical name of the table this view is for.
Required when creating a new view. When updating an existing view, this parameter is optional as the table name is automatically determined from the view's metadata or FetchXML.

```yaml
Type: String
Parameter Sets: (All)
Aliases: EntityName

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -UpdateColumns
Columns to update in the view.
Hashtables with column configuration (name, width, etc.).
Only the specified columns are updated; others remain unchanged.

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

### -ViewType
Specify "System" for system views (savedquery) or "Personal" for personal views (userquery).
Default is "Personal" for new views.
Required when updating existing views.

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: System, Personal

Required: False
Position: Named
Default value: Personal (for new views)
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

### System.Guid
### System.String
### System.Object[]
### System.Collections.Hashtable[]
### Rnwood.Dataverse.Data.PowerShell.Commands.DataverseLinkEntity[]
### System.String[]
### System.Management.Automation.SwitchParameter
### System.Nullable`1[[Rnwood.Dataverse.Data.PowerShell.Commands.QueryType, Rnwood.Dataverse.Data.PowerShell.Cmdlets, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]
## OUTPUTS

### System.Guid
## NOTES

**Upsert Pattern:**
- Creates new views if ID is not specified or view doesn't exist
- Updates existing views if ID matches an existing view
- ViewType must be specified when updating by ID

**Column Format Compatibility:**
- The `Columns` parameter accepts the same format as returned by `Get-DataverseView`
- This enables easy view cloning and modification workflows

**Update Behavior:**
- When updating, only specified parameters are modified
- Other properties remain unchanged
- Use AddColumns, RemoveColumns, UpdateColumns for granular control

**View Types:**
- System views (ViewType="System") are accessible to all users
- Personal views (ViewType="Personal") are user-specific
- Default QueryType is AdvancedSearch for new views

**Complex Filters:**
- Supports nested logical expressions using 'and', 'or', 'not', and 'xor' keys
- Multiple hashtables in FilterValues are combined with OR
- Entries within a hashtable are combined with AND

**Safety Features:**
- Supports WhatIf and Confirm for safe operation
- Use -NoCreate or -NoUpdate to control upsert behavior

## RELATED LINKS

[View Management Documentation](../../docs/core-concepts/view-management.md)

[Get-DataverseView](Get-DataverseView.md)

[Remove-DataverseView](Remove-DataverseView.md)

[Querying Records](../../docs/core-concepts/querying.md)
