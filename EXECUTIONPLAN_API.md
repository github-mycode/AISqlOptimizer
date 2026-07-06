# Execution Plan API

## Overview
The Execution Plan API retrieves SQL Server execution plans for stored procedures without actually executing them. It uses `SET SHOWPLAN_XML ON` to capture the query execution plan in XML format, providing valuable insights for performance analysis and optimization.

## Features

- ✅ **Execution Plan XML** - Full XML execution plan using SHOWPLAN_XML
- ✅ **No Execution** - Gets plan without running the procedure
- ✅ **Cost Analysis** - Extracts estimated subtree cost
- ✅ **Row Estimates** - Extracts estimated number of rows
- ✅ **Parameter Support** - Pass parameters to the stored procedure
- ✅ **SQL Exception Handling** - Comprehensive error handling
- ✅ **Dapper Integration** - High-performance queries
- ✅ **Formatted XML** - Pretty-printed, readable XML output
- ✅ **Async Operations** - Non-blocking async/await

## API Endpoint

### POST /api/executionplan

Retrieves the execution plan for a stored procedure.

**Request Body:**
```json
{
  "server": "localhost",
  "database": "MyDatabase",
  "username": "sa",
  "password": "YourPassword123",
  "trustServerCertificate": true,
  "storedProcedureName": "dbo.sp_GetUserOrders",
  "parameters": "{\"@UserId\": 123, \"@StartDate\": \"2026-01-01\"}"
}
```

**Request Properties:**

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| server | string | Yes | SQL Server instance name or IP |
| database | string | Yes | Database name |
| username | string | No | SQL authentication username (optional for Windows auth) |
| password | string | No | SQL authentication password |
| trustServerCertificate | bool | No | Trust server certificate (default: true) |
| storedProcedureName | string | Yes | Stored procedure name (can include schema) |
| parameters | string | No | JSON object with procedure parameters |

**Response:**
```json
{
  "success": true,
  "storedProcedureName": "dbo.sp_GetUserOrders",
  "databaseName": "MyDatabase",
  "executionPlanXml": "<?xml version=\"1.0\"?>\n<ShowPlanXML xmlns=\"http://schemas.microsoft.com/sqlserver/2004/07/showplan\">\n  <BatchSequence>\n    <Batch>\n      <Statements>\n        <StmtSimple StatementText=\"...\" StatementSubTreeCost=\"0.0065\">\n          ...\n        </StmtSimple>\n      </Statements>\n    </Batch>\n  </BatchSequence>\n</ShowPlanXML>",
  "estimatedCost": 0.0065,
  "estimatedRows": 150,
  "errorMessage": null,
  "timestamp": "2026-07-04T10:30:00Z"
}
```

**Response Properties:**

| Property | Type | Description |
|----------|------|-------------|
| success | bool | Whether the operation succeeded |
| storedProcedureName | string | Stored procedure name |
| databaseName | string | Database name |
| executionPlanXml | string | Full execution plan in XML format |
| estimatedCost | decimal? | Estimated subtree cost from the plan |
| estimatedRows | long? | Estimated number of rows |
| errorMessage | string? | Error message if operation failed |
| timestamp | DateTime | UTC timestamp of when plan was retrieved |

## Usage Examples

### Example 1: Basic Execution Plan Retrieval

```bash
curl -X POST "https://localhost:7xxx/api/executionplan" \
  -H "Content-Type: application/json" \
  -d '{
    "server": "localhost",
    "database": "MyDatabase",
    "username": "sa",
    "password": "YourPassword123",
    "trustServerCertificate": true,
    "storedProcedureName": "dbo.sp_GetUserOrders"
  }'
```

### Example 2: With Parameters

```bash
curl -X POST "https://localhost:7xxx/api/executionplan" \
  -H "Content-Type: application/json" \
  -d '{
    "server": "localhost",
    "database": "MyDatabase",
    "trustServerCertificate": true,
    "storedProcedureName": "dbo.sp_GetUserOrders",
    "parameters": "{\"@UserId\": 123, \"@StartDate\": \"2026-01-01\"}"
  }'
```

### Example 3: Using C# Client

```csharp
using System.Net.Http.Json;

var client = new HttpClient { BaseAddress = new Uri("https://localhost:7xxx") };

var request = new ExecutionPlanRequestDto
{
    Server = "localhost",
    Database = "MyDatabase",
    Username = "sa",
    Password = "YourPassword123",
    TrustServerCertificate = true,
    StoredProcedureName = "dbo.sp_GetUserOrders",
    Parameters = "{\"@UserId\": 123, \"@StartDate\": \"2026-01-01\"}"
};

var response = await client.PostAsJsonAsync("/api/executionplan", request);
var result = await response.Content.ReadFromJsonAsync<ExecutionPlanResponseDto>();

if (result.Success)
{
    Console.WriteLine($"Estimated Cost: {result.EstimatedCost}");
    Console.WriteLine($"Estimated Rows: {result.EstimatedRows}");
    Console.WriteLine($"\nExecution Plan XML:\n{result.ExecutionPlanXml}");
    
    // Save XML to file for analysis in SQL Server Management Studio
    File.WriteAllText("execution-plan.sqlplan", result.ExecutionPlanXml);
}
else
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
}
```

### Example 4: Analyze Multiple Procedures

```csharp
public async Task<Dictionary<string, decimal?>> CompareProcedureCosts(
    List<string> procedureNames)
{
    var costs = new Dictionary<string, decimal?>();
    
    foreach (var procName in procedureNames)
    {
        var request = new ExecutionPlanRequestDto
        {
            Server = "localhost",
            Database = "MyDatabase",
            StoredProcedureName = procName,
            TrustServerCertificate = true
        };
        
        var result = await GetExecutionPlanAsync(request);
        
        if (result.Success)
        {
            costs[procName] = result.EstimatedCost;
        }
    }
    
    // Sort by cost (highest first)
    return costs.OrderByDescending(kvp => kvp.Value)
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
}
```

### Example 5: Save Plans for Documentation

```csharp
public async Task SaveExecutionPlansAsync(string outputDirectory)
{
    var procedures = new[] 
    { 
        "dbo.sp_GetUserOrders", 
        "dbo.sp_ProcessPayment",
        "dbo.sp_GenerateReport"
    };
    
    Directory.CreateDirectory(outputDirectory);
    
    foreach (var procName in procedures)
    {
        var request = new ExecutionPlanRequestDto
        {
            Server = "localhost",
            Database = "MyDatabase",
            StoredProcedureName = procName,
            TrustServerCertificate = true
        };
        
        var result = await GetExecutionPlanAsync(request);
        
        if (result.Success && !string.IsNullOrEmpty(result.ExecutionPlanXml))
        {
            var fileName = procName.Replace(".", "_") + ".sqlplan";
            var filePath = Path.Combine(outputDirectory, fileName);
            
            File.WriteAllText(filePath, result.ExecutionPlanXml);
            
            Console.WriteLine($"Saved: {fileName} (Cost: {result.EstimatedCost})");
        }
    }
}
```

## How It Works

### SHOWPLAN_XML Process

1. **Enable SHOWPLAN_XML**
   ```sql
   SET SHOWPLAN_XML ON
   ```

2. **Execute Procedure** (doesn't actually run)
   ```sql
   EXEC dbo.sp_GetUserOrders @UserId = 123, @StartDate = '2026-01-01'
   ```

3. **Retrieve Plan XML**
   - SQL Server returns the execution plan as XML instead of executing

4. **Disable SHOWPLAN_XML**
   ```sql
   SET SHOWPLAN_XML OFF
   ```

### XML Parsing

The service automatically extracts key metrics:

**Estimated Cost:**
```xml
<StmtSimple StatementSubTreeCost="0.0065">
```

**Estimated Rows:**
```xml
<RelOp EstimateRows="150">
```

## Execution Plan Analysis

### Understanding Estimated Cost

The estimated cost represents SQL Server's internal metric for query complexity:
- **< 0.01** - Very fast, simple query
- **0.01 - 0.1** - Fast query
- **0.1 - 1.0** - Moderate complexity
- **1.0 - 10** - Complex query
- **> 10** - Very complex, potential performance issues

### Reading the XML

The execution plan XML contains:
- **Operators** - Index Scans, Table Scans, Joins, Sorts, etc.
- **Cost Distribution** - Where query spends most resources
- **Missing Indexes** - Recommendations for new indexes
- **Warnings** - Implicit conversions, missing statistics, etc.

### Opening in SSMS

Save the XML with `.sqlplan` extension and open in SQL Server Management Studio for visual analysis:

```csharp
File.WriteAllText("plan.sqlplan", result.ExecutionPlanXml);
// Open plan.sqlplan in SSMS
```

## Parameter Format

### JSON Parameter Format

Parameters must be provided as a JSON object:

```json
{
  "@UserId": 123,
  "@StartDate": "2026-01-01",
  "@IsActive": true,
  "@SearchTerm": "order"
}
```

### Supported Data Types

- **Integers**: `"@Id": 123`
- **Strings**: `"@Name": "John"`
- **Dates**: `"@Date": "2026-01-01"`
- **Booleans**: `"@IsActive": true` (converts to 1/0)
- **Nulls**: `"@Value": null` (converts to NULL)

### Example with Multiple Parameters

```json
{
  "server": "localhost",
  "database": "MyDatabase",
  "storedProcedureName": "dbo.sp_SearchOrders",
  "parameters": "{\"@CustomerId\": 456, \"@StartDate\": \"2026-01-01\", \"@EndDate\": \"2026-12-31\", \"@Status\": \"Active\", \"@MinAmount\": 100.50}"
}
```

## Use Cases

### 1. Performance Optimization

Identify expensive operations:

```csharp
var result = await GetExecutionPlanAsync(request);

if (result.EstimatedCost > 1.0)
{
    Console.WriteLine($"⚠️ High cost procedure: {result.EstimatedCost}");
    
    // Analyze XML for expensive operations
    var doc = XDocument.Parse(result.ExecutionPlanXml);
    var scans = doc.Descendants()
        .Where(e => e.Name.LocalName == "RelOp" && 
                    e.Attribute("PhysicalOp")?.Value.Contains("Scan") == true)
        .ToList();
        
    Console.WriteLine($"Found {scans.Count} table/index scans");
}
```

### 2. Index Recommendations

Extract missing index suggestions:

```csharp
var doc = XDocument.Parse(result.ExecutionPlanXml);
var ns = doc.Root.GetDefaultNamespace();

var missingIndexes = doc.Descendants(ns + "MissingIndex")
    .Select(mi => new
    {
        Database = mi.Attribute("Database")?.Value,
        Schema = mi.Attribute("Schema")?.Value,
        Table = mi.Attribute("Table")?.Value,
        ImpactPercent = mi.Attribute("Impact")?.Value
    })
    .ToList();

foreach (var idx in missingIndexes)
{
    Console.WriteLine($"Missing index on {idx.Schema}.{idx.Table} " +
                     $"(Impact: {idx.ImpactPercent}%)");
}
```

### 3. Before/After Comparison

Compare plans before and after optimization:

```csharp
// Get plan before optimization
var beforePlan = await GetExecutionPlanAsync(request);
var beforeCost = beforePlan.EstimatedCost;

// Apply optimization (add index, rewrite query, etc.)
await ApplyOptimizationAsync();

// Get plan after optimization
var afterPlan = await GetExecutionPlanAsync(request);
var afterCost = afterPlan.EstimatedCost;

var improvement = ((beforeCost - afterCost) / beforeCost * 100);
Console.WriteLine($"Performance improvement: {improvement:F2}%");
```

### 4. Automated Testing

Include in CI/CD pipeline:

```csharp
[Test]
public async Task CriticalProcedures_ShouldHaveAcceptableCost()
{
    var criticalProcs = new[] 
    { 
        "dbo.sp_GetUserOrders",
        "dbo.sp_ProcessPayment"
    };
    
    foreach (var proc in criticalProcs)
    {
        var result = await GetExecutionPlanAsync(new ExecutionPlanRequestDto
        {
            Server = TestConfig.Server,
            Database = TestConfig.Database,
            StoredProcedureName = proc,
            TrustServerCertificate = true
        });
        
        Assert.IsTrue(result.Success, $"{proc} failed to generate plan");
        Assert.IsTrue(result.EstimatedCost < 1.0, 
            $"{proc} cost ({result.EstimatedCost}) exceeds threshold");
    }
}
```

### 5. Documentation Generation

Generate performance documentation:

```csharp
var markdown = new StringBuilder();
markdown.AppendLine("# Stored Procedure Performance Analysis");
markdown.AppendLine();

foreach (var proc in procedures)
{
    var result = await GetExecutionPlanAsync(new ExecutionPlanRequestDto
    {
        StoredProcedureName = proc,
        Server = "localhost",
        Database = "MyDatabase"
    });
    
    markdown.AppendLine($"## {proc}");
    markdown.AppendLine($"- **Estimated Cost:** {result.EstimatedCost}");
    markdown.AppendLine($"- **Estimated Rows:** {result.EstimatedRows}");
    markdown.AppendLine($"- **Retrieved:** {result.Timestamp}");
    markdown.AppendLine();
}

File.WriteAllText("performance-report.md", markdown.ToString());
```

## Implementation Details

### Files Created

1. **ExecutionPlanRequestDto.cs** - Request with connection details and procedure name
2. **ExecutionPlanResponseDto.cs** - Response with XML, cost, and row estimates
3. **IExecutionPlanService.cs** - Service interface
4. **ExecutionPlanService.cs** - Service implementation with SHOWPLAN_XML
5. **ExecutionPlanRequestDtoValidator.cs** - Request validation
6. **ExecutionPlanController.cs** - API controller

### Service Architecture

**ExecutionPlanService:**
- Uses ConnectionService for connection string building
- Enables/disables SHOWPLAN_XML automatically
- Handles parameter formatting (JSON to SQL)
- Parses XML to extract metadata
- Formats XML for readability
- Comprehensive error handling and logging

**Key Methods:**
- `GetExecutionPlanAsync` - Main method
- `GetExecutionPlanXmlAsync` - Retrieves plan using SHOWPLAN_XML
- `ParseProcedureName` - Handles schema.procedure format
- `BuildExecuteCommand` - Builds EXEC statement with parameters
- `FormatParameterValue` - Converts parameter values to SQL format
- `FormatXml` - Pretty-prints XML
- `ExtractPlanMetadata` - Extracts cost and row estimates

### SQL Queries Used

**Enable SHOWPLAN_XML:**
```sql
SET SHOWPLAN_XML ON
```

**Execute Procedure (Gets Plan):**
```sql
EXEC dbo.sp_GetUserOrders @UserId = 123, @StartDate = '2026-01-01'
```

**Disable SHOWPLAN_XML:**
```sql
SET SHOWPLAN_XML OFF
```

## Security Considerations

### Connection Security
- Always use HTTPS in production
- Consider authentication/authorization
- Implement rate limiting
- Don't log passwords

### Permissions Required

```sql
-- User needs SHOWPLAN permission
GRANT SHOWPLAN TO [username];

-- Or database-wide
USE [database];
GRANT SHOWPLAN TO [username];
```

### SQL Injection Prevention

The validator prevents SQL injection:
- Server name validation
- Database name validation
- Procedure name validation (no spaces, semicolons, quotes)
- Parameter JSON validation

## Performance Considerations

### Caching

Consider caching execution plans:

```csharp
public class CachedExecutionPlanService : IExecutionPlanService
{
    private readonly IExecutionPlanService _innerService;
    private readonly IMemoryCache _cache;
    
    public async Task<ExecutionPlanResponseDto> GetExecutionPlanAsync(
        ExecutionPlanRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"plan_{request.Server}_{request.Database}_{request.StoredProcedureName}_{request.Parameters}";
        
        if (!_cache.TryGetValue(cacheKey, out ExecutionPlanResponseDto result))
        {
            result = await _innerService.GetExecutionPlanAsync(request, cancellationToken);
            
            if (result.Success)
            {
                _cache.Set(cacheKey, result, TimeSpan.FromHours(1));
            }
        }
        
        return result;
    }
}
```

### Timeouts

The service uses 30-second command timeout. Adjust if needed for complex procedures:

```csharp
await connection.ExecuteAsync("SET SHOWPLAN_XML ON", commandTimeout: 60);
```

## Troubleshooting

### Issue: "No SHOWPLAN permission"
**Solution**: Grant SHOWPLAN permission to the user:
```sql
GRANT SHOWPLAN TO [username];
```

### Issue: "Invalid parameter format"
**Solution**: Ensure parameters is valid JSON:
```json
{"@UserId": 123}  // ✅ Correct
{'@UserId': 123}  // ❌ Wrong (single quotes)
{@UserId: 123}    // ❌ Wrong (no quotes)
```

### Issue: "Procedure not found"
**Solution**: Include schema in procedure name: `dbo.sp_GetUserOrders`

### Issue: "XML is null"
**Solution**: Check that SHOWPLAN_XML is supported (SQL Server 2000+) and user has permissions.

## Testing in Swagger

1. Navigate to `https://localhost:7xxx`
2. Find **ExecutionPlan** section
3. Click **POST /api/executionplan**
4. Click "Try it out"
5. Enter request body:
   ```json
   {
     "server": "localhost",
     "database": "MyDatabase",
     "storedProcedureName": "dbo.sp_GetUserOrders",
     "trustServerCertificate": true,
     "parameters": "{\"@UserId\": 123}"
   }
   ```
6. Click "Execute"
7. Review the execution plan XML and estimated cost/rows

## Summary

The Execution Plan API provides:

✅ **SHOWPLAN_XML** - Gets execution plans without running procedures  
✅ **Cost Analysis** - Extracts estimated cost and row counts  
✅ **Parameter Support** - Pass parameters as JSON  
✅ **Formatted XML** - Pretty-printed, readable output  
✅ **SQL Exception Handling** - Comprehensive error handling  
✅ **Dapper Performance** - High-speed queries  
✅ **Async/Await** - Non-blocking operations  
✅ **Metadata Extraction** - Automatic cost and row estimation  
✅ **SSMS Compatible** - Save as .sqlplan files  
✅ **Production Ready** - Validation, logging, error handling  

**Build Status:** ✅ Success (No errors, no warnings)

Use this API to analyze stored procedure performance, identify optimization opportunities, and monitor query costs in your applications!
