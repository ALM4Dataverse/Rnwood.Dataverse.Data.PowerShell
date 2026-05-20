using FluentAssertions;
using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Urls
{
    /// <summary>
    /// E2E tests for URL generation cmdlets: Get-DataverseRecordUrl, Get-DataverseMakerPortalUrl, Get-DataverseAdminPortalUrl.
    /// </summary>
    public class UrlCmdletTests : E2ETestBase
    {
        [Fact]
        public void GetDataverseRecordUrl_ExistingRecord_ReturnsCorrectUrl()
        {
            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'

# Get an existing contact record
$contacts = Get-DataverseRecord -Connection $connection -TableName contact -Top 1 -Columns contactid
if (-not $contacts) { throw 'No contact records found for testing' }
$contactId = $contacts.contactid

$url = Get-DataverseRecordUrl -Connection $connection -TableName contact -Id $contactId

Write-Host ""URL: $url""

# Validate URL format
$orgUrl = $connection.ConnectedOrgPublishedEndpoints['WebApplication'].TrimEnd('/')
if (-not $url.StartsWith($orgUrl)) { throw ""URL does not start with org URL. Expected start: $orgUrl, Got: $url"" }
if ($url -notmatch 'main\.aspx') { throw ""URL does not contain main.aspx"" }
if ($url -notmatch 'etn=contact') { throw ""URL does not contain etn=contact"" }
if ($url -notmatch ""id=$contactId"") { throw ""URL does not contain record ID"" }
if ($url -notmatch 'pagetype=entityrecord') { throw ""URL does not contain pagetype=entityrecord"" }

Write-Host 'SUCCESS'
");
            var result = RunScript(script);
            result.StandardOutput.Should().Contain("SUCCESS", because: result.GetFullOutput());
        }

        [Fact]
        public void GetDataverseRecordUrl_NewRecord_ReturnsUrlWithoutId()
        {
            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'

$url = Get-DataverseRecordUrl -Connection $connection -TableName account

Write-Host ""URL: $url""

$orgUrl = $connection.ConnectedOrgPublishedEndpoints['WebApplication'].TrimEnd('/')
if (-not $url.StartsWith($orgUrl)) { throw ""URL does not start with org URL"" }
if ($url -notmatch 'etn=account') { throw ""URL does not contain etn=account"" }
if ($url -match '&id=') { throw ""URL should not contain an id parameter"" }
if ($url -notmatch 'pagetype=entityrecord') { throw ""URL does not contain pagetype=entityrecord"" }

Write-Host 'SUCCESS'
");
            var result = RunScript(script);
            result.StandardOutput.Should().Contain("SUCCESS", because: result.GetFullOutput());
        }

        [Fact]
        public void GetDataverseRecordUrl_WithAppUniqueName_ResolvesAppIdAndIncludesInUrl()
        {
            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'

# Get an existing app module and a contact record
$apps = Get-DataverseRecord -Connection $connection -TableName appmodule -Columns appmoduleid,uniquename -Top 1
if (-not $apps) { throw 'No app modules found for testing' }
$app = $apps[0]
Write-Host ""Using app: $($app.uniquename) / $($app.appmoduleid)""

$contacts = Get-DataverseRecord -Connection $connection -TableName contact -Top 1 -Columns contactid
$contactId = $contacts.contactid

$url = Get-DataverseRecordUrl -Connection $connection -TableName contact -Id $contactId -AppUniqueName $app.uniquename

Write-Host ""URL: $url""

if ($url -notmatch ""appid=$($app.appmoduleid)"") { throw ""URL does not contain expected appid $($app.appmoduleid). URL: $url"" }

Write-Host 'SUCCESS'
");
            var result = RunScript(script);
            result.StandardOutput.Should().Contain("SUCCESS", because: result.GetFullOutput());
        }

        [Fact]
        public void GetDataverseRecordUrl_WithAppId_IncludesAppIdInUrl()
        {
            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'

$apps = Get-DataverseRecord -Connection $connection -TableName appmodule -Columns appmoduleid -Top 1
if (-not $apps) { throw 'No app modules found for testing' }
$appId = $apps[0].appmoduleid

$contacts = Get-DataverseRecord -Connection $connection -TableName contact -Top 1 -Columns contactid
$contactId = $contacts.contactid

$url = Get-DataverseRecordUrl -Connection $connection -TableName contact -Id $contactId -AppId $appId

Write-Host ""URL: $url""

if ($url -notmatch ""appid=$appId"") { throw ""URL does not contain appid. URL: $url"" }

Write-Host 'SUCCESS'
");
            var result = RunScript(script);
            result.StandardOutput.Should().Contain("SUCCESS", because: result.GetFullOutput());
        }

        [Fact]
        public void GetDataverseRecordUrl_WithInvalidAppUniqueName_ThrowsError()
        {
            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'

try {
    Get-DataverseRecordUrl -Connection $connection -TableName contact -AppUniqueName 'nonexistent_app_xyz_12345'
    throw 'Should have thrown an error for non-existent app'
} catch {
    if ($_.Exception.Message -notmatch 'nonexistent_app_xyz_12345') {
        throw ""Unexpected error message: $($_.Exception.Message)""
    }
    Write-Host ""Correctly threw error for missing app""
}

Write-Host 'SUCCESS'
");
            var result = RunScript(script);
            result.StandardOutput.Should().Contain("SUCCESS", because: result.GetFullOutput());
        }

        [Fact]
        public void GetDataverseMakerPortalUrl_NoParameters_ReturnsHomePageUrl()
        {
            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'

$url = Get-DataverseMakerPortalUrl -Connection $connection

Write-Host ""URL: $url""

$orgId = $connection.ConnectedOrgId.ToString('D')
if (-not $url.StartsWith('https://make.powerapps.com/environments/')) { throw ""URL does not start with expected prefix: $url"" }
if ($url -notmatch $orgId) { throw ""URL does not contain org ID $orgId. URL: $url"" }
if (-not $url.EndsWith('/home')) { throw ""URL does not end with /home: $url"" }

Write-Host 'SUCCESS'
");
            var result = RunScript(script);
            result.StandardOutput.Should().Contain("SUCCESS", because: result.GetFullOutput());
        }

        [Fact]
        public void GetDataverseMakerPortalUrl_WithTableName_ReturnsTableContextUrl()
        {
            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'

$url = Get-DataverseMakerPortalUrl -Connection $connection -TableName contact

Write-Host ""URL: $url""

$orgId = $connection.ConnectedOrgId.ToString('D')
if (-not $url.StartsWith('https://make.powerapps.com/environments/')) { throw ""URL does not start with expected prefix: $url"" }
if ($url -notmatch $orgId) { throw ""URL does not contain org ID $orgId. URL: $url"" }
if (-not $url.EndsWith('/entities/entity/contact')) { throw ""URL does not end with table path: $url"" }

Write-Host 'SUCCESS'
");
            var result = RunScript(script);
            result.StandardOutput.Should().Contain("SUCCESS", because: result.GetFullOutput());
        }

        [Fact]
        public void GetDataverseAdminPortalUrl_ReturnsEnvironmentHubUrl()
        {
            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'

$url = Get-DataverseAdminPortalUrl -Connection $connection

Write-Host ""URL: $url""

$orgId = $connection.ConnectedOrgId.ToString('D')
if (-not $url.StartsWith('https://admin.powerplatform.microsoft.com/environments/')) { throw ""URL does not start with expected prefix: $url"" }
if ($url -notmatch $orgId) { throw ""URL does not contain org ID $orgId. URL: $url"" }
if (-not $url.EndsWith('/hub')) { throw ""URL does not end with /hub: $url"" }

Write-Host 'SUCCESS'
");
            var result = RunScript(script);
            result.StandardOutput.Should().Contain("SUCCESS", because: result.GetFullOutput());
        }
    }
}
