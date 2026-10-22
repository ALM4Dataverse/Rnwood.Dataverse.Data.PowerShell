---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseConnectionReference

## SYNOPSIS
Gets connection references from Dataverse.

## SYNTAX

```
Get-DataverseConnectionReference [[-ConnectionReferenceLogicalName] <String>] [-DisplayName <String>]
 [-ConnectorId <String>] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
Retrieves connection references from the Dataverse environment. Connection references
define connections to external services that can be used by Power Automate flows and
other Dataverse components.

You can filter connection references by logical name, display name, or connector ID.
Wildcard patterns (* and ?) are supported for all filter parameters.

## EXAMPLES

### Example 1: Get all connection references
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseConnectionReference
```

Retrieves all connection references from the Dataverse environment.

### Example 2: Get a specific connection reference
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseConnectionReference -ConnectionReferenceLogicalName "new_sharepoint"
```

Retrieves a specific connection reference by its logical name.

### Example 3: Filter by display name
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseConnectionReference -DisplayName "Production*"
```

Retrieves connection references whose display name starts with "Production".

### Example 4: Filter by connector ID
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Get-DataverseConnectionReference -ConnectorId "98765432-4321-4321-4321-210987654321"
```

Retrieves connection references that use a specific connector.

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

### -ConnectionReferenceLogicalName
Logical name of the connection reference to retrieve.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```

### -ConnectorId
Connector ID filter for connection references (supports wildcards).

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
Display name filter for connection references (supports wildcards).

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.String
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES

This cmdlet automatically handles paging when retrieving large numbers of connection references, so you don't need to worry about result size limits.

Wildcard patterns (* and ?) are supported for all filter parameters. The * matches zero or more characters, and ? matches exactly one character. Wildcard patterns are converted to SQL LIKE patterns for the query.

Connection references define connections to external services that can be used by Power Automate flows and other Dataverse components. Each connection reference has a logical name, display name, connector ID, and may be associated with a specific connection.

## RELATED LINKS

[Get-DataverseConnection](Get-DataverseConnection.md)

[Set-DataverseConnectionReference](Set-DataverseConnectionReference.md)

[Remove-DataverseConnectionReference](Remove-DataverseConnectionReference.md)

[Set-DataverseConnectionAsDefault](Set-DataverseConnectionAsDefault.md)
