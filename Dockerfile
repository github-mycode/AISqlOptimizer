# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["SqlOptimizer.Api/SqlOptimizer.Api.csproj", "SqlOptimizer.Api/"]
COPY ["SqlOptimizer.Application/SqlOptimizer.Application.csproj", "SqlOptimizer.Application/"]
COPY ["SqlOptimizer.Infrastructure/SqlOptimizer.Infrastructure.csproj", "SqlOptimizer.Infrastructure/"]
COPY ["SqlOptimizer.Domain/SqlOptimizer.Domain.csproj", "SqlOptimizer.Domain/"]

# Restore dependencies
RUN dotnet restore "SqlOptimizer.Api/SqlOptimizer.Api.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/SqlOptimizer.Api"
RUN dotnet build "SqlOptimizer.Api.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "SqlOptimizer.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user for security
RUN useradd -m -u 1000 appuser && \
    mkdir -p /app/logs && \
    chown -R appuser:appuser /app

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Copy published application
COPY --from=publish /app/publish .

# Switch to non-root user
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD curl --fail http://localhost:8080/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_RUNNING_IN_CONTAINER=true

# Entry point
ENTRYPOINT ["dotnet", "SqlOptimizer.Api.dll"]
