namespace SqlOptimizer.Application.DTOs;

/// <summary>
/// Request DTO for SQL rewrite comparison
/// </summary>
public class SQLRewriteRequestDto
{
    /// <summary>
    /// Original SQL code
    /// </summary>
    public string OriginalSQL { get; set; } = string.Empty;

    /// <summary>
    /// Optimized SQL code from AI
    /// </summary>
    public string OptimizedSQL { get; set; } = string.Empty;

    /// <summary>
    /// Analysis summary
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Performance score of original (0-100)
    /// </summary>
    public int? OriginalPerformanceScore { get; set; }

    /// <summary>
    /// List of issues found
    /// </summary>
    public List<AnalysisIssueDto> Issues { get; set; } = new();

    /// <summary>
    /// List of recommendations
    /// </summary>
    public List<RecommendationDto> Recommendations { get; set; } = new();
}
