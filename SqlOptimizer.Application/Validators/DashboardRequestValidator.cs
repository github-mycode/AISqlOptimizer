using FluentValidation;
using SqlOptimizer.Application.DTOs;

namespace SqlOptimizer.Application.Validators;

/// <summary>
/// Validator for DashboardRequestDto
/// </summary>
public class DashboardRequestValidator : AbstractValidator<DashboardRequestDto>
{
    public DashboardRequestValidator()
    {
        RuleFor(x => x.ServerName)
            .NotEmpty()
            .WithMessage("Server name is required")
            .Must(BeValidServerName)
            .WithMessage("Server name contains invalid characters");

        RuleFor(x => x.DatabaseName)
            .NotEmpty()
            .WithMessage("Database name is required")
            .Must(BeValidDatabaseName)
            .WithMessage("Database name contains invalid characters");

        When(x => !x.UseWindowsAuth, () =>
        {
            RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("Username is required when using SQL Server authentication");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required when using SQL Server authentication");
        });
    }

    private bool BeValidServerName(string? serverName)
    {
        if (string.IsNullOrWhiteSpace(serverName))
            return false;

        // Allow letters, numbers, dots, hyphens, underscores, backslashes (for instances), and commas (for ports)
        var invalidChars = new[] { ';', '\'', '"', '=', '<', '>', '|', '&', '$', '(', ')', '[', ']', '{', '}' };
        return !invalidChars.Any(c => serverName.Contains(c));
    }

    private bool BeValidDatabaseName(string? databaseName)
    {
        if (string.IsNullOrWhiteSpace(databaseName))
            return false;

        // SQL Server database name validation
        var invalidChars = new[] { ';', '\'', '"', '\\', '/', '*', '?', '<', '>', '|', '[', ']' };
        return !invalidChars.Any(c => databaseName.Contains(c));
    }
}
