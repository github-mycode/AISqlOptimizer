namespace SqlOptimizer.Application.DTOs;

/// <summary>
/// Complete database optimization report
/// </summary>
public class DatabaseAnalysisReportDto
{
    /// <summary>
    /// Success status of the operation
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Database name
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// Server name
    /// </summary>
    public string ServerName { get; set; } = string.Empty;

    /// <summary>
    /// Summary of findings
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Total number of stored procedures analyzed
    /// </summary>
    public int TotalProcedures { get; set; }

    /// <summary>
    /// Number of procedures with critical issues
    /// </summary>
    public int CriticalIssues { get; set; }

    /// <summary>
    /// Number of procedures with high severity issues
    /// </summary>
    public int HighIssues { get; set; }

    /// <summary>
    /// Number of procedures with medium severity issues
    /// </summary>
    public int MediumIssues { get; set; }

    /// <summary>
    /// Number of procedures with low severity issues
    /// </summary>
    public int LowIssues { get; set; }

    /// <summary>
    /// Average performance score across all procedures (0-100)
    /// </summary>
    public double AveragePerformanceScore { get; set; }

    /// <summary>
    /// Lowest performance score found
    /// </summary>
    public int? LowestPerformanceScore { get; set; }

    /// <summary>
    /// Highest performance score found
    /// </summary>
    public int? HighestPerformanceScore { get; set; }

    /// <summary>
    /// Individual analysis results for each stored procedure
    /// </summary>
    public List<StoredProcedureAnalysisDto> ProcedureAnalyses { get; set; } = new();

    /// <summary>
    /// Procedures that failed to analyze
    /// </summary>
    public List<FailedAnalysisDto> FailedAnalyses { get; set; } = new();

    /// <summary>
    /// Error message if operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Timestamp of analysis
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Total analysis duration
    /// </summary>
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// Information about a failed procedure analysis
/// </summary>
public class FailedAnalysisDto
{
    /// <summary>
    /// Stored procedure name
    /// </summary>
    public string ProcedureName { get; set; } = string.Empty;

    /// <summary>
    /// Error message
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}
