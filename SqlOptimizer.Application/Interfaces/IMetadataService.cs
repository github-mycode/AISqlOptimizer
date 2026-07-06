using SqlOptimizer.Application.DTOs;

namespace SqlOptimizer.Application.Interfaces;

/// <summary>
/// Service interface for database metadata operations
/// </summary>
public interface IMetadataService
{
    /// <summary>
    /// Get list of databases on the server
    /// </summary>
    /// <param name="request">Connection details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of databases</returns>
    Task<IEnumerable<DatabaseInfoDto>> GetDatabasesAsync(
        MetadataRequestDto request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get list of tables in the database
    /// </summary>
    /// <param name="request">Connection details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of tables</returns>
    Task<IEnumerable<TableInfoDto>> GetTablesAsync(
        MetadataRequestDto request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get list of views in the database
    /// </summary>
    /// <param name="request">Connection details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of views</returns>
    Task<IEnumerable<ViewInfoDto>> GetViewsAsync(
        MetadataRequestDto request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get list of functions in the database
    /// </summary>
    /// <param name="request">Connection details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of functions</returns>
    Task<IEnumerable<FunctionInfoDto>> GetFunctionsAsync(
        MetadataRequestDto request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get list of stored procedures in the database
    /// </summary>
    /// <param name="request">Connection details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of stored procedures</returns>
    Task<IEnumerable<StoredProcedureInfoDto>> GetStoredProceduresAsync(
        MetadataRequestDto request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get list of indexes in the database
    /// </summary>
    /// <param name="request">Connection details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of indexes</returns>
    Task<IEnumerable<IndexInfoDto>> GetIndexesAsync(
        MetadataRequestDto request, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get list of foreign keys in the database
    /// </summary>
    /// <param name="request">Connection details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of foreign keys</returns>
    Task<IEnumerable<ForeignKeyInfoDto>> GetForeignKeysAsync(
        MetadataRequestDto request, 
        CancellationToken cancellationToken = default);
}
