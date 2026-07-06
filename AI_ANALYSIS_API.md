# AI-Powered Stored Procedure Analysis API

## Overview
The AI-Powered Analysis API uses OpenAI's GPT models to provide comprehensive, intelligent analysis of SQL Server stored procedures. It examines procedure definitions, execution plans, table schemas, indexes, and foreign keys to identify performance issues and provide actionable optimization recommendations.

## Features

- ✅ **OpenAI Integration** - GPT-4 powered analysis
- ✅ **Comprehensive Context** - Includes procedure, tables, indexes, foreign keys, execution plan
- ✅ **14 Issue Types** - Checks for missing indexes, table scans, cursors, and more
- ✅ **Structured JSON Response** - Performance score, issues, recommendations
- ✅ **Retry Policy** - Automatic retry with exponential backoff
- ✅ **Timeout Handling** - Configurable request timeout
- ✅ **HttpClientFactory** - Efficient HTTP client management
- ✅ **Dependency Injection** - Full DI support
- ✅ **Detailed Recommendations** - Implementation steps and SQL code
- ✅ **Optimized Code Suggestions** - AI-generated optimized versions

## API Endpoint

### POST /api/analysis

Analyzes a stored procedure using AI and provides optimization recommendations.

**Request Body:**
```json
{
  "server": "localhost",
  "database": "MyDatabase",
  "username": "sa",
  "password": "YourPassword123",
  "trustServerCertificate": true,
  "storedProcedureName": "dbo.sp_GetUserOrders",
  "parameters": "{\"@UserId\": 123}",
  "includeExecutionPlan": true
}
```

**Request Properties:**

| Property | Type | Required | Description |
|----------|------|----------|-------------|
| server | string | Yes | SQL Server instance name or IP |
| database | string | Yes | Database name |
| username | string | No | SQL authentication username |
| password | string | No | SQL authentication password |
| trustServerCertificate | bool | No | Trust server certificate (default: true) |
| storedProcedureName | string | Yes | Stored procedure name (can include schema) |
| parameters | string | No | JSON object with procedure parameters |
| includeExecutionPlan | bool | No | Include execution plan in analysis (default: true) |

**Response:**
```json
{
  "success": true,
  "storedProcedureName": "dbo.sp_GetUserOrders",
  "databaseName": "MyDatabase",
  "performanceScore": 65,
  "severity": "Medium",
  "summary": "The stored procedure has several performance issues including missing indexes and SELECT * usage. Implementing the recommended indexes could improve performance by 40%.",
  "issues": [
    {
      "type": "Missing Index",
      "severity": "High",
      "description": "The Orders table is missing an index on (UserId, OrderDate) which would significantly improve query performance.",
      "lineNumber": 15,
      "codeSnippet": "SELECT * FROM Orders WHERE UserId = @UserId AND OrderDate >= @StartDate"
    },
    {
      "type": "SELECT *",
      "severity": "Medium",
      "description": "Using SELECT * instead of specific columns reduces performance and maintainability.",
      "lineNumber": 15,
      "codeSnippet": "SELECT * FROM Orders WHERE UserId = @UserId"
    },
    {
      "type": "Table Scan",
      "severity": "High",
      "description": "Query performs a full table scan on the Orders table due to missing index.",
      "lineNumber": null,
      "codeSnippet": null
    }
  ],
  "recommendations": [
    {
      "priority": "High",
      "title": "Add Covering Index on Orders Table",
      "description": "Create a covering index on (UserId, OrderDate) including (OrderId, TotalAmount, CustomerId) to eliminate the table scan and improve query performance.",
      "expectedImpact": "40% performance improvement",
      "implementationSteps": [
        "Create the index during off-peak hours",
        "Monitor index usage with sys.dm_db_index_usage_stats",
        "Consider partitioning if table is large"
      ],
      "sqlCode": "CREATE NONCLUSTERED INDEX IX_Orders_UserId_OrderDate\nON dbo.Orders (UserId, OrderDate)\nINCLUDE (OrderId, TotalAmount, CustomerId);"
    },
    {
      "priority": "Medium",
      "title": "Replace SELECT * with Specific Columns",
      "description": "Specify only the columns needed instead of using SELECT * to reduce data transfer and improve performance.",
      "expectedImpact": "15% performance improvement",
      "implementationSteps": [
        "Identify columns actually used by consumers",
        "Update SELECT statement with specific columns",
        "Test all calling applications"
      ],
      "sqlCode": "SELECT o.OrderId, o.OrderDate, o.TotalAmount, c.CustomerName\nFROM Orders o\nINNER JOIN Customers c ON o.CustomerId = c.CustomerId\nWHERE o.UserId = @UserId AND o.OrderDate >= @StartDate"
    }
  ],
  "optimizedCode": "CREATE PROCEDURE dbo.sp_GetUserOrders\n    @UserId INT,\n    @StartDate DATETIME = NULL\nAS\nBEGIN\n    SET NOCOUNT ON;\n    \n    SELECT \n        o.OrderId,\n        o.OrderDate,\n        o.TotalAmount,\n        c.CustomerName\n    FROM dbo.Orders o WITH (NOLOCK)\n    INNER JOIN dbo.Customers c WITH (NOLOCK) ON o.CustomerId = c.CustomerId\n    WHERE o.UserId = @UserId\n        AND (@StartDate IS NULL OR o.OrderDate >= @StartDate)\n    ORDER BY o.OrderDate DESC;\nEND",
  "errorMessage": null,
  "timestamp": "2026-07-04T10:30:00Z"
}
```

## Configuration

### appsettings.json

Add OpenAI configuration to your `appsettings.Development.json`:

```json
{
  "OpenAI": {
    "ApiKey": "sk-your-openai-api-key-here",
    "ApiEndpoint": "https://api.openai.com/v1/chat/completions",
    "Model": "gpt-4",
    "MaxTokens": 2000,
    "Temperature": 0.7,
    "TimeoutSeconds": 60,
    "MaxRetryAttempts": 3,
    "RetryDelayMilliseconds": 1000
  }
}
```

**Configuration Properties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| ApiKey | string | (required) | Your OpenAI API key |
| ApiEndpoint | string | OpenAI chat endpoint | API endpoint URL |
| Model | string | gpt-4 | Model to use (gpt-4, gpt-3.5-turbo) |
| MaxTokens | int | 2000 | Maximum tokens in response |
| Temperature | double | 0.7 | Response randomness (0.0-2.0) |
| TimeoutSeconds | int | 60 | Request timeout in seconds |
| MaxRetryAttempts | int | 3 | Maximum retry attempts on failure |
| RetryDelayMilliseconds | int | 1000 | Delay between retries |

### Getting an OpenAI API Key

1. Go to https://platform.openai.com/
2. Sign up or log in
3. Navigate to API Keys
4. Create a new secret key
5. Copy the key to your `appsettings.Development.json`

⚠️ **Important:** Never commit your API key to source control. Use environment variables or user secrets in production.

## Analysis Checks

The AI analyzes stored procedures for these 14 performance issues:

### 1. **Missing Indexes**
- Identifies tables that would benefit from indexes
- Suggests covering indexes where applicable
- Provides CREATE INDEX statements

### 2. **SELECT ***
- Checks for SELECT * usage
- Recommends specific column lists
- Explains performance and maintainability benefits

### 3. **Table Scan**
- Identifies full table scans
- Suggests indexes to convert scans to seeks
- Estimates performance impact

### 4. **Index Scan**
- Looks for index scans that could be seeks
- Recommends index improvements
- Analyzes WHERE clause predicates

### 5. **Cursor Usage**
- Detects cursor patterns
- Suggests set-based alternatives
- Provides rewritten queries

### 6. **Temp Tables**
- Analyzes temp table usage
- Suggests table variables or CTEs where appropriate
- Checks for proper indexing on temp tables

### 7. **NOLOCK Hints**
- Checks for NOLOCK usage
- Warns about dirty reads
- Suggests alternatives (READ COMMITTED SNAPSHOT)

### 8. **Parameter Sniffing**
- Identifies potential parameter sniffing issues
- Suggests OPTIMIZE FOR hints or RECOMPILE
- Analyzes parameter usage patterns

### 9. **Implicit Conversion**
- Checks for data type mismatches
- Identifies non-SARGable predicates
- Suggests proper data type usage

### 10. **Scalar Functions**
- Identifies inline scalar functions (row-by-row execution)
- Suggests inline table-valued functions
- Estimates performance impact

### 11. **Covering Index**
- Suggests covering indexes for frequently accessed columns
- Analyzes INCLUDE column opportunities
- Balances index size vs performance

### 12. **Nested Queries**
- Checks for nested subqueries
- Suggests JOINs or CTEs
- Provides rewritten queries

### 13. **Duplicate Joins**
- Looks for redundant join conditions
- Identifies unnecessary table references
- Simplifies query structure

### 14. **Query Rewrite**
- Suggests better ways to write queries
- Identifies anti-patterns
- Provides optimized alternatives

## Usage Examples

### Example 1: Basic Analysis

```bash
curl -X POST "https://localhost:7xxx/api/analysis" \
  -H "Content-Type: application/json" \
  -d '{
    "server": "localhost",
    "database": "MyDatabase",
    "storedProcedureName": "dbo.sp_GetUserOrders",
    "trustServerCertificate": true,
    "includeExecutionPlan": true
  }'
```

### Example 2: With Parameters

```bash
curl -X POST "https://localhost:7xxx/api/analysis" \
  -H "Content-Type: application/json" \
  -d '{
    "server": "localhost",
    "database": "MyDatabase",
    "username": "sa",
    "password": "YourPassword123",
    "storedProcedureName": "dbo.sp_SearchOrders",
    "parameters": "{\"@CustomerId\": 456, \"@StartDate\": \"2026-01-01\"}",
    "trustServerCertificate": true,
    "includeExecutionPlan": true
  }'
```

### Example 3: Using C# Client

```csharp
using System.Net.Http.Json;

var client = new HttpClient { BaseAddress = new Uri("https://localhost:7xxx") };

var request = new AnalyzeStoredProcedureRequestDto
{
    Server = "localhost",
    Database = "MyDatabase",
    Username = "sa",
    Password = "YourPassword123",
    TrustServerCertificate = true,
    StoredProcedureName = "dbo.sp_GetUserOrders",
    Parameters = "{\"@UserId\": 123}",
    IncludeExecutionPlan = true
};

var response = await client.PostAsJsonAsync("/api/analysis", request);
var analysis = await response.Content.ReadFromJsonAsync<StoredProcedureAnalysisDto>();

if (analysis.Success)
{
    Console.WriteLine($"Performance Score: {analysis.PerformanceScore}/100");
    Console.WriteLine($"Severity: {analysis.Severity}");
    Console.WriteLine($"\nSummary: {analysis.Summary}");
    
    Console.WriteLine($"\nIssues Found: {analysis.Issues.Count}");
    foreach (var issue in analysis.Issues)
    {
        Console.WriteLine($"  [{issue.Severity}] {issue.Type}: {issue.Description}");
    }
    
    Console.WriteLine($"\nRecommendations: {analysis.Recommendations.Count}");
    foreach (var rec in analysis.Recommendations)
    {
        Console.WriteLine($"  [{rec.Priority}] {rec.Title}");
        Console.WriteLine($"    Impact: {rec.ExpectedImpact}");
        if (!string.IsNullOrEmpty(rec.SqlCode))
        {
            Console.WriteLine($"    SQL:\n{rec.SqlCode}");
        }
    }
}
else
{
    Console.WriteLine($"Error: {analysis.ErrorMessage}");
}
```

### Example 4: Batch Analysis

```csharp
public async Task<Dictionary<string, StoredProcedureAnalysisDto>> AnalyzeAllProceduresAsync(
    List<string> procedureNames)
{
    var results = new Dictionary<string, StoredProcedureAnalysisDto>();
    
    foreach (var procName in procedureNames)
    {
        var request = new AnalyzeStoredProcedureRequestDto
        {
            Server = "localhost",
            Database = "MyDatabase",
            StoredProcedureName = procName,
            TrustServerCertificate = true,
            IncludeExecutionPlan = true
        };
        
        var analysis = await AnalyzeStoredProcedureAsync(request);
        results[procName] = analysis;
        
        // Respect API rate limits
        await Task.Delay(2000);
    }
    
    // Sort by performance score (lowest first - most issues)
    return results.OrderBy(kvp => kvp.Value.PerformanceScore ?? 0)
        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
}
```

### Example 5: Generate Report

```csharp
public async Task GenerateOptimizationReportAsync(string outputPath)
{
    var procedures = new[] 
    { 
        "dbo.sp_GetUserOrders", 
        "dbo.sp_ProcessPayment",
        "dbo.sp_GenerateReport"
    };
    
    var report = new StringBuilder();
    report.AppendLine("# Stored Procedure Optimization Report");
    report.AppendLine($"Generated: {DateTime.Now}");
    report.AppendLine();
    
    foreach (var proc in procedures)
    {
        var request = new AnalyzeStoredProcedureRequestDto
        {
            Server = "localhost",
            Database = "MyDatabase",
            StoredProcedureName = proc,
            TrustServerCertificate = true
        };
        
        var analysis = await AnalyzeStoredProcedureAsync(request);
        
        report.AppendLine($"## {proc}");
        report.AppendLine($"**Score:** {analysis.PerformanceScore}/100");
        report.AppendLine($"**Severity:** {analysis.Severity}");
        report.AppendLine();
        report.AppendLine($"### Summary");
        report.AppendLine(analysis.Summary);
        report.AppendLine();
        
        if (analysis.Issues.Any())
        {
            report.AppendLine($"### Issues ({analysis.Issues.Count})");
            foreach (var issue in analysis.Issues)
            {
                report.AppendLine($"- **[{issue.Severity}] {issue.Type}:** {issue.Description}");
            }
            report.AppendLine();
        }
        
        if (analysis.Recommendations.Any())
        {
            report.AppendLine($"### Recommendations ({analysis.Recommendations.Count})");
            foreach (var rec in analysis.Recommendations)
            {
                report.AppendLine($"#### [{rec.Priority}] {rec.Title}");
                report.AppendLine(rec.Description);
                if (!string.IsNullOrEmpty(rec.ExpectedImpact))
                {
                    report.AppendLine($"**Impact:** {rec.ExpectedImpact}");
                }
                report.AppendLine();
            }
        }
        
        report.AppendLine("---");
        report.AppendLine();
    }
    
    File.WriteAllText(outputPath, report.ToString());
    Console.WriteLine($"Report saved to: {outputPath}");
}
```

## Implementation Details

### Architecture

**PromptBuilderService:**
- Gathers comprehensive context from database
- Queries stored procedure details (definition, parameters, dependencies)
- Retrieves table schemas for referenced tables
- Fetches indexes and foreign keys
- Gets execution plan (if enabled)
- Builds structured prompt with all context and analysis requirements

**OpenAIService:**
- Uses HttpClientFactory for efficient HTTP client management
- Implements retry policy with exponential backoff
- Handles timeouts and errors gracefully
- Requests JSON-only response format
- Parses AI response into structured DTOs
- Maps AI analysis to application DTOs

### Files Created

1. **OpenAIOptions.cs** - Configuration options for OpenAI
2. **AnalyzeStoredProcedureRequestDto.cs** - Request DTO
3. **StoredProcedureAnalysisDto.cs** - Analysis result with issues and recommendations
4. **IOpenAIService.cs** - OpenAI service interface
5. **IPromptBuilderService.cs** - Prompt builder service interface
6. **OpenAIService.cs** - OpenAI integration with retry policy
7. **PromptBuilderService.cs** - Prompt builder implementation
8. **AnalyzeStoredProcedureRequestDtoValidator.cs** - Request validation
9. **AnalysisController.cs** - API controller

### Prompt Structure

The AI receives a comprehensive prompt including:

```
You are an expert SQL Server performance analyst...

**IMPORTANT: Return ONLY valid JSON...**

---

**Database:** MyDatabase
**Stored Procedure:** dbo.sp_GetUserOrders

## Stored Procedure Definition
```sql
CREATE PROCEDURE...
```

### Parameters
- @UserId INT
- @StartDate DATETIME = NULL

## Referenced Tables Schema
### dbo.Orders
- Type: BASE TABLE
- Row Count: 1,500,000
- Total Space: 450.25 MB

## Indexes
- dbo.Orders.PK_Orders (CLUSTERED, PRIMARY KEY)
  - Columns: OrderId

## Foreign Keys
- FK_Orders_Customers
  - dbo.Orders(CustomerId) → dbo.Customers(CustomerId)

## Execution Plan Analysis
- Estimated Cost: 15.43
- Estimated Rows: 25,000

### Execution Plan XML
```xml
...
```

## Analysis Requirements
Analyze for: Missing indexes, SELECT *, Table Scan, ...

**Return ONLY valid JSON**
```

### Retry Policy

The service implements automatic retry with exponential backoff:

1. **Attempt 1** - Immediate
2. **Attempt 2** - After 1 second (1 × RetryDelayMilliseconds)
3. **Attempt 3** - After 2 seconds (2 × RetryDelayMilliseconds)

Retries on:
- TaskCanceledException (timeout)
- HttpRequestException (network errors)

### Error Handling

Comprehensive error handling:
- Missing API key validation
- HTTP request errors
- JSON parsing errors
- Timeout handling
- Retry logic
- Structured error responses

## Security Considerations

### API Key Protection

⚠️ **Critical:** Never commit API keys to source control

**Use User Secrets (Development):**
```bash
dotnet user-secrets init
dotnet user-secrets set "OpenAI:ApiKey" "sk-your-key-here"
```

**Use Environment Variables (Production):**
```bash
export OpenAI__ApiKey="sk-your-key-here"
```

**Use Azure Key Vault (Production):**
```csharp
builder.Configuration.AddAzureKeyVault(/* ... */);
```

### Connection Security

- Always use HTTPS in production
- Implement authentication/authorization
- Consider rate limiting
- Don't log API keys or passwords

## Cost Considerations

### OpenAI Pricing

- **GPT-4:** ~$0.03 per 1K input tokens, ~$0.06 per 1K output tokens
- **GPT-3.5-turbo:** ~$0.0015 per 1K input tokens, ~$0.002 per 1K output tokens

### Typical Request

- **Prompt:** ~2,000-5,000 tokens (procedure + context)
- **Response:** ~500-2,000 tokens (analysis)
- **Cost per analysis (GPT-4):** ~$0.15-$0.45
- **Cost per analysis (GPT-3.5):** ~$0.01-$0.02

### Optimization Tips

1. **Use GPT-3.5-turbo** for cost-effective analysis
2. **Disable execution plan** if not needed (reduces tokens)
3. **Cache results** for frequently analyzed procedures
4. **Batch analysis** during off-peak hours
5. **Set MaxTokens** appropriately (lower = cheaper)

## Performance

### Response Times

- **Prompt Building:** ~1-3 seconds (database queries)
- **OpenAI API:** ~5-15 seconds (depending on model and tokens)
- **Total:** ~6-18 seconds per analysis

### Optimization

- Consider caching analysis results
- Use background jobs for batch analysis
- Implement request throttling
- Use GPT-3.5-turbo for faster responses

## Testing in Swagger

1. Navigate to `https://localhost:7xxx`
2. Find **Analysis** section
3. Click **POST /api/analysis**
4. Click "Try it out"
5. Enter request body with your database details
6. **Important:** Add your OpenAI API key to `appsettings.Development.json` first
7. Click "Execute"
8. Review comprehensive analysis with performance score, issues, and recommendations

## Troubleshooting

### Issue: "OpenAI API key is not configured"
**Solution:** Add your API key to `appsettings.Development.json`:
```json
{
  "OpenAI": {
    "ApiKey": "sk-your-key-here"
  }
}
```

### Issue: "Request timed out"
**Solution:** Increase timeout in configuration:
```json
{
  "OpenAI": {
    "TimeoutSeconds": 120
  }
}
```

### Issue: "Failed to parse AI response"
**Solution:** The AI returned invalid JSON. Check the prompt or try again. The service will retry automatically.

### Issue: "Rate limit exceeded"
**Solution:** Implement request throttling or wait before making more requests.

## Summary

The AI-Powered Analysis API provides:

✅ **OpenAI Integration** - GPT-4 powered intelligent analysis  
✅ **Comprehensive Context** - Procedure, tables, indexes, execution plan  
✅ **14 Issue Types** - Missing indexes, table scans, cursors, and more  
✅ **Structured Results** - Performance score, issues, recommendations  
✅ **Retry Policy** - Automatic retry with exponential backoff  
✅ **Timeout Handling** - Configurable request timeout  
✅ **HttpClientFactory** - Efficient HTTP client management  
✅ **Actionable Recommendations** - Implementation steps and SQL code  
✅ **Optimized Code** - AI-generated optimized procedure versions  
✅ **Production Ready** - Error handling, validation, logging  

**Build Status:** ✅ Success

This comprehensive AI-powered analysis helps identify performance bottlenecks and provides expert-level optimization recommendations automatically! 🚀
