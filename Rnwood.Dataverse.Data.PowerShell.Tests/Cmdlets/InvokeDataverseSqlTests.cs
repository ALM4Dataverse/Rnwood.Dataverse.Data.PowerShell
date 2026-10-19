using FluentAssertions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Tests for Invoke-DataverseSql cmdlet.
    /// Migrated from Pester tests in tests/Invoke-DataverseSql*.Tests.ps1 (13 tests)
    /// SQL4Cds converts SQL to QueryExpression/FetchXML which FakeXrmEasy can handle.
    /// </summary>
    public class InvokeDataverseSqlTests : TestBase
    {

        // ===== INSERT Operation Tests =====

        [Fact]
        public void InvokeDataverseSql_InsertStatement_CreatesRecord()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseSql", typeof(InvokeDataverseSqlCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            CreateMockConnection("contact");
            var sql4CdsConnection = MockSql4CdsServiceClientFactory.CreateForSql4Cds(Service!);

            // Act - Execute INSERT
            var sql = "INSERT INTO contact (firstname, lastname, emailaddress1) VALUES ('Test', 'User', 'test@example.com')";

            ps.AddCommand("Invoke-DataverseSql")
                .AddParameter("Connection", sql4CdsConnection)
                .AddParameter("Sql", sql);

            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();

            // Verify record was created
            var query = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("firstname", "lastname", "emailaddress1"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("lastname", ConditionOperator.Equal, "User")
                    }
                }
            };

            var results = Service!.RetrieveMultiple(query);
            results.Entities.Should().HaveCount(1);
            results.Entities[0].GetAttributeValue<string>("firstname").Should().Be("Test");
            results.Entities[0].GetAttributeValue<string>("emailaddress1").Should().Be("test@example.com");
        }

        [Fact]
        public void InvokeDataverseSql_ParameterizedInsertStatement_CreatesRecordWithBoundParameters()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseSql", typeof(InvokeDataverseSqlCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            CreateMockConnection("contact");
            var sql4CdsConnection = MockSql4CdsServiceClientFactory.CreateForSql4Cds(Service!);

            // Act - Execute parameterized INSERT
            var sql = "INSERT INTO contact (firstname, lastname, emailaddress1) VALUES (@fname, @lname, @email)";

            ps.AddScript(@"
                param($connection, $sql)
                $params = @{ 
                    fname = 'Parameterized'
                    lname = 'Insert'
                    email = 'param@example.com'
                }
                Invoke-DataverseSql -Connection $connection -Sql $sql -Parameters $params
            ")
            .AddParameter("connection", sql4CdsConnection)
            .AddParameter("sql", sql);

            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();

            var query = new QueryExpression("contact")
            {
                ColumnSet = new ColumnSet("firstname", "emailaddress1"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("lastname", ConditionOperator.Equal, "Insert")
                    }
                }
            };

            var results = Service!.RetrieveMultiple(query);
            results.Entities.Should().HaveCount(1);
            results.Entities[0].GetAttributeValue<string>("firstname").Should().Be("Parameterized");
        }

        // ===== UPDATE Operation Tests =====

        [Fact]
        public void InvokeDataverseSql_ParameterizedUpdateWithWhereClause_UpdatesSpecificRecord()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseSql", typeof(InvokeDataverseSqlCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            CreateMockConnection("contact");
            var sql4CdsConnection = MockSql4CdsServiceClientFactory.CreateForSql4Cds(Service!);

            // Create test record
            var recordId = Guid.NewGuid();
            Service!.Create(new Entity("contact")
            {
                Id = recordId,
                ["firstname"] = "OldName",
                ["lastname"] = "UpdateMe",
                ["emailaddress1"] = "old@example.com"
            });

            // Act - Execute parameterized UPDATE
            var sql = "UPDATE contact SET firstname = @newFirstName, emailaddress1 = @newEmail WHERE contactid = @targetId";

            ps.AddScript(@"
                param($connection, $sql, $recordId)
                $params = @{
                    newFirstName = 'NewName'
                    newEmail = 'new@example.com'
                    targetId = $recordId
                }
                Invoke-DataverseSql -Connection $connection -Sql $sql -Parameters $params
            ")
            .AddParameter("connection", sql4CdsConnection)
            .AddParameter("sql", sql)
            .AddParameter("recordId", recordId);

            ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();

            var updated = Service.Retrieve("contact", recordId, new ColumnSet(true));
            updated.GetAttributeValue<string>("firstname").Should().Be("NewName");
            updated.GetAttributeValue<string>("emailaddress1").Should().Be("new@example.com");
            updated.GetAttributeValue<string>("lastname").Should().Be("UpdateMe");
        }

        [Fact]
        public void InvokeDataverseSql_DataSourceNameOverride_AllowsSelectingFromNamedPrimaryDatasource()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseSql", typeof(InvokeDataverseSqlCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            CreateMockConnection("contact");
            var sql4CdsConnection = MockSql4CdsServiceClientFactory.CreateForSql4Cds(Service!);

            Service!.Create(new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Named",
                ["lastname"] = "Datasource"
            });

            // Act
            ps.AddCommand("Invoke-DataverseSql")
                .AddParameter("Connection", sql4CdsConnection)
                .AddParameter("DataSourceName", "main")
                .AddParameter("Sql", "SELECT TOP 1 firstname, lastname FROM main..contact");

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().ContainSingle();
            results[0].Properties["firstname"].Value.Should().Be("Named");
            results[0].Properties["lastname"].Value.Should().Be("Datasource");
        }

        [Fact]
        public void InvokeDataverseSql_AdditionalConnections_AllowsSelectingFromNamedAdditionalDatasource()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseSql", typeof(InvokeDataverseSqlCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            CreateMockConnection("contact");
            var primaryConnection = MockSql4CdsServiceClientFactory.CreateForSql4Cds(Service!);
            var secondaryConnection = MockSql4CdsServiceClientFactory.CreateForSql4Cds(Service!);

            Service!.Create(new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Secondary",
                ["lastname"] = "Datasource"
            });

            var additionalConnections = new Hashtable
            {
                ["secondary"] = secondaryConnection
            };

            // Act
            ps.AddCommand("Invoke-DataverseSql")
                .AddParameter("Connection", primaryConnection)
                .AddParameter("AdditionalConnections", additionalConnections)
                .AddParameter("Sql", "SELECT TOP 1 firstname, lastname FROM secondary..contact");

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().ContainSingle();
            results[0].Properties["firstname"].Value.Should().Be("Secondary");
            results[0].Properties["lastname"].Value.Should().Be("Datasource");
        }

        [Fact]
        public void InvokeDataverseSql_CrossDatasourceQueryWithAdditionalConnections_ReturnsResults()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseSql", typeof(InvokeDataverseSqlCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            CreateMockConnection("contact");
            var primaryConnection = MockSql4CdsServiceClientFactory.CreateForSql4Cds(Service!);
            var secondaryConnection = MockSql4CdsServiceClientFactory.CreateForSql4Cds(Service!);

            Service!.Create(new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "Cross",
                ["lastname"] = "Datasource"
            });

            var additionalConnections = new Hashtable
            {
                ["secondary"] = secondaryConnection
            };

            // Act
            ps.AddCommand("Invoke-DataverseSql")
                .AddParameter("Connection", primaryConnection)
                .AddParameter("DataSourceName", "main")
                .AddParameter("AdditionalConnections", additionalConnections)
                .AddParameter("Sql", "SELECT TOP 1 p.firstname AS primary_name, s.firstname AS secondary_name FROM main..contact p CROSS JOIN secondary..contact s");

            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().ContainSingle();
            results[0].Properties["primary_name"].Value.Should().Be("Cross");
            results[0].Properties["secondary_name"].Value.Should().Be("Cross");
        }

        [Fact]
        public void InvokeDataverseSql_AdditionalConnections_InvalidValueType_ThrowsHelpfulError()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseSql", typeof(InvokeDataverseSqlCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            CreateMockConnection("contact");
            var primaryConnection = MockSql4CdsServiceClientFactory.CreateForSql4Cds(Service!);

            var additionalConnections = new Hashtable
            {
                ["invalid"] = "not a connection"
            };

            // Act
            ps.AddCommand("Invoke-DataverseSql")
                .AddParameter("Connection", primaryConnection)
                .AddParameter("AdditionalConnections", additionalConnections)
                .AddParameter("Sql", "SELECT TOP 1 firstname FROM contact");

            Action invoke = () => ps.Invoke();

            // Assert
            invoke.Should().Throw<CmdletInvocationException>()
                .WithMessage("*must be a ServiceClient or IOrganizationService instance*");
        }

        [Fact]
        public void InvokeDataverseSql_UnsupportedBuiltInFunction_WithoutTdsEndpoint_IncludesGuidance()
        {
            // Arrange
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Invoke-DataverseSql", typeof(InvokeDataverseSqlCmdlet), null));

            using var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            using var ps = PS.Create();
            ps.Runspace = runspace;

            CreateMockConnection("contact");
            var sql4CdsConnection = MockSql4CdsServiceClientFactory.CreateForSql4Cds(Service!);

            // Act
            ps.AddCommand("Invoke-DataverseSql")
                .AddParameter("Connection", sql4CdsConnection)
                .AddParameter("Sql", "SELECT DATEFROMPARTS(2024, 1, 1) AS d");

            Action invoke = () => ps.Invoke();

            // Assert
            invoke.Should().Throw<CmdletInvocationException>()
                .WithMessage("*-UseTdsEndpoint*");
        }
    }
}
