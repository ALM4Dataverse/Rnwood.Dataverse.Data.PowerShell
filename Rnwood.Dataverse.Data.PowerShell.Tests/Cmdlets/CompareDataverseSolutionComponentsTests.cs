using System;
using System.IO;
using System.IO.Compression;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;
using FluentAssertions;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets;

public class CompareDataverseSolutionComponentsTests : TestBase
{
    private static PS CreatePowerShellWithCmdlets()
    {
        var initialSessionState = InitialSessionState.CreateDefault();
        initialSessionState.Commands.Add(new SessionStateCmdletEntry(
            "Compare-DataverseSolutionComponents",
            typeof(CompareDataverseSolutionComponentsCmdlet),
            null));

        var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
        runspace.Open();

        var ps = PS.Create();
        ps.Runspace = runspace;
        return ps;
    }

    private static byte[] CreateSolutionZipBytes(string uniqueName)
    {
        var solutionXml = $"""
            <ImportExportXml>
              <SolutionManifest>
                <UniqueName>{uniqueName}</UniqueName>
                <RootComponents />
              </SolutionManifest>
            </ImportExportXml>
            """;

        using var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            var entry = archive.CreateEntry("solution.xml");
            using var writer = new StreamWriter(entry.Open(), Encoding.UTF8);
            writer.Write(solutionXml);
        }

        return memoryStream.ToArray();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FileComparisonsDoNotRequireADefaultConnection(bool useBytesToFile)
    {
        var sourcePath = Path.Combine(Path.GetTempPath(), $"compare-source-{Guid.NewGuid():N}.zip");
        var targetPath = Path.Combine(Path.GetTempPath(), $"compare-target-{Guid.NewGuid():N}.zip");
        var sourceBytes = CreateSolutionZipBytes("TestSolution");
        File.WriteAllBytes(sourcePath, sourceBytes);
        File.WriteAllBytes(targetPath, sourceBytes);

        try
        {
            DefaultConnectionManager.ClearDefaultConnection();
            using var ps = CreatePowerShellWithCmdlets();
            ps.AddCommand("Compare-DataverseSolutionComponents")
                .AddParameter("TargetSolutionFile", targetPath)
                .AddParameter("TestIfAdditive", true);

            if (useBytesToFile)
            {
                ps.AddParameter("SolutionBytes", sourceBytes)
                    .AddParameter("BytesToFile", true);
            }
            else
            {
                ps.AddParameter("SolutionFile", sourcePath)
                    .AddParameter("FileToFile", true);
            }

            var results = ps.Invoke();

            ps.HadErrors.Should().BeFalse(string.Join("; ", ps.Streams.Error));
            results.Should().ContainSingle();
            results[0].BaseObject.Should().Be(true);
        }
        finally
        {
            DefaultConnectionManager.ClearDefaultConnection();
            File.Delete(sourcePath);
            File.Delete(targetPath);
        }
    }
}
