. "$PSScriptRoot\Common.ps1"

Describe 'Dataverse Ribbon Cmdlets' {
    BeforeAll {
        # Mock connection with request interceptor for ribbon operations
        $requestInterceptor = {
            param($request)
            
            # Handle RetrieveEntityRibbonRequest
            if ($request.GetType().Name -eq 'RetrieveEntityRibbonRequest') {
                $response = New-Object Microsoft.Crm.Sdk.Messages.RetrieveEntityRibbonResponse
                
                # Sample ribbon XML for testing
                $ribbonXml = @"
<RibbonDiffXml>
    <CustomActions>
        <CustomAction Id="TestAction" Location="Mscrm.HomepageGrid.contact.MainTab.Actions.Controls._children" Sequence="10">
            <CommandUIDefinition>
                <Button Id="TestButton" Command="TestCommand" Sequence="10" LabelText="Test Button" TemplateAlias="o1" />
            </CommandUIDefinition>
        </CustomAction>
    </CustomActions>
    <CommandDefinitions>
        <CommandDefinition Id="TestCommand">
            <EnableRules />
            <DisplayRules />
            <Actions>
                <JavaScriptFunction Library="$webresource:test.js" FunctionName="testFunction" />
            </Actions>
        </CommandDefinition>
    </CommandDefinitions>
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
                
                # Sample application ribbon XML
                $ribbonXml = @"
<RibbonDiffXml>
    <CustomActions>
        <CustomAction Id="AppAction" Location="Mscrm.HomepageGrid.MainTab.Actions.Controls._children" Sequence="20">
            <CommandUIDefinition>
                <Button Id="AppButton" Command="AppCommand" Sequence="20" LabelText="App Button" TemplateAlias="o1" />
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
        
        # Only need contact metadata, not ribbondiff (ribbondiff uses normal CRUD operations)
        $connection = getMockConnection -RequestInterceptor $requestInterceptor -Entities @('contact')
    }
    
    Context 'Get-DataverseRibbon' {
        It 'Retrieves entity-specific ribbon' {
            $result = Get-DataverseRibbon -Connection $connection -Entity 'contact'
            
            $result | Should -Not -BeNullOrEmpty
            $result.Entity | Should -Be 'contact'
            $result.IsApplicationRibbon | Should -Be $false
            $result.RibbonDiffXml | Should -Not -BeNullOrEmpty
            $result.RibbonDiffXml | Should -Match 'RibbonDiffXml'
            $result.RibbonDiffXml | Should -Match 'TestAction'
            $result.RibbonDiffXml | Should -Match 'TestButton'
        }
        
        It 'Retrieves application-wide ribbon' {
            $result = Get-DataverseRibbon -Connection $connection
            
            $result | Should -Not -BeNullOrEmpty
            $result.Entity | Should -BeNullOrEmpty
            $result.IsApplicationRibbon | Should -Be $true
            $result.RibbonDiffXml | Should -Not -BeNullOrEmpty
            $result.RibbonDiffXml | Should -Match 'RibbonDiffXml'
            $result.RibbonDiffXml | Should -Match 'AppAction'
            $result.RibbonDiffXml | Should -Match 'AppButton'
        }
        
        It 'Accepts Entity parameter from pipeline' {
            $entityObject = [PSCustomObject]@{
                Entity = 'contact'
            }
            
            $result = $entityObject | Get-DataverseRibbon -Connection $connection
            
            $result | Should -Not -BeNullOrEmpty
            $result.Entity | Should -Be 'contact'
        }
        
        It 'Decompresses GZip-compressed XML correctly' {
            $result = Get-DataverseRibbon -Connection $connection -Entity 'contact'
            
            # Verify XML is valid and decompressed
            $result.RibbonDiffXml | Should -Not -BeNullOrEmpty
            { [xml]$result.RibbonDiffXml } | Should -Not -Throw
            
            $xml = [xml]$result.RibbonDiffXml
            $xml.RibbonDiffXml | Should -Not -BeNullOrEmpty
        }
    }
    
    Context 'Set-DataverseRibbon' {
        It 'Sets entity-specific ribbon' -Skip {
            # Skip: FakeXrmEasy doesn't support ribbondiff table CRUD operations
            # This would require a real Dataverse environment to test
            $ribbonXml = @"
<RibbonDiffXml>
    <CustomActions>
        <CustomAction Id="NewAction" Location="Mscrm.HomepageGrid.contact.MainTab.Actions.Controls._children" Sequence="30">
            <CommandUIDefinition>
                <Button Id="NewButton" Command="NewCommand" Sequence="30" LabelText="New Button" TemplateAlias="o1" />
            </CommandUIDefinition>
        </CustomAction>
    </CustomActions>
</RibbonDiffXml>
"@
            
            { Set-DataverseRibbon -Connection $connection -Entity 'contact' -RibbonDiffXml $ribbonXml -Confirm:$false } | Should -Not -Throw
        }
        
        It 'Sets application-wide ribbon' -Skip {
            # Skip: FakeXrmEasy doesn't support ribbondiff table CRUD operations
            # This would require a real Dataverse environment to test
            $ribbonXml = @"
<RibbonDiffXml>
    <CustomActions>
        <CustomAction Id="GlobalAction" Location="Mscrm.HomepageGrid.MainTab.Actions.Controls._children" Sequence="40">
            <CommandUIDefinition>
                <Button Id="GlobalButton" Command="GlobalCommand" Sequence="40" LabelText="Global Button" TemplateAlias="o1" />
            </CommandUIDefinition>
        </CustomAction>
    </CustomActions>
</RibbonDiffXml>
"@
            
            { Set-DataverseRibbon -Connection $connection -RibbonDiffXml $ribbonXml -Confirm:$false } | Should -Not -Throw
        }
        
        It 'Requires RibbonDiffXml parameter' {
            # PowerShell prompts for mandatory parameters when they're missing
            # We can test this by checking the parameter is indeed mandatory
            $cmdlet = Get-Command Set-DataverseRibbon
            $ribbonDiffXmlParam = $cmdlet.Parameters['RibbonDiffXml']
            $ribbonDiffXmlParam.Attributes.Mandatory | Should -Contain $true
        }
        
        It 'Accepts RibbonDiffXml from pipeline' -Skip {
            # Skip: FakeXrmEasy doesn't support ribbondiff table CRUD operations
            $ribbonObject = [PSCustomObject]@{
                Entity = 'contact'
                RibbonDiffXml = '<RibbonDiffXml><CustomActions /></RibbonDiffXml>'
            }
            
            { $ribbonObject | Set-DataverseRibbon -Connection $connection -Confirm:$false } | Should -Not -Throw
        }
        
        It 'Supports WhatIf' {
            $ribbonXml = '<RibbonDiffXml><CustomActions /></RibbonDiffXml>'
            
            { Set-DataverseRibbon -Connection $connection -Entity 'contact' -RibbonDiffXml $ribbonXml -WhatIf } | Should -Not -Throw
        }
    }
    
    Context 'Ribbon XML Schema' {
        It 'Accepts valid ribbon schema with CustomActions' -Skip {
            # Skip: FakeXrmEasy doesn't support ribbondiff table CRUD operations
            $ribbonXml = @"
<RibbonDiffXml>
    <CustomActions>
        <CustomAction Id="Test.Action" Location="Mscrm.HomepageGrid.contact.MainTab.Actions.Controls._children" Sequence="10" Title="Test">
            <CommandUIDefinition>
                <Button Id="Test.Button" Command="Test.Command" Sequence="10" LabelText="Test" />
            </CommandUIDefinition>
        </CustomAction>
        <HideCustomAction HideActionId="Mscrm.DeletePrimaryRecord" Location="Mscrm.HomepageGrid.contact.MainTab.Actions.Controls._children" />
    </CustomActions>
</RibbonDiffXml>
"@
            
            { Set-DataverseRibbon -Connection $connection -Entity 'contact' -RibbonDiffXml $ribbonXml -Confirm:$false } | Should -Not -Throw
        }
        
        It 'Accepts valid ribbon schema with CommandDefinitions and RuleDefinitions' -Skip {
            # Skip: FakeXrmEasy doesn't support ribbondiff table CRUD operations
            $ribbonXml = @"
<RibbonDiffXml>
    <CommandDefinitions>
        <CommandDefinition Id="Test.Command">
            <EnableRules>
                <EnableRule Id="Test.EnableRule" />
            </EnableRules>
            <DisplayRules>
                <DisplayRule Id="Test.DisplayRule" />
            </DisplayRules>
            <Actions>
                <JavaScriptFunction Library="`$webresource:test.js" FunctionName="testFunc" />
            </Actions>
        </CommandDefinition>
    </CommandDefinitions>
    <RuleDefinitions>
        <EnableRules>
            <EnableRule Id="Test.EnableRule">
                <CustomRule Library="`$webresource:test.js" FunctionName="enableFunc" />
            </EnableRule>
        </EnableRules>
        <DisplayRules>
            <DisplayRule Id="Test.DisplayRule">
                <FormStateRule State="Existing" />
            </DisplayRule>
        </DisplayRules>
    </RuleDefinitions>
</RibbonDiffXml>
"@
            
            { Set-DataverseRibbon -Connection $connection -Entity 'contact' -RibbonDiffXml $ribbonXml -Confirm:$false } | Should -Not -Throw
        }
    }
}
