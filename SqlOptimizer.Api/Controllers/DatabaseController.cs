using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SqlOptimizer.Application.DTOs;
using SqlOptimizer.Application.Interfaces;

namespace SqlOptimizer.Api.Controllers;

/// <summary>
/// Controller for database connection operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DatabaseController : ControllerBase
{
    private readonly IConnectionService _connectionService;
    private readonly IValidator<DatabaseConnectionRequestDto> _validator;
    private readonly ILogger<DatabaseController> _logger;

    public DatabaseController(
        IConnectionService connectionService,
        IValidator<DatabaseConnectionRequestDto> validator,
        ILogger<DatabaseController> logger)
    {
        _connectionService = connectionService ?? throw new ArgumentNullException(nameof(connectionService));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Test a database connection
    /// </summary>
    /// <param name="request">Connection details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Connection test result</returns>
    /// <remarks>
    /// Sample request for MySQL:
    /// 
    ///     POST /api/database/connect
    ///     {
    ///        "databaseType": 1,
    ///        "server": "localhost",
    ///        "database": "CompanyDB",
    ///        "username": "root",
    ///        "password": "yourpassword"
    ///     }
    ///     
    /// Sample request for SQL Server with SQL Authentication:
    /// 
    ///     POST /api/database/connect
    ///     {
    ///        "databaseType": 0,
    ///        "server": "localhost",
    ///        "database": "MyDatabase",
    ///        "username": "sa",
    ///        "password": "YourPassword123",
    ///        "trustServerCertificate": true
    ///     }
    ///     
    /// Or for SQL Server with Windows Authentication:
    /// 
    ///     POST /api/database/connect
    ///     {
    ///        "databaseType": 0,
    ///        "server": "localhost\\SQLEXPRESS",
    ///        "database": "MyDatabase",
    ///        "trustServerCertificate": true
    ///     }
    ///     
    /// Note: DatabaseType values: 0 = SqlServer (default), 1 = MySql
    /// Note: Credentials are NOT stored. This endpoint only tests connectivity.
    /// </remarks>
    /// <response code="200">Connection test completed (check response.success for actual result)</response>
    /// <response code="400">Invalid request data</response>
    [HttpPost("connect")]
    [ProducesResponseType(typeof(DatabaseConnectionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DatabaseConnectionResponseDto>> Connect(
        [FromBody] DatabaseConnectionRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received connection test request for server: {Server}, database: {Database}",
            request.Server, request.Database);

        // Validate the request
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validation failed for connection request: {Errors}",
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

        // Test the connection
        var result = await _connectionService.TestConnectionAsync(request, cancellationToken);

        if (result.Success)
        {
            _logger.LogInformation("Connection test successful for {Server}/{Database}",
                request.Server, request.Database);
        }
        else
        {
            _logger.LogWarning("Connection test failed for {Server}/{Database}: {Error}",
                request.Server, request.Database, result.ErrorDetails);
        }

        // Always return 200 with the result object
        // The client checks the 'success' property to determine if connection worked
        return Ok(result);
    }
}
