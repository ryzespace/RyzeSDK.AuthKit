using System.Text.Json.Serialization;

namespace Infrastructure.Restful.Jwks.DTO.Response;

/// <summary>
/// Represents the current health status of the key management subsystem.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Provides operational metrics and active key information.</item>
/// <item>Useful for monitoring, diagnostics, and system readiness checks.</item>
/// </list>
/// </remarks>
public record HealthResponse(
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("available_keys")] int AvailableKeys,
    [property: JsonPropertyName("active_key_id")] string? ActiveKeyId,
    [property: JsonPropertyName("active_key_created_at")] DateTimeOffset? ActiveKeyCreatedAt,
    [property: JsonPropertyName("details")] Dictionary<string, object>? Details
) {
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}