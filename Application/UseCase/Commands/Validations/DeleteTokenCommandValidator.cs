using Application.UseCase.Commands.Requests;
using FluentValidation;

namespace Application.UseCase.Commands.Validations;

/// <summary>
/// Validator for <see cref="DeleteTokenCommand"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Ensures that the <see cref="DeleteTokenCommand.TokenId"/> is provided and not empty.</item>
/// </list>
/// </remarks>
public class DeleteTokenCommandValidator : AbstractValidator<DeleteTokenCommand>
{
    public DeleteTokenCommandValidator()
    {
        RuleFor(x => x.TokenId)
            .NotEmpty()
            .WithMessage("Token ID is required.");
    }
}