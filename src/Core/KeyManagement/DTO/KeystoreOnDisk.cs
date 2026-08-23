namespace Core.KeyManagement.DTO;

/// <summary>
/// Represents serialized keystore persisted on disk before encryption.
/// </summary>
/// <remarks>
/// <para>
/// The keystore contains the RSA key records used for JWT signing together
/// with the identifier of the key currently active for issuing new tokens.
/// </para>
/// <para>
/// The serialized representation is intended for persistence and is encrypted
/// before being written to disk by the key management infrastructure.
/// </para>
/// </remarks>
public sealed record KeystoreOnDisk
{
    /// <summary>
    /// Gets the identifier of the key currently active for signing new tokens.
    /// </summary>
    /// <remarks>The value corresponds to the Kid of one of the records</remarks>
    public required string ActiveKid { get; init; }

    /// <summary>
    /// Gets the RSA key records contained in the keystore.
    /// </summary>
    /// <remarks>
    /// The collection contains the persisted representation of all keys
    /// managed by the key store, including active, inactive, and revoked keys.
    /// </remarks>
    public required List<KeystoreRecordOnDisk> Records { get; init; }
}