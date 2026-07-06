using Microsoft.Extensions.Logging;
using SqlOptimizer.Application.DTOs;
using SqlOptimizer.Application.Interfaces;
using System.Diagnostics;

namespace SqlOptimizer.Application.Services;

/// <summary>
/// Service for dashboard operations
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IMetadataService _metadataService;
    private readonly ILogger<DashboardService> _logger;
    
    // In-memory cache for analysis results (in production, use Redis or SQL database)
    private static readonly Dictionary<string, List<StoredProcedureAnalysisDto>> _analysisCache = new();
    private static readonly object _cacheLock = new();

    public DashboardService(
        IMetadataService metadataService,
        ILogger<DashboardService> logger)
    {
        _metadataService = metadataService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<DashboardOverviewDto> GetDashboardOverviewAsync(
        DashboardRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        _logger.LogInformation(
            "Fetching dashboard overview for database: {DatabaseName}",
            request.DatabaseName);

        try
        {
            var metadataRequest = new MetadataRequestDto
            {
                DatabaseType = request.DatabaseType,
                Server = request.ServerName,
                Database = request.DatabaseName,
                Username = request.UseWindowsAuth ? null : request.Username,
                Password = request.UseWindowsAuth ? null : request.Password,
                TrustServerCertificate = true
            };

            // Fetch metadata in parallel
            var storedProceduresTask = _metadataService.GetStoredProceduresAsync(metadataRequest, cancellationToken);
            var tablesTask = _metadataService.GetTablesAsync(metadataRequest, cancellationToken);
            var viewsTask = _metadataService.GetViewsAsync(metadataRequest, cancellationToken);
            var indexesTask = _metadataService.GetIndexesAsync(metadataRequest, cancellationToken);

            await Task.WhenAll(storedProceduresTask, tablesTask, viewsTask, indexesTask);

            var storedProcedures = await storedProceduresTask;
            var tables = await tablesTask;
            var views = await viewsTask;
            var indexes = await indexesTask;

            _logger.LogDebug(
                "Metadata fetched: {ProcCount} procedures, {TableCount} tables, {ViewCount} views, {IndexCount} indexes",
                storedProcedures.Count(),
                tables.Count(),
                views.Count(),
                indexes.Count());

            // Get cached analysis results
            var cacheKey = $"{request.ServerName}_{request.DatabaseName}";
            List<StoredProcedureAnalysisDto> analysisResults;
            
            lock (_cacheLock)
            {
                _analysisCache.TryGetValue(cacheKey, out analysisResults!);
            }

            var dashboard = new DashboardOverviewDto
            {
                DatabaseName = request.DatabaseName,
                ServerName = request.ServerName,
                StoredProcedureCount = storedProcedures.Count(),
                TableCount = tables.Count(),
                ViewCount = views.Count(),
                IndexCount = indexes.Count()
            };

            // If we have analysis results, calculate performance metrics
            if (analysisResults != null && analysisResults.Any())
            {
                _logger.LogDebug("Processing {Count} cached analysis results", analysisResults.Count);

                dashboard.AnalyzedProcedureCount = analysisResults.Count;
                dashboard.LastAnalysisDate = analysisResults.Max(a => a.Timestamp);

                // Calculate average performance score
                var scoresWithValue = analysisResults.Where(a => a.PerformanceScore.HasValue).ToList();
                if (scoresWithValue.Any())
                {
                    dashboard.AveragePerformanceScore = Math.Round(
                        scoresWithValue.Average(a => a.PerformanceScore!.Value), 2);
                }

                // Count issues by severity
                var allIssues = analysisResults.SelectMany(a => a.Issues).ToList();
                dashboard.CriticalIssuesCount = allIssues.Count(i => i.Severity.Equals("Critical", StringComparison.OrdinalIgnoreCase));
                dashboard.HighIssuesCount = allIssues.Count(i => i.Severity.Equals("High", StringComparison.OrdinalIgnoreCase));
                dashboard.MediumIssuesCount = allIssues.Count(i => i.Severity.Equals("Medium", StringComparison.OrdinalIgnoreCase));
                dashboard.LowIssuesCount = allIssues.Count(i => i.Severity.Equals("Low", StringComparison.OrdinalIgnoreCase));

                // Get top 10 slowest procedures (lowest performance scores)
                dashboard.Top10SlowProcedures = analysisResults
                    .Where(a => a.PerformanceScore.HasValue)
                    .OrderBy(a => a.PerformanceScore!.Value)
                    .Take(10)
                    .Select(a => new SlowProcedureDto
                    {
                        ProcedureName = a.StoredProcedureName,
                        PerformanceScore = a.PerformanceScore!.Value,
                        Severity = a.Severity,
                        IssueCount = a.Issues.Count,
                        EstimatedExecutionTimeMs = null // Could be enhanced with execution plan data
                    })
                    .ToList();

                // Get most common problems
                var problemGroups = allIssues
                    .GroupBy(i => i.Type)
                    .Select(g => new
                    {
                        Type = g.Key,
                        Count = g.Count(),
                        Severities = g.Select(i => i.Severity).ToList()
                    })
                    .OrderByDescending(g => g.Count)
                    .Take(10)
                    .ToList();

                var totalIssues = allIssues.Count;
                dashboard.MostCommonProblems = problemGroups.Select(g => new CommonProblemDto
                {
                    ProblemType = g.Type,
                    Count = g.Count,
                    Percentage = totalIssues > 0 ? Math.Round((g.Count * 100.0) / totalIssues, 2) : 0,
                    AverageSeverity = GetMostCommonSeverity(g.Severities)
                })
                .ToList();

                _logger.LogInformation(
                    "Dashboard metrics calculated: Avg Score: {AvgScore}, Critical: {Critical}, High: {High}, Medium: {Medium}, Low: {Low}",
                    dashboard.AveragePerformanceScore,
                    dashboard.CriticalIssuesCount,
                    dashboard.HighIssuesCount,
                    dashboard.MediumIssuesCount,
                    dashboard.LowIssuesCount);
            }
            else
            {
                _logger.LogWarning("No cached analysis results found for database: {DatabaseName}", request.DatabaseName);
                dashboard.AnalyzedProcedureCount = 0;
                dashboard.AveragePerformanceScore = 0;
            }

            stopwatch.Stop();
            _logger.LogInformation(
                "Dashboard overview retrieved in {ElapsedMs}ms for database: {DatabaseName}",
                stopwatch.ElapsedMilliseconds,
                request.DatabaseName);

            return dashboard;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "Failed to retrieve dashboard overview after {ElapsedMs}ms for database: {DatabaseName}",
                stopwatch.ElapsedMilliseconds,
                request.DatabaseName);
            throw;
        }
    }

    /// <summary>
    /// Stores analysis result in cache (called from AnalysisController after analysis)
    /// </summary>
    public static void CacheAnalysisResult(string serverName, string databaseName, StoredProcedureAnalysisDto analysis)
    {
        var cacheKey = $"{serverName}_{databaseName}";
        
        lock (_cacheLock)
        {
            if (!_analysisCache.ContainsKey(cacheKey))
            {
                _analysisCache[cacheKey] = new List<StoredProcedureAnalysisDto>();
            }

            // Remove old analysis for same procedure if exists
            _analysisCache[cacheKey].RemoveAll(a => 
                a.StoredProcedureName.Equals(analysis.StoredProcedureName, StringComparison.OrdinalIgnoreCase));

            // Add new analysis
            _analysisCache[cacheKey].Add(analysis);
        }
    }

    /// <summary>
    /// Stores multiple analysis results in cache (called from database-wide analysis)
    /// </summary>
    public static void CacheAnalysisResults(string serverName, string databaseName, List<StoredProcedureAnalysisDto> analyses)
    {
        var cacheKey = $"{serverName}_{databaseName}";
        
        lock (_cacheLock)
        {
            _analysisCache[cacheKey] = analyses;
        }
    }

    /// <summary>
    /// Clears analysis cache for a specific database
    /// </summary>
    public static void ClearCache(string serverName, string databaseName)
    {
        var cacheKey = $"{serverName}_{databaseName}";
        
        lock (_cacheLock)
        {
            _analysisCache.Remove(cacheKey);
        }
    }

    private static string GetMostCommonSeverity(List<string> severities)
    {
        if (!severities.Any()) return "Unknown";

        var severityOrder = new Dictionary<string, int>
        {
            { "Critical", 4 },
            { "High", 3 },
            { "Medium", 2 },
            { "Low", 1 }
        };

        // Group and count severities
        var grouped = severities
            .GroupBy(s => s, StringComparer.OrdinalIgnoreCase)
            .OrderByDescending(g => g.Count())
            .ThenByDescending(g => severityOrder.GetValueOrDefault(g.Key, 0))
            .FirstOrDefault();

        return grouped?.Key ?? "Unknown";
    }
}
