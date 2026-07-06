using Dapper;
using Microsoft.Extensions.Logging;
using SqlOptimizer.Domain.Entities;
using SqlOptimizer.Domain.Interfaces;
using SqlOptimizer.Infrastructure.Data;

namespace SqlOptimizer.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for SqlQuery entity
/// </summary>
public class SqlQueryRepository : BaseRepository<SqlQuery>, ISqlQueryRepository
{
    protected override string TableName => "SqlQueries";

    public SqlQueryRepository(
        IDbConnectionFactory connectionFactory,
        ILogger<SqlQueryRepository> logger) 
        : base(connectionFactory, logger)
    {
    }

    /// <inheritdoc/>
    public override async Task<SqlQuery> AddAsync(SqlQuery entity, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            INSERT INTO SqlQueries (Name, QueryText, Description, DatabaseName, ExecutionTimeMs, 
                                   OptimizedQueryText, OptimizationNotes, IsOptimized, CreatedAt, IsDeleted)
            VALUES (@Name, @QueryText, @Description, @DatabaseName, @ExecutionTimeMs, 
                   @OptimizedQueryText, @OptimizationNotes, @IsOptimized, @CreatedAt, @IsDeleted);
            SELECT CAST(SCOPE_IDENTITY() as int);";

        var id = await connection.ExecuteScalarAsync<int>(sql, entity);
        entity.Id = id;
        return entity;
    }

    /// <inheritdoc/>
    public override async Task UpdateAsync(SqlQuery entity, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            UPDATE SqlQueries 
            SET Name = @Name,
                QueryText = @QueryText,
                Description = @Description,
                DatabaseName = @DatabaseName,
                ExecutionTimeMs = @ExecutionTimeMs,
                OptimizedQueryText = @OptimizedQueryText,
                OptimizationNotes = @OptimizationNotes,
                IsOptimized = @IsOptimized,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        await connection.ExecuteAsync(sql, entity);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<SqlQuery>> GetByDatabaseNameAsync(string databaseName, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM SqlQueries WHERE DatabaseName = @DatabaseName AND IsDeleted = 0";
        return await connection.QueryAsync<SqlQuery>(sql, new { DatabaseName = databaseName });
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<SqlQuery>> GetOptimizedQueriesAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM SqlQueries WHERE IsOptimized = 1 AND IsDeleted = 0";
        return await connection.QueryAsync<SqlQuery>(sql);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<SqlQuery>> GetUnoptimizedQueriesAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM SqlQueries WHERE IsOptimized = 0 AND IsDeleted = 0";
        return await connection.QueryAsync<SqlQuery>(sql);
    }
}
