---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseView

## SYNOPSIS
Retrieves view information (savedquery or userquery) from a Dataverse environment.

## SYNTAX

```
Get-DataverseView [[-Id] <Guid>] [-Name <String>] [-TableName <String>] [-ViewType <String>]
 [-QueryType <QueryType>] [-Raw] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
The Get-DataverseView cmdlet retrieves view definitions from Dataverse environments. Views define how records are displayed in model-driven apps and other Dataverse interfaces.

By default, the cmdlet returns parsed view information with Columns, Filters, Links, and OrderBy properties extracted from the FetchXML and LayoutXML. Use the -Raw parameter to return the raw attribute values from the savedquery/userquery record.

Views can be filtered by ID, name (with wildcard support), table name, view type (system vs personal), and query type.

## EXAMPLES

### Example 1: Get a specific view by ID
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseView -Id "12345678-1234-1234-1234-123456789012"
```

Retrieves a specific view by its ID with parsed Columns, Filters, Links, and OrderBy properties.

### Example 2: Get all views for a table
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseView -TableName contact
```

Retrieves all views (both system and personal) for the contact table.

### Example 3: Get system views only
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseView -ViewType "System"
```

Retrieves all system views (savedquery entities).

### Example 4: Get personal views only
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseView -ViewType "Personal"
```

Retrieves all personal views (userquery entities).

### Example 5: Find views by name with wildcards
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseView -Name "Active*"
```

Finds all views whose names start with "Active" using wildcard pattern matching.

### Example 6: Get views with raw values
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseView -Id $viewId -Raw
```

Retrieves a view with raw attribute values instead of parsed properties. Returns all attributes from the savedquery/userquery record including fetchxml, layoutxml, querytype, etc.

### Example 7: Get Advanced Search views
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseView -QueryType AdvancedSearch
```

Retrieves all Advanced Search views (typically used in Advanced Find).

### Example 8: Get system views for a specific table
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseView -TableName account -ViewType "System"
```

Retrieves all system views for the account table.

### Example 9: Clone a view using parsed properties
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $view = Get-DataverseView -Id $viewId
PS C:\> Set-DataverseView -PassThru `
    -Name "$($view.Name) (Copy)" `
    -TableName $view.TableName `
    -ViewType $view.ViewType `
    -Columns $view.Columns `
    -FilterValues $view.Filters `
    -Links $view.Links `
    -OrderBy $view.OrderBy
```

Gets a view and uses its parsed properties to create a clone. The Columns property includes name and width configuration in the format expected by Set-DataverseView.

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

### -Id
The ID of the view to retrieve.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Name
The name of the view to retrieve.
Supports wildcards (* and ?).

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: True
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
View type to filter by. Different query types are used for different display contexts.
Valid values: MainApplicationView, AdvancedSearch, SubGrid, QuickFindSearch, Reporting, OfflineFilters, LookupView, SMAppointmentBookView, MainApplicationViewWithoutSubject, SavedQueryTypeOther, InteractiveWorkflowView, OfflineTemplate, CustomDefinedView, ExportFieldTranslationsView, OutlookTemplate, AddressBookFilters, OutlookFilters, CopilotView

```yaml
Type: QueryType
Parameter Sets: (All)
Aliases:
Accepted values: MainApplicationView, AdvancedSearch, SubGrid, QuickFindSearch, Reporting, OfflineFilters, LookupView, SMAppointmentBookView, OutlookFilters, AddressBookFilters, MainApplicationViewWithoutSubject, SavedQueryTypeOther, InteractiveWorkflowView, OfflineTemplate, CustomDefinedView, CopilotView, ExportFieldTranslationsView, OutlookTemplate

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Raw
Return raw attribute values instead of parsed properties.
When specified, returns all attributes from the savedquery/userquery record including fetchxml, layoutxml, and other metadata.
When not specified (default), returns parsed properties: Columns (array of column configurations with name/width), Filters (parsed filter hashtables), Links (parsed link entities), and OrderBy (sort specifications).

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

### -TableName
Logical name of the table to retrieve views for.

```yaml
Type: String
Parameter Sets: (All)
Aliases: EntityName

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ViewType
Retrieve views of the specified type.
Valid values: "System" (savedquery) or "Personal" (userquery).
If not specified, retrieves both system and personal views.

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: System, Personal

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Guid
### System.String
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES

**Default Behavior:**
- Returns both system and personal views unless ViewType is specified
- Automatically pages through results if multiple views match criteria
- Converts FetchXML to QueryExpression for parsing filters, links, and ordering

**Parsed Properties:**
- The Columns property includes both column names and widths from LayoutXML
- Filters are converted from FetchXML to hashtable format compatible with Set-DataverseView
- Links are converted from FetchXML link-entity elements to DataverseLinkEntity format
- OrderBy is extracted from FetchXML order elements (suffix "-" indicates descending)

**Use Cases:**
- View auditing and documentation
- Cloning views (parsed properties match Set-DataverseView input format)
- Analyzing view configuration across environments
- Finding views by pattern matching

**Performance:**
- Uses automatic paging for large result sets
- Queries both savedquery and userquery tables when ViewType is not specified

## RELATED LINKS

[View Management Documentation](../../docs/core-concepts/view-management.md)

[Set-DataverseView](Set-DataverseView.md)

[Remove-DataverseView](Remove-DataverseView.md)

[Querying Records](../../docs/core-concepts/querying.md)
