using Application.Features.DeveloperTokens.UseCase.Commands.Requests;
using FluentValidation;

namespace Application.Features.DeveloperTokens.UseCase.Commands.Validations;

/// <summary>
/// Validator for <see cref="CreateDeveloperTokenCommand"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Ensures that required fields are provided and correctly formatted.</item>
/// <item>Validates token name length and presence of at least one scope.</item>
/// <item>Validates optional description length and optional lifetime constraints.</item>
/// </list>
/// </remarks>
public class CreateDeveloperTokenValidator : AbstractValidator<CreateDeveloperTokenCommand>
{
    public CreateDeveloperTokenValidator()
    {
        RuleFor(x => x.DeveloperId)
            .NotEmpty()
            .WithMessage("Developer ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Token name is required.")
            .MaximumLength(100)
            .WithMessage("Token name cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.Scopes)
            .NotEmpty()
            .WithMessage("At least one scope must be provided.")
            .Must(s => s.All(x => !string.IsNullOrWhiteSpace(x)))
            .WithMessage("Scopes cannot contain empty values.");

        RuleFor(x => x.Lifetime)
            .Must(l => l == null || l.Value.TotalDays >= 1)
            .WithMessage("Lifetime must be at least one day if specified.");
    }
}