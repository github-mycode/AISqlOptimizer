using SqlOptimizer.Application.DTOs;

namespace SqlOptimizer.Application.Interfaces;

/// <summary>
/// Service interface for dashboard operations
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Gets dashboard overview with database statistics and analysis metrics
    /// </summary>
    /// <param name="request">Dashboard request with connection details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dashboard overview data</returns>
    Task<DashboardOverviewDto> GetDashboardOverviewAsync(
        DashboardRequestDto request,
        CancellationToken cancellationToken = default);
}
