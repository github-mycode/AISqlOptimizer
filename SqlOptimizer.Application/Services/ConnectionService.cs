using Dapper;
using Microsoft.Extensions.Logging;
using SqlOptimizer.Application.DTOs;
using SqlOptimizer.Application.Interfaces;
using SqlOptimizer.Domain.Enums;
using System.Data;

namespace SqlOptimizer.Application.Services;

/// <summary>
/// Service implementation for database connection operations
/// </summary>
public class ConnectionService : IConnectionService
{
    private readonly IDatabaseProvider _sqlServerProvider;
    private readonly IDatabaseProvider _mySqlProvider;
    private readonly ILogger<ConnectionService> _logger;

    public ConnectionService(
        IEnumerable<IDatabaseProvider> providers,
        ILogger<ConnectionService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        var providerList = providers.ToList();
        _sqlServerProvider = providerList.FirstOrDefault(p => p.GetType().Name.Contains("SqlServer"))
            ?? throw new InvalidOperationException("SqlServerProvider not registered");
        _mySqlProvider = providerList.FirstOrDefault(p => p.GetType().Name.Contains("MySql"))
            ?? throw new InvalidOperationException("MySqlProvider not registered");
    }

    /// <inheritdoc/>
    public async Task<DatabaseConnectionResponseDto> TestConnectionAsync(
        DatabaseConnectionRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var response = new DatabaseConnectionResponseDto
        {
            DatabaseType = request.DatabaseType.ToString()
        };

        try
        {
            _logger.LogInformation(
                "Testing {DatabaseType} connection to server: {Server}, database: {Database}", 
                request.DatabaseType, request.Server, request.Database);

            var provider = GetProvider(request.DatabaseType);
            var connectionString = provider.BuildConnectionString(
                request.Server, 
                request.Database, 
                request.Username, 
                request.Password, 
                request.TrustServerCertificate);

            using var connection = provider.CreateConnection(connectionString);
            
            var (success, version) = await provider.TestConnectionAsync(connection);

            if (success)
            {
                response.Success = true;
                response.Message = $"{request.DatabaseType} connection successful";
                response.ServerVersion = version;
                response.DatabaseName = request.Database;

                _logger.LogInformation(
                    "Successfully connected to {DatabaseType} {Server}/{Database}. Version: {Version}",
                    request.DatabaseType, request.Server, request.Database, version);
            }
            else
            {
                response.Success = false;
                response.Message = $"{request.DatabaseType} connection failed";
                _logger.LogWarning("Connection test failed for {DatabaseType}", request.DatabaseType);
            }
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Message = $"{request.DatabaseType} connection failed";
            response.ErrorDetails = ex.Message;

            _logger.LogError(ex, 
                "{DatabaseType} connection error for server: {Server}, database: {Database}", 
                request.DatabaseType, request.Server, request.Database);
        }

        response.Timestamp = DateTime.UtcNow;
        return response;
    }

    private IDatabaseProvider GetProvider(DatabaseType databaseType)
    {
        return databaseType switch
        {
            DatabaseType.SqlServer => _sqlServerProvider,
            DatabaseType.MySql => _mySqlProvider,
            _ => throw new NotSupportedException($"Database type {databaseType} is not supported")
        };
    }
}
