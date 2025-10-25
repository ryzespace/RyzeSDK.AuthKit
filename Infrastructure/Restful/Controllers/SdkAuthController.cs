using Application.DTO;
using Application.UseCase.Commands.Requests;
using Domain.Entities;
using Infrastructure.Restful.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Infrastructure.Restful.Controllers;

[ApiController]
[Route("sdk/auth")]
public class SdkAuthController(IMessageBus messageBus) : ControllerBase
{
    [HttpPost("tokens")]
   // [Authorize(Roles = "User")]
    public async Task<IActionResult> Register([FromBody] CreateTokenRequest request)
    {
        var userId2 = User.FindFirst("sub")?.Value;


        var userId = new Guid("0f9b2090-949a-45b2-a924-20cafcd3f864");
        
        var lifetime = request.LifetimeDays.HasValue 
            ? TimeSpan.FromDays(request.LifetimeDays.Value) 
            : (TimeSpan?)null;
        
        var cmd = new CreateDeveloperTokenCommand(
            userId,
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
            createdAt = result.Token.CreatedAt,
            expiresAt = result.Token.ExpiresAt,
            isExpired = result.Token.IsExpired
        });
    }

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
    
    [HttpGet("dev/check")]
    [Authorize(Policy = "DeveloperScope:createvm")]
    public IActionResult CheckDevScope()
    {
        return Ok("Developer token valid and has createvm scope");
    }
    
    [HttpGet("dev/deletevm")]
    [Authorize(Policy = "DeveloperScope:deletevm")]
    public IActionResult DeleteDevScopes()
    {
        return Ok("Developer token valid and has deletevm scope");
    }
}