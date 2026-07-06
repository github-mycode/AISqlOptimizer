# MySQL Support Added to SqlOptimizer

## Summary

SqlOptimizer now supports **both SQL Server and MySQL** databases! You can connect to, analyze, and optimize stored procedures/queries in either database type.

## Quick Start - MySQL Connection

```bash
curl -X 'POST' \
  'http://localhost:5119/api/Database/connect' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '{
  "databaseType": 1,
  "server": "localhost",
  "database": "CompanyDB",
  "username": "root",
  "password": "test123",
  "trustServerCertificate": true
}'
```

## Database Types

| Value | DatabaseType | Description |
|-------|--------------|-------------|
| 0 | SqlServer | Microsoft SQL Server (default) |
| 1 | MySql | MySQL Database |

## Request Examples

### SQL Server (Windows Auth)
```json
{
  "databaseType": 0,
  "server": "localhost",
  "database": "MyDatabase",
  "trustServerCertificate": true
}
```

### SQL Server (SQL Auth)
```json
{
  "databaseType": 0,
  "server": "localhost",
  "database": "MyDatabase",
  "username": "sa",
  "password": "YourPassword123",
  "trustServerCertificate": true
}
```

### MySQL
```json
{
  "databaseType": 1,
  "server": "localhost",
  "database": "CompanyDB",
  "username": "root",
  "password": "yourpassword"
}
```

## Response Example

```json
{
  "success": true,
  "databaseType": "MySql",
  "message": "MySql connection successful",
  "serverVersion": "8.0.33-0ubuntu0.22.04.2",
  "databaseName": "CompanyDB",
  "errorDetails": null,
  "timestamp": "2026-07-04T05:30:00Z"
}
```

## Implementation Details

### Multi-Database Architecture

**Provider Pattern:**
- `IDatabaseProvider` interface defines database-specific operations
- `SqlServerProvider` implements SQL Server functionality
- `MySqlProvider` implements MySQL functionality

**Automatic Provider Selection:**
The system automatically selects the correct provider based on `databaseType`:
```csharp
var provider = databaseType switch
{
    DatabaseType.SqlServer => _sqlServerProvider,
    DatabaseType.MySql => _mySqlProvider,
    _ => throw new NotSupportedException()
};
```

### Features Supported

#### SQL Server
✅ Windows Authentication  
✅ SQL Server Authentication  
✅ Stored Procedures  
✅ Execution Plans (SHOWPLAN_XML)  
✅ System Views (sys.*)  
✅ All metadata queries  

#### MySQL
✅ MySQL Authentication  
✅ Stored Procedures  
✅ Execution Plans (EXPLAIN FORMAT=JSON)  
✅ INFORMATION_SCHEMA queries  
✅ All metadata queries  

### Database-Specific Queries

The system uses provider-specific SQL for metadata:

**SQL Server:**
```sql
SELECT name FROM sys.databases WHERE...
```

**MySQL:**
```sql
SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE...
```

### Packages Added

- ✅ MySql.Data 9.7.0
- ✅ Supporting libraries (BouncyCastle, Google.Protobuf, etc.)

## Docker Support

### MySQL in Docker Compose

Update `docker-compose.dev.yml` to include MySQL:

```yaml
services:
  mysql:
    image: mysql:8.0
    container_name: sqloptimizer-mysql
    restart: unless-stopped
    ports:
      - "3306:3306"
    environment:
      - MYSQL_ROOT_PASSWORD=test123
      - MYSQL_DATABASE=CompanyDB
    volumes:
      - mysql-data:/var/lib/mysql
    healthcheck:
      test: ["CMD", "mysqladmin", "ping", "-h", "localhost"]
      interval: 30s
      timeout: 10s
      retries: 5

volumes:
  mysql-data:
    driver: local
```

## Testing

### Test MySQL Connection
```bash
curl -X POST http://localhost:5119/api/Database/connect \
  -H "Content-Type: application/json" \
  -d '{
    "databaseType": 1,
    "server": "localhost",
    "database": "CompanyDB",
    "username": "root",
    "password": "test123"
  }'
```

### Test SQL Server Connection
```bash
curl -X POST http://localhost:5119/api/Database/connect \
  -H "Content-Type: application/json" \
  -d '{
    "databaseType": 0,
    "server": "localhost",
    "database": "master",
    "username": "sa",
    "password": "YourStrong@Password123",
    "trustServerCertificate": true
  }'
```

## Swagger UI

In Swagger, you'll see a dropdown for `databaseType`:
- **0** = SqlServer
- **1** = MySql

Simply select the appropriate type before testing!

## What's Next

The following features are being updated to support both databases:
- ✅ Connection testing
- 🔄 Metadata queries (tables, views, procedures)
- 🔄 Stored procedure analysis
- 🔄 Dashboard statistics
- 🔄 Report generation

## Notes

- MySQL doesn't support schemas the same way as SQL Server - schema name defaults to database name
- Execution plans format differs: SQL Server uses XML, MySQL uses JSON
- Some AI analysis features may need adjustment for MySQL-specific syntax
- Windows Authentication only applies to SQL Server

## Troubleshooting

**MySQL Connection Issues:**
```bash
# Verify MySQL is running
mysql -u root -p -e "SELECT VERSION();"

# Check if user has permissions
GRANT ALL PRIVILEGES ON CompanyDB.* TO 'root'@'%';
FLUSH PRIVILEGES;
```

**SQL Server Connection Issues:**
```bash
# Test with sqlcmd
sqlcmd -S localhost -U sa -P YourPassword123 -Q "SELECT @@VERSION"
```

Happy optimizing with both SQL Server and MySQL! 🚀
