using SqlOptimizer.Application.DTOs;

namespace SqlOptimizer.Application.Interfaces;

/// <summary>
/// Service interface for SQL rewrite and comparison
/// </summary>
public interface ISQLRewriteService
{
    /// <summary>
    /// Generates a side-by-side comparison of original and optimized SQL
    /// </summary>
    /// <param name="request">SQL rewrite request with original and optimized code</param>
    /// <returns>Comparison result with highlighted changes</returns>
    Task<SQLRewriteComparisonDto> GenerateComparisonAsync(SQLRewriteRequestDto request);

    /// <summary>
    /// Creates a SQL rewrite comparison from analysis result
    /// </summary>
    /// <param name="analysis">Analysis result containing original and optimized SQL</param>
    /// <returns>Comparison result with highlighted changes</returns>
    Task<SQLRewriteComparisonDto> CreateFromAnalysisAsync(StoredProcedureAnalysisDto analysis);
}
