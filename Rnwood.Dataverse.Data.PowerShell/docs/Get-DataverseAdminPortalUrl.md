---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseAdminPortalUrl

## SYNOPSIS
Generates a URL to open the Power Platform Admin Center for the current environment.

## SYNTAX

```
Get-DataverseAdminPortalUrl [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION

This cmdlet generates a URL that opens the Power Platform Admin Center for the Dataverse environment associated with the current connection.

The Admin Center is where administrators can:
- Manage environment settings
- View environment details
- Configure environment resources
- Monitor environment health
- Manage environment access and security

## EXAMPLES

### Example 1: Get URL for environment in Admin Center
```powershell
PS C:\> Get-DataverseAdminPortalUrl -Connection $c
```

Returns a URL to open the Admin Center for the connected environment.

### Example 2: Open Admin Center directly in browser
```powershell
PS C:\> Start-Process (Get-DataverseAdminPortalUrl -Connection $c)
```

Opens the Admin Center for the environment directly in the default web browser.

### Example 3: Get Admin URLs for multiple connections
```powershell
PS C:\> $connections | ForEach-Object { Get-DataverseAdminPortalUrl -Connection $_ }
```

Generates Admin Center URLs for multiple connections.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet, or string specifying Dataverse organization URL (e.g. http://server.com/MyOrg/). If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### System.String
## NOTES

The Admin Center URL format is: https://admin.powerplatform.microsoft.com/environments/{environmentId}/hub

Administrative permissions may be required to access the Admin Center.

## RELATED LINKS

[Power Platform Admin Center](https://admin.powerplatform.microsoft.com)
