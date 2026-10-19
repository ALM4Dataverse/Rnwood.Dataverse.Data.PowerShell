using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    /// Basic tests for Get-DataverseRecord cmdlet.
    /// Tests for type conversion, filtering, paging, ordering, and basic query parameters.
    /// </summary>
    public class GetDataverseRecord_BasicTests : TestBase
    {
        private PS CreatePowerShellWithCmdlets()
        {
            var initialSessionState = InitialSessionState.CreateDefault();
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

        #region Type Conversion Tests

        [Fact]
        public void GetDataverseRecord_ConvertsToPSObjectWithNativeTypes()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            var parentContactId = Guid.NewGuid();
            
            var testContact = new Entity("contact")
            {
                Id = Guid.NewGuid(),
                ["firstname"] = "text",
                ["birthdate"] = DateTime.Today,
                ["accountrolecode"] = new OptionSetValue(2),
                ["parentcontactid"] = new EntityReference("contact", parentContactId)
            };
            Service!.Create(testContact);
            
            // Act
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "contactid", "firstname", "birthdate", "accountrolecode", "parentcontactid" });
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            var result = results[0];
            
            // UniqueIdentifier
            result.Properties["contactid"].Value.Should().BeOfType<Guid>();
            result.Properties["contactid"].Value.Should().Be(testContact.Id);
            
            // Text
            result.Properties["firstname"].Value.Should().BeOfType<string>();
            result.Properties["firstname"].Value.Should().Be("text");
            
            // Date
            result.Properties["birthdate"].Value.Should().BeOfType<DateTime>();
            ((DateTime)result.Properties["birthdate"].Value).Date.Should().Be(DateTime.Today);
            
            // Choice (OptionSetValue)
            result.Properties["accountrolecode"].Value.Should().BeOfType<int>();
            result.Properties["accountrolecode"].Value.Should().Be(2);
            
            // Lookup (EntityReference)
            result.Properties["parentcontactid"].Value.Should().BeOfType<DataverseEntityReference>();
            var lookupValue = (DataverseEntityReference)result.Properties["parentcontactid"].Value;
            lookupValue.Id.Should().Be(parentContactId);
            lookupValue.TableName.Should().Be("contact");
        }

        [Fact]
        public void GetDataverseRecord_ConvertsToPSObjectWithIdAndTableNameImplicitProps()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var testContactId = Guid.NewGuid();
            var testContact = new Entity("contact") { Id = testContactId };
            Service!.Create(testContact);
            
            // Act
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "contactid" });
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            var result = results[0];
            
            result.Properties["Id"].Value.Should().BeOfType<Guid>();
            result.Properties["Id"].Value.Should().Be(testContactId);
            result.Properties["TableName"].Value.Should().Be("contact");
        }

        #endregion

        #region Filter Tests

        [Fact]
        public void GetDataverseRecord_FilterWithNoOperator_UsesEquals()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create 10 test contacts
            for (int i = 1; i <= 10; i++)
            {
                var contact = new Entity("contact") { ["firstname"] = i.ToString() };
                Service!.Create(contact);
            }
            
            // Act - Filter for firstname = "1"
            var filter = new Hashtable { ["firstname"] = "1" };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname" })
              .AddParameter("Filter", filter);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["firstname"].Value.Should().Be("1");
        }

        [Fact]
        public void GetDataverseRecord_FilterWithExplicitOperator_DeprecatedSyntax()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            for (int i = 1; i <= 10; i++)
            {
                var contact = new Entity("contact") { ["firstname"] = i.ToString() };
                Service!.Create(contact);
            }
            
            // Act - Filter with deprecated syntax "firstname:Like"="1%"
            var filter = new Hashtable { ["firstname:Like"] = "1%" };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname" })
              .AddParameter("Filter", filter);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(2); // Matches "1" and "10"
        }

        [Fact]
        public void GetDataverseRecord_FilterWithExplicitOperator_NewSyntax()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            for (int i = 1; i <= 10; i++)
            {
                var contact = new Entity("contact") { ["firstname"] = i.ToString() };
                Service!.Create(contact);
            }
            
            // Act - Filter with new syntax @{"operator"="Like"; "value"="1%"}
            var operatorHashtable = new Hashtable { ["operator"] = "Like", ["value"] = "1%" };
            var filter = new Hashtable { ["firstname"] = operatorHashtable };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname" })
              .AddParameter("Filter", filter);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(2);
        }

        [Fact]
        public void GetDataverseRecord_FilterWithOperatorNotRequiringValue_DeprecatedSyntax()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            for (int i = 1; i <= 10; i++)
            {
                var contact = new Entity("contact") { ["firstname"] = i.ToString() };
                Service!.Create(contact);
            }
            
            // Act - Filter for null values using deprecated syntax
            var filter = new Hashtable { ["firstname:Null"] = "" };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname" })
              .AddParameter("Filter", filter);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(0); // No contacts with null firstname
        }

        [Fact]
        public void GetDataverseRecord_FilterWithOperatorNotRequiringValue_NewSyntax()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            for (int i = 1; i <= 10; i++)
            {
                var contact = new Entity("contact") { ["firstname"] = i.ToString() };
                Service!.Create(contact);
            }
            
            // Act - Filter for null values using new syntax
            var operatorHashtable = new Hashtable { ["operator"] = "Null" };
            var filter = new Hashtable { ["firstname"] = operatorHashtable };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname" })
              .AddParameter("Filter", filter);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(0);
        }

        [Fact]
        public void GetDataverseRecord_FilterWithImplicitNullOperator()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            for (int i = 1; i <= 10; i++)
            {
                var contact = new Entity("contact") { ["firstname"] = i.ToString() };
                Service!.Create(contact);
            }
            
            // Create one contact with empty string firstname
            var emptyContact = new Entity("contact") { ["firstname"] = "" };
            Service!.Create(emptyContact);
            
            // Act - Filter for empty string (PowerShell "$null" interpolates to "")
            var filter = new Hashtable { ["firstname"] = "" };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname" })
              .AddParameter("Filter", filter);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
        }

        [Fact]
        public void GetDataverseRecord_FilterWithInOperator()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            for (int i = 1; i <= 10; i++)
            {
                var contact = new Entity("contact") { ["firstname"] = i.ToString() };
                Service!.Create(contact);
            }
            
            // Act - Filter with IN operator
            var values = new object[] { "1", "2" };
            var operatorHashtable = new Hashtable { ["operator"] = "In", ["value"] = values };
            var filter = new Hashtable { ["firstname"] = operatorHashtable };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname" })
              .AddParameter("Filter", filter);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(2);
        }

        [Fact]
        public void GetDataverseRecord_FilterValues_SingleColumn()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create test data
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Joe", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "Two" });
            
            // Act
            var filter = new Hashtable { ["firstname"] = "Rob" };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname" })
              .AddParameter("Filter", filter);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(2);
            results.All(r => r.Properties["firstname"].Value.Equals("Rob")).Should().BeTrue();
        }

        [Fact]
        public void GetDataverseRecord_FilterValues_MultipleColumns_AndLogic()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create test data
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Joe", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "Two" });
            
            // Act - Multiple conditions in same hashtable = AND
            var filter = new Hashtable { ["lastname"] = "One", ["firstname"] = "Joe" };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("Filter", filter);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["firstname"].Value.Should().Be("Joe");
        }

        [Fact]
        public void GetDataverseRecord_FilterValues_MultipleHashtables_OrLogic()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            // Create test data
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Joe", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "Two" });
            
            // Act - Multiple filter hashtables = OR
            var filter = new object[] {
                new Hashtable { ["lastname"] = "One", ["firstname"] = "Rob" },
                new Hashtable { ["firstname"] = "Joe" }
            };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("Filter", filter);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(2);
            var firstnames = results.Select(r => r.Properties["firstname"].Value).ToList();
            firstnames.Should().Contain("Rob");
            firstnames.Should().Contain("Joe");
        }

        #endregion

        #region Filter Grouping Tests

        [Fact]
        public void GetDataverseRecord_GroupedFilterValues_AndKey()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Joe", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "Two" });
            
            // Act - Use 'and' grouping hashtable
            var andGroup = new Hashtable {
                ["and"] = new object[] {
                    new Hashtable { ["firstname"] = "Rob" },
                    new Hashtable { ["lastname"] = "One" }
                }
            };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("FilterValues", andGroup);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["firstname"].Value.Should().Be("Rob");
            results[0].Properties["lastname"].Value.Should().Be("One");
        }

        [Fact]
        public void GetDataverseRecord_NestedGroupedFilterValues_OrInsideAnd()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Joe", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Mary", ["lastname"] = "Two" });
            
            // Act - (firstname=Rob OR firstname=Joe) AND lastname=One
            var filter = new Hashtable {
                ["and"] = new object[] {
                    new Hashtable { 
                        ["or"] = new object[] {
                            new Hashtable { ["firstname"] = "Rob" },
                            new Hashtable { ["firstname"] = "Joe" }
                        }
                    },
                    new Hashtable { ["lastname"] = "One" }
                }
            };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("FilterValues", filter);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(2);
            results.Select(r => r.Properties["firstname"].Value).Should().BeEquivalentTo(new[] { "Rob", "Joe" });
        }

        [Fact]
        public void GetDataverseRecord_GroupedExcludeFilterValues()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Joe", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Mary", ["lastname"] = "Two" });
            
            // Act - Exclude (firstname=Rob OR lastname=Two)
            var excludeFilter = new Hashtable {
                ["or"] = new object[] {
                    new Hashtable { ["firstname"] = "Rob" },
                    new Hashtable { ["lastname"] = "Two" }
                }
            };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("ExcludeFilterValues", excludeFilter);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["firstname"].Value.Should().Be("Joe");
        }

        [Fact]
        public void GetDataverseRecord_NotGrouping_SingleField()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Joe", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Mary", ["lastname"] = "Two" });
            
            // Act - NOT(firstname=Rob)
            var filter = new Hashtable {
                ["not"] = new Hashtable { ["firstname"] = "Rob" }
            };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("FilterValues", filter);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(2);
            results.Select(r => r.Properties["firstname"].Value).Should().BeEquivalentTo(new[] { "Joe", "Mary" });
        }

        [Fact]
        public void GetDataverseRecord_NotGrouping_MultiField()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Joe", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Mary", ["lastname"] = "Two" });
            
            // Act - NOT(firstname=Rob AND lastname=One)
            var filter = new Hashtable {
                ["not"] = new Hashtable { ["firstname"] = "Rob", ["lastname"] = "One" }
            };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("FilterValues", filter);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(2);
            results.Select(r => r.Properties["firstname"].Value).Should().BeEquivalentTo(new[] { "Joe", "Mary" });
        }

        [Fact]
        public void GetDataverseRecord_NotWrappingOr()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Joe", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Mary", ["lastname"] = "Two" });
            
            // Act - NOT(firstname=Rob OR firstname=Joe)
            var filter = new Hashtable {
                ["not"] = new Hashtable {
                    ["or"] = new object[] {
                        new Hashtable { ["firstname"] = "Rob" },
                        new Hashtable { ["firstname"] = "Joe" }
                    }
                }
            };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("FilterValues", filter);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["firstname"].Value.Should().Be("Mary");
        }

        [Fact]
        public void GetDataverseRecord_NotInsideOtherGroup()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Joe", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Mary", ["lastname"] = "One" });
            
            // Act - (NOT firstname=Rob) AND lastname=One
            var filter = new Hashtable {
                ["and"] = new object[] {
                    new Hashtable { ["not"] = new Hashtable { ["firstname"] = "Rob" } },
                    new Hashtable { ["lastname"] = "One" }
                }
            };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("FilterValues", filter);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(2);
            results.Select(r => r.Properties["firstname"].Value).Should().BeEquivalentTo(new[] { "Joe", "Mary" });
        }

        [Fact]
        public void GetDataverseRecord_XorGrouping()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Joe", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Mary", ["lastname"] = "Two" });
            
            // Act - XOR(firstname=Rob, firstname=Joe)
            var filter = new Hashtable {
                ["xor"] = new object[] {
                    new Hashtable { ["firstname"] = "Rob" },
                    new Hashtable { ["firstname"] = "Joe" }
                }
            };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("FilterValues", filter);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(2);
            results.Select(r => r.Properties["firstname"].Value).Should().BeEquivalentTo(new[] { "Rob", "Joe" });
        }

        [Fact]
        public void GetDataverseRecord_XorInExcludeFilterValues()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Joe", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Mary", ["lastname"] = "Two" });
            
            // Act - Exclude XOR(firstname=Rob, firstname=Joe)
            var excludeFilter = new Hashtable {
                ["xor"] = new object[] {
                    new Hashtable { ["firstname"] = "Rob" },
                    new Hashtable { ["firstname"] = "Joe" }
                }
            };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("ExcludeFilterValues", excludeFilter);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["firstname"].Value.Should().Be("Mary");
        }

        [Fact]
        public void GetDataverseRecord_XorNestedInGroups()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Joe", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Mary", ["lastname"] = "One" });
            
            // Act - (XOR(firstname=Rob, firstname=Joe) AND lastname=One)
            var filter = new Hashtable {
                ["and"] = new object[] {
                    new Hashtable { 
                        ["xor"] = new object[] {
                            new Hashtable { ["firstname"] = "Rob" },
                            new Hashtable { ["firstname"] = "Joe" }
                        }
                    },
                    new Hashtable { ["lastname"] = "One" }
                }
            };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("FilterValues", filter);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(2);
            results.Select(r => r.Properties["firstname"].Value).Should().BeEquivalentTo(new[] { "Rob", "Joe" });
        }

        #endregion

        #region Exclude Filter Tests

        [Fact]
        public void GetDataverseRecord_ExcludeFilterValues_WithNoOperator()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Joe", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "Two" });
            
            // Act
            var excludeFilter = new Hashtable { ["firstname"] = "Rob" };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("ExcludeFilterValues", excludeFilter);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["firstname"].Value.Should().Be("Joe");
        }

        [Fact]
        public void GetDataverseRecord_MultipleExcludeFilterValues_OrSemantics()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Joe", ["lastname"] = "Two" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "Three" });
            
            // Act - Exclude (lastname=One OR lastname=Three)
            var excludeFilter = new object[] {
                new Hashtable { ["lastname"] = "One" },
                new Hashtable { ["lastname"] = "Three" }
            };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("ExcludeFilterValues", excludeFilter);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["firstname"].Value.Should().Be("Joe");
        }

        [Fact]
        public void GetDataverseRecord_ExcludeFilterValues_WithOperator()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Joe", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "Two" });
            
            // Act
            var operatorHashtable = new Hashtable { ["operator"] = "Equal", ["value"] = "Rob" };
            var excludeFilter = new Hashtable { ["firstname"] = operatorHashtable };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("ExcludeFilterValues", excludeFilter);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["firstname"].Value.Should().Be("Joe");
        }

        [Fact]
        public void GetDataverseRecord_ExcludeFilter_DefaultAndSemantics()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Joe", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "Two" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Mary", ["lastname"] = "Two" });
            
            // Act - Exclude using AND: (lastname=One AND firstname=Rob)
            var excludeFilter = new object[] {
                new Hashtable { ["lastname"] = "One" },
                new Hashtable { ["firstname"] = "Rob" }
            };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("ExcludeFilter", excludeFilter);
            var results = ps.Invoke();

            // Assert - Should exclude only Rob One -> leaves Joe One, Rob Two, Mary Two
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["firstname"].Value.Should().Be("Mary");
        }

        [Fact]
        public void GetDataverseRecord_IncludeAndExcludeFilter_Together()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Joe", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "Two" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Mary", ["lastname"] = "Two" });
            
            // Act - Include (lastname=One OR firstname=Rob), then Exclude (firstname=Rob)
            var includeFilter = new object[] {
                new Hashtable { ["lastname"] = "One" },
                new Hashtable { ["firstname"] = "Rob" }
            };
            var excludeFilter = new Hashtable { ["firstname"] = "Rob" };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("IncludeFilter", includeFilter)
              .AddParameter("ExcludeFilter", excludeFilter);
            var results = ps.Invoke();

            // Assert - Include gives Rob One, Joe One, Rob Two. Exclude removes Rob One and Rob Two. Result: Joe One
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(1);
            results[0].Properties["firstname"].Value.Should().Be("Joe");
        }

        [Fact]
        public void GetDataverseRecord_IncludeFilter_MultipleHashtables_OrSemantics()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Joe", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "Two" });
            
            // Act - Include (lastname=One OR firstname=Joe) using OR semantics
            var includeFilter = new object[] {
                new Hashtable { ["lastname"] = "One" },
                new Hashtable { ["firstname"] = "Joe" }
            };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("IncludeFilter", includeFilter);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(2);
            results.Select(r => r.Properties["firstname"].Value).Should().BeEquivalentTo(new[] { "Rob", "Joe" });
        }

        [Fact]
        public void GetDataverseRecord_ExcludeFilter_WithExcludeFilterOr()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Joe", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "Two" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Mary", ["lastname"] = "Two" });
            
            // Act - Exclude using OR: (lastname=One OR firstname=Rob) 
            // This should exclude: Rob One, Joe One, Rob Two -> leaves Mary Two
            var excludeFilter = new object[] {
                new Hashtable { ["lastname"] = "One" },
                new Hashtable { ["firstname"] = "Rob" }
            };
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("ExcludeFilter", excludeFilter)
              .AddParameter("ExcludeFilterOr", true);
            var results = ps.Invoke();

            // Assert - Should return all 3: Joe One, Rob Two, Mary Two (NOT matching the exclude)
            // Order is not guaranteed, so check contents rather than positions
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(3);
            var firstnames = results.Select(r => (string)r.Properties["firstname"].Value).ToList();
            firstnames.Should().BeEquivalentTo(new[] { "Joe", "Rob", "Mary" });
            var robTwo = results.First(r => (string)r.Properties["firstname"].Value == "Rob");
            ((string)robTwo.Properties["lastname"].Value).Should().Be("Two");
        }

        #endregion

        #region Paging and Ordering Tests

        [Fact]
        public void GetDataverseRecord_NoFilters_GetsAllRecordsBeyondPageLimit()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            for (int i = 1; i <= 10; i++)
            {
                var contact = new Entity("contact") { ["firstname"] = i.ToString() };
                Service!.Create(contact);
            }
            
            // Act - Get all records with small page size
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname" })
              .AddParameter("PageSize", 2);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(10); // Should get all records despite PageSize=2
        }

        [Fact]
        public void GetDataverseRecord_TopParameter_LimitsResults()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            for (int i = 9; i >= 1; i--) // Create in reverse order
            {
                var contact = new Entity("contact") { ["firstname"] = i.ToString() };
                Service!.Create(contact);
            }
            
            // Act - Get top 5 ordered by firstname
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname" })
              .AddParameter("Top", 5)
              .AddParameter("OrderBy", new[] { "firstname" });
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(5);
            for (int i = 0; i < 5; i++)
            {
                results[i].Properties["firstname"].Value.Should().Be((i + 1).ToString());
            }
        }

        [Fact]
        public void GetDataverseRecord_OrderByAscending()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            for (int i = 9; i >= 1; i--)
            {
                var contact = new Entity("contact") { ["firstname"] = i.ToString() };
                Service!.Create(contact);
            }
            
            // Act - OrderBy firstname ascending
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname" })
              .AddParameter("OrderBy", new[] { "firstname" });
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(9);
            for (int i = 0; i < 9; i++)
            {
                results[i].Properties["firstname"].Value.Should().Be((i + 1).ToString());
            }
        }

        [Fact]
        public void GetDataverseRecord_OrderByAscending_WithTop()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            for (int i = 9; i >= 1; i--)
            {
                var contact = new Entity("contact") { ["firstname"] = i.ToString() };
                Service!.Create(contact);
            }
            
            // Act
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname" })
              .AddParameter("OrderBy", new[] { "firstname" })
              .AddParameter("Top", 5);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(5);
            for (int i = 0; i < 5; i++)
            {
                results[i].Properties["firstname"].Value.Should().Be((i + 1).ToString());
            }
        }

        [Fact]
        public void GetDataverseRecord_OrderByDescending_WithMinus()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            for (int i = 1; i <= 9; i++)
            {
                var contact = new Entity("contact") { ["firstname"] = i.ToString() };
                Service!.Create(contact);
            }
            
            // Act - OrderBy with minus suffix for descending
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname" })
              .AddParameter("OrderBy", new[] { "firstname-" })
              .AddParameter("Top", 5);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(5);
            for (int i = 0; i < 5; i++)
            {
                results[i].Properties["firstname"].Value.Should().Be((9 - i).ToString());
            }
        }

        [Fact]
        public void GetDataverseRecord_OrderByMultipleColumns_Ascending()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var letters = new[] { "a", "b", "c" };
            foreach (var letter in letters)
            {
                for (int i = 3; i >= 1; i--) // Create in reverse order
                {
                    var contact = new Entity("contact")
                    {
                        ["firstname"] = i.ToString(),
                        ["lastname"] = letter
                    };
                    Service!.Create(contact);
                }
            }
            
            // Act
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("OrderBy", new[] { "firstname", "lastname" })
              .AddParameter("Top", 5);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCountGreaterThanOrEqualTo(2);
            results[0].Properties["firstname"].Value.Should().Be("1");
            results[0].Properties["lastname"].Value.Should().Be("a");
            results[1].Properties["firstname"].Value.Should().Be("1");
            results[1].Properties["lastname"].Value.Should().Be("b");
        }

        [Fact]
        public void GetDataverseRecord_OrderByMultipleColumns_MixedSortOrder()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var letters = new[] { "a", "b", "c" };
            foreach (var letter in letters)
            {
                for (int i = 3; i >= 1; i--)
                {
                    var contact = new Entity("contact")
                    {
                        ["firstname"] = i.ToString(),
                        ["lastname"] = letter
                    };
                    Service!.Create(contact);
                }
            }
            
            // Act - First ascending, second descending
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "firstname", "lastname" })
              .AddParameter("OrderBy", new[] { "firstname", "lastname-" })
              .AddParameter("Top", 5);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCountGreaterThanOrEqualTo(2);
            results[0].Properties["firstname"].Value.Should().Be("1");
            results[0].Properties["lastname"].Value.Should().Be("c");
            results[1].Properties["firstname"].Value.Should().Be("1");
            results[1].Properties["lastname"].Value.Should().Be("b");
        }

        #endregion

        #region FetchXml Basic Tests

        [Fact]
        public void GetDataverseRecord_FetchXml_BasicQuery()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Joe", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "Two" });
            
            // Act
            var fetchXml = @"<fetch>
                <entity name='contact'>
                    <attribute name='firstname' />
                    <filter type='and'>
                        <condition attribute='firstname' operator='eq' value='Rob' />
                    </filter>
                </entity>
            </fetch>";
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("FetchXml", fetchXml);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(2);
            results.All(r => r.Properties["firstname"].Value.Equals("Rob")).Should().BeTrue();
            // Only columns in FetchXML should be returned (plus Id, TableName)
            results[0].Properties.Select(p => p.Name).OrderBy(n => n)
                .Should().BeEquivalentTo(new[] { "firstname", "Id", "TableName" }.OrderBy(n => n));
        }

        [Fact]
        public void GetDataverseRecord_FetchXml_WithJoin()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var parentId = Guid.NewGuid();
            Service!.Create(new Entity("contact") { Id = parentId, ["firstname"] = "Joe", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { ["firstname"] = "Rob", ["lastname"] = "Two", ["parentcontactid"] = new EntityReference("contact", parentId) });
            
            // Act
            var fetchXml = @"<fetch>
                <entity name='contact'>
                    <attribute name='firstname' />
                    <link-entity name='contact' from='contactid' to='parentcontactid' link-type='outer' alias='parentcontact'>
                        <attribute name='firstname' />
                    </link-entity>
                </entity>
            </fetch>";
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("FetchXml", fetchXml);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(3);
            results.Select(r => r.Properties["firstname"].Value).Should().BeEquivalentTo(new[] { "Joe", "Rob", "Rob" });
            // Joined column should use alias prefix - find the result that has the joined data
            var joinedResult = results.FirstOrDefault(r => r.Properties["parentcontact.firstname"] != null);
            joinedResult.Should().NotBeNull("at least one result should have joined parentcontact data");
            joinedResult!.Properties["parentcontact.firstname"].Value.Should().Be("Joe");
        }

        [Fact]
        public void GetDataverseRecord_FetchXml_WithTopAttribute()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            for (int i = 1; i <= 20; i++)
            {
                Service!.Create(new Entity("contact") { ["firstname"] = $"Person{i}", ["lastname"] = "Smith" });
            }
            
            // Act
            var fetchXml = @"<fetch top='10'>
                <entity name='contact'>
                    <attribute name='contactid' />
                    <attribute name='firstname' />
                    <filter type='and'>
                        <condition attribute='lastname' operator='eq' value='Smith' />
                    </filter>
                </entity>
            </fetch>";
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("FetchXml", fetchXml);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(10);
        }

        [Fact]
        public void GetDataverseRecord_FetchXml_AutomaticPaging()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            for (int i = 1; i <= 25; i++)
            {
                Service!.Create(new Entity("contact") { ["firstname"] = $"PageTest{i}", ["lastname"] = "PagingTest" });
            }
            
            // Act - No top attribute, should retrieve all with automatic paging
            var fetchXml = @"<fetch>
                <entity name='contact'>
                    <attribute name='contactid' />
                    <attribute name='firstname' />
                    <filter type='and'>
                        <condition attribute='lastname' operator='eq' value='PagingTest' />
                    </filter>
                </entity>
            </fetch>";
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("FetchXml", fetchXml);
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(25);
        }

        [Fact]
        public void GetDataverseRecord_ById_MultipleIds()
        {
            // Arrange
            using var ps = CreatePowerShellWithCmdlets();
            var mockConnection = CreateMockConnection("contact");
            
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var id3 = Guid.NewGuid();
            Service!.Create(new Entity("contact") { Id = id1, ["firstname"] = "Rob", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { Id = id2, ["firstname"] = "Joe", ["lastname"] = "One" });
            Service!.Create(new Entity("contact") { Id = id3, ["firstname"] = "Rob", ["lastname"] = "Two" });
            
            // Act
            ps.AddCommand("Get-DataverseRecord")
              .AddParameter("Connection", mockConnection)
              .AddParameter("TableName", "contact")
              .AddParameter("Columns", new[] { "contactid" })
              .AddParameter("Id", new[] { id1, id2 });
            var results = ps.Invoke();

            // Assert
            ps.HadErrors.Should().BeFalse();
            results.Should().HaveCount(2);
            var returnedIds = results.Select(r => (Guid)r.Properties["Id"].Value).ToList();
            returnedIds.Should().Contain(id1);
            returnedIds.Should().Contain(id2);
        }

        #endregion
    }
}
