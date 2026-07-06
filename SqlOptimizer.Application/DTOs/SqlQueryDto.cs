namespace SqlOptimizer.Application.DTOs;

/// <summary>
/// DTO for SQL query response
/// </summary>
public class SqlQueryDto
{
    /// <summary>
    /// Entity unique identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Query name/title
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The SQL query text
    /// </summary>
    public string QueryText { get; set; } = string.Empty;

    /// <summary>
    /// Query description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Database name where the query runs
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// Execution time in milliseconds
    /// </summary>
    public decimal? ExecutionTimeMs { get; set; }

    /// <summary>
    /// Optimized query text (if optimized)
    /// </summary>
    public string? OptimizedQueryText { get; set; }

    /// <summary>
    /// Optimization notes
    /// </summary>
    public string? OptimizationNotes { get; set; }

    /// <summary>
    /// Whether the query has been optimized
    /// </summary>
    public bool IsOptimized { get; set; }

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
