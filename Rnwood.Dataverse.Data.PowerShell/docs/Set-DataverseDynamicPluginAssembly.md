---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseDynamicPluginAssembly

## SYNOPSIS
Compiles C# source code into a plugin assembly, uploads to Dataverse, and automatically manages plugin types.

## SYNTAX

### SourceCode
```
Set-DataverseDynamicPluginAssembly -SourceCode <String> -Name <String> [-FrameworkReferences <String[]>]
 [-PackageReferences <String[]>] [-StrongNameKeyFile <String>] [-Version <String>] [-Culture <String>]
 [-Description <String>] [-PassThru] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [-WhatIf] [-Confirm] [<CommonParameters>]
```

### SourceFile
```
Set-DataverseDynamicPluginAssembly -SourceFile <String> -Name <String> [-FrameworkReferences <String[]>]
 [-PackageReferences <String[]>] [-StrongNameKeyFile <String>] [-Version <String>] [-Culture <String>]
 [-Description <String>] [-PassThru] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
This cmdlet compiles C# source code into a plugin assembly using the Roslyn compiler, embeds build metadata (source code, references, strong name) in the assembly, uploads it to Dataverse, and automatically discovers and manages plugin types. Plugin types are classes that implement the `Microsoft.Xrm.Sdk.IPlugin` interface.

**Important**: All dynamic plugin assemblies are automatically registered in **Sandbox isolation mode** for security. Full-trust mode is not supported.

Key features:
- **Create or Update**: If an assembly with the same name exists, it is updated; otherwise, a new one is created
- **Auto-detect plugin types**: Automatically finds all classes implementing `IPlugin` and creates/updates plugin type records
- **Reuse existing settings**: If not specified, version, culture, strong name key, and references are reused from the existing assembly
- **Generate strong name key**: If no key is specified and no existing assembly has one, a new key is automatically generated
- **Strong name key persistence**: The strong name key is stored in the assembly metadata and automatically reused for subsequent updates, ensuring the public key token remains consistent across updates and preventing "Plugin Assembly fully qualified name has changed" errors
- **Embed metadata**: Source code and build settings are embedded in the assembly for later extraction
- **Error on no plugins**: Throws an error if no plugin types are found in the source code
- **Sandbox isolation**: All assemblies are registered in Sandbox isolation mode for security

## EXAMPLES

### Example 1: Create a simple plugin assembly
```powershell
$sourceCode = @"
using System;
using Microsoft.Xrm.Sdk;

namespace MyPlugins
{
    public class AccountPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Plugin logic here
        }
    }
}
"@

Set-DataverseDynamicPluginAssembly -SourceCode $sourceCode -Name "MyPlugins"
```

Compiles the source code, generates a strong name key, and uploads the assembly to Dataverse. A plugin type record is automatically created for `MyPlugins.AccountPlugin`.

### Example 2: Update an existing assembly with new source code
```powershell
Set-DataverseDynamicPluginAssembly -SourceFile "C:\Plugins\MyPlugins.cs" -Name "MyPlugins" -Version "2.0.0.0"
```

Reads source code from a file, compiles it with version 2.0.0.0, and updates the existing "MyPlugins" assembly. Plugin types are automatically added/removed based on changes in the source code.

### Example 3: Specify package references
```powershell
$sourceCode = @"
using Newtonsoft.Json;
using Microsoft.Xrm.Sdk;
// ... plugin code using Newtonsoft.Json
"@

Set-DataverseDynamicPluginAssembly -SourceCode $sourceCode -Name "JsonPlugin" `
    -PackageReferences "Newtonsoft.Json@13.0.1", "Microsoft.Xrm.Sdk@9.0.2"
```

Compiles the source code with the specified NuGet package references.

### Example 4: Complete plugin development workflow
```powershell
# 1. Create initial plugin
$pluginCode = @"
using System;
using Microsoft.Xrm.Sdk;

namespace MyCompany.Plugins
{
    public class ContactValidationPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.UserId);
            
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                Entity entity = (Entity)context.InputParameters["Target"];
                
                // Validate email
                if (entity.Contains("emailaddress1"))
                {
                    string email = entity.GetAttributeValue<string>("emailaddress1");
                    if (!email.Contains("@"))
                    {
                        throw new InvalidPluginExecutionException("Invalid email address format.");
                    }
                }
            }
        }
    }
}
"@

# Upload to Dataverse
Set-DataverseDynamicPluginAssembly -SourceCode $pluginCode -Name "ContactValidation" -Version "1.0.0.0" -Description "Contact validation plugin"

# 2. Update the plugin later with bug fix (automatically reuses strong name key)
$updatedCode = $pluginCode -replace 'if \(!email\.Contains\("@"\)\)', 'if (string.IsNullOrEmpty(email) || !email.Contains("@"))'

Set-DataverseDynamicPluginAssembly -SourceCode $updatedCode -Name "ContactValidation" -Version "1.1.0.0"

Write-Host "Plugin updated successfully - strong name key automatically reused"
```

Demonstrates the complete workflow of creating and updating a plugin with automatic strong name key persistence.

### Example 5: Extract and modify existing plugin
```powershell
# 1. Download existing assembly from Dataverse
$assembly = Get-DataverseRecord -TableName pluginassembly -FilterValues @{ name = "ContactValidation" } -Columns content

# 2. Extract metadata and source code
$bytes = [Convert]::FromBase64String($assembly.content)
$metadata = Get-DataverseDynamicPluginAssembly -AssemblyBytes $bytes -OutputSourceFile "C:\Temp\Plugin.cs"

Write-Host "Current version: $($metadata.Version)"
Write-Host "Source code saved to C:\Temp\Plugin.cs"

# 3. Modify source code (edit the file manually)
# ... edit C:\Temp\Plugin.cs ...

# 4. Recompile and upload with incremented version
$modifiedSource = Get-Content "C:\Temp\Plugin.cs" -Raw
Set-DataverseDynamicPluginAssembly -SourceCode $modifiedSource -Name "ContactValidation" -Version "1.2.0.0"

Write-Host "Updated plugin deployed with new version"
```

Shows how to extract, modify, and redeploy an existing dynamic plugin assembly.

## PARAMETERS

### -Connection
Dataverse connection obtained from Get-DataverseConnection. If not specified, uses the default connection.

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

### -Culture
Assembly culture. If not specified, the existing assembly culture is reused or 'neutral' is used.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: "neutral" or existing culture
Accept pipeline input: False
Accept wildcard characters: False
```

### -Description
Description of the assembly.

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

### -FrameworkReferences
Framework assembly references (e.g., 'System', 'System.Core'). If not specified, existing references are reused from the assembly if it already exists.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name
Name of the plugin assembly.

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

### -PackageReferences
NuGet package references with versions (e.g., 'Microsoft.Xrm.Sdk@9.0.2'). If not specified, existing references are reused from the assembly if it already exists.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
If specified, the created/updated assembly is written to the pipeline as a PSObject.

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

### -SourceCode
C# source code to compile.

```yaml
Type: String
Parameter Sets: SourceCode
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SourceFile
Path to a C# source file to compile.

```yaml
Type: String
Parameter Sets: SourceFile
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -StrongNameKeyFile
Path to strong name key file (.snk). If not specified, an existing key is reused if the assembly exists, or a new key is automatically generated.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None (auto-generated or reused)
Accept pipeline input: False
Accept wildcard characters: False
```

### -Version
Assembly version (e.g., '1.0.0.0'). If not specified, the existing assembly version is reused or '1.0.0.0' is used.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: "1.0.0.0" or existing version
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
Default value: False
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
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES
- Source code must contain at least one class implementing `Microsoft.Xrm.Sdk.IPlugin`, otherwise an error is thrown
- Plugin types are automatically created/removed based on classes implementing `IPlugin`
- Build metadata (source code, references, strong name info) is embedded in the assembly and can be extracted using `Get-DataverseDynamicPluginAssembly`
- Strong name key files are automatically generated if not specified and not already present in an existing assembly
- **Strong Name Key Persistence**: The strong name key is stored in the assembly metadata and automatically reused when updating the assembly. This ensures the public key token remains consistent across updates and prevents "Plugin Assembly fully qualified name has changed" errors.
- When updating an assembly, if parameters like Version, Culture, FrameworkReferences, PackageReferences, or StrongNameKeyFile are not specified, they are automatically retrieved from the existing assembly's embedded metadata

## RELATED LINKS

[Get-DataverseDynamicPluginAssembly](Get-DataverseDynamicPluginAssembly.md)
[Get-DataversePluginAssembly](Get-DataversePluginAssembly.md)
[Set-DataversePluginAssembly](Set-DataversePluginAssembly.md)
