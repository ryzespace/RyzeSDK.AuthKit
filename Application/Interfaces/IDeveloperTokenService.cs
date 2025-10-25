using Domain.Entities;

namespace Application.Interfaces;

public interface IDeveloperTokenService
{
    string GenerateToken(DeveloperToken token);
}