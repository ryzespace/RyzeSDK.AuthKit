using Application.UseCase.Commands.Requests;
using Infrastructure.Restful.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Infrastructure.Restful.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IMessageBus messageBus) : ControllerBase
{
    
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request)
    {
        var command = new RegisterUserCommand(
            request.FirstName,
            request.LastName,
            request.Username,
            request.Email,
            request.Password
        );
        await messageBus.InvokeAsync(command);
        
        return Ok(new
        {
            message = "User registered successfully.",
            data = new
            {
                firstName = request.FirstName,
                lastName = request.LastName,
                username = request.Username,
                email = request.Email
            }
        });
    }
}