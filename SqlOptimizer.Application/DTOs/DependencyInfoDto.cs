namespace SqlOptimizer.Application.DTOs;

/// <summary>
/// DTO for dependency information
/// </summary>
public class DependencyInfoDto
{
    /// <summary>
    /// Referenced object schema
    /// </summary>
    public string SchemaName { get; set; } = string.Empty;

    /// <summary>
    /// Referenced object name
    /// </summary>
    public string ObjectName { get; set; } = string.Empty;

    /// <summary>
    /// Referenced object type (TABLE, VIEW, PROCEDURE, FUNCTION)
    /// </summary>
    public string ObjectType { get; set; } = string.Empty;

    /// <summary>
    /// Dependency type description
    /// </summary>
    public string? DependencyType { get; set; }
}
