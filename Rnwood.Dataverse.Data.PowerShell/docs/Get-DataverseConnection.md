---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Get-DataverseConnection

## SYNOPSIS
Gets a connection to a Dataverse environment either interactively or silently and returns it.

All commands that need a connection to Dataverse expect you to provide the connection in `-connection` parameter.
So you can store the output of this command in a variable and pass it to each command that needs it.
See the examples for this pattern below.

## SYNTAX

### Get default connection
```
Get-DataverseConnection [-GetDefault] [-SetAsDefault] [-Timeout <UInt32>] [-TenantId <Guid>]
 [-DisableAffinityCookie] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Authenticate with username and password
```
Get-DataverseConnection [-SetAsDefault] [-SaveCredentials] [-Name <String>] [-ClientId <Guid>] [-Url <Uri>]
 -Username <String> -Password <String> [-Timeout <UInt32>] [-TenantId <Guid>] [-DisableAffinityCookie]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Authenticate with client secret
```
Get-DataverseConnection [-SetAsDefault] [-SaveCredentials] [-Name <String>] -ClientId <Guid> [-Url <Uri>]
 -ClientSecret <String> [-Timeout <UInt32>] [-TenantId <Guid>] [-DisableAffinityCookie]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Authenticate with client certificate
```
Get-DataverseConnection [-SetAsDefault] [-SaveCredentials] [-Name <String>] -ClientId <Guid> [-Url <Uri>]
 -CertificatePath <String> [-CertificatePassword <String>] [-CertificateThumbprint <String>]
 [-CertificateStoreLocation <StoreLocation>] [-CertificateStoreName <StoreName>] [-Timeout <UInt32>]
 [-TenantId <Guid>] [-DisableAffinityCookie] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Authenticate interactively
```
Get-DataverseConnection [-SetAsDefault] [-Name <String>] [-ClientId <Guid>] [-Url <Uri>] [-Username <String>]
 [-Interactive] [-Timeout <UInt32>] [-TenantId <Guid>] [-DisableAffinityCookie]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Authenticate using the device code flow
```
Get-DataverseConnection [-SetAsDefault] [-Name <String>] [-ClientId <Guid>] [-Url <Uri>] [-Username <String>]
 [-DeviceCode] [-Timeout <UInt32>] [-TenantId <Guid>] [-DisableAffinityCookie]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Authenticate with DefaultAzureCredential
```
Get-DataverseConnection [-SetAsDefault] [-Name <String>] [-Url <Uri>] [-DefaultAzureCredential]
 [-Timeout <UInt32>] [-TenantId <Guid>] [-DisableAffinityCookie] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

### Authenticate with ManagedIdentityCredential
```
Get-DataverseConnection [-SetAsDefault] [-Name <String>] [-Url <Uri>] [-ManagedIdentity]
 [-ManagedIdentityClientId <String>] [-Timeout <UInt32>] [-TenantId <Guid>] [-DisableAffinityCookie]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Load a saved named connection
```
Get-DataverseConnection [-SetAsDefault] -Name <String> [-Timeout <UInt32>] [-TenantId <Guid>]
 [-DisableAffinityCookie] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Delete a saved named connection
```
Get-DataverseConnection [-SetAsDefault] -Name <String> [-DeleteConnection] [-Timeout <UInt32>]
 [-TenantId <Guid>] [-DisableAffinityCookie] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Clear all saved connections
```
Get-DataverseConnection [-SetAsDefault] [-ClearAllConnections] [-Timeout <UInt32>] [-TenantId <Guid>]
 [-DisableAffinityCookie] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### List saved named connections
```
Get-DataverseConnection [-SetAsDefault] [-ListConnections] [-Timeout <UInt32>] [-TenantId <Guid>]
 [-DisableAffinityCookie] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Authenticate with access token script block
```
Get-DataverseConnection [-SetAsDefault] [-Url <Uri>] -AccessToken <ScriptBlock> [-Timeout <UInt32>]
 [-TenantId <Guid>] [-DisableAffinityCookie] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Authenticate with Dataverse SDK connection string.
```
Get-DataverseConnection [-SetAsDefault] -ConnectionString <String> [-Timeout <UInt32>] [-TenantId <Guid>]
 [-DisableAffinityCookie] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

### Load connection from PAC CLI profile
```
Get-DataverseConnection [-SetAsDefault] [-FromPac] [-Profile <String>] [-Timeout <UInt32>] [-TenantId <Guid>]
 [-DisableAffinityCookie] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet establishes a connection to a Microsoft Dataverse environment which can then be used with other cmdlets in this module.

All commands that need a connection to Dataverse expect you to provide the connection in `-connection` parameter.
So you can store the output of this command in a variable and pass it to each command that needs it.
See the examples for this pattern below.

Multiple authentication methods are supported:
- Interactive authentication (browser-based)
- Device code flow (for remote/headless scenarios)
- Username/password
- Client secret (for service principal authentication)
- Client certificate (for certificate-based service principal authentication)
- DefaultAzureCredential (automatic credential discovery for Azure environments)
- ManagedIdentityCredential (for Azure managed identity authentication)
- Connection string (for advanced scenarios)
- PAC CLI profile (leverages existing Power Platform CLI authentication)
- Mock connection (for testing)
- Access token script block (for custom authentication scenarios)

Connections can be saved with names for later reuse, with optional credential persistence (not recommended for production).

## EXAMPLES

### Example 1
```powershell
PS C:\> $c = Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -Interactive
```

Gets a connection to MYORG using interactive authentication and stores the result in the `$c` variable for later use.

### Example 2
```powershell
PS C:\> $c = Get-DataverseConnection -url "https://myorg.crm4.dynamics.com" -clientid "3004eb1e-7a00-45e0-a1dc-6703735eac18" -clientsecret "itsasecret"
```

Gets a connection to MYORG using Service Principal client ID and secret auth and stores the result in the `$c` variable for later use.

### Example 2b
```powershell
PS C:\> $c = Get-DataverseConnection -clientid "3004eb1e-7a00-45e0-a1dc-6703735eac18" -clientsecret "itsasecret"
```

Authenticates with client ID and secret without specifying a URL. The cmdlet will use the service principal credentials to discover available environments, then you can select which environment to connect to. Both discovery and the final connection use the same client secret authentication.

### Example 3
```powershell
PS C:\> $c = Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -DefaultAzureCredential
```

Gets a connection to MYORG using DefaultAzureCredential, which automatically discovers credentials from the environment (environment variables, workload identity federation, managed identity, Visual Studio, Azure CLI, Azure PowerShell, or interactive browser). This is ideal for Azure-hosted applications and GitHub Actions/Azure DevOps jobs that expose Azure Identity workload federation variables.

### Example 4
```powershell
PS C:\> $c = Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -ManagedIdentity
```

Gets a connection to MYORG using the system-assigned managed identity. This is useful when running in Azure environments like Azure Functions, App Service, or VMs with managed identity enabled.

### Example 5
```powershell
PS C:\> $c = Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -ManagedIdentity -ManagedIdentityClientId "12345678-1234-1234-1234-123456789abc"
```

Gets a connection to MYORG using a user-assigned managed identity with the specified client ID.

### Example 6
```powershell
PS C:\> $c = Get-DataverseConnection -Interactive
```

Authenticates interactively without specifying a URL. The cmdlet will automatically display a list of available Dataverse environments for the user to select from. This is useful when you have access to multiple environments and don't want to manually specify the URL.

### Example 7
```powershell
PS C:\> $c = Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -AccessToken { "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Im5PbzNaRHJ1VW1sMGhrR2ZaRjBqZWFHWW9XQT..." }
```

Gets a connection to MYORG using a script block that returns an access token. The script block is called whenever a new access token is needed. This is useful for custom authentication scenarios where you manage token acquisition externally.

This also works well with GitHub Actions workload identity federation steps that write a Dataverse access token to `$env:ACCESS_TOKEN`.

### Example 8
```powershell
PS C:\> $c = Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -ClientId "12345678-1234-1234-1234-123456789abc" -CertificatePath "C:\certs\mycert.pfx" -CertificatePassword "P@ssw0rd"
```

Gets a connection to MYORG using client certificate authentication with a certificate file. The certificate file is password-protected.

### Example 9
```powershell
PS C:\> $c = Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -ClientId "12345678-1234-1234-1234-123456789abc" -CertificateThumbprint "A1B2C3D4E5F6789012345678901234567890ABCD"
```

Gets a connection to MYORG using client certificate authentication with a certificate from the Windows certificate store (CurrentUser\My by default).

### Example 10
```powershell
PS C:\> $c = Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -ClientId "12345678-1234-1234-1234-123456789abc" -CertificateThumbprint "A1B2C3D4E5F6789012345678901234567890ABCD" -CertificateStoreLocation LocalMachine -CertificateStoreName Root
```

Gets a connection to MYORG using client certificate authentication with a certificate from the LocalMachine\Root certificate store.

### Example 11
```powershell
PS C:\> $c = Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -ClientId "12345678-1234-1234-1234-123456789abc" -CertificatePath "./mycert.pfx"
```

Gets a connection to MYORG using client certificate authentication with an unencrypted certificate file (no password required).

### Example 11b
```powershell
PS C:\> $c = Get-DataverseConnection -ClientId "12345678-1234-1234-1234-123456789abc" -CertificatePath "./mycert.pfx"
```

Authenticates with client certificate without specifying a URL. The cmdlet will use the certificate credentials to discover available environments, then you can select which environment to connect to. Both discovery and the final connection use the same client certificate authentication.

### Example 12: Save a named connection
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -Interactive -Name "MyOrgProd"
```

Connects to MYORG using interactive authentication and saves the connection with the name "MyOrgProd". The authentication tokens will be cached securely, and connection metadata will be saved for later use.

### Example 13: Load a saved named connection
```powershell
PS C:\> $c = Get-DataverseConnection -Name "MyOrgProd"
```

Loads the previously saved connection named "MyOrgProd". The cmdlet will use the cached authentication tokens, avoiding the need to authenticate again unless the tokens have expired.

### Example 14: List all saved connections
```powershell
PS C:\> Get-DataverseConnection -ListConnections
```

Lists all saved named connections, showing their names, URLs, authentication methods, usernames, and when they were last saved.

### Example 15: Delete a saved connection
```powershell
PS C:\> Get-DataverseConnection -DeleteConnection -Name "MyOrgProd"
```

Deletes the saved connection named "MyOrgProd", removing both the connection metadata and cached authentication tokens.

### Example 16: Save connection with client secret
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -ClientId "3004eb1e-7a00-45e0-a1dc-6703735eac18" -ClientSecret "itsasecret" -Name "MyOrgService"
```

Connects using client secret authentication and saves the connection as "MyOrgService". Note: The client secret itself is NOT saved for security reasons. When loading this connection later, you will need to provide the client secret again.

### Example 17: Save connection with credentials encrypted (NOT RECOMMENDED)
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -ClientId "3004eb1e-7a00-45e0-a1dc-6703735eac18" -ClientSecret "itsasecret" -Name "MyOrgService" -SaveCredentials
```

Saves the connection with the client secret included (encrypted). WARNING: This stores the secret encrypted on disk and is NOT RECOMMENDED for production use. Only use for testing or non-production scenarios. On Windows, uses DPAPI (Data Protection API); on Linux/macOS, uses AES encryption with machine-specific key.

### Example 18: Save certificate connection with credentials encrypted (NOT RECOMMENDED)
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -ClientId "12345678-1234-1234-1234-123456789abc" -CertificatePath "C:\certs\mycert.pfx" -CertificatePassword "P@ssw0rd" -Name "MyCertConn" -SaveCredentials
```

Saves the connection with certificate path and password included (encrypted). WARNING: This stores the password encrypted on disk and is NOT RECOMMENDED for production use. Only use for testing or non-production scenarios.

### Example 19: Save username/password connection with credentials encrypted (NOT RECOMMENDED)
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -Username "user@domain.com" -Password "mypassword" -Name "MyUserConn" -SaveCredentials
```

Saves the connection with the password included (encrypted). WARNING: This stores the password encrypted on disk and is NOT RECOMMENDED for production use. Only use for testing or non-production scenarios.

### Example 20: Clear all saved connections
```powershell
PS C:\> Get-DataverseConnection -ClearAllConnections
```

Clears all saved named connections and cached authentication tokens. This removes all connection metadata and MSAL token cache files.

### Example 21: Use connection string
```powershell
PS C:\> $c = Get-DataverseConnection -ConnectionString "AuthType=OAuth;Url=https://myorg.crm11.dynamics.com;ClientId=12345678-1234-1234-1234-123456789abc;ClientSecret=mysecret"
```

Gets a connection to MYORG using a Dataverse SDK connection string. The connection string contains all necessary authentication information including the URL, so no separate -Url parameter is needed. This is useful for scenarios where connection details are stored in configuration files or environment variables as a single string. See https://learn.microsoft.com/en-us/power-apps/developer/data-platform/xrm-tooling/use-connection-strings-xrm-tooling-connect for more information on connection string syntax.

### Example 22: Use PAC CLI profile
```powershell
PS C:\> $c = Get-DataverseConnection -FromPac
```

Connects to Dataverse using the current Power Platform CLI (PAC) authentication profile. This leverages the authentication you've already established with `pac auth create` and will use the currently selected environment (set via `pac org select`).

### Example 23: Use specific PAC CLI profile by name or index
```powershell
PS C:\> $c = Get-DataverseConnection -FromPac -Profile "MyDevProfile"
```

Connects to Dataverse using a specific named PAC CLI profile. The profile name must match one of the profiles created with `pac auth create --name <profilename>`. Alternatively, you can specify the index of the profile (e.g., "0" for the first profile).

### Example 24: Disable affinity cookie for maximum performance
```powershell
PS C:\> $c = Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -ClientId "3004eb1e-7a00-45e0-a1dc-6703735eac18" -ClientSecret "itsasecret" -DisableAffinityCookie
```

Connects to MYORG with affinity cookie disabled for maximum performance. This allows each call to Dataverse to be routed to any available server node, which can improve performance in parallel operations. However, this may result in eventual consistency issues where recently created or updated data may not be immediately visible on subsequent requests.

### Example 25: Using parallelization without DisableAffinityCookie (will show warning)
```powershell
PS C:\> $c = Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -Interactive
PS C:\> Get-DataverseRecord -Connection $c -TableName account -Top 1000 | Set-DataverseRecord -Connection $c -TableName account -MaxDegreeOfParallelism 4
WARNING: Using parallelization with affinity cookie enabled may reduce performance. Consider using Get-DataverseConnection with -DisableAffinityCookie for better parallel performance. Note: Disabling affinity cookie may result in eventual consistency issues.
```

When using parallelization with MaxDegreeOfParallelism > 1, the cmdlets will emit a warning if affinity cookie is enabled (the default). This is because affinity cookie prefers routing all requests to the same server node, which can reduce parallel performance.

### Example 26: Using parallelization with DisableAffinityCookie (optimal for performance)
```powershell
PS C:\> $c = Get-DataverseConnection -Url https://myorg.crm11.dynamics.com -Interactive -DisableAffinityCookie
PS C:\> Get-DataverseRecord -Connection $c -TableName account -Top 1000 | Set-DataverseRecord -Connection $c -TableName account -MaxDegreeOfParallelism 4
```

When using parallelization with DisableAffinityCookie, the cmdlets will NOT emit a warning. Each parallel worker can be routed to any available server node, maximizing throughput. Note that this may result in eventual consistency where data updated by one worker may not be immediately visible to another worker.

## PARAMETERS

### -AccessToken
Script block that returns an access token string. Called whenever a new access token is needed.

```yaml
Type: ScriptBlock
Parameter Sets: Authenticate with access token script block
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CertificatePassword
Password for the client certificate file. If not provided, the certificate is assumed to be unencrypted.

```yaml
Type: String
Parameter Sets: Authenticate with client certificate
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CertificatePath
Path to the client certificate file (.pfx or .p12) for authentication.

```yaml
Type: String
Parameter Sets: Authenticate with client certificate
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CertificateStoreLocation
Certificate store location to search for the certificate. Default is CurrentUser. Valid values: CurrentUser, LocalMachine.

```yaml
Type: StoreLocation
Parameter Sets: Authenticate with client certificate
Aliases:
Accepted values: CurrentUser, LocalMachine

Required: False
Position: Named
Default value: CurrentUser
Accept pipeline input: False
Accept wildcard characters: False
```

### -CertificateStoreName
Certificate store name to search for the certificate. Default is My (Personal). Valid values include: My, Root, CA, Trust, Disallowed, etc.

```yaml
Type: StoreName
Parameter Sets: Authenticate with client certificate
Aliases:
Accepted values: AddressBook, AuthRoot, CertificateAuthority, Disallowed, My, Root, TrustedPeople, TrustedPublisher

Required: False
Position: Named
Default value: My
Accept pipeline input: False
Accept wildcard characters: False
```

### -CertificateThumbprint
Thumbprint of the certificate in the certificate store. Used to load certificate from the Windows certificate store instead of a file.

```yaml
Type: String
Parameter Sets: Authenticate with client certificate
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ClearAllConnections
Clears all saved named connections and cached tokens.

```yaml
Type: SwitchParameter
Parameter Sets: Clear all saved connections
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ClientId
Client ID to use for authentication. By default the MS provided ID for PAC CLI (`9cee029c-6210-4654-90bb-17e6e9d36617`) is used to make it easy to get started.

```yaml
Type: Guid
Parameter Sets: Authenticate with username and password, Authenticate interactively, Authenticate using the device code flow
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

```yaml
Type: Guid
Parameter Sets: Authenticate with client secret, Authenticate with client certificate
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ClientSecret
Client secret to authenticate with, as registered for the Entra ID application.

```yaml
Type: String
Parameter Sets: Authenticate with client secret
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ConnectionString
Specifies the connection string to authenticate with - see https://learn.microsoft.com/en-us/power-apps/developer/data-platform/xrm-tooling/use-connection-strings-xrm-tooling-connect

```yaml
Type: String
Parameter Sets: Authenticate with Dataverse SDK connection string.
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DefaultAzureCredential
Use DefaultAzureCredential for authentication. This will try multiple authentication methods in order, including environment variables, workload identity federation, managed identity, Visual Studio, Azure CLI, Azure PowerShell, and interactive browser.

```yaml
Type: SwitchParameter
Parameter Sets: Authenticate with DefaultAzureCredential
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DeleteConnection
Deletes a saved named connection. Use with -Name to specify which connection to delete.

```yaml
Type: SwitchParameter
Parameter Sets: Delete a saved named connection
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DeviceCode
Triggers device code authentication where you will be given a URL to visit and a code to complete authentication in web browser.

```yaml
Type: SwitchParameter
Parameter Sets: Authenticate using the device code flow
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DisableAffinityCookie
Disables the affinity cookie to maximize performance at the cost of potential data consistency issues. By default, affinity cookie is enabled to ensure connections prefer a specific server node for better consistency. Only disable this if you need maximum performance and understand the tradeoffs with eventual consistency.

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

### -FromPac
Load connection from a Power Platform CLI (PAC) authentication profile. This uses the authentication profiles created with `pac auth create` and leverages the cached tokens from PAC CLI. The environment URL is determined from the profile's selected organization (set via `pac org select`).

```yaml
Type: SwitchParameter
Parameter Sets: Load connection from PAC CLI profile
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -GetDefault
Gets the current default connection. Returns an error if no default connection is set.

```yaml
Type: SwitchParameter
Parameter Sets: Get default connection
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Interactive
Triggers interactive authentication, where browser will be opened for user to interactively log in.

```yaml
Type: SwitchParameter
Parameter Sets: Authenticate interactively
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ListConnections
Lists all saved named connections.

```yaml
Type: SwitchParameter
Parameter Sets: List saved named connections
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ManagedIdentity
Use ManagedIdentityCredential for authentication. Authenticates using the managed identity assigned to the Azure resource.

```yaml
Type: SwitchParameter
Parameter Sets: Authenticate with ManagedIdentityCredential
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ManagedIdentityClientId
Client ID of the user-assigned managed identity. If not specified, the system-assigned managed identity will be used.

```yaml
Type: String
Parameter Sets: Authenticate with ManagedIdentityCredential
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Name
Name to save this connection under for later retrieval. Allows you to persist and reuse connections.

```yaml
Type: String
Parameter Sets: Authenticate with username and password, Authenticate with client secret, Authenticate with client certificate, Authenticate interactively, Authenticate using the device code flow, Authenticate with DefaultAzureCredential, Authenticate with ManagedIdentityCredential
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

```yaml
Type: String
Parameter Sets: Load a saved named connection, Delete a saved named connection
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Password
Password to authenticate with.

```yaml
Type: String
Parameter Sets: Authenticate with username and password
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Profile
Name or index of the PAC CLI profile to use. If not specified, uses the current/active profile.

```yaml
Type: String
Parameter Sets: Load connection from PAC CLI profile
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

### -SaveCredentials
WARNING: Saves the client secret with the connection. This is NOT RECOMMENDED for security reasons. Only use for testing or non-production scenarios.

```yaml
Type: SwitchParameter
Parameter Sets: Authenticate with username and password, Authenticate with client secret, Authenticate with client certificate
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SetAsDefault
When set, this connection will be used as the default for cmdlets that don't have a connection parameter specified.

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

### -TenantId
{{ Fill TenantId Description }}

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

### -Timeout
Timeout for authentication operations. Defaults to 5 minutes.

```yaml
Type: UInt32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Url
URL of the Dataverse environment to connect to. For example https://myorg.crm11.dynamics.com

```yaml
Type: Uri
Parameter Sets: Authenticate with username and password, Authenticate with client secret, Authenticate with client certificate, Authenticate interactively, Authenticate using the device code flow, Authenticate with DefaultAzureCredential, Authenticate with ManagedIdentityCredential, Authenticate with access token script block
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Username
Username to authenticate with.

```yaml
Type: String
Parameter Sets: Authenticate with username and password
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

```yaml
Type: String
Parameter Sets: Authenticate interactively, Authenticate using the device code flow
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

### None
## OUTPUTS

### Microsoft.PowerPlatform.Dataverse.Client.ServiceClient
## NOTES

## RELATED LINKS
