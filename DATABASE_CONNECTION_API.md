# Database Connection API Feature

## Overview
This feature allows users to test connections to any SQL Server instance without storing credentials. It's useful for validating database connectivity before performing operations.

## Implementation Summary

### Files Created

#### Application Layer - DTOs
1. **DatabaseConnectionRequestDto.cs**
   - Input model for connection requests
   - Properties: Server, Database, Username, Password, TrustServerCertificate
   
2. **DatabaseConnectionResponseDto.cs**
   - Response model with connection test results
   - Properties: Success, Message, ServerVersion, DatabaseName, ErrorDetails, Timestamp

#### Application Layer - Services
3. **IConnectionService.cs** (Interface)
   - `TestConnectionAsync()` - Tests database connectivity
   - `BuildConnectionString()` - Builds SQL connection string
   
4. **ConnectionService.cs** (Implementation)
   - Uses Dapper to query server information
   - Handles both SQL and Windows authentication
   - Comprehensive error handling and logging
   - Async/await throughout

#### Application Layer - Validators
5. **DatabaseConnectionRequestDtoValidator.cs**
   - FluentValidation rules for input validation
   - Validates server and database name formats
   - Ensures Username/Password are provided together
   - Prevents SQL injection attempts

#### API Layer - Controllers
6. **DatabaseController.cs**
   - POST /api/database/connect endpoint
   - Validates input using FluentValidation
   - Returns detailed connection test results
   - Comprehensive XML documentation

#### Configuration
7. **Updated DependencyInjection.cs**
   - Registered IConnectionService -> ConnectionService

## API Endpoint

### POST /api/database/connect

Tests a connection to a SQL Server instance.

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

Or for Windows Authentication:

```json
{
  "server": "localhost\\SQLEXPRESS",
  "database": "MyDatabase",
  "trustServerCertificate": true
}
```

**Success Response (200 OK):**

```json
{
  "success": true,
  "message": "Connection successful",
  "serverVersion": "15.0.2000.5",
  "databaseName": "MyDatabase",
  "errorDetails": null,
  "timestamp": "2026-07-04T10:30:00Z"
}
```

**Failure Response (200 OK):**

```json
{
  "success": false,
  "message": "SQL Server connection failed",
  "serverVersion": null,
  "databaseName": null,
  "errorDetails": "Cannot open database \"MyDatabase\" requested by the login. The login failed.",
  "timestamp": "2026-07-04T10:30:00Z"
}
```

**Validation Error Response (400 Bad Request):**

```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    {
      "property": "Server",
      "error": "Server is required"
    },
    {
      "property": "Database",
      "error": "Database is required"
    }
  ]
}
```

## Features

### Security
- ✅ **No Credential Storage**: Credentials are never stored in the database
- ✅ **Input Validation**: Comprehensive validation to prevent SQL injection
- ✅ **Secure Connection Strings**: Built using SqlConnectionStringBuilder
- ✅ **Server Name Validation**: Prevents invalid characters in server names
- ✅ **Database Name Validation**: Prevents invalid characters in database names

### Error Handling
- ✅ **SQL Exception Handling**: Catches and formats SQL-specific errors
- ✅ **General Exception Handling**: Catches unexpected errors
- ✅ **Detailed Error Messages**: Provides helpful error information
- ✅ **Logging**: All connection attempts are logged (without passwords)

### Performance
- ✅ **Async/Await**: Non-blocking operations throughout
- ✅ **Connection Timeout**: 15-second connection timeout
- ✅ **Efficient Dapper Queries**: Lightweight data access
- ✅ **Proper Resource Disposal**: Using statements for connections

### Authentication Support
- ✅ **SQL Authentication**: Username/password authentication
- ✅ **Windows Authentication**: Integrated security support
- ✅ **Flexible Configuration**: TrustServerCertificate option

## Usage Examples

### Using Swagger UI

1. Navigate to `https://localhost:7xxx`
2. Find **Database** section
3. Expand **POST /api/database/connect**
4. Click "Try it out"
5. Enter connection details
6. Click "Execute"

### Using cURL

**SQL Authentication:**
```bash
curl -X POST "https://localhost:7xxx/api/database/connect" \
  -H "Content-Type: application/json" \
  -d '{
    "server": "localhost",
    "database": "MyDatabase",
    "username": "sa",
    "password": "YourPassword123",
    "trustServerCertificate": true
  }'
```

**Windows Authentication:**
```bash
curl -X POST "https://localhost:7xxx/api/database/connect" \
  -H "Content-Type: application/json" \
  -d '{
    "server": "localhost\\SQLEXPRESS",
    "database": "MyDatabase",
    "trustServerCertificate": true
  }'
```

### Using C# Client

```csharp
using System.Net.Http.Json;

var client = new HttpClient { BaseAddress = new Uri("https://localhost:7xxx") };

var request = new DatabaseConnectionRequestDto
{
    Server = "localhost",
    Database = "MyDatabase",
    Username = "sa",
    Password = "YourPassword123",
    TrustServerCertificate = true
};

var response = await client.PostAsJsonAsync("/api/database/connect", request);
var result = await response.Content.ReadFromJsonAsync<DatabaseConnectionResponseDto>();

if (result.Success)
{
    Console.WriteLine($"Connected! Server version: {result.ServerVersion}");
}
else
{
    Console.WriteLine($"Connection failed: {result.ErrorDetails}");
}
```

## Validation Rules

### Server Name
- **Required**: Yes
- **Max Length**: 255 characters
- **Invalid Characters**: `;`, `'`, `"`, `<`, `>`, `|`, `*`, `?`
- **Examples**: 
  - ✅ `localhost`
  - ✅ `.\SQLEXPRESS`
  - ✅ `server.domain.com`
  - ✅ `192.168.1.1,1433`
  - ❌ `server;DROP TABLE--`

### Database Name
- **Required**: Yes
- **Max Length**: 128 characters
- **Invalid Characters**: `;`, `'`, `"`, `<`, `>`, `|`, `*`, `?`, `/`, `\`, null character
- **Examples**:
  - ✅ `MyDatabase`
  - ✅ `Sales_2024`
  - ✅ `Customer-DB`
  - ❌ `Database'; DROP TABLE--`

### Username
- **Required**: Only when using SQL Authentication
- **Max Length**: 128 characters
- **Note**: If provided, Password must also be provided

### Password
- **Required**: Only when using SQL Authentication
- **Note**: If provided, Username must also be provided

### TrustServerCertificate
- **Type**: Boolean
- **Default**: true
- **Note**: Set to true for local development, false for production with valid certificates

## Connection String Building

The service builds connection strings using `SqlConnectionStringBuilder`:

**SQL Authentication:**
```
Data Source=localhost;Initial Catalog=MyDatabase;User ID=sa;Password=YourPassword123;TrustServerCertificate=True;Connect Timeout=15
```

**Windows Authentication:**
```
Data Source=localhost\SQLEXPRESS;Initial Catalog=MyDatabase;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=15
```

## Logging

All connection attempts are logged with appropriate levels:

**Information:**
- Connection test started
- Connection successful

**Warning:**
- Validation failures
- Connection failures

**Error:**
- SQL exceptions
- Unexpected errors

**Example Logs:**
```
[2026-07-04 10:30:00 INF] Received connection test request for server: localhost, database: MyDatabase
[2026-07-04 10:30:01 INF] Testing connection to server: localhost, database: MyDatabase
[2026-07-04 10:30:01 INF] Successfully connected to localhost/MyDatabase. Version: 15.0.2000.5
[2026-07-04 10:30:01 INF] Connection test successful for localhost/MyDatabase
```

## Common Connection Scenarios

### Scenario 1: LocalDB
```json
{
  "server": "(localdb)\\mssqllocaldb",
  "database": "MyDatabase",
  "trustServerCertificate": true
}
```

### Scenario 2: SQL Express with Windows Auth
```json
{
  "server": "localhost\\SQLEXPRESS",
  "database": "MyDatabase",
  "trustServerCertificate": true
}
```

### Scenario 3: Remote Server with SQL Auth
```json
{
  "server": "192.168.1.100,1433",
  "database": "ProductionDB",
  "username": "app_user",
  "password": "SecurePassword123!",
  "trustServerCertificate": false
}
```

### Scenario 4: Azure SQL Database
```json
{
  "server": "myserver.database.windows.net",
  "database": "MyAzureDB",
  "username": "admin@myserver",
  "password": "AzurePassword123!",
  "trustServerCertificate": true
}
```

## Testing

### Test Cases

1. **Valid SQL Authentication**
   - Provide valid server, database, username, password
   - Expect: `success: true`

2. **Valid Windows Authentication**
   - Provide valid server, database (no username/password)
   - Expect: `success: true`

3. **Invalid Credentials**
   - Provide invalid username or password
   - Expect: `success: false`, error message

4. **Non-existent Database**
   - Provide valid server but invalid database
   - Expect: `success: false`, error message

5. **Non-existent Server**
   - Provide invalid server address
   - Expect: `success: false`, timeout or connection error

6. **Missing Required Fields**
   - Omit server or database
   - Expect: `400 Bad Request`, validation errors

7. **Invalid Characters**
   - Use SQL injection attempts in server name
   - Expect: `400 Bad Request`, validation error

## Architecture Benefits

### Clean Architecture Compliance
- ✅ **Domain Independence**: No dependencies on infrastructure
- ✅ **Application Logic**: Service layer handles business logic
- ✅ **Infrastructure Abstraction**: Connection logic isolated
- ✅ **API Presentation**: Controller handles HTTP concerns

### SOLID Principles
- ✅ **Single Responsibility**: Each class has one clear purpose
- ✅ **Open/Closed**: Extensible through interfaces
- ✅ **Liskov Substitution**: Interface implementations are interchangeable
- ✅ **Interface Segregation**: Small, focused interfaces
- ✅ **Dependency Inversion**: Depends on abstractions (IConnectionService)

## Future Enhancements

Possible improvements for future versions:

1. **Connection Pooling**: Track and manage connection pools
2. **Connection History**: Store connection test results (without credentials)
3. **Batch Testing**: Test multiple connections simultaneously
4. **Query Testing**: Execute test queries to verify permissions
5. **Performance Metrics**: Measure connection time
6. **SSL/TLS Validation**: Enhanced certificate validation
7. **Connection Monitoring**: Real-time connection health checks
8. **Rate Limiting**: Prevent connection spam attacks

## Security Considerations

### Important Notes
- ⚠️ **Never store plaintext passwords** in logs or database
- ⚠️ **Always validate input** to prevent SQL injection
- ⚠️ **Use HTTPS** in production to encrypt credentials in transit
- ⚠️ **Consider authentication** to restrict API access
- ⚠️ **Implement rate limiting** to prevent brute force attacks
- ⚠️ **Monitor failed attempts** for security threats

### Production Recommendations
1. Add authentication/authorization to the endpoint
2. Implement rate limiting (e.g., 10 attempts per minute)
3. Use HTTPS only
4. Set `TrustServerCertificate` to false with valid certificates
5. Add audit logging for connection attempts
6. Consider IP whitelisting for sensitive environments

## Troubleshooting

### Common Issues

**Issue: "Cannot open database"**
- Check database name spelling
- Verify database exists on server
- Check user has access to database

**Issue: "Login failed for user"**
- Verify username and password
- Check SQL Server authentication mode
- Ensure user exists and has login permissions

**Issue: "A network-related or instance-specific error"**
- Verify server address
- Check SQL Server is running
- Ensure port 1433 is open (or specified port)
- Check firewall settings

**Issue: "Validation failed"**
- Review validation error messages
- Ensure all required fields are provided
- Check for invalid characters

## Summary

The Database Connection API provides a secure, production-ready way to test SQL Server connections without storing credentials. It follows Clean Architecture principles, uses async programming throughout, implements comprehensive validation, and provides detailed error handling and logging.

**Key Features:**
- ✅ No credential storage
- ✅ Async/await throughout
- ✅ Dapper for data access
- ✅ FluentValidation for input validation
- ✅ Comprehensive error handling
- ✅ Detailed logging
- ✅ SQL and Windows authentication support
- ✅ XML documentation
- ✅ Clean Architecture
- ✅ SOLID principles
