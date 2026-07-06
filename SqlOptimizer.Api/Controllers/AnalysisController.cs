using Microsoft.AspNetCore.Mvc;
using SqlOptimizer.Application.DTOs;
using SqlOptimizer.Application.Interfaces;

namespace SqlOptimizer.Api.Controllers;

/// <summary>
/// Controller for AI-powered stored procedure analysis
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AnalysisController : ControllerBase
{
    private readonly IPromptBuilderService _promptBuilderService;
    private readonly IOpenAIService _openAIService;
    private readonly ISQLRewriteService _sqlRewriteService;
    private readonly ILogger<AnalysisController> _logger;

    public AnalysisController(
        IPromptBuilderService promptBuilderService,
        IOpenAIService openAIService,
        ISQLRewriteService sqlRewriteService,
        ILogger<AnalysisController> logger)
    {
        _promptBuilderService = promptBuilderService;
        _openAIService = openAIService;
        _sqlRewriteService = sqlRewriteService;
        _logger = logger;
    }

    /// <summary>
    /// Analyzes a stored procedure using AI and provides optimization recommendations
    /// </summary>
    /// <param name="request">Analysis request with connection details and procedure name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Comprehensive analysis with issues and recommendations</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/analysis
    ///     {
    ///         "server": "localhost",
    ///         "database": "MyDatabase",
    ///         "username": "sa",
    ///         "password": "YourPassword123",
    ///         "trustServerCertificate": true,
    ///         "storedProcedureName": "dbo.sp_GetUserOrders",
    ///         "parameters": "{\"@UserId\": 123}",
    ///         "includeExecutionPlan": true
    ///     }
    /// 
    /// This endpoint uses OpenAI to analyze the stored procedure and provides:
    /// - Performance score (0-100)
    /// - List of identified issues (missing indexes, table scans, etc.)
    /// - Prioritized recommendations with implementation steps
    /// - Optimized SQL code suggestions
    /// 
    /// The analysis checks for:
    /// - Missing indexes
    /// - SELECT * usage
    /// - Table/Index scans
    /// - Cursor usage
    /// - Temp table usage
    /// - NOLOCK hints
    /// - Parameter sniffing
    /// - Implicit conversions
    /// - Scalar functions
    /// - Covering indexes
    /// - Nested queries
    /// - Duplicate joins
    /// - Query rewrite opportunities
    /// </remarks>
    /// <response code="200">Returns the analysis result</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="500">If an error occurs during analysis</response>
    [HttpPost]
    [HttpPost("storedprocedure")]
    [ProducesResponseType(typeof(StoredProcedureAnalysisDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<StoredProcedureAnalysisDto>> AnalyzeStoredProcedure(
        [FromBody] AnalyzeStoredProcedureRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Received analysis request for stored procedure: {ProcedureName} on database: {Database}",
            request.StoredProcedureName, request.Database);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid analysis request: {Errors}",
                string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

            return BadRequest(ModelState);
        }

        try
        {
            // Build the prompt with all context
            _logger.LogInformation("Building analysis prompt...");
            var prompt = await _promptBuilderService.BuildAnalysisPromptAsync(request, cancellationToken);

            _logger.LogDebug("Prompt built with {Length} characters", prompt.Length);

            // Call OpenAI for analysis
            _logger.LogInformation("Sending to OpenAI for analysis...");
            var analysis = await _openAIService.AnalyzeStoredProcedureAsync(prompt, cancellationToken);

            // Add metadata
            analysis.StoredProcedureName = request.StoredProcedureName;
            analysis.DatabaseName = request.Database;

            if (analysis.Success)
            {
                _logger.LogInformation(
                    "Analysis completed successfully. Performance Score: {Score}, Severity: {Severity}, Issues: {IssueCount}, Recommendations: {RecommendationCount}",
                    analysis.PerformanceScore, analysis.Severity, analysis.Issues.Count, analysis.Recommendations.Count);

                // Cache the analysis result for dashboard
                Application.Services.DashboardService.CacheAnalysisResult(
                    request.Server, 
                    request.Database, 
                    analysis);
                
                _logger.LogDebug("Analysis result cached for dashboard");
            }
            else
            {
                _logger.LogWarning("Analysis failed: {Error}", analysis.ErrorMessage);
            }

            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing stored procedure: {Message}", ex.Message);

            var errorResult = new StoredProcedureAnalysisDto
            {
                Success = false,
                StoredProcedureName = request.StoredProcedureName,
                DatabaseName = request.Database,
                ErrorMessage = $"An error occurred during analysis: {ex.Message}"
            };

            return Ok(errorResult);
        }
    }

    /// <summary>
    /// Analyzes all stored procedures in a database using parallel execution
    /// </summary>
    /// <param name="request">Database analysis request with connection details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Complete optimization report with aggregated statistics</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/analysis/database
    ///     {
    ///         "server": "localhost",
    ///         "database": "MyDatabase",
    ///         "username": "sa",
    ///         "password": "YourPassword123",
    ///         "trustServerCertificate": true,
    ///         "includeExecutionPlan": true,
    ///         "maxParallelism": 5,
    ///         "schemaFilter": "dbo"
    ///     }
    /// 
    /// This endpoint:
    /// - Retrieves all stored procedures from the database
    /// - Analyzes each procedure in parallel using Task.WhenAll
    /// - Aggregates results into a comprehensive report
    /// - Returns statistics: total procedures, critical/medium/low issues, average performance score
    /// 
    /// The parallel execution is controlled by maxParallelism to prevent resource exhaustion.
    /// </remarks>
    /// <response code="200">Returns the complete database analysis report</response>
    /// <response code="400">If the request is invalid</response>
    /// <response code="500">If an error occurs during analysis</response>
    [HttpPost("database")]
    [ProducesResponseType(typeof(DatabaseAnalysisReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DatabaseAnalysisReportDto>> AnalyzeDatabase(
        [FromBody] AnalyzeDatabaseRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        
        _logger.LogInformation(
            "Received database analysis request for database: {Database} on server: {Server}",
            request.Database, request.Server);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid database analysis request: {Errors}",
                string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

            return BadRequest(ModelState);
        }

        var report = new DatabaseAnalysisReportDto
        {
            DatabaseName = request.Database,
            ServerName = request.Server
        };

        try
        {
            // Step 1: Retrieve all stored procedures
            _logger.LogInformation("Retrieving stored procedures from {Database}...", request.Database);
            
            var metadataRequest = new MetadataRequestDto
            {
                Server = request.Server,
                Database = request.Database,
                Username = request.Username,
                Password = request.Password,
                TrustServerCertificate = request.TrustServerCertificate
            };

            var allProcedures = await RetrieveAllStoredProceduresAsync(metadataRequest, cancellationToken);

            // Filter by schema if specified
            if (!string.IsNullOrEmpty(request.SchemaFilter))
            {
                allProcedures = allProcedures
                    .Where(p => p.SchemaName.Equals(request.SchemaFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                    
                _logger.LogInformation(
                    "Filtered to {Count} procedures in schema: {Schema}", 
                    allProcedures.Count, request.SchemaFilter);
            }

            report.TotalProcedures = allProcedures.Count;
            _logger.LogInformation("Found {Count} stored procedures to analyze", allProcedures.Count);

            if (allProcedures.Count == 0)
            {
                report.Success = true;
                report.Summary = "No stored procedures found to analyze.";
                report.Duration = DateTime.UtcNow - startTime;
                return Ok(report);
            }

            // Step 2: Analyze each procedure in parallel
            _logger.LogInformation(
                "Starting parallel analysis with max parallelism: {MaxParallelism}", 
                request.MaxParallelism);

            var semaphore = new SemaphoreSlim(request.MaxParallelism, request.MaxParallelism);
            var analysisResults = new List<StoredProcedureAnalysisDto>();
            var failedAnalyses = new List<FailedAnalysisDto>();
            var lockObj = new object();

            var tasks = allProcedures.Select(async proc =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var procedureName = $"{proc.SchemaName}.{proc.ProcedureName}";
                    
                    _logger.LogDebug("Analyzing procedure: {ProcedureName}", procedureName);

                    var analysisRequest = new AnalyzeStoredProcedureRequestDto
                    {
                        Server = request.Server,
                        Database = request.Database,
                        Username = request.Username,
                        Password = request.Password,
                        TrustServerCertificate = request.TrustServerCertificate,
                        StoredProcedureName = procedureName,
                        IncludeExecutionPlan = request.IncludeExecutionPlan
                    };

                    // Build prompt
                    var prompt = await _promptBuilderService.BuildAnalysisPromptAsync(
                        analysisRequest, 
                        cancellationToken);

                    // Call OpenAI
                    var analysis = await _openAIService.AnalyzeStoredProcedureAsync(
                        prompt, 
                        cancellationToken);

                    analysis.StoredProcedureName = procedureName;
                    analysis.DatabaseName = request.Database;

                    lock (lockObj)
                    {
                        if (analysis.Success)
                        {
                            analysisResults.Add(analysis);
                            _logger.LogDebug(
                                "Completed analysis for {ProcedureName}: Score={Score}, Severity={Severity}",
                                procedureName, analysis.PerformanceScore, analysis.Severity);
                        }
                        else
                        {
                            failedAnalyses.Add(new FailedAnalysisDto
                            {
                                ProcedureName = procedureName,
                                ErrorMessage = analysis.ErrorMessage ?? "Unknown error"
                            });
                            _logger.LogWarning(
                                "Failed to analyze {ProcedureName}: {Error}",
                                procedureName, analysis.ErrorMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var procedureName = $"{proc.SchemaName}.{proc.ProcedureName}";
                    
                    lock (lockObj)
                    {
                        failedAnalyses.Add(new FailedAnalysisDto
                        {
                            ProcedureName = procedureName,
                            ErrorMessage = ex.Message
                        });
                    }
                    
                    _logger.LogError(ex, "Error analyzing procedure {ProcedureName}", procedureName);
                }
                finally
                {
                    semaphore.Release();
                }
            }).ToArray();

            // Wait for all analyses to complete
            await Task.WhenAll(tasks);

            _logger.LogInformation(
                "Completed parallel analysis. Successful: {Successful}, Failed: {Failed}",
                analysisResults.Count, failedAnalyses.Count);

            // Step 3: Aggregate results
            report.ProcedureAnalyses = analysisResults
                .OrderBy(a => a.PerformanceScore ?? 0)
                .ToList();
            
            report.FailedAnalyses = failedAnalyses;

            // Calculate statistics
            report.CriticalIssues = analysisResults.Count(a => 
                a.Severity.Equals("Critical", StringComparison.OrdinalIgnoreCase));
            
            report.HighIssues = analysisResults.Count(a => 
                a.Severity.Equals("High", StringComparison.OrdinalIgnoreCase));
            
            report.MediumIssues = analysisResults.Count(a => 
                a.Severity.Equals("Medium", StringComparison.OrdinalIgnoreCase));
            
            report.LowIssues = analysisResults.Count(a => 
                a.Severity.Equals("Low", StringComparison.OrdinalIgnoreCase));

            if (analysisResults.Any(a => a.PerformanceScore.HasValue))
            {
                var scores = analysisResults
                    .Where(a => a.PerformanceScore.HasValue)
                    .Select(a => a.PerformanceScore!.Value)
                    .ToList();

                report.AveragePerformanceScore = scores.Average();
                report.LowestPerformanceScore = scores.Min();
                report.HighestPerformanceScore = scores.Max();
            }

            // Generate summary
            report.Summary = GenerateSummary(report);
            report.Success = true;
            report.Duration = DateTime.UtcNow - startTime;

            // Cache all analysis results for dashboard
            if (analysisResults.Any())
            {
                Application.Services.DashboardService.CacheAnalysisResults(
                    request.Server,
                    request.Database,
                    analysisResults);
                
                _logger.LogInformation(
                    "Cached {Count} analysis results for dashboard",
                    analysisResults.Count);
            }

            _logger.LogInformation(
                "Database analysis completed. Total: {Total}, Critical: {Critical}, High: {High}, Medium: {Medium}, Low: {Low}, Avg Score: {AvgScore:F2}",
                report.TotalProcedures, report.CriticalIssues, report.HighIssues, 
                report.MediumIssues, report.LowIssues, report.AveragePerformanceScore);

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing database: {Message}", ex.Message);

            report.Success = false;
            report.ErrorMessage = $"An error occurred during database analysis: {ex.Message}";
            report.Duration = DateTime.UtcNow - startTime;

            return Ok(report);
        }
    }

    /// <summary>
    /// Retrieves all stored procedures from the database
    /// </summary>
    private async Task<List<StoredProcedureInfoDto>> RetrieveAllStoredProceduresAsync(
        MetadataRequestDto request,
        CancellationToken cancellationToken)
    {
        var metadataService = HttpContext.RequestServices
            .GetRequiredService<IMetadataService>();

        var procedures = await metadataService.GetStoredProceduresAsync(request, cancellationToken);
        
        return procedures.ToList();
    }

    /// <summary>
    /// Generates a summary of the database analysis
    /// </summary>
    private string GenerateSummary(DatabaseAnalysisReportDto report)
    {
        var summary = new System.Text.StringBuilder();

        summary.AppendLine($"Analyzed {report.TotalProcedures} stored procedures in database '{report.DatabaseName}'.");
        
        if (report.ProcedureAnalyses.Any())
        {
            summary.AppendLine($"Average Performance Score: {report.AveragePerformanceScore:F2}/100 " +
                             $"(Range: {report.LowestPerformanceScore}-{report.HighestPerformanceScore})");
        }

        if (report.CriticalIssues > 0)
        {
            summary.AppendLine($"⚠️ CRITICAL: {report.CriticalIssues} procedure(s) have critical performance issues requiring immediate attention.");
        }

        if (report.HighIssues > 0)
        {
            summary.AppendLine($"🔴 HIGH: {report.HighIssues} procedure(s) have high severity issues.");
        }

        if (report.MediumIssues > 0)
        {
            summary.AppendLine($"🟡 MEDIUM: {report.MediumIssues} procedure(s) have medium severity issues.");
        }

        if (report.LowIssues > 0)
        {
            summary.AppendLine($"🟢 LOW: {report.LowIssues} procedure(s) have low severity issues.");
        }

        if (report.FailedAnalyses.Any())
        {
            summary.AppendLine($"❌ {report.FailedAnalyses.Count} procedure(s) failed to analyze.");
        }

        var topIssues = report.ProcedureAnalyses
            .OrderBy(a => a.PerformanceScore ?? 100)
            .Take(3)
            .ToList();

        if (topIssues.Any())
        {
            summary.AppendLine();
            summary.AppendLine("Top procedures needing optimization:");
            foreach (var proc in topIssues)
            {
                summary.AppendLine($"  - {proc.StoredProcedureName}: Score {proc.PerformanceScore}/100 ({proc.Severity})");
            }
        }

        return summary.ToString().TrimEnd();
    }

    /// <summary>
    /// Generates a side-by-side comparison of original and optimized SQL code
    /// </summary>
    /// <param name="request">SQL rewrite request with original and optimized code</param>
    /// <returns>Detailed comparison with highlighted changes</returns>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST /api/analysis/rewrite
    ///     {
    ///         "originalSQL": "CREATE PROCEDURE dbo.sp_GetOrders AS BEGIN SELECT * FROM Orders END",
    ///         "optimizedSQL": "CREATE PROCEDURE dbo.sp_GetOrders AS BEGIN SET NOCOUNT ON; SELECT OrderId, OrderDate FROM Orders END",
    ///         "summary": "Optimized query performance",
    ///         "originalPerformanceScore": 65,
    ///         "issues": [...],
    ///         "recommendations": [...]
    ///     }
    /// 
    /// This endpoint:
    /// - Generates side-by-side comparison of original and optimized SQL
    /// - Highlights all changes (Added, Removed, Modified)
    /// - Provides improvement summary
    /// - Calculates estimated performance gain
    /// - Returns line-by-line comparison for UI display
    /// </remarks>
    /// <response code="200">Returns the SQL comparison result</response>
    /// <response code="400">If the request is invalid</response>
    [HttpPost("rewrite")]
    [ProducesResponseType(typeof(SQLRewriteComparisonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SQLRewriteComparisonDto>> CompareSQL(
        [FromBody] SQLRewriteRequestDto request)
    {
        _logger.LogInformation("Received SQL rewrite comparison request");

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(request.OriginalSQL) && string.IsNullOrWhiteSpace(request.OptimizedSQL))
        {
            return BadRequest("Both original and optimized SQL cannot be empty");
        }

        var comparison = await _sqlRewriteService.GenerateComparisonAsync(request);

        return Ok(comparison);
    }

    /// <summary>
    /// Generates SQL comparison from an analysis result
    /// </summary>
    /// <param name="analysis">The analysis result to generate comparison from</param>
    /// <returns>SQL comparison with highlighted changes</returns>
    /// <remarks>
    /// This endpoint takes an analysis result and extracts the SQL comparison.
    /// Use this after analyzing a stored procedure to get a formatted comparison view.
    /// </remarks>
    /// <response code="200">Returns the SQL comparison result</response>
    /// <response code="400">If the analysis is invalid</response>
    [HttpPost("rewrite/from-analysis")]
    [ProducesResponseType(typeof(SQLRewriteComparisonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SQLRewriteComparisonDto>> CompareFromAnalysis(
        [FromBody] StoredProcedureAnalysisDto analysis)
    {
        _logger.LogInformation(
            "Received SQL rewrite request from analysis for: {ProcedureName}",
            analysis.StoredProcedureName);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(analysis.OptimizedCode))
        {
            return BadRequest("Analysis does not contain optimized code");
        }

        var comparison = await _sqlRewriteService.CreateFromAnalysisAsync(analysis);

        return Ok(comparison);
    }
}

