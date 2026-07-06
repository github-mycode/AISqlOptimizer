namespace SqlOptimizer.Application.DTOs;

/// <summary>
/// Dashboard overview response DTO
/// </summary>
public class DashboardOverviewDto
{
    /// <summary>
    /// Database name
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// Server name
    /// </summary>
    public string ServerName { get; set; } = string.Empty;

    /// <summary>
    /// Total number of stored procedures
    /// </summary>
    public int StoredProcedureCount { get; set; }

    /// <summary>
    /// Total number of tables
    /// </summary>
    public int TableCount { get; set; }

    /// <summary>
    /// Total number of views
    /// </summary>
    public int ViewCount { get; set; }

    /// <summary>
    /// Total number of indexes
    /// </summary>
    public int IndexCount { get; set; }

    /// <summary>
    /// Average performance score across analyzed procedures
    /// </summary>
    public double AveragePerformanceScore { get; set; }

    /// <summary>
    /// Number of critical issues found
    /// </summary>
    public int CriticalIssuesCount { get; set; }

    /// <summary>
    /// Number of high severity issues
    /// </summary>
    public int HighIssuesCount { get; set; }

    /// <summary>
    /// Number of medium severity issues
    /// </summary>
    public int MediumIssuesCount { get; set; }

    /// <summary>
    /// Number of low severity issues
    /// </summary>
    public int LowIssuesCount { get; set; }

    /// <summary>
    /// Top 10 slowest stored procedures
    /// </summary>
    public List<SlowProcedureDto> Top10SlowProcedures { get; set; } = new();

    /// <summary>
    /// Most common problems found
    /// </summary>
    public List<CommonProblemDto> MostCommonProblems { get; set; } = new();

    /// <summary>
    /// Total number of procedures analyzed
    /// </summary>
    public int AnalyzedProcedureCount { get; set; }

    /// <summary>
    /// Last analysis date
    /// </summary>
    public DateTime? LastAnalysisDate { get; set; }
}

/// <summary>
/// Slow procedure information
/// </summary>
public class SlowProcedureDto
{
    /// <summary>
    /// Procedure name
    /// </summary>
    public string ProcedureName { get; set; } = string.Empty;

    /// <summary>
    /// Performance score (0-100)
    /// </summary>
    public int PerformanceScore { get; set; }

    /// <summary>
    /// Severity level
    /// </summary>
    public string Severity { get; set; } = string.Empty;

    /// <summary>
    /// Number of issues found
    /// </summary>
    public int IssueCount { get; set; }

    /// <summary>
    /// Estimated execution time (if available)
    /// </summary>
    public double? EstimatedExecutionTimeMs { get; set; }
}

/// <summary>
/// Common problem statistics
/// </summary>
public class CommonProblemDto
{
    /// <summary>
    /// Problem type
    /// </summary>
    public string ProblemType { get; set; } = string.Empty;

    /// <summary>
    /// Number of occurrences
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Percentage of total issues
    /// </summary>
    public double Percentage { get; set; }

    /// <summary>
    /// Average severity of this problem type
    /// </summary>
    public string AverageSeverity { get; set; } = string.Empty;
}
