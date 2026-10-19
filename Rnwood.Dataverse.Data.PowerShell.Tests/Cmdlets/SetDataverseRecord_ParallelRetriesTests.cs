using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.ServiceModel;
using System.Threading;
using AwesomeAssertions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Parallel Retries tests for Set-DataverseRecord cmdlet.
    /// Uses interceptors to simulate transient failures for retry testing.
    /// </summary>
    public class SetDataverseRecord_ParallelRetriesTests : TestBase
    {
        private PS CreatePowerShellWithCmdlets()
        {
            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseRecord", typeof(SetDataverseRecordCmdlet), null));
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseRecord", typeof(GetDataverseRecordCmdlet), null));
            
            var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            
            var ps = PS.Create();
            ps.Runspace = runspace;
            return ps;
        }

        [Fact]
        public void SetDataverseRecord_ParallelRetries_RetriesFailedOperationsWithBatching()
        {
            var simulator = new TransientFailureSimulator(1, TransientFailureSimulator.ContainsCreateOrUpdate);
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection(simulator.Intercept, "contact");

            ps.AddScript(@"
                param($conn)
                $records = 1..4 | ForEach-Object { @{ firstname = ""RetryB$_""; lastname = ""User$_"" } }
                $records | Set-DataverseRecord -Connection $conn -TableName contact -MaxDegreeOfParallelism 2 -BatchSize 3 -Retries 1 -InitialRetryDelay 0
            ");
            ps.AddArgument(mockConnection);

            var results = ps.Invoke();

            ps.HadErrors.Should().BeFalse(string.Join(" | ", ps.Streams.Error.Select(e => e.ToString())));
            results.Should().BeEmpty();

            var created = Service!.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet("firstname") });
            created.Entities.Should().HaveCount(4);
            created.Entities.Should().OnlyContain(e => e.GetAttributeValue<string>("firstname").StartsWith("RetryB"));
        }

        [Fact]
        public void SetDataverseRecord_ParallelRetries_RetriesFailedOperationsWithoutBatching()
        {
            var simulator = new TransientFailureSimulator(1, TransientFailureSimulator.ContainsCreateOrUpdate);
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection(simulator.Intercept, "contact");

            ps.AddScript(@"
                param($conn)
                $records = 1..3 | ForEach-Object { @{ firstname = ""RetryNB$_""; lastname = ""User$_"" } }
                $records | Set-DataverseRecord -Connection $conn -TableName contact -MaxDegreeOfParallelism 2 -BatchSize 1 -Retries 1 -InitialRetryDelay 0
            ");
            ps.AddArgument(mockConnection);

            var results = ps.Invoke();

            ps.HadErrors.Should().BeFalse(string.Join(" | ", ps.Streams.Error.Select(e => e.ToString())));
            results.Should().BeEmpty();

            var created = Service!.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet("firstname") });
            created.Entities.Should().HaveCount(3);
            created.Entities.Should().OnlyContain(e => e.GetAttributeValue<string>("firstname").StartsWith("RetryNB"));
        }

        [Fact]
        public void SetDataverseRecord_ParallelRetries_ExhaustsRetriesAndReportsError()
        {
            var simulator = new TransientFailureSimulator(3, TransientFailureSimulator.ContainsCreateOrUpdate);
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection(simulator.Intercept, "contact");

            ps.AddScript(@"
                param($conn)
                $records = 1..2 | ForEach-Object { @{ firstname = ""RetryFail$_""; lastname = ""User$_"" } }
                $records | Set-DataverseRecord -Connection $conn -TableName contact -MaxDegreeOfParallelism 2 -BatchSize 1 -Retries 1 -InitialRetryDelay 0 -ErrorAction Continue
            ");
            ps.AddArgument(mockConnection);

            var results = ps.Invoke();

            ps.HadErrors.Should().BeTrue();
            ps.Streams.Error.Should().NotBeEmpty();
            results.Should().BeEmpty();

            var created = Service!.RetrieveMultiple(new QueryExpression("contact") { ColumnSet = new ColumnSet("firstname") });
            created.Entities.Count.Should().BeLessThan(2);
        }

        // Note: The main parallel functionality tests (without retry simulation) 
        // are in SetDataverseRecord_ParallelExecutionTests.cs and work with FakeXrmEasy
    }
}
