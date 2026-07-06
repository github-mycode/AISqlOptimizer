namespace SqlOptimizer.Application.DTOs;

/// <summary>
/// DTO for view information
/// </summary>
public class ViewInfoDto
{
    /// <summary>
    /// Schema name
    /// </summary>
    public string SchemaName { get; set; } = string.Empty;

    /// <summary>
    /// View name
    /// </summary>
    public string ViewName { get; set; } = string.Empty;

    /// <summary>
    /// View definition (SQL)
    /// </summary>
    public string? Definition { get; set; }

    /// <summary>
    /// Is the view updatable
    /// </summary>
    public string? IsUpdatable { get; set; }

    /// <summary>
    /// Check option
    /// </summary>
    public string? CheckOption { get; set; }

    /// <summary>
    /// Creation date
    /// </summary>
    public DateTime? CreateDate { get; set; }

    /// <summary>
    /// Last modification date
    /// </summary>
    public DateTime? ModifyDate { get; set; }
}
