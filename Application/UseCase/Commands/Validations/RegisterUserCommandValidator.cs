using Application.UseCase.Commands.Requests;
using FluentValidation;

namespace Application.UseCase.Commands.Validations;

/// <summary>
/// Validator for <see cref="RegisterUserCommand"/> that enforces rules for first name, last name, username, email, and password.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item><description>FirstName and LastName: required, max 50 chars, letters only.</description></item>
/// <item><description>Username: required, 3-20 chars, alphanumeric only.</description></item>
/// <item><description>Email: required, must be a valid email format.</description></item>
/// <item><description>Password: required, 6-100 chars, must include at least one uppercase letter, one lowercase letter, and one number.</description></item>
/// </list>
/// </remarks>
public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50).WithMessage("First name must be at most 50 characters.")
            .Matches("^[a-zA-Z]+$").WithMessage("First name can only contain letters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50).WithMessage("Last name must be at most 50 characters.")
            .Matches("^[a-zA-Z]+$").WithMessage("Last name can only contain letters.");
        
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters.")
            .MaximumLength(20).WithMessage("Username must be at most 20 characters.")
            .Matches("^[a-zA-Z0-9]+$").WithMessage("Username can only contain letters and numbers.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .Length(6, 100).WithMessage("Password must be at least 6 characters long.")
            .Matches("[A-Z]+").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]+").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]+").WithMessage("Password must contain at least one number.");
    }
}