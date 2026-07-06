namespace SqlOptimizer.Application.DTOs;

/// <summary>
/// DTO for database information
/// </summary>
public class DatabaseInfoDto
{
    /// <summary>
    /// Database name
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// Database ID
    /// </summary>
    public int DatabaseId { get; set; }

    /// <summary>
    /// Creation date
    /// </summary>
    public DateTime CreateDate { get; set; }

    /// <summary>
    /// Compatibility level
    /// </summary>
    public byte CompatibilityLevel { get; set; }

    /// <summary>
    /// Collation name
    /// </summary>
    public string? CollationName { get; set; }

    /// <summary>
    /// Recovery model description
    /// </summary>
    public string? RecoveryModel { get; set; }

    /// <summary>
    /// Database state description
    /// </summary>
    public string? State { get; set; }
}
