using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.Restful.Controllers;

[ApiController]
[Route("sdk/v1/function/vm")]
public class VirtualMachineController : ControllerBase
{
    [HttpGet("dev/createvm")]
    [Authorize(Policy = "DeveloperScope:createvm")]
    public IActionResult CreateDevScope()
    {
        return Ok("Developer token valid and has createvm scope");
    }
    
    [HttpGet("dev/deletevm")]
    [Authorize(Policy = "DeveloperScope:deletevm")]
    public IActionResult DeleteDevScopes()
    {
        return Ok("Developer token valid and has deletevm scope");
    }
    
    [HttpGet("dev/update")]
    [Authorize(Policy = "DeveloperScope:update")]
    public IActionResult UpdateDevScopes()
    {
        return Ok("update scope");
    }
}