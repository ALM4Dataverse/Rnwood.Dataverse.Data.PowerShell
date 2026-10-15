using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Commands.Model;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets;

public class CompressDataverseSolutionFileTests
{
    [Fact]
    public void BuildPackArguments_IncludesMappingFileWhenSpecified()
    {
        var arguments = CompressDataverseSolutionFileCmdlet.BuildPackArguments(
            @"C:\Solutions\My Solution.zip",
            @"C:\Solutions\My Solution",
            SolutionPackageType.Both,
            @"C:\Solutions\My Solution\mapping.xml");

        Assert.Equal(
            "solution pack --zipfile \"C:\\Solutions\\My Solution.zip\" --folder \"C:\\Solutions\\My Solution\" --packagetype Both --map \"C:\\Solutions\\My Solution\\mapping.xml\"",
            arguments);
    }

    [Fact]
    public void BuildPackArguments_OmitsMappingFileWhenNotSpecified()
    {
        var arguments = CompressDataverseSolutionFileCmdlet.BuildPackArguments(
            @"C:\Solutions\MySolution.zip",
            @"C:\Solutions\MySolution",
            SolutionPackageType.Unmanaged,
            null);

        Assert.Equal(
            "solution pack --zipfile \"C:\\Solutions\\MySolution.zip\" --folder \"C:\\Solutions\\MySolution\" --packagetype Unmanaged",
            arguments);
    }
}
