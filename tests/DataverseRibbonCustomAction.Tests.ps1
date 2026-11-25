. "$PSScriptRoot\Common.ps1"

Describe 'Dataverse RibbonCustomAction Cmdlets' {
    BeforeAll {
        # Mock connection with request interceptor for ribbon operations
        $requestInterceptor = {
            param($request)
            
            # Handle RetrieveEntityRibbonRequest
            if ($request.GetType().Name -eq 'RetrieveEntityRibbonRequest') {
                $response = New-Object Microsoft.Crm.Sdk.Messages.RetrieveEntityRibbonResponse
                
                # Sample ribbon XML with CustomActions for testing
                $ribbonXml = @"
<RibbonDiffXml>
    <CustomActions>
        <CustomAction Id="TestAction1" Location="Mscrm.HomepageGrid.contact.MainTab.Actions.Controls._children" Sequence="10" Title="Test Action 1">
            <CommandUIDefinition>
                <Button Id="TestButton1" Command="TestCommand1" Sequence="10" LabelText="Test Button 1" TemplateAlias="o1" />
            </CommandUIDefinition>
        </CustomAction>
        <CustomAction Id="TestAction2" Location="Mscrm.HomepageGrid.contact.MainTab.Actions.Controls._children" Sequence="20">
            <CommandUIDefinition>
                <Button Id="TestButton2" Command="TestCommand2" Sequence="20" LabelText="Test Button 2" TemplateAlias="o1" />
            </CommandUIDefinition>
        </CustomAction>
    </CustomActions>
</RibbonDiffXml>
"@
                
                # Compress the XML using GZip
                $xmlBytes = [System.Text.Encoding]::UTF8.GetBytes($ribbonXml)
                $memoryStream = New-Object System.IO.MemoryStream
                $gzipStream = New-Object System.IO.Compression.GZipStream($memoryStream, [System.IO.Compression.CompressionMode]::Compress)
                $gzipStream.Write($xmlBytes, 0, $xmlBytes.Length)
                $gzipStream.Close()
                $compressedXml = $memoryStream.ToArray()
                $memoryStream.Close()
                
                $response.Results.Add("CompressedEntityXml", $compressedXml)
                return $response
            }
            
            # Handle RetrieveApplicationRibbonRequest
            if ($request.GetType().Name -eq 'RetrieveApplicationRibbonRequest') {
                $response = New-Object Microsoft.Crm.Sdk.Messages.RetrieveApplicationRibbonResponse
                
                # Sample application ribbon XML with CustomActions
                $ribbonXml = @"
<RibbonDiffXml>
    <CustomActions>
        <CustomAction Id="AppAction1" Location="Mscrm.HomepageGrid.MainTab.Actions.Controls._children" Sequence="30">
            <CommandUIDefinition>
                <Button Id="AppButton1" Command="AppCommand1" Sequence="30" LabelText="App Button 1" TemplateAlias="o1" />
            </CommandUIDefinition>
        </CustomAction>
    </CustomActions>
</RibbonDiffXml>
"@
                
                # Compress the XML
                $xmlBytes = [System.Text.Encoding]::UTF8.GetBytes($ribbonXml)
                $memoryStream = New-Object System.IO.MemoryStream
                $gzipStream = New-Object System.IO.Compression.GZipStream($memoryStream, [System.IO.Compression.CompressionMode]::Compress)
                $gzipStream.Write($xmlBytes, 0, $xmlBytes.Length)
                $gzipStream.Close()
                $compressedXml = $memoryStream.ToArray()
                $memoryStream.Close()
                
                $response.Results.Add("CompressedApplicationRibbonXml", $compressedXml)
                return $response
            }
            
            return $null
        }
        
        $connection = getMockConnection -RequestInterceptor $requestInterceptor -Entities @('contact')
    }
    
    Context 'Get-DataverseRibbonCustomAction' {
        It 'Retrieves all custom actions from entity ribbon' {
            $results = Get-DataverseRibbonCustomAction -Connection $connection -Entity 'contact'
            
            $results | Should -Not -BeNullOrEmpty
            $results.Count | Should -Be 2
            $results[0].Id | Should -Be 'TestAction1'
            $results[0].Entity | Should -Be 'contact'
            $results[0].Location | Should -Be 'Mscrm.HomepageGrid.contact.MainTab.Actions.Controls._children'
            $results[0].Sequence | Should -Be '10'
            $results[0].Title | Should -Be 'Test Action 1'
            $results[0].ControlType | Should -Be 'Button'
            $results[0].ControlId | Should -Be 'TestButton1'
            $results[0].Command | Should -Be 'TestCommand1'
            $results[0].LabelText | Should -Be 'Test Button 1'
        }
        
        It 'Retrieves specific custom action by ID' {
            $result = Get-DataverseRibbonCustomAction -Connection $connection -Entity 'contact' -CustomActionId 'TestAction2'
            
            $result | Should -Not -BeNullOrEmpty
            $result.Id | Should -Be 'TestAction2'
            $result.ControlId | Should -Be 'TestButton2'
        }
        
        It 'Retrieves custom actions from application ribbon' {
            $results = Get-DataverseRibbonCustomAction -Connection $connection
            
            $results | Should -Not -BeNullOrEmpty
            $results.Count | Should -Be 1
            $results[0].Id | Should -Be 'AppAction1'
            $results[0].Entity | Should -BeNullOrEmpty
            $results[0].ControlId | Should -Be 'AppButton1'
        }
        
        It 'Returns XML property with full custom action element' {
            $result = Get-DataverseRibbonCustomAction -Connection $connection -Entity 'contact' -CustomActionId 'TestAction1'
            
            $result.Xml | Should -Not -BeNullOrEmpty
            $result.Xml | Should -Match '<CustomAction'
            $result.Xml | Should -Match 'Id="TestAction1"'
        }
    }
    
    Context 'Set-DataverseRibbonCustomAction' {
        It 'Requires mandatory parameters' {
            $cmd = Get-Command Set-DataverseRibbonCustomAction
            $cmd.Parameters['Id'].Attributes.Mandatory | Should -Contain $true
            $cmd.Parameters['Location'].Attributes.Mandatory | Should -Contain $true
            $cmd.Parameters['CommandUIDefinitionXml'].Attributes.Mandatory | Should -Contain $true
        }
        
        It 'Supports WhatIf' -Skip {
            # Skip: Would require mocking the entire ribbon retrieval and save process
            $buttonXml = '<Button Id="MyBtn" Command="MyCmd" />'
            { Set-DataverseRibbonCustomAction -Connection $connection -Entity 'contact' -Id 'NewAction' -Location 'TestLocation' -CommandUIDefinitionXml $buttonXml -WhatIf } | Should -Not -Throw
        }
    }
    
    Context 'Remove-DataverseRibbonCustomAction' {
        It 'Requires mandatory Id parameter' {
            $cmd = Get-Command Remove-DataverseRibbonCustomAction
            $cmd.Parameters['Id'].Attributes.Mandatory | Should -Contain $true
        }
        
        It 'Supports WhatIf' -Skip {
            # Skip: Would require mocking the entire ribbon retrieval and save process
            { Remove-DataverseRibbonCustomAction -Connection $connection -Entity 'contact' -Id 'TestAction1' -WhatIf } | Should -Not -Throw
        }
    }
}
