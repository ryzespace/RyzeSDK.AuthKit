using Application.Features.DeveloperTokens.DTO;
using Application.Features.DeveloperTokens.Interfaces;
using Application.Features.DeveloperTokens.UseCase.Queries.Requests;

namespace Application.Features.DeveloperTokens.UseCase.Queries.Handlers;

/// <summary>
/// Handles the <see cref="GetDeveloperTokensQuery"/> query.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Retrieves all developer tokens for a specific developer.</item>
/// <item>Maps domain entities to <see cref="DeveloperTokenDto"/>.</item>
/// </list>
/// </remarks>
public class GetDeveloperTokensQueryHandler(IDeveloperTokenManager manager)
{
    public async Task<IReadOnlyList<DeveloperTokenDto>> Handle(GetDeveloperTokensQuery query, CancellationToken ct = default)
    {
        var tokens = await manager.GetByDeveloperAsync(query.DeveloperId, ct);
        return tokens
            .Select(DeveloperTokenDto.FromDomain)
            .ToList()
            .AsReadOnly();
    }
}