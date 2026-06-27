---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseAppmoduleIconFromSet

## SYNOPSIS
Sets an app module's icon by downloading an icon from an online icon set and creating/updating a web resource.

## SYNTAX

### ById
```
Set-DataverseAppmoduleIconFromSet [-Id] <Guid> [[-IconSet] <String>] [-IconName] <String>
 -PublisherPrefix <String> [-Publish] [-PassThru] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### ByUniqueName
```
Set-DataverseAppmoduleIconFromSet [-UniqueName] <String> [[-IconSet] <String>] [-IconName] <String>
 -PublisherPrefix <String> [-Publish] [-PassThru] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The `Set-DataverseAppmoduleIconFromSet` cmdlet simplifies the process of setting an app module's icon by:
1. Downloading an SVG icon from an online icon set (e.g., Iconoir, FluentUI, Tabler)
2. Creating or updating a web resource with the icon content
3. Updating the app module's webresourceid property to reference the web resource
4. Optionally publishing the changes

This cmdlet eliminates the manual steps of downloading icons, creating web resources, and updating app module metadata.

## EXAMPLES

### Example 1: Set an app module icon by ID
```powershell
Set-DataverseAppmoduleIconFromSet -Id "12345678-1234-1234-1234-123456789012" -IconName "person" -PublisherPrefix "contoso" -Publish
```

Downloads the "person" icon from FluentUI (default), creates a web resource, sets it as the app module's icon, and publishes the changes.

### Example 2: Set an app module icon by UniqueName
```powershell
Set-DataverseAppmoduleIconFromSet -UniqueName "my_custom_app" -IconName "user" -IconSet Iconoir -PublisherPrefix "contoso" -Publish
```

Downloads the "user" icon from Iconoir, creates a web resource, sets it as the app module's icon, and publishes the changes.

### Example 3: Set icon from Tabler icon set
```powershell
Set-DataverseAppmoduleIconFromSet -UniqueName "sales_app" -IconName "building" -IconSet Tabler -PublisherPrefix "contoso" -Publish
```

Downloads the "building" icon from Tabler (5000+ icons), creates a web resource, sets it as the app module's icon, and publishes the changes.

### Example 4: Set icon and return app module metadata
```powershell
$result = Set-DataverseAppmoduleIconFromSet -Id "12345678-1234-1234-1234-123456789012" -IconName "settings" -PublisherPrefix "contoso" -PassThru
$result.WebResourceId
```

Sets the icon and returns the updated app module metadata showing the web resource ID.

### Example 5: Set icons for multiple app modules
```powershell
@("app1", "app2", "app3") | ForEach-Object {
    Set-DataverseAppmoduleIconFromSet -UniqueName $_ -IconName "star" -PublisherPrefix "contoso" -Publish
}
```

Sets the same icon for multiple app modules.

### Example 6: Browse and set icon interactively
```powershell
# First, browse available icons from FluentUI (default)
$icon = Get-DataverseIconSetIcon -Name "*person*" | Out-GridView -OutputMode Single

# Then set the selected icon
Set-DataverseAppmoduleIconFromSet -UniqueName "my_app" -IconName $icon.Name -PublisherPrefix "contoso" -Publish
```

Browse available icons in a grid view, select one, and set it as the app module icon.

### Example 7: Preview changes without applying
```powershell
Set-DataverseAppmoduleIconFromSet -UniqueName "my_app" -IconName "user" -PublisherPrefix "contoso" -WhatIf
```

Shows what changes would be made without actually applying them.

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

### -IconName
Name of the icon to set (e.g., 'user', 'settings')

```yaml
Type: String
Parameter Sets: (All)
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
Parameter Sets: (All)
Aliases:
Accepted values: FluentUI, Iconoir, Tabler

Required: False
Position: 1
Default value: FluentUI
Accept pipeline input: False
Accept wildcard characters: False
```

### -Id
ID of the app module

```yaml
Type: Guid
Parameter Sets: ById
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -PassThru
Return the updated app module

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

### -Publish
Publish the app module and web resource after updating

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

### -PublisherPrefix
Publisher prefix to use for the web resource name. The web resource will be named as `{prefix}_/icons/{iconset}/{iconname}.svg`.

**This parameter is mandatory.**

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UniqueName
Unique name of the app module

```yaml
Type: String
Parameter Sets: ByUniqueName
Aliases:

Required: True
Position: 0
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
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES
- This cmdlet requires internet access to download icons from the online icon set.
- The web resource is created in the format `{PublisherPrefix}_/icons/{IconSet}/{IconName}.svg`.
- If a web resource with the same name already exists, it will be updated with the new icon content.
- Changes are not visible until published. Use the `-Publish` switch to publish immediately.
- The webresourceid property on the app module is a Guid value, not an EntityReference.

## RELATED LINKS

[Get-DataverseIconSetIcon](Get-DataverseIconSetIcon.md)

[Set-DataverseAppModule](Set-DataverseAppModule.md)

[Get-DataverseAppModule](Get-DataverseAppModule.md)

[Set-DataverseTableIconFromSet](Set-DataverseTableIconFromSet.md)

[FluentUI System Icons](https://github.com/microsoft/fluentui-system-icons)

[Iconoir Icons](https://iconoir.com)

[Tabler Icons](https://github.com/tabler/tabler-icons)
