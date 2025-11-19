using Application.Features.DeveloperTokens.DTO;
using Infrastructure.Restful.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Infrastructure.Restful.Controllers;

[ApiController]
[Route("sdk/tokens")]
[SwaggerTag("Developer token lifecycle operations: rotate, revoke, verify")]
public class TokenLifecycleController : ControllerBase
{
    /// <summary>
    /// Revoke an existing token and generate a new one (rotate).
    /// </summary>
    /// <param name="tokenId">ID of the token to revoke and rotate.</param>
    /// <returns>New JWT and developer-friendly short key.</returns>
    [HttpPost("{tokenId:guid}/revoke-rotate")]
    [Authorize(Roles = "User")]
    [SwaggerOperation(
        Summary = "Revoke and rotate token",
        Description = "Revokes the specified token and generates a new JWT and shortKey.")]
    [SwaggerResponse(200, "Token rotated successfully", typeof(DeveloperTokenCreated))]
    [SwaggerResponse(401, "Unauthorized")]
    [SwaggerResponse(404, "Token not found")]
    public async Task<IActionResult> RevokeAndRotate(Guid tokenId, CancellationToken ct)
    {
        return null;
    }

    /// <summary>
    /// Verify a JWT against a stored developer token.
    /// </summary>
    /// <param name="request">Contains JWT and shortKey to verify.</param>
    /// <returns>True if the token is valid, false otherwise.</returns>
    [HttpPost("verify")]
    [SwaggerOperation(
        Summary = "Verify token validity",
        Description = "Verifies that the provided JWT matches the stored token hash.")]
    [SwaggerResponse(200, "Verification result", typeof(bool))]
    [SwaggerResponse(401, "Unauthorized")]
    public IActionResult Verify([FromBody] VerifyTokenRequest request)
    {
        return null;
    }
}