---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseFormLibrary

## SYNOPSIS
Adds or updates a script library in a Dataverse form.

## SYNTAX

```
Set-DataverseFormLibrary -FormId <Guid> -LibraryName <String> [-LibraryUniqueId <Guid>]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Set-DataverseFormLibrary cmdlet adds a new JavaScript library (web resource) to a form, or updates an existing library reference. The cmdlet validates that the web resource exists (including unpublished versions) before adding it. After adding the library, the entity is published unless is specified.

## EXAMPLES

### Example 1: Add a new library to a form
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormLibrary -FormId $formId -LibraryName "new_/scripts/mycode.js"
```

Adds a new JavaScript library to the form and publishes the entity.

### Example 2: Add a library with a specific unique ID
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $libraryId = [Guid]::NewGuid()
PS C:\> Set-DataverseFormLibrary -FormId $formId -LibraryName "new_/scripts/mycode.js" -LibraryUniqueId $libraryId
```

Adds a library with a specific unique identifier.

### Example 3: Add a library without publishing
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormLibrary -FormId $formId -LibraryName "new_/scripts/mycode.js"
```

Adds the library but does not publish the entity. Useful when making multiple changes.

### Example 4: Update an existing library's unique ID
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormLibrary -FormId $formId -LibraryName "new_/scripts/existing.js" -LibraryUniqueId $newId
```

Updates the unique ID of an existing library reference.

### Example 5: Add multiple libraries
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $libraries = @(
PS C:\>     "new_/scripts/main.js",
PS C:\>     "new_/scripts/validation.js",
PS C:\>     "new_/scripts/utils.js"
PS C:\> )
PS C:\> foreach ($lib in $libraries) {
PS C:\>     Set-DataverseFormLibrary -FormId $formId -LibraryName $lib
PS C:\> }
PS C:\> # Note: Use Publish-DataverseAllCustomizations or restart the Dataverse environment to publish changes
```

Adds multiple libraries efficiently by skipping publish and publishing once at the end.

### Example 6: Using WhatIf to preview changes
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseFormLibrary -FormId $formId -LibraryName "new_/scripts/mycode.js" -WhatIf
```

Previews the library addition without making changes.

## PARAMETERS

### -Connection
The Dataverse connection to use.

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

### -FormId
The ID of the form to add the library to.

```yaml
Type: Guid
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -LibraryName
The name of the web resource library to add (e.g., 'new_/scripts/main.js').

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

### -LibraryUniqueId
Optional unique ID for the library. If not specified, a new GUID will be generated.

```yaml
Type: Guid
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

### -Confirm
Prompts for confirmation before adding or updating the library.

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
Shows what would happen if the cmdlet runs without actually executing it.

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
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES
- The web resource must exist in the environment (published or unpublished) before adding it to a form.
- If the library already exists on the form, it will be updated with the new unique ID if provided.
- Use when adding multiple libraries to avoid multiple publish operations.
- The entity is automatically published after the change unless is used.

## RELATED LINKS

[Get-DataverseFormLibrary](Get-DataverseFormLibrary.md)
[Remove-DataverseFormLibrary](Remove-DataverseFormLibrary.md)
[Set-DataverseFormEventHandler](Set-DataverseFormEventHandler.md)
