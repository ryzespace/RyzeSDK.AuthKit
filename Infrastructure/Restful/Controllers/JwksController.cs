using Application.DTO;
using Application.DTO.Key;
using Application.Interfaces.JWTKey;
using Infrastructure.Restful.DTO.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Restful.Controllers;

/// <summary>
/// Exposes the JSON Web Key Set (JWKS) endpoint for public key discovery.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Compliant with RFC 7517 (JSON Web Key) and OpenID Connect Discovery.</item>
/// <item>Returns only non-revoked public keys for JWT signature verification.</item>
/// <item>Supports key rotation by exposing multiple active keys simultaneously.</item>
/// </list>
/// </remarks>
[ApiController]
[Route(".well-known/jwks.json")]
public class JwksController(IJwtKeyStore keyStore, ILogger<JwksController> logger) : ControllerBase
{
    #region Public JWKS Endpoints

    /// <summary>
    /// Returns all currently active public keys in JWKS format.
    /// </summary>
    /// <returns>A <see cref="JwksResponse"/> containing all active public keys.</returns>
    [AllowAnonymous]
    [HttpGet]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, VaryByHeader = "Accept")]
    [ProducesResponseType(typeof(JwksResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public IActionResult GetJwks()
    {
        try
        {
            var publicKeys = keyStore.GetPublicJwks()
                .Select(k => k with
                {
                    X5c = k.X5c.ToArray()
                })
                .ToList();

            logger.Log(
                publicKeys.Count == 0 ? LogLevel.Warning : LogLevel.Debug,
                publicKeys.Count == 0
                    ? "No public keys available in JWKS endpoint - possible configuration issue"
                    : "Returning {KeyCount} public keys in JWKS",
                publicKeys.Count
            );

            Response.Headers["X-Key-Count"] = publicKeys.Count.ToString();
            Response.Headers["X-Generated-At"] = DateTime.UtcNow.ToString("O");

            return Ok(new JwksResponse(publicKeys));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve JWKS from key store");
            return StatusCode(
                500,
                new ErrorResponse(
                    Error: "internal_error",
                    ErrorDescription: "Failed to retrieve public keys"
                )
            );
        }
    }

    /// <summary>
    /// Retrieves metadata for a single key by Key ID (KID).
    /// </summary>
    /// <param name="kid">The unique key identifier.</param>
    /// <returns>A <see cref="SingleKeyResponse"/> with the key and metadata, or 404 if not found.</returns>
    [AllowAnonymous]
    [HttpGet("jwks/keys/{kid}")]
    [ProducesResponseType(typeof(SingleKeyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult GetKeyByKid([FromRoute] string kid)
    {
        try
        {
            var keyDto = keyStore.GetPublicJwks()
                .Where(k => k.Kid == kid)
                .Select(k => new PublicJwkDto(
                    Kty: k.Kty,
                    Use: k.Use,
                    Kid: k.Kid,
                    Alg: k.Alg,
                    N: k.N,
                    E: k.E,
                    X5c: k.X5c?.ToArray() ?? []
                ))
                .FirstOrDefault();

            if (keyDto is null)
            {
                logger.LogWarning("Key with kid '{Kid}' not found or revoked", kid);
                return NotFound(new ErrorResponse(
                    Error: "key_not_found",
                    ErrorDescription: $"Key with kid '{kid}' not found or has been revoked"
                ));
            }

            var metadata = keyStore.GetMetadata(kid);
            return Ok(new SingleKeyResponse(keyDto, metadata));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving key with kid '{Kid}'", kid);
            return StatusCode(
                500,
                new ErrorResponse(
                    Error: "internal_error",
                    ErrorDescription: "Failed to retrieve key"
                )
            );
        }
    }

    #endregion

    #region Health & Monitoring

    /// <summary>
    /// Performs a health check of the JWKS and key store.
    /// </summary>
    /// <returns>A <see cref="HealthResponse"/> describing availability and active keys.</returns>
    [AllowAnonymous]
    [HttpGet("jwks/health")]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(HealthResponse), StatusCodes.Status503ServiceUnavailable)]
    public IActionResult Health()
    {
        try
        {
            var publicKeys = keyStore.GetPublicJwks().ToList();
            var activeCredentials = keyStore.GetActiveSigningCredentials();
            var activeKid = activeCredentials.Kid;

            var activeKeyMetadata = activeKid != null ? keyStore.GetMetadata(activeKid) : null;
            var isHealthy = publicKeys.Count > 0 && activeKid != null;

            var response = new HealthResponse(
                Status: isHealthy ? "healthy" : "degraded",
                AvailableKeys: publicKeys.Count,
                ActiveKeyId: activeKid,
                ActiveKeyCreatedAt: activeKeyMetadata?.CreatedAt,
                Details: new Dictionary<string, object>
                {
                    ["has_active_key"] = activeKid != null,
                    ["key_ids"] = publicKeys.Select(k => k.Kid).ToList()
                }
            );

            var statusCode = isHealthy ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable;
            return StatusCode(statusCode, response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "JWKS health check failed");
            return StatusCode(
                503,
                new HealthResponse(
                    Status: "unhealthy",
                    AvailableKeys: 0,
                    ActiveKeyId: null,
                    ActiveKeyCreatedAt: null,
                    Details: new Dictionary<string, object> { ["error"] = ex.Message }
                )
            );
        }
    }

    /// <summary>
    /// Returns key store statistics, including age of the oldest and newest keys.
    /// </summary>
    /// <returns>A <see cref="KeyStoreStatsResponse"/> with key counts and age distribution.</returns>
    [AllowAnonymous]
    [HttpGet("jwks/stats")]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
    [ProducesResponseType(typeof(KeyStoreStatsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public IActionResult GetStats()
    {
        try
        {
            var publicKeys = keyStore.GetPublicJwks().ToList();
            var allMetadata = publicKeys
                .Select(k => keyStore.GetMetadata(k.Kid))
                .Where(m => m is not null)
                .ToList();

            var oldestKey = allMetadata.MinBy(m => m!.CreatedAt);
            var newestKey = allMetadata.MaxBy(m => m!.CreatedAt);

            return Ok(new KeyStoreStatsResponse(
                TotalKeys: publicKeys.Count,
                OldestKeyAge: oldestKey != null ? DateTime.UtcNow - oldestKey.CreatedAt : null,
                NewestKeyAge: newestKey != null ? DateTime.UtcNow - newestKey.CreatedAt : null,
                KeyIds: publicKeys.Select(k => k.Kid).ToList()
            ));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve key store statistics");
            return StatusCode(
                500,
                new ErrorResponse(
                    Error: "internal_error",
                    ErrorDescription: "Failed to retrieve statistics"
                )
            );
        }
    }

    #endregion
}
