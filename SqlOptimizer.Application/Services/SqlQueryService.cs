using SqlOptimizer.Application.DTOs;
using SqlOptimizer.Application.Interfaces;
using SqlOptimizer.Domain.Entities;
using SqlOptimizer.Domain.Interfaces;

namespace SqlOptimizer.Application.Services;

/// <summary>
/// Service implementation for SQL query operations
/// </summary>
public class SqlQueryService : ISqlQueryService
{
    private readonly ISqlQueryRepository _repository;

    public SqlQueryService(ISqlQueryRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <inheritdoc/>
    public async Task<SqlQueryDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity != null ? MapToDto(entity) : null;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<SqlQueryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetAllAsync(cancellationToken);
        return entities.Select(MapToDto);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<SqlQueryDto>> GetByDatabaseNameAsync(string databaseName, CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetByDatabaseNameAsync(databaseName, cancellationToken);
        return entities.Select(MapToDto);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<SqlQueryDto>> GetOptimizedQueriesAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetOptimizedQueriesAsync(cancellationToken);
        return entities.Select(MapToDto);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<SqlQueryDto>> GetUnoptimizedQueriesAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetUnoptimizedQueriesAsync(cancellationToken);
        return entities.Select(MapToDto);
    }

    /// <inheritdoc/>
    public async Task<SqlQueryDto> CreateAsync(CreateSqlQueryDto dto, CancellationToken cancellationToken = default)
    {
        var entity = new SqlQuery
        {
            Name = dto.Name,
            QueryText = dto.QueryText,
            Description = dto.Description,
            DatabaseName = dto.DatabaseName,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
            IsOptimized = false
        };

        var created = await _repository.AddAsync(entity, cancellationToken);
        return MapToDto(created);
    }

    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(int id, UpdateSqlQueryDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        if (entity == null)
            return false;

        if (!string.IsNullOrEmpty(dto.Name))
            entity.Name = dto.Name;

        if (!string.IsNullOrEmpty(dto.QueryText))
            entity.QueryText = dto.QueryText;

        if (dto.Description != null)
            entity.Description = dto.Description;

        if (!string.IsNullOrEmpty(dto.DatabaseName))
            entity.DatabaseName = dto.DatabaseName;

        if (dto.ExecutionTimeMs.HasValue)
            entity.ExecutionTimeMs = dto.ExecutionTimeMs;

        if (dto.OptimizedQueryText != null)
            entity.OptimizedQueryText = dto.OptimizedQueryText;

        if (dto.OptimizationNotes != null)
            entity.OptimizationNotes = dto.OptimizationNotes;

        if (dto.IsOptimized.HasValue)
            entity.IsOptimized = dto.IsOptimized.Value;

        entity.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(entity, cancellationToken);
        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var exists = await _repository.ExistsAsync(id, cancellationToken);
        if (!exists)
            return false;

        await _repository.DeleteAsync(id, cancellationToken);
        return true;
    }

    private static SqlQueryDto MapToDto(SqlQuery entity)
    {
        return new SqlQueryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            QueryText = entity.QueryText,
            Description = entity.Description,
            DatabaseName = entity.DatabaseName,
            ExecutionTimeMs = entity.ExecutionTimeMs,
            OptimizedQueryText = entity.OptimizedQueryText,
            OptimizationNotes = entity.OptimizationNotes,
            IsOptimized = entity.IsOptimized,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
