
# Connection Management

## Web Account Manager (WAM)

On Windows, use `-UseWAM` to authenticate through the Windows account picker. You can pass `-Username` as an optional login hint to help identify the intended account; it does not restrict which account can be selected:

```powershell
$conn = Get-DataverseConnection -url https://myorg.crm11.dynamics.com -UseWAM -Username "user@contoso.com"
```

WAM connections can be saved with `-Name` and restored like other named connections.

## Default Connection

You can set a connection as the default, so you don't have to pass `-Connection` to every cmdlet:

*Example: Set a default connection and use it implicitly:*
```powershell
# Set a connection as default
Connect-DataverseConnection -url https://myorg.crm11.dynamics.com -interactive -SetAsDefault
# Now you can omit -Connection from all cmdlets
Get-DataverseRecord -tablename contact
Set-DataverseRecord -tablename contact @{firstname="John"; lastname="Doe"}
# You can retrieve the current default connection
$currentDefault = Get-DataverseConnection -GetDefault
```
This is especially useful in interactive sessions and scripts where you're working with a single environment.
## Named Connections
You can save connections with a name for easy reuse. Named connections persist authentication tokens securely using the platform's credential storage (Keychain on macOS, Credential Manager on Windows, libsecret on Linux) and save connection metadata for later retrieval.
### Saving a Named Connection
Add the `-Name` parameter when connecting to save the connection:
*Example: Save a connection for later use:*
```powershell
# Interactive authentication - tokens are cached securely
Get-DataverseConnection -url https://myorg.crm11.dynamics.com -interactive -Name "MyOrgProd"

# Windows Web Account Manager authentication
Get-DataverseConnection -url https://myorg.crm11.dynamics.com -UseWAM -Name "MyOrgWAM"

# Device code authentication
Get-DataverseConnection -url https://myorg.crm11.dynamics.com -devicecode -Name "MyOrgDev"
# Username/password authentication
Get-DataverseConnection -url https://myorg.crm11.dynamics.com -username "user@domain.com" -password "pass" -Name "MyOrgTest"
```
**Security Note:** By default, client secrets, certificate passwords, and user passwords are NOT saved for security reasons. You'll need to provide them again when loading the connection.
If you need to save credentials for testing or non-production scenarios, use the `-SaveCredentials` switch (NOT RECOMMENDED for production):
```powershell
# Save username/password (NOT RECOMMENDED for production)
Get-DataverseConnection -url https://myorg.crm11.dynamics.com -username "user@domain.com" -password "pass" -Name "MyOrgTest" -SaveCredentials
# Save client secret (NOT RECOMMENDED for production)
Get-DataverseConnection -url https://myorg.crm11.dynamics.com -clientid "..." -clientsecret "..." -Name "MyOrgTest" -SaveCredentials

# Save certificate with password (NOT RECOMMENDED for production)
Get-DataverseConnection -url https://myorg.crm11.dynamics.com -clientid "..." -CertificatePath "cert.pfx" -CertificatePassword "..." -Name "MyOrgCert" -SaveCredentials
```

**IMPORTANT:** Using `-SaveCredentials` stores secrets **encrypted** on disk using:
- **Windows**: Data Protection API (DPAPI) - user-specific encryption
- **Linux/macOS**: AES encryption with machine-specific key

While encrypted, this is still NOT RECOMMENDED for production use. Only use for testing or non-production scenarios.

### Loading a Named Connection

Restore a saved connection by name. The module will use cached authentication tokens (if still valid) or prompt for re-authentication:

*Example: Load a saved connection:*
```powershell
$c = Get-DataverseConnection -Name "MyOrgProd"
# Connection restored with cached credentials
```

### Clearing All Saved Connections

Remove all saved connections and cached tokens:

*Example: Clear all connections:*
```powershell
Get-DataverseConnection -ClearAllConnections
# All saved connections and cached tokens have been cleared.
```

### Listing Saved Connections

View all saved named connections:

*Example: List all saved connections:*
```powershell
Get-DataverseConnection -ListConnections
# Output shows: Name, Url, AuthMethod, Username, SavedAt
```

### Deleting a Named Connection

Remove a saved connection and its cached credentials:

*Example: Delete a saved connection:*
```powershell
Get-DataverseConnection -DeleteConnection -Name "MyOrgDev"
# Connection 'MyOrgDev' deleted successfully.
```

**Benefits of Named Connections:**
- **Convenience**: No need to remember URLs or re-authenticate frequently
- **Security**: Tokens are stored securely using platform-native credential storage
- **Multiple Environments**: Easily switch between dev, test, and production environments
- **CI/CD Friendly**: Save connections in CI/CD pipelines with service principal credentials

## See Also

- [Authentication Methods](../getting-started/authentication.md) - Learn about all supported authentication methods
- [Get-DataverseConnection](../../Rnwood.Dataverse.Data.PowerShell/docs/Get-DataverseConnection.md) - Full cmdlet documentation
