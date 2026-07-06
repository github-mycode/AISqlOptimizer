using Serilog;
using Serilog.Events;
using SqlOptimizer.Api.Middleware;
using SqlOptimizer.Application;
using SqlOptimizer.Application.Options;
using SqlOptimizer.Infrastructure;
using System.Reflection;

// Configure Serilog with enrichment
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithThreadId()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProperty("Application", "SqlOptimizer")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj} {Properties:j}{NewLine}{Exception}",
        fileSizeLimitBytes: 10485760, // 10 MB
        retainedFileCountLimit: 30,
        rollOnFileSizeLimit: true)
    .WriteTo.File(
        path: "logs/errors-.txt",
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: LogEventLevel.Error,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] {Message:lj} {Properties:j}{NewLine}{Exception}",
        fileSizeLimitBytes: 10485760,
        retainedFileCountLimit: 90)
    .CreateLogger();

try
{
    Log.Information("Starting SqlOptimizer API");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddControllers();

    // Configure Options
    builder.Services.Configure<OpenAIOptions>(
        builder.Configuration.GetSection(OpenAIOptions.SectionName));

    // Add Application layer services
    builder.Services.AddApplication();

    // Add Infrastructure layer services
    builder.Services.AddInfrastructure(builder.Configuration);

    // Add API documentation
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "SqlOptimizer API",
            Version = "v1",
            Description = "A production-ready ASP.NET Core 8 Web API for SQL query optimization",
            Contact = new()
            {
                Name = "SqlOptimizer Team",
                Email = "support@sqloptimizer.com"
            }
        });

        // Include XML documentation
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    // Add CORS if needed
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // Add health checks
    var connectionString = builder.Configuration.GetSection("Database:ConnectionString").Value;
    if (!string.IsNullOrEmpty(connectionString))
    {
        builder.Services.AddHealthChecks()
            .AddSqlServer(connectionString, name: "sql-server", tags: new[] { "db", "sql" });
    }

    var app = builder.Build();

    // Configure the HTTP request pipeline
    // Enable Swagger in Development or if explicitly enabled via environment variable
    var enableSwagger = app.Environment.IsDevelopment() || 
                       builder.Configuration.GetValue<bool>("EnableSwagger", false);
    
    if (enableSwagger)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "SqlOptimizer API v1");
            options.RoutePrefix = string.Empty; // Set Swagger UI at app's root
        });
        
        Log.Information("Swagger UI enabled at root path (/)");
    }

    // Use request/response logging middleware
    app.UseMiddleware<RequestResponseLoggingMiddleware>();

    // Use global exception handling middleware
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    app.UseHttpsRedirection();

    app.UseCors();

    app.UseAuthorization();

    // Map health checks
    app.MapHealthChecks("/health");

    app.MapControllers();

    Log.Information("SqlOptimizer API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
