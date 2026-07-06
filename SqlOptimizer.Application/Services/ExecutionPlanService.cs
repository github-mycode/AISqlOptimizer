using System.Data;
using System.Text;
using System.Xml.Linq;
using Dapper;
using Microsoft.Extensions.Logging;
using SqlOptimizer.Application.DTOs;
using SqlOptimizer.Application.Interfaces;
using SqlOptimizer.Domain.Enums;

namespace SqlOptimizer.Application.Services;

/// <summary>
/// Service for retrieving SQL execution plans for stored procedures
/// </summary>
public class ExecutionPlanService : IExecutionPlanService
{
    private readonly IDatabaseProvider _sqlServerProvider;
    private readonly IDatabaseProvider _mySqlProvider;
    private readonly ILogger<ExecutionPlanService> _logger;

    public ExecutionPlanService(
        IEnumerable<IDatabaseProvider> providers,
        ILogger<ExecutionPlanService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        var providerList = providers.ToList();
        _sqlServerProvider = providerList.FirstOrDefault(p => p.GetType().Name.Contains("SqlServer"))
            ?? throw new InvalidOperationException("SqlServerProvider not registered");
        _mySqlProvider = providerList.FirstOrDefault(p => p.GetType().Name.Contains("MySql"))
            ?? throw new InvalidOperationException("MySqlProvider not registered");
    }

    /// <inheritdoc />
    public async Task<ExecutionPlanResponseDto> GetExecutionPlanAsync(
        ExecutionPlanRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting execution plan for stored procedure: {ProcedureName} on server: {Server}, database: {Database}",
            request.StoredProcedureName, request.Server, request.Database);

        var response = new ExecutionPlanResponseDto
        {
            StoredProcedureName = request.StoredProcedureName,
            DatabaseName = request.Database
        };

        try
        {
            // Get the provider based on database type
            var provider = GetProvider(request.DatabaseType);
            
            var connectionString = provider.BuildConnectionString(
                request.Server,
                request.Database,
                request.Username,
                request.Password,
                request.TrustServerCertificate);

            using var connection = provider.CreateConnection(connectionString);

            // Get the execution plan XML
            var executionPlanXml = await GetExecutionPlanXmlAsync(
                connection,
                request.StoredProcedureName,
                request.Parameters,
                cancellationToken);

            response.ExecutionPlanXml = executionPlanXml;

            // Parse XML to extract cost and row estimates
            if (!string.IsNullOrEmpty(executionPlanXml))
            {
                ExtractPlanMetadata(executionPlanXml, response);
            }

            response.Success = true;

            _logger.LogInformation(
                "Successfully retrieved execution plan for {ProcedureName}. Estimated cost: {Cost}, Estimated rows: {Rows}",
                request.StoredProcedureName, response.EstimatedCost, response.EstimatedRows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "{DatabaseType} error getting execution plan for {ProcedureName}: {Message}",
                request.DatabaseType, request.StoredProcedureName, ex.Message);

            response.Success = false;
            response.ErrorMessage = $"Error: {ex.Message}";
        }

        return response;
    }

    /// <summary>
    /// Gets the execution plan XML for a stored procedure
    /// </summary>
    private async Task<string> GetExecutionPlanXmlAsync(
        IDbConnection connection,
        string storedProcedureName,
        string? parametersJson,
        CancellationToken cancellationToken)
    {
        var executionPlanXml = new StringBuilder();

        // Parse stored procedure name (handle schema.procedurename format)
        var (schemaName, procedureName) = ParseProcedureName(storedProcedureName);
        var fullProcedureName = string.IsNullOrEmpty(schemaName)
            ? procedureName
            : $"{schemaName}.{procedureName}";

        try
        {
            // Enable SHOWPLAN_XML
            await connection.ExecuteAsync("SET SHOWPLAN_XML ON", commandTimeout: 30);

            _logger.LogDebug("SHOWPLAN_XML enabled for {ProcedureName}", fullProcedureName);

            // Build the EXEC command with parameters
            var execCommand = BuildExecuteCommand(fullProcedureName, parametersJson);

            _logger.LogDebug("Executing command: {Command}", execCommand);

            // Execute and get the execution plan
            // When SHOWPLAN_XML is ON, the query returns the plan XML instead of results
            var planXml = await connection.QueryFirstOrDefaultAsync<string>(
                execCommand,
                commandTimeout: 30);

            if (!string.IsNullOrEmpty(planXml))
            {
                // Format the XML for better readability
                executionPlanXml.Append(FormatXml(planXml));
            }

            _logger.LogDebug("Retrieved execution plan XML ({Length} characters)", executionPlanXml.Length);
        }
        finally
        {
            // Always disable SHOWPLAN_XML
            try
            {
                await connection.ExecuteAsync("SET SHOWPLAN_XML OFF", commandTimeout: 30);
                _logger.LogDebug("SHOWPLAN_XML disabled");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to disable SHOWPLAN_XML");
            }
        }

        return executionPlanXml.ToString();
    }

    /// <summary>
    /// Parses stored procedure name into schema and procedure name
    /// </summary>
    private (string Schema, string ProcedureName) ParseProcedureName(string fullName)
    {
        var parts = fullName.Split('.');
        if (parts.Length == 2)
        {
            return (parts[0].Trim('[', ']'), parts[1].Trim('[', ']'));
        }

        return (string.Empty, fullName.Trim('[', ']'));
    }

    /// <summary>
    /// Builds the EXEC command with parameters
    /// </summary>
    private string BuildExecuteCommand(string procedureName, string? parametersJson)
    {
        var command = new StringBuilder($"EXEC {procedureName}");

        if (!string.IsNullOrEmpty(parametersJson))
        {
            try
            {
                // Parse JSON parameters and add to command
                // This is a simple implementation - you might want to use System.Text.Json for production
                var parameters = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(parametersJson);

                if (parameters != null && parameters.Any())
                {
                    var paramList = parameters.Select(p =>
                    {
                        var value = FormatParameterValue(p.Value);
                        return $"{p.Key} = {value}";
                    });

                    command.Append(" ");
                    command.Append(string.Join(", ", paramList));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse parameters JSON: {Json}", parametersJson);
            }
        }

        return command.ToString();
    }

    /// <summary>
    /// Formats a parameter value for SQL
    /// </summary>
    private string FormatParameterValue(object value)
    {
        if (value == null)
            return "NULL";

        if (value is string || value is DateTime || value is Guid)
            return $"'{value}'";

        if (value is bool boolValue)
            return boolValue ? "1" : "0";

        return value.ToString() ?? "NULL";
    }

    /// <summary>
    /// Formats XML for better readability
    /// </summary>
    private string FormatXml(string xml)
    {
        try
        {
            var document = XDocument.Parse(xml);
            return document.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to format XML");
            return xml;
        }
    }

    /// <summary>
    /// Extracts metadata from execution plan XML
    /// </summary>
    private void ExtractPlanMetadata(string xml, ExecutionPlanResponseDto response)
    {
        try
        {
            var document = XDocument.Parse(xml);
            var ns = document.Root?.GetDefaultNamespace();

            if (ns != null)
            {
                // Extract estimated subtree cost from the root statement
                var statementSubTreeCost = document.Descendants(ns + "StmtSimple")
                    .FirstOrDefault()
                    ?.Attribute("StatementSubTreeCost")
                    ?.Value;

                if (decimal.TryParse(statementSubTreeCost, out var cost))
                {
                    response.EstimatedCost = cost;
                }

                // Extract estimated rows from the first RelOp
                var estimatedRows = document.Descendants(ns + "RelOp")
                    .FirstOrDefault()
                    ?.Attribute("EstimateRows")
                    ?.Value;

                if (long.TryParse(estimatedRows, out var rows))
                {
                    response.EstimatedRows = rows;
                }
                else if (decimal.TryParse(estimatedRows, out var decimalRows))
                {
                    response.EstimatedRows = (long)decimalRows;
                }

                _logger.LogDebug(
                    "Extracted plan metadata - Cost: {Cost}, Rows: {Rows}",
                    response.EstimatedCost, response.EstimatedRows);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract metadata from execution plan XML");
        }
    }

    private IDatabaseProvider GetProvider(DatabaseType databaseType)
    {
        return databaseType switch
        {
            DatabaseType.SqlServer => _sqlServerProvider,
            DatabaseType.MySql => _mySqlProvider,
            _ => throw new NotSupportedException($"Database type {databaseType} is not supported")
        };
    }
}
