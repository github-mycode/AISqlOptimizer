using Dapper;
using Microsoft.Extensions.Logging;
using SqlOptimizer.Domain.Common;
using SqlOptimizer.Domain.Interfaces;
using SqlOptimizer.Infrastructure.Data;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;

namespace SqlOptimizer.Infrastructure.Repositories;

/// <summary>
/// Base repository implementation using Dapper with logging
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public abstract class BaseRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly IDbConnectionFactory _connectionFactory;
    protected readonly ILogger _logger;
    protected abstract string TableName { get; }

    protected BaseRepository(
        IDbConnectionFactory connectionFactory,
        ILogger logger)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = $"SELECT * FROM {TableName} WHERE Id = @Id AND IsDeleted = 0";
            
            _logger.LogDebug("Executing GetByIdAsync. Table: {TableName}, Id: {Id}", TableName, id);
            
            var result = await connection.QueryFirstOrDefaultAsync<T>(sql, new { Id = id });
            
            stopwatch.Stop();
            _logger.LogInformation(
                "GetByIdAsync completed in {ElapsedMs}ms. Table: {TableName}, Id: {Id}, Found: {Found}",
                stopwatch.ElapsedMilliseconds,
                TableName,
                id,
                result != null);
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "GetByIdAsync failed after {ElapsedMs}ms. Table: {TableName}, Id: {Id}",
                stopwatch.ElapsedMilliseconds,
                TableName,
                id);
            throw;
        }
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = $"SELECT * FROM {TableName} WHERE IsDeleted = 0";
            
            _logger.LogDebug("Executing GetAllAsync. Table: {TableName}", TableName);
            
            var result = await connection.QueryAsync<T>(sql);
            
            stopwatch.Stop();
            _logger.LogInformation(
                "GetAllAsync completed in {ElapsedMs}ms. Table: {TableName}, Count: {Count}",
                stopwatch.ElapsedMilliseconds,
                TableName,
                result.Count());
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "GetAllAsync failed after {ElapsedMs}ms. Table: {TableName}",
                stopwatch.ElapsedMilliseconds,
                TableName);
            throw;
        }
    }

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            _logger.LogDebug("Executing FindAsync. Table: {TableName}", TableName);
            
            // Note: This is a simple implementation. For complex predicates, consider using a library like DapperQueryBuilder
            var all = await GetAllAsync(cancellationToken);
            var result = all.Where(predicate.Compile());
            
            stopwatch.Stop();
            _logger.LogInformation(
                "FindAsync completed in {ElapsedMs}ms. Table: {TableName}, Count: {Count}",
                stopwatch.ElapsedMilliseconds,
                TableName,
                result.Count());
            
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "FindAsync failed after {ElapsedMs}ms. Table: {TableName}",
                stopwatch.ElapsedMilliseconds,
                TableName);
            throw;
        }
    }

    /// <inheritdoc/>
    public abstract Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public abstract Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <inheritdoc/>
    public virtual async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = $"UPDATE {TableName} SET IsDeleted = 1 WHERE Id = @Id";
            
            _logger.LogDebug("Executing DeleteAsync. Table: {TableName}, Id: {Id}", TableName, id);
            
            await connection.ExecuteAsync(sql, new { Id = id });
            
            stopwatch.Stop();
            _logger.LogInformation(
                "DeleteAsync completed in {ElapsedMs}ms. Table: {TableName}, Id: {Id}",
                stopwatch.ElapsedMilliseconds,
                TableName,
                id);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "DeleteAsync failed after {ElapsedMs}ms. Table: {TableName}, Id: {Id}",
                stopwatch.ElapsedMilliseconds,
                TableName,
                id);
            throw;
        }
    }

    /// <inheritdoc/>
    public virtual async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = $"SELECT COUNT(1) FROM {TableName} WHERE Id = @Id AND IsDeleted = 0";
            
            _logger.LogDebug("Executing ExistsAsync. Table: {TableName}, Id: {Id}", TableName, id);
            
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
            var exists = count > 0;
            
            stopwatch.Stop();
            _logger.LogInformation(
                "ExistsAsync completed in {ElapsedMs}ms. Table: {TableName}, Id: {Id}, Exists: {Exists}",
                stopwatch.ElapsedMilliseconds,
                TableName,
                id,
                exists);
            
            return exists;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "ExistsAsync failed after {ElapsedMs}ms. Table: {TableName}, Id: {Id}",
                stopwatch.ElapsedMilliseconds,
                TableName,
                id);
            throw;
        }
    }
}
