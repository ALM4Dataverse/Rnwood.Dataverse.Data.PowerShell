---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseCanvasApp

## SYNOPSIS
Creates or updates a Canvas app in a Dataverse environment.

## SYNTAX

```
Set-DataverseCanvasApp [[-Id] <Guid>] [[-Name] <String>] [-DisplayName <String>] [-Description <String>]
 -MsAppPath <String> [-PublisherPrefix <String>] [-PassThru] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Creates or updates a Canvas app in a Dataverse environment (upsert operation). Automatically determines whether to create or update based on ID or Name. Upload a .msapp file that has been modified locally using the MsApp cmdlets. Uses the default connection if -Connection is not specified.

## EXAMPLES

### Example 1: Create a new Canvas app (upsert by Name)
```powershell
PS C:\> Set-DataverseCanvasApp -Name "new_myapp" -DisplayName "My App" -MsAppPath "C:\apps\myapp.msapp"
```

Creates a new Canvas app with the specified name using a local .msapp file. If the app already exists, it will be updated instead.

### Example 2: Update an existing Canvas app (upsert by ID)
```powershell
PS C:\> Set-DataverseCanvasApp -Id "12345678-1234-1234-1234-123456789012" -MsAppPath "C:\apps\updated.msapp"
```

Updates an existing Canvas app with a modified .msapp file. If the ID doesn't exist, creates a new app with that ID.

### Example 3: Workflow - modify .msapp locally then upload
```powershell
PS C:\> # Modify screens/components using MsApp cmdlets
PS C:\> Set-DataverseMsAppScreen -MsAppPath "myapp.msapp" -ScreenName "NewScreen" -YamlContent $yaml
PS C:\> Set-DataverseMsAppComponent -MsAppPath "myapp.msapp" -ComponentName "MyButton" -YamlContent $yaml
PS C:\> # Upload modified app to Dataverse
PS C:\> Set-DataverseCanvasApp -Name "new_myapp" -MsAppPath "myapp.msapp"
```

Complete workflow: modify .msapp locally, then upload to Dataverse. This is much faster than import/export for each change.

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

### -Description
Description for the Canvas app

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

### -DisplayName
Display name for the Canvas app

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
ID of the Canvas app (used as unique key for upsert). If provided and exists, updates the app. If not exists, creates with this ID.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -MsAppPath
Path to a .msapp file to upload as the Canvas app document content. Required.

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

### -Name
Name for the Canvas app (logical name, used as unique key if ID not provided). Used to find existing app for upsert.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
If set, returns the Canvas app ID

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

### -PublisherPrefix
Publisher prefix for the new Canvas app (e.g., 'new')

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

### System.Nullable`1[[System.Guid, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]
## OUTPUTS

### System.Guid
## NOTES

## RELATED LINKS
