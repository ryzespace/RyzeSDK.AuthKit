using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services;

public class DeveloperTokenService : IDeveloperTokenService
{
    private readonly string _jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
                                      ?? throw new InvalidOperationException("Missing JWT_KEY environment variable.");

    public string GenerateToken(DeveloperToken token)
    {
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));

        var claims = new List<Claim>
        {
            new("sub", token.DeveloperId.ToString()),
            new("name", token.Name.ToString())
        };
        
        claims.AddRange(
            token.Scopes.Select(
                scope => new Claim(
                    "scope", scope.Name
                )
            )
        );

        var jwt = new JwtSecurityToken(
            expires: token.Lifetime.ExpiresAt?.UtcDateTime,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256),
            claims: claims
        );

        return handler.WriteToken(jwt);
    }
}