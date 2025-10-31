using System.Security.Claims;
using Application.DTO;
using Application.UseCase.Commands.Requests;
using Application.UseCase.Queries.Requests;
using Infrastructure.Restful.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Wolverine;

namespace Infrastructure.Restful.Controllers;

/// <summary>
/// REST API controller for managing developer tokens.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Supports creation, deletion, and retrieval of developer tokens.</item>
/// <item>Integrates with Wolverine message bus for CQRS command/query handling.</item>
/// <item>Requires authenticated users with role <c>User</c>.</item>
/// </list>
/// </remarks>
[ApiController]
[Route("sdk/developer-tokens")]
public class DeveloperTokensController(IMessageBus messageBus, ILogger<DeveloperTokensController> logger) : ControllerBase
{
    private Guid? GetUserId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
    
    /// <summary>
    /// Registers a new developer token.
    /// </summary>
    /// <param name="request">The request containing token name, description, scopes, and optional lifetime.</param>
    /// <returns>HTTP 200 with created token info, or 401 if unauthorized.</returns>
    [HttpPost]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Register([FromBody] CreateTokenRequest request)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        
        var lifetime = request.LifetimeDays.HasValue 
            ? TimeSpan.FromDays(request.LifetimeDays.Value) 
            : (TimeSpan?)null;
        
        var cmd = new CreateDeveloperTokenCommand(
            (Guid)userId,
            request.Name,
            request.Description,
            request.Scopes,
            lifetime
        );
        var result = await messageBus.InvokeAsync<DeveloperTokenCreated>(cmd);
       
        logger.LogInformation("Token created: TokenId={TokenId}, DeveloperId={DeveloperId}", 
            result.Token.Id, result.Token.DeveloperId);
        
        return Ok(new 
        {
            token = result.Jwt,
            id = result.Token.Id,
            developerId = result.Token.DeveloperId,
            scopes = result.Token.Scopes,
            lifetime = result.Token.Lifetime,
        });
    }
    
    /// <summary>
    /// Deletes an existing developer token by ID.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the token to delete.</param>
    /// <returns>HTTP 200 if deletion succeeded.</returns>
    [HttpDelete("{tokenId:guid}")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> Delete(Guid tokenId)
    {
        var cmd = new DeleteTokenCommand(tokenId);
        await messageBus.InvokeAsync(cmd);
        
        logger.LogInformation("Token deleted successfully: TokenId={TokenId}", tokenId);

        return Ok("Token deleted successfully");
    }
    
    /// <summary>
    /// Retrieves all developer tokens for the current user.
    /// </summary>
    /// <returns>HTTP 200 with a read-only list of <see cref="DeveloperTokenDto"/> or 401 if unauthorized.</returns>
    [HttpGet]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();
        
        var query = new GetDeveloperTokensQuery((Guid)userId);
        var result = await messageBus.InvokeAsync<IReadOnlyList<DeveloperTokenDto>>(query);
      
        logger.LogInformation("Retrieved {Count} tokens for DeveloperId={DeveloperId}", result.Count, userId);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific developer token by ID.
    /// </summary>
    /// <param name="tokenId">The token ID to retrieve.</param>
    /// <returns>HTTP 200 with <see cref="DeveloperTokenDto"/> if found, 404 if not found.</returns>
    [HttpGet("{tokenId:guid}")]
    [Authorize(Roles = "User")]
    public async Task<IActionResult> GetById(Guid tokenId)
    {
        var query = new GetDeveloperTokenByIdQuery(tokenId);
        var result = await messageBus.InvokeAsync<DeveloperTokenDto?>(query);

        if (result is null)
            return NotFound();
        
        logger.LogInformation("Token retrieved successfully: TokenId={TokenId}", tokenId);
        return Ok(result);
    }
}