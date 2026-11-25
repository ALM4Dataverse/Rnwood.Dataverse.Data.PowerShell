. $PSScriptRoot/Common.ps1

Describe 'Get-DataverseAdminPortalUrl' {
    Context 'Admin Portal URL Generation' {
        It "Generates URL for environment page" {
            $connection = getMockConnection
            
            $url = Get-DataverseAdminPortalUrl -Connection $connection
            
            # Verify URL structure
            $url | Should -Not -BeNullOrEmpty
            $url | Should -BeLike "https://admin.powerplatform.microsoft.com/environments/*"
            $url | Should -BeLike "*/hub"
        }

        It "Includes environment ID from connection" {
            $connection = getMockConnection
            
            # Get the organization ID (which is the environment ID)
            $whoami = Get-DataverseWhoAmI -Connection $connection
            $envId = $whoami.OrganizationId
            
            $url = Get-DataverseAdminPortalUrl -Connection $connection
            
            # Verify URL includes the environment ID
            $url | Should -BeLike "*/environments/$envId/hub"
        }

        It "Works with default connection" {
            $connection = getMockConnection
            Set-DataverseConnectionAsDefault -Connection $connection
            
            # Call without explicit connection
            $url = Get-DataverseAdminPortalUrl
            
            # Verify URL generated
            $url | Should -Not -BeNullOrEmpty
            $url | Should -BeLike "https://admin.powerplatform.microsoft.com/*"
        }
    }
}
