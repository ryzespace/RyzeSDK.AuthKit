using Application.UseCase.Commands.Requests;
using FluentValidation;

namespace Application.UseCase.Commands.Validations;

public class CreateDeveloperTokenValidator : AbstractValidator<CreateDeveloperTokenCommand>
{
    public CreateDeveloperTokenValidator()
    {
        
    }
}