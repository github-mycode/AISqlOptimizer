using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SqlOptimizer.Application.DTOs;
using SqlOptimizer.Application.Interfaces;

namespace SqlOptimizer.Api.Controllers;

/// <summary>
/// Controller for SQL query operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SqlQueriesController : ControllerBase
{
    private readonly ISqlQueryService _service;
    private readonly IValidator<CreateSqlQueryDto> _createValidator;
    private readonly IValidator<UpdateSqlQueryDto> _updateValidator;
    private readonly ILogger<SqlQueriesController> _logger;

    public SqlQueriesController(
        ISqlQueryService service,
        IValidator<CreateSqlQueryDto> createValidator,
        IValidator<UpdateSqlQueryDto> updateValidator,
        ILogger<SqlQueriesController> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _createValidator = createValidator ?? throw new ArgumentNullException(nameof(createValidator));
        _updateValidator = updateValidator ?? throw new ArgumentNullException(nameof(updateValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all SQL queries
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of SQL queries</returns>
    /// <response code="200">Returns the list of queries</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SqlQueryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SqlQueryDto>>> GetAll(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting all SQL queries");
        var queries = await _service.GetAllAsync(cancellationToken);
        return Ok(queries);
    }

    /// <summary>
    /// Get SQL query by ID
    /// </summary>
    /// <param name="id">Query ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>SQL query</returns>
    /// <response code="200">Returns the query</response>
    /// <response code="404">Query not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SqlQueryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SqlQueryDto>> GetById(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting SQL query with ID: {Id}", id);
        var query = await _service.GetByIdAsync(id, cancellationToken);
        
        if (query == null)
        {
            _logger.LogWarning("SQL query with ID: {Id} not found", id);
            return NotFound(new { message = $"Query with ID {id} not found" });
        }

        return Ok(query);
    }

    /// <summary>
    /// Get queries by database name
    /// </summary>
    /// <param name="databaseName">Database name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of SQL queries</returns>
    /// <response code="200">Returns the list of queries</response>
    [HttpGet("database/{databaseName}")]
    [ProducesResponseType(typeof(IEnumerable<SqlQueryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SqlQueryDto>>> GetByDatabase(string databaseName, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting SQL queries for database: {DatabaseName}", databaseName);
        var queries = await _service.GetByDatabaseNameAsync(databaseName, cancellationToken);
        return Ok(queries);
    }

    /// <summary>
    /// Get optimized queries
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of optimized SQL queries</returns>
    /// <response code="200">Returns the list of optimized queries</response>
    [HttpGet("optimized")]
    [ProducesResponseType(typeof(IEnumerable<SqlQueryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SqlQueryDto>>> GetOptimized(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting optimized SQL queries");
        var queries = await _service.GetOptimizedQueriesAsync(cancellationToken);
        return Ok(queries);
    }

    /// <summary>
    /// Get unoptimized queries
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of unoptimized SQL queries</returns>
    /// <response code="200">Returns the list of unoptimized queries</response>
    [HttpGet("unoptimized")]
    [ProducesResponseType(typeof(IEnumerable<SqlQueryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SqlQueryDto>>> GetUnoptimized(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting unoptimized SQL queries");
        var queries = await _service.GetUnoptimizedQueriesAsync(cancellationToken);
        return Ok(queries);
    }

    /// <summary>
    /// Create a new SQL query
    /// </summary>
    /// <param name="dto">Create query DTO</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created SQL query</returns>
    /// <response code="201">Query created successfully</response>
    /// <response code="400">Invalid request</response>
    [HttpPost]
    [ProducesResponseType(typeof(SqlQueryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SqlQueryDto>> Create([FromBody] CreateSqlQueryDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new SQL query: {Name}", dto.Name);

        var validationResult = await _createValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validation failed for create query: {Errors}", validationResult.Errors);
            return BadRequest(validationResult.Errors);
        }

        var created = await _service.CreateAsync(dto, cancellationToken);
        _logger.LogInformation("Created SQL query with ID: {Id}", created.Id);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>
    /// Update an existing SQL query
    /// </summary>
    /// <param name="id">Query ID</param>
    /// <param name="dto">Update query DTO</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Query updated successfully</response>
    /// <response code="400">Invalid request</response>
    /// <response code="404">Query not found</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSqlQueryDto dto, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating SQL query with ID: {Id}", id);

        var validationResult = await _updateValidator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Validation failed for update query: {Errors}", validationResult.Errors);
            return BadRequest(validationResult.Errors);
        }

        var updated = await _service.UpdateAsync(id, dto, cancellationToken);
        if (!updated)
        {
            _logger.LogWarning("SQL query with ID: {Id} not found for update", id);
            return NotFound(new { message = $"Query with ID {id} not found" });
        }

        _logger.LogInformation("Updated SQL query with ID: {Id}", id);
        return NoContent();
    }

    /// <summary>
    /// Delete a SQL query
    /// </summary>
    /// <param name="id">Query ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    /// <response code="204">Query deleted successfully</response>
    /// <response code="404">Query not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting SQL query with ID: {Id}", id);

        var deleted = await _service.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            _logger.LogWarning("SQL query with ID: {Id} not found for deletion", id);
            return NotFound(new { message = $"Query with ID {id} not found" });
        }

        _logger.LogInformation("Deleted SQL query with ID: {Id}", id);
        return NoContent();
    }
}
