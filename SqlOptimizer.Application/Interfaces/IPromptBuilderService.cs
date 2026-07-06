using SqlOptimizer.Application.DTOs;

namespace SqlOptimizer.Application.Interfaces;

/// <summary>
/// Service interface for building AI prompts
/// </summary>
public interface IPromptBuilderService
{
    /// <summary>
    /// Builds a comprehensive AI prompt for stored procedure analysis
    /// </summary>
    /// <param name="request">Analysis request with connection details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Detailed prompt for AI analysis</returns>
    Task<string> BuildAnalysisPromptAsync(
        AnalyzeStoredProcedureRequestDto request,
        CancellationToken cancellationToken = default);
}
