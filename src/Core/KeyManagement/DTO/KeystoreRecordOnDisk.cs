namespace Core.KeyManagement.DTO;

/// <summary>
/// Represents single stored RSA key entry within the serialized keystore.
/// </summary>
/// <remarks>
/// <para>
/// The record contains the metadata required to identify and manage the key
/// together with its private RSA key material in serialized representation.
/// </para>
/// <para>
/// The private key is encoded as Base64 string for persistence and should
/// only be handled by the key management infrastructure. It must not be
/// exposed through public key endpoints such as JWKS.
/// </para>
/// </remarks>
public sealed record KeystoreRecordOnDisk
{
    /// <summary>
    /// Gets the metadata associated with the stored RSA key.
    /// </summary>
    public required KeyMetadata Metadata { get; init; }

    /// <summary>
    /// Gets the Base64 encoded private RSA key material.
    /// </summary>
    /// <remarks>
    /// This value contains sensitive cryptographic material and must be
    /// protected from unauthorized access.
    /// </remarks>
    public required string PrivateKeyBase64 { get; init; }
}