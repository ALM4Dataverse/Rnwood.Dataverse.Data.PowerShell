# PowerShell and Module Compatibility

## PowerShell versions

The module supports:

- **PowerShell 7.4 or later** (PowerShell Core) on Windows, Linux, and macOS. PowerShell 7.4 requires the .NET 8 runtime.
- **Windows PowerShell 5.1** (PowerShell Desktop) on Windows.

The CI matrix currently tests PowerShell 5.1, 7.4.14, 7.5.5, 7.6.0, and 7.7.0. PowerShell 7.4 is tested on Windows and Linux; PowerShell 7.5, 7.6, and 7.7 are tested on Windows and Linux; PowerShell 5.1 is tested on Windows. These are tested versions, not a guarantee that every later version of every dependency is compatible.

The module includes runtime-specific .NET assets where available. On PowerShell Core, it selects assets matching the runtime major version and falls back to the .NET 8 assets when no matching folder is packaged. Windows PowerShell uses the .NET Framework assets.

## Using other modules in the same session

PowerShell modules that use .NET libraries share the PowerShell process. Their assemblies may therefore interact with assemblies loaded by this module; changing module import order cannot always resolve an assembly version or type-identity conflict. The module's CI tests importing it both before and after a set of commonly co-used modules:

| Module | Version used in CI | Notes |
| --- | --- | --- |
| Az.Accounts | 5.5.0 | Paired with Az.Resources 9.1.0 in the interoperability tests. |
| Az.Resources | 9.1.0 | Its Az.Accounts requirement is compatible with the tested 5.5.0 version. Keep Azure PowerShell module versions and their declared requirements aligned. |
| PnP.PowerShell | 3.5.0 | Tested with PowerShell Core. |
| Microsoft.Online.SharePoint.PowerShell | 16.0.27814.12015 | Tested with Windows PowerShell 5.1. |
| PowerShellGet and PackageManagement | Runner-provided versions | Imported in the same interoperability tests; versions are not pinned by the workflow. |

These are specific versions tested together, not minimum versions or a promise that all releases work together. In particular, importing Az.Accounts 5.5.5 after the Dataverse module has been observed to fail with an `Azure.Identity.TokenCachePersistenceOptions` load error in a PowerShell 7.6/.NET 10 session. If you encounter this, use a mutually compatible Azure module set (the CI-tested pair above is one option) or run the modules in separate PowerShell processes. A newer Az.Resources release may require a newer Az.Accounts version, so do not change one version without checking the other's module requirements.

## Troubleshooting

1. **Start a fresh PowerShell process.** .NET assemblies generally cannot be unloaded from the process, so restarting is necessary after an assembly conflict or when changing dependency versions.
2. **Check the host and module versions.**

   ```powershell
   $PSVersionTable
   Get-Module Rnwood.Dataverse.Data.PowerShell, Az.Accounts, Az.Resources, PnP.PowerShell, Microsoft.Online.SharePoint.PowerShell |
       Select-Object Name, Version, Path
   ```

3. **Check module dependencies.** For Azure PowerShell, inspect the `RequiredModules` or `ModuleList` in each installed module manifest and install versions whose requirements agree. Avoid loading incompatible versions of the same assembly into one session.
4. **Try the import orders in a clean process.** Import the companion modules before the Dataverse module, then repeat with the Dataverse module first. If one order fails, capture the complete exception and the versions and paths from the previous command.
5. **Separate modules if needed.** If the versions required by two modules cannot coexist in one process, run one module in another PowerShell process and exchange data through files or another suitable interface.
6. **Distinguish loading errors from connection errors.** Once imports succeed, test Dataverse connectivity separately with `Get-DataverseConnection` and `Get-DataverseWhoAmI`; a sign-in, permissions, or network error is different from an assembly-loading failure.
7. **When reporting an issue,** include `$PSVersionTable`, operating system, module versions and paths, import order, and the full exception text. Do not include access tokens, client secrets, or other credentials.
