using System.Text.Json.Serialization;

namespace Infrastructure.Restful.Jwks.DTO.Response;

/// <summary>
/// Represents statistical information about the key store state.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Includes total key count and key age distribution.</item>
/// <item>Used for administrative and monitoring endpoints.</item>
/// </list>
/// </remarks>
public record KeyStoreStatsResponse(
    [property: JsonPropertyName("total_keys")] int TotalKeys,
    [property: JsonPropertyName("oldest_key_age")] TimeSpan? OldestKeyAge,
    [property: JsonPropertyName("newest_key_age")] TimeSpan? NewestKeyAge,
    [property: JsonPropertyName("key_ids")] IReadOnlyList<string> KeyIds
)
{
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}