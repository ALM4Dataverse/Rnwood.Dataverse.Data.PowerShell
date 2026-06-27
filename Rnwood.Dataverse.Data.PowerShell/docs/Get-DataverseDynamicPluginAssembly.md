---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseDynamicPluginAssembly

## SYNOPSIS
Extracts source code and build metadata from a dynamic plugin assembly.

## SYNTAX

### ById (Default)
```
Get-DataverseDynamicPluginAssembly -Id <Guid> [-OutputSourceFile <String>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### VSProjectById
```
Get-DataverseDynamicPluginAssembly -Id <Guid> [-OutputSourceFile <String>] -OutputProjectPath <String>
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### ByName
```
Get-DataverseDynamicPluginAssembly -Name <String> [-OutputSourceFile <String>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### VSProjectByName
```
Get-DataverseDynamicPluginAssembly -Name <String> [-OutputSourceFile <String>] -OutputProjectPath <String>
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Bytes
```
Get-DataverseDynamicPluginAssembly -AssemblyBytes <Byte[]> [-OutputSourceFile <String>]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### VSProjectFromBytes
```
Get-DataverseDynamicPluginAssembly -AssemblyBytes <Byte[]> [-OutputSourceFile <String>]
 -OutputProjectPath <String> [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### FilePath
```
Get-DataverseDynamicPluginAssembly -FilePath <String> [-OutputSourceFile <String>]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### VSProjectFromFile
```
Get-DataverseDynamicPluginAssembly -FilePath <String> [-OutputSourceFile <String>] -OutputProjectPath <String>
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
This cmdlet extracts embedded metadata from plugin assemblies created with `Set-DataverseDynamicPluginAssembly`. The metadata includes the original C# source code, framework and package references, version information, culture, and the strong name key used for signing.

Dynamic plugin assemblies embed this metadata during compilation to enable:
- **Source code retrieval** for review or modification
- **Rebuilding with the same settings** (references, version, culture)
- **Strong name key reuse** to maintain consistent public key tokens across updates
- **Visual Studio project export** for development using standard .NET tools

**Retrieval Methods**

This cmdlet supports multiple ways to retrieve plugin assembly metadata:

1. **Direct from Dataverse** - Retrieve by name or ID using a Dataverse connection
2. **From Assembly Bytes** - Extract metadata from raw assembly bytes (e.g., after downloading from Dataverse)
3. **From File** - Extract metadata from a local DLL file

**Visual Studio Project Export**

When the `-OutputProjectPath` parameter is specified, the cmdlet generates a complete, buildable Visual Studio project:

- **{AssemblyName}.csproj** - SDK-style project file targeting .NET Framework 4.6.2
- **{AssemblyName}.cs** - Extracted C# source code
- **{AssemblyName}.snk** - Strong name key for assembly signing

The generated project can be built using standard `dotnet build` commands without any manual configuration. This enables a complete development workflow:

1. Export plugin to VS project using this cmdlet
2. Modify source code in your favorite editor or IDE
3. Build using `dotnet build` (produces properly signed DLL)
4. Update plugin in Dataverse using `Set-DataverseDynamicPluginAssembly` with the modified source code

**When to Use VS Project Export**

**Use VS project export when you need to:**
- Develop and test plugins locally with standard .NET tooling
- Use an IDE like Visual Studio or VS Code for editing with IntelliSense
- Debug plugins locally before deploying to Dataverse
- Apply refactoring tools or code analysis
- Maintain plugin source code in version control systems
- Collaborate with team members using familiar development workflows

**Continue using inline source code when:**
- Making quick changes or prototyping
- Working with simple, single-file plugins
- Automating deployments in CI/CD pipelines
- No local development environment is needed

If the assembly was not created with `Set-DataverseDynamicPluginAssembly` or does not contain embedded metadata, a warning is displayed and no output is returned.

## EXAMPLES

### Example 1: Extract metadata from an assembly file
```powershell
$metadata = Get-DataverseDynamicPluginAssembly -FilePath "C:\Plugins\MyDynamicPlugin.dll"
Write-Host "Assembly: $($metadata.AssemblyName)"
Write-Host "Version: $($metadata.Version)"
Write-Host "Public Key Token: $($metadata.PublicKeyToken)"
Write-Host "Framework References: $($metadata.FrameworkReferences -join ', ')"
Write-Host "Package References: $($metadata.PackageReferences -join ', ')"
```

Extracts and displays metadata from a plugin assembly file.

### Example 2: Extract source code from downloaded assembly
```powershell
# Download assembly from Dataverse
$assembly = Get-DataverseRecord -TableName pluginassembly -FilterValues @{ name = "MyDynamicPlugin" } -Columns content

# Decode and extract metadata
$assemblyBytes = [Convert]::FromBase64String($assembly.content)
$metadata = Get-DataverseDynamicPluginAssembly -AssemblyBytes $assemblyBytes -OutputSourceFile "C:\Temp\Plugin.cs"

Write-Host "Source code saved to C:\Temp\Plugin.cs"
Write-Host "Assembly uses $($metadata.FrameworkReferences.Count) framework references"
```

Downloads a plugin assembly from Dataverse, extracts its metadata, and saves the source code to a file.

### Example 3: Verify assembly metadata before update
```powershell
# Get current assembly
$assembly = Get-DataverseRecord -TableName pluginassembly -FilterValues @{ name = "MyPlugin" } -Columns content
$bytes = [Convert]::FromBase64String($assembly.content)
$metadata = Get-DataverseDynamicPluginAssembly -AssemblyBytes $bytes

# Display current settings
Write-Host "Current version: $($metadata.Version)"
Write-Host "Current culture: $($metadata.Culture)"
Write-Host "Public key token: $($metadata.PublicKeyToken)"

# Update with modified source code but same settings
Set-DataverseDynamicPluginAssembly -SourceCode $modifiedSource -Name "MyPlugin"
```

Retrieves the current metadata from an existing assembly to verify settings before updating it.

### Example 4: Export to Visual Studio project for local development
```powershell
# Connect to Dataverse
$connection = Get-DataverseConnection -Url "https://org.crm.dynamics.com" -Interactive

# Export plugin assembly directly from Dataverse to a VS project
Get-DataverseDynamicPluginAssembly -Connection $connection `
    -Name "MyCompany.Plugins" `
    -OutputProjectPath "C:\Dev\MyPlugin"

# The following files are created:
#   C:\Dev\MyPlugin\MyCompany.Plugins.csproj  - SDK-style project file (net462)
#   C:\Dev\MyPlugin\MyCompany.Plugins.cs      - C# source code
#   C:\Dev\MyPlugin\MyCompany.Plugins.snk     - Strong name key

# Build the project using standard dotnet tools
cd C:\Dev\MyPlugin
dotnet build  # Produces bin/Debug/net462/MyCompany.Plugins.dll
```

Exports a plugin assembly from Dataverse to a complete Visual Studio project that can be built with standard `dotnet build` commands.

### Example 5: Complete development workflow - Export, modify, build, update
```powershell
$connection = Get-DataverseConnection -Url "https://org.crm.dynamics.com" -Interactive

# Step 1: Export existing plugin to VS project
Get-DataverseDynamicPluginAssembly -Name "MyPlugin" -OutputProjectPath "C:\Dev\MyPlugin"

# Step 2: Modify the source code (edit MyPlugin.cs in your favorite editor)
#   - Use Visual Studio Code, Visual Studio, or any text editor
#   - IntelliSense and debugging are available in IDEs
#   - Make your code changes

# Step 3: Build the modified project
cd C:\Dev\MyPlugin
dotnet build

# Step 4: Update plugin in Dataverse with modified source code
$updatedSource = Get-Content "MyPlugin.cs" -Raw
Set-DataverseDynamicPluginAssembly -SourceCode $updatedSource -Name "MyPlugin" -Version "2.0.0.0"

# The plugin assembly is updated in Dataverse with your changes
# Plugin steps remain registered - no need to recreate them
```

Demonstrates the complete round-trip workflow: export to VS project, modify using standard tools, build locally, and update in Dataverse.

### Example 6: Export from assembly bytes with VS project generation
```powershell
# Download assembly content from Dataverse
$assembly = Get-DataverseRecord -TableName pluginassembly `
    -FilterValues @{ name = "MyPlugin" } `
    -Columns content

# Export to VS project from bytes (no connection required for this step)
$bytes = [Convert]::FromBase64String($assembly.content)
Get-DataverseDynamicPluginAssembly -AssemblyBytes $bytes `
    -OutputProjectPath "C:\Dev\ExportedPlugin"

# Build to verify the exported project is valid
cd C:\Dev\ExportedPlugin
dotnet build
```

Exports a plugin to a VS project from previously downloaded assembly bytes, useful when you already have the assembly content.

### Example 7: Retrieve metadata by ID and export to VS project
```powershell
# Get plugin assembly ID
$assembly = Get-DataverseRecord -TableName pluginassembly `
    -FilterValues @{ name = "MyPlugin" } `
    -Columns pluginassemblyid

# Export using assembly ID
Get-DataverseDynamicPluginAssembly -Id $assembly.pluginassemblyid `
    -OutputProjectPath "C:\Dev\MyPlugin"
```

Retrieves a plugin assembly by ID and exports it to a Visual Studio project.

### Example 8: Use default connection for export
```powershell
# Set default connection
$connection = Get-DataverseConnection -Url "https://org.crm.dynamics.com" -Interactive
Set-DataverseConnectionAsDefault -Connection $connection

# Export without specifying -Connection parameter
Get-DataverseDynamicPluginAssembly -Name "MyPlugin" `
    -OutputProjectPath "C:\Dev\MyPlugin"
```

Uses the default Dataverse connection to export a plugin assembly to a VS project.

## PARAMETERS

### -AssemblyBytes
Assembly bytes

```yaml
Type: Byte[]
Parameter Sets: Bytes, VSProjectFromBytes
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

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

### -FilePath
Path to assembly file

```yaml
Type: String
Parameter Sets: FilePath, VSProjectFromFile
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Id
ID of the plugin assembly

```yaml
Type: Guid
Parameter Sets: ById, VSProjectById
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Name
Name of the plugin assembly

```yaml
Type: String
Parameter Sets: ByName, VSProjectByName
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutputProjectPath
Output directory for Visual Studio project

```yaml
Type: String
Parameter Sets: VSProjectById, VSProjectByName, VSProjectFromBytes, VSProjectFromFile
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutputSourceFile
Output path for extracted source code

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

### -ProgressAction
Determines how PowerShell responds to progress updates. See about_CommonParameters for details.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Guid
### System.Byte[]
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES

**Output Properties:**
- **AssemblyName**: The name of the assembly
- **Version**: Assembly version (e.g., "1.0.0.0")
- **Culture**: Assembly culture (e.g., "neutral")
- **PublicKeyToken**: Hex string of the public key token used for strong naming
- **SourceCode**: The original C# source code used to compile the assembly
- **FrameworkReferences**: Array of .NET Framework assembly references
- **PackageReferences**: Array of NuGet package references with versions (e.g., "Newtonsoft.Json@13.0.1")

**Metadata Format:**
The metadata is embedded at the end of the assembly file with a marker "DPLM" (Dynamic Plugin Library Metadata). The strong name key is stored as a base64-encoded string and is automatically reused when updating the assembly with `Set-DataverseDynamicPluginAssembly`.

**Visual Studio Project Export:**
When `-OutputProjectPath` is specified, three files are generated:
1. **{AssemblyName}.csproj** - SDK-style project file with:
   - Target framework: net462 (required for Dataverse plugins)
   - Assembly name, version, and signing configuration
   - PackageReference to Microsoft.CrmSdk.CoreAssemblies
   - All framework and package references from metadata
2. **{AssemblyName}.cs** - Complete C# source code extracted from assembly
3. **{AssemblyName}.snk** - Strong name key for consistent assembly signing

The generated project:
- Builds successfully with `dotnet build` without any modifications
- Produces a properly signed DLL in bin/Debug/net462/
- Maintains the same PublicKeyToken as the original assembly
- Can be opened in Visual Studio, VS Code, or any .NET IDE
- Supports standard development workflows (debugging, refactoring, testing)

**Connection Parameter:**
- Required for `ById` and `ByName` parameter sets (retrieves from Dataverse)
- Optional for `Bytes` and `FilePath` parameter sets (extracts from local data)
- Can use default connection set via `Set-DataverseConnectionAsDefault`

**Use Cases:**
- Review the source code of deployed plugins
- Extract build settings to replicate the configuration
- Audit framework and package dependencies
- Retrieve the strong name key for manual builds
- **Export to VS project for local development with standard .NET tools**
- **Modify plugins using IDEs with IntelliSense and debugging**
- **Build and test plugins locally before deploying to Dataverse**
- **Maintain plugin source code in version control systems**
- **Enable team collaboration using familiar development workflows**

**Development Workflow:**
The VS project export enables a complete round-trip development workflow:
1. Create plugin with `Set-DataverseDynamicPluginAssembly -SourceCode`
2. Export to VS project with `Get-DataverseDynamicPluginAssembly -OutputProjectPath`
3. Modify source in your preferred IDE
4. Build with `dotnet build` (standard .NET tooling)
5. Update with `Set-DataverseDynamicPluginAssembly -SourceCode` (no deletion needed)

This workflow combines the rapid deployment of dynamic plugins with the power of traditional IDE-based development.

## RELATED LINKS

[Set-DataverseDynamicPluginAssembly](Set-DataverseDynamicPluginAssembly.md)
[Get-DataversePluginAssembly](Get-DataversePluginAssembly.md)

