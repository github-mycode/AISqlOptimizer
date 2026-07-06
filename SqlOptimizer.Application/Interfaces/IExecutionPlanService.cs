using SqlOptimizer.Application.DTOs;

namespace SqlOptimizer.Application.Interfaces;

/// <summary>
/// Service interface for retrieving SQL execution plans
/// </summary>
public interface IExecutionPlanService
{
    /// <summary>
    /// Gets the execution plan for a stored procedure
    /// </summary>
    /// <param name="request">Execution plan request with connection details and procedure name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Execution plan response with XML and metadata</returns>
    Task<ExecutionPlanResponseDto> GetExecutionPlanAsync(
        ExecutionPlanRequestDto request,
        CancellationToken cancellationToken = default);
}
