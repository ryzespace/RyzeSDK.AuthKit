using Infrastructure.Restful.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.Restful.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SdkAuthController : ControllerBase
{
    [HttpPost("register")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var userId = User.FindFirst("sub")?.Value;

        return Ok(new { message = "Register successful userID: " + userId });
    }
}