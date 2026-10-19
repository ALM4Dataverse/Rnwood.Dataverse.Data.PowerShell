using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using FluentAssertions;
using Microsoft.Xrm.Sdk;
using Rnwood.Dataverse.Data.PowerShell.Commands;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using Xunit;
using PS = System.Management.Automation.PowerShell;

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets
{
    /// <summary>
    /// PassThru Uniformity tests for Set-DataverseRecord cmdlet.
    /// Migrated from Pester tests in tests/Set-DataverseRecord-PassThruUniformity.Tests.ps1
    /// </summary>
    public class SetDataverseRecord_PassThruUniformityTests : TestBase
    {
        private PS CreatePowerShellWithCmdlets()
        {
            var initialSessionState = InitialSessionState.CreateDefault2();
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
        public void SetDataverseRecord_PassThru_ReturnsPSObject_FromHashtable()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var hashtableInput = new Hashtable {
                ["firstname"] = "John",
                ["lastname"] = "Doe",
                ["emailaddress1"] = "john@example.com"
            };
            
            // Act
            var inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(hashtableInput));
            inputCollection.Complete();
            
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var results = ps.Invoke(inputCollection);

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            
            var result = results[0];
            result.Should().NotBeNull();
            result.BaseObject.GetType().Name.Should().Be("PSCustomObject");
            
            var id = (Guid)result.Properties["Id"].Value;
            id.Should().NotBe(Guid.Empty);
            result.Properties["firstname"].Value.Should().Be("John");
            result.Properties["lastname"].Value.Should().Be("Doe");
            result.Properties["emailaddress1"].Value.Should().Be("john@example.com");
        }

        [Fact]
        public void SetDataverseRecord_PassThru_ReturnsPSObject_FromEntity()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var entityInput = new Entity("contact");
            entityInput["firstname"] = "Jane";
            entityInput["lastname"] = "Smith";
            entityInput["emailaddress1"] = "jane@example.com";
            
            // Act
            var inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(entityInput));
            inputCollection.Complete();
            
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var results = ps.Invoke(inputCollection);

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            
            var result = results[0];
            result.Should().NotBeNull();
            result.BaseObject.GetType().Name.Should().Be("PSCustomObject");
            
            var id = (Guid)result.Properties["Id"].Value;
            id.Should().NotBe(Guid.Empty);
            result.Properties["TableName"].Value.Should().Be("contact");
            result.Properties["firstname"].Value.Should().Be("Jane");
            result.Properties["lastname"].Value.Should().Be("Smith");
            result.Properties["emailaddress1"].Value.Should().Be("jane@example.com");
        }

        [Fact]
        public void SetDataverseRecord_PassThru_PreservesAllProperties()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var hashtableInput = new Hashtable {
                ["firstname"] = "Test",
                ["lastname"] = "User",
                ["emailaddress1"] = "test@example.com",
                ["telephone1"] = "555-1234",
                ["description"] = "Test description"
            };
            
            // Act
            var inputCollection = new PSDataCollection<PSObject>();
            inputCollection.Add(PSObject.AsPSObject(hashtableInput));
            inputCollection.Complete();
            
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var results = ps.Invoke(inputCollection);

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            
            var result = results[0];
            result.Properties["firstname"].Value.Should().Be("Test");
            result.Properties["lastname"].Value.Should().Be("User");
            result.Properties["emailaddress1"].Value.Should().Be("test@example.com");
            result.Properties["telephone1"].Value.Should().Be("555-1234");
            result.Properties["description"].Value.Should().Be("Test description");
            result.Properties["Id"].Value.Should().BeOfType<Guid>();
        }

        [Fact]
        public void SetDataverseRecord_PassThru_BatchOperations()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var inputs = new object[] {
                new Hashtable { ["firstname"] = "Hash1", ["lastname"] = "User1" },
                PSObject.AsPSObject(new { TableName = "contact", firstname = "PS1", lastname = "User2" })
            };
            
            // Act
            var inputCollection = new PSDataCollection<PSObject>();
            foreach (var input in inputs)
                inputCollection.Add(PSObject.AsPSObject(input));
            inputCollection.Complete();
            
            ps.AddCommand("Set-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("CreateOnly", true)
              .AddParameter("PassThru", true);
            var results = ps.Invoke(inputCollection);

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(2);
            
            foreach (var result in results)
            {
                result.BaseObject.GetType().Name.Should().Be("PSCustomObject");
                var id = (Guid)result.Properties["Id"].Value;
                id.Should().NotBe(Guid.Empty);
            }
        }
    }
}
