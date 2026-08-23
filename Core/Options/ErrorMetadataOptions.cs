namespace Core.Options;

/// <summary>
/// Represents configuration options for error documentation metadata.
/// </summary>
public sealed record ErrorMetadataOptions
{
    /// <summary>
    /// Gets the base URL used to reference error documentation.
    /// </summary>
    public string DocsBaseUrl { get; init; } = "https://localhost:8080/errors";
}