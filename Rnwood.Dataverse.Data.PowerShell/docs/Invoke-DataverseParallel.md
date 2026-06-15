---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseParallel

## SYNOPSIS
Processes input objects in parallel using chunked batches with cloned Dataverse connections.

## SYNTAX

```
Invoke-DataverseParallel [-ScriptBlock] <ScriptBlock> -InputObject <PSObject> [-ChunkSize <Int32>]
 [-MaxDegreeOfParallelism <Int32>] [-ExcludeModule <String[]>] [-Connection <ServiceClient>]
 [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
The Invoke-DataverseParallel cmdlet processes input objects in parallel using multiple runspaces.
It automatically chunks the input data, clones the Dataverse connection for each parallel worker, and makes the cloned connection available as the default connection within each script block.

The chunk for each invocation is available as $_ within the block. This contains multiple records, so pipe it to `Set-DataverseRecord` for example.

This cmdlet is useful for improving performance when processing large numbers of records where the operations are independent and can be executed concurrently.

Take care to make the operations within the script block as efficient as possible. With `Set-DataverseRecord` by default it does per-record existence/update checks to avoid sending unneeded record/colum updates, so use one of the switches that force it to assume what it should do. For instance in the example below, the -UpdateAllColumns switch makes it assume the record exists, and sends all columns in the update.

## EXAMPLES

### Example 1: Update records in parallel
```powershell
PS C:\> $connection = Get-DataverseConnection -url 'https://myorg.crm.dynamics.com' -ClientId $env:CLIENT_ID -ClientSecret $env:CLIENT_SECRET -SetAsDefault
PS C:\> Get-DataverseRecord -TableName contact -Top 1000 -Columns emailaddress1 |
  Invoke-DataverseParallel -Connection $connection -ChunkSize 50 -MaxDegreeOfParallelism 8 -ScriptBlock {
    $_ |
       ForEach-Object{ $_.emailaddress1 = "updated-$($_.contactid)@example.com"; $_ } |
       Set-DataverseRecord -TableName contact -UpdateAllColumns
  }
```

Updates 1000 contact records in parallel with 8 concurrent workers, processing 50 records per chunk.

## PARAMETERS

### -ChunkSize
Number of input objects to process in each parallel batch.
Default is 50.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet.
If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.

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

### -ExcludeModule
Array of module name patterns (supports wildcards) to exclude from parallel runspaces. Pester is always excluded. Example: @('PSReadLine', 'Test*')

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputObject
Input objects to process in parallel.

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -MaxDegreeOfParallelism
Maximum number of parallel operations.
Default is the number of processors.

```yaml
Type: Int32
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

### -ScriptBlock
Script block to execute for each chunk.
The chunk is available as $_ and a cloned connection is set as the default connection.

```yaml
Type: ScriptBlock
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Management.Automation.PSObject
## OUTPUTS

### System.Management.Automation.PSObject
## NOTES

## RELATED LINKS
