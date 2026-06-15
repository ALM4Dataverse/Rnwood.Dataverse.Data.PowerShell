---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseXrmToolbox

## SYNOPSIS
Invokes an XrmToolbox plugin downloaded from NuGet with the current Dataverse connection injected.

## SYNTAX

```
Invoke-DataverseXrmToolbox [-PackageName] <String> [-Version <String>] [-CacheDirectory <String>] [-Force]
 [-Name <String>] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

The `Invoke-DataverseXrmToolbox` cmdlet downloads and executes XrmToolbox plugins directly from PowerShell. It automatically injects your current Dataverse connection into the plugin, allowing you to use XrmToolbox tools without leaving your PowerShell environment.

The cmdlet handles:
- Downloading plugins from NuGet
- Caching downloaded packages for reuse
- Launching a .NET Framework 4.8 host process (required for XrmToolbox plugin compatibility)
- Injecting the current Dataverse connection via named pipes
- Providing dynamic token refresh for long-running sessions

## EXAMPLES

### Example 1: Launch FetchXML Builder
```powershell
# Connect to Dataverse
$conn = Get-DataverseConnection -Interactive -Url "https://yourorg.crm.dynamics.com"

# Launch FetchXML Builder plugin
Invoke-DataverseXrmToolbox -PackageName "Cinteros.Xrm.FetchXMLBuilder"
```

This example connects to a Dataverse environment and launches the FetchXML Builder plugin with the connection automatically injected.

### Example 2: Launch a specific version
```powershell
# Launch a specific version of the Metadata Browser
Invoke-DataverseXrmToolbox -PackageName "MsCrmTools.MetadataBrowser" -Version "1.2024.5.12"
```

This example launches a specific version of the Metadata Browser plugin.

### Example 3: Force refresh of cached plugin
```powershell
# Force re-download of the plugin
Invoke-DataverseXrmToolbox -PackageName "Cinteros.Xrm.FetchXMLBuilder" -Force
```

This example forces a re-download of the plugin, even if it's already cached locally.

### Example 4: Use custom cache directory
```powershell
# Use a custom cache directory
Invoke-DataverseXrmToolbox `
    -PackageName "MsCrmTools.WebResourcesManager" `
    -CacheDirectory "C:\MyPluginCache"
```

This example uses a custom directory for caching downloaded plugins.

## PARAMETERS

### -CacheDirectory
The directory where NuGet packages should be cached. Defaults to `%LOCALAPPDATA%\Rnwood.Dataverse.Data.PowerShell\XrmToolboxPlugins`.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: %LOCALAPPDATA%\Rnwood.Dataverse.Data.PowerShell\XrmToolboxPlugins
Accept pipeline input: False
Accept wildcard characters: False
```

### -Connection
The Dataverse connection to use. If not specified, the default connection will be used.

```yaml
Type: ServiceClient
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: Default connection
Accept pipeline input: False
Accept wildcard characters: False
```

### -Force
Force re-download of the package even if it's already cached.

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

### -Name
The name of the plugin to load if the assembly contains multiple plugins.

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

### -PackageName
The NuGet package ID of the XrmToolbox plugin to execute. Examples include:
- `Cinteros.Xrm.FetchXMLBuilder` - FetchXML Builder
- `MsCrmTools.MetadataBrowser` - Metadata Browser
- `MsCrmTools.WebResourcesManager` - Web Resources Manager

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

### -Version
The version of the NuGet package to download. If not specified, the latest version will be used.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: Latest version
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### System.Void
## NOTES

**Important**: This cmdlet requires a .NET Framework 4.8 host process to run XrmToolbox plugins, as they are built for .NET Framework. The host process is automatically launched when you invoke a plugin.

**Platform Support**: 
- Windows: Fully supported
- Linux/macOS: Not supported (requires .NET Framework 4.8 and Windows Forms)

**Connection Injection**: The cmdlet uses named pipes to pass connection details to the host process. This allows:
- Dynamic token refresh for long-running sessions
- Secure transfer of credentials
- Support for all authentication methods supported by `Get-DataverseConnection`

**Caching**: Downloaded plugins are cached locally to improve performance. Use the `-Force` parameter to force a re-download.

**Runtime Isolation**: Each invocation creates a unique runtime directory with the XrmToolBox folder structure (`Plugins/`, `Settings/`, `Logs/`, `Connections/`). This ensures:
- Plugins that use `XrmToolBox.Extensibility.Paths` work correctly
- Multiple concurrent plugin invocations don't interfere with each other
- Each session has isolated settings and logs
- Runtime directories are automatically cleaned up after 1 hour

**Plugin Compatibility**: This cmdlet is compatible with any XrmToolbox plugin distributed as a NuGet package. If a plugin has specific requirements or dependencies, you may need to manually install them.

## RELATED LINKS

[Get-DataverseConnection](Get-DataverseConnection.md)

[XrmToolbox Website](https://www.xrmtoolbox.com/)

[XrmToolbox Plugin Store](https://www.xrmtoolbox.com/plugins)
