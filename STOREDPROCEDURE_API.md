# Stored Procedure Detail API

## Overview
The Stored Procedure Detail API provides comprehensive information about SQL Server stored procedures, including their definitions, parameters, dependencies, and referenced objects. This API uses sys.sql_modules, OBJECT_DEFINITION, and other system catalog views to extract detailed metadata.

## Features

- ✅ **Complete SQL Definition** - Full procedure text using OBJECT_DEFINITION
- ✅ **Parameter Details** - Input/output parameters with data types, defaults, and nullability
- ✅ **Dependency Analysis** - Referenced tables, views, procedures, and functions
- ✅ **Reverse Dependencies** - Objects that call this procedure
- ✅ **Return Type Detection** - Identifies return types (NONE, INTEGER, TABLE, OUTPUT_PARAMETERS)
- ✅ **Schema Information** - Creation and modification dates
- ✅ **Async Operations** - Non-blocking async/await
- ✅ **Dapper Integration** - High-performance queries
- ✅ **Structured JSON** - Strongly typed response

## API Endpoints

### GET /api/storedprocedure/{name}

Gets comprehensive details about a specific stored procedure.

**Route Parameters:**
- `name` - Stored procedure name (can include schema, e.g., `dbo.sp_GetUserOrders`)

**Query Parameters:**
- `server` (required) - SQL Server instance
- `database` (required) - Database name
- `username` (optional) - SQL authentication username
- `password` (optional) - SQL authentication password
- `trustServerCertificate` (optional, default: true) - Trust server certificate

**Example Request:**
```
GET /api/storedprocedure/dbo.sp_GetUserOrders?server=localhost&database=MyDatabase&trustServerCertificate=true
```

### POST /api/storedprocedure/{name}

Alternative POST endpoint for better security (credentials in body instead of URL).

**Route Parameters:**
- `name` - Stored procedure name

**Request Body:**
```json
{
  "server": "localhost",
  "database": "MyDatabase",
  "username": "sa",
  "password": "YourPassword123",
  "trustServerCertificate": true
}
```

## Response Structure

### Complete Response Example

```json
{
  "schemaName": "dbo",
  "procedureName": "sp_GetUserOrders",
  "definition": "CREATE PROCEDURE sp_GetUserOrders\n    @UserId INT,\n    @StartDate DATETIME = NULL,\n    @OrderCount INT OUTPUT\nAS\nBEGIN\n    SELECT @OrderCount = COUNT(*)\n    FROM Orders\n    WHERE UserId = @UserId\n        AND (@StartDate IS NULL OR OrderDate >= @StartDate);\n    \n    SELECT \n        o.OrderId,\n        o.OrderDate,\n        o.TotalAmount,\n        c.CustomerName\n    FROM Orders o\n    INNER JOIN Customers c ON o.CustomerId = c.CustomerId\n    WHERE o.UserId = @UserId\n        AND (@StartDate IS NULL OR o.OrderDate >= @StartDate)\n    ORDER BY o.OrderDate DESC;\nEND",
  "createDate": "2026-03-15T10:30:00",
  "modifyDate": "2026-06-20T14:45:00",
  "isEncrypted": false,
  "returnType": "OUTPUT_PARAMETERS",
  "parameters": [
    {
      "parameterName": "@UserId",
      "dataType": "int",
      "maxLength": 4,
      "precision": 10,
      "scale": 0,
      "isOutput": false,
      "hasDefaultValue": false,
      "defaultValue": null,
      "isNullable": false
    },
    {
      "parameterName": "@StartDate",
      "dataType": "datetime",
      "maxLength": 8,
      "precision": 23,
      "scale": 3,
      "isOutput": false,
      "hasDefaultValue": true,
      "defaultValue": "NULL",
      "isNullable": true
    },
    {
      "parameterName": "@OrderCount",
      "dataType": "int",
      "maxLength": 4,
      "precision": 10,
      "scale": 0,
      "isOutput": true,
      "hasDefaultValue": false,
      "defaultValue": null,
      "isNullable": false
    }
  ],
  "referencedTables": [
    {
      "schemaName": "dbo",
      "objectName": "Orders",
      "objectType": "USER_TABLE",
      "dependencyType": "SELECT"
    },
    {
      "schemaName": "dbo",
      "objectName": "Customers",
      "objectType": "USER_TABLE",
      "dependencyType": "SELECT"
    }
  ],
  "referencedViews": [],
  "dependencies": [
    {
      "schemaName": "dbo",
      "objectName": "Orders",
      "objectType": "USER_TABLE",
      "dependencyType": "SELECT"
    },
    {
      "schemaName": "dbo",
      "objectName": "Customers",
      "objectType": "USER_TABLE",
      "dependencyType": "SELECT"
    }
  ],
  "dependentObjects": [
    {
      "schemaName": "dbo",
      "objectName": "sp_GenerateUserReport",
      "objectType": "SQL_STORED_PROCEDURE",
      "dependencyType": "CALLER"
    }
  ]
}
```

## Response Properties

### Main Properties

| Property | Type | Description |
|----------|------|-------------|
| schemaName | string | Schema name (e.g., "dbo") |
| procedureName | string | Stored procedure name |
| definition | string | Complete SQL definition |
| createDate | DateTime | Creation timestamp |
| modifyDate | DateTime | Last modification timestamp |
| isEncrypted | bool | Whether the procedure is encrypted |
| returnType | string | Return type (see below) |
| parameters | array | List of parameters |
| referencedTables | array | Tables referenced by this procedure |
| referencedViews | array | Views referenced by this procedure |
| dependencies | array | All dependencies (tables, views, procedures, functions) |
| dependentObjects | array | Objects that call this procedure |

### Return Types

- **NONE** - No return value
- **INTEGER** - Returns an integer value (RETURN statement)
- **TABLE** - Returns a table/result set
- **OUTPUT_PARAMETERS** - Uses OUTPUT parameters
- **UNKNOWN** - Cannot determine (usually for encrypted procedures)

### Parameter Properties

| Property | Type | Description |
|----------|------|-------------|
| parameterName | string | Parameter name (includes @) |
| dataType | string | SQL data type |
| maxLength | int? | Maximum length for strings |
| precision | byte? | Precision for numeric types |
| scale | byte? | Scale for numeric types |
| isOutput | bool | Is this an OUTPUT parameter |
| hasDefaultValue | bool | Has a default value |
| defaultValue | string? | Default value if any |
| isNullable | bool | Can accept NULL |

### Dependency Properties

| Property | Type | Description |
|----------|------|-------------|
| schemaName | string | Referenced object schema |
| objectName | string | Referenced object name |
| objectType | string | Object type (USER_TABLE, VIEW, SQL_STORED_PROCEDURE, etc.) |
| dependencyType | string | Type of dependency (SELECT, INSERT, UPDATE, DELETE, REFERENCE, CALLER) |

## Usage Examples

### Example 1: Get Procedure Details (GET with Query Params)

```bash
curl -X GET "https://localhost:7xxx/api/storedprocedure/dbo.sp_GetUserOrders?server=localhost&database=MyDatabase&trustServerCertificate=true"
```

### Example 2: Get Procedure Details (POST with Body)

```bash
curl -X POST "https://localhost:7xxx/api/storedprocedure/dbo.sp_GetUserOrders" \
  -H "Content-Type: application/json" \
  -d '{
    "server": "localhost",
    "database": "MyDatabase",
    "username": "sa",
    "password": "YourPassword123",
    "trustServerCertificate": true
  }'
```

### Example 3: Using C# Client

```csharp
using System.Net.Http.Json;

var client = new HttpClient { BaseAddress = new Uri("https://localhost:7xxx") };

var request = new StoredProcedureRequestDto
{
    Server = "localhost",
    Database = "MyDatabase",
    ProcedureName = "dbo.sp_GetUserOrders",
    Username = "sa",
    Password = "YourPassword123",
    TrustServerCertificate = true
};

var response = await client.PostAsJsonAsync(
    "/api/storedprocedure/dbo.sp_GetUserOrders", 
    request
);

var result = await response.Content.ReadFromJsonAsync<StoredProcedureDetailDto>();

// Display procedure information
Console.WriteLine($"Procedure: {result.SchemaName}.{result.ProcedureName}");
Console.WriteLine($"Return Type: {result.ReturnType}");
Console.WriteLine($"Parameters: {result.Parameters.Count}");
Console.WriteLine($"Referenced Tables: {result.ReferencedTables.Count}");

// Display parameters
foreach (var param in result.Parameters)
{
    var output = param.IsOutput ? " OUTPUT" : "";
    var defaultVal = param.HasDefaultValue ? $" = {param.DefaultValue}" : "";
    Console.WriteLine($"  {param.ParameterName} {param.DataType}{output}{defaultVal}");
}

// Display dependencies
Console.WriteLine("\nReferenced Tables:");
foreach (var table in result.ReferencedTables)
{
    Console.WriteLine($"  {table.SchemaName}.{table.ObjectName} ({table.DependencyType})");
}
```

### Example 4: Analyze Procedure Dependencies

```csharp
var result = await GetStoredProcedureDetailAsync("dbo.sp_ProcessOrder");

// Find all tables modified by the procedure
var modifiedTables = result.Dependencies
    .Where(d => d.ObjectType == "USER_TABLE" && 
                (d.DependencyType == "INSERT" || 
                 d.DependencyType == "UPDATE" || 
                 d.DependencyType == "DELETE"))
    .Select(d => $"{d.SchemaName}.{d.ObjectName}")
    .ToList();

Console.WriteLine("Modified Tables:");
foreach (var table in modifiedTables)
{
    Console.WriteLine($"  - {table}");
}

// Find procedures that call this procedure
var callers = result.DependentObjects
    .Where(d => d.ObjectType.Contains("PROCEDURE"))
    .ToList();

Console.WriteLine("\nCalled By:");
foreach (var caller in callers)
{
    Console.WriteLine($"  - {caller.SchemaName}.{caller.ObjectName}");
}
```

## SQL Queries Used

### Basic Procedure Information

```sql
SELECT 
    s.name AS SchemaName,
    p.name AS ProcedureName,
    OBJECT_DEFINITION(p.object_id) AS Definition,
    p.create_date AS CreateDate,
    p.modify_date AS ModifyDate,
    p.is_encrypted AS IsEncrypted
FROM sys.procedures p
INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
WHERE s.name = @SchemaName AND p.name = @ProcedureName
```

### Parameters Query

```sql
SELECT 
    p.name AS ParameterName,
    TYPE_NAME(p.user_type_id) AS DataType,
    p.max_length AS MaxLength,
    p.precision AS Precision,
    p.scale AS Scale,
    p.is_output AS IsOutput,
    p.has_default_value AS HasDefaultValue,
    p.default_value AS DefaultValue,
    p.is_nullable AS IsNullable
FROM sys.parameters p
INNER JOIN sys.procedures pr ON p.object_id = pr.object_id
INNER JOIN sys.schemas s ON pr.schema_id = s.schema_id
WHERE s.name = @SchemaName 
    AND pr.name = @ProcedureName
    AND p.parameter_id > 0  -- Exclude return value
ORDER BY p.parameter_id
```

### Dependencies Query

```sql
SELECT DISTINCT
    s.name AS SchemaName,
    o.name AS ObjectName,
    o.type_desc AS ObjectType,
    CASE 
        WHEN sed.is_selected = 1 THEN 'SELECT'
        WHEN sed.is_updated = 1 THEN 'UPDATE'
        WHEN sed.is_insert_all = 1 OR sed.is_insert_selective = 1 THEN 'INSERT'
        WHEN sed.is_delete = 1 THEN 'DELETE'
        ELSE 'REFERENCE'
    END AS DependencyType
FROM sys.procedures p
INNER JOIN sys.schemas ps ON p.schema_id = ps.schema_id
INNER JOIN sys.sql_expression_dependencies sed ON p.object_id = sed.referencing_id
INNER JOIN sys.objects o ON sed.referenced_id = o.object_id
INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
WHERE ps.name = @SchemaName 
    AND p.name = @ProcedureName
    AND o.type IN ('U', 'V', 'P', 'FN', 'IF', 'TF')
ORDER BY s.name, o.name
```

### Dependent Objects Query

```sql
SELECT DISTINCT
    s.name AS SchemaName,
    o.name AS ObjectName,
    o.type_desc AS ObjectType,
    'CALLER' AS DependencyType
FROM sys.procedures p
INNER JOIN sys.schemas ps ON p.schema_id = ps.schema_id
INNER JOIN sys.sql_expression_dependencies sed ON p.object_id = sed.referenced_id
INNER JOIN sys.objects o ON sed.referencing_id = o.object_id
INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
WHERE ps.name = @SchemaName 
    AND p.name = @ProcedureName
ORDER BY s.name, o.name
```

## Use Cases

### 1. Documentation Generation

Generate comprehensive documentation for stored procedures:

```csharp
var procedures = await GetAllStoredProceduresAsync();
var documentation = new StringBuilder();

foreach (var procName in procedures)
{
    var details = await GetStoredProcedureDetailAsync(procName);
    
    documentation.AppendLine($"## {details.SchemaName}.{details.ProcedureName}");
    documentation.AppendLine($"**Created:** {details.CreateDate}");
    documentation.AppendLine($"**Modified:** {details.ModifyDate}");
    documentation.AppendLine($"**Return Type:** {details.ReturnType}");
    
    documentation.AppendLine("\n### Parameters");
    foreach (var param in details.Parameters)
    {
        documentation.AppendLine($"- `{param.ParameterName}` ({param.DataType}) {(param.IsOutput ? "OUTPUT" : "")}");
    }
    
    documentation.AppendLine("\n### Dependencies");
    foreach (var dep in details.Dependencies)
    {
        documentation.AppendLine($"- {dep.SchemaName}.{dep.ObjectName} ({dep.DependencyType})");
    }
    
    documentation.AppendLine($"\n### Definition\n```sql\n{details.Definition}\n```\n");
}

File.WriteAllText("procedures-documentation.md", documentation.ToString());
```

### 2. Impact Analysis

Analyze the impact of changing a table:

```csharp
public async Task<List<string>> AnalyzeTableImpact(string tableName)
{
    var allProcedures = await GetAllStoredProceduresAsync();
    var impactedProcedures = new List<string>();
    
    foreach (var procName in allProcedures)
    {
        var details = await GetStoredProcedureDetailAsync(procName);
        
        var usesTable = details.Dependencies
            .Any(d => d.ObjectName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            
        if (usesTable)
        {
            var dependency = details.Dependencies
                .First(d => d.ObjectName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
                
            impactedProcedures.Add(
                $"{details.SchemaName}.{details.ProcedureName} ({dependency.DependencyType})"
            );
        }
    }
    
    return impactedProcedures;
}
```

### 3. Dependency Graph Visualization

Build a dependency graph:

```csharp
public class DependencyGraph
{
    public Dictionary<string, List<string>> Dependencies { get; } = new();
    
    public async Task BuildGraphAsync(string procedureName)
    {
        var details = await GetStoredProcedureDetailAsync(procedureName);
        var procFullName = $"{details.SchemaName}.{details.ProcedureName}";
        
        if (Dependencies.ContainsKey(procFullName))
            return; // Already processed
            
        var deps = details.Dependencies
            .Select(d => $"{d.SchemaName}.{d.ObjectName}")
            .ToList();
            
        Dependencies[procFullName] = deps;
        
        // Recursively process procedure dependencies
        foreach (var dep in details.Dependencies.Where(d => d.ObjectType.Contains("PROCEDURE")))
        {
            await BuildGraphAsync($"{dep.SchemaName}.{dep.ObjectName}");
        }
    }
}
```

### 4. Parameter Validation

Validate procedure calls:

```csharp
public async Task<bool> ValidateProcedureCall(
    string procedureName, 
    Dictionary<string, object> parameters)
{
    var details = await GetStoredProcedureDetailAsync(procedureName);
    
    // Check required parameters
    var requiredParams = details.Parameters
        .Where(p => !p.HasDefaultValue && !p.IsOutput)
        .Select(p => p.ParameterName.TrimStart('@'))
        .ToList();
        
    var missingParams = requiredParams
        .Where(p => !parameters.ContainsKey(p))
        .ToList();
        
    if (missingParams.Any())
    {
        throw new ArgumentException(
            $"Missing required parameters: {string.Join(", ", missingParams)}"
        );
    }
    
    return true;
}
```

### 5. Code Generation

Generate procedure call code:

```csharp
public string GenerateCSharpCode(StoredProcedureDetailDto details)
{
    var code = new StringBuilder();
    
    code.AppendLine($"// Execute {details.SchemaName}.{details.ProcedureName}");
    code.AppendLine("using (var connection = new SqlConnection(connectionString))");
    code.AppendLine("{");
    code.AppendLine("    await connection.OpenAsync();");
    code.AppendLine($"    using (var command = new SqlCommand(\"{details.SchemaName}.{details.ProcedureName}\", connection))");
    code.AppendLine("    {");
    code.AppendLine("        command.CommandType = CommandType.StoredProcedure;");
    
    foreach (var param in details.Parameters)
    {
        var direction = param.IsOutput ? "ParameterDirection.Output" : "ParameterDirection.Input";
        code.AppendLine($"        command.Parameters.Add(\"{param.ParameterName}\", SqlDbType.{GetSqlDbType(param.DataType)}).Direction = {direction};");
        
        if (!param.IsOutput)
        {
            code.AppendLine($"        command.Parameters[\"{param.ParameterName}\"].Value = /* value */;");
        }
    }
    
    code.AppendLine("        await command.ExecuteNonQueryAsync();");
    code.AppendLine("    }");
    code.AppendLine("}");
    
    return code.ToString();
}
```

## Implementation Details

### Files Created

1. **ParameterInfoDto.cs** - Parameter information
2. **DependencyInfoDto.cs** - Dependency information
3. **StoredProcedureDetailDto.cs** - Main response DTO
4. **StoredProcedureRequestDto.cs** - Request DTO
5. **IStoredProcedureService.cs** - Service interface
6. **StoredProcedureService.cs** - Service implementation
7. **StoredProcedureRequestDtoValidator.cs** - Request validator
8. **StoredProcedureController.cs** - API controller

### Service Architecture

**StoredProcedureService:**
- Uses Dapper for high-performance queries
- Leverages ConnectionService for connection management
- Comprehensive error handling and logging
- Async/await throughout
- Parses schema.procedurename format
- Determines return type from definition

**Return Type Detection:**
1. Checks for table returns (RETURN + SELECT/TABLE)
2. Checks for OUTPUT parameters
3. Checks for explicit RETURN statements
4. Defaults to NONE

### System Views Used

- `sys.procedures` - Procedure metadata
- `sys.parameters` - Parameter information
- `sys.sql_expression_dependencies` - Dependencies
- `sys.objects` - Object metadata
- `sys.schemas` - Schema information
- `OBJECT_DEFINITION()` - Get procedure definition

## Security Considerations

### Connection Security
- Use POST endpoint for sensitive credentials
- Always use HTTPS in production
- Consider authentication/authorization
- Implement rate limiting

### Permissions Required

```sql
-- View procedure definitions
GRANT VIEW DEFINITION ON SCHEMA::dbo TO [username];

-- Or database-wide
GRANT VIEW ANY DEFINITION TO [username];

-- View metadata
GRANT VIEW DATABASE STATE TO [username];
```

### Encrypted Procedures

For encrypted procedures:
- `isEncrypted` will be `true`
- `definition` will be `null`
- `returnType` will be `UNKNOWN`
- Parameters and dependencies still available

## Performance Optimization

### Caching

Consider caching procedure metadata:

```csharp
public class CachedStoredProcedureService : IStoredProcedureService
{
    private readonly IStoredProcedureService _innerService;
    private readonly IMemoryCache _cache;
    
    public async Task<StoredProcedureDetailDto> GetStoredProcedureDetailAsync(
        StoredProcedureRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"proc_{request.Server}_{request.Database}_{request.ProcedureName}";
        
        if (!_cache.TryGetValue(cacheKey, out StoredProcedureDetailDto result))
        {
            result = await _innerService.GetStoredProcedureDetailAsync(request, cancellationToken);
            
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(30));
        }
        
        return result;
    }
}
```

### Query Optimization

The service uses:
- Efficient sys catalog views
- Minimal joins
- Filtered queries (parameter_id > 0)
- Indexed system tables

## Troubleshooting

### Issue: "Stored procedure not found"
**Solution**: Verify procedure name and schema. Use schema.procedurename format or ensure "dbo" schema exists.

### Issue: "Definition is null"
**Solution**: Procedure is encrypted. User needs VIEW DEFINITION permission, or procedure needs to be decrypted.

### Issue: "Empty dependencies"
**Solution**: sys.sql_expression_dependencies requires SQL Server 2008+. Ensure proper permissions.

### Issue: "Timeout errors"
**Solution**: Increase command timeout or optimize large procedures.

## Testing in Swagger

1. Navigate to `https://localhost:7xxx`
2. Find **StoredProcedure** section
3. Try **GET /api/storedprocedure/{name}**
4. Enter procedure name (e.g., `dbo.sp_GetUserOrders`)
5. Add query parameters for connection
6. Click "Execute"

Or use POST endpoint:
1. Try **POST /api/storedprocedure/{name}**
2. Enter procedure name in route
3. Add connection details in body
4. Click "Execute"

## Summary

The Stored Procedure Detail API provides:

✅ **Comprehensive Metadata** - Everything about a stored procedure
✅ **SQL Queries** - sys.sql_modules and OBJECT_DEFINITION
✅ **Structured JSON** - Clean, strongly-typed responses
✅ **Dependency Analysis** - Forward and reverse dependencies
✅ **Parameter Details** - Complete parameter information
✅ **Return Type Detection** - Automatic return type identification
✅ **Async/Await** - Non-blocking operations
✅ **Dapper Performance** - High-speed queries
✅ **Error Handling** - Comprehensive error management
✅ **Two Endpoints** - GET (query params) and POST (body)

**Build Status:** ✅ Success (No errors, no warnings)
