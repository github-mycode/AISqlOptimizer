namespace SqlOptimizer.Application.DTOs;

/// <summary>
/// DTO for function information
/// </summary>
public class FunctionInfoDto
{
    /// <summary>
    /// Schema name
    /// </summary>
    public string SchemaName { get; set; } = string.Empty;

    /// <summary>
    /// Function name
    /// </summary>
    public string FunctionName { get; set; } = string.Empty;

    /// <summary>
    /// Function type
    /// </summary>
    public string FunctionType { get; set; } = string.Empty;

    /// <summary>
    /// Function type description
    /// </summary>
    public string? FunctionTypeDesc { get; set; }

    /// <summary>
    /// Function definition (SQL)
    /// </summary>
    public string? Definition { get; set; }

    /// <summary>
    /// Creation date
    /// </summary>
    public DateTime? CreateDate { get; set; }

    /// <summary>
    /// Last modification date
    /// </summary>
    public DateTime? ModifyDate { get; set; }
}
