using SqlOptimizer.Domain.Enums;

namespace SqlOptimizer.Application.DTOs;

/// <summary>
/// Request DTO for analyzing all stored procedures in a database
/// </summary>
public class AnalyzeDatabaseRequestDto
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
    /// Include execution plan in analysis (default: true)
    /// </summary>
    public bool IncludeExecutionPlan { get; set; } = true;

    /// <summary>
    /// Maximum number of parallel analyses (default: 5)
    /// </summary>
    public int MaxParallelism { get; set; } = 5;

    /// <summary>
    /// Optional schema filter (e.g., "dbo"). If null, analyzes all schemas.
    /// </summary>
    public string? SchemaFilter { get; set; }
}
