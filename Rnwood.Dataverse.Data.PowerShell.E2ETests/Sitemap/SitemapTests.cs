using AwesomeAssertions;
using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Sitemap
{
    /// <summary>
    /// Live sitemap coverage retained in E2E for publish behavior that standard tests cannot prove.
    /// </summary>
    /// <remarks>
    /// Broad sitemap CRUD and entry manipulation coverage moved to standard tests in SitemapsTests.
    /// This E2E test remains to verify publish behavior against a real Dataverse environment.
    /// </remarks>
    [Collection(SchemaChangesCollection.Name)]
    public class SitemapTests : E2ETestBase
    {
        [Fact]
        public void PublishByUniqueName_PreservesSitemapXml_AndMakesPublishedVersionRetrievable()
        {
            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'

try {
    $testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
    $sitemapUniqueName = ""test_sitemap_publish_$testRunId""
    $sitemapName = ""Test Sitemap Publish $testRunId""
    $updatedName = ""Updated $sitemapName""

    Write-Host ""Using test sitemap unique name: $sitemapUniqueName""

    $sitemapXml = @""
<SiteMap IntroducedVersion=""7.0.0.0"">
  <Area Id=""area_test_$testRunId"" ResourceId=""Area.Test"" ShowGroups=""true"" IntroducedVersion=""7.0.0.0"">
    <Titles>
      <Title LCID=""1033"" Title=""Test Area $testRunId"" />
    </Titles>
    <Group Id=""group_test_$testRunId"" ResourceId=""Group.Test"" IntroducedVersion=""7.0.0.0"" IsProfile=""false"">
      <Titles>
        <Title LCID=""1033"" Title=""Test Group $testRunId"" />
      </Titles>
      <SubArea Id=""subarea_test_$testRunId"" Icon=""/_imgs/imagestrips/transparent_spacer.gif"" Entity=""contact"" Client=""All,Outlook,OutlookLaptopClient,OutlookWorkstationClient,Web"" AvailableOffline=""true"" PassParams=""false"" Sku=""All,OnPremise,Live,SPLA"">
        <Titles>
          <Title LCID=""1033"" Title=""Test SubArea $testRunId"" />
        </Titles>
      </SubArea>
    </Group>
  </Area>
</SiteMap>
""@

    Write-Host 'Creating unpublished sitemap...'
    $sitemapId = Invoke-WithRetry {
        Set-DataverseSitemap -Connection $connection -Name $sitemapName -UniqueName $sitemapUniqueName -SitemapXml $sitemapXml -PassThru -Confirm:$false
    }

    if (-not $sitemapId) {
        throw 'Failed to create sitemap - no ID returned'
    }

    Write-Host 'Updating sitemap and publishing it...'
    $currentSitemap = Invoke-WithRetry {
        Get-DataverseSitemap -Connection $connection -Id $sitemapId
    }

    Invoke-WithRetry {
        Set-DataverseSitemap -Connection $connection -Id $sitemapId -Name $updatedName -SitemapXml $currentSitemap.SitemapXml -Publish -Confirm:$false
    }

    $publishedSitemap = Invoke-WithRetry {
        Get-DataverseSitemap -Connection $connection -Id $sitemapId -Published
    }

    if (-not $publishedSitemap) {
        throw 'Failed to retrieve published sitemap'
    }

    if ($publishedSitemap.Name -ne $updatedName) {
        throw ""Published sitemap name mismatch. Expected: $updatedName, Got: $($publishedSitemap.Name)""
    }

    Write-Host 'Verified published sitemap is retrievable after publish'

    $sitemapBeforePublish = Invoke-WithRetry {
        Get-DataverseSitemap -Connection $connection -UniqueName $sitemapUniqueName
    }

    if ([string]::IsNullOrWhiteSpace($sitemapBeforePublish.SitemapXml)) {
        throw 'Sitemap XML is already empty before publish-only test'
    }

    $originalSitemapXml = $sitemapBeforePublish.SitemapXml
    Write-Host ""Original sitemap XML length: $($originalSitemapXml.Length) characters""

    Write-Host 'Testing publish-only by UniqueName regression path...'
    Invoke-WithRetry {
        Set-DataverseSitemap -Connection $connection -UniqueName $sitemapUniqueName -Publish -Confirm:$false
    }

    $sitemapAfterPublish = Invoke-WithRetry {
        Get-DataverseSitemap -Connection $connection -UniqueName $sitemapUniqueName
    }

    if ([string]::IsNullOrWhiteSpace($sitemapAfterPublish.SitemapXml)) {
        throw 'REGRESSION: Sitemap XML became empty after publish-only operation.'
    }

    $publishedAgain = Invoke-WithRetry {
        Get-DataverseSitemap -Connection $connection -UniqueName $sitemapUniqueName -Published
    }

    if (-not $publishedAgain) {
        throw 'Failed to retrieve published sitemap after publish-only operation'
    }

    if ([string]::IsNullOrWhiteSpace($publishedAgain.SitemapXml)) {
        throw 'Published sitemap XML became empty after publish-only operation'
    }

    Write-Host 'Publish-only operation preserved sitemap XML and published version remained retrievable'

    Invoke-WithRetry {
        Remove-DataverseSitemap -Connection $connection -Id $sitemapId -IfExists -Confirm:$false
    }

    $deletedSitemap = Invoke-WithRetry {
        Get-DataverseSitemap -Connection $connection -Id $sitemapId
    }

    if ($deletedSitemap) {
        throw 'Sitemap still exists after cleanup'
    }

    Write-Host 'All sitemap publish tests passed successfully'
}
catch {
    Write-Host ""ERROR: Test failed with exception: $_""
    Write-Host ""Stack trace: $($_.ScriptStackTrace)""

    try {
        if ($sitemapId) {
            Remove-DataverseSitemap -Connection $connection -Id $sitemapId -IfExists -Confirm:$false
            Write-Host 'Cleanup successful'
        }
    }
    catch {
        Write-Warning ""Failed to cleanup test sitemap: $_""
    }

    throw
}
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("All sitemap publish tests passed successfully", because: result.GetFullOutput());
        }
    }
}