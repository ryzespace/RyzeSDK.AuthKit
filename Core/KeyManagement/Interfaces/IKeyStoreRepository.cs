namespace Core.KeyManagement.Interfaces;

/// <summary>
/// Defines an abstraction for persisting and loading protected keystore data.
/// </summary>
/// <remarks>
/// <para>
/// The repository is responsible only for persistence. It does not perform
/// encryption or decryption of keystore contents.
///
/// Keystore data should be encrypted by <see cref="IKeyEncryptor"/> before
/// being passed to <see cref="SaveAsync"/> and decrypted after being returned
/// by <see cref="LoadAsync"/>.
/// </para>
/// </remarks>
public interface IKeyStoreRepository
{
    /// <summary>
    /// Loads the persisted keystore data.
    /// </summary>
    /// <remarks>
    /// The returned data is expected to contain the encrypted representation
    /// produced by the configured <see cref="IKeyEncryptor"/>.
    /// </remarks>
    /// <returns>
    /// A <see cref="Memory{T}"/> containing the encrypted keystore data.
    /// Returns an empty memory region when no persisted keystore exists.
    /// </returns>
    Task<Memory<byte>> LoadAsync();

    /// <summary>
    /// Persists encrypted keystore data.
    /// </summary>
    /// <remarks>
    /// The repository stores the provided data as-is and does not perform
    /// encryption itself.
    /// </remarks>
    /// <param name="data">The encrypted keystore data to persist.</param>
    /// <returns>
    /// A task representing the asynchronous persistence operation.
    /// </returns>
    Task SaveAsync(ReadOnlyMemory<byte> data);
}
