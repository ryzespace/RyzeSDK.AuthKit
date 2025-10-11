using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.Restful.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult Public()
    {
        return Ok(new { message = "Public endpoint – no authentication required." });
    }

    [HttpGet("private")]
    [Authorize]
    public IActionResult Private()
    {
        var username = User.Identity?.Name ?? "unknown";
        return Ok(new { message = $"Hello {username}, you accessed a private endpoint!" });
    }
    
    [HttpPost("login")]
    [AllowAnonymous]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        return Ok(new
        {
            message = "Login successful (simulate Keycloak token issuance)",
            access_token = "fake-jwt-token",
            refresh_token = "fake-refresh-token"
        });
    }
    
    [HttpPost("refresh")]
    [AllowAnonymous]
    public IActionResult Refresh([FromBody] RefreshRequest request)
    {
        return Ok(new
        {
            access_token = "new-fake-jwt-token",
            refresh_token = "new-fake-refresh-token"
        });
    }
    
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        return Ok(new { message = "User logged out" });
    }
}