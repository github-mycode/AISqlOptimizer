# Database Metadata API

## Overview
The Metadata API provides comprehensive access to SQL Server database metadata, allowing you to query information about databases, tables, views, functions, stored procedures, indexes, and foreign keys using INFORMATION_SCHEMA and sys catalog views.

## Features

- ✅ **Async Operations** - All methods use async/await for non-blocking I/O
- ✅ **Dapper Integration** - High-performance data access
- ✅ **Strongly Typed DTOs** - Type-safe responses
- ✅ **INFORMATION_SCHEMA** - Standard SQL metadata queries
- ✅ **Sys Catalog Views** - Extended SQL Server metadata
- ✅ **Comprehensive Logging** - All operations logged
- ✅ **Error Handling** - Graceful error management
- ✅ **Input Validation** - FluentValidation for requests

## API Endpoints

All endpoints use POST with connection details in the request body for security.

### 1. Get Databases
**POST** `/api/metadata/databases`

Returns list of user databases on the SQL Server instance (excludes system databases).

**Request Body:**
```json
{
  "server": "localhost",
  "database": "master",
  "username": "sa",
  "password": "YourPassword123",
  "trustServerCertificate": true
}
```

**Response:**
```json
[
  {
    "databaseName": "MyDatabase",
    "databaseId": 5,
    "createDate": "2026-01-15T10:30:00",
    "compatibilityLevel": 160,
    "collationName": "SQL_Latin1_General_CP1_CI_AS",
    "recoveryModel": "SIMPLE",
    "state": "ONLINE"
  }
]
```

### 2. Get Tables
**POST** `/api/metadata/tables`

Returns list of tables with size and row count information.

**Response:**
```json
[
  {
    "schemaName": "dbo",
    "tableName": "Users",
    "tableType": "USER_TABLE",
    "rowCount": 1500,
    "totalSpaceKB": 256,
    "usedSpaceKB": 192,
    "createDate": "2026-01-20T14:20:00",
    "modifyDate": "2026-07-03T09:15:00"
  }
]
```

### 3. Get Views
**POST** `/api/metadata/views`

Returns list of views with their definitions.

**Response:**
```json
[
  {
    "schemaName": "dbo",
    "viewName": "vw_ActiveUsers",
    "definition": "CREATE VIEW vw_ActiveUsers AS SELECT * FROM Users WHERE IsActive = 1",
    "isUpdatable": "YES",
    "checkOption": "NONE",
    "createDate": "2026-02-10T11:00:00",
    "modifyDate": "2026-06-15T16:30:00"
  }
]
```

### 4. Get Functions
**POST** `/api/metadata/functions`

Returns list of user-defined functions.

**Response:**
```json
[
  {
    "schemaName": "dbo",
    "functionName": "fn_CalculateDiscount",
    "functionType": "FN",
    "functionTypeDesc": "SQL_SCALAR_FUNCTION",
    "definition": "CREATE FUNCTION fn_CalculateDiscount(@amount DECIMAL) RETURNS DECIMAL AS BEGIN...",
    "createDate": "2026-03-05T13:45:00",
    "modifyDate": "2026-05-20T10:20:00"
  }
]
```

**Function Types:**
- **FN** - SQL Scalar Function
- **IF** - SQL Inline Table-Valued Function
- **TF** - SQL Table-Valued Function
- **FS** - Assembly (CLR) Scalar Function
- **FT** - Assembly (CLR) Table-Valued Function

### 5. Get Stored Procedures
**POST** `/api/metadata/storedprocedures`

Returns list of stored procedures with metadata.

**Response:**
```json
[
  {
    "schemaName": "dbo",
    "procedureName": "sp_GetUserOrders",
    "procedureType": "SQL_STORED_PROCEDURE",
    "definition": "CREATE PROCEDURE sp_GetUserOrders @UserId INT AS BEGIN...",
    "createDate": "2026-02-15T09:30:00",
    "modifyDate": "2026-06-28T14:15:00",
    "isRecompiled": false,
    "isEncrypted": false
  }
]
```

### 6. Get Indexes
**POST** `/api/metadata/indexes`

Returns list of indexes with their columns and properties.

**Response:**
```json
[
  {
    "schemaName": "dbo",
    "tableName": "Users",
    "indexName": "IX_Users_Email",
    "indexType": "NONCLUSTERED",
    "isUnique": true,
    "isPrimaryKey": false,
    "isUniqueConstraint": false,
    "indexedColumns": "Email ASC",
    "includedColumns": "FirstName, LastName",
    "filterDefinition": null
  }
]
```

**Index Types:**
- **CLUSTERED** - Clustered index
- **NONCLUSTERED** - Non-clustered index
- **XML** - XML index
- **SPATIAL** - Spatial index
- **CLUSTERED COLUMNSTORE** - Columnstore index
- **NONCLUSTERED COLUMNSTORE** - Non-clustered columnstore

### 7. Get Foreign Keys
**POST** `/api/metadata/foreignkeys`

Returns list of foreign key relationships.

**Response:**
```json
[
  {
    "constraintName": "FK_Orders_Users",
    "schemaName": "dbo",
    "tableName": "Orders",
    "columnNames": "UserId",
    "referencedSchemaName": "dbo",
    "referencedTableName": "Users",
    "referencedColumnNames": "UserId",
    "deleteRule": "NO_ACTION",
    "updateRule": "NO_ACTION",
    "isDisabled": false,
    "isNotTrusted": false
  }
]
```

**Referential Actions:**
- **NO_ACTION** - Prevent delete/update if referenced
- **CASCADE** - Delete/update related rows
- **SET_NULL** - Set foreign key to NULL
- **SET_DEFAULT** - Set foreign key to default value

## Implementation Details

### DTOs Created

1. **MetadataRequestDto** - Connection details for metadata queries
2. **DatabaseInfoDto** - Database information
3. **TableInfoDto** - Table metadata with size and row count
4. **ViewInfoDto** - View metadata with definition
5. **FunctionInfoDto** - Function metadata with definition
6. **StoredProcedureInfoDto** - Stored procedure metadata
7. **IndexInfoDto** - Index metadata with columns
8. **ForeignKeyInfoDto** - Foreign key relationships

### Service Architecture

**IMetadataService Interface:**
- `GetDatabasesAsync()` - Query sys.databases
- `GetTablesAsync()` - Query sys.tables with size info
- `GetViewsAsync()` - Query sys.views with INFORMATION_SCHEMA
- `GetFunctionsAsync()` - Query sys.objects for functions
- `GetStoredProceduresAsync()` - Query sys.procedures
- `GetIndexesAsync()` - Query sys.indexes with columns
- `GetForeignKeysAsync()` - Query sys.foreign_keys with details

**MetadataService Implementation:**
- Uses Dapper for high-performance queries
- Leverages ConnectionService for connection string building
- Comprehensive error handling and logging
- Async/await throughout

### SQL Queries Used

#### Databases Query
```sql
SELECT 
    name AS DatabaseName,
    database_id AS DatabaseId,
    create_date AS CreateDate,
    compatibility_level AS CompatibilityLevel,
    collation_name AS CollationName,
    recovery_model_desc AS RecoveryModel,
    state_desc AS State
FROM sys.databases
WHERE database_id > 4 -- Exclude system databases
ORDER BY name
```

#### Tables Query
```sql
SELECT 
    s.name AS SchemaName,
    t.name AS TableName,
    t.type_desc AS TableType,
    p.rows AS RowCount,
    SUM(a.total_pages) * 8 AS TotalSpaceKB,
    SUM(a.used_pages) * 8 AS UsedSpaceKB,
    t.create_date AS CreateDate,
    t.modify_date AS ModifyDate
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
LEFT JOIN sys.indexes i ON t.object_id = i.object_id AND i.index_id < 2
LEFT JOIN sys.partitions p ON i.object_id = p.object_id AND i.index_id = p.index_id
LEFT JOIN sys.allocation_units a ON p.partition_id = a.container_id
WHERE t.is_ms_shipped = 0
GROUP BY s.name, t.name, t.type_desc, p.rows, t.create_date, t.modify_date
ORDER BY s.name, t.name
```

#### Views Query
```sql
SELECT 
    s.name AS SchemaName,
    v.name AS ViewName,
    m.definition AS Definition,
    iv.IS_UPDATABLE AS IsUpdatable,
    iv.CHECK_OPTION AS CheckOption,
    v.create_date AS CreateDate,
    v.modify_date AS ModifyDate
FROM sys.views v
INNER JOIN sys.schemas s ON v.schema_id = s.schema_id
LEFT JOIN sys.sql_modules m ON v.object_id = m.object_id
LEFT JOIN INFORMATION_SCHEMA.VIEWS iv 
    ON s.name = iv.TABLE_SCHEMA AND v.name = iv.TABLE_NAME
WHERE v.is_ms_shipped = 0
ORDER BY s.name, v.name
```

#### Indexes Query (with column details)
```sql
SELECT 
    s.name AS SchemaName,
    t.name AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.is_unique AS IsUnique,
    i.is_primary_key AS IsPrimaryKey,
    i.is_unique_constraint AS IsUniqueConstraint,
    STUFF((
        SELECT ', ' + c.name + CASE WHEN ic.is_descending_key = 1 THEN ' DESC' ELSE ' ASC' END
        FROM sys.index_columns ic
        INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
        WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 0
        ORDER BY ic.key_ordinal
        FOR XML PATH('')
    ), 1, 2, '') AS IndexedColumns,
    STUFF((
        SELECT ', ' + c.name
        FROM sys.index_columns ic
        INNER JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
        WHERE ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.is_included_column = 1
        ORDER BY ic.index_column_id
        FOR XML PATH('')
    ), 1, 2, '') AS IncludedColumns,
    i.filter_definition AS FilterDefinition
FROM sys.indexes i
INNER JOIN sys.tables t ON i.object_id = t.object_id
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE i.type > 0 AND t.is_ms_shipped = 0
ORDER BY s.name, t.name, i.name
```

## Usage Examples

### Example 1: Get All Databases

```bash
curl -X POST "https://localhost:7xxx/api/metadata/databases" \
  -H "Content-Type: application/json" \
  -d '{
    "server": "localhost",
    "database": "master",
    "username": "sa",
    "password": "YourPassword123",
    "trustServerCertificate": true
  }'
```

### Example 2: Get Tables in a Database

```bash
curl -X POST "https://localhost:7xxx/api/metadata/tables" \
  -H "Content-Type: application/json" \
  -d '{
    "server": "localhost",
    "database": "MyDatabase",
    "trustServerCertificate": true
  }'
```

### Example 3: Get Indexes

```bash
curl -X POST "https://localhost:7xxx/api/metadata/indexes" \
  -H "Content-Type: application/json" \
  -d '{
    "server": "localhost",
    "database": "MyDatabase",
    "username": "sa",
    "password": "YourPassword123",
    "trustServerCertificate": true
  }'
```

### Example 4: Using C# Client

```csharp
using System.Net.Http.Json;

var client = new HttpClient { BaseAddress = new Uri("https://localhost:7xxx") };

var request = new MetadataRequestDto
{
    Server = "localhost",
    Database = "MyDatabase",
    Username = "sa",
    Password = "YourPassword123",
    TrustServerCertificate = true
};

// Get tables
var tablesResponse = await client.PostAsJsonAsync("/api/metadata/tables", request);
var tables = await tablesResponse.Content.ReadFromJsonAsync<List<TableInfoDto>>();

foreach (var table in tables)
{
    Console.WriteLine($"{table.SchemaName}.{table.TableName} - {table.RowCount} rows");
}

// Get indexes
var indexesResponse = await client.PostAsJsonAsync("/api/metadata/indexes", request);
var indexes = await indexesResponse.Content.ReadFromJsonAsync<List<IndexInfoDto>>();

foreach (var index in indexes)
{
    Console.WriteLine($"{index.TableName}.{index.IndexName} ({index.IndexType})");
}
```

## Use Cases

### 1. Database Documentation Generator
Query all metadata to generate comprehensive database documentation.

```csharp
var tables = await metadataService.GetTablesAsync(request);
var views = await metadataService.GetViewsAsync(request);
var procedures = await metadataService.GetStoredProceduresAsync(request);
var foreignKeys = await metadataService.GetForeignKeysAsync(request);

// Generate markdown documentation
GenerateDocumentation(tables, views, procedures, foreignKeys);
```

### 2. Database Schema Comparison
Compare schemas between different environments.

```csharp
var devRequest = new MetadataRequestDto { Server = "dev-server", Database = "MyDb" };
var prodRequest = new MetadataRequestDto { Server = "prod-server", Database = "MyDb" };

var devTables = await metadataService.GetTablesAsync(devRequest);
var prodTables = await metadataService.GetTablesAsync(prodRequest);

var missingInProd = devTables.Where(t => !prodTables.Any(p => p.TableName == t.TableName));
```

### 3. Index Analysis
Identify missing or redundant indexes.

```csharp
var indexes = await metadataService.GetIndexesAsync(request);
var tables = await metadataService.GetTablesAsync(request);

// Find tables without indexes
var tablesWithoutIndexes = tables
    .Where(t => !indexes.Any(i => i.TableName == t.TableName))
    .ToList();

// Find duplicate indexes
var duplicateIndexes = indexes
    .GroupBy(i => new { i.TableName, i.IndexedColumns })
    .Where(g => g.Count() > 1);
```

### 4. Database Migration Planning
Analyze foreign key dependencies for migration order.

```csharp
var foreignKeys = await metadataService.GetForeignKeysAsync(request);

// Build dependency graph
var dependencyGraph = foreignKeys
    .GroupBy(fk => fk.TableName)
    .ToDictionary(
        g => g.Key,
        g => g.Select(fk => fk.ReferencedTableName).ToList()
    );

// Determine migration order
var migrationOrder = TopologicalSort(dependencyGraph);
```

### 5. Performance Monitoring
Track database growth and index usage.

```csharp
var tables = await metadataService.GetTablesAsync(request);

// Identify large tables
var largeTables = tables
    .Where(t => t.TotalSpaceKB > 1024 * 100) // > 100 MB
    .OrderByDescending(t => t.TotalSpaceKB)
    .ToList();

// Tables with many rows but little space (compression candidates)
var compressionCandidates = tables
    .Where(t => t.RowCount > 1000000 && t.TotalSpaceKB < 10240)
    .ToList();
```

## Security Considerations

### Connection Details
- ⚠️ Connection details are passed in request body (not URL)
- ⚠️ Always use HTTPS in production
- ⚠️ Consider authentication/authorization for the API
- ⚠️ Implement rate limiting to prevent abuse

### Sensitive Information
- ⚠️ View/function/procedure definitions may contain sensitive logic
- ⚠️ Consider filtering or masking sensitive metadata
- ⚠️ Audit metadata access for compliance

### Best Practices
1. Use dedicated read-only accounts for metadata queries
2. Implement caching to reduce database load
3. Add pagination for large result sets
4. Log all metadata access for audit trails
5. Consider IP whitelisting for sensitive environments

## Performance Optimization

### Caching Strategy
Consider caching metadata results as they change infrequently:

```csharp
public class CachedMetadataService : IMetadataService
{
    private readonly IMetadataService _innerService;
    private readonly IMemoryCache _cache;

    public async Task<IEnumerable<TableInfoDto>> GetTablesAsync(
        MetadataRequestDto request, 
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"tables_{request.Server}_{request.Database}";
        
        if (!_cache.TryGetValue(cacheKey, out IEnumerable<TableInfoDto> tables))
        {
            tables = await _innerService.GetTablesAsync(request, cancellationToken);
            
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
                
            _cache.Set(cacheKey, tables, cacheOptions);
        }
        
        return tables;
    }
}
```

### Query Optimization
The queries use:
- Efficient sys catalog views
- Proper filtering (is_ms_shipped = 0)
- Indexes on system tables
- Minimal joins

### Pagination
For large databases, consider adding pagination:

```csharp
public class PaginatedMetadataRequestDto : MetadataRequestDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}
```

## Troubleshooting

### Issue: "Invalid object name 'sys.tables'"
**Solution**: Ensure you're connected to the correct database. System views exist in all databases.

### Issue: "SELECT permission denied on object 'sys.databases'"
**Solution**: User needs VIEW ANY DATABASE permission to query sys.databases.

### Issue: "Empty definitions returned for procedures/views"
**Solution**: User needs VIEW DEFINITION permission to see object definitions.

### Issue: "Timeout errors on large databases"
**Solution**: Add pagination or increase command timeout in connection string.

## Required Permissions

To use all metadata endpoints, the SQL user needs:

```sql
-- View database metadata
GRANT VIEW ANY DATABASE TO [username];

-- View object definitions
GRANT VIEW DEFINITION ON DATABASE::MyDatabase TO [username];

-- View server-level metadata
GRANT VIEW ANY DEFINITION TO [username];

-- Or use db_datareader for read-only access
ALTER ROLE db_datareader ADD MEMBER [username];
```

## Testing in Swagger

1. Navigate to `https://localhost:7xxx`
2. Find the **Metadata** section
3. Try **POST /api/metadata/tables**
4. Click "Try it out"
5. Enter connection details:
```json
{
  "server": "localhost",
  "database": "MyDatabase",
  "trustServerCertificate": true
}
```
6. Click "Execute"

## Files Created

### DTOs (8 files)
- MetadataRequestDto.cs
- DatabaseInfoDto.cs
- TableInfoDto.cs
- ViewInfoDto.cs
- FunctionInfoDto.cs
- StoredProcedureInfoDto.cs
- IndexInfoDto.cs
- ForeignKeyInfoDto.cs

### Services (2 files)
- IMetadataService.cs
- MetadataService.cs

### Controllers (1 file)
- MetadataController.cs

### Validators (1 file)
- MetadataRequestDtoValidator.cs

### Configuration
- Updated DependencyInjection.cs

## Summary

The Metadata API provides comprehensive access to SQL Server metadata using:
- ✅ 7 endpoints for different metadata types
- ✅ Strongly typed DTOs
- ✅ Dapper for performance
- ✅ Async/await throughout
- ✅ INFORMATION_SCHEMA and sys catalog views
- ✅ FluentValidation
- ✅ Comprehensive logging
- ✅ Error handling
- ✅ XML documentation

**Total Lines of Code:** ~1,500+
**Endpoints:** 7
**DTOs:** 8
**Services:** 1
**Build Status:** ✅ Success
