using AwesomeAssertions;
using Xunit;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using System.IO;
using System;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets;

public class ModuleLoadingTests : TestBase
{
    [Fact]
    public void Module_CanBeLoadedSuccessfully_WhenNotAlreadyLoaded()
    {
        EnsureModulePath();

        var script = @"
# Module is imported by PowerShellProcessRunner before this script executes
$module = Get-Module Rnwood.Dataverse.Data.PowerShell
if (-not $module) { throw 'Module was not imported' }

$sdkAssembly = [AppDomain]::CurrentDomain.GetAssemblies() |
    Where-Object { $_.GetName().Name -eq 'Microsoft.Xrm.Sdk' } |
    Select-Object -First 1

if (-not $sdkAssembly) { throw 'Microsoft.Xrm.Sdk assembly not loaded' }

Write-Output 'PASS'
";

        var result = PowerShellProcessRunner.Run(script);
        result.Success.Should().BeTrue($"Script failed: {result.GetFullOutput()}");
    }

    [Fact]
    public void Module_CanBeLoadedSuccessfully_WhenSdkAssembliesAlreadyLoaded()
    {
        EnsureModulePath();

        var script = @"
# Module manifest and path are provided by PowerShellProcessRunner
if (-not (Test-Path $moduleManifest)) { throw ""Module manifest not found at $moduleManifest"" }
if (-not (Test-Path $modulePath)) { throw ""Module path not found at $modulePath"" }

# Detect PowerShell edition and select appropriate cmdlets directory
# Desktop = Windows PowerShell 5.1 (.NET Framework) → net462
# Core = PowerShell 7+ (.NET Core/8+) → net8.0
$targetFramework = if ($PSVersionTable.PSEdition -eq 'Core') { 'net8.0' } else { 'net462' }
Write-Verbose ""Detected PowerShell edition: $($PSVersionTable.PSEdition), using target framework: $targetFramework""

# Pre-load SDK assembly from module path to simulate preexisting load
# Note: Add-Type may throw ReflectionTypeLoadException because some types in the SDK
# have dependencies that aren't loaded yet, but the assembly itself still gets loaded
# into the AppDomain. We suppress this expected error.
$assemblyPath = Join-Path $modulePath ""cmdlets/$targetFramework/Microsoft.Xrm.Sdk.dll""
if (-not (Test-Path $assemblyPath)) { throw ""SDK assembly not found at $assemblyPath"" }
$resolvedAssemblyPath = (Resolve-Path $assemblyPath).Path
try {
    Add-Type -Path $resolvedAssemblyPath -ErrorAction Stop
} catch [System.Reflection.ReflectionTypeLoadException] {
    # Expected error - assembly is still loaded despite some types failing to load
    Write-Verbose ""ReflectionTypeLoadException caught (expected): $($_.Exception.Message)""
}

# Import module after SDK assembly is already loaded
Import-Module $moduleManifest -Force -ErrorAction Stop

$sdkAssembly = [AppDomain]::CurrentDomain.GetAssemblies() |
    Where-Object { $_.GetName().Name -eq 'Microsoft.Xrm.Sdk' } |
    Select-Object -First 1

if (-not $sdkAssembly) { throw 'Microsoft.Xrm.Sdk assembly not loaded after import' }
if ($sdkAssembly.Location -ne $resolvedAssemblyPath) { throw ""Assembly path mismatch: $($sdkAssembly.Location)"" }

Write-Output 'PASS'
";

        var result = PowerShellProcessRunner.Run(script, importModule: false);
        result.Success.Should().BeTrue($"Script failed: {result.GetFullOutput()}");
    }

    [Fact]
    public void Module_HelpContentReflectsHelpFilesWithExpectedStructure()
    {
        EnsureModulePath();

        var script = @"
$testCmdlets = @(
    'Get-DataverseConnection',
    'Get-DataverseRecord',
    'Set-DataverseRecord',
    'Remove-DataverseRecord',
    'Invoke-DataverseRequest',
    'Invoke-DataverseSql',
    'Get-DataverseWhoAmI'
)

$issues = @()

foreach ($cmdletName in $testCmdlets) {
    $help = Get-Help $cmdletName -Full

    if (-not $help.Name) {
        $issues += ""$cmdletName`: Missing Name""
    }

    if (-not $help.Syntax) {
        $issues += ""$cmdletName`: Missing Syntax""
    }

    if (-not $help.Parameters) {
        $issues += ""$cmdletName`: Missing Parameters section""
    }

    if ($help.Parameters -and $help.Parameters.Parameter) {
        $paramsWithDescription = @($help.Parameters.Parameter | Where-Object {
            ($_.Description -is [string] -and $_.Description) -or
            ($_.Description.Text -and $_.Description.Text)
        })

        if ($paramsWithDescription.Count -eq 0) {
            $issues += ""$cmdletName`: No parameters have descriptions""
        }
    }
}

if ($issues.Count -gt 0) {
    throw ""Help validation issues found:`n$($issues -join ""`n"")""
}

Write-Output 'PASS'
";

        var result = PowerShellProcessRunner.Run(script);
        result.Success.Should().BeTrue($"Script failed: {result.GetFullOutput()}");
    }

    [Fact]
    public void Module_HelpFilesExistInEnGbDirectory()
    {
        EnsureModulePath();

        var script = @"
$module = Get-Module Rnwood.Dataverse.Data.PowerShell
if (-not $module) { throw 'Module was not imported' }

$helpPath = Join-Path $module.ModuleBase 'en-GB'
if (-not (Test-Path $helpPath)) {
    throw ""Help directory not found at: $helpPath""
}

$helpFiles = Get-ChildItem -Path $helpPath -Filter '*.xml'
if ($helpFiles.Count -eq 0) {
    throw ""No help XML files found in $helpPath""
}

$mainHelpFile = Join-Path $helpPath 'Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml'
if (-not (Test-Path $mainHelpFile)) {
    throw ""Main help file not found: $mainHelpFile""
}

Write-Output 'PASS'
";

        var result = PowerShellProcessRunner.Run(script);
        result.Success.Should().BeTrue($"Script failed: {result.GetFullOutput()}");
    }

    [Fact]
    public void Module_GetDataverseConnectionWorksWhenAzureFunctionsWorkerHasConflictingAssemblyPreloaded()
    {
        EnsureModulePath();

        var funcWorkerPs74Dir = FindFuncWorkerPs74Dir();
        if (funcWorkerPs74Dir == null)
        {
            return;
        }

        var conflictingDll = Path.Combine(funcWorkerPs74Dir!, "Microsoft.Extensions.Logging.Abstractions.dll");

        var script = $@"
$conflictingDll = '{conflictingDll.Replace("'", "''")}'
$preloaded = [System.Runtime.Loader.AssemblyLoadContext]::Default.LoadFromAssemblyPath($conflictingDll)
Write-Host ""[AzureFunc simulation] Pre-loaded: $($preloaded.GetName().Name) v$($preloaded.GetName().Version) into DEFAULT ALC""

try {{
    Get-DataverseConnection -GetDefault -ErrorAction Stop
    throw 'Expected Get-DataverseConnection -GetDefault to fail without a default connection'
}}
catch {{
    if ($_.FullyQualifiedErrorId -notlike '*NoDefaultConnection*') {{
        throw
    }}
}}

Write-Host 'SUCCESS: module and Get-DataverseConnection work despite Azure Functions assembly conflict'
";

        var result = PowerShellProcessRunner.Run(script);

        result.StandardOutput.Should().Contain("SUCCESS",
            because: $"Module should work in Azure Functions-style assembly conflict conditions.\nFull output:\n{result.GetFullOutput()}");
        result.Success.Should().BeTrue(
            because: $"Script should exit cleanly.\nFull output:\n{result.GetFullOutput()}");
    }

    [Fact]
    public void Module_SdkDependenciesLoadFromRuntimeMatchedTfmFolder()
    {
        EnsureModulePath();

        var script = @"
$runtimeMajor = [Environment]::Version.Major
$expectedTfm  = if ($runtimeMajor -ge 10) { ""net$runtimeMajor.0"" }
                elseif ($runtimeMajor -eq 9) { ""net9.0"" }
                else { ""net8.0"" }

$moduleRoot = Split-Path -Parent $moduleManifest
$tfmFolder  = Join-Path $moduleRoot ""cmdlets/$expectedTfm""
if (-not (Test-Path $tfmFolder)) {
    $expectedTfm = ""net8.0""
}

$sdkAsms = [System.Runtime.Loader.AssemblyLoadContext]::All |
    ForEach-Object { $_.Assemblies } |
    Where-Object {
        $_.Location -and (
            $_.GetName().Name -eq 'Microsoft.Xrm.Sdk' -or
            $_.GetName().Name -eq 'Microsoft.PowerPlatform.Dataverse.Client'
        )
    }

if (-not $sdkAsms) {
    throw 'No SDK assemblies found in any AssemblyLoadContext after module import'
}

$failures = @()
foreach ($asm in $sdkAsms) {
    $location = $asm.Location -replace '\\', '/'
    if ($location -notlike ""*/cmdlets/$expectedTfm/*"") {
        $failures += ""$($asm.GetName().Name): expected path containing 'cmdlets/$expectedTfm/' but got: $location""
    }
}

if ($failures.Count -gt 0) {
    throw ""TFM routing mismatch:`n$($failures -join ""`n"")""
}

Write-Host ""SUCCESS: all SDK assemblies loaded from cmdlets/$expectedTfm/""
";

        var result = PowerShellProcessRunner.Run(script, usePowerShellCore: true, timeoutSeconds: 120);

        result.StandardOutput.Should().Contain("SUCCESS",
            because: $"SDK dependencies should load from the runtime-matched TFM folder.\nFull output:\n{result.GetFullOutput()}");
        result.Success.Should().BeTrue(
            because: $"Script should exit cleanly.\nFull output:\n{result.GetFullOutput()}");
    }

    [Fact]
    public void Module_ServiceClientSharesLoggingAbstractionsWithDefaultLoadContext()
    {
        EnsureModulePath();

        var script = @"
$runtimeMajor = [Environment]::Version.Major
$expectedTfm = if ($runtimeMajor -ge 10) { ""net$runtimeMajor.0"" }
               elseif ($runtimeMajor -eq 9) { ""net9.0"" }
               else { ""net8.0"" }

$moduleRoot = Split-Path -Parent $moduleManifest
$tfmFolder = Join-Path $moduleRoot ""cmdlets/$expectedTfm""
if (-not (Test-Path $tfmFolder)) {
    $expectedTfm = ""net8.0""
    $tfmFolder = Join-Path $moduleRoot ""cmdlets/$expectedTfm""
}

$loggingPath = Join-Path $tfmFolder 'Microsoft.Extensions.Logging.Abstractions.dll'
$defaultLogging = [System.Runtime.Loader.AssemblyLoadContext]::Default.Assemblies |
    Where-Object { $_.GetName().Name -eq 'Microsoft.Extensions.Logging.Abstractions' } |
    Select-Object -First 1
if (-not $defaultLogging) {
    $defaultLogging = [System.Runtime.Loader.AssemblyLoadContext]::Default.LoadFromAssemblyPath($loggingPath)
}

Import-Module $moduleManifest -Force -ErrorAction Stop
$cmdletsAssembly = [System.Runtime.Loader.AssemblyLoadContext]::Default.Assemblies |
    Where-Object { $_.GetName().Name -eq 'Rnwood.Dataverse.Data.PowerShell.Cmdlets' } |
    Select-Object -First 1
$cmdletType = $cmdletsAssembly.GetType('Rnwood.Dataverse.Data.PowerShell.Commands.GetDataverseEnvironmentCmdlet', $true)
$serviceClientType = $cmdletType.GetProperty('Connection').PropertyType
$discoverMethod = $serviceClientType.GetMethods() |
    Where-Object { $_.Name -eq 'DiscoverOnlineOrganizationsAsync' } |
    Select-Object -First 1
if (-not $discoverMethod) {
    throw 'ServiceClient.DiscoverOnlineOrganizationsAsync was not found'
}

$loggerType = $discoverMethod.GetParameters() |
    Where-Object { $_.ParameterType.FullName -eq 'Microsoft.Extensions.Logging.ILogger' } |
    Select-Object -ExpandProperty ParameterType -First 1
if (-not $loggerType) {
    throw 'DiscoverOnlineOrganizationsAsync ILogger parameter was not found'
}
if (-not [object]::ReferenceEquals($loggerType.Assembly, $defaultLogging)) {
    throw ""ServiceClient uses a different Microsoft.Extensions.Logging.Abstractions assembly than the default context""
}

Write-Host 'SUCCESS: ServiceClient shares Microsoft.Extensions.Logging.Abstractions with the default context'
";

        var result = PowerShellProcessRunner.Run(script, usePowerShellCore: true, timeoutSeconds: 120, importModule: false);

        result.StandardOutput.Should().Contain("SUCCESS",
            because: $"ServiceClient should share logging abstractions with the default context.\nFull output:\n{result.GetFullOutput()}");
        result.Success.Should().BeTrue(
            because: $"Script should exit cleanly.\nFull output:\n{result.GetFullOutput()}");
    }

    [Fact]
    public void Module_ServiceClientUsesBundledOptionsWhenDefaultContextHasOptionsLoaded()
    {
        EnsureModulePath();

        var script = @"
$runtimeMajor = [Environment]::Version.Major
$expectedTfm = if ($runtimeMajor -ge 10) { ""net$runtimeMajor.0"" }
               elseif ($runtimeMajor -eq 9) { ""net9.0"" }
               else { ""net8.0"" }

$moduleRoot = Split-Path -Parent $moduleManifest
$tfmFolder = Join-Path $moduleRoot ""cmdlets/$expectedTfm""
if (-not (Test-Path $tfmFolder)) {
    $tfmFolder = Join-Path $moduleRoot 'cmdlets/net8.0'
}

$optionsPath = Join-Path $tfmFolder 'Microsoft.Extensions.Options.dll'
$defaultOptions = [System.Runtime.Loader.AssemblyLoadContext]::Default.Assemblies |
    Where-Object { $_.GetName().Name -eq 'Microsoft.Extensions.Options' } |
    Select-Object -First 1
if (-not $defaultOptions) {
    $defaultOptions = [System.Runtime.Loader.AssemblyLoadContext]::Default.LoadFromAssemblyPath($optionsPath)
}

Import-Module $moduleManifest -Force -ErrorAction Stop
$cmdletsAssembly = [System.Runtime.Loader.AssemblyLoadContext]::Default.Assemblies |
    Where-Object { $_.GetName().Name -eq 'Rnwood.Dataverse.Data.PowerShell.Cmdlets' } |
    Select-Object -First 1
$cmdletType = $cmdletsAssembly.GetType('Rnwood.Dataverse.Data.PowerShell.Commands.GetDataverseEnvironmentCmdlet', $true)
$serviceClientType = $cmdletType.GetProperty('Connection').PropertyType
$clientContext = [System.Runtime.Loader.AssemblyLoadContext]::GetLoadContext($serviceClientType.Assembly)
$clientOptions = $clientContext.LoadFromAssemblyName([System.Reflection.AssemblyName]::new('Microsoft.Extensions.Options'))

if ([object]::ReferenceEquals($clientOptions, $defaultOptions)) {
    throw 'ServiceClient resolved Microsoft.Extensions.Options from the default context instead of its bundled dependencies'
}
if ([System.IO.Path]::GetFullPath($clientOptions.Location) -ne [System.IO.Path]::GetFullPath($optionsPath)) {
    throw ""ServiceClient loaded Microsoft.Extensions.Options from '$($clientOptions.Location)' instead of '$optionsPath'""
}

$optionsExtensions = $clientOptions.GetType('Microsoft.Extensions.DependencyInjection.OptionsServiceCollectionExtensions', $true)
$serviceCollectionType = $clientContext.LoadFromAssemblyName([System.Reflection.AssemblyName]::new('Microsoft.Extensions.DependencyInjection.Abstractions')).GetType('Microsoft.Extensions.DependencyInjection.IServiceCollection', $true)
$addOptions = $optionsExtensions.GetMethods() |
    Where-Object {
        $_.Name -eq 'AddOptions' -and
        $_.IsGenericMethod -and
        $_.ReturnType.Name -eq 'OptionsBuilder`1' -and
        $_.GetParameters().Length -eq 1 -and
        $_.GetParameters()[0].ParameterType -eq $serviceCollectionType
    } |
    Select-Object -First 1
if (-not $addOptions) {
    throw 'The bundled Options assembly does not expose the expected AddOptions API'
}

Write-Host 'SUCCESS: ServiceClient uses bundled Microsoft.Extensions.Options'
";

        var result = PowerShellProcessRunner.Run(script, usePowerShellCore: true, timeoutSeconds: 120, importModule: false);

        result.StandardOutput.Should().Contain("SUCCESS",
            because: $"ServiceClient should use its bundled options dependencies.\nFull output:\n{result.GetFullOutput()}");
        result.Success.Should().BeTrue(
            because: $"Script should exit cleanly.\nFull output:\n{result.GetFullOutput()}");
    }

    private static void EnsureModulePath()
    {
        // Check if TESTMODULEPATH is already set (e.g., by CI)
        var existingPath = System.Environment.GetEnvironmentVariable("TESTMODULEPATH");
        if (!string.IsNullOrEmpty(existingPath))
        {
            if (!Directory.Exists(existingPath))
            {
                throw new InvalidOperationException($"TESTMODULEPATH is set to {existingPath}, but directory does not exist.");
            }
            return; // Already set, nothing to do
        }
        
        // Not set - try to find it using solution root
        var solutionRoot = FindSolutionRoot();
        var defaultPath = Path.Combine(solutionRoot, "Rnwood.Dataverse.Data.PowerShell", "bin", "Debug", "netstandard2.0");

        if (!Directory.Exists(defaultPath))
        {
            throw new InvalidOperationException($"Expected module output at {defaultPath}. Build the module or set TESTMODULEPATH explicitly.");
        }

        System.Environment.SetEnvironmentVariable("TESTMODULEPATH", defaultPath);
    }

    private static string FindSolutionRoot()
    {
        var slnName = "Rnwood.Dataverse.Data.PowerShell.sln";
        var startDirs = new[]
        {
            new DirectoryInfo(Directory.GetCurrentDirectory()),
            new DirectoryInfo(Path.GetDirectoryName(typeof(ModuleLoadingTests).Assembly.Location)!)
        };

        foreach (var start in startDirs)
        {
            for (var dir = start; dir != null; dir = dir.Parent)
            {
                var candidate = Path.Combine(dir.FullName, slnName);
                if (File.Exists(candidate))
                {
                    return dir.FullName;
                }
            }
        }

        throw new InvalidOperationException($"Could not locate solution root containing {slnName} starting from {Directory.GetCurrentDirectory()} and {typeof(ModuleLoadingTests).Assembly.Location}.");
    }

    private static string? FindFuncWorkerPs74Dir()
    {
        const string workerRelativePath = "bin/in-proc8/workers/powershell/7.4";
        const string workerRelativePathWin = @"bin\in-proc8\workers\powershell\7.4";

        var searchRoots = new System.Collections.Generic.List<string>
        {
            "/opt/homebrew/lib/node_modules/azure-functions-core-tools",
            "/usr/lib/node_modules/azure-functions-core-tools",
            Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile), ".npm-global/lib/node_modules/azure-functions-core-tools"),
            Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), @"npm\node_modules\azure-functions-core-tools"),
            @"C:\ProgramData\chocolatey\lib\azure-functions-core-tools\tools",
        };

        try
        {
            using var npmRoot = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "npm",
                Arguments = "root -g",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            });

            if (npmRoot != null)
            {
                var root = npmRoot.StandardOutput.ReadToEnd().Trim();
                npmRoot.WaitForExit();
                if (!string.IsNullOrEmpty(root))
                {
                    searchRoots.Insert(0, Path.Combine(root, "azure-functions-core-tools"));
                }
            }
        }
        catch
        {
        }

        var workerSuffix = Path.DirectorySeparatorChar == '\\' ? workerRelativePathWin : workerRelativePath;
        foreach (var toolsRoot in searchRoots)
        {
            var candidatePath = Path.Combine(toolsRoot, workerSuffix);
            if (Directory.Exists(candidatePath) &&
                File.Exists(Path.Combine(candidatePath, "Microsoft.Extensions.Logging.Abstractions.dll")))
            {
                return candidatePath;
            }
        }

        return null;
    }
}
