namespace SqlOptimizer.Application.DTOs;

/// <summary>
/// DTO for foreign key information
/// </summary>
public class ForeignKeyInfoDto
{
    /// <summary>
    /// Foreign key constraint name
    /// </summary>
    public string ConstraintName { get; set; } = string.Empty;

    /// <summary>
    /// Schema of the referencing table
    /// </summary>
    public string SchemaName { get; set; } = string.Empty;

    /// <summary>
    /// Referencing table name
    /// </summary>
    public string TableName { get; set; } = string.Empty;

    /// <summary>
    /// Referencing column names (comma-separated)
    /// </summary>
    public string ColumnNames { get; set; } = string.Empty;

    /// <summary>
    /// Schema of the referenced table
    /// </summary>
    public string ReferencedSchemaName { get; set; } = string.Empty;

    /// <summary>
    /// Referenced table name
    /// </summary>
    public string ReferencedTableName { get; set; } = string.Empty;

    /// <summary>
    /// Referenced column names (comma-separated)
    /// </summary>
    public string ReferencedColumnNames { get; set; } = string.Empty;

    /// <summary>
    /// Delete rule (CASCADE, SET NULL, etc.)
    /// </summary>
    public string? DeleteRule { get; set; }

    /// <summary>
    /// Update rule (CASCADE, SET NULL, etc.)
    /// </summary>
    public string? UpdateRule { get; set; }

    /// <summary>
    /// Is the constraint disabled
    /// </summary>
    public bool IsDisabled { get; set; }

    /// <summary>
    /// Is the constraint trusted
    /// </summary>
    public bool IsNotTrusted { get; set; }
}
