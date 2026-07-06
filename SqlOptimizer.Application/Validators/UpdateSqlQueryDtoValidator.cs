using FluentValidation;
using SqlOptimizer.Application.DTOs;

namespace SqlOptimizer.Application.Validators;

/// <summary>
/// Validator for UpdateSqlQueryDto
/// </summary>
public class UpdateSqlQueryDtoValidator : AbstractValidator<UpdateSqlQueryDto>
{
    public UpdateSqlQueryDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Query name must not exceed 200 characters")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.QueryText)
            .MaximumLength(5000).WithMessage("Query text must not exceed 5000 characters")
            .When(x => !string.IsNullOrEmpty(x.QueryText));

        RuleFor(x => x.DatabaseName)
            .MaximumLength(100).WithMessage("Database name must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.DatabaseName));

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.OptimizedQueryText)
            .MaximumLength(5000).WithMessage("Optimized query text must not exceed 5000 characters")
            .When(x => !string.IsNullOrEmpty(x.OptimizedQueryText));

        RuleFor(x => x.OptimizationNotes)
            .MaximumLength(2000).WithMessage("Optimization notes must not exceed 2000 characters")
            .When(x => !string.IsNullOrEmpty(x.OptimizationNotes));

        RuleFor(x => x.ExecutionTimeMs)
            .GreaterThanOrEqualTo(0).WithMessage("Execution time must be non-negative")
            .When(x => x.ExecutionTimeMs.HasValue);
    }
}
