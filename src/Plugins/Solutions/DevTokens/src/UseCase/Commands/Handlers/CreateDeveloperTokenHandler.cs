using DevTokens.DeveloperTokens;
using DevTokens.DTO;
using DevTokens.Exceptions;
using DevTokens.Interfaces;
using DevTokens.Options;
using DevTokens.Repositories;
using DevTokens.UseCase.Commands.Requests;
using Microsoft.Extensions.Options;

namespace DevTokens.UseCase.Commands.Handlers;

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