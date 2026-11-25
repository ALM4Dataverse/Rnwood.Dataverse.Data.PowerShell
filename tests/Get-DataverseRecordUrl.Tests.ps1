. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseRecordUrl' {
    Context 'URL Generation for Records' {
        It "Generates URL for existing record with ID" {
            $connection = getMockConnection
            
            # Generate URL for a specific contact record
            $recordId = [Guid]::NewGuid()
            $url = Get-DataverseRecordUrl -Connection $connection -TableName "contact" -Id $recordId
            
            # Verify URL structure
            $url | Should -Not -BeNullOrEmpty
            $url | Should -BeLike "*main.aspx*"
            $url | Should -BeLike "*etn=contact*"
            $url | Should -BeLike "*id=$recordId*"
            $url | Should -BeLike "*pagetype=entityrecord*"
        }

        It "Generates URL for creating new record without ID" {
            $connection = getMockConnection
            
            # Generate URL for creating a new account record
            $url = Get-DataverseRecordUrl -Connection $connection -TableName "account"
            
            # Verify URL structure (no ID parameter)
            $url | Should -Not -BeNullOrEmpty
            $url | Should -BeLike "*main.aspx*"
            $url | Should -BeLike "*etn=account*"
            $url | Should -Not -BeLike "*id=*"
            $url | Should -BeLike "*pagetype=entityrecord*"
        }

        It "Includes AppId parameter when provided" {
            $connection = getMockConnection
            
            $recordId = [Guid]::NewGuid()
            $appId = [Guid]::NewGuid()
            $url = Get-DataverseRecordUrl -Connection $connection -TableName "contact" -Id $recordId -AppId $appId
            
            # Verify URL includes appid
            $url | Should -BeLike "*appid=$appId*"
        }

        It "Resolves AppUniqueName to AppId" {
            $connection = getMockConnection
            
            # Create a mock app module
            $appId = [Guid]::NewGuid()
            $appModule = @{
                appmoduleid = $appId
                uniquename = "testapp_123"
                name = "Test App"
            } | Set-DataverseRecord -Connection $connection -TableName appmodule -CreateOnly -PassThru
            
            $recordId = [Guid]::NewGuid()
            $url = Get-DataverseRecordUrl -Connection $connection -TableName "contact" -Id $recordId -AppUniqueName "testapp_123"
            
            # Verify URL includes the resolved appid
            $url | Should -BeLike "*appid=$appId*"
        }

        It "Includes FormId parameter when provided" {
            $connection = getMockConnection
            
            $recordId = [Guid]::NewGuid()
            $formId = [Guid]::NewGuid()
            $url = Get-DataverseRecordUrl -Connection $connection -TableName "contact" -Id $recordId -FormId $formId
            
            # Verify URL includes formid
            $url | Should -BeLike "*formid=$formId*"
        }

        It "Includes both AppId and FormId when provided" {
            $connection = getMockConnection
            
            $recordId = [Guid]::NewGuid()
            $appId = [Guid]::NewGuid()
            $formId = [Guid]::NewGuid()
            $url = Get-DataverseRecordUrl -Connection $connection -TableName "contact" -Id $recordId -AppId $appId -FormId $formId
            
            # Verify URL includes both parameters
            $url | Should -BeLike "*appid=$appId*"
            $url | Should -BeLike "*formid=$formId*"
        }

        It "Works with pipeline input from Get-DataverseRecord" {
            $connection = getMockConnection -Entities contact
            
            # Create test records
            $contact1 = @{ firstname = "John"; lastname = "Doe" } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            $contact2 = @{ firstname = "Jane"; lastname = "Smith" } | Set-DataverseRecord -Connection $connection -TableName contact -CreateOnly -PassThru
            
            # Generate URLs from pipeline
            $urls = Get-DataverseRecord -Connection $connection -TableName contact | Get-DataverseRecordUrl -Connection $connection -TableName contact
            
            # Verify URLs generated for both records
            $urls | Should -HaveCount 2
            $urls[0] | Should -BeLike "*id=$($contact1.Id)*"
            $urls[1] | Should -BeLike "*id=$($contact2.Id)*"
        }

        It "Uses connection's organization URL" {
            $connection = getMockConnection
            
            $url = Get-DataverseRecordUrl -Connection $connection -TableName "contact"
            
            # Verify URL uses the connection's org URL
            $url | Should -BeLike "https://*"
        }

        It "Works with default connection" {
            $connection = getMockConnection
            Set-DataverseConnectionAsDefault -Connection $connection
            
            # Call without explicit connection
            $recordId = [Guid]::NewGuid()
            $url = Get-DataverseRecordUrl -TableName "contact" -Id $recordId
            
            # Verify URL generated
            $url | Should -Not -BeNullOrEmpty
            $url | Should -BeLike "*id=$recordId*"
        }
    }
}
