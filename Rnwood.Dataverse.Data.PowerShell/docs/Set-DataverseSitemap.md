---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseSitemap

## SYNOPSIS
Creates or updates a sitemap in Dataverse.

## SYNTAX

```
Set-DataverseSitemap [[-Name] <String>] [-Id <Guid>] [-UniqueName <String>] [-SitemapXml <String>] [-PassThru]
 [-Publish] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

## DESCRIPTION

This cmdlet creates a new sitemap or updates an existing sitemap in a Dataverse environment. Sitemaps define the navigation structure for model-driven apps.

When creating a new sitemap, the Name parameter is required. When updating an existing sitemap, specify the Id or UniqueName parameter to identify which sitemap to update. The Name parameter is optional when updating.

## EXAMPLES

### Example 1: Create a new sitemap
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $sitemapXml = @"
<SiteMap>
  <Area Id="Area1" ResourceId="Area1.Title">
    <Group Id="Group1" ResourceId="Group1.Title">
      <SubArea Id="SubArea1" ResourceId="SubArea1.Title" Entity="account" />
    </Group>
  </Area>
</SiteMap>
"@

PS C:\> Set-DataverseSitemap -Name "MySitemap" -SitemapXml $sitemapXml -PassThru

a1b2c3d4-5678-90ab-cdef-1234567890ab
```

Creates a new sitemap with the specified name and XML definition, returning the new sitemap ID.

### Example 2: Update an existing sitemap
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $sitemap = Get-DataverseSitemap -Name "MySitemap"
PS C:\> $updatedXml = $sitemap.SitemapXml -replace 'Area1', 'UpdatedArea1'
PS C:\> Set-DataverseSitemap -Name "MySitemap" -Id $sitemap.Id -SitemapXml $updatedXml

Sitemap 'MySitemap' updated successfully.
```

Retrieves an existing sitemap, modifies its XML, and updates it.

### Example 3: Load sitemap XML from file and create sitemap
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $sitemapXml = Get-Content -Path "MySitemap.xml" -Raw
PS C:\> Set-DataverseSitemap -Name "ImportedSitemap" -SitemapXml $sitemapXml

Sitemap 'ImportedSitemap' created successfully with ID: b2c3d4e5-6789-01bc-def2-345678901bcd
```

Loads sitemap XML from a file and creates a new sitemap.

### Example 4: Add a new area to an existing sitemap
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $sitemap = Get-DataverseSitemap -Name "MySitemap"
PS C:\> $xml = [xml]$sitemap.SitemapXml
PS C:\> 
PS C:\> # Create new area element
PS C:\> $newArea = $xml.CreateElement("Area")
PS C:\> $newArea.SetAttribute("Id", "NewArea")
PS C:\> $newArea.SetAttribute("ResourceId", "NewArea.Title")
PS C:\> 
PS C:\> # Add to sitemap
PS C:\> $xml.SiteMap.AppendChild($newArea)
PS C:\> 
PS C:\> # Update sitemap
PS C:\> Set-DataverseSitemap -Name "MySitemap" -Id $sitemap.Id -SitemapXml $xml.OuterXml

Sitemap 'MySitemap' updated successfully.
```

Demonstrates programmatic manipulation of sitemap XML to add a new navigation area.

### Example 5: Clone a sitemap
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $source = Get-DataverseSitemap -Name "OriginalSitemap"
PS C:\> Set-DataverseSitemap -Name "ClonedSitemap" -SitemapXml $source.SitemapXml -PassThru

c3d4e5f6-7890-12cd-ef34-56789012cdef
```

Creates a copy of an existing sitemap with a different name.

### Example 6: Create and publish a sitemap
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $sitemapXml = @"
<SiteMap>
  <Area Id="SalesArea" ResourceId="SalesArea.Title">
    <Group Id="SalesGroup" ResourceId="SalesGroup.Title">
      <SubArea Id="Accounts" ResourceId="Accounts.Title" Entity="account" />
      <SubArea Id="Contacts" ResourceId="Contacts.Title" Entity="contact" />
    </Group>
  </Area>
</SiteMap>
"@

PS C:\> Set-DataverseSitemap -Name "SalesSitemap" -SitemapXml $sitemapXml -Publish

Sitemap 'SalesSitemap' created successfully with ID: d4e5f6g7-8901-23de-f456-789012d3ef45
Published sitemap with ID: d4e5f6g7-8901-23de-f456-789012d3ef45
```

Creates a new sitemap and immediately publishes it so it's available in model-driven apps.

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
The unique identifier of the sitemap to update. If not specified, a new sitemap is created.

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

### -Name
The name property of the sitemap. Required when creating a new sitemap, optional when updating.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -PassThru
If specified, the cmdlet returns the ID of the created or updated sitemap.

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

### -Publish
If specified, publishes the sitemap after creating or updating.

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

### -SitemapXml
The XML definition of the sitemap. Must be valid XML conforming to the SiteMap schema.

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

### -UniqueName
The unique name (key) of the sitemap to update. If a sitemap with this unique name exists, it will be updated; otherwise, a new sitemap is created with this unique name.

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

### System.String
### System.Nullable`1[[System.Guid, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]
## OUTPUTS

### System.Guid
## NOTES

This cmdlet requires an active connection to a Dataverse environment.

The sitemap XML must conform to the SiteMap schema. A sitemap typically contains Area elements (top-level navigation areas), which contain Group elements (groupings within an area), which contain SubArea elements (individual navigation items).

## RELATED LINKS

[Get-DataverseSitemap](Get-DataverseSitemap.md)

[Remove-DataverseSitemap](Remove-DataverseSitemap.md)

[Set-DataverseRecord](Set-DataverseRecord.md)
