using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SqlOptimizer.Application.DTOs;
using SqlOptimizer.Application.Interfaces;

namespace SqlOptimizer.Application.Services;

/// <summary>
/// Service for SQL rewrite and comparison
/// </summary>
public class SQLRewriteService : ISQLRewriteService
{
    private readonly ILogger<SQLRewriteService> _logger;

    public SQLRewriteService(ILogger<SQLRewriteService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<SQLRewriteComparisonDto> CreateFromAnalysisAsync(StoredProcedureAnalysisDto analysis)
    {
        _logger.LogInformation(
            "Creating SQL rewrite comparison for procedure: {ProcedureName}",
            analysis.StoredProcedureName);

        // Extract original SQL from the analysis (would come from stored procedure definition)
        // For now, we'll work with what we have in the optimized code
        var request = new SQLRewriteRequestDto
        {
            OriginalSQL = "-- Original SQL would be extracted from stored procedure definition",
            OptimizedSQL = analysis.OptimizedCode ?? string.Empty,
            Summary = analysis.Summary,
            OriginalPerformanceScore = analysis.PerformanceScore,
            Issues = analysis.Issues,
            Recommendations = analysis.Recommendations
        };

        return GenerateComparisonAsync(request);
    }

    /// <inheritdoc />
    public Task<SQLRewriteComparisonDto> GenerateComparisonAsync(SQLRewriteRequestDto request)
    {
        _logger.LogInformation("Generating SQL comparison");

        try
        {
            var comparison = new SQLRewriteComparisonDto
            {
                Success = true,
                OriginalSQL = request.OriginalSQL,
                OptimizedSQL = request.OptimizedSQL
            };

            // Generate improvement summary
            comparison.ImprovementSummary = GenerateImprovementSummary(request);

            // Calculate estimated gain
            comparison.EstimatedGain = CalculateEstimatedGain(request);

            // Identify changes
            comparison.Changes = IdentifyChanges(request);

            // Generate side-by-side comparison
            comparison.SideBySideComparison = GenerateSideBySideComparison(
                request.OriginalSQL, 
                request.OptimizedSQL);

            // Calculate statistics
            comparison.Statistics = CalculateStatistics(comparison.SideBySideComparison);

            _logger.LogInformation(
                "Comparison generated. Changes: {ChangeCount}, Lines Modified: {Modified}",
                comparison.Changes.Count, comparison.Statistics.LinesModified);

            return Task.FromResult(comparison);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SQL comparison: {Message}", ex.Message);

            return Task.FromResult(new SQLRewriteComparisonDto
            {
                Success = false,
                ErrorMessage = $"Error generating comparison: {ex.Message}",
                OriginalSQL = request.OriginalSQL,
                OptimizedSQL = request.OptimizedSQL
            });
        }
    }

    /// <summary>
    /// Generates an improvement summary
    /// </summary>
    private string GenerateImprovementSummary(SQLRewriteRequestDto request)
    {
        var summary = new StringBuilder();

        if (!string.IsNullOrEmpty(request.Summary))
        {
            summary.AppendLine(request.Summary);
            summary.AppendLine();
        }

        if (request.Issues.Any())
        {
            summary.AppendLine("### Issues Addressed:");
            foreach (var issue in request.Issues.Take(5))
            {
                summary.AppendLine($"- [{issue.Severity}] {issue.Type}: {issue.Description}");
            }
            summary.AppendLine();
        }

        if (request.Recommendations.Any())
        {
            summary.AppendLine("### Key Optimizations Applied:");
            foreach (var rec in request.Recommendations.Take(5))
            {
                summary.AppendLine($"- [{rec.Priority}] {rec.Title}");
                if (!string.IsNullOrEmpty(rec.ExpectedImpact))
                {
                    summary.AppendLine($"  Impact: {rec.ExpectedImpact}");
                }
            }
        }

        return summary.ToString().TrimEnd();
    }

    /// <summary>
    /// Calculates estimated performance gain
    /// </summary>
    private string? CalculateEstimatedGain(SQLRewriteRequestDto request)
    {
        // Aggregate estimated gains from recommendations
        var gains = new List<double>();

        foreach (var rec in request.Recommendations)
        {
            if (!string.IsNullOrEmpty(rec.ExpectedImpact))
            {
                // Try to extract percentage from impact string
                var match = Regex.Match(rec.ExpectedImpact, @"(\d+)%");
                if (match.Success && double.TryParse(match.Groups[1].Value, out var percentage))
                {
                    gains.Add(percentage);
                }
            }
        }

        if (gains.Any())
        {
            var totalGain = gains.Sum();
            var avgGain = gains.Average();

            return totalGain > 100 
                ? $"Up to {totalGain:F0}% cumulative improvement" 
                : $"Approximately {avgGain:F0}% performance improvement";
        }

        // Estimate based on performance score if available
        if (request.OriginalPerformanceScore.HasValue)
        {
            var estimatedImprovement = 100 - request.OriginalPerformanceScore.Value;
            if (estimatedImprovement > 10)
            {
                return $"Estimated {estimatedImprovement / 2}%-{estimatedImprovement}% improvement";
            }
        }

        return "Performance improvement expected";
    }

    /// <summary>
    /// Identifies specific changes between original and optimized SQL
    /// </summary>
    private List<SQLChangeDto> IdentifyChanges(SQLRewriteRequestDto request)
    {
        var changes = new List<SQLChangeDto>();

        // Analyze based on recommendations
        foreach (var rec in request.Recommendations)
        {
            var change = new SQLChangeDto
            {
                ChangeType = DetermineChangeType(rec.Title),
                Description = rec.Title,
                Impact = rec.Priority,
                NewSnippet = rec.SqlCode
            };

            changes.Add(change);
        }

        // Analyze based on issues
        foreach (var issue in request.Issues)
        {
            if (!changes.Any(c => c.Description.Contains(issue.Type, StringComparison.OrdinalIgnoreCase)))
            {
                var change = new SQLChangeDto
                {
                    ChangeType = "Optimized",
                    Description = $"Fixed: {issue.Type}",
                    OriginalLineNumber = issue.LineNumber,
                    OriginalSnippet = issue.CodeSnippet,
                    Impact = issue.Severity
                };

                changes.Add(change);
            }
        }

        // Add common SQL optimizations detected by pattern
        DetectCommonPatterns(request.OriginalSQL, request.OptimizedSQL, changes);

        return changes;
    }

    /// <summary>
    /// Determines change type from recommendation title
    /// </summary>
    private string DetermineChangeType(string title)
    {
        var titleLower = title.ToLowerInvariant();

        if (titleLower.Contains("add") || titleLower.Contains("create"))
            return "Added";
        if (titleLower.Contains("remove") || titleLower.Contains("delete"))
            return "Removed";
        if (titleLower.Contains("replace") || titleLower.Contains("rewrite"))
            return "Modified";
        
        return "Optimized";
    }

    /// <summary>
    /// Detects common SQL optimization patterns
    /// </summary>
    private void DetectCommonPatterns(string original, string optimized, List<SQLChangeDto> changes)
    {
        var originalLower = original.ToLowerInvariant();
        var optimizedLower = optimized.ToLowerInvariant();

        // SELECT * replaced with specific columns
        if (originalLower.Contains("select *") && !optimizedLower.Contains("select *"))
        {
            changes.Add(new SQLChangeDto
            {
                ChangeType = "Modified",
                Description = "Replaced SELECT * with specific columns",
                Impact = "Medium",
                OriginalSnippet = "SELECT *",
                NewSnippet = "SELECT [specific columns]"
            });
        }

        // Added SET NOCOUNT ON
        if (!originalLower.Contains("set nocount on") && optimizedLower.Contains("set nocount on"))
        {
            changes.Add(new SQLChangeDto
            {
                ChangeType = "Added",
                Description = "Added SET NOCOUNT ON for performance",
                Impact = "Low",
                NewSnippet = "SET NOCOUNT ON;"
            });
        }

        // Cursor replaced with set-based operation
        if (originalLower.Contains("cursor") && !optimizedLower.Contains("cursor"))
        {
            changes.Add(new SQLChangeDto
            {
                ChangeType = "Removed",
                Description = "Replaced cursor with set-based operation",
                Impact = "High",
                OriginalSnippet = "DECLARE CURSOR..."
            });
        }

        // Added index hints
        if (!originalLower.Contains("with (index") && optimizedLower.Contains("with (index"))
        {
            changes.Add(new SQLChangeDto
            {
                ChangeType = "Added",
                Description = "Added index hints for better performance",
                Impact = "Medium",
                NewSnippet = "WITH (INDEX(...))"
            });
        }
    }

    /// <summary>
    /// Generates side-by-side comparison with line-by-line highlighting
    /// </summary>
    private SideBySideComparisonDto GenerateSideBySideComparison(string originalSQL, string optimizedSQL)
    {
        var comparison = new SideBySideComparisonDto();

        var originalLines = SplitIntoLines(originalSQL);
        var optimizedLines = SplitIntoLines(optimizedSQL);

        // Simple line-by-line comparison (could be enhanced with diff algorithm)
        var maxLines = Math.Max(originalLines.Count, optimizedLines.Count);

        for (int i = 0; i < maxLines; i++)
        {
            var originalLine = i < originalLines.Count ? originalLines[i] : string.Empty;
            var optimizedLine = i < optimizedLines.Count ? optimizedLines[i] : string.Empty;

            var originalTrimmed = originalLine.Trim();
            var optimizedTrimmed = optimizedLine.Trim();

            // Determine highlight type
            var isChanged = !string.Equals(originalTrimmed, optimizedTrimmed, StringComparison.OrdinalIgnoreCase);

            if (i < originalLines.Count)
            {
                comparison.OriginalLines.Add(new CodeLineDto
                {
                    LineNumber = i + 1,
                    Content = originalLine,
                    HighlightType = DetermineHighlightType(originalTrimmed, optimizedTrimmed, true),
                    IsChanged = isChanged
                });
            }

            if (i < optimizedLines.Count)
            {
                comparison.OptimizedLines.Add(new CodeLineDto
                {
                    LineNumber = i + 1,
                    Content = optimizedLine,
                    HighlightType = DetermineHighlightType(originalTrimmed, optimizedTrimmed, false),
                    IsChanged = isChanged
                });
            }
        }

        return comparison;
    }

    /// <summary>
    /// Determines highlight type for a line
    /// </summary>
    private string DetermineHighlightType(string originalLine, string optimizedLine, bool isOriginal)
    {
        if (string.IsNullOrWhiteSpace(originalLine) && !string.IsNullOrWhiteSpace(optimizedLine))
            return isOriginal ? "None" : "Added";

        if (!string.IsNullOrWhiteSpace(originalLine) && string.IsNullOrWhiteSpace(optimizedLine))
            return isOriginal ? "Removed" : "None";

        if (!string.Equals(originalLine, optimizedLine, StringComparison.OrdinalIgnoreCase))
            return "Modified";

        return "None";
    }

    /// <summary>
    /// Splits SQL into lines
    /// </summary>
    private List<string> SplitIntoLines(string sql)
    {
        if (string.IsNullOrEmpty(sql))
            return new List<string>();

        return sql.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();
    }

    /// <summary>
    /// Calculates statistics about the changes
    /// </summary>
    private ChangeStatisticsDto CalculateStatistics(SideBySideComparisonDto comparison)
    {
        var stats = new ChangeStatisticsDto
        {
            OriginalLineCount = comparison.OriginalLines.Count,
            OptimizedLineCount = comparison.OptimizedLines.Count
        };

        stats.LinesAdded = comparison.OptimizedLines.Count(l => l.HighlightType == "Added");
        stats.LinesRemoved = comparison.OriginalLines.Count(l => l.HighlightType == "Removed");
        stats.LinesModified = comparison.OriginalLines.Count(l => l.HighlightType == "Modified");

        var totalChanges = stats.LinesAdded + stats.LinesRemoved + stats.LinesModified;
        var totalLines = Math.Max(stats.OriginalLineCount, 1);
        
        stats.PercentageChanged = (double)totalChanges / totalLines * 100;

        return stats;
    }
}
