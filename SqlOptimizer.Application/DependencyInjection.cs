using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SqlOptimizer.Application.Interfaces;
using SqlOptimizer.Application.Services;
using System.Reflection;

namespace SqlOptimizer.Application;

/// <summary>
/// Dependency injection configuration for Application layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Add Application layer services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<ISqlQueryService, SqlQueryService>();
        services.AddScoped<IConnectionService, ConnectionService>();
        services.AddScoped<IMetadataService, MetadataService>();
        services.AddScoped<IStoredProcedureService, StoredProcedureService>();
        services.AddScoped<IExecutionPlanService, ExecutionPlanService>();
        services.AddScoped<IPromptBuilderService, PromptBuilderService>();
        services.AddScoped<IOpenAIService, OpenAIService>();
        services.AddScoped<ISQLRewriteService, SQLRewriteService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IDashboardService, DashboardService>();

        // Register HttpClient for OpenAI
        services.AddHttpClient("OpenAI");

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
