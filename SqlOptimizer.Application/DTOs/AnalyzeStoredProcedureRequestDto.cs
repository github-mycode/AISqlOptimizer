using SqlOptimizer.Domain.Enums;

namespace SqlOptimizer.Application.DTOs;

/// <summary>
/// Request DTO for analyzing a stored procedure with AI
/// </summary>
public class AnalyzeStoredProcedureRequestDto
{
    /// <summary>
    /// Database type (SqlServer or MySql)
    /// </summary>
    public DatabaseType DatabaseType { get; set; } = DatabaseType.SqlServer;

    /// <summary>
    /// Server instance name or IP address
    /// </summary>
    public string Server { get; set; } = string.Empty;

    /// <summary>
    /// Database name
    /// </summary>
    public string Database { get; set; } = string.Empty;

    /// <summary>
    /// Username for SQL authentication (optional for Windows authentication)
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Password for SQL authentication (optional for Windows authentication)
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Trust server certificate (useful for development/testing)
    /// </summary>
    public bool TrustServerCertificate { get; set; } = true;

    /// <summary>
    /// Stored procedure name (can include schema, e.g., dbo.sp_GetOrders)
    /// </summary>
    public string StoredProcedureName { get; set; } = string.Empty;

    /// <summary>
    /// Optional parameters for the stored procedure in JSON format
    /// Example: { "@UserId": 123, "@StartDate": "2026-01-01" }
    /// </summary>
    public string? Parameters { get; set; }

    /// <summary>
    /// Include execution plan in analysis (default: true)
    /// </summary>
    public bool IncludeExecutionPlan { get; set; } = true;
}
