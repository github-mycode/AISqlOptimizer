# Quick Start Guide

## Prerequisites
- .NET 8 SDK installed
- SQL Server (LocalDB, Express, or full version)
- Your favorite code editor (VS Code or Visual Studio 2022)

## Step-by-Step Setup

### 1. Create the Database

Open SQL Server Management Studio or Azure Data Studio and run:

```sql
CREATE DATABASE SqlOptimizerDb;
GO

USE SqlOptimizerDb;
GO

-- Run the script from SqlOptimizer.Infrastructure/Scripts/CreateTables.sql
CREATE TABLE SqlQueries (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,
    QueryText NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(1000) NULL,
    DatabaseName NVARCHAR(100) NOT NULL,
    ExecutionTimeMs DECIMAL(18,2) NULL,
    OptimizedQueryText NVARCHAR(MAX) NULL,
    OptimizationNotes NVARCHAR(2000) NULL,
    IsOptimized BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NULL,
    IsDeleted BIT NOT NULL DEFAULT 0
);

CREATE INDEX IX_SqlQueries_DatabaseName ON SqlQueries(DatabaseName) WHERE IsDeleted = 0;
CREATE INDEX IX_SqlQueries_IsOptimized ON SqlQueries(IsOptimized) WHERE IsDeleted = 0;
CREATE INDEX IX_SqlQueries_CreatedAt ON SqlQueries(CreatedAt DESC) WHERE IsDeleted = 0;
```

### 2. Update Connection String

Edit `SqlOptimizer.Api/appsettings.json` and update the connection string:

```json
{
  "Database": {
    "ConnectionString": "Server=localhost;Database=SqlOptimizerDb;Trusted_Connection=True;TrustServerCertificate=True;",
    "CommandTimeout": 30
  }
}
```

Common connection strings:
- **LocalDB**: `Server=(localdb)\\mssqllocaldb;Database=SqlOptimizerDb;Trusted_Connection=True;`
- **Express**: `Server=localhost\\SQLEXPRESS;Database=SqlOptimizerDb;Trusted_Connection=True;TrustServerCertificate=True;`
- **SQL Authentication**: `Server=localhost;Database=SqlOptimizerDb;User Id=youruser;Password=yourpassword;TrustServerCertificate=True;`

### 3. Run the Application

```bash
cd SqlOptimizer.Api
dotnet run
```

You should see output like:
```
[2026-07-04 10:30:00 INF] Starting SqlOptimizer API
[2026-07-04 10:30:01 INF] SqlOptimizer API started successfully
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7xxx
```

### 4. Open Swagger UI

Navigate to: `https://localhost:7xxx`

You'll see the interactive API documentation.

## Testing the API

### Create a Query

1. In Swagger, expand **POST /api/sqlqueries**
2. Click "Try it out"
3. Replace the example JSON with:

```json
{
  "name": "Get All Users",
  "queryText": "SELECT * FROM Users WHERE IsActive = 1",
  "description": "Retrieves all active users",
  "databaseName": "MyDatabase"
}
```

4. Click "Execute"
5. You should get a 201 Created response with the created query

### Get All Queries

1. Expand **GET /api/sqlqueries**
2. Click "Try it out"
3. Click "Execute"
4. You should see a list containing the query you just created

### Update a Query (Mark as Optimized)

1. Expand **PUT /api/sqlqueries/{id}**
2. Click "Try it out"
3. Enter the ID of the query you created (e.g., 1)
4. Use this JSON:

```json
{
  "optimizedQueryText": "SELECT UserId, FirstName, LastName, Email FROM Users WHERE IsActive = 1",
  "optimizationNotes": "Removed * and selected specific columns to improve performance",
  "isOptimized": true,
  "executionTimeMs": 15.5
}
```

5. Click "Execute"
6. You should get a 204 No Content response

## Health Check

Navigate to: `https://localhost:7xxx/health`

You should see:
```json
{
  "status": "Healthy"
}
```

## Logs

Check the `logs` folder in the `SqlOptimizer.Api` directory for detailed logs:
```
logs/log-20260704.txt
```

## Troubleshooting

### Cannot connect to database
- Verify SQL Server is running
- Check connection string is correct
- Ensure database exists
- Test connection using SQL Server Management Studio

### Port already in use
Edit `SqlOptimizer.Api/Properties/launchSettings.json` to use different ports

### Missing logs folder
The folder is created automatically on first run, but you can create it manually:
```bash
mkdir SqlOptimizer.Api/logs
```

## Next Steps

- Explore all API endpoints in Swagger
- Implement authentication and authorization
- Add more entities and features
- Write unit and integration tests
- Deploy to Azure or your preferred cloud platform

## Sample Queries to Try

```json
{
  "name": "Get Top 10 Orders",
  "queryText": "SELECT TOP 10 * FROM Orders ORDER BY OrderDate DESC",
  "description": "Get most recent orders",
  "databaseName": "SalesDB"
}
```

```json
{
  "name": "Customer Count by Region",
  "queryText": "SELECT Region, COUNT(*) as CustomerCount FROM Customers GROUP BY Region",
  "description": "Count customers by region",
  "databaseName": "CustomerDB"
}
```

Enjoy using SqlOptimizer API! 🚀
