using Dapper;
using Microsoft.Extensions.Logging;
using SqlOptimizer.Application.DTOs;
using SqlOptimizer.Application.Interfaces;
using SqlOptimizer.Domain.Enums;

namespace SqlOptimizer.Application.Services;

/// <summary>
/// Service implementation for database metadata operations
/// </summary>
public class MetadataService : IMetadataService
{
    private readonly IDatabaseProvider _sqlServerProvider;
    private readonly IDatabaseProvider _mySqlProvider;
    private readonly ILogger<MetadataService> _logger;

    public MetadataService(
        IEnumerable<IDatabaseProvider> providers,
        ILogger<MetadataService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        var providerList = providers.ToList();
        _sqlServerProvider = providerList.FirstOrDefault(p => p.GetType().Name.Contains("SqlServer"))
            ?? throw new InvalidOperationException("SqlServerProvider not registered");
        _mySqlProvider = providerList.FirstOrDefault(p => p.GetType().Name.Contains("MySql"))
            ?? throw new InvalidOperationException("MySqlProvider not registered");
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<DatabaseInfoDto>> GetDatabasesAsync(
        MetadataRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting databases from {DatabaseType} server: {Server}", 
            request.DatabaseType, request.Server);

        var provider = GetProvider(request.DatabaseType);
        var sql = provider.GetDatabasesQuery();

        return await ExecuteQueryAsync<DatabaseInfoDto>(request, provider, sql, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TableInfoDto>> GetTablesAsync(
        MetadataRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting tables from {DatabaseType} database: {Database}", 
            request.DatabaseType, request.Database);

        var provider = GetProvider(request.DatabaseType);
        var sql = provider.GetTablesQuery(request.Database);

        return await ExecuteQueryAsync<TableInfoDto>(request, provider, sql, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ViewInfoDto>> GetViewsAsync(
        MetadataRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting views from {DatabaseType} database: {Database}", 
            request.DatabaseType, request.Database);

        var provider = GetProvider(request.DatabaseType);
        var sql = provider.GetViewsQuery(request.Database);

        return await ExecuteQueryAsync<ViewInfoDto>(request, provider, sql, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<FunctionInfoDto>> GetFunctionsAsync(
        MetadataRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting functions from {DatabaseType} database: {Database}", 
            request.DatabaseType, request.Database);

        var provider = GetProvider(request.DatabaseType);
        var sql = provider.GetFunctionsQuery(request.Database);

        return await ExecuteQueryAsync<FunctionInfoDto>(request, provider, sql, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<StoredProcedureInfoDto>> GetStoredProceduresAsync(
        MetadataRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting stored procedures from {DatabaseType} database: {Database}", 
            request.DatabaseType, request.Database);

        var provider = GetProvider(request.DatabaseType);
        var sql = provider.GetStoredProceduresQuery(request.Database);

        return await ExecuteQueryAsync<StoredProcedureInfoDto>(request, provider, sql, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<IndexInfoDto>> GetIndexesAsync(
        MetadataRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting indexes from {DatabaseType} database: {Database}", 
            request.DatabaseType, request.Database);

        var provider = GetProvider(request.DatabaseType);
        var sql = provider.GetIndexesQuery(request.Database);

        return await ExecuteQueryAsync<IndexInfoDto>(request, provider, sql, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ForeignKeyInfoDto>> GetForeignKeysAsync(
        MetadataRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting foreign keys from {DatabaseType} database: {Database}", 
            request.DatabaseType, request.Database);

        var provider = GetProvider(request.DatabaseType);
        var sql = provider.GetForeignKeysQuery(request.Database);

        return await ExecuteQueryAsync<ForeignKeyInfoDto>(request, provider, sql, cancellationToken);
    }

    /// <summary>
    /// Helper method to execute queries with connection management
    /// </summary>
    private async Task<IEnumerable<T>> ExecuteQueryAsync<T>(
        MetadataRequestDto request,
        IDatabaseProvider provider,
        string sql,
        CancellationToken cancellationToken)
    {
        try
        {
            var connectionString = provider.BuildConnectionString(
                request.Server,
                request.Database,
                request.Username,
                request.Password,
                request.TrustServerCertificate);

            using var connection = provider.CreateConnection(connectionString);

            var result = await connection.QueryAsync<T>(sql);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{DatabaseType} error while querying metadata from {Database}", 
                request.DatabaseType, request.Database);
            throw new InvalidOperationException($"Failed to query metadata: {ex.Message}", ex);
        }
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
