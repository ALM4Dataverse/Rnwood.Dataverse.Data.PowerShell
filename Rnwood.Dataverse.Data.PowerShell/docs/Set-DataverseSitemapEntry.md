---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseSitemapEntry

## SYNOPSIS
Creates or updates an entry (Area, Group, or SubArea) in a Dataverse sitemap.

## SYNTAX

### Area
```
Set-DataverseSitemapEntry [-InputObject <SitemapEntryInfo>] [-Sitemap <SitemapInfo>]
 [[-SitemapUniqueName] <String>] [-SitemapId <Guid>] [-Area] -EntryId <String> [-ResourceId <String>]
 [-DescriptionResourceId <String>] [-ToolTipResourceId <String>] [-Titles <Hashtable>]
 [-Descriptions <Hashtable>] [-Icon <String>] [-Entity <String>] [-Url <String>] [-Index <Int32>]
 [-Before <String>] [-After <String>] [-IsDefault] [-PassThru] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### Group
```
Set-DataverseSitemapEntry [-InputObject <SitemapEntryInfo>] [-Sitemap <SitemapInfo>]
 [[-SitemapUniqueName] <String>] [-SitemapId <Guid>] [-Group] -EntryId <String> [-ResourceId <String>]
 [-DescriptionResourceId <String>] [-ToolTipResourceId <String>] [-Titles <Hashtable>]
 [-Descriptions <Hashtable>] [-Icon <String>] [-Entity <String>] [-Url <String>] [-ParentAreaId <String>]
 [-Index <Int32>] [-Before <String>] [-After <String>] [-IsDefault] [-PassThru] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### SubArea
```
Set-DataverseSitemapEntry [-InputObject <SitemapEntryInfo>] [-Sitemap <SitemapInfo>]
 [[-SitemapUniqueName] <String>] [-SitemapId <Guid>] [-SubArea] -EntryId <String> [-ResourceId <String>]
 [-DescriptionResourceId <String>] [-ToolTipResourceId <String>] [-Titles <Hashtable>]
 [-Descriptions <Hashtable>] [-Icon <String>] [-Entity <String>] [-Url <String>] [-ParentAreaId <String>]
 [-ParentGroupId <String>] [-Index <Int32>] [-Before <String>] [-After <String>] [-IsDefault] [-PassThru]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### Privilege
```
Set-DataverseSitemapEntry [-InputObject <SitemapEntryInfo>] [-Sitemap <SitemapInfo>]
 [[-SitemapUniqueName] <String>] [-SitemapId <Guid>] [-Privilege] [-EntryId <String>] [-ResourceId <String>]
 [-DescriptionResourceId <String>] [-ToolTipResourceId <String>] [-Titles <Hashtable>]
 [-Descriptions <Hashtable>] [-Icon <String>] [-Entity <String>] [-Url <String>] -ParentSubAreaId <String>
 -PrivilegeEntity <String> -PrivilegeName <String> [-Index <Int32>] [-Before <String>] [-After <String>]
 [-IsDefault] [-PassThru] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf]
 [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Set-DataverseSitemapEntry cmdlet creates or updates navigation entries (Areas, Groups, SubAreas) in a Dataverse sitemap. It automatically detects whether an entry exists and either creates a new entry or updates the existing one. The cmdlet supports precise positioning control when creating entries.

**Multilingual Support:** Use the `-Titles` and `-Descriptions` parameters to specify multilingual labels keyed by LCID (Locale ID). Common LCIDs include:
- 1033: English (US)
- 1036: French
- 1031: German
- 1034: Spanish
- 1041: Japanese

## EXAMPLES

### Example 1: Create a new Area with multilingual titles
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $titles = New-Object 'System.Collections.Generic.Dictionary[[int],[string]]'
PS C:\> $titles.Add(1033, "Sales")
PS C:\> $titles.Add(1036, "Ventes")
PS C:\> Set-DataverseSitemapEntry -SitemapUniqueName "MySitemap" -Area -EntryId "SalesArea" -Titles $titles -Icon "/_imgs/area/sales_24x24.gif"
```

Creates a new Area entry with titles in English and French.

### Example 2: Create a Group with descriptions
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $titles = New-Object 'System.Collections.Generic.Dictionary[[int],[string]]'
PS C:\> $titles.Add(1033, "Leads")
PS C:\> $descriptions = New-Object 'System.Collections.Generic.Dictionary[[int],[string]]'
PS C:\> $descriptions.Add(1033, "Manage potential customers")
PS C:\> Set-DataverseSitemapEntry -SitemapUniqueName "MySitemap" -Group -EntryId "LeadsGroup" -ParentAreaId "SalesArea" -Titles $titles -Descriptions $descriptions -Index 0
```

Creates a new Group at the first position (index 0) within the SalesArea with titles and descriptions.

### Example 3: Create a SubArea with entity reference
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $titles = New-Object 'System.Collections.Generic.Dictionary[[int],[string]]'
PS C:\> $titles.Add(1033, "Accounts")
PS C:\> Set-DataverseSitemapEntry -SitemapUniqueName "MySitemap" -SubArea -EntryId "AccountsSubarea" `
    -ParentAreaId "SalesArea" -ParentGroupId "CustomersGroup" `
    -Entity "account" -Titles $titles -Before "ContactsSubarea"
```

Creates a new SubArea positioned before the "ContactsSubarea" entry.

### Example 4: Add French translation to existing entry
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $frenchTitles = New-Object 'System.Collections.Generic.Dictionary[[int],[string]]'
PS C:\> $frenchTitles.Add(1036, "Ventes et Marketing")
PS C:\> Set-DataverseSitemapEntry -SitemapUniqueName "MySitemap" -Area -EntryId "SalesArea" -Titles $frenchTitles
```

Adds a French title to an existing Area entry (additively - English title is preserved).

### Example 5: Remove a language translation
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $removeFrench = New-Object 'System.Collections.Generic.Dictionary[[int],[string]]'
PS C:\> $removeFrench.Add(1036, $null)
PS C:\> Set-DataverseSitemapEntry -SitemapUniqueName "MySitemap" -Area -EntryId "SalesArea" -Titles $removeFrench
```

Removes the French translation from an Area entry (setting a language to $null removes it).

## PARAMETERS

### -After
The ID of the sibling entry after which this entry should be inserted or moved.

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

### -Area
Create or update an Area entry.

```yaml
Type: SwitchParameter
Parameter Sets: Area
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Before
The ID of the sibling entry before which this entry should be inserted or moved.

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

### -DescriptionResourceId
The resource ID for localized descriptions.

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

### -Descriptions
The descriptions of the entry as a dictionary keyed by LCID (Locale ID). Null values for a specific LCID will remove that LCID's description. Updates are additive - existing LCIDs are preserved unless explicitly set to null.

Common LCIDs: 1033 (English), 1036 (French), 1031 (German), 1034 (Spanish), 1041 (Japanese)

```yaml
Type: Hashtable
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Entity
The new entity logical name (for SubAreas).

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

### -EntryId
The ID of the entry to update.

```yaml
Type: String
Parameter Sets: Area, Group, SubArea
Aliases: Id

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

```yaml
Type: String
Parameter Sets: Privilege
Aliases: Id

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Group
Create or update a Group entry.

```yaml
Type: SwitchParameter
Parameter Sets: Group
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Icon
The new icon path (for Areas and SubAreas).

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

### -Index
The zero-based index position where the entry should be inserted or moved to.

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

### -InputObject
Entry object from Get-DataverseSitemapEntry.

```yaml
Type: SitemapEntryInfo
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -IsDefault
Whether the entry is a default entry.

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

### -ParentAreaId
The parent Area ID (for locating Groups and SubAreas).

```yaml
Type: String
Parameter Sets: Group, SubArea
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ParentGroupId
The parent Group ID (for locating SubAreas).

```yaml
Type: String
Parameter Sets: SubArea
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ParentSubAreaId
The parent SubArea ID (required for Privileges when creating).

```yaml
Type: String
Parameter Sets: Privilege
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
If specified, returns the created or updated entry.

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

### -Privilege
The privilege required to view this entry.

```yaml
Type: SwitchParameter
Parameter Sets: Privilege
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PrivilegeEntity
The entity name for privilege entries.

```yaml
Type: String
Parameter Sets: Privilege
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PrivilegeName
The privilege name for privilege entries (e.g., Read, Write, Create, Delete).

```yaml
Type: String
Parameter Sets: Privilege
Aliases:
Accepted values: Read, Write, Create, Delete, Append, AppendTo, Share, Assign

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
Determines how PowerShell responds to progress updates generated by the cmdlet.

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

### -ResourceId
The new resource ID for localized titles.

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

### -Sitemap
Sitemap object from Get-DataverseSitemap.

```yaml
Type: SitemapInfo
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -SitemapId
The ID of the sitemap containing the entry.

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

### -SitemapUniqueName
The unique name of the sitemap containing the entry.

```yaml
Type: String
Parameter Sets: (All)
Aliases: Name

Required: False
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -SubArea
Create or update a SubArea entry.

```yaml
Type: SwitchParameter
Parameter Sets: SubArea
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Titles
The titles of the entry as a dictionary keyed by LCID (Locale ID). Null values for a specific LCID will remove that LCID's title. Updates are additive - existing LCIDs are preserved unless explicitly set to null.

Common LCIDs: 1033 (English), 1036 (French), 1031 (German), 1034 (Spanish), 1041 (Japanese)

```yaml
Type: Hashtable
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ToolTipResourceId
The resource ID for localized tooltips.

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

### -Url
The new URL (for SubAreas).

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

### Rnwood.Dataverse.Data.PowerShell.Commands.SitemapEntryInfo
### Rnwood.Dataverse.Data.PowerShell.Commands.SitemapInfo
### System.String
### System.Nullable`1[[System.Guid, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]
## OUTPUTS

### Rnwood.Dataverse.Data.PowerShell.Commands.SitemapEntryInfo
## NOTES

## RELATED LINKS
