---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Clear-DataverseMetadataCache

## SYNOPSIS
Clears the global metadata cache used by Get cmdlets.

## SYNTAX

```
Clear-DataverseMetadataCache [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
```

## DESCRIPTION
The `Clear-DataverseMetadataCache` cmdlet clears the global metadata cache that stores entity, attribute, and relationship metadata when using the `-UseMetadataCache` parameter with Get cmdlets.

The metadata cache improves performance by storing retrieved metadata in memory for reuse. However, the cache needs to be cleared when:
- Metadata changes are made outside the PowerShell module
- You need to ensure you're seeing the latest schema changes
- Memory cleanup is required
- Testing scenarios where fresh metadata is needed

**Important Notes:**
- The cache is automatically invalidated when using Set or Remove metadata cmdlets
- The cache is shared across all cmdlets in your PowerShell session
- The cache is per-connection (each connection has its own cache)
- Clearing the cache does not affect Dataverse - it only clears the local PowerShell cache

You typically don't need to call this cmdlet manually unless you're making schema changes through other tools (Solution Explorer, Power Apps maker portal, etc.) and need to see those changes in PowerShell.

## EXAMPLES

### Example 1: Clear all cached metadata
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Clear-DataverseMetadataCache
```

Clears the metadata cache for all connections.

### Example 2: Clear cache for a specific connection
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $conn = Get-DataverseConnection -Url "https://myorg.crm.dynamics.com" -Interactive
PS C:\> Clear-DataverseMetadataCache
```

Clears the metadata cache only for the specified connection.

### Example 3: Clear cache after external changes
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # You made changes in Power Apps maker portal
PS C:\> # Now clear the cache to see the changes
PS C:\> Clear-DataverseMetadataCache

PS C:\> # Fetch fresh metadata
PS C:\> $metadata = Get-DataverseEntityMetadata -EntityName account -UseMetadataCache
```

Clears the cache after making changes through another tool.

### Example 4: Verify cache behavior
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # First call - populates cache
PS C:\> Measure-Command { 
    Get-DataverseEntityMetadata -EntityName contact -UseMetadataCache 
} | Select-Object Milliseconds

Milliseconds
------------
450

PS C:\> # Second call - uses cache (fast)
PS C:\> Measure-Command { 
    Get-DataverseEntityMetadata -EntityName contact -UseMetadataCache 
} | Select-Object Milliseconds

Milliseconds
------------
2

PS C:\> # Clear cache
PS C:\> Clear-DataverseMetadataCache

PS C:\> # Third call - cache cleared, fetches again
PS C:\> Measure-Command { 
    Get-DataverseEntityMetadata -EntityName contact -UseMetadataCache 
} | Select-Object Milliseconds

Milliseconds
------------
420
```

Demonstrates the cache behavior and impact of clearing it.

### Example 5: Clear cache in a script
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Clear cache at start of script to ensure fresh data
PS C:\> Clear-DataverseMetadataCache

PS C:\> # Now proceed with metadata operations
PS C:\> $entities = Get-DataverseEntityMetadata -UseMetadataCache
```

Ensures a script starts with a clean cache for reliable results.

### Example 6: Clear cache after bulk metadata changes
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Make multiple metadata changes
PS C:\> Set-DataverseAttributeMetadata -EntityName account -AttributeName new_field1 ...
PS C:\> Set-DataverseAttributeMetadata -EntityName account -AttributeName new_field2 ...
PS C:\> Set-DataverseAttributeMetadata -EntityName contact -AttributeName new_field3 ...

PS C:\> # Cache is automatically invalidated per entity
# But if you want to ensure complete cleanup:
PS C:\> Clear-DataverseMetadataCache
```

Clears cache after making multiple metadata changes.

### Example 7: Use in testing scenarios
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Describe "Metadata Tests" {
    BeforeEach {
        # Clear cache before each test
        Clear-DataverseMetadataCache
    }
    
    It "Should retrieve entity metadata" {
        $metadata = Get-DataverseEntityMetadata -EntityName account
        $metadata | Should -Not -BeNullOrEmpty
    }
}
```

Uses cache clearing in Pester tests for isolation.

### Example 8: Memory management in long-running scripts
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # Process many entities
PS C:\> foreach ($entity in $entityList) {
    # Get metadata with cache
    $metadata = Get-DataverseEntityMetadata -EntityName $entity -UseMetadataCache
    
    # Process metadata...
    
    # Clear cache every 100 entities to manage memory
    if ($processedCount % 100 -eq 0) {
        Clear-DataverseMetadataCache
    }
}
```

Manages memory in long-running scripts processing many entities.

### Example 9: Clear cache when debugging
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> # During debugging, you might want fresh data
PS C:\> Clear-DataverseMetadataCache -Verbose
VERBOSE: Clearing metadata cache for all connections

PS C:\> $metadata = Get-DataverseEntityMetadata -EntityName account -UseMetadataCache -Verbose
VERBOSE: Retrieving entity metadata from server (cache miss)
```

Uses verbose output to see cache operations during debugging.

### Example 10: Clear cache for multiple connections
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $dev = Get-DataverseConnection -Url "https://dev.crm.dynamics.com" -Interactive
PS C:\> $test = Get-DataverseConnection -Url "https://test.crm.dynamics.com" -Interactive

PS C:\> # Clear cache for dev
PS C:\> Clear-DataverseMetadataCache

PS C:\> # Clear cache for test
PS C:\> Clear-DataverseMetadataCache

PS C:\> # Or clear all at once
PS C:\> Clear-DataverseMetadataCache
```

Clears cache for multiple connections.

## PARAMETERS

### -Connection
Connection to clear cache for.
If not specified, clears all cached metadata.

```yaml
Type: ServiceClient
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ProgressAction
{{ Fill ProgressAction Description }}

```yaml
Type: ActionPreference
Parameter Sets: (All)
Aliases: proga

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None
## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
