namespace SqlOptimizer.Application.DTOs;

/// <summary>
/// DTO for database connection response
/// </summary>
public class DatabaseConnectionResponseDto
{
    /// <summary>
    /// Indicates if the connection was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Database type that was tested (SqlServer or MySql)
    /// </summary>
    public string DatabaseType { get; set; } = "SqlServer";

    /// <summary>
    /// Message describing the connection result
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Server version information (if connection successful)
    /// </summary>
    public string? ServerVersion { get; set; }

    /// <summary>
    /// Database name that was connected to
    /// </summary>
    public string? DatabaseName { get; set; }

    /// <summary>
    /// Error details (if connection failed)
    /// </summary>
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// Connection timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
