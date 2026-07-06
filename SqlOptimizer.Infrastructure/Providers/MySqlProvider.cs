using System.Data;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using SqlOptimizer.Application.Interfaces;

namespace SqlOptimizer.Infrastructure.Providers;

/// <summary>
/// MySQL specific database provider
/// </summary>
public class MySqlProvider : IDatabaseProvider
{
    private readonly ILogger<MySqlProvider> _logger;

    public MySqlProvider(ILogger<MySqlProvider> logger)
    {
        _logger = logger;
    }

    public IDbConnection CreateConnection(string connectionString)
    {
        return new MySqlConnection(connectionString);
    }

    public string BuildConnectionString(string server, string database, string? username = null, string? password = null, bool trustServerCertificate = true)
    {
        var builder = new MySqlConnectionStringBuilder
        {
            Server = server,
            Database = database,
            ConnectionTimeout = 30,
            AllowUserVariables = true
        };

        if (!string.IsNullOrEmpty(username))
        {
            builder.UserID = username;
            builder.Password = password ?? string.Empty;
        }

        if (trustServerCertificate)
        {
            builder.SslMode = MySqlSslMode.Disabled;
        }

        return builder.ConnectionString;
    }

    public string GetDatabasesQuery()
    {
        return @"
            SELECT 
                SCHEMA_NAME AS DatabaseName,
                NULL AS DatabaseId,
                NULL AS CreatedDate
            FROM INFORMATION_SCHEMA.SCHEMATA
            WHERE SCHEMA_NAME NOT IN ('information_schema', 'mysql', 'performance_schema', 'sys')
            ORDER BY SCHEMA_NAME";
    }

    public string GetTablesQuery(string database)
    {
        return $@"
            SELECT 
                TABLE_SCHEMA AS SchemaName,
                TABLE_NAME AS TableName,
                TABLE_TYPE AS TableType
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_SCHEMA = '{database}'
                AND TABLE_TYPE = 'BASE TABLE'
            ORDER BY TABLE_SCHEMA, TABLE_NAME";
    }

    public string GetViewsQuery(string database)
    {
        return $@"
            SELECT 
                TABLE_SCHEMA AS SchemaName,
                TABLE_NAME AS ViewName
            FROM INFORMATION_SCHEMA.VIEWS
            WHERE TABLE_SCHEMA = '{database}'
            ORDER BY TABLE_SCHEMA, TABLE_NAME";
    }

    public string GetStoredProceduresQuery(string database)
    {
        return $@"
            SELECT 
                ROUTINE_SCHEMA AS SchemaName,
                ROUTINE_NAME AS ProcedureName,
                CREATED AS CreatedDate,
                LAST_ALTERED AS ModifiedDate
            FROM INFORMATION_SCHEMA.ROUTINES
            WHERE ROUTINE_SCHEMA = '{database}'
                AND ROUTINE_TYPE = 'PROCEDURE'
            ORDER BY ROUTINE_SCHEMA, ROUTINE_NAME";
    }

    public string GetFunctionsQuery(string database)
    {
        return $@"
            SELECT 
                ROUTINE_SCHEMA AS SchemaName,
                ROUTINE_NAME AS FunctionName,
                ROUTINE_TYPE AS FunctionType
            FROM INFORMATION_SCHEMA.ROUTINES
            WHERE ROUTINE_SCHEMA = '{database}'
                AND ROUTINE_TYPE = 'FUNCTION'
            ORDER BY ROUTINE_SCHEMA, ROUTINE_NAME";
    }

    public string GetIndexesQuery(string database)
    {
        return $@"
            SELECT 
                TABLE_SCHEMA AS SchemaName,
                TABLE_NAME AS TableName,
                INDEX_NAME AS IndexName,
                INDEX_TYPE AS IndexType,
                NON_UNIQUE = 0 AS IsUnique,
                INDEX_NAME = 'PRIMARY' AS IsPrimaryKey
            FROM INFORMATION_SCHEMA.STATISTICS
            WHERE TABLE_SCHEMA = '{database}'
                AND INDEX_NAME IS NOT NULL
            GROUP BY TABLE_SCHEMA, TABLE_NAME, INDEX_NAME, INDEX_TYPE, NON_UNIQUE
            ORDER BY TABLE_SCHEMA, TABLE_NAME, INDEX_NAME";
    }

    public string GetForeignKeysQuery(string database)
    {
        return $@"
            SELECT 
                CONSTRAINT_NAME AS ForeignKeyName,
                TABLE_SCHEMA AS SchemaName,
                TABLE_NAME AS TableName,
                COLUMN_NAME AS ColumnName,
                REFERENCED_TABLE_SCHEMA AS ReferencedSchemaName,
                REFERENCED_TABLE_NAME AS ReferencedTableName,
                REFERENCED_COLUMN_NAME AS ReferencedColumnName
            FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
            WHERE TABLE_SCHEMA = '{database}'
                AND REFERENCED_TABLE_NAME IS NOT NULL
            ORDER BY TABLE_SCHEMA, TABLE_NAME, CONSTRAINT_NAME";
    }

    public string GetProcedureDefinitionQuery(string procedureName)
    {
        return $@"
            SELECT 
                ROUTINE_DEFINITION AS Definition
            FROM INFORMATION_SCHEMA.ROUTINES
            WHERE ROUTINE_NAME = '{procedureName}'
                AND ROUTINE_TYPE = 'PROCEDURE'";
    }

    public string GetProcedureParametersQuery(string procedureName)
    {
        return $@"
            SELECT 
                PARAMETER_NAME AS ParameterName,
                DATA_TYPE AS DataType,
                PARAMETER_MODE AS Mode
            FROM INFORMATION_SCHEMA.PARAMETERS
            WHERE SPECIFIC_NAME = '{procedureName}'
            ORDER BY ORDINAL_POSITION";
    }

    public string GetExecutionPlanQuery(string query)
    {
        // MySQL uses EXPLAIN instead of SHOWPLAN_XML
        return $@"EXPLAIN FORMAT=JSON {query}";
    }

    public async Task<(bool success, string? version)> TestConnectionAsync(IDbConnection connection)
    {
        try
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT VERSION()";
            var version = await Task.Run(() => command.ExecuteScalar()?.ToString());
            
            return (true, version);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MySQL connection test failed");
            return (false, null);
        }
    }
}
