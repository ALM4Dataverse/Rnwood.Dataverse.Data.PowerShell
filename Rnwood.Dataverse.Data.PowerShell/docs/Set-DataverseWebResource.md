---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseWebResource

## SYNOPSIS
Creates or updates web resources in a Dataverse environment.

## SYNTAX

### InputObject
```
Set-DataverseWebResource -InputObject <PSObject> [-Id <Guid>] [-Name <String>] [-Publish] [-PassThru]
 [-NoUpdate] [-NoCreate] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf]
 [-Confirm] [<CommonParameters>]
```

### File
```
Set-DataverseWebResource [-Id <Guid>] -Name <String> -Path <String> [-DisplayName <String>]
 [-Description <String>] [-WebResourceType <WebResourceType>] [-Publish] [-PassThru] [-NoUpdate] [-NoCreate]
 [-PublisherPrefix <String>] [-IfNewer] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [-WhatIf] [-Confirm] [<CommonParameters>]
```

### Folder
```
Set-DataverseWebResource -Folder <String> [-DisplayName <String>] [-Publish] [-PassThru] [-NoUpdate]
 [-NoCreate] [-FileFilter <String>] [-PublisherPrefix <String>] [-IfNewer] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Creates or updates web resources in Dataverse. Web resources are files like JavaScript, HTML, CSS, images, 
and other static assets that can be referenced from forms, apps, and other components.

The cmdlet supports three modes:
- **File mode**: Upload a single file as a web resource
- **Folder mode**: Upload multiple files from a folder
- **InputObject mode**: Create/update from an object (useful with pipeline)

By default, the cmdlet will update existing web resources or create new ones. Use -NoUpdate or -NoCreate 
to control this behavior. The web resource type is auto-detected from file extension unless explicitly specified.

## EXAMPLES

### Example 1: Upload a JavaScript file
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseWebResource -Name "new_myscript" -Path "./script.js" -Publish
```

Uploads a JavaScript file as a web resource and publishes it.

### Example 2: Upload multiple files from a folder
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseWebResource -Folder "./webresources" -PublisherPrefix "new_" -Publish
```

Uploads all files from the folder as web resources with the specified publisher prefix.

### Example 3: Update only if file is newer
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseWebResource -Folder "./dist" -PublisherPrefix "new_" -IfNewer -Publish
```

Updates web resources only if the local files are newer than the existing web resources.

### Example 4: Create from object
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $webResource = @{
    name = "new_customscript"
    displayname = "Custom Script"
    webresourcetype = 3  # JavaScript
    content = [Convert]::ToBase64String([System.Text.Encoding]::UTF8.GetBytes("alert('Hello');"))
}
PS C:\> $webResource | Set-DataverseWebResource -PassThru
```

Creates a web resource from a PowerShell object and returns the created resource.

### Example 5: Filter files in folder
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseWebResource -Folder "./assets" -FileFilter "*.png" -PublisherPrefix "new_" -Publish
```

Uploads only PNG files from the folder as web resources.

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

### -Description
Description for the web resource

```yaml
Type: String
Parameter Sets: File
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DisplayName
Display name for the web resource

```yaml
Type: String
Parameter Sets: File, Folder
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -FileFilter
File filter pattern for folder operations (e.g., '*.js', '*.html').
Default is '*.*'

```yaml
Type: String
Parameter Sets: Folder
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Folder
Folder path containing multiple web resource files.
File names are used as web resource names.

```yaml
Type: String
Parameter Sets: Folder
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Id
ID of the web resource to update.
If not specified, creates a new web resource or matches by name.

```yaml
Type: Guid
Parameter Sets: InputObject, File
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IfNewer
If set, only updates if the file modification time is newer than the web resource modified time

```yaml
Type: SwitchParameter
Parameter Sets: File, Folder
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputObject
Object containing web resource properties (name, displayname, webresourcetype, content, etc.)

```yaml
Type: PSObject
Parameter Sets: InputObject
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -Name
Name of the web resource.
Used to match existing resources if Id is not provided.

```yaml
Type: String
Parameter Sets: InputObject
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

```yaml
Type: String
Parameter Sets: File
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoCreate
If set, skips creating new web resources (only updates existing ones)

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

### -NoUpdate
If set, skips updating existing web resources (only creates new ones)

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

### -PassThru
If set, returns the created/updated web resource object

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

### -Path
File path containing the web resource content

```yaml
Type: String
Parameter Sets: File
Aliases:

Required: True
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
If set, publishes the web resource after creation/update

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
Publisher prefix for new web resources (e.g., 'new_').
Required for new web resources if Name doesn't contain a prefix.

```yaml
Type: String
Parameter Sets: File, Folder
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WebResourceType
Web resource type: 1=HTML, 2=CSS, 3=JS, 4=XML, 5=PNG, 6=JPG, 7=GIF, 8=XAP, 9=XSL, 10=ICO, 11=SVG, 12=RESX.
Auto-detected from file extension if not specified.

```yaml
Type: WebResourceType
Parameter Sets: File
Aliases:
Accepted values: HTML, CSS, JavaScript, XML, PNG, JPG, GIF, XAP, XSL, ICO, SVG, RESX

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

### System.Management.Automation.PSObject
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES

## RELATED LINKS
