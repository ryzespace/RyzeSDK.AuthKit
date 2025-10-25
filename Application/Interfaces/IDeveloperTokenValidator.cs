namespace Application.Interfaces;

public interface IDeveloperTokenValidator
{
    Task<DeveloperTokenPrincipal?> ValidateAsync(string token);
}