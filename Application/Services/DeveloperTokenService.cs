using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services;

/// <summary>
/// Service responsible for generating JWT tokens for <see cref="DeveloperToken"/> entities.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Generates signed JWT tokens including developer ID, token name, and scopes as claims.</item>
/// <item>Uses HMAC SHA256 algorithm and a symmetric key from the <c>JWT_KEY</c> environment variable.</item>
/// <item>Throws <see cref="InvalidOperationException"/> if the <c>JWT_KEY</c> environment variable is missing.</item>
/// </list>
/// </remarks>
public class DeveloperTokenService : IDeveloperTokenService
{
    private readonly string _jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
                                      ?? throw new InvalidOperationException("Missing JWT_KEY environment variable.");

    /// <inheritdoc />
    public Task<string> GenerateToken(DeveloperToken token)
    {
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));

        var claims = new List<Claim>
        {
            new("sub", token.DeveloperId.ToString()),
            new("name", token.Name.ToString())
        };

        claims.AddRange(
            token.Scopes.Select(scope => new Claim("scope", scope.Value))
        );

        var jwt = new JwtSecurityToken(
            expires: token.Lifetime.ExpiresAt?.UtcDateTime,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
            claims: claims
        );

        return Task.FromResult(handler.WriteToken(jwt));
    }
}