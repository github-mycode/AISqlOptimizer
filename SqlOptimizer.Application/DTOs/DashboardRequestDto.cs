using SqlOptimizer.Domain.Enums;

namespace SqlOptimizer.Application.DTOs;

/// <summary>
/// Request DTO for dashboard data
/// </summary>
public class DashboardRequestDto
{
    /// <summary>
    /// Database type (SqlServer or MySql)
    /// </summary>
    public DatabaseType DatabaseType { get; set; } = DatabaseType.SqlServer;

    /// <summary>
    /// Server name
    /// </summary>
    public string ServerName { get; set; } = string.Empty;

    /// <summary>
    /// Database name
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// Use Windows Authentication (true) or Database Authentication (false)
    /// Only applies to SQL Server
    /// </summary>
    public bool UseWindowsAuth { get; set; } = false;

    /// <summary>
    /// Database username (required if UseWindowsAuth is false)
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Database password (required if UseWindowsAuth is false)
    /// </summary>
    public string? Password { get; set; }
}
