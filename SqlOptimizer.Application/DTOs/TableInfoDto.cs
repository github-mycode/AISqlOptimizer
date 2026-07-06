namespace SqlOptimizer.Application.DTOs;

/// <summary>
/// DTO for table information
/// </summary>
public class TableInfoDto
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
    /// Table type (BASE TABLE, VIEW)
    /// </summary>
    public string TableType { get; set; } = string.Empty;

    /// <summary>
    /// Number of rows (approximate)
    /// </summary>
    public long? RowCount { get; set; }

    /// <summary>
    /// Total space used in KB
    /// </summary>
    public long? TotalSpaceKB { get; set; }

    /// <summary>
    /// Data space used in KB
    /// </summary>
    public long? UsedSpaceKB { get; set; }

    /// <summary>
    /// Creation date
    /// </summary>
    public DateTime? CreateDate { get; set; }

    /// <summary>
    /// Last modification date
    /// </summary>
    public DateTime? ModifyDate { get; set; }
}
