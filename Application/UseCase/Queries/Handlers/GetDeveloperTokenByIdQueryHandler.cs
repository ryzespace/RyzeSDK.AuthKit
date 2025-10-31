using Application.DTO;
using Application.Interfaces;
using Application.UseCase.Queries.Requests;

namespace Application.UseCase.Queries.Handlers;

/// <summary>
/// Handles the <see cref="GetDeveloperTokenByIdQuery"/> query.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Retrieves a specific developer token by its unique identifier.</item>
/// <item>Returns a <see cref="DeveloperTokenDto"/> if found, otherwise <c>null</c>.</item>
/// </list>
/// </remarks>
public class GetDeveloperTokenByIdQueryHandler(IDeveloperTokenManager manager)
{
    public async Task<DeveloperTokenDto?> Handle(GetDeveloperTokenByIdQuery query)
    {
        var token = await manager.GetByIdAsync(query.TokenId);
        if (token == null)
            return null;

        return new DeveloperTokenDto
        {
            Id = token.Id,
            DeveloperId = token.DeveloperId,
            Name = token.Name.ToString(),
            Scopes = token.Scopes.Select(s => s.Value).ToList(),
            CreatedAt = token.Lifetime.CreatedAt,
            ExpiresAt = token.Lifetime.ExpiresAt
        };
    }
}
