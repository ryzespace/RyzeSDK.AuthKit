using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.Controllers;

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
}