using SqlOptimizer.Application.DTOs;

namespace SqlOptimizer.Application.Interfaces;

/// <summary>
/// Service interface for database connection operations
/// </summary>
public interface IConnectionService
{
    /// <summary>
    /// Test a database connection
    /// </summary>
    /// <param name="request">Connection request details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Connection response with success/failure details</returns>
    Task<DatabaseConnectionResponseDto> TestConnectionAsync(
        DatabaseConnectionRequestDto request, 
        CancellationToken cancellationToken = default);
}
