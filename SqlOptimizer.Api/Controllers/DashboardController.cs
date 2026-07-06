using Microsoft.AspNetCore.Mvc;
using SqlOptimizer.Application.DTOs;
using SqlOptimizer.Application.Interfaces;

namespace SqlOptimizer.Api.Controllers;

/// <summary>
/// Controller for dashboard operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IDashboardService dashboardService,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Gets dashboard overview with database statistics and analysis metrics
    /// </summary>
    /// <param name="request">Dashboard request with connection details</param>
    /// <returns>Dashboard overview data</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/dashboard
    ///     {
    ///         "serverName": "localhost",
    ///         "databaseName": "MyDatabase",
    ///         "useWindowsAuth": true
    ///     }
    /// 
    /// This endpoint returns comprehensive dashboard metrics including:
    /// - Database metadata counts (procedures, tables, views, indexes)
    /// - Performance analysis metrics (average score, issue counts by severity)
    /// - Top 10 slowest procedures (lowest performance scores)
    /// - Most common problems found across all analyzed procedures
    /// 
    /// **Note:** Analysis metrics are only available after running stored procedure analysis.
    /// Use POST /api/analysis/database to analyze all procedures first.
    /// 
    /// The dashboard aggregates data from:
    /// 1. Real-time metadata queries (current database state)
    /// 2. Cached analysis results (from previous analyses)
    /// 
    /// **Performance:**
    /// - Metadata queries: ~200-500ms depending on database size
    /// - Analysis aggregation: ~50-100ms for 100+ procedures
    /// </remarks>
    /// <response code="200">Returns the dashboard overview</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="500">If an error occurs while fetching dashboard data</response>
    [HttpPost]
    [ProducesResponseType(typeof(DashboardOverviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DashboardOverviewDto>> GetDashboardOverview(
        [FromBody] DashboardRequestDto request)
    {
        _logger.LogInformation(
            "Received dashboard request for database: {DatabaseName}",
            request.DatabaseName);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _dashboardService.GetDashboardOverviewAsync(request);

        return Ok(result);
    }

    /// <summary>
    /// Gets dashboard overview with database statistics (GET method for convenience)
    /// </summary>
    /// <param name="serverName">Server name</param>
    /// <param name="databaseName">Database name</param>
    /// <param name="useWindowsAuth">Use Windows Authentication (default: true)</param>
    /// <param name="username">SQL Server username (optional)</param>
    /// <param name="password">SQL Server password (optional)</param>
    /// <returns>Dashboard overview data</returns>
    /// <remarks>
    /// Convenience GET endpoint for dashboard data with query parameters.
    /// 
    /// Example:
    /// 
    ///     GET /api/dashboard?serverName=localhost&amp;databaseName=MyDatabase
    /// 
    /// For SQL Server authentication:
    /// 
    ///     GET /api/dashboard?serverName=localhost&amp;databaseName=MyDatabase&amp;useWindowsAuth=false&amp;username=sa&amp;password=yourpassword
    /// 
    /// **Note:** For security, prefer using the POST endpoint when providing credentials.
    /// </remarks>
    /// <response code="200">Returns the dashboard overview</response>
    /// <response code="400">If the parameters are invalid</response>
    /// <response code="500">If an error occurs while fetching dashboard data</response>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardOverviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DashboardOverviewDto>> GetDashboardOverviewByQuery(
        [FromQuery] string serverName,
        [FromQuery] string databaseName,
        [FromQuery] bool useWindowsAuth = true,
        [FromQuery] string? username = null,
        [FromQuery] string? password = null)
    {
        _logger.LogInformation(
            "Received GET dashboard request for database: {DatabaseName}",
            databaseName);

        var request = new DashboardRequestDto
        {
            ServerName = serverName,
            DatabaseName = databaseName,
            UseWindowsAuth = useWindowsAuth,
            Username = username,
            Password = password
        };

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _dashboardService.GetDashboardOverviewAsync(request);

        return Ok(result);
    }
}
