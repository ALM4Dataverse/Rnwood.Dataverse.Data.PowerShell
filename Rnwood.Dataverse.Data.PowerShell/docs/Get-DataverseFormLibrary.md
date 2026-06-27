---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseFormLibrary

## SYNOPSIS
Retrieves script libraries from a Dataverse form.

## SYNTAX

```
Get-DataverseFormLibrary -FormId <Guid> [-LibraryName <String>] [-LibraryUniqueId <Guid>] [-Published]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The Get-DataverseFormLibrary cmdlet retrieves JavaScript libraries (web resources) that are referenced by a Dataverse form. Libraries can be filtered by name or unique ID. Form libraries are used to provide JavaScript code for form events and business logic.

## EXAMPLES

### Example 1: Get all libraries from a form
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $libraries = Get-DataverseFormLibrary -FormId $formId
PS C:\> $libraries | Format-Table Name, LibraryUniqueId

Name                           LibraryUniqueId
----                           ---------------
new_/scripts/main.js           a1b2c3d4-e5f6-4789-abcd-ef0123456789
new_/scripts/validation.js     b2c3d4e5-f6a7-4890-bcde-f01234567890
```

Retrieves all script libraries from a form.

### Example 2: Get a specific library by name
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseFormLibrary -FormId $formId -LibraryName "new_/scripts/main.js"
```

Retrieves a specific library by its web resource name.

### Example 3: Get a library by unique ID
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $uniqueId = 'a1b2c3d4-e5f6-4789-abcd-ef0123456789'
PS C:\> Get-DataverseFormLibrary -FormId $formId -LibraryUniqueId $uniqueId
```

Retrieves a library by its unique identifier.

### Example 4: List all libraries across multiple forms
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $forms = Get-DataverseForm -Entity 'contact'
PS C:\> foreach ($form in $forms) {
PS C:\>     Write-Host "Form: $($form.Name)"
PS C:\>     Get-DataverseFormLibrary -FormId $form.FormId | 
PS C:\>         Format-Table Name -AutoSize
PS C:\> }
```

Retrieves all libraries from all forms for an entity.

### Example 5: Get libraries from published form only
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseFormLibrary -FormId $formId -Published
```

Retrieves libraries from the published version of the form only. By default, the cmdlet retrieves from the unpublished (draft) version which includes all recent changes.

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
The ID of the form to retrieve libraries from.

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
Optional filter to retrieve a specific library by web resource name.

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

### -LibraryUniqueId
Optional filter to retrieve a specific library by its unique ID.

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

### -Published
Retrieve only the published version of the form (default is unpublished)

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Guid
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES
Form libraries must reference valid web resources of type Script (JScript). The web resource should exist in the environment before being added to a form.

## RELATED LINKS

[Set-DataverseFormLibrary](Set-DataverseFormLibrary.md)
[Remove-DataverseFormLibrary](Remove-DataverseFormLibrary.md)
[Get-DataverseFormEventHandler](Get-DataverseFormEventHandler.md)
