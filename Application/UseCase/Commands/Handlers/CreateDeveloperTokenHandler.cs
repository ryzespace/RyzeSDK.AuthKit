using Application.DTO;
using Application.Interfaces;
using Application.UseCase.Commands.Requests;
using Domain.Entities;
using Domain.ValueObject;

namespace Application.UseCase.Commands.Handlers;

public class CreateDeveloperTokenHandler(IDeveloperTokenService tokenService)
{
    public async Task<DeveloperTokenCreated> Handle(CreateDeveloperTokenCommand cmd)
    {
        var token = DeveloperToken.Create(
            cmd.DeveloperId, 
            cmd.Name,
            cmd.Scopes.Select(s => (TokenScope)s),
            cmd.Lifetime
        );
        
        var jwt = tokenService.GenerateToken(token);

        return new DeveloperTokenCreated
        {
            Jwt = jwt,
            Token = token
        };
    }
}