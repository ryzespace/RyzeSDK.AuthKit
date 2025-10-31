using Application.Interfaces;
using Application.UseCase.Commands.Requests;

namespace Application.UseCase.Commands.Handlers;

/// <summary>
/// Handles the <see cref="DeleteTokenCommand"/> command.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Delegates token deletion to <see cref="IDeveloperTokenManager"/>.</item>
/// <item>Used to remove an existing <c>DeveloperToken</c> by its unique identifier.</item>
/// </list>
/// </remarks>
public class DeleteTokenCommandHandler(IDeveloperTokenManager manager)
{
    public async Task Handle(DeleteTokenCommand cmd) =>
        await manager.DeleteAsync(
            cmd.TokenId
        );
}