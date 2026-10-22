---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseWebResource

## SYNOPSIS
Retrieves web resources from a Dataverse environment.

## SYNTAX

### Id
```
Get-DataverseWebResource -Id <Guid> [-Path <String>] [-Folder <String>] [-IncludeContent] [-DecodeContent]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Query
```
Get-DataverseWebResource [-Name <String>] [-WebResourceType <WebResourceType>] [-DisplayName <String>]
 [-Unmanaged] [-Path <String>] [-Folder <String>] [-IncludeContent] [-DecodeContent]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Retrieves web resources from Dataverse with support for filtering by name, type, and other attributes.
By default, the content property is excluded from results to improve performance. 
Use -IncludeContent to include the content property or -Path/-Folder to save content to files.

## EXAMPLES

### Example 1: Get web resource metadata without content
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseWebResource -Name "new_myscript"
```

Retrieves the web resource metadata without the content property.

### Example 2: Get web resource with content
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseWebResource -Name "new_myscript" -IncludeContent
```

Retrieves the web resource including its base64-encoded content.

### Example 3: Download web resource to file
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseWebResource -Name "new_myscript" -Path "./local-script.js"
```

Downloads the web resource content to a local file. Content is automatically fetched even without -IncludeContent.

### Example 4: Get web resources by type
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseWebResource -WebResourceType JavaScript
```

Retrieves all JavaScript web resources (metadata only, no content).

### Example 5: Download multiple web resources to folder
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseWebResource -Name "new_*" -Folder "./webresources"
```

Downloads all web resources matching the pattern to a folder.

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

### -DecodeContent
If set, decodes the content from base64 and returns as byte array. Implies -IncludeContent.

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

### -DisplayName
Display name or pattern to filter by.
Supports wildcards (* and ?)

```yaml
Type: String
Parameter Sets: Query
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Folder
Folder path to save multiple web resource files.
File names are based on the web resource name.

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

### -Id
ID of the web resource to retrieve

```yaml
Type: Guid
Parameter Sets: Id
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IncludeContent
If set, includes the content property in the results. By default, content is excluded for better performance.

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

### -Name
Name or name pattern of the web resource.
Supports wildcards (* and ?)

```yaml
Type: String
Parameter Sets: Query
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path
File path to save the web resource content.
If not specified, content is returned as a property.

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

### -Unmanaged
If set, filters to only unmanaged web resources

```yaml
Type: SwitchParameter
Parameter Sets: Query
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WebResourceType
Web resource type to filter by: 1=HTML, 2=CSS, 3=JS, 4=XML, 5=PNG, 6=JPG, 7=GIF, 8=XAP, 9=XSL, 10=ICO, 11=SVG, 12=RESX

```yaml
Type: WebResourceType
Parameter Sets: Query
Aliases:
Accepted values: HTML, CSS, JavaScript, XML, PNG, JPG, GIF, XAP, XSL, ICO, SVG, RESX

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

### System.Management.Automation.PSObject
## NOTES

## RELATED LINKS
