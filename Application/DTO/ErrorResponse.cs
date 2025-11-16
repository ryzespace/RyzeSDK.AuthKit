using System.Text.Json.Serialization;

namespace Application.DTO;

/// <summary>
/// Standardized error response DTO for RESTful endpoints.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Maps to JSON properties `error` and `error_description` for client consumption.</item>
/// <item>Includes a UTC timestamp indicating when the error was generated.</item>
/// <item>Intended for consistent error reporting across the API.</item>
/// </list>
/// </remarks>
public record ErrorResponse(
    [property: JsonPropertyName("error")] string Error,
    [property: JsonPropertyName("error_description")] string ErrorDescription
) {
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}