using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using AwesomeAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Forms
{
    /// <summary>
    /// Live-environment smoke tests for form publishing.
    /// Standard tests cover form XML manipulation in detail; this class keeps only the
    /// real Dataverse publish/persistence path.
    /// </summary>
    [Collection(SchemaChangesCollection.Name)]
    public class FormManipulationTests : E2ETestBase
    {
        [Fact]
        public void FormCustomizations_CanBePublished_AndCleanedUp()
        {
            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'
$ConfirmPreference = 'None'
$VerbosePreference = 'Continue'

try {
    $testRunId = [guid]::NewGuid().ToString('N').Substring(0, 8)
    $formName = ""E2ETestPublishForm-$testRunId""
    $entityName = 'account'
    $formId = $null

    Write-Host '=========================================='
    Write-Host 'Starting Form Publish E2E Test'
    Write-Host ""Test Run ID: $testRunId""
    Write-Host ""Form Name: $formName""
    Write-Host '=========================================='

    $formId = Invoke-WithRetry {
        Set-DataverseForm -Connection $connection `
            -Entity $entityName `
            -Name $formName `
            -FormType Main `
            -Description ""E2E publish smoke test - Run $testRunId"" `
            -IsActive `
            -PassThru
    }

    if (-not $formId) {
        throw 'Failed to create form'
    }

    Write-Host ""Created form: $formId""

    Invoke-WithRetry {
        Set-DataverseFormTab -Connection $connection `
            -FormId $formId `
            -Name 'PublishTab' `
            -Label 'Publish Tab' `
            -Layout OneColumn `
            -Expanded `
            -ShowLabel `
            -Confirm:$false
    }

    Invoke-WithRetry {
        Set-DataverseFormSection -Connection $connection `
            -FormId $formId `
            -TabName 'PublishTab' `
            -Name 'PublishSection' `
            -Label 'Publish Section' `
            -ShowLabel `
            -Confirm:$false
    }

    Invoke-WithRetry {
        Set-DataverseFormControl -Connection $connection `
            -FormId $formId `
            -TabName 'PublishTab' `
            -SectionName 'PublishSection' `
            -ControlId 'name' `
            -DataField 'name' `
            -ControlType Standard `
            -Labels @{1033 = 'Account Name'} `
            -Confirm:$false
    }

    Invoke-WithRetry {
        Set-DataverseForm -Connection $connection `
            -Id $formId `
            -Publish `
            -Confirm:$false
    }

    $publishedForm = Invoke-WithRetry {
        Get-DataverseForm -Connection $connection -Id $formId -IncludeFormXml
    }

    if (-not $publishedForm) {
        throw 'Failed to retrieve published form'
    }

    if (-not $publishedForm.formxml) {
        throw 'Published formxml should not be empty'
    }

    $publishedControl = Invoke-WithRetry {
        Get-DataverseFormControl -Connection $connection `
            -FormId $formId `
            -TabName 'PublishTab' `
            -SectionName 'PublishSection' `
            -ControlId 'name'
    }

    if (-not $publishedControl) {
        throw 'Expected published control was not found'
    }

    Write-Host 'Verified: published form and control are available'
}
catch {
    Write-Host ''
    Write-Host '=========================================='
    Write-Host 'ERROR: Form Publish E2E Test FAILED'
    Write-Host '=========================================='
    Write-Host ($_ | Out-String)
    throw
}
finally {
    if ($formId) {
        try {
            Invoke-WithRetry {
                Remove-DataverseForm -Connection $connection -Id $formId -Confirm:$false
            }
            Write-Host ""Cleaned up form: $formId""
        }
        catch {
            Write-Host ""Cleanup failed for form ${formId}: $($_.Exception.Message)""
            throw
        }
    }
}

Write-Host 'Form Publish E2E Test PASSED'
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("Form Publish E2E Test PASSED", because: result.GetFullOutput());
        }
    }
}
