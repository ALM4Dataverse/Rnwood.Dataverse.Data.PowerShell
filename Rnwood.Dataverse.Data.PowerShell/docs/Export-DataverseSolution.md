---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Export-DataverseSolution

## SYNOPSIS
Exports a solution from Dataverse using an asynchronous job with progress reporting.

## SYNTAX

### ToFile (Default)
```
Export-DataverseSolution [-SolutionName] <String> [-Managed] [-TargetVersion <String>]
 [-ExportAutoNumberingSettings] [-ExportCalendarSettings] [-ExportCustomizationSettings]
 [-ExportEmailTrackingSettings] [-ExportGeneralSettings] [-ExportMarketingSettings]
 [-ExportOutlookSynchronizationSettings] [-ExportRelationshipRoles] [-ExportIsvConfig] [-ExportSales]
 [-ExportExternalApplications] [-OutFile <String>] [-PassThru] [-PollingIntervalSeconds <Int32>]
 [-TimeoutSeconds <Int32>] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf]
 [-Confirm] [<CommonParameters>]
```

### ToFolder
```
Export-DataverseSolution [-SolutionName] <String> [-TargetVersion <String>] [-ExportAutoNumberingSettings]
 [-ExportCalendarSettings] [-ExportCustomizationSettings] [-ExportEmailTrackingSettings]
 [-ExportGeneralSettings] [-ExportMarketingSettings] [-ExportOutlookSynchronizationSettings]
 [-ExportRelationshipRoles] [-ExportIsvConfig] [-ExportSales] [-ExportExternalApplications] -OutFolder <String>
 [-UnpackMsApp] [-PackageType <SolutionPackageType>] [-PollingIntervalSeconds <Int32>]
 [-TimeoutSeconds <Int32>] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf]
 [-Confirm] [<CommonParameters>]
```

## DESCRIPTION

This cmdlet exports a Dataverse solution using an asynchronous job and monitors the job progress, providing real-time progress updates.

The cmdlet:
1. Initiates an asynchronous solution export using ExportSolutionAsyncRequest
2. Monitors the async operation status by polling the asyncoperation table
3. Reports progress using PowerShell's progress bar
4. Downloads the solution file when the export completes
5. Optionally saves the solution to a file or outputs the bytes to the pipeline

This is particularly useful for exporting large solutions where the synchronous export would time out.

## EXAMPLES

### Example 1: Export an unmanaged solution to a file
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Export-DataverseSolution -SolutionName "MySolution" -OutFile "C:\Exports\MySolution.zip"
```

Exports the unmanaged version of "MySolution" and saves it to the specified file.

### Example 2: Export a managed solution with progress monitoring
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Export-DataverseSolution -SolutionName "MySolution" -Managed -OutFile "C:\Exports\MySolution_managed.zip" -Verbose
```

Exports the managed version of "MySolution" with verbose output showing the export progress.

### Example 3: Export solution with settings included
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Export-DataverseSolution -SolutionName "MySolution" -ExportAutoNumberingSettings -ExportCalendarSettings -OutFile "C:\Exports\MySolution.zip"
```

Exports "MySolution" including auto-numbering and calendar settings.

### Example 4: Export solution and return bytes to pipeline
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $solutionBytes = Export-DataverseSolution -SolutionName "MySolution" -PassThru
PS C:\> [System.IO.File]::WriteAllBytes("C:\Exports\MySolution.zip", $solutionBytes)
```

Exports "MySolution" and captures the raw bytes in a variable for further processing.

### Example 5: Export solution with custom timeout
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Export-DataverseSolution -SolutionName "LargeSolution" -OutFile "C:\Exports\LargeSolution.zip" -TimeoutSeconds 1200 -PollingIntervalSeconds 10
```

Exports a large solution with a 20-minute timeout and checks status every 10 seconds.

## PARAMETERS

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet. If not provided, uses the default connection set via Get-DataverseConnection -SetAsDefault.

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

### -ExportAutoNumberingSettings
Include auto numbering settings in the exported solution.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExportCalendarSettings
Include calendar settings in the exported solution.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExportCustomizationSettings
Include customization settings in the exported solution.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExportEmailTrackingSettings
Include email tracking settings in the exported solution.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExportExternalApplications
For internal use only.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExportGeneralSettings
Include general settings in the exported solution.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExportIsvConfig
Include ISV.Config settings in the exported solution.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExportMarketingSettings
Include marketing settings in the exported solution.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExportOutlookSynchronizationSettings
Include Outlook synchronization settings in the exported solution.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExportRelationshipRoles
Include relationship role settings in the exported solution.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExportSales
Include sales settings in the exported solution.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -Managed
Export as a managed solution. Default is unmanaged (false).

```yaml
Type: SwitchParameter
Parameter Sets: ToFile
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutFile
Path where the exported solution file should be saved.

```yaml
Type: String
Parameter Sets: ToFile
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutFolder
Path where the exported solution will be unpacked.

```yaml
Type: String
Parameter Sets: ToFolder
Aliases:

Required: True
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PackageType
Package type: 'Unmanaged', 'Managed', or 'Both' (default) for dual Managed and Unmanaged operation. Controls which solution types are exported and unpacked.

```yaml
Type: SolutionPackageType
Parameter Sets: ToFolder
Aliases:
Accepted values: Unmanaged, Managed, Both

Required: False
Position: Named
Default value: Both
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
Output the solution file bytes to the pipeline.

```yaml
Type: SwitchParameter
Parameter Sets: ToFile
Aliases:

Required: False
Position: Named
Default value: False
Accept pipeline input: False
Accept wildcard characters: False
```

### -PollingIntervalSeconds
Polling interval in seconds for checking job status. Default is 5.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 5
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

### -SolutionName
The unique name of the solution to be exported. Required.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TargetVersion
The version that the exported solution will support.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TimeoutSeconds
Timeout in seconds for the export operation. Default is 600 (10 minutes).

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 600
Accept pipeline input: False
Accept wildcard characters: False
```

### -UnpackMsApp
Unpack .msapp files found in the solution into folders.

```yaml
Type: SwitchParameter
Parameter Sets: ToFolder
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Confirm
Prompts you for confirmation before running the cmdlet.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: cf

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WhatIf
Shows what would happen if the cmdlet runs. The cmdlet is not run.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: wi

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

### System.Byte[]
## NOTES

This cmdlet uses the ExportSolutionAsyncRequest API which exports the solution in the background. The cmdlet monitors the async operation and downloads the solution file when complete.

For synchronous exports (useful for small solutions), use Invoke-DataverseExportSolution.

Progress is reported using PowerShell's progress API and shows:
- Current status (Waiting, In progress, etc.)
- Percentage complete
- Status messages from the async operation

The async operation can be canceled by pressing Ctrl+C.

See also:
- https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.exportsolutionasyncrequest
- https://learn.microsoft.com/en-us/dotnet/api/microsoft.crm.sdk.messages.downloadsolutionexportdatarequest

## RELATED LINKS

[Get-DataverseConnection]()
[Invoke-DataverseExportSolution]()
[Invoke-DataverseExportSolutionAsync]()
[Invoke-DataverseImportSolution]()
