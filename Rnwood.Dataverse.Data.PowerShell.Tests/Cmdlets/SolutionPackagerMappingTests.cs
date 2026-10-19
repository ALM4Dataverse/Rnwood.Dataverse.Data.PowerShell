using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Commands.Model;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets;

public class SolutionPackagerMappingTests
{
    [Fact]
    public void BuildSolutionUnpackArguments_IncludesMappingFileWhenSpecified()
    {
        var arguments = PacCliHelper.BuildSolutionUnpackArguments(
            @"C:\Solutions\My Solution.zip",
            @"C:\Solutions\My Solution",
            SolutionPackageType.Both,
            @"C:\Solutions\My Solution\mapping.xml");

        Assert.Equal(
            "solution unpack --zipfile \"C:\\Solutions\\My Solution.zip\" --folder \"C:\\Solutions\\My Solution\" --packagetype Both --clobber --allowDelete --map \"C:\\Solutions\\My Solution\\mapping.xml\"",
            arguments);
    }

    [Fact]
    public void BuildSolutionUnpackArguments_OmitsMappingFileWhenNotSpecified()
    {
        var arguments = PacCliHelper.BuildSolutionUnpackArguments(
            @"C:\Solutions\MySolution.zip",
            @"C:\Solutions\MySolution",
            SolutionPackageType.Unmanaged,
            null);

        Assert.Equal(
            "solution unpack --zipfile \"C:\\Solutions\\MySolution.zip\" --folder \"C:\\Solutions\\MySolution\" --packagetype Unmanaged --clobber --allowDelete",
            arguments);
    }
}
