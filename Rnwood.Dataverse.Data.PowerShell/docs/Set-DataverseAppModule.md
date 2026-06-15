---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseAppModule

## SYNOPSIS
Creates or updates an app module (model-driven app) in Dataverse.

## SYNTAX

```
Set-DataverseAppModule [-Id <Guid>] [-UniqueName <String>] [-Name <String>] [-Description <String>]
 [-Url <String>] [-WebResourceId <Guid>] [-FormFactor <Int32>] [-ClientType <Int32>]
 [-NavigationType <NavigationType>] [-IsFeatured <Boolean>] [-NoUpdate] [-NoCreate] [-PassThru] [-Publish]
 [-Validate] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm]
 [<CommonParameters>]
```

## DESCRIPTION
The Set-DataverseAppModule cmdlet creates or updates model-driven app definitions in Dataverse. If an app module with the specified ID or UniqueName exists, it will be updated; otherwise, a new app module is created.

The cmdlet supports both creation and update scenarios with full control through switches like NoUpdate and NoCreate to prevent unintended modifications.

## EXAMPLES

### Example 1: Create a new app module
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAppModule -PassThru `
    -UniqueName "myapp_unique" `
    -Name "My Custom App" `
    -Description "A custom model-driven application"
```

Creates a new app module with the specified unique name, display name, and description.

### Example 2: Update an existing app module by ID
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAppModule `
    -Id "12345678-1234-1234-1234-123456789012" `
    -Name "Updated App Name" `
    -Description "Updated description"
```

Updates the name and description of an existing app module.

### Example 3: Update an app module by UniqueName
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAppModule `
    -UniqueName "myapp_unique" `
    -Description "New description"
```

Updates an app module identified by its unique name.

### Example 4: Create with specific properties
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $webResourceId = (Get-DataverseRecord -TableName webresource -FilterValues @{name="app_icon.svg"}).webresourceid
PS C:\> Set-DataverseAppModule -PassThru `
    -UniqueName "sales_app" `
    -Name "Sales Application" `
    -Url "/main.aspx?app=sales" `
    -WebResourceId $webResourceId `
    -FormFactor 1 `
    -ClientType 1
```

Creates a new app module with icon, URL, and form factor settings.

### Example 5: Create with navigation type and featured setting
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAppModule -PassThru `
    -UniqueName "featured_app" `
    -Name "Featured Application" `
    -NavigationType MultiSession `
    -IsFeatured $true
```

Creates a new app module with multi-session navigation and marks it as featured.

### Example 6: Create and publish an app module
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAppModule -PassThru `
    -UniqueName "ready_app" `
    -Name "Ready Application" `
    -Validate `
    -Publish
```

Creates a new app module, validates it, and publishes it immediately.

### Example 7: Upsert pattern with NoCreate
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAppModule `
    -UniqueName "existing_app" `
    -Name "Updated Name" `
    -NoCreate
```

Updates the app if it exists, but does nothing if it doesn't exist (prevents accidental creation).

### Example 8: Safe update with NoUpdate
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseAppModule `
    -Id $appId `
    -Name "New Name" `
    -NoUpdate
```

Returns the existing app ID without making any changes (prevents accidental updates).

## PARAMETERS

### -ClientType
Client type for the app module

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

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
Description of the app module

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -FormFactor
Form factor for the app module (1=Main, 2=Quick, 3=Preview, 4=Dashboard)

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Id
ID of the app module to update.
If not specified or if the app module doesn't exist, a new app module is created.

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

### -IsFeatured
Whether the app module is featured

```yaml
Type: Boolean
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Name
Display name of the app module

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -NavigationType
Navigation type for the app module (SingleSession or MultiSession)

```yaml
Type: NavigationType
Parameter Sets: (All)
Aliases:
Accepted values: SingleSession, MultiSession

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -NoCreate
If specified, then no app module will be created even if no existing app module matching the ID or UniqueName is found

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
If specified, existing app modules matching the ID or UniqueName will not be updated

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
If specified, returns the ID of the created or updated app module

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
If specified, publishes the app module after creating or updating

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

### -UniqueName
Unique name of the app module.
Required when creating a new app module.
Can also be used to identify an existing app module for update.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Url
URL of the app module

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Validate
If specified, validates the app module before publishing

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

### -WebResourceId
Web resource ID for the app module icon. When creating, if not specified the default 953b9fac-1e5e-e611-80d6-00155ded156f is used.

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
### System.Nullable`1[[System.Guid, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]
### System.Nullable`1[[System.Int32, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]
### System.Nullable`1[[Rnwood.Dataverse.Data.PowerShell.Commands.NavigationType, Rnwood.Dataverse.Data.PowerShell.Cmdlets, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]
### System.Nullable`1[[System.Boolean, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]
## OUTPUTS

### System.Guid
## NOTES

**Required Parameters:**
- When creating: UniqueName is required
- When updating: Either Id or UniqueName must identify the existing app

**Upsert Behavior:**
- If ID is provided and exists: updates that record
- If ID not found but UniqueName exists: updates the record with that UniqueName
- If neither ID nor UniqueName found: creates new record

**Control Flags:**
- NoUpdate: Prevents updating existing records (returns ID if found, does nothing if not found)
- NoCreate: Prevents creating new records (updates if found, does nothing if not found)
- PassThru: Returns the ID of the created or updated record

**Publishing and Validation:**
- Validate: Validates the app module configuration and reports any issues
- Publish: Publishes the app module making it available to users
- Both can be used together to validate before publishing

**Navigation Types:**
- SingleSession: Traditional single-session navigation (value = 0)
- MultiSession: Multi-session navigation allowing multiple browser tabs (value = 1)

**Best Practices:**
- Use UniqueName as the stable identifier for apps across environments
- Include Name parameter when creating apps (defaults to UniqueName if not provided)
- Use NoCreate for update-only scenarios
- Use WhatIf to preview changes before execution
- Validate apps before publishing to catch configuration issues early
- Use IsFeatured to highlight important apps in the app launcher

## RELATED LINKS

[Get-DataverseAppModule](Get-DataverseAppModule.md)

[Remove-DataverseAppModule](Remove-DataverseAppModule.md)

[Set-DataverseAppModuleComponent](Set-DataverseAppModuleComponent.md)
