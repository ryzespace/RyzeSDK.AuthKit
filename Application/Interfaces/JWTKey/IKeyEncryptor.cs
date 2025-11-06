namespace Application.Interfaces.JWTKey;

/// <summary>
/// Defines a contract for encrypting and decrypting keystore data using symmetric or asymmetric algorithms.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implemented by concrete providers such as <c>AesKeyEncryptor</c> for AES-based encryption.</item>
/// <item>Responsible for securing sensitive key material, configuration values, or tokens at rest.</item>
/// <item>Ensures reversible protection through <see cref="Encrypt"/> and <see cref="Decrypt"/> operations.</item>
/// </list>
/// </remarks>
public interface IKeyEncryptor
{
    /// <summary>
    /// Encrypts the provided plaintext into a binary ciphertext representation.
    /// </summary>
    /// <param name="plaintext">The UTF-8 encoded string to encrypt.</param>
    /// <returns>A byte array containing the encrypted data.</returns>
    byte[] Encrypt(string plaintext);

    /// <summary>
    /// Decrypts the provided ciphertext back into its original plaintext form.
    /// </summary>
    /// <param name="ciphertext">The byte array containing encrypted data.</param>
    /// <returns>The decrypted UTF-8 string.</returns>
    string Decrypt(byte[] ciphertext);
}