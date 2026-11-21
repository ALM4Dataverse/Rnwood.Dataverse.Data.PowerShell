using AwesomeAssertions;
using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Module
{
    public class ModuleImportOrderTests : E2ETestBase
    {
        private const string CommonModuleImports = @"
Import-Module PackageManagement -Force -ErrorAction Stop
Import-Module PowerShellGet -Force -ErrorAction Stop
Import-Module Az.Accounts -Force -ErrorAction Stop
Import-Module Az.Resources -Force -ErrorAction Stop

if ($PSVersionTable.PSEdition -eq 'Core') {
    Import-Module PnP.PowerShell -Force -ErrorAction Stop
}
else {
    Import-Module Microsoft.Online.SharePoint.PowerShell -Force -ErrorAction Stop
}
";

        [SkippableFact]
        [CrossPlatformTest]
        public void DataverseModule_WhoAmI_WorksWhenImportedAfterCommonModules()
        {
            var result = RunScript(GetScriptWithModuleImportOrder(commonModulesFirst: true));

            result.StandardOutput.Should().Contain("SUCCESS: Get-DataverseWhoAmI returned identity information",
                because: result.GetFullOutput());
        }

        [SkippableFact]
        [CrossPlatformTest]
        public void DataverseModule_WhoAmI_WorksWhenImportedBeforeCommonModules()
        {
            var result = RunScript(GetScriptWithModuleImportOrder(commonModulesFirst: false));

            result.StandardOutput.Should().Contain("SUCCESS: Get-DataverseWhoAmI returned identity information",
                because: result.GetFullOutput());
        }

        private string GetScriptWithModuleImportOrder(bool commonModulesFirst)
        {
            var moduleImports = commonModulesFirst
                ? $"{CommonModuleImports}{GetModuleImportStatement()}"
                : $"{GetModuleImportStatement()}{CommonModuleImports}";

            return $@"
$ErrorActionPreference = 'Stop'
{moduleImports}

$connection = Get-DataverseConnection -Url '{E2ETestsUrl}' -ClientId '{E2ETestsClientId}' -ClientSecret '{E2ETestsClientSecret}' -ErrorAction Stop
$identity = Get-DataverseWhoAmI -Connection $connection -ErrorAction Stop

if ($identity.UserId -eq [guid]::Empty -or
    $identity.BusinessUnitId -eq [guid]::Empty -or
    $identity.OrganizationId -eq [guid]::Empty) {{
    throw 'Get-DataverseWhoAmI returned incomplete identity information'
}}

Write-Host 'SUCCESS: Get-DataverseWhoAmI returned identity information'
";
        }
    }
}
