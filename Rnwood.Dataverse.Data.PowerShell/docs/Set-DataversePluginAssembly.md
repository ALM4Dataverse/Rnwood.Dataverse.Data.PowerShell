---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataversePluginAssembly

## SYNOPSIS
Creates or updates a plugin assembly in a Dataverse environment.

## SYNTAX

### Content
```
Set-DataversePluginAssembly [-Id <Guid>] -Name <String> -Content <Byte[]>
 [-IsolationMode <PluginAssemblyIsolationMode>] [-SourceType <PluginAssemblySourceType>] [-Version <String>]
 [-Culture <String>] [-PublicKeyToken <String>] [-Description <String>] [-PassThru]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### FilePath
```
Set-DataversePluginAssembly [-Id <Guid>] -Name <String> -FilePath <String>
 [-IsolationMode <PluginAssemblyIsolationMode>] [-SourceType <PluginAssemblySourceType>] [-Version <String>]
 [-Culture <String>] [-PublicKeyToken <String>] [-Description <String>] [-PassThru]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Set-DataversePluginAssembly cmdlet creates a new plugin assembly or updates an existing one in a Dataverse environment. You can provide the assembly content as a byte array or read from a file path.

## EXAMPLES

### Example 1: Create a new plugin assembly from file
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataversePluginAssembly -Name "MyPlugin" -FilePath "C:\Plugins\MyPlugin.dll" -IsolationMode 2
```

Creates a new plugin assembly by reading from a file with external isolation mode.

### Example 2: Update an existing plugin assembly
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataversePluginAssembly -Id 12345678-1234-1234-1234-123456789012 -Name "MyPlugin" -FilePath "C:\Plugins\MyPlugin.dll" -Version "2.0.0"
```

Updates an existing plugin assembly with new content and version.

### Example 3: Create plugin assembly with PassThru
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $assembly = Set-DataversePluginAssembly -Name "MyPlugin" -FilePath "C:\Plugins\MyPlugin.dll" -PassThru
PS C:\> $assembly.Id
```

Creates a new plugin assembly and returns the created object.

## PARAMETERS

### -Connection
The Dataverse ServiceClient connection to use. If not specified, the default connection is used.

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

### -Content
The content of the assembly as a byte array.

```yaml
Type: Byte[]
Parameter Sets: Content
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Culture
The culture of the assembly.

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

### -Description
The description of the assembly.

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

### -FilePath
The path to the assembly file to upload.

```yaml
Type: String
Parameter Sets: FilePath
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Id
The ID of the plugin assembly to update. If not specified, a new assembly is created.

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

### -IsolationMode
The isolation mode: 0=None, 1=Sandbox, 2=External (default).

```yaml
Type: PluginAssemblyIsolationMode
Parameter Sets: (All)
Aliases:
Accepted values: None, Sandbox, External

Required: False
Position: Named
Default value: 2
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name
The name of the plugin assembly.

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

### -PassThru
If specified, the created/updated assembly is written to the pipeline.

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

### -PublicKeyToken
The public key token of the assembly.

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

### -SourceType
The source type: 0=Database (default), 1=Disk, 2=Normal, 3=AzureWebApp.

```yaml
Type: PluginAssemblySourceType
Parameter Sets: (All)
Aliases:
Accepted values: Database, Disk, Normal, AzureWebApp, FileStore

Required: False
Position: Named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -Version
The version of the assembly.

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

### System.Nullable`1[[System.Guid, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES

## RELATED LINKS

[Get-DataversePluginAssembly](Get-DataversePluginAssembly.md)
[Remove-DataversePluginAssembly](Remove-DataversePluginAssembly.md)
