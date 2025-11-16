using Application.UseCase.Commands.Requests;

namespace Application.UseCase.Commands.Handlers;

public class ClientAuthTokenCommandHandler
{
    public async Task Handle(ClientAuthTokenCommand cmd)
    {
        Console.WriteLine(cmd);
    }
}