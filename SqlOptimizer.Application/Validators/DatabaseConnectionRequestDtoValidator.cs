using FluentValidation;
using SqlOptimizer.Application.DTOs;

namespace SqlOptimizer.Application.Validators;

/// <summary>
/// Validator for DatabaseConnectionRequestDto
/// </summary>
public class DatabaseConnectionRequestDtoValidator : AbstractValidator<DatabaseConnectionRequestDto>
{
    public DatabaseConnectionRequestDtoValidator()
    {
        RuleFor(x => x.Server)
            .NotEmpty().WithMessage("Server is required")
            .MaximumLength(255).WithMessage("Server name must not exceed 255 characters")
            .Must(BeValidServerName).WithMessage("Server name contains invalid characters");

        RuleFor(x => x.Database)
            .NotEmpty().WithMessage("Database is required")
            .MaximumLength(128).WithMessage("Database name must not exceed 128 characters")
            .Must(BeValidDatabaseName).WithMessage("Database name contains invalid characters");

        RuleFor(x => x.Username)
            .MaximumLength(128).WithMessage("Username must not exceed 128 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Username));

        // If username is provided, password should also be provided
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required when Username is provided")
            .When(x => !string.IsNullOrWhiteSpace(x.Username));

        // If password is provided without username, that's an error
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required when Password is provided")
            .When(x => !string.IsNullOrWhiteSpace(x.Password));
    }

    /// <summary>
    /// Validate server name format
    /// </summary>
    private bool BeValidServerName(string? serverName)
    {
        if (string.IsNullOrWhiteSpace(serverName))
            return false;

        // Basic validation - allow letters, numbers, dots, backslashes, commas, and hyphens
        // Examples: localhost, .\SQLEXPRESS, server.domain.com, 192.168.1.1, server\instance
        var invalidChars = new[] { ';', '\'', '"', '<', '>', '|', '*', '?' };
        return !invalidChars.Any(c => serverName.Contains(c));
    }

    /// <summary>
    /// Validate database name format
    /// </summary>
    private bool BeValidDatabaseName(string? databaseName)
    {
        if (string.IsNullOrWhiteSpace(databaseName))
            return false;

        // SQL Server database name validation - avoid special characters
        var invalidChars = new[] { ';', '\'', '"', '<', '>', '|', '*', '?', '/', '\\', '\0' };
        return !invalidChars.Any(c => databaseName.Contains(c));
    }
}
