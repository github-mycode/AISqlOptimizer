# Project Structure

```
SqlOptimizer/
│
├── SqlOptimizer.sln                          # Solution file
├── README.md                                  # Comprehensive documentation
├── QUICKSTART.md                              # Quick start guide
├── .gitignore                                 # Git ignore rules
│
├── SqlOptimizer.Api/                          # 🌐 API Layer (Presentation)
│   ├── Controllers/
│   │   └── SqlQueriesController.cs           # REST API endpoints
│   ├── Middleware/
│   │   └── ExceptionHandlingMiddleware.cs    # Global error handling
│   ├── Properties/
│   │   └── launchSettings.json               # Launch configurations
│   ├── appsettings.json                       # Production configuration
│   ├── appsettings.Development.json           # Development configuration
│   ├── Program.cs                             # Application entry point
│   └── SqlOptimizer.Api.csproj               # Project file
│
├── SqlOptimizer.Application/                  # 💼 Application Layer
│   ├── DTOs/
│   │   ├── CreateSqlQueryDto.cs              # Create operation DTO
│   │   ├── UpdateSqlQueryDto.cs              # Update operation DTO
│   │   └── SqlQueryDto.cs                     # Response DTO
│   ├── Interfaces/
│   │   └── ISqlQueryService.cs               # Service contract
│   ├── Services/
│   │   └── SqlQueryService.cs                # Business logic implementation
│   ├── Validators/
│   │   ├── CreateSqlQueryDtoValidator.cs     # Create validation rules
│   │   └── UpdateSqlQueryDtoValidator.cs     # Update validation rules
│   ├── DependencyInjection.cs                # Service registration
│   └── SqlOptimizer.Application.csproj       # Project file
│
├── SqlOptimizer.Domain/                       # 🎯 Domain Layer (Core)
│   ├── Common/
│   │   └── BaseEntity.cs                      # Base entity class
│   ├── Entities/
│   │   └── SqlQuery.cs                        # SqlQuery entity
│   ├── Interfaces/
│   │   ├── IRepository.cs                     # Generic repository interface
│   │   └── ISqlQueryRepository.cs            # SqlQuery repository interface
│   └── SqlOptimizer.Domain.csproj            # Project file
│
└── SqlOptimizer.Infrastructure/               # 🔧 Infrastructure Layer
    ├── Data/
    │   └── SqlConnectionFactory.cs           # Database connection factory
    ├── Options/
    │   └── DatabaseOptions.cs                # Configuration options
    ├── Repositories/
    │   ├── BaseRepository.cs                 # Generic repository with Dapper
    │   └── SqlQueryRepository.cs             # SqlQuery repository with Dapper
    ├── Scripts/
    │   └── CreateTables.sql                  # Database schema script
    ├── DependencyInjection.cs                # Service registration
    └── SqlOptimizer.Infrastructure.csproj    # Project file
```

## Layer Dependencies

```
┌─────────────────────────────────────────┐
│         SqlOptimizer.Api                │  ← Entry Point
│    (Controllers, Middleware, Config)    │
└────────────┬────────────────────────────┘
             │ References
             ↓
┌────────────────────────────────────────┐
│    SqlOptimizer.Application            │
│   (Services, DTOs, Validators)         │
└────────────┬───────────────────────────┘
             │ References
             ↓
┌────────────────────────────────────────┐
│       SqlOptimizer.Domain              │  ← Core
│  (Entities, Interfaces, Rules)         │     (No dependencies)
└────────────┬───────────────────────────┘
             ↑
             │ References
┌────────────┴───────────────────────────┐
│    SqlOptimizer.Infrastructure         │
│  (Repositories, Data Access, Dapper)   │
└────────────────────────────────────────┘
```

## Data Flow

```
HTTP Request
     │
     ↓
┌─────────────────┐
│   Controller    │  → Receives request, validates input
└────────┬────────┘
         │
         ↓
┌─────────────────┐
│    Service      │  → Executes business logic
└────────┬────────┘
         │
         ↓
┌─────────────────┐
│   Repository    │  → Performs data operations (Dapper)
└────────┬────────┘
         │
         ↓
┌─────────────────┐
│   SQL Server    │  → Persistent storage
└─────────────────┘
```

## Key Files Description

### API Layer
- **SqlQueriesController.cs**: RESTful endpoints for CRUD operations
- **ExceptionHandlingMiddleware.cs**: Catches and formats all exceptions
- **Program.cs**: Configures Serilog, DI, Swagger, health checks, CORS

### Application Layer
- **ISqlQueryService.cs**: Defines business operations contract
- **SqlQueryService.cs**: Implements business logic and DTO mapping
- **Validators**: FluentValidation rules for input validation
- **DTOs**: Data transfer objects for API communication

### Domain Layer
- **BaseEntity.cs**: Common properties (Id, CreatedAt, UpdatedAt, IsDeleted)
- **SqlQuery.cs**: Core business entity
- **IRepository.cs**: Generic data access contract
- **ISqlQueryRepository.cs**: SqlQuery-specific data operations

### Infrastructure Layer
- **SqlConnectionFactory.cs**: Creates SQL Server connections
- **BaseRepository.cs**: Generic Dapper implementation
- **SqlQueryRepository.cs**: SqlQuery-specific Dapper queries
- **CreateTables.sql**: Database schema definition

## NuGet Packages by Project

### SqlOptimizer.Api
- Serilog.AspNetCore
- AspNetCore.HealthChecks.SqlServer
- Microsoft.Extensions.Diagnostics.HealthChecks
- Swashbuckle.AspNetCore
- Microsoft.AspNetCore.OpenApi

### SqlOptimizer.Application
- FluentValidation
- FluentValidation.DependencyInjectionExtensions

### SqlOptimizer.Infrastructure
- Dapper
- Microsoft.Data.SqlClient
- Microsoft.Extensions.Options.ConfigurationExtensions

### SqlOptimizer.Domain
- No external dependencies (by design)

## Clean Architecture Benefits

1. **Testability**: Each layer can be tested independently
2. **Maintainability**: Clear separation of concerns
3. **Flexibility**: Easy to swap implementations (e.g., replace Dapper with EF Core)
4. **Independence**: Domain logic doesn't depend on external frameworks
5. **Scalability**: Easy to extend with new features

## SOLID Principles Applied

- **S**ingle Responsibility: Each class has one reason to change
- **O**pen/Closed: Open for extension (interfaces), closed for modification
- **L**iskov Substitution: Interfaces can be replaced with implementations
- **I**nterface Segregation: Small, focused interfaces
- **D**ependency Inversion: Depend on abstractions, not concretions
