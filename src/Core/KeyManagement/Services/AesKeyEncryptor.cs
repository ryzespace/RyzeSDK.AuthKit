using System.Security.Cryptography;
using System.Text;
using Core.KeyManagement.Interfaces;

namespace Core.KeyManagement.Services;

/// <summary>
/// Provides AES256-CBC encryption and decryption for persisted keystore data.
/// </summary>
/// <remarks>
/// <para>
/// The encryptor derives its symmetric encryption key from Base64 encoded
/// 256 bit master key supplied during construction.
///
/// Each encryption operation generates cryptographically secure random
/// initialization vector (IV). The IV is prefixed to the ciphertext so that
/// it can be recovered during decryption.
/// </para>
/// <para>
/// AES-CBC provides confidentiality but does not provide authenticated
/// integrity protection. The encrypted representation must therefore be
/// protected against tampering by another mechanism, or this implementation
/// should be replaced with an authenticated encryption mode such as AES-GCM.
/// </para>
/// </remarks>
public sealed class AesKeyEncryptor : IKeyEncryptor
{
    private readonly byte[] _key;

    /// <summary>
    /// Initializes new instance of the <see cref="AesKeyEncryptor"/> class.
    /// </summary>
    /// <param name="masterKeyBase64">Base64 encoded 256 bit AES key. The decoded value must contain exactly 32 bytes.</param>
    /// <exception cref="FormatException">Thrown when <paramref name="masterKeyBase64"/> is not a valid Base64 string.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the decoded key does not contain exactly 32 bytes.</exception>
    public AesKeyEncryptor(string masterKeyBase64)
    {
        _key = Convert.FromBase64String(masterKeyBase64);

        if (_key.Length != 32)
        {
            throw new InvalidOperationException(
                "Master key must be exactly 32 bytes (256 bits) when decoded from Base64.");
        }
    }

    /// <summary>
    /// Encrypts plaintext using AES-256-CBC with a randomly generated IV.
    /// </summary>
    /// <param name="plaintext">The UTF-8 text to encrypt.</param>
    /// <returns>
    /// A byte array containing the randomly generated IV followed by the
    /// encrypted ciphertext.
    /// </returns>
    public byte[] Encrypt(string plaintext)
    {
        using var aes = Aes.Create();

        aes.Key = _key;
        aes.GenerateIV();

        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = Transform(aes.CreateEncryptor(), plaintextBytes);

        return Combine(aes.IV, ciphertext);
    }

    /// <summary>
    /// Decrypts an AES256-CBC encrypted payload.
    /// </summary>
    /// <param name="blob">
    /// A byte array containing the IV followed by the ciphertext produced
    /// by <see cref="Encrypt"/>.
    /// </param>
    /// <returns>The decrypted UTF-8 plaintext.</returns>
    /// <exception cref="ArgumentException">Thrown when the encrypted payload does not contain a complete IV.</exception>
    /// <exception cref="CryptographicException">Thrown when the ciphertext cannot be decrypted using the configured key.</exception>
    public string Decrypt(byte[] blob)
    {
        using var aes = Aes.Create();

        aes.Key = _key;

        var ivLength = aes.BlockSize / 8;

        if (blob.Length < ivLength)
        {
            throw new ArgumentException(
                "Encrypted payload does not contain a complete initialization vector.",
                nameof(blob));
        }

        var iv = blob[..ivLength];
        var ciphertext = blob[ivLength..];

        aes.IV = iv;

        var plaintextBytes = Transform(
            aes.CreateDecryptor(),
            ciphertext);

        return Encoding.UTF8.GetString(plaintextBytes);
    }

    private static byte[] Transform(
        ICryptoTransform transform,
        byte[] data) =>
        transform.TransformFinalBlock(data, 0, data.Length);

    private static byte[] Combine(
        byte[] first,
        byte[] second)
    {
        var result = new byte[first.Length + second.Length];

        Buffer.BlockCopy(first, 0, result, 0, first.Length);
        Buffer.BlockCopy(
            second,
            0,
            result,
            first.Length,
            second.Length);

        return result;
    }
}
