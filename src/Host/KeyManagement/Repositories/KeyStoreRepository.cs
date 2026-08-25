using Core.KeyManagement.Interfaces;
using Marten;

namespace Host.KeyManagement.Repositories;

/// <summary>
/// Provides Marten based repository for persisting the encrypted AuthKit keystore.
/// </summary>
/// <remarks>
/// <para>
/// The repository stores the keystore as a single document identified by a fixed
/// document identifier. Only the encrypted representation of the keystore is
/// persisted; encryption and decryption are handled by the key management layer.
/// </para>
/// <para>
/// The repository uses lightweight Marten sessions for both read and write
/// operations and persists changes asynchronously.
/// </para>
/// </remarks>
public sealed class KeyStoreRepository(IDocumentStore store) : IKeyStoreRepository
{
    /// <summary>
    /// The fixed Marten document identifier used for the singleton keystore document.
    /// </summary>
    private const string DocumentId = "singleton";

    /// <summary>
    /// Asynchronously loads the encrypted keystore from Marten.
    /// </summary>
    /// <returns>
    /// A <see cref="Memory{T}"/> containing the encrypted keystore data,
    /// or <see cref="Memory{T}.Empty"/> when the keystore does not exist
    /// or contains no encrypted data.
    /// </returns>
    public async Task<Memory<byte>> LoadAsync()
    {
        await using var session = store.LightweightSession();
        var document = await session.LoadAsync<KeystoreDocument>(DocumentId);

        if (document is null || document.EncryptedData.Length == 0)
            return Memory<byte>.Empty;

        return document.EncryptedData;
    }

    /// <summary>
    /// Asynchronously persists the encrypted keystore to Marten.
    /// </summary>
    /// <param name="data">
    /// The encrypted keystore data to persist.
    /// </param>
    /// <remarks>
    /// <para>
    /// If the singleton keystore document does not exist, a new document is
    /// created. Otherwise, the existing document is updated with the supplied
    /// encrypted data.
    /// </para>
    /// <para>
    /// The supplied data is copied into a new byte array before being stored,
    /// ensuring that the persisted document does not reference the caller's
    /// <see cref="ReadOnlyMemory{T}"/> directly.
    /// </para>
    /// </remarks>
    public async Task SaveAsync(ReadOnlyMemory<byte> data)
    {
        await using var session = store.LightweightSession();
        var document = await session.LoadAsync<KeystoreDocument>(DocumentId);

        if (document is null)
        {
            document = new KeystoreDocument
            {
                Id = DocumentId,
                EncryptedData = data.ToArray()
            };

            session.Store(document);
        }
        else
        {
            document.EncryptedData = data.ToArray();
        }

        await session.SaveChangesAsync();
    }

    /// <summary>
    /// Represents the persisted Marten document containing the encrypted keystore.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The document intentionally contains only the encrypted keystore payload.
    /// The repository does not perform encryption or decryption itself.
    /// </para>
    /// <para>
    /// The document uses <see cref="Id"/> as its Marten identity and
    /// <see cref="EncryptedData"/> as the encrypted key material.
    /// </para>
    /// </remarks>
    public sealed class KeystoreDocument
    {
        /// <summary>
        /// Gets or sets the Marten document identifier.
        /// </summary>
        public string Id { get; set; } = null!;

        /// <summary>
        /// Gets or sets the encrypted keystore payload.
        /// </summary>
        public byte[] EncryptedData { get; set; } = null!;
    }
}