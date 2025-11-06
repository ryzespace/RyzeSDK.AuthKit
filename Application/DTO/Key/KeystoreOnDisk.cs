namespace Application.DTO.Key;

/// <summary>
/// Represents the serialized keystore persisted on disk (before encryption).
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Contains a collection of all RSA key records used for JWT signing.</item>
/// <item>Defines which key is currently active for signing new tokens.</item>
/// </list>
/// </remarks>
public sealed record KeystoreOnDisk(
    string ActiveKid,
    List<KeystoreRecordOnDisk> Records
);