namespace Core.KeyManagement.Interfaces;

/// <summary>
/// Defines contract for encrypting and decrypting sensitive data.
/// </summary>
/// <remarks>
/// <para>
/// Implementations provide reversible protection for sensitive data stored
/// at rest, such as private cryptographic key material, configuration
/// secrets, or tokens.
/// </para>
/// <para>
/// The encryption mechanism and key management strategy are implementation
/// details of the concrete encryptor. The contract guarantees only that data
/// encrypted by <see cref="Encrypt"/> can subsequently be restored using
/// <see cref="Decrypt"/> with the corresponding protected data.
/// </para>
/// <para>
/// Implementations should use authenticated encryption or another mechanism
/// that provides integrity protection in addition to confidentiality.
/// </para>
/// </remarks>
public interface IKeyEncryptor
{
    /// <summary>
    /// Encrypts plaintext into protected binary representation.
    /// </summary>
    /// <param name="plaintext">The plaintext string to encrypt.</param>
    /// <returns>A byte array containing the encrypted representation of the plaintext.</returns>
    /// <remarks>
    /// Implementations are responsible for selecting an appropriate encoding
    /// for the plaintext and for including any metadata required to decrypt
    /// the resulting ciphertext.
    /// </remarks>
    byte[] Encrypt(string plaintext);

    /// <summary>
    /// Decrypts encrypted data and restores its original plaintext representation.
    /// </summary>
    /// <param name="ciphertext">The encrypted binary representation produced by <see cref="Encrypt"/>.</param>
    /// <returns>The decrypted plaintext string.</returns>
    /// <remarks>
    /// The method should reject ciphertext that is malformed, corrupted,
    /// tampered with, or cannot be decrypted using the configured key.
    /// </remarks>
    string Decrypt(byte[] ciphertext);
}
