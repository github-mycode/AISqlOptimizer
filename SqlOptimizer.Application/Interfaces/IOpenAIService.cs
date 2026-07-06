using SqlOptimizer.Application.DTOs;

namespace SqlOptimizer.Application.Interfaces;

/// <summary>
/// Service interface for OpenAI API integration
/// </summary>
public interface IOpenAIService
{
    /// <summary>
    /// Analyzes a stored procedure using OpenAI and returns structured recommendations
    /// </summary>
    /// <param name="prompt">The detailed prompt for AI analysis</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Parsed analysis result</returns>
    Task<StoredProcedureAnalysisDto> AnalyzeStoredProcedureAsync(
        string prompt,
        CancellationToken cancellationToken = default);
}
