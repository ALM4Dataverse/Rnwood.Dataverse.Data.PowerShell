using System;
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

namespace Rnwood.Dataverse.Data.PowerShell.Tests.Cmdlets;

/// <summary>
/// Tests for Form-related cmdlets:
/// - Get-DataverseForm
/// - Set-DataverseForm  
/// - Get-DataverseFormTab
/// - Get-DataverseFormSection
/// - Get-DataverseFormControl
/// - Set-DataverseFormControl
/// - Remove-DataverseFormControl
/// - New-DataverseFormControl
/// - Get-DataverseFormLibrary
/// - Set-DataverseFormLibrary
/// - Get-DataverseFormEventHandler
/// - Set-DataverseFormEventHandler
/// </summary>
public class FormsTests : TestBase
{
    // Sample FormXML for testing - represents a typical form structure
    private const string SampleFormXml = @"<form>
        <tabs>
            <tab name=""GeneralTab"" id=""{12345678-1234-1234-1234-123456789001}"" expanded=""true"" visible=""true"" showlabel=""true"">
                <labels><label description=""General"" languagecode=""1033"" /></labels>
                <columns>
                    <column width=""50%"">
                        <sections>
                            <section name=""GeneralSection"" id=""{12345678-1234-1234-1234-123456789011}"" showlabel=""true"" showbar=""true"" visible=""true"" columns=""2"">
                                <labels><label description=""General Info"" languagecode=""1033"" /></labels>
                                <rows>
                                    <row>
                                        <cell id=""{12345678-1234-1234-1234-123456789021}"" colspan=""1"" rowspan=""1"" auto=""true"">
                                            <labels><label description=""First Name"" languagecode=""1033"" /></labels>
                                            <control id=""firstname"" classid=""{4273EDBD-AC1D-40D3-9FB2-095C621B552D}"" datafieldname=""firstname"" disabled=""false"" />
                                        </cell>
                                        <cell id=""{12345678-1234-1234-1234-123456789022}"">
                                            <labels><label description=""Last Name"" languagecode=""1033"" /></labels>
                                            <control id=""lastname"" classid=""{4273EDBD-AC1D-40D3-9FB2-095C621B552D}"" datafieldname=""lastname"" />
                                        </cell>
                                    </row>
                                    <row>
                                        <cell id=""{12345678-1234-1234-1234-123456789023}"">
                                            <labels><label description=""Email"" languagecode=""1033"" /></labels>
                                            <control id=""emailaddress1"" classid=""{4273EDBD-AC1D-40D3-9FB2-095C621B552D}"" datafieldname=""emailaddress1"" />
                                        </cell>
                                    </row>
                                </rows>
                            </section>
                        </sections>
                    </column>
                    <column width=""50%"">
                        <sections>
                            <section name=""DetailsSection"" id=""{12345678-1234-1234-1234-123456789012}"" showlabel=""true"" visible=""true"">
                                <labels><label description=""Details"" languagecode=""1033"" /></labels>
                                <rows>
                                    <row>
                                        <cell id=""{12345678-1234-1234-1234-123456789024}"">
                                            <labels><label description=""Phone"" languagecode=""1033"" /></labels>
                                            <control id=""telephone1"" classid=""{4273EDBD-AC1D-40D3-9FB2-095C621B552D}"" datafieldname=""telephone1"" />
                                        </cell>
                                    </row>
                                </rows>
                            </section>
                        </sections>
                    </column>
                </columns>
            </tab>
            <tab name=""AddressTab"" id=""{12345678-1234-1234-1234-123456789002}"" expanded=""false"" visible=""true"" showlabel=""true"">
                <labels><label description=""Address"" languagecode=""1033"" /></labels>
                <columns>
                    <column width=""100%"">
                        <sections>
                            <section name=""AddressSection"" id=""{12345678-1234-1234-1234-123456789013}"" showlabel=""true"" visible=""true"">
                                <labels><label description=""Address Info"" languagecode=""1033"" /></labels>
                                <rows>
                                    <row>
                                        <cell id=""{12345678-1234-1234-1234-123456789025}"">
                                            <labels><label description=""Street"" languagecode=""1033"" /></labels>
                                            <control id=""address1_line1"" classid=""{4273EDBD-AC1D-40D3-9FB2-095C621B552D}"" datafieldname=""address1_line1"" />
                                        </cell>
                                    </row>
                                </rows>
                            </section>
                        </sections>
                    </column>
                </columns>
            </tab>
        </tabs>
        <header>
            <rows>
                <row>
                    <cell id=""{12345678-1234-1234-1234-123456789030}"">
                        <control id=""header_fullname"" classid=""{4273EDBD-AC1D-40D3-9FB2-095C621B552D}"" datafieldname=""fullname"" />
                    </cell>
                </row>
            </rows>
        </header>
        <hiddencontrols>
            <data id=""contactid"" datafieldname=""contactid"" classid=""{4273EDBD-AC1D-40D3-9FB2-095C621B552D}"" />
        </hiddencontrols>
        <clientresources>
            <internalresources>
                <clientincludes>
                    <webresource name=""new_contactform.js"" type=""JScript"" libraryuniquename=""contact_form_lib"" />
                </clientincludes>
            </internalresources>
        </clientresources>
        <formLibraries>
            <Library name=""new_contactform.js"" libraryUniqueId=""{12345678-1234-1234-1234-123456789050}"" />
            <Library name=""another_library.js"" libraryUniqueId=""{12345678-1234-1234-1234-123456789051}"" />
        </formLibraries>
        <events>
            <event name=""onload"" application=""false"" active=""true"">
                <Handlers>
                    <Handler functionName=""onLoad"" libraryName=""new_contactform.js"" handlerUniqueId=""{12345678-1234-1234-1234-123456789040}"" enabled=""true"" parameters="""" passExecutionContext=""true"" />
                </Handlers>
            </event>
        </events>
    </form>";

    private new PS CreatePowerShellWithCmdlets()
    {
        var initialSessionState = InitialSessionState.CreateDefault2();
        
        // Register form-related cmdlets
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Get-DataverseForm", typeof(GetDataverseFormCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Get-DataverseFormTab", typeof(GetDataverseFormTabCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Get-DataverseFormSection", typeof(GetDataverseFormSectionCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Get-DataverseFormControl", typeof(GetDataverseFormControlCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Get-DataverseFormLibrary", typeof(GetDataverseFormLibraryCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Get-DataverseFormEventHandler", typeof(GetDataverseFormEventHandlerCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Get-DataverseRecord", typeof(GetDataverseRecordCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Set-DataverseRecord", typeof(SetDataverseRecordCmdlet), null));
        
        // Register write operation cmdlets
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Set-DataverseForm", typeof(SetDataverseFormCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Remove-DataverseForm", typeof(RemoveDataverseFormCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Set-DataverseFormSection", typeof(SetDataverseFormSectionCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Remove-DataverseFormSection", typeof(RemoveDataverseFormSectionCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Set-DataverseFormControl", typeof(SetDataverseFormControlCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Remove-DataverseFormControl", typeof(RemoveDataverseFormControlCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Set-DataverseFormTab", typeof(SetDataverseFormTabCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Remove-DataverseFormTab", typeof(RemoveDataverseFormTabCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Set-DataverseFormLibrary", typeof(SetDataverseFormLibraryCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Remove-DataverseFormLibrary", typeof(RemoveDataverseFormLibraryCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Set-DataverseFormEventHandler", typeof(SetDataverseFormEventHandlerCmdlet), null));
        initialSessionState.Commands.Add(new SessionStateCmdletEntry("Remove-DataverseFormEventHandler", typeof(RemoveDataverseFormEventHandlerCmdlet), null));
        
        var runspace = RunspaceFactory.CreateRunspace(initialSessionState);
        runspace.Open();
        
        var ps = PS.Create()!;
        ps.Runspace = runspace;
        return ps;
    }

    private Entity CreateTestForm(Guid formId, string formName, string objectTypeCode, string formXml = null)
    {
        return new Entity("systemform")
        {
            Id = formId,
            ["formid"] = formId,
            ["name"] = formName,
            ["uniquename"] = $"form_{formName.ToLower().Replace(" ", "_")}",
            ["objecttypecode"] = objectTypeCode,
            ["type"] = new OptionSetValue(2), // Main form
            ["description"] = $"Test form: {formName}",
            ["formactivationstate"] = new OptionSetValue(1), // Active
            ["formpresentation"] = new OptionSetValue(1), // Air form
            ["isdefault"] = false,
            ["formxml"] = formXml ?? SampleFormXml
        };
    }

    #region Get-DataverseForm Tests

        [Fact]
        public void SetDataverseForm_CreatesNewFormWithGeneratedFormXml()
        {
                using var ps = CreatePowerShellWithCmdlets();
                var mockConnection = CreateMockConnection("systemform", "contact");

                ps.AddCommand("Set-DataverseForm")
                    .AddParameter("Connection", mockConnection)
                    .AddParameter("Entity", "contact")
                    .AddParameter("Name", "CreatedContactForm")
                    .AddParameter("FormType", FormType.Main)
                    .AddParameter("Description", "Created from standard test")
                    .AddParameter("IsActive", true)
                    .AddParameter("PassThru", true);
                var results = ps.Invoke();

                ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
                results.Should().HaveCount(1);

                var createdFormId = (Guid)results[0].BaseObject;
                var createdForm = Service!.Retrieve("systemform", createdFormId, new ColumnSet("name", "objecttypecode", "description", "type", "formxml", "formactivationstate"));
                createdForm.GetAttributeValue<string>("name").Should().Be("CreatedContactForm");
                createdForm.GetAttributeValue<string>("objecttypecode").Should().Be("contact");
                createdForm.GetAttributeValue<string>("description").Should().Be("Created from standard test");
                createdForm.GetAttributeValue<OptionSetValue>("type")!.Value.Should().Be((int)FormType.Main);
                createdForm.GetAttributeValue<OptionSetValue>("formactivationstate")!.Value.Should().Be(1);
                createdForm.GetAttributeValue<string>("formxml").Should().Contain("<tab");
        }

        [Fact]
        public void SetDataverseForm_Publish_ExecutesPublishXmlRequestForEntity()
        {
                string publishXml = null;
                var mockConnection = CreateMockConnection(request =>
                {
                        if (request.GetType().Name == "PublishXmlRequest")
                        {
                                publishXml = request.Parameters["ParameterXml"] as string;
                                return new Microsoft.Crm.Sdk.Messages.PublishXmlResponse();
                        }

                        return null;
                }, "systemform", "contact");

                using var ps = CreatePowerShellWithCmdlets();
                ps.AddCommand("Set-DataverseForm")
                    .AddParameter("Connection", mockConnection)
                    .AddParameter("Entity", "contact")
                    .AddParameter("Name", "PublishedContactForm")
                    .AddParameter("FormType", FormType.Main)
                    .AddParameter("Publish", true)
                    .AddParameter("PassThru", true);
                var results = ps.Invoke();

                ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
                results.Should().HaveCount(1);
                publishXml.Should().Contain("<entity>contact</entity>");
        }

        [Fact]
        public void RemoveDataverseForm_RemovesExistingFormById()
        {
                using var ps = CreatePowerShellWithCmdlets();
                var mockConnection = CreateMockConnection("systemform", "contact");

                var formId = Guid.NewGuid();
                Service!.Create(CreateTestForm(formId, "DeleteMe", "contact"));

                ps.AddCommand("Remove-DataverseForm")
                    .AddParameter("Connection", mockConnection)
                    .AddParameter("Id", formId)
                    .AddParameter("Confirm", false);
                ps.Invoke();

                ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));

                var results = Service.RetrieveMultiple(new QueryExpression("systemform")
                {
                        ColumnSet = new ColumnSet("formid")
                }).Entities.Where(entity => entity.Id == formId).ToList();
                results.Should().BeEmpty();
        }

        [Fact]
        public void RemoveDataverseForm_IfExists_DoesNotErrorWhenFormIsMissing()
        {
                using var ps = CreatePowerShellWithCmdlets();
                var mockConnection = CreateMockConnection("systemform", "contact");

                ps.AddCommand("Remove-DataverseForm")
                    .AddParameter("Connection", mockConnection)
                    .AddParameter("Entity", "contact")
                    .AddParameter("Name", "MissingForm")
                    .AddParameter("IfExists", true)
                    .AddParameter("Confirm", false);
                ps.Invoke();

                ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        }

    [Fact]
    public void GetDataverseForm_RetrievesFormsByEntity()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "TestContactForm", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseForm")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Entity", "contact");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["Name"].Value.Should().Be("TestContactForm");
    }

    [Fact]
    public void GetDataverseForm_IncludesFormXml_WhenIncludeFormXmlSwitch()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "TestFormWithXml", "contact", SampleFormXml);
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseForm")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Entity", "contact")
          .AddParameter("IncludeFormXml", true);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["FormXml"].Should().NotBeNull();
        ((string)results[0].Properties["FormXml"].Value).Should().Contain("<form>");
    }

    [Fact]
    public void GetDataverseForm_FiltersByFormType()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var mainFormId = Guid.NewGuid();
        var mainForm = CreateTestForm(mainFormId, "MainForm", "contact");
        mainForm["type"] = new OptionSetValue(2); // Main
        Service!.Create(mainForm);
        
        var quickCreateFormId = Guid.NewGuid();
        var quickCreateForm = CreateTestForm(quickCreateFormId, "QuickCreateForm", "contact");
        quickCreateForm["type"] = new OptionSetValue(7); // QuickCreate
        Service!.Create(quickCreateForm);
        
        // Act
        ps.AddCommand("Get-DataverseForm")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Entity", "contact")
          .AddParameter("FormType", FormType.Main);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["Name"].Value.Should().Be("MainForm");
    }

    [Fact]
    public void GetDataverseForm_RetrievesByFormId()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "SpecificForm", "contact");
        Service!.Create(testForm);
        
        // Create another form to ensure we get the right one
        var otherFormId = Guid.NewGuid();
        var otherForm = CreateTestForm(otherFormId, "OtherForm", "contact");
        Service!.Create(otherForm);
        
        // Act
        ps.AddCommand("Get-DataverseForm")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Id", formId);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["Name"].Value.Should().Be("SpecificForm");
    }

    #endregion

    #region Get-DataverseFormTab Tests

    [Fact]
    public void GetDataverseFormTab_RetrievesAllTabsFromForm()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithTabs", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormTab")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(2); // GeneralTab and AddressTab
        results.Select(r => r.Properties["Name"].Value).Should().Contain("GeneralTab");
        results.Select(r => r.Properties["Name"].Value).Should().Contain("AddressTab");
    }

    [Fact]
    public void GetDataverseFormTab_RetrievesSpecificTabByName()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithTabs", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormTab")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "GeneralTab");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["Name"].Value.Should().Be("GeneralTab");
        results[0].Properties["Expanded"].Value.Should().Be(true);
    }

    [Fact]
    public void GetDataverseFormTab_IncludesSectionsInTabOutput()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithTabs", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormTab")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "GeneralTab");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        var sections = results[0].Properties["Sections"].Value as object[];
        sections.Should().NotBeNull();
        sections.Should().HaveCountGreaterThan(0);
    }

    #endregion

    #region Get-DataverseFormSection Tests

    [Fact]
    public void GetDataverseFormSection_RetrievesAllSectionsFromForm()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithSections", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormSection")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCountGreaterThanOrEqualTo(3); // GeneralSection, DetailsSection, AddressSection
        results.Select(r => r.Properties["Name"].Value).Should().Contain("GeneralSection");
    }

    [Fact]
    public void GetDataverseFormSection_RetrievesSectionsByTabName()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithSections", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormSection")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "GeneralTab");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(2); // GeneralSection and DetailsSection in GeneralTab
    }

    [Fact]
    public void GetDataverseFormSection_RetrievesSpecificSectionByNameAndTab()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithSections", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormSection")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "GeneralTab")
          .AddParameter("SectionName", "GeneralSection");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["Name"].Value.Should().Be("GeneralSection");
    }

    [Fact]
    public void GetDataverseFormSection_IncludesControlsInSectionOutput()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithSections", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormSection")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "GeneralTab")
          .AddParameter("SectionName", "GeneralSection");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        var controls = results[0].Properties["Controls"].Value as object[];
        controls.Should().NotBeNull();
        controls.Should().HaveCountGreaterThan(0);
    }

    #endregion

    #region Get-DataverseFormControl Tests

    [Fact]
    public void GetDataverseFormControl_RetrievesAllControls()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithControls", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormControl")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCountGreaterThanOrEqualTo(5); // firstname, lastname, emailaddress1, telephone1, address1_line1
    }

    [Fact]
    public void GetDataverseFormControl_FiltersByTabName()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithControls", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormControl")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "AddressTab");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1); // Only address1_line1
        results[0].Properties["DataField"].Value.Should().Be("address1_line1");
    }

    [Fact]
    public void GetDataverseFormControl_FiltersBySectionName()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithControls", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormControl")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "GeneralTab")
          .AddParameter("SectionName", "DetailsSection");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1); // Only telephone1
        results[0].Properties["DataField"].Value.Should().Be("telephone1");
    }

    [Fact]
    public void GetDataverseFormControl_RetrievesSpecificControlById()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithControls", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormControl")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("ControlId", "firstname");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["Id"].Value.Should().Be("firstname");
        results[0].Properties["DataField"].Value.Should().Be("firstname");
    }

    [Fact]
    public void GetDataverseFormControl_RetrievesHeaderControls()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithHeaderControls", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormControl")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "[Header]");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["DataField"].Value.Should().Be("fullname");
    }

    #endregion

    #region Get-DataverseFormLibrary Tests

    [Fact]
    public void GetDataverseFormLibrary_RetrievesLibrariesFromForm()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithLibraries", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormLibrary")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCountGreaterThanOrEqualTo(1);
        results.Select(r => r.Properties["Name"].Value).Should().Contain("new_contactform.js");
    }

    #endregion

    #region Get-DataverseFormEventHandler Tests

    [Fact]
    public void GetDataverseFormEventHandler_RetrievesEventHandlersFromForm()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithEvents", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormEventHandler")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCountGreaterThanOrEqualTo(1);
        results.Select(r => r.Properties["FunctionName"].Value).Should().Contain("onLoad");
    }

    #endregion

    #region Form XML Parsing Tests

    [Fact]
    public void DataverseForm_ParsesTabLayoutCorrectly()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithLayout", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormTab")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "GeneralTab");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["Layout"].Value.Should().Be("TwoColumns");
        results[0].Properties["Column1Width"].Value.Should().Be(50);
        results[0].Properties["Column2Width"].Value.Should().Be(50);
    }

    [Fact]
    public void DataverseForm_ParsesSingleColumnLayout()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithSingleColumn", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormTab")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "AddressTab");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["Layout"].Value.Should().Be("OneColumn");
        results[0].Properties["Column1Width"].Value.Should().Be(100);
    }

    [Fact]
    public void DataverseForm_ParsesTabExpandedState()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithExpandedTabs", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormTab")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId);
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(2);
        var generalTab = results.First(r => (string)r.Properties["Name"].Value == "GeneralTab");
        var addressTab = results.First(r => (string)r.Properties["Name"].Value == "AddressTab");
        generalTab.Properties["Expanded"].Value.Should().Be(true);
        addressTab.Properties["Expanded"].Value.Should().Be(false);
    }

    [Fact]
    public void DataverseForm_ParsesControlCellAttributes()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "FormWithCellAttrs", "contact");
        Service!.Create(testForm);
        
        // Act
        ps.AddCommand("Get-DataverseFormControl")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("ControlId", "firstname");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["Row"].Should().NotBeNull();
        results[0].Properties["Column"].Should().NotBeNull();
    }

    #endregion

    #region Form Retrieval With Multiple Forms Tests

    [Fact]
    public void GetDataverseForm_ReturnsMultipleFormsForEntity()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        for (int i = 1; i <= 3; i++)
        {
            var formId = Guid.NewGuid();
            var testForm = CreateTestForm(formId, $"Form{i}", "contact");
            Service!.Create(testForm);
        }
        
        // Act
        ps.AddCommand("Get-DataverseForm")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Entity", "contact");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(3);
    }

    [Fact]
    public void GetDataverseForm_OnlyReturnsFormsForSpecifiedEntity()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        // Only use systemform metadata to avoid issues with missing metadata for other entities
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var contactFormId = Guid.NewGuid();
        var contactForm = CreateTestForm(contactFormId, "ContactForm", "contact");
        Service!.Create(contactForm);
        
        // Create another form for a different entity code to test filtering
        var otherFormId = Guid.NewGuid();
        var otherForm = CreateTestForm(otherFormId, "OtherForm", "lead");
        Service!.Create(otherForm);
        
        // Act
        ps.AddCommand("Get-DataverseForm")
          .AddParameter("Connection", mockConnection)
          .AddParameter("Entity", "contact");
        var results = ps.Invoke();

        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["Name"].Value.Should().Be("ContactForm");
    }

    #endregion

    // Set-DataverseFormControl Tests

    [Fact]
    public void SetDataverseFormControl_UpdatesControlProperties()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "TestForm", "contact");
        Service!.Create(testForm);
        
        // Act - Update firstname control to make it disabled and hidden
        ps.AddCommand("Set-DataverseFormControl")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "GeneralTab")
          .AddParameter("SectionName", "GeneralSection")
          .AddParameter("DataField", "firstname")
          .AddParameter("ControlId", "firstname")
          .AddParameter("Disabled", true)
          .AddParameter("Hidden", true);
        var results = ps.Invoke();
        
        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        
        // Retrieve updated form
        var updatedForm = Service.Retrieve("systemform", formId, new ColumnSet("formxml"));
        var formXml = updatedForm.GetAttributeValue<string>("formxml");
        formXml.Should().Contain("disabled=\"true\"");
        formXml.Should().Contain("visible=\"false\"");
    }

    [Fact]
    public void SetDataverseFormControl_UpdateByControlId_WithoutDataField_ShouldSucceed()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "TestForm", "contact");
        Service!.Create(testForm);
        
        // Act - Update firstname control using only ControlId (without DataField parameter)
        // This is the scenario from the bug report
        ps.AddCommand("Set-DataverseFormControl")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "GeneralTab")
          .AddParameter("SectionName", "GeneralSection")
          .AddParameter("ControlId", "firstname")
          .AddParameter("Labels", new System.Collections.Hashtable { { 1033, "Updated Label" } });
        var results = ps.Invoke();
        
        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        
        // Retrieve updated form and verify label was updated
        var updatedForm = Service.Retrieve("systemform", formId, new ColumnSet("formxml"));
        var formXml = updatedForm.GetAttributeValue<string>("formxml");
        formXml.Should().Contain("Updated Label");
    }

    [Fact]
    public void SetDataverseFormControl_AutoControlType_DetectsTypeFromAttribute()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "TestForm", "contact");
        Service!.Create(testForm);
        
        // Act - Set control without specifying ControlType (should auto-detect from attribute metadata)
        ps.AddCommand("Set-DataverseFormControl")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "GeneralTab")
          .AddParameter("SectionName", "GeneralSection")
          .AddParameter("DataField", "firstname")
          .AddParameter("ControlId", "firstname_auto");
        var results = ps.Invoke();
        
        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        
        // Retrieve updated form and verify control was created with appropriate classid
        var updatedForm = Service.Retrieve("systemform", formId, new ColumnSet("formxml"));
        var formXml = updatedForm.GetAttributeValue<string>("formxml");
        formXml.Should().Contain("id=\"firstname_auto\"");
        formXml.Should().Contain("datafieldname=\"firstname\"");
    }

    [Fact]
    public void SetDataverseFormControl_Subgrid_CreatesSubgridControl()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "TestForm", "contact");
        Service!.Create(testForm);
        
        // Act - Create a subgrid control (doesn't require DataField)
        ps.AddCommand("Set-DataverseFormControl")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "GeneralTab")
          .AddParameter("SectionName", "GeneralSection")
          .AddParameter("ControlId", "contacts_subgrid")
          .AddParameter("ControlType", "Subgrid");
        var results = ps.Invoke();
        
        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        
        // Retrieve updated form and verify subgrid control was created
        var updatedForm = Service.Retrieve("systemform", formId, new ColumnSet("formxml"));
        var formXml = updatedForm.GetAttributeValue<string>("formxml");
        formXml.Should().Contain("id=\"contacts_subgrid\"");
    }

    [Fact]
    public void SetDataverseFormControl_SupportsWhatIf()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "TestForm", "contact");
        var originalFormXml = testForm.GetAttributeValue<string>("formxml");
        Service!.Create(testForm);
        
        // Act - Execute with -WhatIf
        ps.AddCommand("Set-DataverseFormControl")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "GeneralTab")
          .AddParameter("SectionName", "GeneralSection")
          .AddParameter("DataField", "firstname")
          .AddParameter("ControlId", "firstname")
          .AddParameter("Disabled", true)
          .AddParameter("WhatIf", true);
        var results = ps.Invoke();
        
        // Assert - Form should NOT be updated
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        
        var unchangedForm = Service.Retrieve("systemform", formId, new ColumnSet("formxml"));
        unchangedForm.GetAttributeValue<string>("formxml").Should().Be(originalFormXml);
    }

    // Set-DataverseFormControl Tests (New control scenarios)

    [Fact]
    public void SetDataverseFormControl_CreatesNewControlInSection()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "TestForm", "contact");
        Service!.Create(testForm);
        
        // Act - Create a new control for an attribute not already on the form
        ps.AddCommand("Set-DataverseFormControl")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "AddressTab")
          .AddParameter("SectionName", "AddressSection")
          .AddParameter("DataField", "address1_city")
          .AddParameter("ControlId", "city_control")
          .AddParameter("PassThru", true);
        var results = ps.Invoke();
        
        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].BaseObject.ToString().Should().Be("city_control");
        
        // Verify control was added to form
        var updatedForm = Service.Retrieve("systemform", formId, new ColumnSet("formxml"));
        var formXml = updatedForm.GetAttributeValue<string>("formxml");
        formXml.Should().Contain("id=\"city_control\"");
        formXml.Should().Contain("datafieldname=\"address1_city\"");
    }

    [Fact]
    public void SetDataverseFormControl_Header_CreatesControlInHeader()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "TestForm", "contact");
        Service!.Create(testForm);
        
        // Act - Create a new control in the header
        ps.AddCommand("Set-DataverseFormControl")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "[Header]")
          .AddParameter("DataField", "emailaddress1")
          .AddParameter("ControlId", "header_email")
          .AddParameter("PassThru", true);
        var results = ps.Invoke();
        
        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].BaseObject.ToString().Should().Be("header_email");
        
        // Verify control was added to header
        var updatedForm = Service.Retrieve("systemform", formId, new ColumnSet("formxml"));
        var formXml = updatedForm.GetAttributeValue<string>("formxml");
        formXml.Should().Contain("<header");
        formXml.Should().Contain("id=\"header_email\"");
    }

    // Remove-DataverseFormControl Tests

    [Fact]
    public void RemoveDataverseFormControl_RemovesControlById()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "TestForm", "contact");
        Service!.Create(testForm);
        
        // Verify control exists
        var originalForm = Service.Retrieve("systemform", formId, new ColumnSet("formxml"));
        originalForm.GetAttributeValue<string>("formxml").Should().Contain("id=\"firstname\"");
        
        // Act - Remove the firstname control
        ps.AddCommand("Remove-DataverseFormControl")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("ControlId", "firstname")
          .AddParameter("Confirm", false);
        var results = ps.Invoke();
        
        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        
        // Verify control was removed
        var updatedForm = Service.Retrieve("systemform", formId, new ColumnSet("formxml"));
        var formXml = updatedForm.GetAttributeValue<string>("formxml");
        formXml.Should().NotContain("id=\"firstname\"");
    }

        [Fact]
        public void SetDataverseFormSection_CreatesSectionInTab()
        {
                using var ps = CreatePowerShellWithCmdlets();
                var mockConnection = CreateMockConnection("systemform", "contact");

                var formId = Guid.NewGuid();
                Service!.Create(CreateTestForm(formId, "SectionForm", "contact"));

                ps.AddCommand("Set-DataverseFormSection")
                    .AddParameter("Connection", mockConnection)
                    .AddParameter("FormId", formId)
                    .AddParameter("TabName", "GeneralTab")
                    .AddParameter("Name", "InsertedSection")
                    .AddParameter("Label", "Inserted Section")
                    .AddParameter("Columns", 2)
                    .AddParameter("ShowBar", true)
                    .AddParameter("PassThru", true);
                var results = ps.Invoke();

                ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
                results.Should().HaveCount(1);

                var updatedForm = Service.Retrieve("systemform", formId, new ColumnSet("formxml"));
                var formXml = updatedForm.GetAttributeValue<string>("formxml");
                formXml.Should().Contain("name=\"InsertedSection\"");
                formXml.Should().Contain("description=\"Inserted Section\"");
                formXml.Should().Contain("columns=\"2\"");
        }

        [Fact]
        public void RemoveDataverseFormSection_RemovesSectionFromTab()
        {
                using var ps = CreatePowerShellWithCmdlets();
                var mockConnection = CreateMockConnection("systemform", "contact");

                var formId = Guid.NewGuid();
                Service!.Create(CreateTestForm(formId, "SectionRemovalForm", "contact"));

                ps.AddCommand("Remove-DataverseFormSection")
                    .AddParameter("Connection", mockConnection)
                    .AddParameter("FormId", formId)
                    .AddParameter("TabName", "GeneralTab")
                    .AddParameter("SectionName", "DetailsSection")
                    .AddParameter("Confirm", false);
                ps.Invoke();

                ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));

                var updatedForm = Service.Retrieve("systemform", formId, new ColumnSet("formxml"));
                updatedForm.GetAttributeValue<string>("formxml").Should().NotContain("name=\"DetailsSection\"");
        }

    // Form Tab Layout Tests

    [Fact]
    public void SetDataverseFormTab_CanReorderTabs()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "TestForm", "contact");
        Service!.Create(testForm);
        
        // Get original tab order
        var originalForm = Service.Retrieve("systemform", formId, new ColumnSet("formxml"));
        var originalXml = originalForm.GetAttributeValue<string>("formxml");
        originalXml.Should().Contain("name=\"GeneralTab\"");
        originalXml.Should().Contain("name=\"AddressTab\"");
        
        // Act - Add a new tab before GeneralTab to test tab positioning
        ps.AddCommand("Set-DataverseFormTab")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("Name", "HeaderTab")
          .AddParameter("Label", "Header Info")
          .AddParameter("InsertBefore", "GeneralTab");
        var results = ps.Invoke();
        
        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        
        // Verify new tab was inserted before GeneralTab
        var updatedForm = Service.Retrieve("systemform", formId, new ColumnSet("formxml"));
        var updatedXml = updatedForm.GetAttributeValue<string>("formxml");
        var headerTabIndex = updatedXml.IndexOf("name=\"HeaderTab\"");
        var generalTabIndex = updatedXml.IndexOf("name=\"GeneralTab\"");
        headerTabIndex.Should().BeLessThan(generalTabIndex);
    }

    [Fact]
    public void SetDataverseFormTab_CanAddNewTab()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "TestForm", "contact");
        Service!.Create(testForm);
        
        // Act - Add a new tab
        ps.AddCommand("Set-DataverseFormTab")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("Name", "NotesTab")
          .AddParameter("Label", "Notes & Activities")
          .AddParameter("PassThru", true);
        var results = ps.Invoke();
        
        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        
        // Verify tab was added (note: & is HTML-encoded as &amp; in XML)
        var updatedForm = Service.Retrieve("systemform", formId, new ColumnSet("formxml"));
        var formXml = updatedForm.GetAttributeValue<string>("formxml");
        formXml.Should().Contain("name=\"NotesTab\"");
        formXml.Should().Contain("Notes &amp; Activities");
    }

    [Fact]
    public void RemoveDataverseFormTab_CanRemoveTab()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "TestForm", "contact");
        Service!.Create(testForm);
        
        // Verify tab exists
        var originalForm = Service.Retrieve("systemform", formId, new ColumnSet("formxml"));
        originalForm.GetAttributeValue<string>("formxml").Should().Contain("name=\"AddressTab\"");
        
        // Act - Remove AddressTab
        ps.AddCommand("Remove-DataverseFormTab")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "AddressTab")
          .AddParameter("Confirm", false);
        var results = ps.Invoke();
        
        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        
        // Verify tab was removed
        var updatedForm = Service.Retrieve("systemform", formId, new ColumnSet("formxml"));
        var formXml = updatedForm.GetAttributeValue<string>("formxml");
        formXml.Should().NotContain("name=\"AddressTab\"");
    }

    // Form Library and Event Handler Tests - Write operations

    [Fact]
    public void SetDataverseFormLibrary_CanAddJavaScriptLibrary()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "TestForm", "contact");
        Service!.Create(testForm);
        
        // Act - Add a new library
        ps.AddCommand("Set-DataverseFormLibrary")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("LibraryName", "new_customscripts.js");
        var results = ps.Invoke();
        
        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["Name"].Value.Should().Be("new_customscripts.js");
        
        // Verify library was added
        var updatedForm = Service.Retrieve("systemform", formId, new ColumnSet("formxml"));
        var formXml = updatedForm.GetAttributeValue<string>("formxml");
        formXml.Should().Contain("name=\"new_customscripts.js\"");
    }

    [Fact]
    public void RemoveDataverseFormLibrary_CanRemoveLibrary()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "TestForm", "contact");
        Service!.Create(testForm);
        
        // Verify library exists
        var originalForm = Service.Retrieve("systemform", formId, new ColumnSet("formxml"));
        var originalXml = originalForm.GetAttributeValue<string>("formxml");
        originalXml.Should().Contain("name=\"new_contactform.js\"");
        
        // Act - Remove the library
        ps.AddCommand("Remove-DataverseFormLibrary")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("LibraryName", "new_contactform.js")
          .AddParameter("Confirm", false);
        var results = ps.Invoke();
        
        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        
        // Verify library was removed from formLibraries element
        var updatedForm = Service.Retrieve("systemform", formId, new ColumnSet("formxml"));
        var formXml = updatedForm.GetAttributeValue<string>("formxml");
        // Check that it's not in formLibraries (but may still be referenced elsewhere)
        formXml.Should().NotContain("<Library name=\"new_contactform.js\"");
    }

    [Fact]
    public void SetDataverseFormEventHandler_CanAddOnLoadEventHandler()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "TestForm", "contact");
        Service!.Create(testForm);
        
        // Act - Add a new OnLoad event handler
        ps.AddCommand("Set-DataverseFormEventHandler")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("EventName", "onload")
          .AddParameter("FunctionName", "customOnLoad")
          .AddParameter("LibraryName", "new_contactform.js");
        var results = ps.Invoke();
        
        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["FunctionName"].Value.Should().Be("customOnLoad");
        
        // Verify event handler was added
        var updatedForm = Service.Retrieve("systemform", formId, new ColumnSet("formxml"));
        var formXml = updatedForm.GetAttributeValue<string>("formxml");
        formXml.Should().Contain("functionName=\"customOnLoad\"");
    }

    [Fact]
    public void SetDataverseFormEventHandler_CanAddControlEventHandler()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "TestForm", "contact");
        Service!.Create(testForm);
        
        // Act - Add OnChange event handler to firstname control
        ps.AddCommand("Set-DataverseFormEventHandler")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("TabName", "GeneralTab")
          .AddParameter("SectionName", "GeneralSection")
          .AddParameter("ControlId", "firstname")
          .AddParameter("EventName", "onchange")
          .AddParameter("FunctionName", "onFirstNameChange")
          .AddParameter("LibraryName", "new_contactform.js");
        var results = ps.Invoke();
        
        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        results.Should().HaveCount(1);
        results[0].Properties["ControlId"].Value.Should().Be("firstname");
        
        // Verify event handler was added to control
        var updatedForm = Service.Retrieve("systemform", formId, new ColumnSet("formxml"));
        var formXml = updatedForm.GetAttributeValue<string>("formxml");
        formXml.Should().Contain("functionName=\"onFirstNameChange\"");
    }

    [Fact]
    public void RemoveDataverseFormEventHandler_CanRemoveEventHandler()
    {
        // Arrange
        using var ps = CreatePowerShellWithCmdlets();
        var mockConnection = CreateMockConnection("systemform", "contact");
        
        var formId = Guid.NewGuid();
        var testForm = CreateTestForm(formId, "TestForm", "contact");
        Service!.Create(testForm);
        
        // Verify event handler exists
        var originalForm = Service.Retrieve("systemform", formId, new ColumnSet("formxml"));
        originalForm.GetAttributeValue<string>("formxml").Should().Contain("functionName=\"onLoad\"");
        
        // Act - Remove the onload event handler (need to specify LibraryName with FunctionName)
        ps.AddCommand("Remove-DataverseFormEventHandler")
          .AddParameter("Connection", mockConnection)
          .AddParameter("FormId", formId)
          .AddParameter("EventName", "onload")
          .AddParameter("FunctionName", "onLoad")
          .AddParameter("LibraryName", "new_contactform.js")
          .AddParameter("Confirm", false);
        var results = ps.Invoke();
        
        // Assert
        ps.HadErrors.Should().BeFalse(string.Join(", ", ps.Streams.Error.Select(e => e.ToString())));
        
        // Verify event handler was removed
        var updatedForm = Service.Retrieve("systemform", formId, new ColumnSet("formxml"));
        var formXml = updatedForm.GetAttributeValue<string>("formxml");
        formXml.Should().NotContain("functionName=\"onLoad\"");
    }
}
