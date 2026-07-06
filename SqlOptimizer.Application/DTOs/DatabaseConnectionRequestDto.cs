using SqlOptimizer.Domain.Enums;

namespace SqlOptimizer.Application.DTOs;

/// <summary>
/// DTO for database connection request
/// </summary>
public class DatabaseConnectionRequestDto
{
    /// <summary>
    /// Database type (SqlServer or MySql)
    /// </summary>
    public DatabaseType DatabaseType { get; set; } = DatabaseType.SqlServer;

    /// <summary>
    /// Server instance name or address
    /// </summary>
    public string Server { get; set; } = string.Empty;

    /// <summary>
    /// Database name
    /// </summary>
    public string Database { get; set; } = string.Empty;

    /// <summary>
    /// Username for authentication (optional for SQL Server Windows auth)
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Password for authentication (optional for SQL Server Windows auth)
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Trust server certificate (recommended for local development)
    /// </summary>
    public bool TrustServerCertificate { get; set; } = true;
}
