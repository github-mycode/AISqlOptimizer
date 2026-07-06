using System.Data;

namespace SqlOptimizer.Application.Interfaces;

/// <summary>
/// Interface for database-specific operations
/// </summary>
public interface IDatabaseProvider
{
    /// <summary>
    /// Creates a database connection
    /// </summary>
    IDbConnection CreateConnection(string connectionString);

    /// <summary>
    /// Builds a connection string from parameters
    /// </summary>
    string BuildConnectionString(string server, string database, string? username = null, string? password = null, bool trustServerCertificate = true);

    /// <summary>
    /// Gets list of databases query
    /// </summary>
    string GetDatabasesQuery();

    /// <summary>
    /// Gets list of tables query
    /// </summary>
    string GetTablesQuery(string database);

    /// <summary>
    /// Gets list of views query
    /// </summary>
    string GetViewsQuery(string database);

    /// <summary>
    /// Gets list of stored procedures query
    /// </summary>
    string GetStoredProceduresQuery(string database);

    /// <summary>
    /// Gets list of functions query
    /// </summary>
    string GetFunctionsQuery(string database);

    /// <summary>
    /// Gets list of indexes query
    /// </summary>
    string GetIndexesQuery(string database);

    /// <summary>
    /// Gets list of foreign keys query
    /// </summary>
    string GetForeignKeysQuery(string database);

    /// <summary>
    /// Gets stored procedure definition query
    /// </summary>
    string GetProcedureDefinitionQuery(string procedureName);

    /// <summary>
    /// Gets stored procedure parameters query
    /// </summary>
    string GetProcedureParametersQuery(string procedureName);

    /// <summary>
    /// Gets execution plan query
    /// </summary>
    string GetExecutionPlanQuery(string query);

    /// <summary>
    /// Tests database connection
    /// </summary>
    Task<(bool success, string? version)> TestConnectionAsync(IDbConnection connection);
}
