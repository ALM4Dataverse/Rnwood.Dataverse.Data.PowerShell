---
Module Name: Rnwood.Dataverse.Data.PowerShell
Module Guid: {{ Update Module Guid }}
Download Help Link: {{ Update Download Link }}
Help Version: {{ Update Help Version }}
Locale: {{ Update Locale }}
---

# Rnwood.Dataverse.Data.PowerShell Module
## Description
{{ Fill in the Description }}

## Rnwood.Dataverse.Data.PowerShell Cmdlets
### [Clear-DataverseMetadataCache](Clear-DataverseMetadataCache.md)
Clears the global metadata cache used by Get cmdlets.

### [Compare-DataverseSolutionComponents](Compare-DataverseSolutionComponents.md)
Compares a solution file with the state of that solution in the target environment or with another solution file.

### [Compress-DataverseSolutionFile](Compress-DataverseSolutionFile.md)
Packs a Dataverse solution folder using the Power Apps CLI.

### [Expand-DataverseSolutionFile](Expand-DataverseSolutionFile.md)
Unpacks a Dataverse solution file using the Power Apps CLI.

### [Export-DataverseSolution](Export-DataverseSolution.md)
Exports a solution from Dataverse using an asynchronous job with progress reporting.

### [Get-DataverseAdminPortalUrl](Get-DataverseAdminPortalUrl.md)
Generates a URL to open the Power Platform Admin Center for the current environment.

### [Get-DataverseAppModule](Get-DataverseAppModule.md)
Retrieves app module (model-driven app) information from a Dataverse environment.

### [Get-DataverseAppModuleComponent](Get-DataverseAppModuleComponent.md)
Retrieves app module component information from a Dataverse environment.

### [Get-DataverseAttributeMetadata](Get-DataverseAttributeMetadata.md)
Retrieves attribute (column) metadata from Dataverse.

### [Get-DataverseCanvasApp](Get-DataverseCanvasApp.md)
Retrieves Canvas apps from a Dataverse environment.

### [Get-DataverseComponentDependency](Get-DataverseComponentDependency.md)
Retrieves component dependencies in Dataverse.

### [Get-DataverseConnection](Get-DataverseConnection.md)
Gets a connection to a Dataverse environment either interactively or silently and returns it.

All commands that need a connection to Dataverse expect you to provide the connection in `-connection` parameter.
So you can store the output of this command in a variable and pass it to each command that needs it.
See the examples for this pattern below.

### [Get-DataverseConnectionReference](Get-DataverseConnectionReference.md)
Gets connection references from Dataverse.

### [Get-DataverseDynamicPluginAssembly](Get-DataverseDynamicPluginAssembly.md)
Extracts source code and build metadata from a dynamic plugin assembly.

### [Get-DataverseEntityKeyMetadata](Get-DataverseEntityKeyMetadata.md)
Retrieves entity key (alternate key) metadata for a Dataverse table.

### [Get-DataverseEntityMetadata](Get-DataverseEntityMetadata.md)
Retrieves entity (table) metadata from Dataverse.

### [Get-DataverseEnvironment](Get-DataverseEnvironment.md)
Lists Dataverse environments accessible to the authenticated user.

### [Get-DataverseEnvironmentVariableDefinition](Get-DataverseEnvironmentVariableDefinition.md)
Gets environment variable definitions from Dataverse.

### [Get-DataverseEnvironmentVariableValue](Get-DataverseEnvironmentVariableValue.md)
Gets environment variable values from Dataverse.

### [Get-DataverseFileData](Get-DataverseFileData.md)
Downloads file data from a Dataverse file column.

### [Get-DataverseForm](Get-DataverseForm.md)
Retrieves forms from a Dataverse environment.

### [Get-DataverseFormControl](Get-DataverseFormControl.md)
Retrieves control information from a Dataverse form including controls from tabs, sections, and the form header.

### [Get-DataverseFormEventHandler](Get-DataverseFormEventHandler.md)
Retrieves event handlers from a Dataverse form (form-level, attribute-level, tab-level, or control-level events).

### [Get-DataverseFormLibrary](Get-DataverseFormLibrary.md)
Retrieves script libraries from a Dataverse form.

### [Get-DataverseFormSection](Get-DataverseFormSection.md)
Retrieves section information from a Dataverse form.

### [Get-DataverseFormTab](Get-DataverseFormTab.md)
Retrieves tab information from a Dataverse form.

### [Get-DataverseIconSetIcon](Get-DataverseIconSetIcon.md)
Retrieves available icons from supported online icon sets.

### [Get-DataverseMakerPortalUrl](Get-DataverseMakerPortalUrl.md)
Generates a URL to open the Power Apps Maker Portal for the current environment.

### [Get-DataverseMsAppComponent](Get-DataverseMsAppComponent.md)
Retrieves components from a .msapp file.

### [Get-DataverseMsAppControlTemplate](Get-DataverseMsAppControlTemplate.md)
Lists Canvas control templates from embedded all-controls metadata, optionally augmented by a .msapp file.

### [Get-DataverseMsAppControlTemplateProperty](Get-DataverseMsAppControlTemplateProperty.md)
Lists properties for a Canvas control template from embedded all-controls metadata, optionally augmented by a .msapp file.

### [Get-DataverseMsAppDataSource](Get-DataverseMsAppDataSource.md)
Gets data sources from a Canvas app .msapp file.

### [Get-DataverseMsAppProperties](Get-DataverseMsAppProperties.md)
Gets app-level properties from a Canvas app .msapp file.

### [Get-DataverseMsAppScreen](Get-DataverseMsAppScreen.md)
Retrieves screens from a .msapp file.

### [Get-DataverseOptionSetMetadata](Get-DataverseOptionSetMetadata.md)
Retrieves option set (choice) metadata from Dataverse.

### [Get-DataverseOrganizationSettings](Get-DataverseOrganizationSettings.md)
Gets organization settings from the single organization record in a Dataverse environment.

### [Get-DataversePluginAssembly](Get-DataversePluginAssembly.md)
Retrieves plugin assembly records from a Dataverse environment.

### [Get-DataversePluginPackage](Get-DataversePluginPackage.md)
Retrieves plugin package records from a Dataverse environment.

### [Get-DataversePluginStep](Get-DataversePluginStep.md)
Retrieves plugin step (SDK message processing step) records from a Dataverse environment.

### [Get-DataversePluginStepImage](Get-DataversePluginStepImage.md)
Retrieves plugin step image records from a Dataverse environment.

### [Get-DataversePluginType](Get-DataversePluginType.md)
Retrieves plugin type records from a Dataverse environment.

### [Get-DataverseRecord](Get-DataverseRecord.md)
Retrieves records from Dataverse tables using a variety of strategies to specify what should be retrieved.

### [Get-DataverseRecordAccess](Get-DataverseRecordAccess.md)
Retrieves all principals (users or teams) who have shared access to a specific record.

### [Get-DataverseRecordsFolder](Get-DataverseRecordsFolder.md)
Reads a folder of JSON files written out by `Set-DataverseRecordFolder` and converts back into a stream of PS objects.
Together these commands can be used to extract and import data to and from files, for instance for inclusion in source control, or build/deployment assets.

### [Get-DataverseRecordUrl](Get-DataverseRecordUrl.md)
Generates a URL to open a record in the Dataverse web interface.

### [Get-DataverseRelationshipMetadata](Get-DataverseRelationshipMetadata.md)
Retrieves relationship metadata from Dataverse.

### [Get-DataverseSitemap](Get-DataverseSitemap.md)
Retrieves sitemap information from a Dataverse environment.

### [Get-DataverseSitemapEntry](Get-DataverseSitemapEntry.md)
Retrieves sitemap entries (Areas, Groups, SubAreas) from a Dataverse sitemap.

### [Get-DataverseSolution](Get-DataverseSolution.md)
Retrieves solution information from a Dataverse environment.

### [Get-DataverseSolutionComponent](Get-DataverseSolutionComponent.md)
Retrieves the components of a solution from a Dataverse environment.

### [Get-DataverseSolutionDependency](Get-DataverseSolutionDependency.md)
Retrieves solution dependencies in Dataverse.

### [Get-DataverseSolutionFile](Get-DataverseSolutionFile.md)
Parses a Dataverse solution file and returns metadata information.

### [Get-DataverseSolutionFileComponent](Get-DataverseSolutionFileComponent.md)
Retrieves the components from a Dataverse solution file (.zip).

### [Get-DataverseView](Get-DataverseView.md)
Retrieves view information (savedquery or userquery) from a Dataverse environment.

### [Get-DataverseWebResource](Get-DataverseWebResource.md)
Retrieves web resources from a Dataverse environment.

### [Get-DataverseWhoAmI](Get-DataverseWhoAmI.md)
Retrieves details about the current Dataverse user and organization specified by the connection provided.

### [Import-DataverseSolution](Import-DataverseSolution.md)
Imports a solution to Dataverse using an asynchronous job with progress reporting.

### [Invoke-DataverseParallel](Invoke-DataverseParallel.md)
Processes input objects in parallel using chunked batches with cloned Dataverse connections.

### [Invoke-DataverseRequest](Invoke-DataverseRequest.md)
Invokes an arbitrary Dataverse request and returns the response.

### [Invoke-DataverseSolutionUpgrade](Invoke-DataverseSolutionUpgrade.md)
Applies a staged solution upgrade by deleting the original solution and promoting the holding solution.

### [Invoke-DataverseSql](Invoke-DataverseSql.md)
Invokes a Dataverse SQL query using Sql4Cds and writes any resulting rows to the pipeline.

### [Invoke-DataverseXrmToolbox](Invoke-DataverseXrmToolbox.md)
Invokes an XrmToolbox plugin downloaded from NuGet with the current Dataverse connection injected.

### [New-DataverseMsApp](New-DataverseMsApp.md)
Creates a new default .msapp file for a Canvas app.

### [Publish-DataverseCustomizations](Publish-DataverseCustomizations.md)
Publishes customizations in Dataverse.

### [Remove-DataverseAppModule](Remove-DataverseAppModule.md)
Removes an app module (model-driven app) from Dataverse.

### [Remove-DataverseAppModuleComponent](Remove-DataverseAppModuleComponent.md)
Removes an app module component from Dataverse.

### [Remove-DataverseAttributeMetadata](Remove-DataverseAttributeMetadata.md)
Deletes an attribute (column) from a Dataverse entity.

### [Remove-DataverseCanvasApp](Remove-DataverseCanvasApp.md)
Removes a Canvas app from a Dataverse environment.

### [Remove-DataverseConnectionReference](Remove-DataverseConnectionReference.md)
Removes a connection reference from a Dataverse environment.

### [Remove-DataverseEntityKeyMetadata](Remove-DataverseEntityKeyMetadata.md)
Deletes an alternate key from an entity (table) in Dataverse.

### [Remove-DataverseEntityMetadata](Remove-DataverseEntityMetadata.md)
Deletes an entity (table) from Dataverse.

### [Remove-DataverseEnvironmentVariableDefinition](Remove-DataverseEnvironmentVariableDefinition.md)
Removes environment variable definitions from Dataverse.

### [Remove-DataverseEnvironmentVariableValue](Remove-DataverseEnvironmentVariableValue.md)
Removes environment variable values from Dataverse.

### [Remove-DataverseFileData](Remove-DataverseFileData.md)
Deletes file data from a Dataverse file column.

### [Remove-DataverseForm](Remove-DataverseForm.md)
Removes/deletes a form from a Dataverse environment.

### [Remove-DataverseFormControl](Remove-DataverseFormControl.md)
Removes a control from a Dataverse form section.

### [Remove-DataverseFormEventHandler](Remove-DataverseFormEventHandler.md)
Removes an event handler from a Dataverse form (form-level, attribute-level, tab-level, or control-level).

### [Remove-DataverseFormLibrary](Remove-DataverseFormLibrary.md)
Removes a script library from a Dataverse form.

### [Remove-DataverseFormSection](Remove-DataverseFormSection.md)
Removes a section from a Dataverse form tab.

### [Remove-DataverseFormTab](Remove-DataverseFormTab.md)
Removes a tab from a Dataverse form.

### [Remove-DataverseMsAppComponent](Remove-DataverseMsAppComponent.md)
Removes a component from a .msapp file.

### [Remove-DataverseMsAppDataSource](Remove-DataverseMsAppDataSource.md)
Removes a data source from a Canvas app .msapp file.

### [Remove-DataverseMsAppScreen](Remove-DataverseMsAppScreen.md)
Removes a screen from a .msapp file.

### [Remove-DataverseOptionSetMetadata](Remove-DataverseOptionSetMetadata.md)
Deletes a global option set from Dataverse.

### [Remove-DataversePluginAssembly](Remove-DataversePluginAssembly.md)
Removes a plugin assembly from a Dataverse environment.

### [Remove-DataversePluginPackage](Remove-DataversePluginPackage.md)
Removes a plugin package from a Dataverse environment.

### [Remove-DataversePluginStep](Remove-DataversePluginStep.md)
Removes a plugin step from a Dataverse environment.

### [Remove-DataversePluginStepImage](Remove-DataversePluginStepImage.md)
Removes a plugin step image from a Dataverse environment.

### [Remove-DataversePluginType](Remove-DataversePluginType.md)
Removes a plugin type from a Dataverse environment.

### [Remove-DataverseRecord](Remove-DataverseRecord.md)
Deletes an existing Dataverse record, including M:M association records.

### [Remove-DataverseRecordAccess](Remove-DataverseRecordAccess.md)
Revokes access rights for a security principal (user or team) on a specific record.

### [Remove-DataverseRelationshipMetadata](Remove-DataverseRelationshipMetadata.md)
Deletes a relationship from Dataverse.

### [Remove-DataverseSitemap](Remove-DataverseSitemap.md)
Removes (deletes) a sitemap from Dataverse.

### [Remove-DataverseSitemapEntry](Remove-DataverseSitemapEntry.md)
Removes an entry (Area, Group, or SubArea) from a Dataverse sitemap.

### [Remove-DataverseSolution](Remove-DataverseSolution.md)
Removes (uninstalls) a solution from Dataverse.

### [Remove-DataverseSolutionComponent](Remove-DataverseSolutionComponent.md)
Removes a solution component from an unmanaged solution.

### [Remove-DataverseView](Remove-DataverseView.md)
Removes Dataverse views (savedquery and userquery entities).

### [Remove-DataverseWebResource](Remove-DataverseWebResource.md)
Removes a web resource from a Dataverse environment.

### [Set-DataverseAppModule](Set-DataverseAppModule.md)
Creates or updates an app module (model-driven app) in Dataverse.

### [Set-DataverseAppModuleComponent](Set-DataverseAppModuleComponent.md)
Creates or updates an app module component in Dataverse.

### [Set-DataverseAppmoduleIconFromSet](Set-DataverseAppmoduleIconFromSet.md)
Sets an app module's icon by downloading an icon from an online icon set and creating/updating a web resource.

### [Set-DataverseAttributeMetadata](Set-DataverseAttributeMetadata.md)
Creates or updates an attribute (column) in Dataverse.

### [Set-DataverseCanvasApp](Set-DataverseCanvasApp.md)
Creates or updates a Canvas app in a Dataverse environment.

### [Set-DataverseConnectionAsDefault](Set-DataverseConnectionAsDefault.md)
Sets the specified Dataverse connection as the default connection for cmdlets that don't specify a connection.

### [Set-DataverseConnectionReference](Set-DataverseConnectionReference.md)
Creates or updates connection reference values in Dataverse.

### [Set-DataverseDynamicPluginAssembly](Set-DataverseDynamicPluginAssembly.md)
Compiles C# source code into a plugin assembly, uploads to Dataverse, and automatically manages plugin types.

### [Set-DataverseEntityKeyMetadata](Set-DataverseEntityKeyMetadata.md)
Creates an alternate key on an entity (table) in Dataverse.

### [Set-DataverseEntityMetadata](Set-DataverseEntityMetadata.md)
Creates or updates an entity (table) in Dataverse.

### [Set-DataverseEnvironmentVariableDefinition](Set-DataverseEnvironmentVariableDefinition.md)
Creates or updates environment variable definitions in Dataverse.

### [Set-DataverseEnvironmentVariableValue](Set-DataverseEnvironmentVariableValue.md)
Sets environment variable values in Dataverse.

### [Set-DataverseFileData](Set-DataverseFileData.md)
Uploads file data to a Dataverse file column.

### [Set-DataverseForm](Set-DataverseForm.md)
Creates or updates a form in a Dataverse environment.

### [Set-DataverseFormControl](Set-DataverseFormControl.md)
Creates or updates a control in a Dataverse form section or header.

### [Set-DataverseFormEventHandler](Set-DataverseFormEventHandler.md)
Adds or updates an event handler in a Dataverse form (form-level, attribute-level, tab-level, or control-level).

### [Set-DataverseFormLibrary](Set-DataverseFormLibrary.md)
Adds or updates a script library in a Dataverse form.

### [Set-DataverseFormSection](Set-DataverseFormSection.md)
Creates or updates a section in a Dataverse form tab.

### [Set-DataverseFormTab](Set-DataverseFormTab.md)
Creates or updates a tab on a Dataverse form with support for column layouts.

### [Set-DataverseMsAppComponent](Set-DataverseMsAppComponent.md)
Adds or updates a component in a .msapp file.

### [Set-DataverseMsAppDataSource](Set-DataverseMsAppDataSource.md)
Adds or updates a data source in a Canvas app .msapp file.

### [Set-DataverseMsAppProperties](Set-DataverseMsAppProperties.md)
Sets the app-level properties of a Canvas app .msapp file using Power Apps YAML format.

### [Set-DataverseMsAppScreen](Set-DataverseMsAppScreen.md)
Adds or updates a screen in a .msapp file.

### [Set-DataverseOptionSetMetadata](Set-DataverseOptionSetMetadata.md)
Creates or updates a global or local option set in Dataverse.

### [Set-DataverseOrganizationSettings](Set-DataverseOrganizationSettings.md)
Updates organization settings in the single organization record in a Dataverse environment.

### [Set-DataversePluginAssembly](Set-DataversePluginAssembly.md)
Creates or updates a plugin assembly in a Dataverse environment.

### [Set-DataversePluginPackage](Set-DataversePluginPackage.md)
Creates or updates a plugin package in a Dataverse environment.

### [Set-DataversePluginStep](Set-DataversePluginStep.md)
Creates or updates a plugin step (SDK message processing step) in a Dataverse environment.

### [Set-DataversePluginStepImage](Set-DataversePluginStepImage.md)
Creates or updates a plugin step image in a Dataverse environment.

### [Set-DataversePluginType](Set-DataversePluginType.md)
Creates or updates a plugin type in a Dataverse environment.

### [Set-DataverseRecord](Set-DataverseRecord.md)
Creates or updates Dataverse records including M:M association/disassociation, status and assignment changes.

### [Set-DataverseRecordAccess](Set-DataverseRecordAccess.md)
Grants or modifies access rights for a security principal (user or team) on a specific record.

### [Set-DataverseRecordsFolder](Set-DataverseRecordsFolder.md)
Writes a list of Dataverse records to a folder of JSON files.

### [Set-DataverseRelationshipMetadata](Set-DataverseRelationshipMetadata.md)
Creates or updates a relationship in Dataverse.

### [Set-DataverseSitemap](Set-DataverseSitemap.md)
Creates or updates a sitemap in Dataverse.

### [Set-DataverseSitemapEntry](Set-DataverseSitemapEntry.md)
Creates or updates an entry (Area, Group, or SubArea) in a Dataverse sitemap.

### [Set-DataverseSolution](Set-DataverseSolution.md)
Creates or updates a solution in Dataverse. Allows setting friendly name, description, version, and publisher.

### [Set-DataverseSolutionComponent](Set-DataverseSolutionComponent.md)
Adds or updates a solution component in an unmanaged solution, with automatic handling of behavior changes.

### [Set-DataverseTableIconFromSet](Set-DataverseTableIconFromSet.md)
Sets a table's vector icon by downloading an icon from an online icon set and creating/updating a web resource.

### [Set-DataverseView](Set-DataverseView.md)
Creates or updates Dataverse views (savedquery and userquery entities) with flexible column and filter configuration.

### [Set-DataverseWebResource](Set-DataverseWebResource.md)
Creates or updates web resources in a Dataverse environment.

### [Test-DataverseRecordAccess](Test-DataverseRecordAccess.md)
Tests the access rights a security principal (user or team) has for a specific record.

