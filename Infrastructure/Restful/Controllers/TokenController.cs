using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.Restful.Controllers;

[ApiController]
[Route("sdk/token")]
public class TokenController : ControllerBase
{
    [HttpGet("tokens")]
    public async Task<IActionResult> List([FromQuery] string developerId)
    {
        if (!Guid.TryParse(developerId, out var devId))
            return BadRequest("Invalid DeveloperId");

        return Ok(new { message = "List successful developerID: " + devId });
    }

    [HttpPost("tokens/{id:guid}/revoke")]
    public async Task<IActionResult> Revoke(Guid id)
    {
        return NoContent();
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