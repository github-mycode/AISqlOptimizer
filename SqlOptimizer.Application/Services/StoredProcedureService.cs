using Dapper;
using Microsoft.Extensions.Logging;
using SqlOptimizer.Application.DTOs;
using SqlOptimizer.Application.Interfaces;
using SqlOptimizer.Domain.Enums;
using System.Data;

namespace SqlOptimizer.Application.Services;

/// <summary>
/// Service implementation for stored procedure operations
/// </summary>
public class StoredProcedureService : IStoredProcedureService
{
    private readonly IDatabaseProvider _sqlServerProvider;
    private readonly IDatabaseProvider _mySqlProvider;
    private readonly ILogger<StoredProcedureService> _logger;

    public StoredProcedureService(
        IEnumerable<IDatabaseProvider> providers,
        ILogger<StoredProcedureService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        var providerList = providers.ToList();
        _sqlServerProvider = providerList.FirstOrDefault(p => p.GetType().Name.Contains("SqlServer"))
            ?? throw new InvalidOperationException("SqlServerProvider not registered");
        _mySqlProvider = providerList.FirstOrDefault(p => p.GetType().Name.Contains("MySql"))
            ?? throw new InvalidOperationException("MySqlProvider not registered");
    }

    /// <inheritdoc/>
    public async Task<StoredProcedureDetailDto> GetStoredProcedureDetailAsync(
        StoredProcedureRequestDto request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting stored procedure details for: {ProcedureName} in database: {Database}",
            request.ProcedureName, request.Database);

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

            // Parse schema and procedure name
            var (schemaName, procedureName) = ParseProcedureName(request.ProcedureName);

            // Get basic procedure information
            var basicInfo = await GetBasicProcedureInfoAsync(connection, schemaName, procedureName);
            if (basicInfo == null)
            {
                throw new InvalidOperationException($"Stored procedure '{request.ProcedureName}' not found");
            }

            // Get parameters
            var parameters = await GetParametersAsync(connection, schemaName, procedureName);

            // Get dependencies (what this procedure uses)
            var dependencies = await GetDependenciesAsync(connection, schemaName, procedureName);

            // Get dependent objects (what uses this procedure)
            var dependentObjects = await GetDependentObjectsAsync(connection, schemaName, procedureName);

            // Build the result
            var result = new StoredProcedureDetailDto
            {
                SchemaName = basicInfo.SchemaName,
                ProcedureName = basicInfo.ProcedureName,
                Definition = basicInfo.Definition,
                CreateDate = basicInfo.CreateDate,
                ModifyDate = basicInfo.ModifyDate,
                IsEncrypted = basicInfo.IsEncrypted,
                ReturnType = DetermineReturnType(basicInfo.Definition, parameters),
                Parameters = parameters.ToList(),
                Dependencies = dependencies.ToList(),
                ReferencedTables = dependencies.Where(d => d.ObjectType == "USER_TABLE").ToList(),
                ReferencedViews = dependencies.Where(d => d.ObjectType == "VIEW").ToList(),
                DependentObjects = dependentObjects.ToList()
            };

            _logger.LogInformation("Successfully retrieved details for stored procedure: {ProcedureName}",
                request.ProcedureName);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{DatabaseType} error while getting stored procedure details: {ProcedureName}",
                request.DatabaseType, request.ProcedureName);
            throw new InvalidOperationException($"Failed to get stored procedure details: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Get basic stored procedure information
    /// </summary>
    private async Task<BasicProcedureInfo?> GetBasicProcedureInfoAsync(
        IDbConnection connection,
        string schemaName,
        string procedureName)
    {
        var sql = @"
            SELECT 
                s.name AS SchemaName,
                p.name AS ProcedureName,
                OBJECT_DEFINITION(p.object_id) AS Definition,
                p.create_date AS CreateDate,
                p.modify_date AS ModifyDate,
                p.is_encrypted AS IsEncrypted
            FROM sys.procedures p
            INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
            WHERE s.name = @SchemaName AND p.name = @ProcedureName";

        return await connection.QueryFirstOrDefaultAsync<BasicProcedureInfo>(
            sql,
            new { SchemaName = schemaName, ProcedureName = procedureName });
    }

    /// <summary>
    /// Get stored procedure parameters
    /// </summary>
    private async Task<IEnumerable<ParameterInfoDto>> GetParametersAsync(
        IDbConnection connection,
        string schemaName,
        string procedureName)
    {
        var sql = @"
            SELECT 
                p.name AS ParameterName,
                TYPE_NAME(p.user_type_id) AS DataType,
                p.max_length AS MaxLength,
                p.precision AS Precision,
                p.scale AS Scale,
                p.is_output AS IsOutput,
                p.has_default_value AS HasDefaultValue,
                p.default_value AS DefaultValue,
                p.is_nullable AS IsNullable
            FROM sys.parameters p
            INNER JOIN sys.procedures pr ON p.object_id = pr.object_id
            INNER JOIN sys.schemas s ON pr.schema_id = s.schema_id
            WHERE s.name = @SchemaName 
                AND pr.name = @ProcedureName
                AND p.parameter_id > 0  -- Exclude return value
            ORDER BY p.parameter_id";

        return await connection.QueryAsync<ParameterInfoDto>(
            sql,
            new { SchemaName = schemaName, ProcedureName = procedureName });
    }

    /// <summary>
    /// Get dependencies (objects this procedure references)
    /// </summary>
    private async Task<IEnumerable<DependencyInfoDto>> GetDependenciesAsync(
        IDbConnection connection,
        string schemaName,
        string procedureName)
    {
        var sql = @"
            SELECT DISTINCT
                s.name AS SchemaName,
                o.name AS ObjectName,
                o.type_desc AS ObjectType,
                CASE 
                    WHEN sed.is_selected = 1 THEN 'SELECT'
                    WHEN sed.is_updated = 1 THEN 'UPDATE'
                    WHEN sed.is_insert_all = 1 OR sed.is_insert_selective = 1 THEN 'INSERT'
                    WHEN sed.is_delete = 1 THEN 'DELETE'
                    ELSE 'REFERENCE'
                END AS DependencyType
            FROM sys.procedures p
            INNER JOIN sys.schemas ps ON p.schema_id = ps.schema_id
            INNER JOIN sys.sql_expression_dependencies sed ON p.object_id = sed.referencing_id
            INNER JOIN sys.objects o ON sed.referenced_id = o.object_id
            INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
            WHERE ps.name = @SchemaName 
                AND p.name = @ProcedureName
                AND o.type IN ('U', 'V', 'P', 'FN', 'IF', 'TF')  -- Tables, Views, Procedures, Functions
            ORDER BY s.name, o.name";

        return await connection.QueryAsync<DependencyInfoDto>(
            sql,
            new { SchemaName = schemaName, ProcedureName = procedureName });
    }

    /// <summary>
    /// Get dependent objects (objects that reference this procedure)
    /// </summary>
    private async Task<IEnumerable<DependencyInfoDto>> GetDependentObjectsAsync(
        IDbConnection connection,
        string schemaName,
        string procedureName)
    {
        var sql = @"
            SELECT DISTINCT
                s.name AS SchemaName,
                o.name AS ObjectName,
                o.type_desc AS ObjectType,
                'CALLER' AS DependencyType
            FROM sys.procedures p
            INNER JOIN sys.schemas ps ON p.schema_id = ps.schema_id
            INNER JOIN sys.sql_expression_dependencies sed ON p.object_id = sed.referenced_id
            INNER JOIN sys.objects o ON sed.referencing_id = o.object_id
            INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
            WHERE ps.name = @SchemaName 
                AND p.name = @ProcedureName
            ORDER BY s.name, o.name";

        return await connection.QueryAsync<DependencyInfoDto>(
            sql,
            new { SchemaName = schemaName, ProcedureName = procedureName });
    }

    /// <summary>
    /// Parse procedure name into schema and procedure name
    /// </summary>
    private (string schemaName, string procedureName) ParseProcedureName(string fullName)
    {
        var parts = fullName.Split('.');
        if (parts.Length == 2)
        {
            return (parts[0].Trim('[', ']'), parts[1].Trim('[', ']'));
        }
        return ("dbo", fullName.Trim('[', ']'));
    }

    /// <summary>
    /// Determine return type based on definition and parameters
    /// </summary>
    private string DetermineReturnType(string? definition, IEnumerable<ParameterInfoDto> parameters)
    {
        if (string.IsNullOrEmpty(definition))
            return "UNKNOWN";

        // Check if procedure returns a table
        if (definition.Contains("RETURN", StringComparison.OrdinalIgnoreCase) &&
            (definition.Contains("TABLE", StringComparison.OrdinalIgnoreCase) ||
             definition.Contains("SELECT", StringComparison.OrdinalIgnoreCase)))
        {
            return "TABLE";
        }

        // Check for OUTPUT parameters
        if (parameters.Any(p => p.IsOutput))
        {
            return "OUTPUT_PARAMETERS";
        }

        // Check for explicit RETURN statement with value
        if (definition.Contains("RETURN", StringComparison.OrdinalIgnoreCase))
        {
            return "INTEGER";
        }

        return "NONE";
    }

    /// <summary>
    /// Private class for basic procedure info
    /// </summary>
    private class BasicProcedureInfo
    {
        public string SchemaName { get; set; } = string.Empty;
        public string ProcedureName { get; set; } = string.Empty;
        public string? Definition { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime ModifyDate { get; set; }
        public bool IsEncrypted { get; set; }
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
