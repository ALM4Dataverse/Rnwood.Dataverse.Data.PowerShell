---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseCanvasApp

## SYNOPSIS
Retrieves Canvas apps from a Dataverse environment.

## SYNTAX

### Id
```
Get-DataverseCanvasApp -Id <Guid> [-IncludeDocument] [-DocumentPath <String>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Query
```
Get-DataverseCanvasApp [[-Name] <String>] [-DisplayName <String>] [-Unmanaged] [-IncludeDocument]
 [-DocumentPath <String>] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
Retrieves Canvas apps from a Dataverse environment. You can retrieve apps by ID, name pattern, or display name pattern. By default, the document content (.msapp file) is excluded for performance reasons.

## EXAMPLES

### Example 1: Get a Canvas app by ID
```powershell
PS C:\> Get-DataverseCanvasApp -Id "12345678-1234-1234-1234-123456789012"
```

Retrieves a specific Canvas app by its ID using the default connection.

### Example 2: Get all Canvas apps by name pattern
```powershell
PS C:\> Get-DataverseCanvasApp -Name "MyApp*"
```

Retrieves all Canvas apps whose name starts with "MyApp".

### Example 3: Get unmanaged Canvas apps
```powershell
PS C:\> Get-DataverseCanvasApp -Unmanaged
```

Retrieves all unmanaged Canvas apps in the environment.

### Example 4: Get a Canvas app with document content
```powershell
PS C:\> $app = Get-DataverseCanvasApp -Name "MyApp" -IncludeDocument
PS C:\> $app.document  # Contains base64-encoded .msapp file bytes
```

Retrieves a Canvas app and includes the document content (.msapp file).

### Example 5: Save Canvas app document to file
```powershell
PS C:\> Get-DataverseCanvasApp -Name "MyApp" -DocumentPath "MyApp.msapp"
```

Retrieves a Canvas app and saves the .msapp file to the specified path.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet, or string specifying Dataverse organization URL (e.g.
http://server.com/MyOrg/).
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

### -DocumentPath
Path to save the .msapp document file to. Implies -IncludeDocument.

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
ID of the Canvas app to retrieve

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

### -IncludeDocument
If set, includes the document content (.msapp file bytes) in the results

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
Name or name pattern of the Canvas app.
Supports wildcards (* and ?)

```yaml
Type: String
Parameter Sets: Query
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

### -Unmanaged
If set, filters to only unmanaged Canvas apps

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES

## RELATED LINKS
