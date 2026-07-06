using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlOptimizer.Application.Interfaces;
using SqlOptimizer.Domain.Interfaces;
using SqlOptimizer.Infrastructure.Data;
using SqlOptimizer.Infrastructure.Options;
using SqlOptimizer.Infrastructure.Providers;
using SqlOptimizer.Infrastructure.Repositories;

namespace SqlOptimizer.Infrastructure;

/// <summary>
/// Dependency injection configuration for Infrastructure layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Add Infrastructure layer services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure database options
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));

        // Register database providers
        services.AddSingleton<IDatabaseProvider, SqlServerProvider>();
        services.AddSingleton<IDatabaseProvider, MySqlProvider>();

        // Register database connection factory
        services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();

        // Register repositories
        services.AddScoped<ISqlQueryRepository, SqlQueryRepository>();

        return services;
    }
}
