using Rnwood.Dataverse.Data.PowerShell.E2ETests.Infrastructure;
using Rnwood.Dataverse.Data.PowerShell.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace Rnwood.Dataverse.Data.PowerShell.E2ETests.CloudFlow
{
    /// <summary>
    /// E2E tests for cloud flow cmdlets.
    /// Tests Get-DataverseCloudFlow, Set-DataverseCloudFlow, Remove-DataverseCloudFlow,
    /// and action manipulation cmdlets.
    /// </summary>
    public class CloudFlowCmdletsTests : E2ETestBase
    {
        [Fact]
        public void GetDataverseCloudFlow_Should_ReturnFlows()
        {
            var script = GetConnectionScript(@"
# Test Get-DataverseCloudFlow
Write-Host 'Testing Get-DataverseCloudFlow...'
$flows = Get-DataverseCloudFlow -Connection $connection
Write-Host ""Found $($flows.Count) cloud flows""

if ($flows.Count -eq 0) {
    throw 'Expected at least one cloud flow in the environment'
}

# Verify properties
$flow = $flows[0]
if (-not $flow.Id) {
    throw 'Flow missing Id property'
}
if (-not $flow.Name) {
    throw 'Flow missing Name property'
}
if (-not $flow.State) {
    throw 'Flow missing State property'
}
if ($flow.Category -ne 5) {
    throw ""Expected Category to be 5 (Modern Flow), got $($flow.Category)""
}

Write-Host 'Success: Get-DataverseCloudFlow returned flows with correct properties'
");

            var result = RunScript(script);
            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void GetDataverseCloudFlow_WithNameFilter_Should_ReturnMatchingFlows()
        {
            var script = GetConnectionScript(@"
# Get a flow to test with
$allFlows = Get-DataverseCloudFlow -Connection $connection
if ($allFlows.Count -eq 0) {
    Write-Host 'SKIP: No flows available for testing'
    exit 0
}

$testFlowName = $allFlows[0].Name
Write-Host ""Testing with flow: $testFlowName""

# Test exact name match
$specificFlow = Get-DataverseCloudFlow -Connection $connection -Name $testFlowName
if ($specificFlow.Count -ne 1) {
    throw ""Expected 1 flow, got $($specificFlow.Count)""
}
if ($specificFlow.Name -ne $testFlowName) {
    throw ""Expected flow name '$testFlowName', got '$($specificFlow.Name)'""
}

Write-Host 'Success: Name filtering works correctly'
");

            var result = RunScript(script);
            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().MatchRegex("Success|SKIP");
        }

        [Fact]
        public void GetDataverseCloudFlow_WithStateFilter_Should_FilterByState()
        {
            var script = GetConnectionScript(@"
# Test state filtering
Write-Host 'Testing state filters...'

$draftFlows = Get-DataverseCloudFlow -Connection $connection -Draft
Write-Host ""Found $($draftFlows.Count) draft flows""

$activatedFlows = Get-DataverseCloudFlow -Connection $connection -Activated
Write-Host ""Found $($activatedFlows.Count) activated flows""

# Verify all draft flows have State = 'Draft'
foreach ($flow in $draftFlows) {
    if ($flow.State -ne 'Draft') {
        throw ""Expected draft flow to have State='Draft', got '$($flow.State)'""
    }
}

# Verify all activated flows have State = 'Activated'
foreach ($flow in $activatedFlows) {
    if ($flow.State -ne 'Activated') {
        throw ""Expected activated flow to have State='Activated', got '$($flow.State)'""
    }
}

Write-Host 'Success: State filtering works correctly'
");

            var result = RunScript(script);
            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void GetDataverseCloudFlow_WithIncludeClientData_Should_ReturnClientData()
        {
            var script = GetConnectionScript(@"
# Get a flow to test with
$flows = Get-DataverseCloudFlow -Connection $connection
if ($flows.Count -eq 0) {
    Write-Host 'SKIP: No flows available for testing'
    exit 0
}

$testFlowId = $flows[0].Id
Write-Host ""Testing with flow ID: $testFlowId""

# Get flow without clientdata
$flowWithoutData = Get-DataverseCloudFlow -Connection $connection -Id $testFlowId
if ($flowWithoutData.ClientData) {
    throw 'Expected ClientData to be null when not requested'
}

# Get flow with clientdata
$flowWithData = Get-DataverseCloudFlow -Connection $connection -Id $testFlowId -IncludeClientData
if (-not $flowWithData.ClientData) {
    throw 'Expected ClientData to be present when requested'
}

Write-Host ""ClientData length: $($flowWithData.ClientData.Length) characters""
Write-Host 'Success: IncludeClientData parameter works correctly'
");

            var result = RunScript(script);
            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().MatchRegex("Success|SKIP");
        }

        [Fact]
        public void GetDataverseCloudFlowAction_Should_ReturnActions()
        {
            var script = GetConnectionScript(@"
# Get a flow to test with
$flows = Get-DataverseCloudFlow -Connection $connection
if ($flows.Count -eq 0) {
    Write-Host 'SKIP: No flows available for testing'
    exit 0
}

$testFlow = $flows[0]
Write-Host ""Testing actions for flow: $($testFlow.Name) (ID: $($testFlow.Id))""

# Get actions
$actions = Get-DataverseCloudFlowAction -Connection $connection -FlowId $testFlow.Id
Write-Host ""Found $($actions.Count) actions""

if ($actions.Count -gt 0) {
    # Verify properties
    $action = $actions[0]
    if (-not $action.ActionId) {
        throw 'Action missing ActionId property'
    }
    if (-not $action.Name) {
        throw 'Action missing Name property'
    }
    if (-not $action.Type) {
        throw 'Action missing Type property'
    }
    if ($action.FlowId -ne $testFlow.Id) {
        throw ""Action FlowId mismatch: expected $($testFlow.Id), got $($action.FlowId)""
    }
    
    Write-Host 'Success: Get-DataverseCloudFlowAction returned actions with correct properties'
} else {
    Write-Host 'SKIP: No actions found in flow'
}
");

            var result = RunScript(script);
            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().MatchRegex("Success|SKIP");
        }

        [Fact]
        public void GetDataverseCloudFlowAction_WithActionNameFilter_Should_FilterActions()
        {
            var script = GetConnectionScript(@"
# Get a flow to test with
$flows = Get-DataverseCloudFlow -Connection $connection
if ($flows.Count -eq 0) {
    Write-Host 'SKIP: No flows available for testing'
    exit 0
}

$testFlow = $flows[0]
$allActions = Get-DataverseCloudFlowAction -Connection $connection -FlowId $testFlow.Id

if ($allActions.Count -eq 0) {
    Write-Host 'SKIP: No actions found in flow'
    exit 0
}

$firstActionName = $allActions[0].Name
Write-Host ""Testing with action: $firstActionName""

# Test exact name match
$specificAction = Get-DataverseCloudFlowAction -Connection $connection -FlowId $testFlow.Id -ActionName $firstActionName
if ($specificAction.Count -ne 1) {
    throw ""Expected 1 action, got $($specificAction.Count)""
}
if ($specificAction.Name -ne $firstActionName) {
    throw ""Expected action name '$firstActionName', got '$($specificAction.Name)'""
}

Write-Host 'Success: Action name filtering works correctly'
");

            var result = RunScript(script);
            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().MatchRegex("Success|SKIP");
        }

        [Fact]
        public void GetDataverseCloudFlowAction_ByFlowName_Should_Work()
        {
            var script = GetConnectionScript(@"
# Get a flow to test with
$flows = Get-DataverseCloudFlow -Connection $connection
if ($flows.Count -eq 0) {
    Write-Host 'SKIP: No flows available for testing'
    exit 0
}

$testFlow = $flows[0]
Write-Host ""Testing with flow: $($testFlow.Name)""

# Get actions by flow name
$actions = Get-DataverseCloudFlowAction -Connection $connection -FlowName $testFlow.Name
Write-Host ""Found $($actions.Count) actions""

# Verify all actions have the correct flow ID
foreach ($action in $actions) {
    if ($action.FlowId -ne $testFlow.Id) {
        throw ""Action FlowId mismatch: expected $($testFlow.Id), got $($action.FlowId)""
    }
    if ($action.FlowName -ne $testFlow.Name) {
        throw ""Action FlowName mismatch: expected $($testFlow.Name), got $($action.FlowName)""
    }
}

Write-Host 'Success: Get-DataverseCloudFlowAction by FlowName works correctly'
");

            var result = RunScript(script);
            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().MatchRegex("Success|SKIP");
        }

        [Fact]
        public void SetDataverseCloudFlow_CreateNew_Should_CreateFlowWhenNotExists()
        {
            var flowName = $"E2E Test Create {System.Guid.NewGuid():N}";
            var script = GetConnectionScript($@"
$flowName = '{flowName}'
Write-Host ""Creating new cloud flow: $flowName""

# Verify flow doesn't exist yet
$existing = Get-DataverseCloudFlow -Connection $connection -Name $flowName
if ($existing.Count -gt 0) {{
    throw ""Flow '$flowName' already exists before creation test""
}}

# Create the flow
$flowId = Set-DataverseCloudFlow -Connection $connection -Name $flowName -Description 'E2E test flow' -PassThru
Write-Host ""Created flow with ID: $flowId""

if (-not $flowId) {{
    throw 'Expected flow ID to be returned'
}}

# Verify the flow now exists
$created = Get-DataverseCloudFlow -Connection $connection -Name $flowName
if ($created.Count -ne 1) {{
    throw ""Expected 1 flow named '$flowName', found $($created.Count)""
}}
if ($created.Id -ne $flowId) {{
    throw ""Flow ID mismatch: expected $flowId, got $($created.Id)""
}}
if ($created.State -ne 'Draft') {{
    throw ""Expected new flow to be in Draft state, got $($created.State)""
}}

# Clean up
Remove-DataverseCloudFlow -Connection $connection -Id $flowId -Confirm:$false
Write-Host 'Success: Flow created, verified, and deleted'
");

            var result = RunScript(script);
            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void SetDataverseCloudFlow_CreateNew_IsIdempotent()
        {
            var flowName = $"E2E Test Idempotent {System.Guid.NewGuid():N}";
            var script = GetConnectionScript($@"
$flowName = '{flowName}'
Write-Host ""Testing idempotent create for: $flowName""

# Create the flow twice
$flowId1 = Set-DataverseCloudFlow -Connection $connection -Name $flowName -PassThru
Write-Host ""First call created/found flow: $flowId1""

$flowId2 = Set-DataverseCloudFlow -Connection $connection -Name $flowName -PassThru
Write-Host ""Second call found flow: $flowId2""

if ($flowId1 -ne $flowId2) {{
    # Clean up before throwing
    try {{ Remove-DataverseCloudFlow -Connection $connection -Id $flowId1 -Confirm:$false }} catch {{}}
    try {{ Remove-DataverseCloudFlow -Connection $connection -Id $flowId2 -Confirm:$false }} catch {{}}
    throw ""Expected same ID on second call, got different IDs: $flowId1 vs $flowId2""
}}

# Clean up
Remove-DataverseCloudFlow -Connection $connection -Id $flowId1 -Confirm:$false
Write-Host 'Success: Idempotent create returns same flow ID'
");

            var result = RunScript(script);
            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void SetDataverseCloudFlow_CreateNew_WhatIf_Should_NotCreateFlow()
        {
            var flowName = $"E2E WhatIf {System.Guid.NewGuid():N}";
            var script = GetConnectionScript($@"
$flowName = '{flowName}'
Write-Host ""Testing WhatIf for create: $flowName""

# Use WhatIf - should not create
Set-DataverseCloudFlow -Connection $connection -Name $flowName -WhatIf

# Verify flow was NOT created
$existing = Get-DataverseCloudFlow -Connection $connection -Name $flowName
if ($existing.Count -gt 0) {{
    # Clean up
    Remove-DataverseCloudFlow -Connection $connection -Id $existing[0].Id -Confirm:$false
    throw 'WhatIf created a flow!'
}}

Write-Host 'Success: WhatIf does not create flow'
");

            var result = RunScript(script);
            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void SetDataverseCloudFlowAction_CreateNew_Should_AddActionToFlow()
        {
            var flowName = $"E2E Action Test {System.Guid.NewGuid():N}";
            var script = GetConnectionScript($@"
$flowName = '{flowName}'
Write-Host ""Creating test flow: $flowName""

# Create a flow to test with
$flowId = Set-DataverseCloudFlow -Connection $connection -Name $flowName -PassThru
Write-Host ""Created flow: $flowId""

try {{
    # Create a new action in the flow
    Write-Host 'Creating new action in flow...'
    Set-DataverseCloudFlowAction -Connection $connection -FlowId $flowId -ActionId 'Initialize_Counter' -Type 'InitializeVariable' -Inputs @{{variables=@(@{{name='counter';type='Integer';value=0}})}} -Description 'Counter variable'

    # Verify the action was created
    $actions = Get-DataverseCloudFlowAction -Connection $connection -FlowId $flowId
    $newAction = $actions | Where-Object {{ $_.ActionId -eq 'Initialize_Counter' }}

    if (-not $newAction) {{
        throw 'Action was not found after creation'
    }}
    if ($newAction.Type -ne 'InitializeVariable') {{
        throw ""Expected action type 'InitializeVariable', got '$($newAction.Type)'""
    }}
    if ($newAction.Description -ne 'Counter variable') {{
        throw ""Expected description 'Counter variable', got '$($newAction.Description)'""
    }}

    Write-Host ""Success: Action '$($newAction.ActionId)' of type '$($newAction.Type)' created successfully""
}} finally {{
    try {{ Remove-DataverseCloudFlow -Connection $connection -Id $flowId -Confirm:$false }} catch {{}}
}}
");

            var result = RunScript(script);
            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void SetDataverseCloudFlowAction_UpdateExisting_Should_UpdateActionInFlow()
        {
            var flowName = $"E2E Action Update {System.Guid.NewGuid():N}";
            var script = GetConnectionScript($@"
$flowName = '{flowName}'
$flowId = Set-DataverseCloudFlow -Connection $connection -Name $flowName -PassThru
Write-Host ""Created flow: $flowId""

try {{
    # Create initial action
    Set-DataverseCloudFlowAction -Connection $connection -FlowId $flowId -ActionId 'My_Action' -Type 'Compose' -Inputs @{{method='GET'; uri='https://example.com'}} -Description 'Initial description'

    # Update the action's description and inputs
    Set-DataverseCloudFlowAction -Connection $connection -FlowId $flowId -ActionId 'My_Action' -Inputs @{{method='POST'; uri='https://example.com/update'}} -Description 'Updated description'

    # Verify update
    $actions = Get-DataverseCloudFlowAction -Connection $connection -FlowId $flowId
    $updatedAction = $actions | Where-Object {{ $_.ActionId -eq 'My_Action' }}

    if (-not $updatedAction) {{
        throw 'Action not found after update'
    }}
    if ($updatedAction.Description -ne 'Updated description') {{
        throw ""Expected description 'Updated description', got '$($updatedAction.Description)'""
    }}

    Write-Host 'Success: Action updated with new description and inputs'
}} finally {{
    try {{ Remove-DataverseCloudFlow -Connection $connection -Id $flowId -Confirm:$false }} catch {{}}
}}
");

            var result = RunScript(script);
            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void SetDataverseCloudFlowAction_CreateNew_RequiresType()
        {
            var flowName = $"E2E Action NoType {System.Guid.NewGuid():N}";
            var script = GetConnectionScript($@"
$flowName = '{flowName}'
$flowId = Set-DataverseCloudFlow -Connection $connection -Name $flowName -PassThru

try {{
    # Try to create an action without specifying -Type (should fail)
    try {{
        Set-DataverseCloudFlowAction -Connection $connection -FlowId $flowId -ActionId 'New_Action' -Inputs 'value'
        throw 'Expected error for missing -Type but no error was thrown'
    }} catch {{
        if ($_.ToString() -like '*-Type parameter is required*' -or $_.ToString() -like '*TypeRequiredForCreate*') {{
            Write-Host 'Success: Got expected error for missing -Type'
        }} else {{
            throw ""Unexpected error: $_""
        }}
    }}
}} finally {{
    try {{ Remove-DataverseCloudFlow -Connection $connection -Id $flowId -Confirm:$false }} catch {{}}
}}
");

            var result = RunScript(script);
            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void SetDataverseCloudFlowAction_CreateNew_WhatIf_Should_NotCreateAction()
        {
            var flowName = $"E2E Action WhatIf {System.Guid.NewGuid():N}";
            var script = GetConnectionScript($@"
$flowName = '{flowName}'
$flowId = Set-DataverseCloudFlow -Connection $connection -Name $flowName -PassThru

try {{
    # Use WhatIf - should not create action
    Set-DataverseCloudFlowAction -Connection $connection -FlowId $flowId -ActionId 'WhatIf_Action' -Type 'Compose' -Inputs 'test' -WhatIf

    # Verify action was NOT created
    $actions = Get-DataverseCloudFlowAction -Connection $connection -FlowId $flowId
    $whatIfAction = $actions | Where-Object {{ $_.ActionId -eq 'WhatIf_Action' }}

    if ($whatIfAction) {{
        throw 'WhatIf created an action!'
    }}

    Write-Host 'Success: WhatIf does not create action'
}} finally {{
    try {{ Remove-DataverseCloudFlow -Connection $connection -Id $flowId -Confirm:$false }} catch {{}}
}}
");

            var result = RunScript(script);
            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void SetDataverseCloudFlowAction_ByFlowName_Should_Work()
        {
            var flowName = $"E2E Action ByName {System.Guid.NewGuid():N}";
            var script = GetConnectionScript($@"
$flowName = '{flowName}'
$flowId = Set-DataverseCloudFlow -Connection $connection -Name $flowName -PassThru

try {{
    # Create action by flow name
    Set-DataverseCloudFlowAction -Connection $connection -FlowName $flowName -ActionId 'Test_Action' -Type 'Compose' -Inputs @{{value='hello'}}

    # Verify action exists
    $actions = Get-DataverseCloudFlowAction -Connection $connection -FlowId $flowId
    $action = $actions | Where-Object {{ $_.ActionId -eq 'Test_Action' }}

    if (-not $action) {{
        throw 'Action not found after creation by flow name'
    }}

    Write-Host 'Success: Action created using FlowName parameter'
}} finally {{
    try {{ Remove-DataverseCloudFlow -Connection $connection -Id $flowId -Confirm:$false }} catch {{}}
}}
");

            var result = RunScript(script);
            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void SetDataverseCloudFlow_WithClientData_Should_UpdateFlowDefinition()
        {
            var flowName = $"E2E ClientData {System.Guid.NewGuid():N}";
            var script = GetConnectionScript($@"
$flowName = '{flowName}'
$flowId = Set-DataverseCloudFlow -Connection $connection -Name $flowName -PassThru
Write-Host ""Created flow: $flowId""

try {{
    # Get the current flow definition
    $flowData = Get-DataverseCloudFlow -Connection $connection -Id $flowId -IncludeClientData

    # Parse and modify the clientdata to add an action
    $def = $flowData.ClientData | ConvertFrom-Json
    $def.properties.definition.actions | Add-Member -NotePropertyName 'Test_Compose' -NotePropertyValue ([PSCustomObject]@{{type='Compose';runAfter=@{{}};inputs='hello'}})
    $newClientData = $def | ConvertTo-Json -Depth 20 -Compress

    # Update the flow with new clientdata
    Set-DataverseCloudFlow -Connection $connection -Id $flowId -ClientData $newClientData

    # Verify the action is now in the flow
    $actions = Get-DataverseCloudFlowAction -Connection $connection -FlowId $flowId
    $action = $actions | Where-Object {{ $_.ActionId -eq 'Test_Compose' }}

    if (-not $action) {{
        throw 'Action not found after clientdata update'
    }}

    Write-Host 'Success: Flow definition updated via -ClientData parameter'
}} finally {{
    try {{ Remove-DataverseCloudFlow -Connection $connection -Id $flowId -Confirm:$false }} catch {{}}
}}
");

            var result = RunScript(script);
            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }

        [Fact]
        public void SetDataverseCloudFlow_WhatIf_Should_NotModifyFlow()
        {
            var script = GetConnectionScript(@"
# Get a flow to test with
$flows = Get-DataverseCloudFlow -Connection $connection
if ($flows.Count -eq 0) {
    Write-Host 'SKIP: No flows available for testing'
    exit 0
}

$testFlow = $flows[0]
$originalDesc = $testFlow.Description
Write-Host ""Testing WhatIf with flow: $($testFlow.Name)""

# Try to update with WhatIf
Set-DataverseCloudFlow -Connection $connection -Id $testFlow.Id -Description 'Test description' -WhatIf

# Verify flow was not modified
$afterWhatIf = Get-DataverseCloudFlow -Connection $connection -Id $testFlow.Id
if ($afterWhatIf.Description -ne $originalDesc) {
    throw ""WhatIf modified the flow! Original: '$originalDesc', After: '$($afterWhatIf.Description)'""
}

Write-Host 'Success: WhatIf does not modify flows'
");

            var result = RunScript(script);
            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().MatchRegex("Success|SKIP");
        }

        [Fact]
        public void CloudFlowCmdlets_HelpIsAvailable()
        {
            var script = $@"
{GetModuleImportStatement()}

$cmdlets = @(
    'Get-DataverseCloudFlow',
    'Set-DataverseCloudFlow',
    'Remove-DataverseCloudFlow',
    'Get-DataverseCloudFlowAction',
    'Set-DataverseCloudFlowAction',
    'Remove-DataverseCloudFlowAction'
)

$issues = @()

foreach ($cmdletName in $cmdlets) {{
    Write-Host ""Testing help for $cmdletName""
    $help = Get-Help $cmdletName -Full
    
    if (-not $help.Name) {{
        $issues += ""$cmdletName`: Missing Name""
    }}
    
    if (-not $help.Syntax) {{
        $issues += ""$cmdletName`: Missing Syntax""
    }}
    
    if (-not $help.Examples) {{
        $issues += ""$cmdletName`: Missing Examples""
    }}
    
    # Verify help has parameters
    if (-not $help.Parameters) {{
        $issues += ""$cmdletName`: Missing Parameters section""
    }}
}}

if ($issues.Count -gt 0) {{
    throw ""Help validation issues found:`n$($issues -join ""`n"")""
}}

Write-Host 'Success: All cloud flow cmdlets have complete help'
";

            var result = RunScript(script);
            result.Success.Should().BeTrue($"Script should succeed.\nStdOut: {result.StandardOutput}\nStdErr: {result.StandardError}");
            result.StandardOutput.Should().Contain("Success");
        }
    }
}
