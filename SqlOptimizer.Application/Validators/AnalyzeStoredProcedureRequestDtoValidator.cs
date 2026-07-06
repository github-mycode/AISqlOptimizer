using FluentValidation;
using SqlOptimizer.Application.DTOs;

namespace SqlOptimizer.Application.Validators;

/// <summary>
/// Validator for AnalyzeStoredProcedureRequestDto
/// </summary>
public class AnalyzeStoredProcedureRequestDtoValidator : AbstractValidator<AnalyzeStoredProcedureRequestDto>
{
    public AnalyzeStoredProcedureRequestDtoValidator()
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

        RuleFor(x => x.StoredProcedureName)
            .NotEmpty()
            .WithMessage("Stored procedure name is required.")
            .MaximumLength(256)
            .WithMessage("Stored procedure name must not exceed 256 characters.")
            .Must(BeValidProcedureName)
            .WithMessage("Stored procedure name contains invalid characters.");

        RuleFor(x => x.Username)
            .MaximumLength(128)
            .WithMessage("Username must not exceed 128 characters.")
            .When(x => !string.IsNullOrEmpty(x.Username));

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required when username is provided.")
            .When(x => !string.IsNullOrEmpty(x.Username));

        RuleFor(x => x.Parameters)
            .Must(BeValidJson)
            .WithMessage("Parameters must be valid JSON.")
            .When(x => !string.IsNullOrEmpty(x.Parameters));
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

    private bool BeValidProcedureName(string procedureName)
    {
        if (string.IsNullOrEmpty(procedureName))
            return false;

        var invalidChars = new[] { ';', '\'', '"', '=', '<', '>', '|', '/', '\\', ' ' };
        return !invalidChars.Any(procedureName.Contains);
    }

    private bool BeValidJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return true;

        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
