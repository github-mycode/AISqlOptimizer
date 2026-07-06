using SqlOptimizer.Domain.Entities;

namespace SqlOptimizer.Domain.Interfaces;

/// <summary>
/// Repository interface for SqlQuery entity
/// </summary>
public interface ISqlQueryRepository : IRepository<SqlQuery>
{
    /// <summary>
    /// Get queries by database name
    /// </summary>
    /// <param name="databaseName">Database name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of queries</returns>
    Task<IEnumerable<SqlQuery>> GetByDatabaseNameAsync(string databaseName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get optimized queries
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of optimized queries</returns>
    Task<IEnumerable<SqlQuery>> GetOptimizedQueriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get unoptimized queries
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of unoptimized queries</returns>
    Task<IEnumerable<SqlQuery>> GetUnoptimizedQueriesAsync(CancellationToken cancellationToken = default);
}
