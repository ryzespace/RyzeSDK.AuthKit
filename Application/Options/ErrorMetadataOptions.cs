namespace Application.Options;

/// <summary>
/// Configuration options for error documentation metadata.
/// </summary>
public record ErrorMetadataOptions
{
    public string DocsBaseUrl { get; init; } = "https://localhost:8080/errors";
}