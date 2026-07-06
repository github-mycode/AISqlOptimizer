using FluentValidation;
using SqlOptimizer.Application.DTOs;

namespace SqlOptimizer.Application.Validators;

/// <summary>
/// Validator for StoredProcedureRequestDto
/// </summary>
public class StoredProcedureRequestDtoValidator : AbstractValidator<StoredProcedureRequestDto>
{
    public StoredProcedureRequestDtoValidator()
    {
        RuleFor(x => x.Server)
            .NotEmpty().WithMessage("Server is required")
            .MaximumLength(255).WithMessage("Server name must not exceed 255 characters");

        RuleFor(x => x.Database)
            .NotEmpty().WithMessage("Database is required")
            .MaximumLength(128).WithMessage("Database name must not exceed 128 characters");

        RuleFor(x => x.ProcedureName)
            .NotEmpty().WithMessage("Procedure name is required")
            .MaximumLength(256).WithMessage("Procedure name must not exceed 256 characters");

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
}
