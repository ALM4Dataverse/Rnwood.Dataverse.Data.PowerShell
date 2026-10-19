using AwesomeAssertions;
using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.Sql
{
    public class InvokeDataverseSqlTests : E2ETestBase
    {
        [Fact]
        public void UnsupportedSqlFunctionError_SuggestsUseTdsEndpoint()
        {
            var script = GetConnectionScript(@"
$ErrorActionPreference = 'Stop'

try {
    Invoke-DataverseSql -Connection $connection -Sql 'SELECT DATEFROMPARTS(2024, 1, 1) AS d' -ErrorAction Stop | Out-Null
    throw 'Expected DATEFROMPARTS query to fail in default SQL4CDS mode, but it succeeded.'
}
catch {
    $message = $_.Exception.Message
    if ($message -notlike '*-UseTdsEndpoint*') {
        throw ""Expected error guidance mentioning -UseTdsEndpoint. Actual: $message""
    }

    Write-Host 'SUCCESS: -UseTdsEndpoint guidance present in query error'
}
");

            var result = RunScript(script);

            result.Success.Should().BeTrue($"Script should succeed. StdErr: {result.StandardError}\nStdOut: {result.StandardOutput}");
            result.StandardOutput.Should().Contain("SUCCESS", because: result.GetFullOutput());
        }
    }
}
