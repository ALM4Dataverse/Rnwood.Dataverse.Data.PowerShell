---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Publish-DataverseCustomizations

## SYNOPSIS
Publishes customizations in Dataverse.

## SYNTAX

```
Publish-DataverseCustomizations [[-EntityName] <String>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet publishes customizations in Dataverse. You can publish all customizations or customizations for a specific entity.

Publishing customizations makes them available to users and is required after making changes to entity definitions, forms, views, and other customizations.

## EXAMPLES

### Example 1: Publish all customizations
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Publish-DataverseCustomizations
Customizations published successfully.
```

Publishes all customizations in the environment.

### Example 2: Publish customizations for a specific entity
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Publish-DataverseCustomizations -EntityName "contact"
Customizations published successfully.
```

Publishes only the customizations for the contact entity.

### Example 3: Publish with confirmation
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Publish-DataverseCustomizations -Confirm

Confirm
Are you sure you want to perform this action?
Performing the operation "Publish" on target "All customizations".
[Y] Yes  [A] Yes to All  [N] No  [L] No to All  [S] Suspend  [?] Help (default is "Y"): Y
Customizations published successfully.
```

Publishes customizations with explicit confirmation.

### Example 4: Publish after making schema changes
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Add a new field to contact entity
PS C:\> Invoke-DataverseCreateAttribute -EntityName "contact" -AttributeName "new_customfield" -AttributeType "String"
PS C:\> # Publish the changes
PS C:\> Publish-DataverseCustomizations -EntityName "contact"
```

Adds a new field and publishes the changes to make them available.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet. If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.

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

### -EntityName
The logical name of the entity to publish. If not specified, all customizations are published.

```yaml
Type: String
Parameter Sets: (All)
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

### None
## OUTPUTS

### System.Object
## NOTES

- Publishing can take some time depending on the number of customizations being published.
- This cmdlet requires an active connection to a Dataverse environment.
- Publishing all customizations may impact system performance temporarily in large environments.

## RELATED LINKS

[Import-DataverseSolution](Import-DataverseSolution.md)

[Export-DataverseSolution](Export-DataverseSolution.md)
