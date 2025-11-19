namespace Application.Features.KeyManagement.DTO;

/// <summary>
/// Represents a single stored RSA key entry within the serialized keystore.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Contains metadata such as KID, creation timestamp, and revocation state.</item>
/// <item>Holds the private RSA key encoded in Base64 for persistence.</item>
/// </list>
/// </remarks>
public sealed record KeystoreRecordOnDisk(
    KeyMetadata Metadata,
    string PrivateKeyBase64
);