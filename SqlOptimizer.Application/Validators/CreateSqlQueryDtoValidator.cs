using FluentValidation;
using SqlOptimizer.Application.DTOs;

namespace SqlOptimizer.Application.Validators;

/// <summary>
/// Validator for CreateSqlQueryDto
/// </summary>
public class CreateSqlQueryDtoValidator : AbstractValidator<CreateSqlQueryDto>
{
    public CreateSqlQueryDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Query name is required")
            .MaximumLength(200).WithMessage("Query name must not exceed 200 characters");

        RuleFor(x => x.QueryText)
            .NotEmpty().WithMessage("Query text is required")
            .MaximumLength(5000).WithMessage("Query text must not exceed 5000 characters");

        RuleFor(x => x.DatabaseName)
            .NotEmpty().WithMessage("Database name is required")
            .MaximumLength(100).WithMessage("Database name must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}
