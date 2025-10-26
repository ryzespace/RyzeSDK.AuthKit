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
}