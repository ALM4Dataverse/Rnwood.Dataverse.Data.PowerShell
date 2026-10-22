. "$PSScriptRoot\Common.ps1"

Describe 'Dataverse RibbonCommandDefinition Cmdlets' {
    BeforeAll {
        # Mock connection with request interceptor for ribbon operations
        $requestInterceptor = {
            param($request)
            
            # Handle RetrieveEntityRibbonRequest
            if ($request.GetType().Name -eq 'RetrieveEntityRibbonRequest') {
                $response = New-Object Microsoft.Crm.Sdk.Messages.RetrieveEntityRibbonResponse
                
                # Sample ribbon XML with CommandDefinitions for testing
                $ribbonXml = @"
<RibbonDiffXml>
    <CommandDefinitions>
        <CommandDefinition Id="TestCommand1">
            <EnableRules>
                <EnableRule Id="EnableRule1" />
            </EnableRules>
            <DisplayRules>
                <DisplayRule Id="DisplayRule1" />
            </DisplayRules>
            <Actions>
                <JavaScriptFunction Library="`$webresource:test.js" FunctionName="testFunc1" />
            </Actions>
        </CommandDefinition>
        <CommandDefinition Id="TestCommand2">
            <EnableRules />
            <DisplayRules />
            <Actions>
                <Url Address="https://example.com" />
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
            
            return $null
        }
        
        $connection = getMockConnection -RequestInterceptor $requestInterceptor -Entities @('contact')
    }
    
    Context 'Get-DataverseRibbonCommandDefinition' {
        It 'Retrieves all command definitions from entity ribbon' {
            $results = Get-DataverseRibbonCommandDefinition -Connection $connection -Entity 'contact'
            
            $results | Should -Not -BeNullOrEmpty
            $results.Count | Should -Be 2
            $results[0].Id | Should -Be 'TestCommand1'
            $results[0].Entity | Should -Be 'contact'
            $results[0].EnableRules | Should -Contain 'EnableRule1'
            $results[0].DisplayRules | Should -Contain 'DisplayRule1'
            $results[0].Actions | Should -Not -BeNullOrEmpty
        }
        
        It 'Retrieves specific command definition by ID' {
            $result = Get-DataverseRibbonCommandDefinition -Connection $connection -Entity 'contact' -CommandId 'TestCommand2'
            
            $result | Should -Not -BeNullOrEmpty
            $result.Id | Should -Be 'TestCommand2'
        }
        
        It 'Returns XML property with full command definition element' {
            $result = Get-DataverseRibbonCommandDefinition -Connection $connection -Entity 'contact' -CommandId 'TestCommand1'
            
            $result.Xml | Should -Not -BeNullOrEmpty
            $result.Xml | Should -Match '<CommandDefinition'
            $result.Xml | Should -Match 'Id="TestCommand1"'
        }
    }
    
    Context 'Set-DataverseRibbonCommandDefinition' {
        It 'Requires mandatory parameters' {
            $cmd = Get-Command Set-DataverseRibbonCommandDefinition
            $cmd.Parameters['Id'].Attributes.Mandatory | Should -Contain $true
            $cmd.Parameters['CommandDefinitionXml'].Attributes.Mandatory | Should -Contain $true
        }
    }
    
    Context 'Remove-DataverseRibbonCommandDefinition' {
        It 'Requires mandatory Id parameter' {
            $cmd = Get-Command Remove-DataverseRibbonCommandDefinition
            $cmd.Parameters['Id'].Attributes.Mandatory | Should -Contain $true
        }
    }
}

Describe 'Dataverse RibbonRuleDefinition Cmdlets' {
    BeforeAll {
        $requestInterceptor = {
            param($request)
            
            if ($request.GetType().Name -eq 'RetrieveEntityRibbonRequest') {
                $response = New-Object Microsoft.Crm.Sdk.Messages.RetrieveEntityRibbonResponse
                
                $ribbonXml = @"
<RibbonDiffXml>
    <RuleDefinitions>
        <EnableRules>
            <EnableRule Id="EnableRule1">
                <FormStateRule State="Existing" />
            </EnableRule>
            <EnableRule Id="EnableRule2">
                <CustomRule Library="`$webresource:test.js" FunctionName="checkEnable" />
            </EnableRule>
        </EnableRules>
        <DisplayRules>
            <DisplayRule Id="DisplayRule1">
                <EntityRule EntityName="contact" />
            </DisplayRule>
        </DisplayRules>
    </RuleDefinitions>
</RibbonDiffXml>
"@
                
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
            
            return $null
        }
        
        $connection = getMockConnection -RequestInterceptor $requestInterceptor -Entities @('contact')
    }
    
    Context 'Get-DataverseRibbonRuleDefinition' {
        It 'Retrieves all rule definitions from entity ribbon' {
            $results = Get-DataverseRibbonRuleDefinition -Connection $connection -Entity 'contact'
            
            $results | Should -Not -BeNullOrEmpty
            $results.Count | Should -Be 3  # 2 EnableRules + 1 DisplayRule
        }
        
        It 'Filters by rule type - EnableRule' {
            $results = Get-DataverseRibbonRuleDefinition -Connection $connection -Entity 'contact' -RuleType 'EnableRule'
            
            $results | Should -Not -BeNullOrEmpty
            $results.Count | Should -Be 2
            $results[0].RuleType | Should -Be 'EnableRule'
        }
        
        It 'Filters by rule type - DisplayRule' {
            $results = Get-DataverseRibbonRuleDefinition -Connection $connection -Entity 'contact' -RuleType 'DisplayRule'
            
            $results | Should -Not -BeNullOrEmpty
            $results.Count | Should -Be 1
            $results[0].RuleType | Should -Be 'DisplayRule'
        }
        
        It 'Filters by specific rule ID' {
            $result = Get-DataverseRibbonRuleDefinition -Connection $connection -Entity 'contact' -RuleId 'EnableRule1'
            
            $result | Should -Not -BeNullOrEmpty
            $result.Id | Should -Be 'EnableRule1'
            $result.RuleType | Should -Be 'EnableRule'
        }
        
        It 'Returns conditions with rule' {
            $result = Get-DataverseRibbonRuleDefinition -Connection $connection -Entity 'contact' -RuleId 'EnableRule1'
            
            $result.Conditions | Should -Not -BeNullOrEmpty
            $result.Conditions[0].Type | Should -Be 'FormStateRule'
        }
    }
}
