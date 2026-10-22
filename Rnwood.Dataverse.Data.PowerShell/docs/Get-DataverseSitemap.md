---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseSitemap

## SYNOPSIS
Retrieves sitemap information from a Dataverse environment.

## SYNTAX

```
Get-DataverseSitemap [[-Name] <String>] [-UniqueName <String>] [-Id <Guid>] [-Published]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet retrieves sitemap information from a Dataverse environment. Sitemaps define the navigation structure for model-driven apps.

You can retrieve all sitemaps or filter by name, ID, or unique name.

The cmdlet returns SitemapInfo objects with metadata about each sitemap including name, XML definition, and timestamps.

## EXAMPLES

### Example 1: Retrieve all sitemaps
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseSitemap

Name              Id                                   UniqueName
----              --                                   ----------
MySitemap         a1b2c3d4-5678-90ab-cdef-1234567890ab mysitemap
DefaultSitemap    f1e2d3c4-b5a6-9087-6543-210fedcba987 defaultsitemap
```

Retrieves all sitemaps from the Dataverse environment.

### Example 2: Retrieve a specific sitemap by name
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseSitemap -Name "MySitemap"

Name              : MySitemap
Id                : a1b2c3d4-5678-90ab-cdef-1234567890ab
UniqueName        : mysitemap
CreatedOn         : 1/15/2024 10:30:00 AM
ModifiedOn        : 1/20/2024 2:45:00 PM
SitemapXml        : <SiteMap>...</SiteMap>
```

Retrieves a specific sitemap by its name with full details.

### Example 3: Get sitemap by unique name
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseSitemap -UniqueName "mysitemap_unique"

Name              Id                                   UniqueName
----              --                                   ----------
MySitemap         a1b2c3d4-5678-90ab-cdef-1234567890ab mysitemap_unique
```

Retrieves a specific sitemap by its unique name.

### Example 4: Get sitemap by ID
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseSitemap -Id "a1b2c3d4-5678-90ab-cdef-1234567890ab"

Name              Id                                   CreatedOn             ModifiedOn
----              --                                   ---------             ----------
MySitemap         a1b2c3d4-5678-90ab-cdef-1234567890ab 1/15/2024 10:30:00 AM 1/20/2024 2:45:00 PM
```

Retrieves a sitemap by its unique identifier.

### Example 5: Export sitemap XML to file
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $sitemap = Get-DataverseSitemap -Name "MySitemap"
PS C:\> $sitemap.SitemapXml | Out-File -FilePath "MySitemap.xml"
```

Retrieves a sitemap and exports its XML definition to a file.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet. If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.

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
The unique identifier of the sitemap to retrieve.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name
The name of the sitemap to retrieve. If not specified, all sitemaps are returned.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

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
Allows published records to be retrieved instead of the default behavior that includes both published and unpublished records

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

### -UniqueName
The unique name of the sitemap to retrieve.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### Rnwood.Dataverse.Data.PowerShell.Commands.SitemapInfo
## NOTES

This cmdlet requires an active connection to a Dataverse environment.

Sitemaps define the navigation structure for model-driven apps in Dataverse. The sitemap XML follows the SiteMap schema and contains Area, Group, and SubArea elements that define the app navigation.

## RELATED LINKS

[Set-DataverseSitemap](Set-DataverseSitemap.md)

[Remove-DataverseSitemap](Remove-DataverseSitemap.md)

[Get-DataverseRecord](Get-DataverseRecord.md)
