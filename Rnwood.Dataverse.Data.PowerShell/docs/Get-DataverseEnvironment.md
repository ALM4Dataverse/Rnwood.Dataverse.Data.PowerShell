---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseEnvironment

## SYNOPSIS
Lists Dataverse environments accessible to the authenticated user.

## SYNTAX

### With access token
```
Get-DataverseEnvironment -AccessToken <ScriptBlock> [-FriendlyName <String>] [-Geo <String>]
 [-OrganizationType <OrganizationType>] [-Timeout <UInt32>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### With connection
```
Get-DataverseEnvironment [-Connection <ServiceClient>] [-FriendlyName <String>] [-Geo <String>]
 [-OrganizationType <OrganizationType>] [-Timeout <UInt32>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
The `Get-DataverseEnvironment` cmdlet discovers and lists Microsoft Dataverse environments that the authenticated user has access to. This cmdlet uses the Global Discovery Service to enumerate available environments across all regions.

The cmdlet supports filtering by friendly name (with wildcards), geographic region, and organization type to help you quickly find specific environments.

The cmdlet can work in two modes:
1. Using an existing connection (default) - extracts the authentication from a connection object
2. Using a custom access token provider - allows you to supply your own authentication logic

## EXAMPLES

### Example 1: List all environments using the default connection
```powershell
Get-DataverseEnvironment
```

This command lists all Dataverse environments accessible using the default connection. You must have previously set a default connection using `Get-DataverseConnection -SetAsDefault`.

### Example 2: List environments using a specific connection
```powershell
$conn = Get-DataverseConnection -Interactive -SetAsDefault
Get-DataverseEnvironment -Connection $conn
```

This command creates a connection with interactive authentication and then lists all accessible environments using that connection.

### Example 3: Filter environments by friendly name with wildcards
```powershell
Get-DataverseEnvironment -FriendlyName "Contoso*"
```

This command lists all environments whose friendly name starts with "Contoso". The FriendlyName parameter supports wildcards (* and ?).

### Example 4: Filter environments by geographic region
```powershell
Get-DataverseEnvironment -Geo "NA"
```

This command lists all environments in the North America (NA) region. Common geo values include NA (North America), EMEA (Europe/Middle East/Africa), APAC (Asia Pacific), and others.

### Example 5: Filter by multiple criteria
```powershell
Get-DataverseEnvironment -FriendlyName "*Production*" -Geo "NA" -OrganizationType Production
```

This command lists all production environments in North America with "Production" in their friendly name.

### Example 6: List environments and select one interactively
```powershell
$envs = Get-DataverseEnvironment
$envs | Format-Table FriendlyName, UniqueName, Geo, @{Name="WebAppUrl"; Expression={$_.Endpoints["WebApplication"]}}

# Connect to a specific environment
$selectedEnv = $envs | Where-Object { $_.UniqueName -eq "myorg" } | Select-Object -First 1
$url = $selectedEnv.Endpoints["WebApplication"]
$conn = Get-DataverseConnection -Interactive -Url $url
```

This example retrieves all environments, displays them in a table, and then connects to a specific environment by its unique name.

### Example 7: List environments using a custom access token provider
```powershell
$tokenProvider = {
    param($url)
    # Custom logic to get an access token for the given URL
    # For example, using Azure CLI:
    $scope = "$url/.default"
    $token = az account get-access-token --resource $url --query accessToken -o tsv
    return $token
}

Get-DataverseEnvironment -AccessToken $tokenProvider
```

This example uses a custom script block to provide access tokens. The script block receives the resource URL as a parameter and must return an access token string.

### Example 8: Export filtered environment list to CSV
```powershell
Get-DataverseEnvironment -Geo "NA" | 
    Select-Object FriendlyName, UniqueName, OrganizationId, UrlName, Geo, OrganizationType,
        @{Name="WebAppUrl"; Expression={$_.Endpoints["WebApplication"]}},
        @{Name="ApiUrl"; Expression={$_.Endpoints["OrganizationService"]}} |
    Export-Csv -Path "na-dataverse-environments.csv" -NoTypeInformation
```

This example exports all North America environment details to a CSV file for documentation or reporting purposes.

### Example 9: Find a specific environment by name
```powershell
$env = Get-DataverseEnvironment -FriendlyName "My Dev Environment"
if ($env) {
    $url = $env.Endpoints["WebApplication"]
    Write-Host "Found environment at: $url"
}
```

This example searches for a specific environment by its exact friendly name.

## PARAMETERS

### -AccessToken
Script block that returns an access token string. The script block receives the resource URL as a parameter and must return a valid access token for that resource.

This parameter is useful when you want to use a custom authentication method or integrate with external token providers.

```yaml
Type: ScriptBlock
Parameter Sets: With access token
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Connection
Connection to use for discovering environments. If not specified, the default connection is used.

The connection must have been created using an authentication method that supports token providers (Interactive, DeviceCode, ClientSecret, ClientCertificate, DefaultAzureCredential, ManagedIdentity, or AccessToken). Connections created with ConnectionString are not supported.

```yaml
Type: ServiceClient
Parameter Sets: With connection
Aliases:

Required: False
Position: Named
Default value: None (uses default connection)
Accept pipeline input: False
Accept wildcard characters: False
```

### -FriendlyName
Filter environments by friendly name. Supports wildcards (* and ?).

Use this parameter to find environments with specific names or name patterns. For example:
- "Contoso Production" - exact match
- "Contoso*" - starts with Contoso
- "*Dev*" - contains Dev
- "Test?" - Test followed by any single character

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: True
```

### -Geo
Filter environments by geographic region.

Common values include:
- NA - North America
- EMEA - Europe, Middle East, and Africa
- APAC - Asia Pacific
- SAM - South America
- CAN - Canada
- EUR - Europe
- FRA - France
- GER - Germany
- IND - India
- JPN - Japan
- OCE - Oceania
- UK - United Kingdom

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

### -OrganizationType
Filter environments by organization type.

Valid values are:
- Production - Production environments
- Sandbox - Sandbox/non-production environments
- Trial - Trial environments
- Developer - Developer environments
- Support - Microsoft support environments

```yaml
Type: OrganizationType
Parameter Sets: (All)
Aliases:
Accepted values: Customer, Monitoring, Support, BackEnd, Secondary, CustomerTest, CustomerFreeTest, CustomerPreview, Placeholder, TestDrive, MsftInvestigation, EmailTrial, Default, Developer, Trial, Teams, Platform

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

### -Timeout
Timeout for the discovery operation in seconds. Defaults to 5 minutes (300 seconds).

```yaml
Type: UInt32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 300
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### Microsoft.Xrm.Sdk.Discovery.OrganizationDetail
## NOTES
- This cmdlet requires an active internet connection to communicate with the Global Discovery Service.
- The authenticated user must have access to at least one Dataverse environment to receive results.
- Environment discovery works across all geographic regions automatically.
- The cmdlet uses the Global Discovery Service endpoint at https://globaldisco.crm.dynamics.com.
- Filters are applied after all environments are retrieved, so filtering does not improve performance for large environment lists.

## RELATED LINKS

[Get-DataverseConnection](Get-DataverseConnection.md)
