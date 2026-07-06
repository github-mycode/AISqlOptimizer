using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SqlOptimizer.Application.DTOs;
using SqlOptimizer.Application.Interfaces;

namespace SqlOptimizer.Api.Controllers;

/// <summary>
/// Controller for database metadata operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MetadataController : ControllerBase
{
    private readonly IMetadataService _metadataService;
    private readonly IValidator<MetadataRequestDto> _validator;
    private readonly ILogger<MetadataController> _logger;

    public MetadataController(
        IMetadataService metadataService,
        IValidator<MetadataRequestDto> validator,
        ILogger<MetadataController> logger)
    {
        _metadataService = metadataService ?? throw new ArgumentNullException(nameof(metadataService));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get list of databases on the SQL Server instance
    /// </summary>
    /// <param name="request">Connection details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of databases</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/metadata/databases
    ///     {
    ///        "server": "localhost",
    ///        "database": "master",
    ///        "username": "sa",
    ///        "password": "YourPassword123",
    ///        "trustServerCertificate": true
    ///     }
    /// </remarks>
    /// <response code="200">Returns the list of databases</response>
    /// <response code="400">Invalid request data</response>
    [HttpPost("databases")]
    [ProducesResponseType(typeof(IEnumerable<DatabaseInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<DatabaseInfoDto>>> GetDatabases(
        [FromBody] MetadataRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting databases from server: {Server}", request.Server);

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
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
            var databases = await _metadataService.GetDatabasesAsync(request, cancellationToken);
            return Ok(databases);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting databases from {Server}", request.Server);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Get list of tables in the database
    /// </summary>
    /// <param name="request">Connection details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of tables</returns>
    /// <response code="200">Returns the list of tables</response>
    /// <response code="400">Invalid request data</response>
    [HttpPost("tables")]
    [ProducesResponseType(typeof(IEnumerable<TableInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<TableInfoDto>>> GetTables(
        [FromBody] MetadataRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting tables from database: {Database}", request.Database);

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
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
            var tables = await _metadataService.GetTablesAsync(request, cancellationToken);
            return Ok(tables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tables from {Database}", request.Database);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Get list of views in the database
    /// </summary>
    /// <param name="request">Connection details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of views</returns>
    /// <response code="200">Returns the list of views</response>
    /// <response code="400">Invalid request data</response>
    [HttpPost("views")]
    [ProducesResponseType(typeof(IEnumerable<ViewInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ViewInfoDto>>> GetViews(
        [FromBody] MetadataRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting views from database: {Database}", request.Database);

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
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
            var views = await _metadataService.GetViewsAsync(request, cancellationToken);
            return Ok(views);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting views from {Database}", request.Database);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Get list of functions in the database
    /// </summary>
    /// <param name="request">Connection details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of functions</returns>
    /// <response code="200">Returns the list of functions</response>
    /// <response code="400">Invalid request data</response>
    [HttpPost("functions")]
    [ProducesResponseType(typeof(IEnumerable<FunctionInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<FunctionInfoDto>>> GetFunctions(
        [FromBody] MetadataRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting functions from database: {Database}", request.Database);

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
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
            var functions = await _metadataService.GetFunctionsAsync(request, cancellationToken);
            return Ok(functions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting functions from {Database}", request.Database);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Get list of stored procedures in the database
    /// </summary>
    /// <param name="request">Connection details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of stored procedures</returns>
    /// <response code="200">Returns the list of stored procedures</response>
    /// <response code="400">Invalid request data</response>
    [HttpPost("storedprocedures")]
    [ProducesResponseType(typeof(IEnumerable<StoredProcedureInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<StoredProcedureInfoDto>>> GetStoredProcedures(
        [FromBody] MetadataRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting stored procedures from database: {Database}", request.Database);

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
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
            var procedures = await _metadataService.GetStoredProceduresAsync(request, cancellationToken);
            return Ok(procedures);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stored procedures from {Database}", request.Database);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Get list of indexes in the database
    /// </summary>
    /// <param name="request">Connection details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of indexes</returns>
    /// <response code="200">Returns the list of indexes</response>
    /// <response code="400">Invalid request data</response>
    [HttpPost("indexes")]
    [ProducesResponseType(typeof(IEnumerable<IndexInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<IndexInfoDto>>> GetIndexes(
        [FromBody] MetadataRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting indexes from database: {Database}", request.Database);

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
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
            var indexes = await _metadataService.GetIndexesAsync(request, cancellationToken);
            return Ok(indexes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting indexes from {Database}", request.Database);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Get list of foreign keys in the database
    /// </summary>
    /// <param name="request">Connection details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of foreign keys</returns>
    /// <response code="200">Returns the list of foreign keys</response>
    /// <response code="400">Invalid request data</response>
    [HttpPost("foreignkeys")]
    [ProducesResponseType(typeof(IEnumerable<ForeignKeyInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ForeignKeyInfoDto>>> GetForeignKeys(
        [FromBody] MetadataRequestDto request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting foreign keys from database: {Database}", request.Database);

        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
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
            var foreignKeys = await _metadataService.GetForeignKeysAsync(request, cancellationToken);
            return Ok(foreignKeys);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting foreign keys from {Database}", request.Database);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}
