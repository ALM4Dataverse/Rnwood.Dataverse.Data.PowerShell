---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Remove-DataverseFormLibrary

## SYNOPSIS
Removes a script library from a Dataverse form.

## SYNTAX

### ByName
```
Remove-DataverseFormLibrary -FormId <Guid> -LibraryName <String> [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### ByUniqueId
```
Remove-DataverseFormLibrary -FormId <Guid> -LibraryUniqueId <Guid> [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
The Remove-DataverseFormLibrary cmdlet removes a JavaScript library reference from a Dataverse form. The library can be identified by its name or unique ID. After removal, the entity is published unless is specified. Note that this only removes the library reference from the form; it does not delete the web resource itself.

## EXAMPLES

### Example 1: Remove a library by name
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseFormLibrary -FormId $formId -LibraryName "new_/scripts/oldcode.js"
```

Removes the specified library from the form and publishes the entity.

### Example 2: Remove a library by unique ID
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $libraryId = 'a1b2c3d4-e5f6-4789-abcd-ef0123456789'
PS C:\> Remove-DataverseFormLibrary -FormId $formId -LibraryUniqueId $libraryId
```

Removes a library by its unique identifier.

### Example 3: Remove without publishing
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Remove-DataverseFormLibrary -FormId $formId -LibraryName "new_/scripts/temp.js"
```

Removes the library without publishing. Useful when making multiple changes.

### Example 4: Remove all unused libraries
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $libraries = Get-DataverseFormLibrary -FormId $formId
PS C:\> $unusedLibs = $libraries | Where-Object { $_.Name -like "*test*" }
PS C:\> foreach ($lib in $unusedLibs) {
PS C:\>     Remove-DataverseFormLibrary -FormId $formId -LibraryUniqueId $lib.LibraryUniqueId -Confirm:$false
PS C:\> }
```

Removes multiple libraries matching a pattern.

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
The ID of the form to remove the library from.

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
The name of the library to remove.

```yaml
Type: String
Parameter Sets: ByName
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -LibraryUniqueId
The unique ID of the library to remove.

```yaml
Type: Guid
Parameter Sets: ByUniqueId
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

### -Confirm
Prompts for confirmation before removing the library.

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

### System.Object
## NOTES

## RELATED LINKS

[Get-DataverseFormLibrary](Get-DataverseFormLibrary.md)
[Set-DataverseFormLibrary](Set-DataverseFormLibrary.md)
