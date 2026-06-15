---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseSitemapEntry

## SYNOPSIS
Removes an entry (Area, Group, or SubArea) from a Dataverse sitemap.

## SYNTAX

### Area
```
Remove-DataverseSitemapEntry [-InputObject <SitemapEntryInfo>] [-Sitemap <SitemapInfo>]
 [[-SitemapUniqueName] <String>] [-SitemapId <Guid>] [-Area] -EntryId <String> [-IfExists]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### Group
```
Remove-DataverseSitemapEntry [-InputObject <SitemapEntryInfo>] [-Sitemap <SitemapInfo>]
 [[-SitemapUniqueName] <String>] [-SitemapId <Guid>] [-Group] -EntryId <String> [-IfExists]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### SubArea
```
Remove-DataverseSitemapEntry [-InputObject <SitemapEntryInfo>] [-Sitemap <SitemapInfo>]
 [[-SitemapUniqueName] <String>] [-SitemapId <Guid>] [-SubArea] -EntryId <String> [-IfExists]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### Privilege
```
Remove-DataverseSitemapEntry [-InputObject <SitemapEntryInfo>] [-Sitemap <SitemapInfo>]
 [[-SitemapUniqueName] <String>] [-SitemapId <Guid>] [-Privilege] -PrivilegeEntity <String>
 -PrivilegeName <String> -ParentSubAreaId <String> [-IfExists] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Removes a specified entry (such as an Area, Group, or SubArea) from a Dataverse sitemap. This cmdlet allows administrators and customizers to programmatically delete sitemap entries, which control the navigation structure in model-driven Power Apps and Dynamics 365 applications. 

You can target entries by their ID, type, and parent relationships, and optionally specify the sitemap by name or ID. Use this cmdlet to automate the cleanup or restructuring of app navigation, especially in ALM (Application Lifecycle Management) or deployment scripts.

The cmdlet supports the -IfExists parameter to suppress errors when removing entries that may not exist, making it useful in idempotent deployment scenarios.

## EXAMPLES

### Example 1: Remove an Area from a sitemap
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseSitemapEntry -SitemapName "Default" -EntryType Area -EntryId "Sales"
```

Removes the Area with ID "Sales" from the sitemap named "Default".

### Example 2: Remove a SubArea using pipeline
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseSitemapEntry -SitemapName "Default" -EntryType SubArea -EntryId "Contacts" | Remove-DataverseSitemapEntry
```

Retrieves a specific SubArea entry and removes it via pipeline.

### Example 3: Remove an entry with -IfExists for idempotent scripts
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseSitemapEntry -SitemapName "Default" -EntryType Group -EntryId "Marketing" -IfExists
```

Removes the Group with ID "Marketing" if it exists, without raising an error if it doesn't exist. Useful in deployment scripts that need to be idempotent.

## PARAMETERS

### -Area
Remove an Area entry.

```yaml
Type: SwitchParameter
Parameter Sets: Area
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
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

### -EntryId
The ID of the entry to remove.

```yaml
Type: String
Parameter Sets: Area, Group, SubArea
Aliases: Id

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Group
Remove a Group entry.

```yaml
Type: SwitchParameter
Parameter Sets: Group
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IfExists
If specified, the cmdlet will not raise an error if the entry does not exist.

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

### -InputObject
Entry object from Get-DataverseSitemapEntry.

```yaml
Type: SitemapEntryInfo
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -ParentSubAreaId
The parent SubArea ID containing the privilege.

```yaml
Type: String
Parameter Sets: Privilege
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Privilege
Remove a Privilege entry.

```yaml
Type: SwitchParameter
Parameter Sets: Privilege
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PrivilegeEntity
The entity name for the privilege to remove.

```yaml
Type: String
Parameter Sets: Privilege
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -PrivilegeName
The privilege name to remove.

```yaml
Type: String
Parameter Sets: Privilege
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -ProgressAction
Determines how PowerShell responds to progress updates generated by the cmdlet.

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

### -Sitemap
Sitemap object from Get-DataverseSitemap.

```yaml
Type: SitemapInfo
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -SitemapId
The ID of the sitemap containing the entry.

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

### -SitemapUniqueName
The unique name of the sitemap containing the entry.

```yaml
Type: String
Parameter Sets: (All)
Aliases: Name

Required: False
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -SubArea
Remove a SubArea entry.

```yaml
Type: SwitchParameter
Parameter Sets: SubArea
Aliases:

Required: True
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

### Rnwood.Dataverse.Data.PowerShell.Commands.SitemapEntryInfo
### Rnwood.Dataverse.Data.PowerShell.Commands.SitemapInfo
### System.String
### System.Nullable`1[[System.Guid, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
