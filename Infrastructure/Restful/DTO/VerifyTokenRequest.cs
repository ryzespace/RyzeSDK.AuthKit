namespace Infrastructure.Restful.DTO;

/// <summary>
/// Request DTO for verifying a developer token using its key.
/// </summary>
public record VerifyTokenRequest(
    string Key
);