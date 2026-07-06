using Microsoft.AspNetCore.Mvc;
using SqlOptimizer.Application.DTOs;
using SqlOptimizer.Application.Interfaces;

namespace SqlOptimizer.Api.Controllers;

/// <summary>
/// Controller for generating reports
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReportController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportController> _logger;

    public ReportController(
        IReportService reportService,
        ILogger<ReportController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    /// <summary>
    /// Generates an HTML report
    /// </summary>
    /// <param name="request">Report generation request</param>
    /// <returns>HTML content</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/report/html
    ///     {
    ///         "databaseName": "MyDatabase",
    ///         "procedureName": "dbo.sp_GetUserOrders",
    ///         "performanceScore": 65,
    ///         "severity": "Medium",
    ///         "summary": "Several performance issues identified...",
    ///         "originalSQL": "...",
    ///         "optimizedSQL": "...",
    ///         "issues": [...],
    ///         "recommendations": [...]
    ///     }
    /// 
    /// This endpoint generates a comprehensive HTML report that can be:
    /// - Viewed directly in a browser
    /// - Saved as an HTML file
    /// - Embedded in web applications
    /// 
    /// The report includes:
    /// - Database information
    /// - Performance score and severity
    /// - Summary of findings
    /// - List of issues with code snippets
    /// - Recommendations with implementation steps
    /// - Side-by-side SQL comparison
    /// </remarks>
    /// <response code="200">Returns the HTML report as text/html</response>
    /// <response code="400">If the request is invalid</response>
    [HttpPost("html")]
    [Produces("text/html")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK, "text/html")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateHtmlReport([FromBody] GenerateReportRequestDto request)
    {
        _logger.LogInformation(
            "Received HTML report request for procedure: {ProcedureName}",
            request.ProcedureName);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var html = await _reportService.GenerateHtmlReportAsync(request);

        return Content(html, "text/html");
    }

    /// <summary>
    /// Generates a PDF report
    /// </summary>
    /// <param name="request">Report generation request</param>
    /// <returns>PDF file</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     GET /api/report/pdf
    ///     {
    ///         "databaseName": "MyDatabase",
    ///         "procedureName": "dbo.sp_GetUserOrders",
    ///         "performanceScore": 65,
    ///         "severity": "Medium",
    ///         "summary": "Several performance issues identified...",
    ///         "originalSQL": "...",
    ///         "optimizedSQL": "...",
    ///         "issues": [...],
    ///         "recommendations": [...]
    ///     }
    /// 
    /// This endpoint generates a professional PDF report using QuestPDF that includes:
    /// - Header with database information
    /// - Performance score with color coding
    /// - Severity level indicator
    /// - Summary section
    /// - Detailed issues list
    /// - Prioritized recommendations
    /// - Optimized SQL code
    /// - Page numbers and footer
    /// 
    /// The PDF is optimized for printing and archiving.
    /// </remarks>
    /// <response code="200">Returns the PDF report as application/pdf</response>
    /// <response code="400">If the request is invalid</response>
    [HttpPost("pdf")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK, "application/pdf")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GeneratePdfReport([FromBody] GenerateReportRequestDto request)
    {
        _logger.LogInformation(
            "Received PDF report request for procedure: {ProcedureName}",
            request.ProcedureName);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var pdfBytes = await _reportService.GeneratePdfReportAsync(request);

        var fileName = $"SQL_Optimizer_Report_{request.ProcedureName.Replace(".", "_")}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }

    /// <summary>
    /// Generates an HTML report from an analysis result
    /// </summary>
    /// <param name="analysis">Analysis result</param>
    /// <returns>HTML content</returns>
    /// <remarks>
    /// This endpoint takes an analysis result and generates an HTML report.
    /// Use this after analyzing a stored procedure to quickly generate a report.
    /// </remarks>
    /// <response code="200">Returns the HTML report</response>
    /// <response code="400">If the analysis is invalid</response>
    [HttpPost("html/from-analysis")]
    [Produces("text/html")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK, "text/html")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateHtmlFromAnalysis([FromBody] StoredProcedureAnalysisDto analysis)
    {
        _logger.LogInformation(
            "Received HTML report request from analysis for: {ProcedureName}",
            analysis.StoredProcedureName);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var request = _reportService.CreateFromAnalysis(analysis);
        var html = await _reportService.GenerateHtmlReportAsync(request);

        return Content(html, "text/html");
    }

    /// <summary>
    /// Generates a PDF report from an analysis result
    /// </summary>
    /// <param name="analysis">Analysis result</param>
    /// <returns>PDF file</returns>
    /// <remarks>
    /// This endpoint takes an analysis result and generates a PDF report.
    /// Use this after analyzing a stored procedure to quickly generate a downloadable PDF.
    /// </remarks>
    /// <response code="200">Returns the PDF report</response>
    /// <response code="400">If the analysis is invalid</response>
    [HttpPost("pdf/from-analysis")]
    [Produces("application/pdf")]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK, "application/pdf")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GeneratePdfFromAnalysis([FromBody] StoredProcedureAnalysisDto analysis)
    {
        _logger.LogInformation(
            "Received PDF report request from analysis for: {ProcedureName}",
            analysis.StoredProcedureName);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var request = _reportService.CreateFromAnalysis(analysis);
        var pdfBytes = await _reportService.GeneratePdfReportAsync(request);

        var fileName = $"SQL_Optimizer_Report_{analysis.StoredProcedureName.Replace(".", "_")}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }
}
