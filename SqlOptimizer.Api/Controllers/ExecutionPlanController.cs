using Microsoft.AspNetCore.Mvc;
using SqlOptimizer.Application.DTOs;
using SqlOptimizer.Application.Interfaces;

namespace SqlOptimizer.Api.Controllers;

/// <summary>
/// Controller for execution plan operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ExecutionPlanController : ControllerBase
{
    private readonly IExecutionPlanService _executionPlanService;
    private readonly ILogger<ExecutionPlanController> _logger;

    public ExecutionPlanController(
        IExecutionPlanService executionPlanService,
        ILogger<ExecutionPlanController> logger)
    {
        _executionPlanService = executionPlanService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the execution plan for a stored procedure
    /// </summary>
    /// <param name="request">Execution plan request with connection details and procedure name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Execution plan response with XML and metadata</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/executionplan
    ///     {
    ///         "server": "localhost",
    ///         "database": "MyDatabase",
    ///         "username": "sa",
    ///         "password": "YourPassword123",
    ///         "trustServerCertificate": true,
    ///         "storedProcedureName": "dbo.sp_GetUserOrders",
    ///         "parameters": "{\"@UserId\": 123, \"@StartDate\": \"2026-01-01\"}"
    ///     }
    /// 
    /// This endpoint enables SHOWPLAN_XML to retrieve the execution plan without executing the procedure.
    /// The execution plan XML can be used to analyze query performance and optimization opportunities.
    /// </remarks>
    /// <response code="200">Returns the execution plan XML with metadata</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="500">If an error occurs during execution plan retrieval</response>
    [HttpPost]
    [ProducesResponseType(typeof(ExecutionPlanResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ExecutionPlanResponseDto>> GetExecutionPlan(
        [FromBody] ExecutionPlanRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Received execution plan request for stored procedure: {ProcedureName}",
            request.StoredProcedureName);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid execution plan request: {Errors}",
                string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

            return BadRequest(ModelState);
        }

        var result = await _executionPlanService.GetExecutionPlanAsync(request, cancellationToken);

        if (!result.Success)
        {
            _logger.LogWarning(
                "Failed to get execution plan for {ProcedureName}: {Error}",
                request.StoredProcedureName, result.ErrorMessage);
        }

        // Always return 200 with success flag in the response body
        // This allows clients to handle errors gracefully
        return Ok(result);
    }
}
