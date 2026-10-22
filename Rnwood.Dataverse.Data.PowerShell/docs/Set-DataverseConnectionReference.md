---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Set-DataverseConnectionReference

## SYNOPSIS
Creates or updates connection reference values in Dataverse.

## SYNTAX

### Single (Default)
```
Set-DataverseConnectionReference [-ConnectionReferenceLogicalName] <String> [-ConnectionId] <String>
 [[-ConnectorId] <String>] [-DisplayName <String>] [-Description <String>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### Multiple
```
Set-DataverseConnectionReference -ConnectionReferences <Hashtable> [-SolutionUniqueName <String>]
 [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Creates or updates connection reference values in Dataverse. Can set a single connection reference or multiple connection references at once. 

The single parameter set will create a connection reference if it does not exist, or update the existing connection reference.

The multiple parameter set only updates existing connection references if they are found. The keys in the hastable are the logical names of the connection references to update, and the values are the connection IDs to set.
When using the multiple parameter set, connector names can be used as keys for fallback matching. If a key does not match any connection reference logical name, the cmdlet will query all existing connection references to get their connector IDs and check if the key matches a connector name. All connection references using that connector will be mapped to the specified connection ID.
Specify the -SolutionUniqueName parameter to limit the search for connection references to those included in a specific solution.

## EXAMPLES

### Example 1: Create a new connection reference
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseConnectionReference -ConnectionReferenceLogicalName "new_sharedconnectionref" -ConnectionId "12345678-1234-1234-1234-123456789012" -ConnectorId "98765432-4321-4321-4321-210987654321" -DisplayName "Shared SharePoint Connection"
```

Creates a new connection reference with the specified logical name, connection ID, connector ID, and display name.

### Example 2: Update an existing connection reference
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseConnectionReference -ConnectionReferenceLogicalName "new_sharedconnectionref" -ConnectionId "87654321-4321-4321-4321-210987654321"
```

Updates the connection ID of an existing connection reference. ConnectorId is not required for updates.

### Example 3: Create connection reference with description
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseConnectionReference -ConnectionReferenceLogicalName "new_sqlconnection" -ConnectionId "12345678-1234-1234-1234-123456789012" -ConnectorId "98765432-4321-4321-4321-210987654321" -DisplayName "Production SQL Database" -Description "Connection to the production SQL database server"
```

Creates a connection reference with display name and description.

### Example 4: Set multiple connection references
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseConnectionReference -ConnectionReferences @{
    'new_sharepoint' = '12345678-1234-1234-1234-123456789012'
    'new_sql' = '87654321-4321-4321-4321-210987654321'
}
```

Sets multiple connection references at once using a hashtable.

### Example 5: Set connection references using connector name fallback
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Set-DataverseConnectionReference -ConnectionReferences @{
    # All SharePoint connection references will use this connection
    'shared_sharepointonline' = '12345678-1234-1234-1234-123456789012'
    # All SQL connection references will use this connection  
    'shared_sql' = '87654321-4321-4321-4321-210987654321'
    # Override for a specific connection reference
    'new_sharepoint_special' = '11111111-1111-1111-1111-111111111111'
}
```

Sets connection references using connector names as fallback. All connection references using the SharePoint connector will be mapped to the first connection ID, except for 'new_sharepoint_special' which has a specific override. All SQL connection references will use the second connection ID.

### Example 11: Set connection references with stored connection IDs
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $connectionIds = @{
    'new_dataverse' = (Get-DataverseRecord -TableName connection -FilterValues @{ name = 'Production Dataverse' }).connectionid
    'new_sharepoint' = (Get-DataverseRecord -TableName connection -FilterValues @{ name = 'Production SharePoint' }).connectionid
}
PS C:\> Set-DataverseConnectionReference -ConnectionReferences $connectionIds
```

Retrieves connection IDs by name and sets them for the connection references.

### Example 11: Use with solution import workflow
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Set connection references before importing solution
PS C:\> Set-DataverseConnectionReference -ConnectionReferences @{
    'new_sharepoint' = '12345678-1234-1234-1234-123456789012'
    'new_sql' = '87654321-4321-4321-4321-210987654321'
}
PS C:\> Import-DataverseSolution -InFile "solution.zip"
```

Sets connection references before importing a solution, ensuring they are configured correctly.

### Example 11: View operation results
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $result = Set-DataverseConnectionReference -ConnectionReferenceLogicalName "new_sharedconnectionref" -ConnectionId "12345678-1234-1234-1234-123456789abc"
PS C:\> Write-Host "Operation: $($result.Operation)"
PS C:\> if ($result.PreviousConnectionId) {
    Write-Host "Changed from $($result.PreviousConnectionId) to $($result.ConnectionId)"
}
```

Sets a connection reference and displays the operation type and connection ID changes.

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

### -ConnectionId
Connection ID (GUID) to set for the connection reference (for single parameter set).

```yaml
Type: String
Parameter Sets: Single
Aliases:

Required: True
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ConnectionReferenceLogicalName
Logical name of the connection reference to create or update (for single parameter set).

```yaml
Type: String
Parameter Sets: Single
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ConnectionReferences
Hashtable of connection reference logical names or connector names to connection IDs. Used to set multiple connection references at once.

Keys can be either:
- Specific connection reference logical names (e.g., 'new_sharepoint_conn1')
- Connector names for fallback matching (e.g., 'shared_sharepointonline')

The connector name is the value after the last '/' in the full connector ID path. For example, if the full connector ID is '/providers/Microsoft.PowerApps/apis/shared_sharepointonline', the connector name is 'shared_sharepointonline'.

When a hashtable key matches a connection reference logical name, it is used directly. If no direct match is found, the cmdlet queries all existing connection references to get their connector IDs and checks if the key matches a connector name. All connection references using that connector will be mapped to the specified connection ID.

Logical name matches take precedence over connector name matches, allowing you to override the connector-level default for specific connection references.

Example using logical names:


@{
    'new_sharepoint_conn1' = '12345678-1234-1234-1234-123456789012'
    'new_sharepoint_conn2' = '87654321-4321-4321-4321-210987654321'
}

Example using connector name fallback:


@{
    # All SharePoint connection references will use this connection
    'shared_sharepointonline' = '12345678-1234-1234-1234-123456789012'
    # All SQL connection references will use this connection
    'shared_sql' = '87654321-4321-4321-4321-210987654321'
}

Example mixing both approaches:


@{
    # Default for all SharePoint connection references
    'shared_sharepointonline' = '12345678-1234-1234-1234-123456789012'
    # Override for a specific SharePoint connection reference
    'new_sharepoint_special' = '11111111-1111-1111-1111-111111111111'
}

```yaml
Type: Hashtable
Parameter Sets: Multiple
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ConnectorId
Connector ID (GUID) that defines the type of connection (for single parameter set). This identifies the connector/API that the connection reference is for. Required when creating new connection references, cannot be changed for existing ones.

```yaml
Type: String
Parameter Sets: Single
Aliases:

Required: False
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Description
Description for the connection reference (for single parameter set only).

```yaml
Type: String
Parameter Sets: Single
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DisplayName
Display name for the connection reference (for single parameter set only). If not specified when creating, defaults to the logical name.

```yaml
Type: String
Parameter Sets: Single
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

### -SolutionUniqueName
Solution unique name to filter connection references by. When specified, only connection references that are components of this solution will be processed.

```yaml
Type: String
Parameter Sets: Multiple
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

### None
## OUTPUTS

### System.Object
## NOTES
- The single parameter set will create a connection reference if it does not exist, or update it if it does exist.
- The multiple parameter set only updates existing connection references and will return an error if any specified connection reference does not exist.
- The logical name cannot be changed once a connection reference is created.
- Connection IDs must be valid GUIDs that correspond to connection records in Dataverse.
- ConnectorId is required when creating new connection references but cannot be changed for existing ones.
- Display name and description can only be set when using the single parameter set.
- This cmdlet follows the same conventions as the Import-DataverseSolution cmdlet's ConnectionReferences parameter.

## RELATED LINKS

[Import-DataverseSolution](Import-DataverseSolution.md)
[Get-DataverseConnectionReference](Get-DataverseConnectionReference.md)
[Remove-DataverseConnectionReference](Remove-DataverseConnectionReference.md)
[Set-DataverseEnvironmentVariableDefinition](Set-DataverseEnvironmentVariableDefinition.md)
[Set-DataverseEnvironmentVariableValue](Set-DataverseEnvironmentVariableValue.md)
