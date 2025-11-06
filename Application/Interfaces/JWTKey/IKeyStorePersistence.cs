namespace Application.Interfaces.JWTKey;

/// <summary>
/// Defines a contract for persisting encrypted keystore data to and from storage.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Handles reading and writing of encrypted key material.</item>
/// <item>Used by <c>JwtKeyStore</c> or similar services to persist cryptographic state.</item>
/// <item>Abstracts underlying storage (e.g., file system, database, or cloud vault).</item>
/// </list>
/// </remarks>
public interface IKeyStorePersistence
{
    /// <summary>
    /// Loads the encrypted keystore data from persistent storage.
    /// </summary>
    /// <returns>
    /// A byte array containing the encrypted keystore data, or <c>null</c> if no data is available.
    /// </returns>
    byte[]? Load();

    /// <summary>
    /// Saves the provided encrypted keystore data to persistent storage.
    /// </summary>
    /// <param name="encryptedData">The encrypted keystore data to store.</param>
    void Save(byte[] encryptedData);
}