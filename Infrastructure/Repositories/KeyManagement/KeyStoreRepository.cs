using Application.Features.KeyManagement.Interfaces;
using Marten;

namespace Infrastructure.Repositories.KeyManagement;

public sealed class KeyStoreRepository(IDocumentSession session) : IKeyStoreRepository
{
    private const string DocumentId = "singleton";

    public async Task<Memory<byte>> LoadAsync()
    {
        var doc = await session.LoadAsync<KeystoreDocument>(DocumentId);
        if (doc == null || doc.EncryptedData.Length == 0)
            return Memory<byte>.Empty;

        return doc.EncryptedData;
    }

    public async Task SaveAsync(ReadOnlyMemory<byte> data)
    {
        var doc = await session.LoadAsync<KeystoreDocument>(DocumentId);
        if (doc == null)
        {
            doc = new KeystoreDocument { Id = DocumentId, EncryptedData = data.ToArray() };
            session.Store(doc);
        }
        else
        {
            doc.EncryptedData = data.ToArray();
        }

        await session.SaveChangesAsync();
    }
    
    public sealed class KeystoreDocument
    {
        public string Id { get; set; } = null!;
        public byte[] EncryptedData { get; set; } = null!;
    }
}