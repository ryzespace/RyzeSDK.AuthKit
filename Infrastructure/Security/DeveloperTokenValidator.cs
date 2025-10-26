using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application;
using Application.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Security;

/// <summary>
/// Validates developer JWT tokens and extracts principal information.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Uses a symmetric key from the environment variable <c>JWT_KEY</c> for validation.</item>
/// <item>Validates token signature, lifetime, and claims.</item>
/// <item>Extracts developer ID, token name, and scopes from the token claims.</item>
/// <item>Returns a <see cref="DeveloperTokenPrincipal"/> if the token is valid, otherwise null.</item>
/// </list>
/// </remarks>
public class DeveloperTokenValidator : IDeveloperTokenValidator
{
    private readonly string _jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
                                      ?? throw new InvalidOperationException("Missing JWT_KEY environment variable for AuthKit.");
    private readonly JwtSecurityTokenHandler _handler = new();

    /// <inheritdoc />
    public Task<DeveloperTokenPrincipal?> ValidateAsync(string token)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey)),
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        var principal = _handler.ValidateToken(token, parameters, out var validatedToken);

        if (validatedToken is not JwtSecurityToken)
            return Task.FromResult<DeveloperTokenPrincipal?>(null);

        var developerId = principal.FindFirstValue("sub");
        var tokenName = principal.FindFirstValue("name") ?? "unknown";
        var scopes = principal.FindAll("scope").Select(c => c.Value);

        if (Guid.TryParse(developerId, out var id))
        {
            return Task.FromResult<DeveloperTokenPrincipal?>(
                new DeveloperTokenPrincipal(id, tokenName, scopes)
            );
        }

        return Task.FromResult<DeveloperTokenPrincipal?>(null);
    }
}