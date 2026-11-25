using System;
using System.Collections;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using AwesomeAssertions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// Link entity tests for Get-DataverseRecord cmdlet.
    /// Tests for joining related tables using Links parameter.
    /// </summary>
    [Collection("LinkEntity")]
    public class GetDataverseRecord_LinkEntityTests : TestBase
    {
        private PS CreatePowerShellWithCmdlets()
        {
            var initialSessionState = InitialSessionState.CreateDefault2();
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Get-DataverseRecord", typeof(GetDataverseRecordCmdlet), null));
            initialSessionState.Commands.Add(new SessionStateCmdletEntry(
                "Set-DataverseRecord", typeof(SetDataverseRecordCmdlet), null));
            
            var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            runspace.Open();
            
            var ps = PS.Create();
            ps.Runspace = runspace;
            return ps;
        }

        [Fact]
        public void GetDataverseRecord_Links_SdkLinkEntityObject()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact", "account");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "John", ["lastname"] = "Doe" });
            
            // Act - Create a LinkEntity to test the existing syntax
            var linkEntity = new LinkEntity
            {
                LinkFromEntityName = "contact",
                LinkToEntityName = "account",
                LinkFromAttributeName = "accountid",
                LinkToAttributeName = "accountid",
                JoinOperator = JoinOperator.Inner
            };
            
            // This should work without error - the mock doesn't fully support links but we can test it doesn't throw
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("Links", linkEntity);
            
            ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void GetDataverseRecord_Links_SimplifiedHashtableSyntax()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact", "account");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "John", ["lastname"] = "Doe" });
            
            // Act - Test simplified syntax
            var simplifiedLink = new Hashtable
            {
                ["contact.accountid"] = "account.accountid"
            };
            
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("Links", simplifiedLink);
            
            ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void GetDataverseRecord_Links_SimplifiedSyntax_WithType()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact", "account");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "John", ["lastname"] = "Doe" });
            
            // Act
            var simplifiedLink = new Hashtable
            {
                ["contact.accountid"] = "account.accountid",
                ["type"] = "LeftOuter"
            };
            
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("Links", simplifiedLink);
            
            ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void GetDataverseRecord_Links_SimplifiedSyntax_WithAlias()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact", "account");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "John", ["lastname"] = "Doe" });
            
            // Act
            var simplifiedLink = new Hashtable
            {
                ["contact.accountid"] = "account.accountid",
                ["alias"] = "linkedAccount"
            };
            
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("Links", simplifiedLink);
            
            ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void GetDataverseRecord_Links_SimplifiedSyntax_WithFilter()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact", "account");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "John", ["lastname"] = "Doe" });
            
            // Act
            var simplifiedLink = new Hashtable
            {
                ["contact.accountid"] = "account.accountid",
                ["filter"] = new Hashtable
                {
                    ["name"] = new Hashtable { ["operator"] = "Like", ["value"] = "Contoso%" },
                    ["statecode"] = new Hashtable { ["operator"] = "Equal", ["value"] = 0 }
                }
            };
            
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("Links", simplifiedLink);
            
            ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void GetDataverseRecord_Links_MultipleSimplifiedLinks()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact", "account", "systemuser");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "John", ["lastname"] = "Doe" });
            
            // Act
            var links = new object[]
            {
                new Hashtable
                {
                    ["contact.accountid"] = "account.accountid",
                    ["type"] = "LeftOuter"
                },
                new Hashtable
                {
                    ["contact.ownerid"] = "systemuser.systemuserid"
                }
            };
            
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("Links", links);
            
            ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void GetDataverseRecord_Links_NestedLinks_Array()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact", "account", "systemuser");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "John", ["lastname"] = "Doe" });
            
            // Act
            var links = new object[]
            {
                new Hashtable
                {
                    ["contact.accountid"] = "account.accountid",
                    ["links"] = new object[]
                    {
                        new Hashtable
                        {
                            ["account.ownerid"] = "systemuser.systemuserid",
                            ["type"] = "LeftOuter",
                            ["alias"] = "owner"
                        }
                    }
                }
            };
            
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("Links", links);
            
            ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
        }

        [Fact]
        public void GetDataverseRecord_Links_NestedLinks_SingleHashtable()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact", "account", "systemuser");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "John", ["lastname"] = "Doe" });
            
            // Act
            var links = new object[]
            {
                new Hashtable
                {
                    ["contact.accountid"] = "account.accountid",
                    ["links"] = new Hashtable
                    {
                        ["account.ownerid"] = "systemuser.systemuserid",
                        ["type"] = "Inner"
                    }
                }
            };
            
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("Links", links);
            
            ps.Invoke();
            
            // Assert
            ps.HadErrors.Should().BeFalse();
        }
    }
}
