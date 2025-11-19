using Application.Features.KeyManagement.Interfaces;

namespace Infrastructure.Persistence.KeyManagement;

/// <summary>
/// Persists encrypted keystore data to the local file system.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Stores keystore data under a configurable file path (default: ~/.authkit/keystore.json).</item>
/// <item>Supports asynchronous load and save operations.</item>
/// <item>Creates parent directories automatically if they do not exist.</item>
/// </list>
/// </remarks>
public class FileKeyStorePersistence : IKeyStorePersistence
{
    private readonly string _filePath;

    public FileKeyStorePersistence(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".authkit/keystore.json"
        );
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
    }

    /// <inheritdoc />
    public async Task<Memory<byte>> LoadAsync()
    {
        if (!File.Exists(_filePath))
            return Memory<byte>.Empty;

        await using var fs = new FileStream(
            _filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            true
        );
        var data = new byte[fs.Length];
        var read = 0;
        while (read < data.Length)
        {
            var n = await fs.ReadAsync(data.AsMemory(read, data.Length - read)).ConfigureAwait(false);
            if (n == 0) break;
            read += n;
        }

        return data;
    }

    /// <inheritdoc />
    public async Task SaveAsync(ReadOnlyMemory<byte> data)
    {
        await using var fs = new FileStream(
            _filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            4096,
            true
        );
        await fs.WriteAsync(data)
            .ConfigureAwait(false);
        await fs.FlushAsync()
            .ConfigureAwait(false);
    }
}