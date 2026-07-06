namespace SqlOptimizer.Infrastructure.Options;

/// <summary>
/// Database connection options
/// </summary>
public class DatabaseOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "Database";

    /// <summary>
    /// SQL Server connection string
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Command timeout in seconds
    /// </summary>
    public int CommandTimeout { get; set; } = 30;
}
