using System.Text;
using Microsoft.Extensions.Logging;
using SqlOptimizer.Application.DTOs;
using SqlOptimizer.Application.Interfaces;

namespace SqlOptimizer.Application.Services;

/// <summary>
/// Service for building comprehensive AI prompts for stored procedure analysis
/// </summary>
public class PromptBuilderService : IPromptBuilderService
{
    private readonly IStoredProcedureService _storedProcedureService;
    private readonly IMetadataService _metadataService;
    private readonly IExecutionPlanService _executionPlanService;
    private readonly ILogger<PromptBuilderService> _logger;

    public PromptBuilderService(
        IStoredProcedureService storedProcedureService,
        IMetadataService metadataService,
        IExecutionPlanService executionPlanService,
        ILogger<PromptBuilderService> logger)
    {
        _storedProcedureService = storedProcedureService;
        _metadataService = metadataService;
        _executionPlanService = executionPlanService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> BuildAnalysisPromptAsync(
        AnalyzeStoredProcedureRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Building analysis prompt for stored procedure: {ProcedureName}",
            request.StoredProcedureName);

        var prompt = new StringBuilder();

        // Add system instructions
        prompt.AppendLine("You are an expert SQL Server performance analyst. Analyze the following stored procedure and provide a comprehensive performance analysis.");
        prompt.AppendLine();
        prompt.AppendLine("**IMPORTANT: Return your response ONLY as valid JSON matching this exact structure:**");
        prompt.AppendLine(@"{
  ""performanceScore"": <number 0-100>,
  ""severity"": ""<Low|Medium|High|Critical>"",
  ""summary"": ""<brief summary>"",
  ""issues"": [
    {
      ""type"": ""<issue type>"",
      ""severity"": ""<Low|Medium|High|Critical>"",
      ""description"": ""<description>"",
      ""lineNumber"": <number or null>,
      ""codeSnippet"": ""<code or null>""
    }
  ],
  ""recommendations"": [
    {
      ""priority"": ""<High|Medium|Low>"",
      ""title"": ""<title>"",
      ""description"": ""<description>"",
      ""expectedImpact"": ""<impact or null>"",
      ""implementationSteps"": [""<step1>"", ""<step2>""],
      ""sqlCode"": ""<sql or null>""
    }
  ],
  ""optimizedCode"": ""<optimized sql or null>""
}");
        prompt.AppendLine();
        prompt.AppendLine("---");
        prompt.AppendLine();

        // Add database context
        prompt.AppendLine($"**Database:** {request.Database}");
        prompt.AppendLine($"**Stored Procedure:** {request.StoredProcedureName}");
        prompt.AppendLine();

        // Get stored procedure details
        var procedureDetails = await GetProcedureDetailsAsync(request, cancellationToken);
        if (procedureDetails != null)
        {
            prompt.AppendLine("## Stored Procedure Definition");
            prompt.AppendLine("```sql");
            prompt.AppendLine(procedureDetails.Definition ?? "-- Definition not available");
            prompt.AppendLine("```");
            prompt.AppendLine();

            if (procedureDetails.Parameters.Any())
            {
                prompt.AppendLine("### Parameters");
                foreach (var param in procedureDetails.Parameters)
                {
                    var output = param.IsOutput ? " OUTPUT" : "";
                    var defaultVal = param.HasDefaultValue ? $" = {param.DefaultValue}" : "";
                    prompt.AppendLine($"- `{param.ParameterName}` {param.DataType}{output}{defaultVal}");
                }
                prompt.AppendLine();
            }
        }

        // Get table schemas for referenced tables
        if (procedureDetails?.ReferencedTables.Any() == true)
        {
            prompt.AppendLine("## Referenced Tables Schema");
            await AppendTableSchemasAsync(request, procedureDetails.ReferencedTables, prompt, cancellationToken);
            prompt.AppendLine();
        }

        // Get indexes for referenced tables
        if (procedureDetails?.ReferencedTables.Any() == true)
        {
            prompt.AppendLine("## Indexes");
            await AppendIndexesAsync(request, procedureDetails.ReferencedTables, prompt, cancellationToken);
            prompt.AppendLine();
        }

        // Get foreign keys
        if (procedureDetails?.ReferencedTables.Any() == true)
        {
            prompt.AppendLine("## Foreign Keys");
            await AppendForeignKeysAsync(request, procedureDetails.ReferencedTables, prompt, cancellationToken);
            prompt.AppendLine();
        }

        // Get execution plan
        if (request.IncludeExecutionPlan)
        {
            var executionPlan = await GetExecutionPlanAsync(request, cancellationToken);
            if (executionPlan != null && executionPlan.Success)
            {
                prompt.AppendLine("## Execution Plan Analysis");
                prompt.AppendLine($"- **Estimated Cost:** {executionPlan.EstimatedCost}");
                prompt.AppendLine($"- **Estimated Rows:** {executionPlan.EstimatedRows}");
                prompt.AppendLine();
                
                if (!string.IsNullOrEmpty(executionPlan.ExecutionPlanXml))
                {
                    prompt.AppendLine("### Execution Plan XML (Key Sections)");
                    prompt.AppendLine("```xml");
                    // Include first 5000 characters of execution plan to avoid token limits
                    var planPreview = executionPlan.ExecutionPlanXml.Length > 5000 
                        ? executionPlan.ExecutionPlanXml.Substring(0, 5000) + "... (truncated)"
                        : executionPlan.ExecutionPlanXml;
                    prompt.AppendLine(planPreview);
                    prompt.AppendLine("```");
                    prompt.AppendLine();
                }
            }
        }

        // Add analysis requirements
        prompt.AppendLine("## Analysis Requirements");
        prompt.AppendLine("Analyze the stored procedure for the following issues and provide specific recommendations:");
        prompt.AppendLine();
        prompt.AppendLine("### Performance Issues to Check:");
        prompt.AppendLine("1. **Missing Indexes** - Identify tables that would benefit from indexes");
        prompt.AppendLine("2. **SELECT *** - Check for SELECT * usage and recommend specific columns");
        prompt.AppendLine("3. **Table Scan** - Identify full table scans that could be optimized");
        prompt.AppendLine("4. **Index Scan** - Look for index scans that could be seeks");
        prompt.AppendLine("5. **Cursor Usage** - Check for cursors and suggest set-based alternatives");
        prompt.AppendLine("6. **Temp Tables** - Analyze temp table usage and suggest alternatives");
        prompt.AppendLine("7. **NOLOCK Hints** - Check for NOLOCK and warn about dirty reads");
        prompt.AppendLine("8. **Parameter Sniffing** - Identify potential parameter sniffing issues");
        prompt.AppendLine("9. **Implicit Conversion** - Check for data type mismatches");
        prompt.AppendLine("10. **Scalar Functions** - Identify inline scalar functions (performance killer)");
        prompt.AppendLine("11. **Covering Indexes** - Suggest covering indexes where beneficial");
        prompt.AppendLine("12. **Nested Queries** - Check for nested subqueries that could be JOINs");
        prompt.AppendLine("13. **Duplicate Joins** - Look for redundant join conditions");
        prompt.AppendLine("14. **Query Rewrite** - Suggest better ways to write the query");
        prompt.AppendLine();
        prompt.AppendLine("### Response Format:");
        prompt.AppendLine("- Assign a performance score (0-100, higher is better)");
        prompt.AppendLine("- Set overall severity (Low, Medium, High, Critical)");
        prompt.AppendLine("- List all identified issues with severity and descriptions");
        prompt.AppendLine("- Provide actionable recommendations with priority");
        prompt.AppendLine("- Include SQL code for implementing recommendations");
        prompt.AppendLine("- Suggest optimized version of the stored procedure if applicable");
        prompt.AppendLine();
        prompt.AppendLine("**Remember: Return ONLY valid JSON, no additional text or markdown formatting.**");

        var finalPrompt = prompt.ToString();
        _logger.LogDebug("Built prompt with length: {Length} characters", finalPrompt.Length);

        return finalPrompt;
    }

    /// <summary>
    /// Gets stored procedure details
    /// </summary>
    private async Task<StoredProcedureDetailDto?> GetProcedureDetailsAsync(
        AnalyzeStoredProcedureRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var procedureRequest = new StoredProcedureRequestDto
            {
                Server = request.Server,
                Database = request.Database,
                Username = request.Username,
                Password = request.Password,
                TrustServerCertificate = request.TrustServerCertificate,
                ProcedureName = request.StoredProcedureName
            };

            return await _storedProcedureService.GetStoredProcedureDetailAsync(
                procedureRequest, 
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get procedure details for {ProcedureName}", 
                request.StoredProcedureName);
            return null;
        }
    }

    /// <summary>
    /// Appends table schemas to the prompt
    /// </summary>
    private async Task AppendTableSchemasAsync(
        AnalyzeStoredProcedureRequestDto request,
        List<DependencyInfoDto> referencedTables,
        StringBuilder prompt,
        CancellationToken cancellationToken)
    {
        try
        {
            var metadataRequest = new MetadataRequestDto
            {
                Server = request.Server,
                Database = request.Database,
                Username = request.Username,
                Password = request.Password,
                TrustServerCertificate = request.TrustServerCertificate
            };

            var tables = await _metadataService.GetTablesAsync(metadataRequest, cancellationToken);

            foreach (var refTable in referencedTables.Where(t => t.ObjectType == "USER_TABLE").Take(10))
            {
                var tableInfo = tables.FirstOrDefault(t => 
                    t.SchemaName == refTable.SchemaName && 
                    t.TableName == refTable.ObjectName);

                if (tableInfo != null)
                {
                    prompt.AppendLine($"### {tableInfo.SchemaName}.{tableInfo.TableName}");
                    prompt.AppendLine($"- Type: {tableInfo.TableType}");
                    prompt.AppendLine($"- Row Count: {tableInfo.RowCount:N0}");
                    prompt.AppendLine($"- Total Space: {tableInfo.TotalSpaceKB / 1024:N2} MB");
                    prompt.AppendLine();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get table schemas");
        }
    }

    /// <summary>
    /// Appends indexes to the prompt
    /// </summary>
    private async Task AppendIndexesAsync(
        AnalyzeStoredProcedureRequestDto request,
        List<DependencyInfoDto> referencedTables,
        StringBuilder prompt,
        CancellationToken cancellationToken)
    {
        try
        {
            var metadataRequest = new MetadataRequestDto
            {
                Server = request.Server,
                Database = request.Database,
                Username = request.Username,
                Password = request.Password,
                TrustServerCertificate = request.TrustServerCertificate
            };

            var indexes = await _metadataService.GetIndexesAsync(metadataRequest, cancellationToken);

            var relevantIndexes = indexes.Where(idx =>
                referencedTables.Any(t =>
                    t.SchemaName == idx.SchemaName &&
                    t.ObjectName == idx.TableName)).ToList();

            if (relevantIndexes.Any())
            {
                foreach (var idx in relevantIndexes.Take(20))
                {
                    prompt.AppendLine($"- **{idx.SchemaName}.{idx.TableName}.{idx.IndexName}**");
                    prompt.AppendLine($"  - Type: {idx.IndexType}, Unique: {idx.IsUnique}, PK: {idx.IsPrimaryKey}");
                    prompt.AppendLine($"  - Columns: {idx.IndexedColumns}");
                    if (!string.IsNullOrEmpty(idx.IncludedColumns))
                    {
                        prompt.AppendLine($"  - Included: {idx.IncludedColumns}");
                    }
                    prompt.AppendLine();
                }
            }
            else
            {
                prompt.AppendLine("No indexes found for referenced tables.");
                prompt.AppendLine();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get indexes");
        }
    }

    /// <summary>
    /// Appends foreign keys to the prompt
    /// </summary>
    private async Task AppendForeignKeysAsync(
        AnalyzeStoredProcedureRequestDto request,
        List<DependencyInfoDto> referencedTables,
        StringBuilder prompt,
        CancellationToken cancellationToken)
    {
        try
        {
            var metadataRequest = new MetadataRequestDto
            {
                Server = request.Server,
                Database = request.Database,
                Username = request.Username,
                Password = request.Password,
                TrustServerCertificate = request.TrustServerCertificate
            };

            var foreignKeys = await _metadataService.GetForeignKeysAsync(metadataRequest, cancellationToken);

            var relevantFKs = foreignKeys.Where(fk =>
                referencedTables.Any(t =>
                    t.SchemaName == fk.SchemaName &&
                    t.ObjectName == fk.TableName)).ToList();

            if (relevantFKs.Any())
            {
                foreach (var fk in relevantFKs.Take(20))
                {
                    prompt.AppendLine($"- **{fk.ConstraintName}**");
                    prompt.AppendLine($"  - {fk.SchemaName}.{fk.TableName}({fk.ColumnNames}) → " +
                                    $"{fk.ReferencedSchemaName}.{fk.ReferencedTableName}({fk.ReferencedColumnNames})");
                    prompt.AppendLine($"  - Delete Rule: {fk.DeleteRule}, Update Rule: {fk.UpdateRule}");
                    prompt.AppendLine();
                }
            }
            else
            {
                prompt.AppendLine("No foreign keys found for referenced tables.");
                prompt.AppendLine();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get foreign keys");
        }
    }

    /// <summary>
    /// Gets execution plan
    /// </summary>
    private async Task<ExecutionPlanResponseDto?> GetExecutionPlanAsync(
        AnalyzeStoredProcedureRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var planRequest = new ExecutionPlanRequestDto
            {
                Server = request.Server,
                Database = request.Database,
                Username = request.Username,
                Password = request.Password,
                TrustServerCertificate = request.TrustServerCertificate,
                StoredProcedureName = request.StoredProcedureName,
                Parameters = request.Parameters
            };

            return await _executionPlanService.GetExecutionPlanAsync(planRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get execution plan for {ProcedureName}", 
                request.StoredProcedureName);
            return null;
        }
    }
}
