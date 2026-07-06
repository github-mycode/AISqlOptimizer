using SqlOptimizer.Application.DTOs;

namespace SqlOptimizer.Application.Interfaces;

/// <summary>
/// Service interface for stored procedure operations
/// </summary>
public interface IStoredProcedureService
{
    /// <summary>
    /// Get detailed information about a stored procedure
    /// </summary>
    /// <param name="request">Request with connection details and procedure name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed stored procedure information</returns>
    Task<StoredProcedureDetailDto> GetStoredProcedureDetailAsync(
        StoredProcedureRequestDto request,
        CancellationToken cancellationToken = default);
}
