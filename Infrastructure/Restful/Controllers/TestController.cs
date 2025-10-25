using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.Restful.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet("whoami")]
    [Authorize]
    public IActionResult WhoAmI()
    {
        var userId = User.FindFirst("sub")?.Value;
        var email = User.FindFirst("email")?.Value;
        var emailVerified = User.FindFirst("email_verified")?.Value;

        return Ok(new
        {
            userId,
            email,
            emailVerified,
            roles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
        });
    }
    
    [HttpGet("seller-info")]
    [Authorize(Roles = "Seller")]
    public IActionResult SellerInfo()
    {
        var userId = User.FindFirst("sub")?.Value;
        var email = User.FindFirst("email")?.Value;
        var emailVerified = User.FindFirst("email_verified")?.Value;

        return Ok(new
        {
            message = "Dostępne tylko dla roli Seller",
            userId,
            email,
            emailVerified
        });
    }
}