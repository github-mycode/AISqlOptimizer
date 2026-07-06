using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlOptimizer.Infrastructure.Options;
using System.Data;
using System.Diagnostics;

namespace SqlOptimizer.Infrastructure.Data;

/// <summary>
/// Database connection factory interface
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Create a new database connection
    /// </summary>
    /// <returns>Database connection</returns>
    IDbConnection CreateConnection();
}

/// <summary>
/// SQL Server connection factory implementation with logging
/// </summary>
public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly DatabaseOptions _options;
    private readonly ILogger<SqlConnectionFactory> _logger;

    public SqlConnectionFactory(
        IOptions<DatabaseOptions> options,
        ILogger<SqlConnectionFactory> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public IDbConnection CreateConnection()
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var connection = new SqlConnection(_options.ConnectionString);
            
            _logger.LogDebug(
                "Database connection created. Server: {Server}, Database: {Database}",
                connection.DataSource,
                connection.Database);
            
            return connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create database connection");
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogDebug("Database connection creation took {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
        }
    }
}
