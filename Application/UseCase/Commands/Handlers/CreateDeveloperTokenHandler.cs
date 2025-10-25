using Application.DTO;
using Application.Interfaces;
using Application.UseCase.Commands.Requests;
using Domain.Entities;

namespace Application.UseCase.Commands.Handlers;

public class CreateDeveloperTokenHandler(IDeveloperTokenService tokenService)
{
    public async Task<DeveloperTokenCreated> Handle(CreateDeveloperTokenCommand cmd)
    {
        var token = new DeveloperToken
        {
            DeveloperId = cmd.DeveloperId,
            Name = cmd.Name,
            Scopes = cmd.Scopes.ToList(),
            ExpiresAt = cmd.Lifetime.HasValue
                ? DateTimeOffset.UtcNow.Add(cmd.Lifetime.Value)
                : null
        };

        var jwt = tokenService.GenerateToken(token);

        return new DeveloperTokenCreated
        {
            Jwt = jwt,
            Token = token
        };
    }
}