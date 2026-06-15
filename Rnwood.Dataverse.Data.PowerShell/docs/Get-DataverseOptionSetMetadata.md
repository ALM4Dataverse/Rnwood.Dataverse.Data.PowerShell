---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseOptionSetMetadata

## SYNOPSIS
Retrieves option set (choice) metadata from Dataverse.

## SYNTAX

### EntityAttribute
```
Get-DataverseOptionSetMetadata [-EntityName] <String> [-AttributeName] <String> [-Published]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Global
```
Get-DataverseOptionSetMetadata [-Name <String>] [-Published] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The `Get-DataverseOptionSetMetadata` cmdlet retrieves metadata for option sets (choice fields) in Dataverse. It supports both:
- **Local option sets**: Choice fields specific to a single entity attribute
- **Global option sets**: Reusable choice fields shared across multiple entities

The cmdlet returns option set information including:
- Option values and labels
- Option descriptions
- Option colors (for status reasons)
- External values (for virtual entities)
- Option set display name and description

Option set metadata is essential for:
- Understanding available choice values
- Building dropdowns and choice lists
- Validating user input
- Data migration and synchronization
- Generating user documentation

## EXAMPLES

### Example 1: Get option set for an entity attribute
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $optionSet = Get-DataverseOptionSetMetadata -EntityName contact -AttributeName gendercode
PS C:\> $optionSet.Options | Select-Object Value, Label

Value Label
----- -----
1     Male
2     Female
```

Retrieves the options for the `gendercode` choice field on the `contact` entity.

### Example 2: Get all options with descriptions
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $optionSet = Get-DataverseOptionSetMetadata -EntityName lead -AttributeName industrycode
PS C:\> $optionSet.Options | ForEach-Object {
    [PSCustomObject]@{
        Value = $_.Value
        Label = $_.Label.UserLocalizedLabel.Label
        Description = $_.Description.UserLocalizedLabel.Label
    }
}

Value Label              Description
----- -----              -----------
1     Agriculture        Agricultural and farming industry
2     Banking            Financial services and banking
3     Construction       Construction and building
```

Retrieves options with descriptions for the industry code field.

### Example 3: Get a global option set by name
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $globalOptionSet = Get-DataverseOptionSetMetadata -Name new_customerstatus
PS C:\> $globalOptionSet.Options | Select-Object Value, Label

Value Label
----- -----
1     Active
2     Inactive
3     Pending
```

Retrieves a global option set that can be reused across entities.

### Example 4: List all global option sets
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $allGlobalOptionSets = Get-DataverseOptionSetMetadata
PS C:\> $allGlobalOptionSets | Select-Object -First 10 Name, DisplayName

Name                DisplayName
----                -----------
new_customerstatus  Customer Status
new_priority        Priority Levels
new_category        Product Categories
```

Retrieves all global option sets in the organization.

### Example 5: Export option values to CSV
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $optionSet = Get-DataverseOptionSetMetadata -EntityName account -AttributeName industrycode
PS C:\> $optionSet.Options | 
    Select-Object Value, @{N='Label';E={$_.Label.UserLocalizedLabel.Label}} |
    Export-Csv -Path "industry_codes.csv" -NoTypeInformation
```

Exports option set values to CSV for documentation or import.

### Example 6: Find option by label
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $optionSet = Get-DataverseOptionSetMetadata -EntityName contact -AttributeName preferredcontactmethodcode
PS C:\> $emailOption = $optionSet.Options | 
    Where-Object { $_.Label.UserLocalizedLabel.Label -eq 'Email' }
    
PS C:\> $emailOption.Value
1
```

Finds the numeric value for a specific option label.

### Example 7: Get status reason options
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $statusOptions = Get-DataverseOptionSetMetadata -EntityName opportunity -AttributeName statuscode
PS C:\> $statusOptions.Options | Select-Object Value, 
    @{N='Label';E={$_.Label.UserLocalizedLabel.Label}},
    @{N='State';E={$_.State}},
    @{N='Color';E={$_.Color}}

Value Label            State Color
----- -----            ----- -----
1     Open             0     #0078D4
2     Won              1     #107C10
3     Lost             2     #D13438
```

Retrieves status reason options with their colors and state values.

### Example 8: Count options in a choice field
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $optionSet = Get-DataverseOptionSetMetadata -EntityName account -AttributeName accountcategorycode
PS C:\> Write-Host "Number of categories: $($optionSet.Options.Count)"
Number of categories: 5
```

Counts the number of options in a choice field.

### Example 9: Compare option sets between environments
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $dev = Get-DataverseConnection -Url "https://dev.crm.dynamics.com" -Interactive
PS C:\> $prod = Get-DataverseConnection -Url "https://prod.crm.dynamics.com" -Interactive

PS C:\> $devOptions = Get-DataverseOptionSetMetadata -EntityName account -AttributeName industrycode
PS C:\> $prodOptions = Get-DataverseOptionSetMetadata -EntityName account -AttributeName industrycode

PS C:\> $devValues = $devOptions.Options.Value
PS C:\> $prodValues = $prodOptions.Options.Value

PS C:\> $onlyInDev = $devValues | Where-Object { $_ -notin $prodValues }
PS C:\> Write-Host "Options only in Dev: $($onlyInDev -join ', ')"
```

Compares option set values between development and production environments.

### Example 10: Generate lookup table for choice fields
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $optionSet = Get-DataverseOptionSetMetadata -EntityName lead -AttributeName leadqualitycode
PS C:\> $lookup = @{}
PS C:\> foreach ($option in $optionSet.Options) {
    $lookup[$option.Value] = $option.Label.UserLocalizedLabel.Label
}

PS C:\> # Use lookup table
PS C:\> $lookup[3]
Hot
```

Creates a hashtable for quick value-to-label lookups.

### Example 11: Find multi-select choice options
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $attr = Get-DataverseAttributeMetadata -EntityName contact -AttributeName new_interests
PS C:\> if ($attr.AttributeType -eq 'MultiSelectPicklist') {
    $optionSet = Get-DataverseOptionSetMetadata -EntityName contact -AttributeName new_interests
    $optionSet.Options | Select-Object Value, Label
}

Value Label
----- -----
1     Technology
2     Sports
3     Music
4     Travel
```

Retrieves options for a multi-select choice field.

### Example 12: Validate choice value exists
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $optionSet = Get-DataverseOptionSetMetadata -EntityName account -AttributeName industrycode
PS C:\> $valueToCheck = 99
PS C:\> $validValues = $optionSet.Options.Value

PS C:\> if ($valueToCheck -in $validValues) {
    Write-Host "Value $valueToCheck is valid"
} else {
    Write-Host "Value $valueToCheck is NOT valid"
}
```

Validates if a choice value exists in the option set.

### Example 13: Get default option value
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $attr = Get-DataverseAttributeMetadata -EntityName account -AttributeName industrycode
PS C:\> if ($attr.DefaultFormValue) {
    $optionSet = Get-DataverseOptionSetMetadata -EntityName account -AttributeName industrycode
    $defaultOption = $optionSet.Options | 
        Where-Object { $_.Value -eq $attr.DefaultFormValue }
    Write-Host "Default value: $($defaultOption.Label.UserLocalizedLabel.Label)"
}
```

Finds the default value for a choice field.

### Example 14: Generate documentation for all choice fields
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $attributes = Get-DataverseAttributeMetadata -EntityName account
PS C:\> $choiceFields = $attributes | 
    Where-Object { $_.AttributeType -in @('Picklist', 'State', 'Status') }

PS C:\> foreach ($attr in $choiceFields) {
    Write-Host "\n$($attr.DisplayName.UserLocalizedLabel.Label):"
    $optionSet = Get-DataverseOptionSetMetadata -EntityName account -AttributeName $attr.LogicalName
    $optionSet.Options | ForEach-Object {
        Write-Host "  $($_.Value): $($_.Label.UserLocalizedLabel.Label)"
    }
}
```

Generates documentation for all choice fields on an entity.

### Example 15: Handle missing or null labels
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $optionSet = Get-DataverseOptionSetMetadata -EntityName account -AttributeName industrycode
PS C:\> $optionSet.Options | ForEach-Object {
    $label = if ($_.Label.UserLocalizedLabel) { 
        $_.Label.UserLocalizedLabel.Label 
    } else { 
        "(No label)" 
    }
    [PSCustomObject]@{
        Value = $_.Value
        Label = $label
    }
}
```

Handles option sets with missing or null labels gracefully.

### Example 16: Query only published metadata
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Get only published option sets
PS C:\> $publishedOptionSets = Get-DataverseOptionSetMetadata -Published
PS C:\> $publishedOptionSets.Count
52

PS C:\> # Default behavior includes unpublished (draft) option sets
PS C:\> $unpublishedOptionSets = Get-DataverseOptionSetMetadata
PS C:\> $unpublishedOptionSets.Count
55
```

Demonstrates retrieving only published option set metadata vs unpublished (draft) metadata. By default, the cmdlet retrieves unpublished metadata which may include option sets that have been modified but not yet published.

## PARAMETERS

### -AttributeName
Logical name of the choice attribute (column)

```yaml
Type: String
Parameter Sets: EntityAttribute
Aliases: ColumnName

Required: True
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
Parameter Sets: EntityAttribute
Aliases: TableName

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name
Name of the global option set.
If not specified, returns all global option sets.

```yaml
Type: String
Parameter Sets: Global
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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### Microsoft.Xrm.Sdk.Metadata.OptionSetMetadataBase
## NOTES

This cmdlet provides programmatic access to Dataverse metadata. For comprehensive documentation and examples, see the metadata concept guide at docs/core-concepts/metadata.md

## RELATED LINKS
