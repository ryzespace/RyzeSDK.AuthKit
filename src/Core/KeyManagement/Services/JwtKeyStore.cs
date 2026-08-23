using System.Buffers;
using System.Buffers.Text;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Core.KeyManagement.DTO;
using Core.KeyManagement.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Core.KeyManagement.Services;

/// <summary>
/// Manages RSA signing keys used for JWT issuance and signature verification.
/// </summary>
/// <remarks>
/// <para>
/// Maintains RSA signing keys in memory together with their metadata,
/// signing credentials, and public JWK representation.
///
/// Supports key store initialization, key rotation, key revocation, signing
/// key lookup, and JWKS public key discovery.
/// </para>
/// </remarks>
public sealed class JwtKeyStore(
    IKeyStoreRepository repository,
    IKeyEncryptor encryptor,
    IKeyGenerator generator)
    : IJwtKeyStore, IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, KeyEntry> _keys = new();
    private volatile string? _activeKid;
    private volatile PublicJwkDto[]? _cachedJwks;
    private int _disposed;

    /// <summary>
    /// Initializes the key store by loading persisted encrypted key material.
    /// </summary>
    /// <remarks>
    /// If no persisted key store exists, new RSA signing key is generated
    /// and persisted automatically.
    /// </remarks>
    public async Task InitializeAsync()
    {
        ObjectDisposedException.ThrowIf(
            Volatile.Read(ref _disposed) != 0,
            this);

        var encrypted = await repository.LoadAsync().ConfigureAwait(false);

        if (encrypted.Length == 0)
        {
            await RotateAsync().ConfigureAwait(false);
            return;
        }

        var decrypted = encryptor.Decrypt(encrypted.ToArray());

        var keystore = JsonSerializer.Deserialize<KeystoreOnDisk>(decrypted)
            ?? throw new InvalidOperationException(
                "The persisted keystore could not be deserialized.");

        foreach (var record in keystore.Records)
        {
            var rsa = RSA.Create();

            try
            {
                rsa.ImportRSAPrivateKey(
                    Convert.FromBase64String(record.PrivateKeyBase64),
                    out _);

                var key = new RsaSecurityKey(rsa)
                {
                    KeyId = record.Metadata.Kid
                };

                var publicParameters = rsa.ExportParameters(false);

                var entry = new KeyEntry
                {
                    Key = key,
                    Signing = new SigningCredentials(
                        key,
                        SecurityAlgorithms.RsaSha256),
                    Meta = record.Metadata,
                    N = Base64UrlEncode(publicParameters.Modulus!),
                    E = Base64UrlEncode(publicParameters.Exponent!)
                };

                _keys[record.Metadata.Kid] = entry;
            }
            catch
            {
                rsa.Dispose();
                throw;
            }
        }

        if (!_keys.ContainsKey(keystore.ActiveKid))
        {
            throw new InvalidOperationException(
                $"The persisted keystore references unknown active key '{keystore.ActiveKid}'.");
        }

        _activeKid = keystore.ActiveKid;
        RebuildJwksCache();
    }

    /// <summary>
    /// Retrieves the signing credentials associated with the currently active key.
    /// </summary>
    /// <returns>The <see cref="SigningCredentials"/> used to sign newly issued JWTs.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no active signing key is available or the active key has been revoked.</exception>
    public SigningCredentials GetActiveSigningCredentials()
    {
        ObjectDisposedException.ThrowIf(
            Volatile.Read(ref _disposed) != 0,
            this);

        var activeKid = _activeKid;

        if (string.IsNullOrWhiteSpace(activeKid) ||
            !_keys.TryGetValue(activeKid, out var entry))
        {
            throw new InvalidOperationException(
                "No active signing key is available.");
        }

        if (entry.Meta.Revoked)
        {
            throw new InvalidOperationException(
                $"The active signing key '{activeKid}' has been revoked.");
        }

        return entry.Signing;
    }

    /// <summary>
    /// Retrieves signing credentials associated with specific key identifier.
    /// </summary>
    /// <param name="kid">The unique key identifier.</param>
    /// <returns>
    /// The matching <see cref="SigningCredentials"/>, or <c>null</c> when
    /// the key does not exist or has been revoked.
    /// </returns>
    public SigningCredentials? GetSigningCredentialsByKid(string kid)
    {
        ObjectDisposedException.ThrowIf(
            Volatile.Read(ref _disposed) != 0,
            this);

        return _keys.TryGetValue(kid, out var entry) &&
               !entry.Meta.Revoked
            ? entry.Signing
            : null;
    }

    /// <summary>
    /// Returns the public signing keys exposed by the key store in JWKS-compatible format.
    /// </summary>
    /// <returns>
    /// A collection of <see cref="PublicJwkDto"/> instances representing
    /// all non revoked public signing keys.
    /// </returns>
    public IEnumerable<PublicJwkDto> GetPublicJwks()
    {
        ObjectDisposedException.ThrowIf(
            Volatile.Read(ref _disposed) != 0,
            this);

        var cache = _cachedJwks;

        if (cache is not null)
            return cache;

        RebuildJwksCache();

        return _cachedJwks!;
    }

    private void RebuildJwksCache()
    {
        var keys = _keys.Values
            .Where(entry => !entry.Meta.Revoked)
            .Select(entry => new PublicJwkDto
            {
                Kty = "RSA",
                Use = "sig",
                Kid = entry.Meta.Kid,
                Alg = entry.Meta.Algorithm,
                N = entry.N,
                E = entry.E
            })
            .ToArray();

        _cachedJwks = keys;
    }

    /// <summary>
    /// Generates and activates a new RSA signing key.
    /// </summary>
    /// <param name="rsaBits">The RSA key size in bits. Defaults to 4096 bits.</param>
    /// <returns>Metadata describing the newly generated signing key.</returns>
    public async Task<KeyMetadata> RotateAsync(int rsaBits = 4096)
    {
        ObjectDisposedException.ThrowIf(
            Volatile.Read(ref _disposed) != 0,
            this);

        var (key, metadata) = generator.Generate(rsaBits);

        key.KeyId ??= metadata.Kid;

        var publicParameters = key.Rsa.ExportParameters(false);

        var entry = new KeyEntry
        {
            Key = key,
            Signing = new SigningCredentials(
                key,
                SecurityAlgorithms.RsaSha256),
            Meta = metadata,
            N = Base64UrlEncode(publicParameters.Modulus!),
            E = Base64UrlEncode(publicParameters.Exponent!)
        };

        _keys[metadata.Kid] = entry;
        _activeKid = metadata.Kid;
        _cachedJwks = null;

        await PersistAsync().ConfigureAwait(false);

        return metadata;
    }

    /// <summary>
    /// Revokes the signing key associated with the specified key identifier.
    /// </summary>
    /// <param name="kid">
    /// The unique key identifier of the key to revoke.
    /// </param>
    /// <returns>
    /// <c>true</c> when the key was found and revoked; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when attempting to revoke the currently active signing key.
    /// </exception>
    public async Task<bool> RevokeAsync(string kid)
    {
        ObjectDisposedException.ThrowIf(
            Volatile.Read(ref _disposed) != 0,
            this);

        if (!_keys.TryGetValue(kid, out var entry))
            return false;

        if (string.Equals(_activeKid, kid, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The active signing key cannot be revoked.");
        }

        _keys[kid] = entry with
        {
            Meta = entry.Meta with
            {
                Revoked = true
            }
        };

        _cachedJwks = null;

        await PersistAsync().ConfigureAwait(false);

        return true;
    }

    /// <summary>
    /// Retrieves metadata associated with a specific signing key.
    /// </summary>
    /// <param name="kid">The unique key identifier.</param>
    /// <returns>
    /// The <see cref="KeyMetadata"/> associated with the specified key,
    /// or <c>null</c> when the key does not exist.
    /// </returns>
    public KeyMetadata? GetMetadata(string kid)
    {
        ObjectDisposedException.ThrowIf(
            Volatile.Read(ref _disposed) != 0,
            this);

        return _keys.TryGetValue(kid, out var entry)
            ? entry.Meta
            : null;
    }

    private async Task PersistAsync()
    {
        var records = _keys.Values.Select(entry => new KeystoreRecordOnDisk
        {
            Metadata = entry.Meta,
            PrivateKeyBase64 = Convert.ToBase64String(
                entry.Key.Rsa.ExportRSAPrivateKey())
        })
        .ToList();

        var keystore = new KeystoreOnDisk
        {
            ActiveKid = _activeKid
                ?? throw new InvalidOperationException(
                    "Cannot persist a keystore without an active key."),
            Records = records
        };

        var json = JsonSerializer.Serialize(keystore);
        var encrypted = encryptor.Encrypt(json);

        await repository
            .SaveAsync(encrypted)
            .ConfigureAwait(false);
    }

    private static string Base64UrlEncode(byte[] input)
    {
        ArgumentNullException.ThrowIfNull(input);

        Span<byte> buffer = stackalloc byte[
            Base64.GetMaxEncodedToUtf8Length(input.Length)];

        var status = Base64.EncodeToUtf8(
            input,
            buffer,
            out _,
            out var bytesWritten);

        if (status != OperationStatus.Done)
        {
            throw new InvalidOperationException(
                $"Base64 encoding failed: {status}");
        }

        for (var i = 0; i < bytesWritten; i++)
        {
            buffer[i] = buffer[i] switch
            {
                (byte)'+' => (byte)'-',
                (byte)'/' => (byte)'_',
                _ => buffer[i]
            };
        }

        var end = bytesWritten;

        while (end > 0 && buffer[end - 1] == (byte)'=')
            end--;

        return Encoding.ASCII.GetString(buffer[..end]);
    }

    /// <summary>
    /// Releases all cryptographic resources owned by the key store.
    /// </summary>
    public ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
            return ValueTask.CompletedTask;

        foreach (var entry in _keys.Values)
            entry.Key.Rsa.Dispose();

        _keys.Clear();
        _cachedJwks = null;
        _activeKid = null;

        return ValueTask.CompletedTask;
    }
}
