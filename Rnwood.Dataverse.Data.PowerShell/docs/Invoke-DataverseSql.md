---
external help file: Rnwood.Dataverse.Data.PowerShell.Cmdlets.dll-Help.xml
Module Name: Rnwood.Dataverse.Data.PowerShell
online version:
schema: 2.0.0
---

# Invoke-DataverseSql

## SYNOPSIS
Invokes a Dataverse SQL query using Sql4Cds and writes any resulting rows to the pipeline.

## SYNTAX

```
Invoke-DataverseSql [-Sql] <String> [-UseTdsEndpoint] [-Timeout <Int32>] [-Parameters <PSObject>]
 [-BatchSize <Int32>] [-MaxDegreeOfParallelism <Int32>] [-BypassCustomPluginExecution] [-UseBulkDelete]
 [-ReturnEntityReferenceAsGuid] [-UseLocalTimezone] [-AdditionalConnections <Hashtable>]
 [-DataSourceName <String>] [-Connection <ServiceClient>] [-ProgressAction <ActionPreference>] [-WhatIf]
 [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Sql4Cds is a powerfull engine which can translate many SELECT, INSERT, UPDATE and DELETE Sql queries and execute them against Dataverse. This Cmdlet uses Sql4Cds to execute such queries.

If the query returns a result set, it will output to the pipeline with an object per row having a property per column in the result set.
If applicable (e.g. for UPDATE), the affected row count is written to verbose output.

By default, queries run through SQL4CDS compatibility mode (`-UseTdsEndpoint` is off). Some SQL Server-specific functions (for example `DATEFROMPARTS` or `EOMONTH`) may not be available in that mode. If you need those functions and your connection supports it, use `-UseTdsEndpoint`.

**SQL Parameters**

The cmdlet supports parameterized queries using the `@paramname` syntax. Parameters help avoid SQL injection and improve query readability.

Use the `-Parameters` parameter to provide values as a hashtable or PSObject:

    Invoke-DataverseSql -Sql "SELECT * FROM contact WHERE lastname = @name AND createdon > @date" -Parameters @{ name = 'Smith'; date = '2024-01-01' }

When piping objects to the cmdlet, each object's properties become available as parameters, and the query executes once per input object.

**Cross-Datasource Queries**

The cmdlet supports querying across multiple Dataverse environments using the `datasourcename..tablename` syntax (double-dot notation).

**Syntax:** `datasourcename..tablename` where:
- `datasourcename` = the name assigned to a connection (via `-AdditionalConnections` or `-DataSourceName`)
- `..` = double-dot separator (NOT a single dot `.`)  
- `tablename` = logical name of the table

**Setting datasource names:**
- Primary connection defaults to organization unique name (e.g., `org12345abc`)
- Use `-DataSourceName` to override the primary connection's name
- Use `-AdditionalConnections` to register named secondary connections

**Why use `-DataSourceName`?**
Organization unique names differ across environments (dev/test/prod). By setting explicit datasource names, your SQL queries remain portable:

**Without `-DataSourceName`:** Environment-specific - `Invoke-DataverseSql -Sql "SELECT * FROM org12345abc..account"` (Only works in one environment)

**With `-DataSourceName`:** Portable across all environments - `Invoke-DataverseSql -DataSourceName "primary" -Sql "SELECT * FROM primary..account"` (Works everywhere)

## EXAMPLES

### Example 1: Basic query with SQL parameter
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Invoke-DataverseSql -Sql "SELECT TOP 1 createdon FROM Contact WHERE lastname=@lastname" -Parameters @{
    lastname = "Wood"
}

createdon
---------
28/11/2024 16:28:12
```

Executes a SELECT query using the `@lastname` parameter. The `@paramname` syntax in SQL queries is mapped to properties in the `-Parameters` hashtable.

### Example 2: Simplified positional syntax
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> Invoke-DataverseSql "SELECT TOP 1 createdon FROM Contact WHERE lastname='Wood'"

createdon
---------
28/11/2024 16:28:12
```

The `-Sql` parameter is positional (position 0), allowing you to omit the parameter name for cleaner syntax.

### Example 3: Pipeline parameters for batch execution
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> @(
    @{ lastname = "Wood" },
    @{ lastname = "Smith" }
) | Invoke-DataverseSql -Sql "SELECT TOP 1 lastname, createdon FROM Contact WHERE lastname=@lastname"

lastname createdon
-------- ---------
Wood     28/11/2024 16:28:12
Smith    28/11/2024 16:42:30
```

Pipelines multiple parameter sets to the cmdlet. The query executes once for each input object, with that object's properties becoming available as `@parameter` values.

### Example 4: Cross-datasource query using double-dot syntax
```powershell
PS C:\> $primaryConn = Get-DataverseConnection -Url "https://primary.crm.dynamics.com" -ClientId $id -ClientSecret $secret
PS C:\> $secondaryConn = Get-DataverseConnection -Url "https://secondary.crm.dynamics.com" -ClientId $id -ClientSecret $secret

PS C:\> $additionalConnections = @{
    "secondary" = $secondaryConn
}

PS C:\> # Query using double-dot syntax: datasourcename..tablename
PS C:\> Invoke-DataverseSql -Connection $primaryConn -AdditionalConnections $additionalConnections -Sql "
    SELECT p.fullname AS primary_user, s.email AS secondary_email
    FROM org12345abc..systemuser p
    INNER JOIN secondary..systemuser s ON p.domainname = s.domainname
    WHERE p.isdisabled = 0
"

primary_user       secondary_email
------------       ---------------
John Smith         jsmith@company.com
Jane Doe           jdoe@company.com
```

Executes a cross-datasource query that joins data from two Dataverse environments:
- `org12345abc..systemuser` references the primary connection's datasource (organization unique name)
- `secondary..systemuser` references the datasource named "secondary" from `-AdditionalConnections`

Note the **double-dot** (`..`) separator - this is the correct syntax, not a single dot.

### Example 5: Using DataSourceName for portable queries
```powershell
PS C:\> $devConn = Get-DataverseConnection -Url "https://dev-org123.crm.dynamics.com" -ClientId $id -ClientSecret $secret
PS C:\> $prodConn = Get-DataverseConnection -Url "https://prod-org789.crm.dynamics.com" -ClientId $id -ClientSecret $secret

PS C:\> $additionalConnections = @{
    "production" = $prodConn
}

PS C:\> # Set explicit datasource name for primary connection
PS C:\> Invoke-DataverseSql -Connection $devConn -DataSourceName "primary" -AdditionalConnections $additionalConnections -Sql "
    SELECT p.name, prod.revenue
    FROM primary..account p
    LEFT JOIN production..account prod ON p.accountnumber = prod.accountnumber
    WHERE p.statecode = 0
"

name                revenue
----                -------
Contoso Ltd         1500000.00
Fabrikam Inc        2350000.00
```

Uses `-DataSourceName "primary"` to assign a stable, environment-independent name to the main connection. This makes the SQL query portable:
- Without `-DataSourceName`: Would use "org123" (dev) or "org789" (prod) - query changes per environment
- With `-DataSourceName "primary"`: Same query works in all environments

This is essential for CI/CD pipelines and deployment scripts that run across multiple environments.

### Example 6: Multiple parameters with complex filtering
```powershell
PS C:\> Get-DataverseConnection -Url https://myorg.crm.dynamics.com -Interactive -SetAsDefault
PS C:\> $params = @{
    minRevenue = 100000
    maxRevenue = 500000
    industry = 'Technology'
    city = 'Seattle'
}

PS C:\> Invoke-DataverseSql -Sql "
    SELECT name, revenue, address1_city
    FROM account
    WHERE revenue BETWEEN @minRevenue AND @maxRevenue
      AND industrycode = @industry
      AND address1_city = @city
" -Parameters $params

name            revenue     address1_city
----            -------     -------------
TechCorp        250000.00   Seattle
InnoSoft        350000.00   Seattle
```

Demonstrates using multiple parameters in a single query for complex filtering. All `@paramname` references in the SQL are replaced with values from the `-Parameters` hashtable.

### Example 7: Cross-datasource comparison report
```powershell
PS C:\> $stagingConn = Get-DataverseConnection -Url "https://staging.crm.dynamics.com" -ClientId $id -ClientSecret $secret
PS C:\> $prodConn = Get-DataverseConnection -Url "https://prod.crm.dynamics.com" -ClientId $id -ClientSecret $secret

PS C:\> $additional = @{ "prod" = $prodConn }

PS C:\> # Compare record counts between staging and production
PS C:\> Invoke-DataverseSql -Connection $stagingConn -DataSourceName "staging" -AdditionalConnections $additional -Sql "
    SELECT 
        'Contacts' AS entity_name,
        (SELECT COUNT(*) FROM staging..contact) AS staging_count,
        (SELECT COUNT(*) FROM prod..contact) AS prod_count
    UNION ALL
    SELECT 
        'Accounts',
        (SELECT COUNT(*) FROM staging..account),
        (SELECT COUNT(*) FROM prod..account)
"

entity_name    staging_count    prod_count
-----------    -------------    ----------
Contacts       1523             1498
Accounts       847              842
```

Uses cross-datasource queries for environment comparison reports. The double-dot syntax allows querying multiple environments in a single SQL statement.

## PARAMETERS

### -AdditionalConnections
Hashtable of additional Dataverse connections for cross-datasource queries. Keys are datasource names (strings), values are ServiceClient connections.

When specified, these connections can be referenced in SQL queries using `datasourcename..tablename` syntax (double-dot).

**Example:**


$additionalConnections = @{
    "secondary" = $secondaryConnection
    "archive" = $archiveConnection
}

**SQL usage:**


SELECT a.name, s.email
FROM primary..account a
JOIN secondary..contact s ON a.primarycontactid = s.contactid
LEFT JOIN archive..auditlog al ON a.accountid = al.objectid

```yaml
Type: Hashtable
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -BatchSize
Controls the batch size used by Sql4Cds.

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

### -BypassCustomPluginExecution
Bypasses custom plugins. See Sql4Cds docs.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Connection
DataverseConnection instance obtained from Get-DataverseConnection cmdlet

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

### -DataSourceName
Overrides the default datasource name for the primary connection. If not specified, defaults to the organization unique name (e.g., "org12345abc").

Use this to assign a stable, environment-independent name for portable SQL queries that work across dev/test/prod.

**Why use this:** Organization unique names differ between environments. Setting an explicit datasource name ensures SQL queries remain identical across all environments.

**Example without `-DataSourceName`:**


-- Environment-specific (breaks when moving between environments)
SELECT * FROM org12345abc..account  -- Only works in one environment

**Example with `-DataSourceName "primary"`:**


-- Portable (works in all environments)
SELECT * FROM primary..account  -- Works everywhere

Used in combination with `-AdditionalConnections` and the `datasourcename..tablename` syntax. 

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: Organization unique name
Accept pipeline input: False
Accept wildcard characters: False
```

### -MaxDegreeOfParallelism
Maximum number of threads to use. See Sql4Cds docs.

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

### -Parameters
Provides values for `@parameters` referenced in the SQL query. Accepts a Hashtable or any PSObject with properties.

**Parameter syntax in SQL:** Use `@paramname` to reference parameters:


SELECT * FROM contact WHERE lastname = @name AND createdon > @date

**Providing values:**


-Parameters @{ name = 'Smith'; date = '2024-01-01' }

**Pipeline usage:** When reading from the pipeline, the query executes once per input object with that object's properties as parameter values:


@(
    @{ name = 'Smith'; dept = 'Sales' },
    @{ name = 'Jones'; dept = 'Marketing' }
) | Invoke-DataverseSql -Sql "UPDATE contact SET department = @dept WHERE lastname = @name"

```yaml
Type: PSObject
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
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

### -ReturnEntityReferenceAsGuid
Returns lookup column values as simple Guid as opposed to SqlEntityReference type. See Sql4Cds docs.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Sql
SQL to execute. See Sql4Cds docs for supported queries.

**Supported features:**
- Standard SQL: SELECT, INSERT, UPDATE, DELETE
- `@paramname` syntax for parameters (values from `-Parameters`)
- `datasourcename..tablename` syntax for cross-datasource queries (requires `-AdditionalConnections`)
- SQL4CDS compatibility mode by default; SQL Server-specific functions may require `-UseTdsEndpoint`

The -Sql parameter is positional (position 0), allowing you to omit the parameter name:


Invoke-DataverseSql "SELECT * FROM contact WHERE lastname = @name" -Parameters @{ name = 'Smith' }

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

### -Timeout
Timeout for query to execute. See Sql4Cds docs.

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

### -UseBulkDelete
Uses bulk delete for supported DELETE operations. See Sql4Cds docs.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UseLocalTimezone
When working with date values, this property indicates the local time zone should be used. See Sql4Cds docs.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -UseTdsEndpoint
Lets Sql4Cds use the Dataverse TDS endpoint for compatible queries. By default this is off, so queries run in SQL4CDS compatibility mode for more predictable support across connections.

If a query fails because a SQL Server-specific function is not recognized (for example `DATEFROMPARTS` or `EOMONTH`), retry with `-UseTdsEndpoint`.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
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
Shows what would happen if the cmdlet runs. The cmdlet is not run. Does not apply to read only queries.

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

### System.String
### System.Management.Automation.PSObject
## OUTPUTS

### System.Object
## NOTES
A special thanks to Mark Carrington for his amazing open-source project that has enabled this.

## RELATED LINKS

[https://github.com/MarkMpn/Sql4Cds](https://github.com/MarkMpn/Sql4Cds)
