---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseTableIconFromSet

## SYNOPSIS
Sets a table's vector icon by downloading an icon from an online icon set and creating/updating a web resource.

## SYNTAX

### FromIconSet (Default)
```
Set-DataverseTableIconFromSet [-EntityName] <String> [[-IconSet] <String>] [-IconName] <String>
 [-PublisherPrefix] <String> [-Publish] [-PassThru] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### FromContent
```
Set-DataverseTableIconFromSet [-EntityName] <String> -IconContent <String> [-PublisherPrefix] <String>
 [-Publish] [-PassThru] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

## DESCRIPTION
The `Set-DataverseTableIconFromSet` cmdlet simplifies the process of setting a table's vector icon by:
1. Downloading an SVG icon from an online icon set (e.g., Iconoir)
2. Creating or updating a web resource with the icon content
3. Updating the table's IconVectorName metadata property to reference the web resource
4. Optionally publishing the changes

This cmdlet eliminates the manual steps of downloading icons, creating web resources, and updating table metadata.

## EXAMPLES

### Example 1: Set a table icon
```powershell
Set-DataverseTableIconFromSet -EntityName "contact" -IconName "person" -PublisherPrefix "contoso" -Publish
```

Downloads the "person" icon from FluentUI (default), creates a web resource, sets it as the contact table's icon, and publishes the changes.

### Example 1b: Set a table icon from a specific icon set
```powershell
Set-DataverseTableIconFromSet -EntityName "contact" -IconName "user" -IconSet Iconoir -PublisherPrefix "contoso" -Publish
```

Downloads the "user" icon from Iconoir, creates a web resource, sets it as the contact table's icon, and publishes the changes.

### Example 1c: Set a table icon from Tabler
```powershell
Set-DataverseTableIconFromSet -EntityName "account" -IconName "building" -IconSet Tabler -PublisherPrefix "contoso" -Publish
```

Downloads the "building" icon from Tabler (5000+ icons), creates a web resource, sets it as the account table's icon, and publishes the changes.

### Example 2: Set icon with custom publisher prefix
```powershell
Set-DataverseTableIconFromSet -EntityName "new_customtable" -IconName "settings" -PublisherPrefix "contoso"
```

Sets the icon using a custom publisher prefix. The web resource will be named "contoso_/icons/settings.svg".

### Example 3: Set icon and return metadata
```powershell
$result = Set-DataverseTableIconFromSet -EntityName "account" -IconName "building" -PublisherPrefix "contoso" -PassThru
$result.IconVectorName
```

Sets the icon and returns the updated entity metadata showing the icon web resource name.

### Example 4: Set icons for multiple tables
```powershell
@("contact", "account", "lead") | ForEach-Object {
    Set-DataverseTableIconFromSet -EntityName $_ -IconName "user" -PublisherPrefix "contoso" -Publish
}
```

Sets the same icon for multiple tables.

### Example 5: Browse and set icon interactively
```powershell
# First, browse available icons from FluentUI (default)
$icon = Get-DataverseIconSetIcon -Name "*person*" | Out-GridView -OutputMode Single

# Then set the selected icon
Set-DataverseTableIconFromSet -EntityName "contact" -IconName $icon.Name -PublisherPrefix "contoso" -Publish
```

Browse available icons in a grid view, select one, and set it as the table icon.

### Example 5b: Browse and set icon from Tabler
```powershell
# Browse available icons from Tabler (5000+ icons)
$icon = Get-DataverseIconSetIcon -IconSet Tabler -Name "*user*" | Out-GridView -OutputMode Single

# Then set the selected icon from Tabler
Set-DataverseTableIconFromSet -EntityName "contact" -IconName $icon.Name -IconSet Tabler -PublisherPrefix "contoso" -Publish
```

Browse available icons from Tabler icon set (5000+ icons), select one, and set it as the table icon.

### Example 6: Preview changes without applying
```powershell
Set-DataverseTableIconFromSet -EntityName "contact" -IconName "user" -PublisherPrefix "contoso" -WhatIf
```

Shows what changes would be made without actually applying them.

## PARAMETERS

### -Connection
Dataverse connection obtained from `Get-DataverseConnection`. If not provided, uses the default connection.

```yaml
Type: ServiceClient
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None (uses default connection)
Accept pipeline input: False
Accept wildcard characters: False
```

### -EntityName
Logical name of the entity (table) to set the icon for.

```yaml
Type: String
Parameter Sets: (All)
Aliases: TableName

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -IconContent
SVG content of the icon to set

```yaml
Type: String
Parameter Sets: FromContent
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IconName
Name of the icon to set (e.g., 'user', 'settings'). Use `Get-DataverseIconSetIcon` to browse available icons.

```yaml
Type: String
Parameter Sets: FromIconSet
Aliases:

Required: True
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IconSet
Icon set to retrieve the icon from. Currently supported icon sets:
- **FluentUI**: Microsoft's Fluent UI System Icons - comprehensive icon library with 2000+ icons from https://github.com/microsoft/fluentui-system-icons
- **Iconoir**: Modern, open-source SVG icon library with 1000+ icons from https://iconoir.com
- **Tabler**: Tabler Icons - open-source icon library with 5000+ icons from https://github.com/tabler/tabler-icons

Default value: `FluentUI`

```yaml
Type: String
Parameter Sets: FromIconSet
Aliases:
Accepted values: FluentUI, Iconoir, Tabler

Required: False
Position: 1
Default value: FluentUI
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
If specified, returns the updated entity metadata.

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
If specified, publishes the entity and web resource after updating.

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

### -PublisherPrefix
Publisher prefix to use for the web resource name. The web resource will be named as `{prefix}_/icons/{iconset}/{iconname}.svg`.


```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 3
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
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES
- This cmdlet requires internet access to download icons from the online icon set.
- The web resource is created in the format `{PublisherPrefix}_/icons/{IconName}.svg`.
- If a web resource with the same name already exists, it will be updated with the new icon content.
- Changes are not visible until published. Use the `-Publish` switch to publish immediately.

## RELATED LINKS

[Get-DataverseIconSetIcon](Get-DataverseIconSetIcon.md)

[Set-DataverseEntityMetadata](Set-DataverseEntityMetadata.md)

[Get-DataverseEntityMetadata](Get-DataverseEntityMetadata.md)

[FluentUI System Icons](https://github.com/microsoft/fluentui-system-icons)

[Iconoir Icons](https://iconoir.com)

[Tabler Icons](https://github.com/tabler/tabler-icons)
