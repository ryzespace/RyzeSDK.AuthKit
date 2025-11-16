using System.Buffers;
using System.Buffers.Text;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Application.DTO.Key;
using Application.Interfaces.JWTKey;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services.Key;

/// <summary>
/// Manages RSA keys for JWT signing, including rotation, revocation, and JWKS exposure.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Maintains in-memory key entries with metadata.</item>
/// <item>Supports key rotation and marking keys as revoked.</item>
/// <item>Provides JWKS-compliant public keys for signature verification.</item>
/// <item>Persists encrypted key material using <see cref="IKeyStorePersistence"/> and <see cref="IKeyEncryptor"/>.</item>
/// <item>Designed for asynchronous initialization and disposal of cryptographic resources.</item>
/// </list>
/// </remarks>
public class JwtKeyStore(IKeyStorePersistence persistence, IKeyEncryptor encryptor, IKeyGenerator generator)
    : IJwtKeyStore, IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, KeyEntry> _keys = new();
    private volatile string _activeKid = null!;
    private volatile PublicJwkDto[]? _cachedJwks;
    private bool _disposed;
    
    public async Task InitializeAsync()
    {
        var enc = await persistence.LoadAsync().ConfigureAwait(false);
        if (enc.Length > 0)
        {
            var decrypted = encryptor.Decrypt(enc.ToArray());
            var ser = JsonSerializer.Deserialize<KeystoreOnDisk>(decrypted)!;
            
            Parallel.ForEach(ser.Records, rec =>
            {
                var rsa = RSA.Create();
                rsa.ImportRSAPrivateKey(Convert.FromBase64String(rec.PrivateKeyBase64), out _);
                var key = new RsaSecurityKey(rsa) { KeyId = rec.Metadata.Kid };

                var pubParams = rsa.ExportParameters(false);
                var n = Base64UrlEncode(pubParams.Modulus!);
                var e = Base64UrlEncode(pubParams.Exponent!);

                _keys[rec.Metadata.Kid] = new KeyEntry(
                    key,
                    new SigningCredentials(key, SecurityAlgorithms.RsaSha256),
                    rec.Metadata,
                    n,
                    e
                );
            });

            _activeKid = ser.ActiveKid;
            RebuildJwksCache();
        }
        else
        {
            await RotateAsync().ConfigureAwait(false);
        }
    }
    
    /// <inheritdoc />
    public SigningCredentials GetActiveSigningCredentials() => _keys[_activeKid].Signing;

    /// <inheritdoc />
    public SigningCredentials? GetSigningCredentialsByKid(string kid)
        => _keys.TryGetValue(kid, out var e) && !e.Meta.Revoked ? e.Signing : null;

    /// <inheritdoc />
    public IEnumerable<PublicJwkDto> GetPublicJwks()
    {
        var cache = _cachedJwks;
        if (cache != null) return cache;
        RebuildJwksCache();
        return _cachedJwks!;
    }

    private void RebuildJwksCache()
    {
        var list = new List<PublicJwkDto>(_keys.Count);
        list.AddRange(from e in _keys.Values
        where !e.Meta.Revoked
        select new PublicJwkDto
        {
            Kty = "RSA",
            Use = "sig",
            Kid = e.Meta.Kid,
            Alg = "RS256",
            N = e.N,
            E = e.E
        });
        _cachedJwks = list.ToArray();
    }

    /// <inheritdoc />
    public async Task<KeyMetadata> RotateAsync(int rsaBits = 4096)
    {
        var (key, meta) = generator.Generate(rsaBits);

        if (string.IsNullOrWhiteSpace(key.KeyId))
            key.KeyId = meta.Kid;

        var signing = new SigningCredentials(key, SecurityAlgorithms.RsaSha256);

        var pubParams = key.Rsa.ExportParameters(false);
        var n = Base64UrlEncode(pubParams.Modulus!);
        var e = Base64UrlEncode(pubParams.Exponent!);

        var entry = new KeyEntry(
            key,
            signing,
            meta,
            n,
            e
        );

        _keys[meta.Kid] = entry;
        _activeKid = meta.Kid;
        _cachedJwks = null;
        await PersistAsync().ConfigureAwait(false);
        return meta;
    }

    /// <inheritdoc />
    public async Task<bool> RevokeAsync(string kid)
    {
        if (!_keys.TryGetValue(kid, out var e)) return false;
        
        _keys[kid] = e with
        {
            Meta = e.Meta with
            {
                Revoked = true
            }
        };
        _cachedJwks = null;
        await PersistAsync().ConfigureAwait(false);
        return true;
    }
    
    /// <inheritdoc />
    public KeyMetadata? GetMetadata(string kid) => _keys.TryGetValue(kid, out var e) ? e.Meta : null;

    private async Task PersistAsync()
    {
        var records = new List<KeystoreRecordOnDisk>(_keys.Count);
        records.AddRange(
            from e in _keys.Values
            let privKey = e.Key.Rsa.ExportRSAPrivateKey()
            let base64 = Convert.ToBase64String(privKey)
            select new KeystoreRecordOnDisk(e.Meta, base64)
        );

        var data = new KeystoreOnDisk(_activeKid, records);
        var json = JsonSerializer.Serialize(data);
        var enc = encryptor.Encrypt(json);
        await persistence.SaveAsync(enc).ConfigureAwait(false);
    }

    private static string Base64UrlEncode(byte[] input)
    {
        if (input == null) 
            throw new ArgumentNullException(nameof(input), "RSA parameter cannot be null");

        Span<byte> buffer = stackalloc byte[Base64.GetMaxEncodedToUtf8Length(input.Length)];
        var status = Base64.EncodeToUtf8(input, buffer, out _, out int bytesWritten);
        if (status != OperationStatus.Done)
            throw new InvalidOperationException($"Base64 encoding failed: {status}");

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
        while (end > 0 && buffer[end - 1] == (byte)'=') end--;

        return Encoding.ASCII.GetString(buffer[..end]);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        foreach (var e in _keys.Values)
            e.Key.Rsa.Dispose();
        _disposed = true;
        await Task.CompletedTask;
    }
}
