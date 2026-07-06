namespace SqlOptimizer.Application.DTOs;

/// <summary>
/// Response DTO containing execution plan information
/// </summary>
public class ExecutionPlanResponseDto
{
    /// <summary>
    /// Success status of the operation
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Stored procedure name
    /// </summary>
    public string StoredProcedureName { get; set; } = string.Empty;

    /// <summary>
    /// Database name
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// Execution plan in XML format
    /// </summary>
    public string? ExecutionPlanXml { get; set; }

    /// <summary>
    /// Estimated subtree cost
    /// </summary>
    public decimal? EstimatedCost { get; set; }

    /// <summary>
    /// Estimated number of rows
    /// </summary>
    public long? EstimatedRows { get; set; }

    /// <summary>
    /// Error message if operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Timestamp when the plan was retrieved
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
