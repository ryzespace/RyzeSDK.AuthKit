using System.Security.Cryptography;
using System.Text;
using Application.Interfaces.JWTKey;

namespace Application.Services.Key;

/// <summary>
/// AES-256-CBC-based implementation of <see cref="IKeyEncryptor"/> for securing keystore data.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Uses a 256-bit symmetric key derived from a Base64-encoded master key.</item>
/// <item>Encrypts and decrypts UTF-8 encoded text using AES in CBC mode with a random IV.</item>
/// <item>Ensures that ciphertext includes the IV as a prefix for later decryption.</item>
/// </list>
/// </remarks>
public class AesKeyEncryptor : IKeyEncryptor
{
    private readonly byte[] _key;

    /// <summary>
    /// Creates an encryptor with a Base64-encoded 256-bit key.
    /// </summary>
    /// <param name="masterKeyBase64">Base64-encoded AES key (32 bytes).</param>
    public AesKeyEncryptor(string masterKeyBase64)
    {
        _key = Convert.FromBase64String(masterKeyBase64);
        if (_key.Length < 32)
            throw new InvalidOperationException("Master key must be 32 bytes (Base64).");
    }

    /// <inheritdoc />
    public byte[] Encrypt(string plaintext)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = Transform(aes.CreateEncryptor(), plaintextBytes);

        return Combine(aes.IV, ciphertext);
    }

    /// <inheritdoc />
    public string Decrypt(byte[] blob)
    {
        using var aes = Aes.Create();
        aes.Key = _key;

        var ivLength = aes.BlockSize / 8;
        var iv = blob[..ivLength];
        var ciphertext = blob[ivLength..];
        aes.IV = iv;

        var plaintextBytes = Transform(aes.CreateDecryptor(), ciphertext);
        return Encoding.UTF8.GetString(plaintextBytes);
    }

    private static byte[] Transform(ICryptoTransform transform, byte[] data) =>
        transform.TransformFinalBlock(data, 0, data.Length);

    private static byte[] Combine(byte[] first, byte[] second)
    {
        var result = new byte[first.Length + second.Length];
        Buffer.BlockCopy(first, 0, result, 0, first.Length);
        Buffer.BlockCopy(second, 0, result, first.Length, second.Length);
        return result;
    }
}
