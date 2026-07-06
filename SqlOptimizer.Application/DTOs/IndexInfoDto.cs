namespace SqlOptimizer.Application.DTOs;

/// <summary>
/// DTO for index information
/// </summary>
public class IndexInfoDto
{
    /// <summary>
    /// Schema name
    /// </summary>
    public string SchemaName { get; set; } = string.Empty;

    /// <summary>
    /// Table name
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Index name
    /// </summary>
    public string IndexName { get; set; } = string.Empty;

    /// <summary>
    /// Index type description
    /// </summary>
    public string IndexType { get; set; } = string.Empty;

    /// <summary>
    /// Is unique index
    /// </summary>
    public bool IsUnique { get; set; }

    /// <summary>
    /// Is primary key
    /// </summary>
    public bool IsPrimaryKey { get; set; }

    /// <summary>
    /// Is unique constraint
    /// </summary>
    public bool IsUniqueConstraint { get; set; }

    /// <summary>
    /// Indexed columns (comma-separated)
    /// </summary>
    public string? IndexedColumns { get; set; }

    /// <summary>
    /// Included columns (comma-separated)
    /// </summary>
    public string? IncludedColumns { get; set; }

    /// <summary>
    /// Filter definition (for filtered indexes)
    /// </summary>
    public string? FilterDefinition { get; set; }
}
