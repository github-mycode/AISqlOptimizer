namespace SqlOptimizer.Application.DTOs;

/// <summary>
/// SQL rewrite comparison result
/// </summary>
public class SQLRewriteComparisonDto
{
    /// <summary>
    /// Success status
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Original SQL code
    /// </summary>
    public string OriginalSQL { get; set; } = string.Empty;

    /// <summary>
    /// Optimized SQL code
    /// </summary>
    public string OptimizedSQL { get; set; } = string.Empty;

    /// <summary>
    /// Improvement summary
    /// </summary>
    public string ImprovementSummary { get; set; } = string.Empty;

    /// <summary>
    /// Estimated performance gain percentage
    /// </summary>
    public string? EstimatedGain { get; set; }

    /// <summary>
    /// List of changes made
    /// </summary>
    public List<SQLChangeDto> Changes { get; set; } = new();

    /// <summary>
    /// Side-by-side comparison (line by line)
    /// </summary>
    public SideBySideComparisonDto SideBySideComparison { get; set; } = new();

    /// <summary>
    /// Statistics about the changes
    /// </summary>
    public ChangeStatisticsDto Statistics { get; set; } = new();

    /// <summary>
    /// Error message if operation failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Individual SQL change
/// </summary>
public class SQLChangeDto
{
    /// <summary>
    /// Type of change (Added, Removed, Modified, Optimized)
    /// </summary>
    public string ChangeType { get; set; } = string.Empty;

    /// <summary>
    /// Description of the change
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Line number in original SQL (if applicable)
    /// </summary>
    public int? OriginalLineNumber { get; set; }

    /// <summary>
    /// Original code snippet
    /// </summary>
    public string? OriginalSnippet { get; set; }

    /// <summary>
    /// New code snippet
    /// </summary>
    public string? NewSnippet { get; set; }

    /// <summary>
    /// Impact level (High, Medium, Low)
    /// </summary>
    public string Impact { get; set; } = string.Empty;
}

/// <summary>
/// Side-by-side comparison of SQL code
/// </summary>
public class SideBySideComparisonDto
{
    /// <summary>
    /// Original SQL lines with highlighting
    /// </summary>
    public List<CodeLineDto> OriginalLines { get; set; } = new();

    /// <summary>
    /// Optimized SQL lines with highlighting
    /// </summary>
    public List<CodeLineDto> OptimizedLines { get; set; } = new();
}

/// <summary>
/// Individual line of code with highlighting
/// </summary>
public class CodeLineDto
{
    /// <summary>
    /// Line number
    /// </summary>
    public int LineNumber { get; set; }

    /// <summary>
    /// Code content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Highlight type (None, Added, Removed, Modified)
    /// </summary>
    public string HighlightType { get; set; } = "None";

    /// <summary>
    /// Is this line a change?
    /// </summary>
    public bool IsChanged { get; set; }
}

/// <summary>
/// Statistics about the changes
/// </summary>
public class ChangeStatisticsDto
{
    /// <summary>
    /// Total number of lines in original
    /// </summary>
    public int OriginalLineCount { get; set; }

    /// <summary>
    /// Total number of lines in optimized
    /// </summary>
    public int OptimizedLineCount { get; set; }

    /// <summary>
    /// Number of lines added
    /// </summary>
    public int LinesAdded { get; set; }

    /// <summary>
    /// Number of lines removed
    /// </summary>
    public int LinesRemoved { get; set; }

    /// <summary>
    /// Number of lines modified
    /// </summary>
    public int LinesModified { get; set; }

    /// <summary>
    /// Percentage of code changed
    /// </summary>
    public double PercentageChanged { get; set; }
}
