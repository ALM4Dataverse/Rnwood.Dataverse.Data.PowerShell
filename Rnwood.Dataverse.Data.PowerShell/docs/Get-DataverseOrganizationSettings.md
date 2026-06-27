---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseOrganizationSettings

## SYNOPSIS
Gets organization settings from the single organization record in a Dataverse environment.

## SYNTAX

```
Get-DataverseOrganizationSettings [-OrgDbOrgSettings] [-IncludeRawXml] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The Get-DataverseOrganizationSettings cmdlet retrieves settings from the single organization record in a Dataverse environment.

When -OrgDbOrgSettings is NOT specified: Returns all organization table columns as PSObject properties.
When -OrgDbOrgSettings IS specified: Returns only the parsed OrgDbOrgSettings as a PSObject with settings as properties.

## EXAMPLES

### Example 1: Get organization record
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $connection = Get-DataverseConnection -Url "https://contoso.crm.dynamics.com" -Interactive
PS C:\> $org = Get-DataverseOrganizationSettings
PS C:\> $org.name
Contoso Corporation
```

Gets the full organization record and displays the organization name.

### Example 2: Get only OrgDbOrgSettings
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $settings = Get-DataverseOrganizationSettings -OrgDbOrgSettings
PS C:\> $settings.MaxUploadFileSize
5242880
PS C:\> $settings.EnableBingMapsIntegration
True
```

Gets only the OrgDbOrgSettings as a PSObject with parsed settings as properties.

### Example 3: Include raw XML in organization record
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $org = Get-DataverseOrganizationSettings -IncludeRawXml
PS C:\> $org.orgdborgsettings
<OrgSettings>
  <MaxUploadFileSize>5242880</MaxUploadFileSize>
  ...
</OrgSettings>
```

Gets the organization record and includes the raw OrgDbOrgSettings XML. Note: -IncludeRawXml is ignored when -OrgDbOrgSettings is used.

## PARAMETERS

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

### -IncludeRawXml
If specified, includes the raw OrgDbOrgSettings XML string in the output when getting the full organization record. This parameter is ignored when -OrgDbOrgSettings is used.

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

### -OrgDbOrgSettings
If specified, returns only the parsed OrgDbOrgSettings as a PSObject instead of the full organization record. Settings are returned as properties with parsed types (bool, int, or string).

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
If specified, returns only OrgDbOrgSettings instead of the full organization record

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

### System.Management.Automation.PSObject
## NOTES

## RELATED LINKS

[Set-DataverseOrganizationSettings](Set-DataverseOrganizationSettings.md)
[Get-DataverseConnection](Get-DataverseConnection.md)
