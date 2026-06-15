---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseSitemapEntry

## SYNOPSIS
Retrieves sitemap entries (Areas, Groups, SubAreas) from a Dataverse sitemap.

## SYNTAX

### Default (Default)
```
Get-DataverseSitemapEntry [-Sitemap <SitemapInfo>] [[-SitemapUniqueName] <String>] [-SitemapId <Guid>]
 [-EntryId <String>] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Area
```
Get-DataverseSitemapEntry [-Sitemap <SitemapInfo>] [[-SitemapUniqueName] <String>] [-SitemapId <Guid>] [-Area]
 [-EntryId <String>] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Group
```
Get-DataverseSitemapEntry [-Sitemap <SitemapInfo>] [[-SitemapUniqueName] <String>] [-SitemapId <Guid>] [-Group]
 [-EntryId <String>] [-ParentAreaId <String>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### SubArea
```
Get-DataverseSitemapEntry [-Sitemap <SitemapInfo>] [[-SitemapUniqueName] <String>] [-SitemapId <Guid>]
 [-SubArea] [-EntryId <String>] [-ParentAreaId <String>] [-ParentGroupId <String>]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Privilege
```
Get-DataverseSitemapEntry [-Sitemap <SitemapInfo>] [[-SitemapUniqueName] <String>] [-SitemapId <Guid>]
 [-Privilege] [-EntryId <String>] -ParentSubAreaId <String> [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The Get-DataverseSitemapEntry cmdlet retrieves navigation entries (Areas, Groups, SubAreas) from a Dataverse sitemap XML definition. It supports filtering by entry type, ID, and parent relationships.

## EXAMPLES

### Example 1: Get all entries from a sitemap
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseSitemapEntry -SitemapUniqueName "MySitemap"
```

Retrieves all navigation entries (Areas, Groups, and SubAreas) from the specified sitemap.

### Example 2: Get only Areas
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseSitemapEntry -SitemapUniqueName "MySitemap" -Area
```

Retrieves only Area entries from the sitemap.

### Example 3: Get a specific entry
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseSitemapEntry -SitemapUniqueName "MySitemap" -EntryId "SalesArea"
```

Retrieves a specific entry by its ID.

### Example 4: Get Groups within an Area
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseSitemapEntry -SitemapUniqueName "MySitemap" -Group -ParentAreaId "SalesArea"
```

Retrieves all Groups within the specified Area.

### Example 5: Use pipeline
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseSitemap -Name "MySitemap" | Get-DataverseSitemapEntry -Area
```

Pipes a sitemap object to retrieve its Area entries.

## PARAMETERS

### -Area
Retrieve Area entries.

```yaml
Type: SwitchParameter
Parameter Sets: Area
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

### -EntryId
The ID of a specific entry to retrieve.

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

### -Group
Retrieve Group entries.

```yaml
Type: SwitchParameter
Parameter Sets: Group
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ParentAreaId
Filter entries by parent Area ID (for Groups and SubAreas).

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
Filter SubAreas by parent Group ID.

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
Filter Privileges by parent SubArea ID (required for Privileges).

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

### -Privilege
Retrieve Privilege entries.

```yaml
Type: SwitchParameter
Parameter Sets: Privilege
Aliases:

Required: False
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

### -Sitemap
Sitemap object from Get-DataverseSitemap.

```yaml
Type: SitemapInfo
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -SitemapId
The ID of the sitemap to retrieve entries from.

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
The unique name of the sitemap to retrieve entries from.

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
Retrieve SubArea entries.

```yaml
Type: SwitchParameter
Parameter Sets: SubArea
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

### Rnwood.Dataverse.Data.PowerShell.Commands.SitemapInfo
### System.String
### System.Nullable`1[[System.Guid, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]
## OUTPUTS

### Rnwood.Dataverse.Data.PowerShell.Commands.SitemapEntryInfo
## NOTES

## RELATED LINKS
