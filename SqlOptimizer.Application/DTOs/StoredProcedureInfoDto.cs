namespace SqlOptimizer.Application.DTOs;

/// <summary>
/// DTO for stored procedure information
/// </summary>
public class StoredProcedureInfoDto
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
    /// Procedure type
    /// </summary>
    public string ProcedureType { get; set; } = string.Empty;

    /// <summary>
    /// Procedure definition (SQL)
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
    /// Is the procedure recompiled on execution
    /// </summary>
    public bool IsRecompiled { get; set; }

    /// <summary>
    /// Is the procedure encrypted
    /// </summary>
    public bool IsEncrypted { get; set; }
}
