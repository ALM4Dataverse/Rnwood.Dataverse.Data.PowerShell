. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseMakerPortalUrl' {
    Context 'Maker Portal URL Generation' {
        It "Generates URL for maker portal home page by default" {
            $connection = getMockConnection
            
            $url = Get-DataverseMakerPortalUrl -Connection $connection
            
            # Verify URL structure
            $url | Should -Not -BeNullOrEmpty
            $url | Should -BeLike "https://make.powerapps.com/environments/*"
            $url | Should -BeLike "*/home"
        }

        It "Generates URL for a specific table" {
            $connection = getMockConnection
            
            $url = Get-DataverseMakerPortalUrl -Connection $connection -TableName "contact"
            
            # Verify URL points to the table
            $url | Should -BeLike "*/entities/entity/contact"
        }

        It "Includes environment ID from connection" {
            $connection = getMockConnection
            
            # Get the organization ID (which is the environment ID)
            $whoami = Get-DataverseWhoAmI -Connection $connection
            $envId = $whoami.OrganizationId
            
            $url = Get-DataverseMakerPortalUrl -Connection $connection
            
            # Verify URL includes the environment ID
            $url | Should -BeLike "*/environments/$envId/*"
        }

        It "Works with default connection" {
            $connection = getMockConnection
            Set-DataverseConnectionAsDefault -Connection $connection
            
            # Call without explicit connection
            $url = Get-DataverseMakerPortalUrl
            
            # Verify URL generated
            $url | Should -Not -BeNullOrEmpty
            $url | Should -BeLike "https://make.powerapps.com/*"
        }

        It "Supports pipeline input for TableName" {
            $connection = getMockConnection -Entities contact
            
            # Create a table metadata object with LogicalName property
            $metadata = Get-DataverseEntityMetadata -Connection $connection -TableName "contact"
            
            $url = $metadata | Get-DataverseMakerPortalUrl -Connection $connection
            
            # Verify URL includes the table
            $url | Should -BeLike "*/entities/entity/contact"
        }
    }
}
