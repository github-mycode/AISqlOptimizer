using FluentValidation;
using SqlOptimizer.Application.DTOs;

namespace SqlOptimizer.Application.Validators;

/// <summary>
/// Validator for MetadataRequestDto
/// </summary>
public class MetadataRequestDtoValidator : AbstractValidator<MetadataRequestDto>
{
    public MetadataRequestDtoValidator()
    {
        RuleFor(x => x.Server)
            .NotEmpty().WithMessage("Server is required")
            .MaximumLength(255).WithMessage("Server name must not exceed 255 characters");

        RuleFor(x => x.Database)
            .NotEmpty().WithMessage("Database is required")
            .MaximumLength(128).WithMessage("Database name must not exceed 128 characters");

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
