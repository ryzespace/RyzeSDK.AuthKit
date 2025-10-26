using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.Restful.Controllers;

[ApiController]
[Route("sdk/function")]
public class MarketplaceController : ControllerBase
{
    [HttpGet("check_new_function")]
    [Authorize]
    public IActionResult check_new_function()
    {
        return Ok("Hello from SDK");
    }
}