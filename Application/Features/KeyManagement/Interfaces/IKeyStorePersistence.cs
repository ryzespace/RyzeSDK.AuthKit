namespace Application.Features.KeyManagement.Interfaces;

/// <summary>
/// Provides abstraction for persisting and loading encrypted keystore data.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Supports asynchronous I/O operations for high-performance scenarios.</item>
/// <item>The storage medium can be a file system, database, or any other persistent store.</item>
/// <item>The data is expected to be already encrypted by <see cref="IKeyEncryptor"/> before saving.</item>
/// </list>
/// </remarks>
public interface IKeyStorePersistence
{
    /// <summary>
    /// Asynchronously loads the persisted keystore bytes.
    /// </summary>
    /// <returns>A <see cref="Memory{Byte}"/> containing the persisted encrypted keystore data. 
    /// Returns empty memory if no data is found.</returns>
    Task<Memory<byte>> LoadAsync();

    /// <summary>
    /// Asynchronously saves the provided encrypted keystore bytes to persistent storage.
    /// </summary>
    /// <param name="data">Encrypted keystore data to persist.</param>
    Task SaveAsync(ReadOnlyMemory<byte> data);
}