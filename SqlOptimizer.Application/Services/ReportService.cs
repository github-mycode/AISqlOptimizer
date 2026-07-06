using System.Text;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SqlOptimizer.Application.DTOs;
using SqlOptimizer.Application.Interfaces;

namespace SqlOptimizer.Application.Services;

/// <summary>
/// Service for generating HTML and PDF reports
/// </summary>
public class ReportService : IReportService
{
    private readonly ILogger<ReportService> _logger;

    public ReportService(ILogger<ReportService> logger)
    {
        _logger = logger;
        
        // Configure QuestPDF license (Community license for free use)
        QuestPDF.Settings.License = LicenseType.Community;
    }

    /// <inheritdoc />
    public GenerateReportRequestDto CreateFromAnalysis(
        StoredProcedureAnalysisDto analysis, 
        string? originalSQL = null)
    {
        return new GenerateReportRequestDto
        {
            DatabaseName = analysis.DatabaseName,
            ServerName = analysis.StoredProcedureName.Split('.').FirstOrDefault() ?? "",
            ProcedureName = analysis.StoredProcedureName,
            PerformanceScore = analysis.PerformanceScore ?? 0,
            Severity = analysis.Severity,
            Summary = analysis.Summary,
            OriginalSQL = originalSQL,
            OptimizedSQL = analysis.OptimizedCode,
            Issues = analysis.Issues,
            Recommendations = analysis.Recommendations,
            AnalysisDate = analysis.Timestamp
        };
    }

    /// <inheritdoc />
    public Task<string> GenerateHtmlReportAsync(GenerateReportRequestDto request)
    {
        _logger.LogInformation(
            "Generating HTML report for procedure: {ProcedureName}",
            request.ProcedureName);

        var html = new StringBuilder();

        // HTML Header
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang='en'>");
        html.AppendLine("<head>");
        html.AppendLine("    <meta charset='UTF-8'>");
        html.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
        html.AppendLine("    <title>SQL Optimizer Report - " + EscapeHtml(request.ProcedureName) + "</title>");
        html.AppendLine(@"
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; padding: 20px; background: #f5f5f5; }
        .container { max-width: 1200px; margin: 0 auto; background: white; padding: 40px; box-shadow: 0 0 10px rgba(0,0,0,0.1); }
        h1 { color: #2c3e50; border-bottom: 3px solid #3498db; padding-bottom: 10px; margin-bottom: 30px; }
        h2 { color: #34495e; margin-top: 30px; margin-bottom: 15px; border-left: 4px solid #3498db; padding-left: 15px; }
        h3 { color: #7f8c8d; margin-top: 20px; margin-bottom: 10px; }
        .header-info { display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr)); gap: 20px; margin-bottom: 30px; }
        .info-card { background: #ecf0f1; padding: 15px; border-radius: 5px; }
        .info-label { font-weight: bold; color: #7f8c8d; font-size: 0.9em; }
        .info-value { font-size: 1.2em; color: #2c3e50; margin-top: 5px; }
        .score { font-size: 2em; font-weight: bold; }
        .score.excellent { color: #27ae60; }
        .score.good { color: #f39c12; }
        .score.poor { color: #e74c3c; }
        .severity { padding: 5px 15px; border-radius: 20px; display: inline-block; font-weight: bold; }
        .severity.low { background: #d4edda; color: #155724; }
        .severity.medium { background: #fff3cd; color: #856404; }
        .severity.high { background: #f8d7da; color: #721c24; }
        .severity.critical { background: #e74c3c; color: white; }
        .summary { background: #e8f4f8; padding: 20px; border-radius: 5px; border-left: 4px solid #3498db; margin: 20px 0; }
        .issue-list, .recommendation-list { margin: 20px 0; }
        .issue-item, .recommendation-item { background: #fff; border: 1px solid #ddd; padding: 15px; margin-bottom: 15px; border-radius: 5px; }
        .issue-item.critical { border-left: 4px solid #e74c3c; }
        .issue-item.high { border-left: 4px solid #f39c12; }
        .issue-item.medium { border-left: 4px solid #f1c40f; }
        .issue-item.low { border-left: 4px solid #27ae60; }
        .recommendation-item { border-left: 4px solid #3498db; }
        .code-block { background: #2c3e50; color: #ecf0f1; padding: 20px; border-radius: 5px; overflow-x: auto; font-family: 'Courier New', monospace; font-size: 0.9em; white-space: pre-wrap; margin: 15px 0; }
        .badge { padding: 3px 10px; border-radius: 3px; font-size: 0.85em; font-weight: bold; }
        .badge.priority-high { background: #e74c3c; color: white; }
        .badge.priority-medium { background: #f39c12; color: white; }
        .badge.priority-low { background: #27ae60; color: white; }
        .footer { margin-top: 40px; padding-top: 20px; border-top: 1px solid #ddd; text-align: center; color: #7f8c8d; font-size: 0.9em; }
        .sql-comparison { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; margin: 20px 0; }
        .sql-column h3 { margin-bottom: 10px; }
        @media (max-width: 768px) { .sql-comparison { grid-template-columns: 1fr; } }
    </style>
");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine("<div class='container'>");

        // Title
        html.AppendLine($"    <h1>SQL Optimizer Report</h1>");

        // Header Information
        html.AppendLine("    <div class='header-info'>");
        html.AppendLine("        <div class='info-card'>");
        html.AppendLine("            <div class='info-label'>Database</div>");
        html.AppendLine($"            <div class='info-value'>{EscapeHtml(request.DatabaseName)}</div>");
        html.AppendLine("        </div>");
        html.AppendLine("        <div class='info-card'>");
        html.AppendLine("            <div class='info-label'>Stored Procedure</div>");
        html.AppendLine($"            <div class='info-value'>{EscapeHtml(request.ProcedureName)}</div>");
        html.AppendLine("        </div>");
        html.AppendLine("        <div class='info-card'>");
        html.AppendLine("            <div class='info-label'>Performance Score</div>");
        var scoreClass = request.PerformanceScore >= 80 ? "excellent" : 
                        request.PerformanceScore >= 60 ? "good" : "poor";
        html.AppendLine($"            <div class='info-value score {scoreClass}'>{request.PerformanceScore}/100</div>");
        html.AppendLine("        </div>");
        html.AppendLine("        <div class='info-card'>");
        html.AppendLine("            <div class='info-label'>Severity</div>");
        html.AppendLine($"            <div class='info-value'><span class='severity {request.Severity.ToLower()}'>{EscapeHtml(request.Severity)}</span></div>");
        html.AppendLine("        </div>");
        html.AppendLine("    </div>");

        // Summary
        if (!string.IsNullOrEmpty(request.Summary))
        {
            html.AppendLine("    <div class='summary'>");
            html.AppendLine($"        <p>{EscapeHtml(request.Summary).Replace("\n", "<br>")}</p>");
            html.AppendLine("    </div>");
        }

        // Issues
        if (request.Issues.Any())
        {
            html.AppendLine($"    <h2>Issues Found ({request.Issues.Count})</h2>");
            html.AppendLine("    <div class='issue-list'>");
            foreach (var issue in request.Issues)
            {
                html.AppendLine($"        <div class='issue-item {issue.Severity.ToLower()}'>");
                html.AppendLine($"            <h3>{EscapeHtml(issue.Type)} <span class='severity {issue.Severity.ToLower()}'>{EscapeHtml(issue.Severity)}</span></h3>");
                html.AppendLine($"            <p>{EscapeHtml(issue.Description)}</p>");
                if (!string.IsNullOrEmpty(issue.CodeSnippet))
                {
                    html.AppendLine($"            <div class='code-block'>{EscapeHtml(issue.CodeSnippet)}</div>");
                }
                html.AppendLine("        </div>");
            }
            html.AppendLine("    </div>");
        }

        // Recommendations
        if (request.Recommendations.Any())
        {
            html.AppendLine($"    <h2>Recommendations ({request.Recommendations.Count})</h2>");
            html.AppendLine("    <div class='recommendation-list'>");
            foreach (var rec in request.Recommendations)
            {
                html.AppendLine("        <div class='recommendation-item'>");
                html.AppendLine($"            <h3>{EscapeHtml(rec.Title)} <span class='badge priority-{rec.Priority.ToLower()}'>{EscapeHtml(rec.Priority)}</span></h3>");
                html.AppendLine($"            <p>{EscapeHtml(rec.Description)}</p>");
                if (!string.IsNullOrEmpty(rec.ExpectedImpact))
                {
                    html.AppendLine($"            <p><strong>Expected Impact:</strong> {EscapeHtml(rec.ExpectedImpact)}</p>");
                }
                if (rec.ImplementationSteps.Any())
                {
                    html.AppendLine("            <p><strong>Implementation Steps:</strong></p>");
                    html.AppendLine("            <ol>");
                    foreach (var step in rec.ImplementationSteps)
                    {
                        html.AppendLine($"                <li>{EscapeHtml(step)}</li>");
                    }
                    html.AppendLine("            </ol>");
                }
                if (!string.IsNullOrEmpty(rec.SqlCode))
                {
                    html.AppendLine($"            <div class='code-block'>{EscapeHtml(rec.SqlCode)}</div>");
                }
                html.AppendLine("        </div>");
            }
            html.AppendLine("    </div>");
        }

        // SQL Comparison
        if (!string.IsNullOrEmpty(request.OriginalSQL) || !string.IsNullOrEmpty(request.OptimizedSQL))
        {
            html.AppendLine("    <h2>SQL Code Comparison</h2>");
            html.AppendLine("    <div class='sql-comparison'>");
            
            if (!string.IsNullOrEmpty(request.OriginalSQL))
            {
                html.AppendLine("        <div class='sql-column'>");
                html.AppendLine("            <h3>Original SQL</h3>");
                html.AppendLine($"            <div class='code-block'>{EscapeHtml(request.OriginalSQL)}</div>");
                html.AppendLine("        </div>");
            }
            
            if (!string.IsNullOrEmpty(request.OptimizedSQL))
            {
                html.AppendLine("        <div class='sql-column'>");
                html.AppendLine("            <h3>Optimized SQL</h3>");
                html.AppendLine($"            <div class='code-block'>{EscapeHtml(request.OptimizedSQL)}</div>");
                html.AppendLine("        </div>");
            }
            
            html.AppendLine("    </div>");
        }

        // Footer
        html.AppendLine("    <div class='footer'>");
        html.AppendLine($"        <p>Report generated on {request.AnalysisDate:yyyy-MM-dd HH:mm:ss} UTC</p>");
        html.AppendLine("        <p>SQL Optimizer - Powered by AI</p>");
        html.AppendLine("    </div>");

        html.AppendLine("</div>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        _logger.LogInformation("HTML report generated successfully");

        return Task.FromResult(html.ToString());
    }

    /// <inheritdoc />
    public Task<byte[]> GeneratePdfReportAsync(GenerateReportRequestDto request)
    {
        _logger.LogInformation(
            "Generating PDF report for procedure: {ProcedureName}",
            request.ProcedureName);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(50);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header()
                    .Height(100)
                    .Background(Colors.Blue.Lighten4)
                    .Padding(20)
                    .Column(column =>
                    {
                        column.Item().Text("SQL Optimizer Report")
                            .FontSize(24)
                            .Bold()
                            .FontColor(Colors.Blue.Darken2);
                        
                        column.Item().Text($"Database: {request.DatabaseName}")
                            .FontSize(12);
                        
                        column.Item().Text($"Generated: {request.AnalysisDate:yyyy-MM-dd HH:mm:ss}")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);
                    });

                page.Content()
                    .PaddingVertical(20)
                    .Column(column =>
                    {
                        // Database Information
                        column.Item().Element(container => DatabaseInfoSection(container, request));

                        column.Item().PaddingTop(20);

                        // Performance Score
                        column.Item().Element(container => PerformanceScoreSection(container, request));

                        column.Item().PaddingTop(20);

                        // Summary
                        if (!string.IsNullOrEmpty(request.Summary))
                        {
                            column.Item().Element(container => SummarySection(container, request));
                            column.Item().PaddingTop(20);
                        }

                        // Issues
                        if (request.Issues.Any())
                        {
                            column.Item().Element(container => IssuesSection(container, request));
                            column.Item().PaddingTop(20);
                        }

                        // Recommendations
                        if (request.Recommendations.Any())
                        {
                            column.Item().Element(container => RecommendationsSection(container, request));
                            column.Item().PaddingTop(20);
                        }

                        // SQL Code
                        if (!string.IsNullOrEmpty(request.OptimizedSQL))
                        {
                            column.Item().Element(container => SqlCodeSection(container, request));
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                        x.Span(" of ");
                        x.TotalPages();
                    });
            });
        });

        var pdfBytes = document.GeneratePdf();

        _logger.LogInformation("PDF report generated successfully ({Size} bytes)", pdfBytes.Length);

        return Task.FromResult(pdfBytes);
    }

    private void DatabaseInfoSection(IContainer container, GenerateReportRequestDto request)
    {
        container.Column(column =>
        {
            column.Item().Text("Database Information").FontSize(16).Bold();
            
            column.Item().PaddingTop(10).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(150);
                    columns.RelativeColumn();
                });

                table.Cell().Element(CellStyle).Text("Stored Procedure:").Bold();
                table.Cell().Element(CellStyle).Text(request.ProcedureName);

                table.Cell().Element(CellStyle).Text("Database:").Bold();
                table.Cell().Element(CellStyle).Text(request.DatabaseName);

                table.Cell().Element(CellStyle).Text("Analysis Date:").Bold();
                table.Cell().Element(CellStyle).Text(request.AnalysisDate.ToString("yyyy-MM-dd HH:mm:ss"));
            });
        });
    }

    private void PerformanceScoreSection(IContainer container, GenerateReportRequestDto request)
    {
        container.Column(column =>
        {
            column.Item().Text("Performance Analysis").FontSize(16).Bold();
            
            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Performance Score").FontSize(12).Bold();
                    var scoreColor = request.PerformanceScore >= 80 ? Colors.Green.Darken1 :
                                   request.PerformanceScore >= 60 ? Colors.Orange.Darken1 :
                                   Colors.Red.Darken1;
                    col.Item().Text($"{request.PerformanceScore}/100")
                        .FontSize(36)
                        .Bold()
                        .FontColor(scoreColor);
                });

                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Severity Level").FontSize(12).Bold();
                    col.Item().PaddingTop(5).Text(request.Severity)
                        .FontSize(18)
                        .Bold()
                        .FontColor(GetSeverityColor(request.Severity));
                });
            });
        });
    }

    private void SummarySection(IContainer container, GenerateReportRequestDto request)
    {
        container.Column(column =>
        {
            column.Item().Text("Summary").FontSize(16).Bold();
            column.Item().PaddingTop(10)
                .Background(Colors.Blue.Lighten4)
                .Padding(15)
                .Text(request.Summary)
                .FontSize(11);
        });
    }

    private void IssuesSection(IContainer container, GenerateReportRequestDto request)
    {
        container.Column(column =>
        {
            column.Item().Text($"Issues Found ({request.Issues.Count})").FontSize(16).Bold();

            foreach (var issue in request.Issues)
            {
                column.Item().PaddingTop(10).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                column.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text(issue.Type).FontSize(12).Bold();
                    row.ConstantItem(100).AlignRight().Text(issue.Severity)
                        .FontSize(10)
                        .FontColor(GetSeverityColor(issue.Severity));
                });
                column.Item().PaddingTop(5).Text(issue.Description).FontSize(10);
                
                if (!string.IsNullOrEmpty(issue.CodeSnippet))
                {
                    column.Item().PaddingTop(5)
                        .Background(Colors.Grey.Lighten3)
                        .Padding(10)
                        .Text(issue.CodeSnippet)
                        .FontSize(9)
                        .FontFamily("Courier New");
                }
            }
        });
    }

    private void RecommendationsSection(IContainer container, GenerateReportRequestDto request)
    {
        container.Column(column =>
        {
            column.Item().Text($"Recommendations ({request.Recommendations.Count})").FontSize(16).Bold();

            foreach (var rec in request.Recommendations)
            {
                column.Item().PaddingTop(10).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                column.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text(rec.Title).FontSize(12).Bold();
                    row.ConstantItem(100).AlignRight().Text(rec.Priority)
                        .FontSize(10)
                        .FontColor(Colors.Blue.Darken1);
                });
                column.Item().PaddingTop(5).Text(rec.Description).FontSize(10);

                if (!string.IsNullOrEmpty(rec.ExpectedImpact))
                {
                    column.Item().PaddingTop(5).Text($"Expected Impact: {rec.ExpectedImpact}")
                        .FontSize(10)
                        .Italic()
                        .FontColor(Colors.Green.Darken1);
                }

                if (!string.IsNullOrEmpty(rec.SqlCode))
                {
                    column.Item().PaddingTop(5)
                        .Background(Colors.Grey.Lighten3)
                        .Padding(10)
                        .Text(rec.SqlCode)
                        .FontSize(8)
                        .FontFamily("Courier New");
                }
            }
        });
    }

    private void SqlCodeSection(IContainer container, GenerateReportRequestDto request)
    {
        container.Column(column =>
        {
            column.Item().Text("Optimized SQL Code").FontSize(16).Bold();
            column.Item().PaddingTop(10)
                .Background(Colors.Grey.Lighten3)
                .Padding(15)
                .Text(request.OptimizedSQL ?? "")
                .FontSize(8)
                .FontFamily("Courier New");
        });
    }

    private IContainer CellStyle(IContainer container)
    {
        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
    }

    private string GetSeverityColor(string severity)
    {
        return severity.ToLowerInvariant() switch
        {
            "critical" => Colors.Red.Darken1,
            "high" => Colors.Orange.Darken1,
            "medium" => Colors.Yellow.Darken2,
            "low" => Colors.Green.Darken1,
            _ => Colors.Grey.Darken1
        };
    }

    private string EscapeHtml(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&#39;");
    }
}
