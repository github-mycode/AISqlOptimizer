namespace SqlOptimizer.Application.DTOs;

/// <summary>
/// DTO for stored procedure parameter information
/// </summary>
public class ParameterInfoDto
{
    /// <summary>
    /// Parameter name
    /// </summary>
    public string ParameterName { get; set; } = string.Empty;

    /// <summary>
    /// Parameter data type
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Maximum length for string types
    /// </summary>
    public int? MaxLength { get; set; }

    /// <summary>
    /// Precision for numeric types
    /// </summary>
    public byte? Precision { get; set; }

    /// <summary>
    /// Scale for numeric types
    /// </summary>
    public byte? Scale { get; set; }

    /// <summary>
    /// Is this an output parameter
    /// </summary>
    public bool IsOutput { get; set; }

    /// <summary>
    /// Does the parameter have a default value
    /// </summary>
    public bool HasDefaultValue { get; set; }

    /// <summary>
    /// Default value (if any)
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Is the parameter nullable
    /// </summary>
    public bool IsNullable { get; set; }
}
