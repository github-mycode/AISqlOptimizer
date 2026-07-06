using SqlOptimizer.Application.DTOs;

namespace SqlOptimizer.Application.Interfaces;

/// <summary>
/// Service interface for generating reports
/// </summary>
public interface IReportService
{
    /// <summary>
    /// Generates an HTML report
    /// </summary>
    /// <param name="request">Report generation request</param>
    /// <returns>HTML content</returns>
    Task<string> GenerateHtmlReportAsync(GenerateReportRequestDto request);

    /// <summary>
    /// Generates a PDF report
    /// </summary>
    /// <param name="request">Report generation request</param>
    /// <returns>PDF content as byte array</returns>
    Task<byte[]> GeneratePdfReportAsync(GenerateReportRequestDto request);

    /// <summary>
    /// Creates a report request from analysis result
    /// </summary>
    /// <param name="analysis">Analysis result</param>
    /// <param name="originalSQL">Original SQL code (optional)</param>
    /// <returns>Report request DTO</returns>
    GenerateReportRequestDto CreateFromAnalysis(
        StoredProcedureAnalysisDto analysis, 
        string? originalSQL = null);
}
