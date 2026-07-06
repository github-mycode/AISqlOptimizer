using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SqlOptimizer.Application.Interfaces;

namespace SqlOptimizer.Infrastructure.Providers;

/// <summary>
/// SQL Server specific database provider
/// </summary>
public class SqlServerProvider : IDatabaseProvider
{
    private readonly ILogger<SqlServerProvider> _logger;

    public SqlServerProvider(ILogger<SqlServerProvider> logger)
    {
        _logger = logger;
    }

    public IDbConnection CreateConnection(string connectionString)
    {
        return new SqlConnection(connectionString);
    }

    public string BuildConnectionString(string server, string database, string? username = null, string? password = null, bool trustServerCertificate = true)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = server,
            InitialCatalog = database,
            TrustServerCertificate = trustServerCertificate,
            ConnectTimeout = 30
        };

        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            builder.UserID = username;
            builder.Password = password;
            builder.IntegratedSecurity = false;
        }
        else
        {
            builder.IntegratedSecurity = true;
        }

        return builder.ConnectionString;
    }

    public string GetDatabasesQuery()
    {
        return @"
            SELECT 
                name AS DatabaseName,
                database_id AS DatabaseId,
                create_date AS CreatedDate
            FROM sys.databases
            WHERE name NOT IN ('master', 'tempdb', 'model', 'msdb')
            ORDER BY name";
    }

    public string GetTablesQuery(string database)
    {
        return $@"
            SELECT 
                TABLE_SCHEMA AS SchemaName,
                TABLE_NAME AS TableName,
                TABLE_TYPE AS TableType
            FROM [{database}].INFORMATION_SCHEMA.TABLES
            WHERE TABLE_TYPE = 'BASE TABLE'
            ORDER BY TABLE_SCHEMA, TABLE_NAME";
    }

    public string GetViewsQuery(string database)
    {
        return $@"
            SELECT 
                TABLE_SCHEMA AS SchemaName,
                TABLE_NAME AS ViewName
            FROM [{database}].INFORMATION_SCHEMA.VIEWS
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
            FROM [{database}].INFORMATION_SCHEMA.ROUTINES
            WHERE ROUTINE_TYPE = 'PROCEDURE'
            ORDER BY ROUTINE_SCHEMA, ROUTINE_NAME";
    }

    public string GetFunctionsQuery(string database)
    {
        return $@"
            SELECT 
                ROUTINE_SCHEMA AS SchemaName,
                ROUTINE_NAME AS FunctionName,
                ROUTINE_TYPE AS FunctionType
            FROM [{database}].INFORMATION_SCHEMA.ROUTINES
            WHERE ROUTINE_TYPE = 'FUNCTION'
            ORDER BY ROUTINE_SCHEMA, ROUTINE_NAME";
    }

    public string GetIndexesQuery(string database)
    {
        return $@"
            SELECT 
                s.name AS SchemaName,
                t.name AS TableName,
                i.name AS IndexName,
                i.type_desc AS IndexType,
                i.is_unique AS IsUnique,
                i.is_primary_key AS IsPrimaryKey
            FROM [{database}].sys.indexes i
            INNER JOIN [{database}].sys.tables t ON i.object_id = t.object_id
            INNER JOIN [{database}].sys.schemas s ON t.schema_id = s.schema_id
            WHERE i.name IS NOT NULL
            ORDER BY s.name, t.name, i.name";
    }

    public string GetForeignKeysQuery(string database)
    {
        return $@"
            SELECT 
                fk.name AS ForeignKeyName,
                OBJECT_SCHEMA_NAME(fk.parent_object_id, DB_ID('{database}')) AS SchemaName,
                OBJECT_NAME(fk.parent_object_id, DB_ID('{database}')) AS TableName,
                COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ColumnName,
                OBJECT_SCHEMA_NAME(fk.referenced_object_id, DB_ID('{database}')) AS ReferencedSchemaName,
                OBJECT_NAME(fk.referenced_object_id, DB_ID('{database}')) AS ReferencedTableName,
                COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS ReferencedColumnName
            FROM [{database}].sys.foreign_keys fk
            INNER JOIN [{database}].sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
            ORDER BY SchemaName, TableName, ForeignKeyName";
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
        return $@"
            SET SHOWPLAN_XML ON;
            {query}
            SET SHOWPLAN_XML OFF;";
    }

    public async Task<(bool success, string? version)> TestConnectionAsync(IDbConnection connection)
    {
        try
        {
            if (connection.State != ConnectionState.Open)
                connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT @@VERSION";
            var version = await Task.Run(() => command.ExecuteScalar()?.ToString());
            
            return (true, version);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQL Server connection test failed");
            return (false, null);
        }
    }
}
