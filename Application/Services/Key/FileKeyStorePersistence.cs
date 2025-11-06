using Application.Interfaces.JWTKey;

namespace Application.Services.Key;

/// <summary>
/// File-based keystore persistence provider.
/// </summary>
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

    public byte[]? Load() => File.Exists(_filePath) ? File.ReadAllBytes(_filePath) : null;
    public void Save(byte[] encryptedData) => File.WriteAllBytes(_filePath, encryptedData);
}