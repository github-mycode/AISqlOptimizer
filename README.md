# SqlOptimizer API

A production-ready ASP.NET Core 8 Web API for SQL query optimization built with Clean Architecture principles.

## 🏗️ Architecture

This solution follows **Clean Architecture** with clear separation of concerns across four projects:

```
SqlOptimizer/
├── SqlOptimizer.Api/              # API Layer (Presentation)
├── SqlOptimizer.Application/      # Application Layer (Business Logic)
├── SqlOptimizer.Domain/           # Domain Layer (Entities & Interfaces)
└── SqlOptimizer.Infrastructure/   # Infrastructure Layer (Data Access)
```

## 🚀 Features

- ✅ **ASP.NET Core 8** - Latest .NET framework
- ✅ **Clean Architecture** - Maintainable and testable code structure
- ✅ **SQL Server** - Robust database support
- ✅ **Dapper** - Lightweight ORM for high-performance data access
- ✅ **Swagger/OpenAPI** - Interactive API documentation
- ✅ **Dependency Injection** - Built-in DI container
- ✅ **Options Pattern** - Strongly-typed configuration
- ✅ **Global Exception Middleware** - Centralized error handling
- ✅ **Serilog** - Structured logging to console and file
- ✅ **Health Checks** - SQL Server health monitoring
- ✅ **FluentValidation** - Input validation
- ✅ **Async Programming** - Non-blocking operations
- ✅ **SOLID Principles** - Clean code practices
- ✅ **Repository Pattern** - Data access abstraction
- ✅ **Service Layer** - Business logic separation
- ✅ **XML Documentation** - Code documentation

## 📋 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server) (Express or higher)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

## 🔧 Setup Instructions

### 1. Clone/Navigate to the Repository

```bash
cd c:\AI\SqlOptimizer
```

### 2. Configure Database Connection

Update the connection string in `SqlOptimizer.Api/appsettings.json`:

```json
{
  "Database": {
    "ConnectionString": "Server=YOUR_SERVER;Database=SqlOptimizerDb;Trusted_Connection=True;TrustServerCertificate=True;",
    "CommandTimeout": 30
  }
}
```

### 3. Create Database

Run the SQL script located at `SqlOptimizer.Infrastructure/Scripts/CreateTables.sql` in SQL Server Management Studio or Azure Data Studio to create the required tables.

```sql
-- Create the database first
CREATE DATABASE SqlOptimizerDb;
GO

USE SqlOptimizerDb;
GO

-- Then run the CreateTables.sql script
```

### 4. Restore NuGet Packages

```bash
dotnet restore
```

### 5. Build the Solution

```bash
dotnet build
```

### 6. Run the API

```bash
cd SqlOptimizer.Api
dotnet run
```

The API will start at:
- HTTPS: `https://localhost:7xxx`
- HTTP: `http://localhost:5xxx`

### 7. Access Swagger UI

Open your browser and navigate to the root URL (Swagger is configured at the app's root):
- `https://localhost:7xxx`

## 📁 Project Structure

### SqlOptimizer.Domain
Contains core business entities and interfaces:
- `Common/BaseEntity.cs` - Base entity with common properties
- `Entities/SqlQuery.cs` - SQL query entity
- `Interfaces/IRepository.cs` - Generic repository interface
- `Interfaces/ISqlQueryRepository.cs` - Specific repository interface

### SqlOptimizer.Application
Contains business logic and DTOs:
- `DTOs/` - Data Transfer Objects
- `Interfaces/ISqlQueryService.cs` - Service interface
- `Services/SqlQueryService.cs` - Service implementation
- `Validators/` - FluentValidation validators
- `DependencyInjection.cs` - DI configuration

### SqlOptimizer.Infrastructure
Contains data access implementation:
- `Data/SqlConnectionFactory.cs` - Database connection factory
- `Repositories/BaseRepository.cs` - Generic repository with Dapper
- `Repositories/SqlQueryRepository.cs` - Specific repository implementation
- `Options/DatabaseOptions.cs` - Database configuration options
- `Scripts/CreateTables.sql` - Database schema script
- `DependencyInjection.cs` - DI configuration

### SqlOptimizer.Api
Contains API endpoints and configuration:
- `Controllers/SqlQueriesController.cs` - REST API controller
- `Middleware/ExceptionHandlingMiddleware.cs` - Global error handling
- `Program.cs` - Application entry point and configuration
- `appsettings.json` - Application configuration

## 🔌 API Endpoints

### SQL Queries

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/sqlqueries` | Get all queries |
| GET | `/api/sqlqueries/{id}` | Get query by ID |
| GET | `/api/sqlqueries/database/{databaseName}` | Get queries by database |
| GET | `/api/sqlqueries/optimized` | Get optimized queries |
| GET | `/api/sqlqueries/unoptimized` | Get unoptimized queries |
| POST | `/api/sqlqueries` | Create a new query |
| PUT | `/api/sqlqueries/{id}` | Update a query |
| DELETE | `/api/sqlqueries/{id}` | Delete a query |

### Health Check

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Check API and database health |

## 📝 Configuration

### Database Options (appsettings.json)

```json
{
  "Database": {
    "ConnectionString": "Server=localhost;Database=SqlOptimizerDb;...",
    "CommandTimeout": 30
  }
}
```

### Serilog Configuration (appsettings.json)

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.AspNetCore": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/log-.txt",
          "rollingInterval": "Day"
        }
      }
    ]
  }
}
```

## 🧪 Testing the API

### Using Swagger UI
1. Navigate to `https://localhost:7xxx`
2. Expand any endpoint
3. Click "Try it out"
4. Enter parameters
5. Click "Execute"

### Using curl

Create a query:
```bash
curl -X POST "https://localhost:7xxx/api/sqlqueries" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Get All Users",
    "queryText": "SELECT * FROM Users",
    "databaseName": "MyDatabase",
    "description": "Retrieves all users from the database"
  }'
```

Get all queries:
```bash
curl -X GET "https://localhost:7xxx/api/sqlqueries"
```

## 🏥 Health Checks

The API includes health checks for SQL Server:

```bash
curl -X GET "https://localhost:7xxx/health"
```

Response:
```json
{
  "status": "Healthy",
  "results": {
    "sql-server": {
      "status": "Healthy"
    }
  }
}
```

## 📊 Logging

Logs are written to:
- **Console** - Real-time output during development
- **File** - `logs/log-YYYYMMDD.txt` - Daily rolling files

Log format:
```
[2026-07-04 10:30:45 INF] Getting all SQL queries
[2026-07-04 10:30:45 INF] Created SQL query with ID: 5
```

## 🔐 Best Practices Implemented

### SOLID Principles
- **S**ingle Responsibility - Each class has one reason to change
- **O**pen/Closed - Open for extension, closed for modification
- **L**iskov Substitution - Interfaces can be substituted with implementations
- **I**nterface Segregation - Specific interfaces for different needs
- **D**ependency Inversion - Depend on abstractions, not concretions

### Clean Architecture
- Domain layer has no dependencies
- Application layer depends only on Domain
- Infrastructure depends on Domain and Application
- API depends on Application and Infrastructure
- Data flows inward, dependencies point inward

### Async/Await
- All database operations use async/await
- CancellationToken support for graceful cancellation
- Non-blocking I/O operations

### Repository Pattern
- Generic repository for common operations
- Specific repositories for entity-specific queries
- Abstraction over data access

### Options Pattern
- Strongly-typed configuration
- Validation at startup
- Easy to test and modify

## 🔄 Extending the API

### Adding a New Entity

1. **Create entity in Domain layer**
   ```csharp
   // SqlOptimizer.Domain/Entities/YourEntity.cs
   public class YourEntity : BaseEntity { }
   ```

2. **Create repository interface**
   ```csharp
   // SqlOptimizer.Domain/Interfaces/IYourEntityRepository.cs
   public interface IYourEntityRepository : IRepository<YourEntity> { }
   ```

3. **Create DTOs in Application layer**
   ```csharp
   // SqlOptimizer.Application/DTOs/YourEntityDto.cs
   ```

4. **Create service interface and implementation**
   ```csharp
   // SqlOptimizer.Application/Interfaces/IYourEntityService.cs
   // SqlOptimizer.Application/Services/YourEntityService.cs
   ```

5. **Create validators**
   ```csharp
   // SqlOptimizer.Application/Validators/YourEntityValidator.cs
   ```

6. **Create repository implementation**
   ```csharp
   // SqlOptimizer.Infrastructure/Repositories/YourEntityRepository.cs
   ```

7. **Create controller**
   ```csharp
   // SqlOptimizer.Api/Controllers/YourEntityController.cs
   ```

8. **Register services in DI**
   - Update `SqlOptimizer.Application/DependencyInjection.cs`
   - Update `SqlOptimizer.Infrastructure/DependencyInjection.cs`

## 🛠️ Development

### Building
```bash
dotnet build
```

### Running
```bash
dotnet run --project SqlOptimizer.Api
```

### Publishing
```bash
dotnet publish -c Release -o ./publish
```

## 📦 NuGet Packages

### SqlOptimizer.Api
- Serilog.AspNetCore
- AspNetCore.HealthChecks.SqlServer
- Microsoft.Extensions.Diagnostics.HealthChecks
- Swashbuckle.AspNetCore

### SqlOptimizer.Application
- FluentValidation
- FluentValidation.DependencyInjectionExtensions

### SqlOptimizer.Infrastructure
- Dapper
- Microsoft.Data.SqlClient
- Microsoft.Extensions.Options.ConfigurationExtensions

## 📄 License

This project is licensed under the MIT License.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Open a Pull Request

## 📞 Support

For issues, questions, or contributions, please open an issue on GitHub.

---

**Built with ❤️ using ASP.NET Core 8 and Clean Architecture**
