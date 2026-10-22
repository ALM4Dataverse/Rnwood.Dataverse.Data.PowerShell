---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseSolution

## SYNOPSIS
Creates or updates a solution in Dataverse. Allows setting friendly name, description, version, and publisher.

## SYNTAX

```
Set-DataverseSolution [-UniqueName] <String> [-Name <String>] [-Description <String>] [-Version <String>]
 [-PublisherUniqueName <String>] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf]
 [-Confirm] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet creates a new solution if the specified unique name does not exist, or updates an existing solution. It supports updating the friendly name, description, version (unmanaged only), and publisher (unmanaged only). When creating a solution, the publisher is required.

## EXAMPLES

### Example1: Update solution description
```powershell
Set-DataverseSolution -UniqueName "MySolution" -Description "Updated solution description"
```

Updates the description of an unmanaged solution.

### Example2: Update solution name and version
```powershell
Set-DataverseSolution -UniqueName "MySolution" -Name "My Updated Solution" -Version "1.1.0.0"
```

Updates both the friendly name and version of an unmanaged solution.

### Example3: Update all updatable properties
```powershell
Set-DataverseSolution -UniqueName "MySolution" -Name "Updated Name" -Description "New description" -Version "2.0.0.0"
```

Updates the name, description, and version in one operation.

### Example4: Attempt to update managed solution (shows warning)
```powershell
Set-DataverseSolution -UniqueName "ManagedSolution" -Name "New Name" -Version "2.0.0.0"
WARNING: Solution is managed. Only the description can be updated for managed solutions.
WARNING: Cannot update name of managed solution. Skipping name update.
WARNING: Cannot update version of managed solution. Skipping version update.
WARNING: No updates to apply. Please specify at least one property to update (Name, Description, or Version).
```

Attempts to update a managed solution, but only description updates are allowed.

### Example5: Create a new solution
```powershell
Set-DataverseSolution -UniqueName "NewSolution" -Name "My New Solution" -Description "Description" -Version "1.0.0.0" -PublisherUniqueName "defaultpublisher"
```

Creates a new solution with the specified properties.

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

### -Description
The new description for the solution. Can be updated for both managed and unmanaged solutions.

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

### -Name
The new friendly name for the solution. Only applicable to unmanaged solutions.

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

### -PublisherUniqueName
The unique name of the publisher. This is required when creating a new solution.

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

### -UniqueName
The unique name of the solution to create or update.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName)
Accept wildcard characters: False
```

### -Version
The new version for the solution (e.g., '1.0.0.0'). Only applicable to unmanaged solutions.

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

### System.String
## OUTPUTS

### System.Object
## NOTES

- Managed solutions can only have their description updated.
- For unmanaged solutions, you can update the name, description, and version.
- Version must be in the format 'major.minor.build.revision' (e.g., '1.0.0.0').
- At least one property (Name, Description, or Version) must be specified for the update to proceed.
- When creating a solution, PublisherUniqueName is required.

## RELATED LINKS

[Get-DataverseSolution](Get-DataverseSolution.md)

[Remove-DataverseSolution](Remove-DataverseSolution.md)

[Import-DataverseSolution](Import-DataverseSolution.md)
