using SqlOptimizer.Application.DTOs;

namespace SqlOptimizer.Application.Interfaces;

/// <summary>
/// Service interface for SQL query operations
/// </summary>
public interface ISqlQueryService
{
    /// <summary>
    /// Get query by ID
    /// </summary>
    /// <param name="id">Query ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Query DTO or null</returns>
    Task<SqlQueryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all queries
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of query DTOs</returns>
    Task<IEnumerable<SqlQueryDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get queries by database name
    /// </summary>
    /// <param name="databaseName">Database name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of query DTOs</returns>
    Task<IEnumerable<SqlQueryDto>> GetByDatabaseNameAsync(string databaseName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get optimized queries
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of optimized query DTOs</returns>
    Task<IEnumerable<SqlQueryDto>> GetOptimizedQueriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get unoptimized queries
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of unoptimized query DTOs</returns>
    Task<IEnumerable<SqlQueryDto>> GetUnoptimizedQueriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new query
    /// </summary>
    /// <param name="dto">Create query DTO</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created query DTO</returns>
    Task<SqlQueryDto> CreateAsync(CreateSqlQueryDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing query
    /// </summary>
    /// <param name="id">Query ID</param>
    /// <param name="dto">Update query DTO</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if updated successfully</returns>
    Task<bool> UpdateAsync(int id, UpdateSqlQueryDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a query
    /// </summary>
    /// <param name="id">Query ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
