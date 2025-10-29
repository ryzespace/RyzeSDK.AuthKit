using System.Security.Claims;
using Application.DTO;
using Application.UseCase.Commands.Requests;
using Infrastructure.Restful.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace Infrastructure.Restful.Controllers;

[ApiController]
[Route("sdk/auth")]
public class SdkAuthController(IMessageBus messageBus, ILogger<SdkAuthController> logger) : ControllerBase
{
    [HttpPost("tokens")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Register([FromBody] CreateTokenRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized("Missing or invalid 'sub' claim");

        var lifetime = request.LifetimeDays.HasValue 
            ? TimeSpan.FromDays(request.LifetimeDays.Value) 
            : (TimeSpan?)null;
        
        var cmd = new CreateDeveloperTokenCommand(
            new Guid(userId),
            request.Name,
            request.Description,
            request.Scopes,
            lifetime
        );
        
        var result = await messageBus.InvokeAsync<DeveloperTokenCreated>(cmd);

        return Ok(new 
        {
            token = result.Jwt,
            id = result.Token.Id,
            developerId = result.Token.DeveloperId,
            scopes = result.Token.Scopes,
            lifetime = result.Token.Lifetime,
        });
    }

    [HttpPost("client/{key}")]
    public async Task<IActionResult> Client(string key)
    {
        var cmd = new ClientAuthTokenCommand(
            key
        );
        await messageBus.InvokeAsync(cmd);
        
        return Ok(new 
        {
                scopes = "scopes",
                createdAt = "createdAt",
                expiresAt = "expiresAt",
                isExpired = "isExpired"
         });   
    }

    [HttpGet("roles")]
    [Authorize]
    public IActionResult Roles()
    {
        var roles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        logger.LogInformation("user {User} has {Count} roles: {Roles}",
            User.Identity?.Name ?? "unknown", roles.Count, string.Join(", ", roles));

        return Ok(roles);
    }
    
    [HttpGet("debug-auth")]
    public IActionResult DebugAuth()
    {
        var authHeader = Request.Headers["Authorization"].ToString();
    
        return Ok(new
        {
            HasAuthHeader = !string.IsNullOrEmpty(authHeader),
            AuthHeaderPrefix = authHeader.Length > 20 ? authHeader.Substring(0, 20) : authHeader,
            User.Identity?.IsAuthenticated,
            User.Identity?.AuthenticationType,
            IdentityType = User.Identity?.GetType().FullName,
            PrincipalType = User.GetType().FullName,
            ClaimsCount = User.Claims.Count(),
            AllClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList(),
            RoleClaimsCount = User.Claims.Count(c => c.Type == ClaimTypes.Role),
            RoleClaims = User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList()
        });
    }
}