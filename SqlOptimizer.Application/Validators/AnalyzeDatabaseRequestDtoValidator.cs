using FluentValidation;
using SqlOptimizer.Application.DTOs;

namespace SqlOptimizer.Application.Validators;

/// <summary>
/// Validator for AnalyzeDatabaseRequestDto
/// </summary>
public class AnalyzeDatabaseRequestDtoValidator : AbstractValidator<AnalyzeDatabaseRequestDto>
{
    public AnalyzeDatabaseRequestDtoValidator()
    {
        RuleFor(x => x.Server)
            .NotEmpty()
            .WithMessage("Server is required.")
            .MaximumLength(255)
            .WithMessage("Server name must not exceed 255 characters.")
            .Must(BeValidServerName)
            .WithMessage("Server name contains invalid characters.");

        RuleFor(x => x.Database)
            .NotEmpty()
            .WithMessage("Database is required.")
            .MaximumLength(128)
            .WithMessage("Database name must not exceed 128 characters.")
            .Must(BeValidDatabaseName)
            .WithMessage("Database name contains invalid characters.");

        RuleFor(x => x.Username)
            .MaximumLength(128)
            .WithMessage("Username must not exceed 128 characters.")
            .When(x => !string.IsNullOrEmpty(x.Username));

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required when username is provided.")
            .When(x => !string.IsNullOrEmpty(x.Username));

        RuleFor(x => x.MaxParallelism)
            .GreaterThan(0)
            .WithMessage("Max parallelism must be greater than 0.")
            .LessThanOrEqualTo(20)
            .WithMessage("Max parallelism must not exceed 20 to prevent resource exhaustion.");

        RuleFor(x => x.SchemaFilter)
            .MaximumLength(128)
            .WithMessage("Schema filter must not exceed 128 characters.")
            .Must(BeValidSchemaName)
            .WithMessage("Schema filter contains invalid characters.")
            .When(x => !string.IsNullOrEmpty(x.SchemaFilter));
    }

    private bool BeValidServerName(string serverName)
    {
        if (string.IsNullOrEmpty(serverName))
            return false;

        var invalidChars = new[] { ';', '\'', '"', '=', '<', '>', '|' };
        return !invalidChars.Any(serverName.Contains);
    }

    private bool BeValidDatabaseName(string databaseName)
    {
        if (string.IsNullOrEmpty(databaseName))
            return false;

        var invalidChars = new[] { ';', '\'', '"', '=', '<', '>', '|', '/', '\\' };
        return !invalidChars.Any(databaseName.Contains);
    }

    private bool BeValidSchemaName(string? schemaName)
    {
        if (string.IsNullOrEmpty(schemaName))
            return true;

        var invalidChars = new[] { ';', '\'', '"', '=', '<', '>', '|', '/', '\\', ' ' };
        return !invalidChars.Any(schemaName.Contains);
    }
}
