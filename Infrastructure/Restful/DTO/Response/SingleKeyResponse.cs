using System.Text.Json.Serialization;
using Application.DTO.Key;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Restful.DTO.Response;

/// <summary>
/// Represents a single JSON Web Key (JWK) with its associated metadata.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Used for debugging or administrative inspection of specific keys.</item>
/// <item>Contains the public key material and related lifecycle metadata.</item>
/// </list>
/// </remarks>
public record SingleKeyResponse(
    [property: JsonPropertyName("key")] PublicJwkDto? Key,
    [property: JsonPropertyName("metadata")] KeyMetadata? Metadata
);