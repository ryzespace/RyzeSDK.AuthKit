using System.Text.Json.Serialization;
using Application.DTO.Key;
using Application.Interfaces.JWTKey;
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
    /// <returns>A <see cref="JwksResponse"/> containing active public keys.</returns>
    [AllowAnonymous]
    [HttpGet]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any, VaryByHeader = "Accept")]
    [ProducesResponseType(typeof(JwksResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public IActionResult GetJwks()
    {
        try
        {
            var publicKeys = keyStore.GetPublicJwks().ToList();
            
            logger.Log(
                publicKeys.Count == 0 ? LogLevel.Warning : LogLevel.Debug,
                publicKeys.Count == 0 
                    ? "No public keys available in JWKS endpoint - this may indicate a configuration issue"
                    : "Returning {KeyCount} public keys in JWKS",
                publicKeys.Count
            );

            var jwks = new JwksResponse
            {
                Keys = publicKeys
            };
            
            Response.Headers["X-Key-Count"] = publicKeys.Count.ToString();
            Response.Headers["X-Generated-At"] = DateTime.UtcNow.ToString("O");

            return Ok(jwks);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve JWKS from key store");
            return StatusCode(500, new ErrorResponse
            {
                Error = "internal_error",
                ErrorDescription = "Failed to retrieve public keys",
                Timestamp = DateTime.UtcNow
            });
        }
    }
    
    /// <summary>
    /// Retrieves metadata for a single key by Key ID (KID).
    /// </summary>
    /// <param name="kid">The unique key identifier.</param>
    /// <returns>A <see cref="SingleKeyResponse"/> with key and metadata, or 404 if not found.</returns>
    [AllowAnonymous]
    [HttpGet("jwks/keys/{kid}")]
    [ProducesResponseType(typeof(SingleKeyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public IActionResult GetKeyByKid([FromRoute] string kid)
    {
        try
        {
            var allKeys = keyStore.GetPublicJwks().ToList();
            var key = allKeys.FirstOrDefault(k => k.Kid == kid);

            if (key == null)
            {
                logger.LogWarning("Key with kid '{Kid}' not found or is revoked", kid);
                return NotFound(new ErrorResponse
                {
                    Error = "key_not_found",
                    ErrorDescription = $"Key with kid '{kid}' not found or has been revoked",
                    Timestamp = DateTime.UtcNow
                });
            }

            var metadata = keyStore.GetMetadata(kid);

            return Ok(new SingleKeyResponse
            {
                Key = key,
                Metadata = metadata
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving key with kid '{Kid}'", kid);
            return StatusCode(500, new ErrorResponse
            {
                Error = "internal_error",
                ErrorDescription = "Failed to retrieve key",
                Timestamp = DateTime.UtcNow
            });
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public IActionResult Health()
    {
        try
        {
            var publicKeys = keyStore.GetPublicJwks().ToList();
            var activeCredentials = keyStore.GetActiveSigningCredentials();
            var activeKid = activeCredentials?.Kid;
            
            KeyMetadata? activeKeyMetadata = null;
            if (activeKid != null)
            {
                activeKeyMetadata = keyStore.GetMetadata(activeKid);
            }

            var isHealthy = publicKeys.Count > 0 && activeKid != null;

            var response = new HealthResponse
            {
                Status = isHealthy ? "healthy" : "degraded",
                AvailableKeys = publicKeys.Count,
                ActiveKeyId = activeKid,
                ActiveKeyCreatedAt = activeKeyMetadata?.CreatedAt,
                Timestamp = DateTime.UtcNow,
                Details = new Dictionary<string, object>
                {
                    ["has_active_key"] = activeKid != null,
                    ["key_ids"] = publicKeys.Select(k => k.Kid).ToList()
                }
            };

            var statusCode = isHealthy ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable;
            return StatusCode(statusCode, response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "JWKS health check failed");
            return StatusCode(503, new HealthResponse
            {
                Status = "unhealthy",
                AvailableKeys = 0,
                Timestamp = DateTime.UtcNow,
                Details = new Dictionary<string, object>
                {
                    ["error"] = ex.Message
                }
            });
        }
    }
    
    /// <summary>
    /// Returns key store statistics, including age of oldest and newest keys.
    /// </summary>
    /// <returns>A <see cref="KeyStoreStatsResponse"/> with key counts and ages.</returns>
    [HttpGet("jwks/stats")]
    [AllowAnonymous]
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)] // 5 minutes
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetStats()
    {
        try
        {
            var publicKeys = keyStore.GetPublicJwks().ToList();
            var allMetadata = publicKeys
                .Select(k => keyStore.GetMetadata(k.Kid))
                .Where(m => m != null)
                .ToList();

            var oldestKey = allMetadata.MinBy(m => m!.CreatedAt);
            var newestKey = allMetadata.MaxBy(m => m!.CreatedAt);

            return Ok(new KeyStoreStatsResponse
            {
                TotalKeys = publicKeys.Count,
                OldestKeyAge = oldestKey != null 
                    ? DateTime.UtcNow - oldestKey.CreatedAt 
                    : null,
                NewestKeyAge = newestKey != null 
                    ? DateTime.UtcNow - newestKey.CreatedAt 
                    : null,
                KeyIds = publicKeys.Select(k => k.Kid).ToList(),
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to retrieve key store statistics");
            return StatusCode(500, new ErrorResponse
            {
                Error = "internal_error",
                ErrorDescription = "Failed to retrieve statistics",
                Timestamp = DateTime.UtcNow
            });
        }
    }
    #endregion
    
    #region Response Models

    /// <summary>
    /// Standard JWKS response format (RFC 7517).
    /// </summary>
    public class JwksResponse
    {
        [JsonPropertyName("keys")]
        public IEnumerable<Microsoft.IdentityModel.Tokens.JsonWebKey> Keys { get; set; } 
            = Array.Empty<Microsoft.IdentityModel.Tokens.JsonWebKey>();
    }

    /// <summary>
    /// Single key response with metadata.
    /// </summary>
    public class SingleKeyResponse
    {
        [JsonPropertyName("key")]
        public Microsoft.IdentityModel.Tokens.JsonWebKey? Key { get; set; }
        
        [JsonPropertyName("metadata")]
        public KeyMetadata? Metadata { get; set; }
    }
    
    /// <summary>
    /// Health check response.
    /// </summary>
    public class HealthResponse
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;
        
        [JsonPropertyName("available_keys")]
        public int AvailableKeys { get; set; }
        
        [JsonPropertyName("active_key_id")]
        public string? ActiveKeyId { get; set; }
        
        [JsonPropertyName("active_key_created_at")]
        public DateTimeOffset? ActiveKeyCreatedAt { get; set; }
        
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
        
        [JsonPropertyName("details")]
        public Dictionary<string, object>? Details { get; set; }
    }

    /// <summary>
    /// Key store statistics response.
    /// </summary>
    public class KeyStoreStatsResponse
    {
        [JsonPropertyName("total_keys")]
        public int TotalKeys { get; set; }
        
        [JsonPropertyName("oldest_key_age")]
        public TimeSpan? OldestKeyAge { get; set; }
        
        [JsonPropertyName("newest_key_age")]
        public TimeSpan? NewestKeyAge { get; set; }
        
        [JsonPropertyName("key_ids")]
        public List<string> KeyIds { get; set; } = [];
        
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Error response format.
    /// </summary>
    public class ErrorResponse
    {
        [JsonPropertyName("error")]
        public string Error { get; set; } = string.Empty;
        
        [JsonPropertyName("error_description")]
        public string ErrorDescription { get; set; } = string.Empty;
        
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }
    #endregion
}