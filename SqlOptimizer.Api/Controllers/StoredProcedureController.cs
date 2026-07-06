using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SqlOptimizer.Application.DTOs;
using SqlOptimizer.Application.Interfaces;

namespace SqlOptimizer.Api.Controllers;

/// <summary>
/// Controller for stored procedure operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StoredProcedureController : ControllerBase
{
    private readonly IStoredProcedureService _storedProcedureService;
    private readonly IValidator<StoredProcedureRequestDto> _validator;
    private readonly ILogger<StoredProcedureController> _logger;

    public StoredProcedureController(
        IStoredProcedureService storedProcedureService,
        IValidator<StoredProcedureRequestDto> validator,
        ILogger<StoredProcedureController> logger)
    {
        _storedProcedureService = storedProcedureService ?? throw new ArgumentNullException(nameof(storedProcedureService));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get detailed information about a stored procedure
    /// </summary>
    /// <param name="name">Stored procedure name (can include schema, e.g., dbo.MyProcedure)</param>
    /// <param name="server">SQL Server instance name or address</param>
    /// <param name="database">Database name</param>
    /// <param name="username">Username for SQL authentication (optional)</param>
    /// <param name="password">Password for SQL authentication (optional)</param>
    /// <param name="trustServerCertificate">Trust server certificate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed stored procedure information including definition, parameters, and dependencies</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/storedprocedure/dbo.sp_GetUserOrders
    ///     Body:
    ///     {
    ///        "server": "localhost",
    ///        "database": "MyDatabase",
    ///        "username": "sa",
    ///        "password": "YourPassword123",
    ///        "trustServerCertificate": true
    ///     }
    ///     
    /// Returns comprehensive information including:
    /// - Procedure name and schema
    /// - Complete SQL definition
    /// - Input and output parameters with data types
    /// - Referenced tables and views
    /// - All dependencies (tables, views, other procedures, functions)
    /// - Objects that depend on this procedure
    /// - Return type (NONE, INTEGER, TABLE, OUTPUT_PARAMETERS)
    /// - Creation and modification dates
    /// </remarks>
    /// <response code="200">Returns the stored procedure details</response>
    /// <response code="400">Invalid request data or procedure not found</response>
    /// <response code="404">Stored procedure not found</response>
    [HttpGet("{name}")]
    [ProducesResponseType(typeof(StoredProcedureDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StoredProcedureDetailDto>> GetStoredProcedureDetail(
        string name,
        [FromQuery] string server,
        [FromQuery] string database,
        [FromQuery] string? username = null,
        [FromQuery] string? password = null,
        [FromQuery] bool trustServerCertificate = true,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting stored procedure details for: {Name} in database: {Database}",
            name, database);

        // Build request from route and query parameters
        var request = new StoredProcedureRequestDto
        {
            Server = server,
            Database = database,
            ProcedureName = name,
            Username = username,
            Password = password,
            TrustServerCertificate = trustServerCertificate
        };

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validation failed for stored procedure request: {Errors}",
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

            return BadRequest(new
            {
                success = false,
                message = "Validation failed",
                errors = validationResult.Errors.Select(e => new
                {
                    property = e.PropertyName,
                    error = e.ErrorMessage
                })
            });
        }

        try
        {
            var result = await _storedProcedureService.GetStoredProcedureDetailAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning("Stored procedure not found: {Name}", name);
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stored procedure details for: {Name}", name);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Get detailed information about a stored procedure (POST version for better security)
    /// </summary>
    /// <param name="name">Stored procedure name (can include schema, e.g., dbo.MyProcedure)</param>
    /// <param name="request">Connection details in request body</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed stored procedure information</returns>
    /// <response code="200">Returns the stored procedure details</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Stored procedure not found</response>
    [HttpPost("{name}")]
    [ProducesResponseType(typeof(StoredProcedureDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StoredProcedureDetailDto>> GetStoredProcedureDetailPost(
        string name,
        [FromBody] StoredProcedureRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting stored procedure details for: {Name} in database: {Database}",
            name, request.Database);

        // Override procedure name from route
        request.ProcedureName = name;

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validation failed for stored procedure request: {Errors}",
                string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

            return BadRequest(new
            {
                success = false,
                message = "Validation failed",
                errors = validationResult.Errors.Select(e => new
                {
                    property = e.PropertyName,
                    error = e.ErrorMessage
                })
            });
        }

        try
        {
            var result = await _storedProcedureService.GetStoredProcedureDetailAsync(request, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning("Stored procedure not found: {Name}", name);
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stored procedure details for: {Name}", name);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}
