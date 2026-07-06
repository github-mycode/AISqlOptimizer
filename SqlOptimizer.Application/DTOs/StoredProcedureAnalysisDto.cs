namespace SqlOptimizer.Application.DTOs;

/// <summary>
/// Analysis result for a stored procedure
/// </summary>
public class StoredProcedureAnalysisDto
{
    /// <summary>
    /// Success status of the analysis
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
    /// Overall performance score (0-100, higher is better)
    /// </summary>
    public int? PerformanceScore { get; set; }

    /// <summary>
    /// Severity level (Low, Medium, High, Critical)
    /// </summary>
    public string Severity { get; set; } = "Unknown";

    /// <summary>
    /// Summary of findings
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// List of identified issues
    /// </summary>
    public List<AnalysisIssueDto> Issues { get; set; } = new();

    /// <summary>
    /// List of recommendations
    /// </summary>
    public List<RecommendationDto> Recommendations { get; set; } = new();

    /// <summary>
    /// Optimized SQL code suggestion (if applicable)
    /// </summary>
    public string? OptimizedCode { get; set; }

    /// <summary>
    /// Error message if analysis failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Timestamp of analysis
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Individual issue found in the stored procedure
/// </summary>
public class AnalysisIssueDto
{
    /// <summary>
    /// Issue type (e.g., "Missing Index", "Table Scan", "SELECT *")
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Severity (Low, Medium, High, Critical)
    /// </summary>
    public string Severity { get; set; } = string.Empty;

    /// <summary>
    /// Description of the issue
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Line number in the code (if applicable)
    /// </summary>
    public int? LineNumber { get; set; }

    /// <summary>
    /// Code snippet showing the issue
    /// </summary>
    public string? CodeSnippet { get; set; }
}

/// <summary>
/// Recommendation for improving the stored procedure
/// </summary>
public class RecommendationDto
{
    /// <summary>
    /// Priority (High, Medium, Low)
    /// </summary>
    public string Priority { get; set; } = string.Empty;

    /// <summary>
    /// Title of the recommendation
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Expected impact (e.g., "30% performance improvement")
    /// </summary>
    public string? ExpectedImpact { get; set; }

    /// <summary>
    /// Implementation steps
    /// </summary>
    public List<string> ImplementationSteps { get; set; } = new();

    /// <summary>
    /// SQL code to implement the recommendation
    /// </summary>
    public string? SqlCode { get; set; }
}
