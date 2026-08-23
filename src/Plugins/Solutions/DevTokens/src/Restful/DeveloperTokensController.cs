using System.Security.Claims;
using DevTokens.DTO;
using DevTokens.UseCase.Commands.Requests;
using DevTokens.UseCase.Queries.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using Wolverine;

namespace DevTokens.Restful;

/// <summary>
/// Provides REST API endpoints for managing developer tokens.
/// </summary>
/// <remarks>
/// <para>
/// The controller exposes operations for creating, deleting, and retrieving
/// developer tokens belonging to the authenticated user.
/// </para>
/// <list type="bullet">
/// <item>Uses Wolverine to dispatch commands and queries to the corresponding application handlers. </item>
/// <item>Requires an authenticated user with the <c>User</c> role for all endpoints. </item>
/// <item>Extracts the authenticated developer identifier from the <see cref="ClaimTypes.NameIdentifier"/> claim. </item>
/// <item>Documents the available operations and responses using Swagger annotations. </item>
/// </list>
/// </remarks>
[ApiController]
[Route("sdk/developer-tokens")]
[SwaggerTag("Operations related to developer tokens")]
public class DeveloperTokensController(
    IMessageBus messageBus,
    ILogger<DeveloperTokensController> logger) : ControllerBase
{
    /// <summary>
    /// Gets the unique identifier of the currently authenticated user.
    /// </summary>
    /// <returns>
    /// The parsed user identifier, or <c>null</c> when the
    /// <see cref="ClaimTypes.NameIdentifier"/> claim is missing or invalid.
    /// </returns>
    private Guid? GetUserId() =>
        Guid.TryParse(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            out var id)
            ? id
            : null;

    /// <summary>
    /// Registers a new developer token for the authenticated user.
    /// </summary>
    /// <param name="request">
    /// The request containing the token name, description, scopes,
    /// and optional lifetime in days.
    /// </param>
    /// <returns>
    /// An HTTP 200 response containing the generated JWT, short API key,
    /// token identifier, developer identifier, scopes, and lifetime.
    /// Returns HTTP 401 when the authenticated user identifier cannot be resolved.
    /// </returns>
    [HttpPost]
    [Authorize(Roles = "User")]
    [SwaggerOperation(
        Summary = "Registers new developer token",
        Description = "Creates developer token for the authenticated user. Requires token name, description, scopes, and optional lifetime in days.")]
    [SwaggerResponse(
        200,
        "Returns the created developer token",
        typeof(DeveloperTokenCreated))]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task<IActionResult> Register(
        [FromBody] CreateTokenRequest request)
    {
        var userId = GetUserId();

        if (userId is null)
            return Unauthorized();

        var lifetime = request.LifetimeDays.HasValue
            ? TimeSpan.FromDays(request.LifetimeDays.Value)
            : (TimeSpan?)null;

        var command = new CreateDeveloperTokenCommand(
            userId.Value,
            request.Name,
            request.Description,
            request.Scopes,
            lifetime);

        var result =
            await messageBus.InvokeAsync<DeveloperTokenCreated>(command);

        logger.LogInformation(
            "Token created: TokenId={TokenId}, DeveloperId={DeveloperId}",
            result.Token.Id,
            result.Token.DeveloperId);

        return Ok(new
        {
            jwt = result.Jwt,
            key = result.ShortKey,
            id = result.Token.Id,
            developerId = result.Token.DeveloperId,
            scopes = result.Token.Scopes,
            lifetime = result.Token.Lifetime
        });
    }

    /// <summary>
    /// Deletes an existing developer token by its unique identifier.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token to delete. </param>
    /// <returns>
    /// An HTTP 200 response when the token is successfully deleted.
    /// </returns>
    [HttpDelete("{tokenId:guid}")]
    [Authorize(Roles = "User")]
    [SwaggerOperation(
        Summary = "Deletes a developer token",
        Description = "Deletes the specified developer token by its unique ID.")]
    [SwaggerResponse(200, "Token deleted successfully")]
    [SwaggerResponse(401, "Unauthorized")]
    [SwaggerResponse(404, "Token not found")]
    public async Task<IActionResult> Delete(Guid tokenId)
    {
        var command = new DeleteTokenCommand(tokenId);

        await messageBus.InvokeAsync(command);

        logger.LogInformation(
            "Token deleted successfully: TokenId={TokenId}",
            tokenId);

        return Ok("Token deleted successfully");
    }

    /// <summary>
    /// Retrieves all developer tokens belonging to the authenticated user.
    /// </summary>
    /// <returns>
    /// An HTTP 200 response containing a read-only list of
    /// <see cref="DeveloperTokenDto"/> instances.
    /// Returns HTTP 401 when the authenticated user identifier cannot be resolved.
    /// </returns>
    [HttpGet]
    [Authorize(Roles = "User")]
    [SwaggerOperation(
        Summary = "Retrieves all developer tokens",
        Description = "Returns a read-only list of developer tokens for the authenticated user.")]
    [SwaggerResponse(
        200,
        "List of developer tokens",
        typeof(IReadOnlyList<DeveloperTokenDto>))]
    [SwaggerResponse(401, "Unauthorized")]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();

        if (userId is null)
            return Unauthorized();

        var query = new GetDeveloperTokensQuery(userId.Value);
        var result = await messageBus.InvokeAsync<IReadOnlyList<DeveloperTokenDto>>(query);

        logger.LogInformation("Retrieved {Count} tokens for DeveloperId={DeveloperId}", result.Count, userId);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves specific developer token by its unique identifier.
    /// </summary>
    /// <param name="tokenId">The unique identifier of the developer token to retrieve.</param>
    /// <returns>
    /// An HTTP 200 response containing the requested
    /// <see cref="DeveloperTokenDto"/>, or HTTP 404 when the token does not exist.
    /// </returns>
    [HttpGet("{tokenId:guid}")]
    [Authorize(Roles = "User")]
    [SwaggerOperation(
        Summary = "Retrieves a developer token by ID",
        Description = "Returns a single developer token by its unique ID.")]
    [SwaggerResponse(
        200,
        "Developer token details",
        typeof(DeveloperTokenDto))]
    [SwaggerResponse(401, "Unauthorized")]
    [SwaggerResponse(404, "Token not found")]
    public async Task<IActionResult> GetById(Guid tokenId)
    {
        var query = new GetDeveloperTokenByIdQuery(tokenId);

        var result =
            await messageBus.InvokeAsync<DeveloperTokenDto?>(query);

        if (result is null)
            return NotFound();

        logger.LogInformation("Token retrieved successfully: TokenId={TokenId}", tokenId);
        return Ok(result);
    }
}