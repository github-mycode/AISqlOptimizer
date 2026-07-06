namespace SqlOptimizer.Application.DTOs;

/// <summary>
/// DTO for updating an existing SQL query
/// </summary>
public class UpdateSqlQueryDto
{
    /// <summary>
    /// Query name/title
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The SQL query text
    /// </summary>
    public string? QueryText { get; set; }

    /// <summary>
    /// Query description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Database name where the query runs
    /// </summary>
    public string? DatabaseName { get; set; }

    /// <summary>
    /// Execution time in milliseconds
    /// </summary>
    public decimal? ExecutionTimeMs { get; set; }

    /// <summary>
    /// Optimized query text
    /// </summary>
    public string? OptimizedQueryText { get; set; }

    /// <summary>
    /// Optimization notes
    /// </summary>
    public string? OptimizationNotes { get; set; }

    /// <summary>
    /// Whether the query has been optimized
    /// </summary>
    public bool? IsOptimized { get; set; }
}
