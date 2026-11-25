. $PSScriptRoot/Common.ps1

Describe "Get-DataverseCloudFlow" {
    It "Retrieves all cloud flows" {
        # Create a real entity using SDK
        $flow1 = New-Object Microsoft.Xrm.Sdk.Entity("workflow", [Guid]::NewGuid())
        $flow1.Attributes.Add("name", "Test Flow 1")
        $flow1.Attributes.Add("description", "Test Description")
        $flow1.Attributes.Add("category", (New-Object Microsoft.Xrm.Sdk.OptionSetValue(5)))
        $flow1.Attributes.Add("statecode", (New-Object Microsoft.Xrm.Sdk.OptionSetValue(1)))
        $flow1.Attributes.Add("statuscode", (New-Object Microsoft.Xrm.Sdk.OptionSetValue(2)))
        $flow1.Attributes.Add("type", (New-Object Microsoft.Xrm.Sdk.OptionSetValue(1)))
        $flow1.Attributes.Add("primaryentity", "contact")
        $flow1.Attributes.Add("ownerid", (New-Object Microsoft.Xrm.Sdk.EntityReference("systemuser", [Guid]::NewGuid())))
        $flow1.Attributes.Add("createdon", [DateTime]::UtcNow.AddDays(-10))
        $flow1.Attributes.Add("modifiedon", [DateTime]::UtcNow)
        
        $connection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.GetType().Name -eq 'RetrieveMultipleRequest') {
                $query = $request.Query
                if ($query.EntityName -eq 'workflow') {
                    # Return mock cloud flow
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                    $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                    $entityCollection.Entities.Add($flow1)
                    $response.Results.Add("EntityCollection", $entityCollection)
                    return $response
                }
            }
            
            return $null
        }
        
        $flows = Get-DataverseCloudFlow -Connection $connection
        $flows | Should -Not -BeNullOrEmpty
        $flows.Name | Should -Be "Test Flow 1"
        $flows.State | Should -Be "Activated"
        $flows.Category | Should -Be 5
    }
    
    It "Filters by name" {
        # Create a real entity using SDK
        $flow1 = New-Object Microsoft.Xrm.Sdk.Entity("workflow", [Guid]::NewGuid())
        $flow1.Attributes.Add("name", "Specific Flow")
        $flow1.Attributes.Add("category", (New-Object Microsoft.Xrm.Sdk.OptionSetValue(5)))
        $flow1.Attributes.Add("statecode", (New-Object Microsoft.Xrm.Sdk.OptionSetValue(0)))
        $flow1.Attributes.Add("statuscode", (New-Object Microsoft.Xrm.Sdk.OptionSetValue(1)))
        $flow1.Attributes.Add("type", (New-Object Microsoft.Xrm.Sdk.OptionSetValue(1)))
        $flow1.Attributes.Add("primaryentity", "none")
        $flow1.Attributes.Add("ownerid", (New-Object Microsoft.Xrm.Sdk.EntityReference("systemuser", [Guid]::NewGuid())))
        
        $connection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.GetType().Name -eq 'RetrieveMultipleRequest') {
                $query = $request.Query
                if ($query.EntityName -eq 'workflow') {
                    # Verify filter is applied
                    $nameFilter = $query.Criteria.Conditions | Where-Object { $_.AttributeName -eq 'name' }
                    $nameFilter | Should -Not -BeNullOrEmpty
                    $nameFilter.Values[0] | Should -Be "Specific Flow"
                    
                    # Return filtered result
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                    $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                    $entityCollection.Entities.Add($flow1)
                    $response.Results.Add("EntityCollection", $entityCollection)
                    return $response
                }
            }
            
            return $null
        }
        
        $flows = Get-DataverseCloudFlow -Connection $connection -Name "Specific Flow"
        $flows | Should -Not -BeNullOrEmpty
        $flows.Name | Should -Be "Specific Flow"
    }
    
    It "Filters by Activated state" {
        # Create a real entity using SDK
        $flow1 = New-Object Microsoft.Xrm.Sdk.Entity("workflow", [Guid]::NewGuid())
        $flow1.Attributes.Add("name", "Active Flow")
        $flow1.Attributes.Add("category", (New-Object Microsoft.Xrm.Sdk.OptionSetValue(5)))
        $flow1.Attributes.Add("statecode", (New-Object Microsoft.Xrm.Sdk.OptionSetValue(1)))
        $flow1.Attributes.Add("statuscode", (New-Object Microsoft.Xrm.Sdk.OptionSetValue(2)))
        $flow1.Attributes.Add("type", (New-Object Microsoft.Xrm.Sdk.OptionSetValue(1)))
        $flow1.Attributes.Add("primaryentity", "none")
        $flow1.Attributes.Add("ownerid", (New-Object Microsoft.Xrm.Sdk.EntityReference("systemuser", [Guid]::NewGuid())))
        
        $connection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.GetType().Name -eq 'RetrieveMultipleRequest') {
                $query = $request.Query
                if ($query.EntityName -eq 'workflow') {
                    # Verify state filter
                    $stateFilter = $query.Criteria.Conditions | Where-Object { $_.AttributeName -eq 'statecode' }
                    $stateFilter | Should -Not -BeNullOrEmpty
                    $stateFilter.Values[0] | Should -Be 1
                    
                    # Return activated flows
                    $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveMultipleResponse
                    $entityCollection = New-Object Microsoft.Xrm.Sdk.EntityCollection
                    $entityCollection.Entities.Add($flow1)
                    $response.Results.Add("EntityCollection", $entityCollection)
                    return $response
                }
            }
            
            return $null
        }
        
        $flows = Get-DataverseCloudFlow -Connection $connection -Activated
        $flows | Should -Not -BeNullOrEmpty
        $flows.State | Should -Be "Activated"
    }
}

Describe "Set-DataverseCloudFlow" {
    It "Updates flow name" {
        $flowId = [Guid]::NewGuid()
        
        # Create a real entity using SDK
        $flow = New-Object Microsoft.Xrm.Sdk.Entity("workflow", $flowId)
        $flow.Attributes.Add("name", "Old Name")
        $flow.Attributes.Add("statecode", (New-Object Microsoft.Xrm.Sdk.OptionSetValue(0)))
        $flow.Attributes.Add("statuscode", (New-Object Microsoft.Xrm.Sdk.OptionSetValue(1)))
        
        $connection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.GetType().Name -eq 'RetrieveRequest') {
                # Return existing flow
                $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveResponse
                $response.Results.Add("Entity", $flow)
                return $response
            }
            
            if ($request.GetType().Name -eq 'UpdateRequest') {
                # Verify update
                $entity = $request.Target
                $entity.Attributes["name"] | Should -Be "New Name"
                
                $response = New-Object Microsoft.Xrm.Sdk.Messages.UpdateResponse
                return $response
            }
            
            return $null
        }
        
        $result = Set-DataverseCloudFlow -Connection $connection -Id $flowId -NewName "New Name"
        $result | Should -Match "updated successfully"
    }
    
    It "Activates a flow" {
        $flowId = [Guid]::NewGuid()
        
        # Create a real entity using SDK
        $flow = New-Object Microsoft.Xrm.Sdk.Entity("workflow", $flowId)
        $flow.Attributes.Add("name", "Test Flow")
        $flow.Attributes.Add("statecode", (New-Object Microsoft.Xrm.Sdk.OptionSetValue(0)))
        $flow.Attributes.Add("statuscode", (New-Object Microsoft.Xrm.Sdk.OptionSetValue(1)))
        
        $connection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.GetType().Name -eq 'RetrieveRequest') {
                $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveResponse
                $response.Results.Add("Entity", $flow)
                return $response
            }
            
            if ($request.GetType().Name -eq 'SetStateRequest') {
                # Verify activation
                $request.State.Value | Should -Be 1
                $request.Status.Value | Should -Be 2
                
                $response = New-Object Microsoft.Crm.Sdk.Messages.SetStateResponse
                return $response
            }
            
            return $null
        }
        
        $result = Set-DataverseCloudFlow -Connection $connection -Id $flowId -Activate
        $result | Should -Match "updated successfully"
    }
}

Describe "Remove-DataverseCloudFlow" {
    It "Deletes a flow by ID" {
        $flowId = [Guid]::NewGuid()
        $global:testDeleted = $false
        
        $connection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.GetType().Name -eq 'RetrieveRequest') {
                $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveResponse
                $flow = New-Object Microsoft.Xrm.Sdk.Entity "workflow", $flowId
                $flow.Attributes["name"] = "Flow To Delete"
                $response.Results.Add("Entity", $flow)
                return $response
            }
            
            if ($request.GetType().Name -eq 'DeleteRequest') {
                $global:testDeleted = $true
                $request.Target.Id | Should -Be $flowId
                
                $response = New-Object Microsoft.Xrm.Sdk.Messages.DeleteResponse
                return $response
            }
            
            return $null
        }
        
        $result = Remove-DataverseCloudFlow -Connection $connection -Id $flowId -Confirm:$false
        $result | Should -Match "removed successfully"
        $global:testDeleted | Should -Be $true
    }
}

Describe "Get-DataverseCloudFlowAction" {
    It "Retrieves actions from flow definition" {
        $flowId = [Guid]::NewGuid()
        $clientData = @'
{
    "properties": {
        "definition": {
            "actions": {
                "Send_email": {
                    "type": "OpenApiConnection",
                    "inputs": {
                        "host": "test",
                        "parameters": {
                            "operationId": "SendEmailV2"
                        }
                    },
                    "runAfter": {},
                    "metadata": {
                        "description": "Send an email notification"
                    }
                },
                "Get_record": {
                    "type": "OpenApiConnection",
                    "inputs": {
                        "host": "test"
                    },
                    "runAfter": {}
                }
            }
        }
    }
}
'@
        
        $connection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.GetType().Name -eq 'RetrieveRequest') {
                $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveResponse
                $flow = New-Object Microsoft.Xrm.Sdk.Entity "workflow", $flowId
                $flow["name"] = "Test Flow"
                $flow["clientdata"] = $clientData
                $response.Results.Add("Entity", $flow)
                return $response
            }
            
            return $null
        }
        
        $actions = Get-DataverseCloudFlowAction -Connection $connection -FlowId $flowId
        $actions | Should -Not -BeNullOrEmpty
        $actions.Count | Should -Be 2
        $actions[0].Name | Should -Be "Send_email"
        $actions[0].Type | Should -Be "OpenApiConnection"
        $actions[0].Description | Should -Be "Send an email notification"
    }
    
    It "Filters actions by name pattern" {
        $flowId = [Guid]::NewGuid()
        $clientData = @'
{
    "properties": {
        "definition": {
            "actions": {
                "Send_email": {
                    "type": "OpenApiConnection",
                    "inputs": {}
                },
                "Get_record": {
                    "type": "OpenApiConnection",
                    "inputs": {}
                }
            }
        }
    }
}
'@
        
        $connection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.GetType().Name -eq 'RetrieveRequest') {
                $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveResponse
                $flow = New-Object Microsoft.Xrm.Sdk.Entity "workflow", $flowId
                $flow["name"] = "Test Flow"
                $flow["clientdata"] = $clientData
                $response.Results.Add("Entity", $flow)
                return $response
            }
            
            return $null
        }
        
        $actions = Get-DataverseCloudFlowAction -Connection $connection -FlowId $flowId -ActionName "Send*"
        $actions | Should -Not -BeNullOrEmpty
        $actions.Count | Should -Be 1
        $actions[0].Name | Should -Be "Send_email"
    }
}

Describe "Set-DataverseCloudFlowAction" {
    It "Updates action inputs" {
        $flowId = [Guid]::NewGuid()
        $clientData = @'
{
    "properties": {
        "definition": {
            "actions": {
                "Send_email": {
                    "type": "OpenApiConnection",
                    "inputs": {
                        "to": "old@example.com"
                    }
                }
            }
        }
    }
}
'@
        
        $connection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.GetType().Name -eq 'RetrieveRequest') {
                $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveResponse
                $flow = New-Object Microsoft.Xrm.Sdk.Entity "workflow", $flowId
                $flow["name"] = "Test Flow"
                $flow["clientdata"] = $clientData
                $response.Results.Add("Entity", $flow)
                return $response
            }
            
            if ($request.GetType().Name -eq 'UpdateRequest') {
                $entity = $request.Target
                $updatedData = $entity["clientdata"]
                $updatedData | Should -Match "new@example.com"
                
                $response = New-Object Microsoft.Xrm.Sdk.Messages.UpdateResponse
                return $response
            }
            
            return $null
        }
        
        $newInputs = @{ to = "new@example.com" }
        $result = Set-DataverseCloudFlowAction -Connection $connection -FlowId $flowId -ActionId "Send_email" -Inputs $newInputs
        $result | Should -Match "updated successfully"
    }
}

Describe "Remove-DataverseCloudFlowAction" {
    It "Removes action from flow definition" {
        $flowId = [Guid]::NewGuid()
        $clientData = @'
{
    "properties": {
        "definition": {
            "actions": {
                "Send_email": {
                    "type": "OpenApiConnection",
                    "inputs": {}
                },
                "Get_record": {
                    "type": "OpenApiConnection",
                    "inputs": {}
                }
            }
        }
    }
}
'@
        
        $connection = getMockConnection -RequestInterceptor {
            param($request)
            
            if ($request.GetType().Name -eq 'RetrieveRequest') {
                $response = New-Object Microsoft.Xrm.Sdk.Messages.RetrieveResponse
                $flow = New-Object Microsoft.Xrm.Sdk.Entity "workflow", $flowId
                $flow["name"] = "Test Flow"
                $flow["clientdata"] = $clientData
                $response.Results.Add("Entity", $flow)
                return $response
            }
            
            if ($request.GetType().Name -eq 'UpdateRequest') {
                $entity = $request.Target
                $updatedData = $entity["clientdata"]
                $updatedData | Should -Not -Match "Send_email"
                $updatedData | Should -Match "Get_record"
                
                $response = New-Object Microsoft.Xrm.Sdk.Messages.UpdateResponse
                return $response
            }
            
            return $null
        }
        
        $result = Remove-DataverseCloudFlowAction -Connection $connection -FlowId $flowId -ActionId "Send_email" -Confirm:$false
        $result | Should -Match "removed"
    }
}
