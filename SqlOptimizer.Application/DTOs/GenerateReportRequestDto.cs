namespace SqlOptimizer.Application.DTOs;

/// <summary>
/// Request DTO for generating reports
/// </summary>
public class GenerateReportRequestDto
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
    /// Stored procedure name
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
    /// Summary of findings
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Original SQL code
    /// </summary>
    public string? OriginalSQL { get; set; }

    /// <summary>
    /// Optimized SQL code
    /// </summary>
    public string? OptimizedSQL { get; set; }

    /// <summary>
    /// List of issues
    /// </summary>
    public List<AnalysisIssueDto> Issues { get; set; } = new();

    /// <summary>
    /// List of recommendations
    /// </summary>
    public List<RecommendationDto> Recommendations { get; set; } = new();

    /// <summary>
    /// Analysis timestamp
    /// </summary>
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
}
