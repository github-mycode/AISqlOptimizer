namespace SqlOptimizer.Application.DTOs;

/// <summary>
/// DTO for detailed stored procedure information
/// </summary>
public class StoredProcedureDetailDto
{
    /// <summary>
    /// Schema name
    /// </summary>
    public string SchemaName { get; set; } = string.Empty;

    /// <summary>
    /// Stored procedure name
    /// </summary>
    public string ProcedureName { get; set; } = string.Empty;

    /// <summary>
    /// Complete SQL definition
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

    /// <summary>
    /// Is the procedure encrypted
    /// </summary>
    public bool IsEncrypted { get; set; }

    /// <summary>
    /// Return type (NONE, INTEGER, TABLE)
    /// </summary>
    public string ReturnType { get; set; } = "NONE";

    /// <summary>
    /// List of parameters
    /// </summary>
    public List<ParameterInfoDto> Parameters { get; set; } = new();

    /// <summary>
    /// List of referenced tables
    /// </summary>
    public List<DependencyInfoDto> ReferencedTables { get; set; } = new();

    /// <summary>
    /// List of referenced views
    /// </summary>
    public List<DependencyInfoDto> ReferencedViews { get; set; } = new();

    /// <summary>
    /// All dependencies (tables, views, procedures, functions)
    /// </summary>
    public List<DependencyInfoDto> Dependencies { get; set; } = new();

    /// <summary>
    /// Objects that depend on this procedure
    /// </summary>
    public List<DependencyInfoDto> DependentObjects { get; set; } = new();
}
