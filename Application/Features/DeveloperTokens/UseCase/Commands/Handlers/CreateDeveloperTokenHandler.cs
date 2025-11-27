using Application.Features.DeveloperTokens.DTO;
using Application.Features.DeveloperTokens.Interfaces;
using Application.Features.DeveloperTokens.UseCase.Commands.Requests;
using Application.Options;
using Domain.Features.DeveloperTokens;
using Domain.Features.DeveloperTokens.Repositories;
using Microsoft.Extensions.Options;

namespace Application.Features.DeveloperTokens.UseCase.Commands.Handlers;

/// <summary>
/// Handles the <see cref="CreateDeveloperTokenCommand"/> command.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Delegates token creation to <see cref="IDeveloperTokenManager"/>.</item>
/// <item>Returns a <see cref="DeveloperTokenCreated"/> DTO with created token details.</item>
/// </list>
/// </remarks>
public class CreateDeveloperTokenHandler(
    IDeveloperTokenManager manager,
    IDeveloperTokenRepository repository,
    IOptions<AuthKitOptions> options)
{
    public async Task<DeveloperTokenCreated> Handle(CreateDeveloperTokenCommand cmd)
    {
        var maxTokens = options.Value.MaxDeveloperTokens;
        var tokens = await repository.GetByDeveloperIdAsync(cmd.DeveloperId);
        
        if (tokens.Count >= maxTokens)
            throw new DeveloperTokenLimitExceededException(cmd.DeveloperId, maxTokens);

        return await manager.CreateAsync(
            cmd.DeveloperId,
            cmd.Name,
            cmd.Scopes,
            cmd.Lifetime
        );
    }
}