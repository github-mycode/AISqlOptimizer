namespace SqlOptimizer.Application.DTOs;

/// <summary>
/// DTO for creating a new SQL query
/// </summary>
public class CreateSqlQueryDto
{
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
}
