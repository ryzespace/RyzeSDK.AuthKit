using System.Security.Cryptography;
using System.Text.Json;
using Application.DTO.Key;
using Application.Interfaces;
using Application.Interfaces.JWTKey;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services.Key;

/// <summary>
/// Provides centralized management of RSA signing keys for JWTs, supporting rotation, revocation, and JWKS exposure.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Ensures thread-safe access to all stored keys using <see cref="ReaderWriterLockSlim"/>.</item>
/// <item>Persists encrypted keystore data using <see cref="IKeyStorePersistence"/> and <see cref="IKeyEncryptor"/>.</item>
/// <item>Automatically initializes or rotates the active key on startup if no valid data is present.</item>
/// </list>
/// </remarks>
public class JwtKeyStore : IJwtKeyStore, IDisposable
{
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
    private readonly IKeyStorePersistence _persistence;
    private readonly IKeyEncryptor _encryptor;
    private readonly IKeyGenerator _generator;
    private readonly Dictionary<string, (RsaSecurityKey Key, KeyMetadata Meta)> _keys = new();
    private string _activeKid = null!;
    private bool _disposed;

    public JwtKeyStore(IKeyStorePersistence persistence, IKeyEncryptor encryptor, IKeyGenerator generator)
    {
        _persistence = persistence;
        _encryptor = encryptor;
        _generator = generator;
        LoadOrInitialize();
    }

    private void LoadOrInitialize()
    {
        _lock.EnterWriteLock();
        try
        {
            var enc = _persistence.Load();
            if (enc is { Length: > 0 })
            {
                var json = _encryptor.Decrypt(enc);
                var ser = JsonSerializer.Deserialize<KeystoreOnDisk>(json)!;
                foreach (var rec in ser.Records)
                {
                    var rsa = RSA.Create();
                    rsa.ImportRSAPrivateKey(Convert.FromBase64String(rec.PrivateKeyBase64), out _);
                    var key = new RsaSecurityKey(rsa) { KeyId = rec.Metadata.Kid };
                    _keys[rec.Metadata.Kid] = (key, rec.Metadata);
                }
                _activeKid = ser.ActiveKid;
            }
            else
            {
                var meta = Rotate();
                Persist();
            }
        }
        finally { _lock.ExitWriteLock(); }
    }
    
    /// <inheritdoc />
    public SigningCredentials GetActiveSigningCredentials()
    {
        _lock.EnterReadLock();
        try
        {
            return new SigningCredentials(_keys[_activeKid].Key, SecurityAlgorithms.RsaSha256);
        }
        finally { _lock.ExitReadLock(); }
    }

    /// <inheritdoc />
    public SigningCredentials? GetSigningCredentialsByKid(string kid)
    {
        _lock.EnterReadLock();
        try
        {
            if (_keys.TryGetValue(kid, out var entry) && !entry.Meta.Revoked)
                return new SigningCredentials(entry.Key, SecurityAlgorithms.RsaSha256);

            return null;
        }
        finally { _lock.ExitReadLock(); }
    }

    /// <inheritdoc />
    public IEnumerable<JsonWebKey> GetPublicJwks()
    {
        _lock.EnterReadLock();
        try
        {
            return _keys.Values
                .Where(v => !v.Meta.Revoked)
                .Select(v =>
                {
                    var rsa = v.Key.Rsa;
                    var publicParams = rsa.ExportParameters(includePrivateParameters: false);
                
                    var jwk = new JsonWebKey
                    {
                        Kty = "RSA",
                        Use = "sig",
                        Kid = v.Meta.Kid,
                        Alg = "RS256",
                        N = Base64UrlEncoder.Encode(publicParams.Modulus),
                        E = Base64UrlEncoder.Encode(publicParams.Exponent)
                    };
                
                    return jwk;
                })
                .ToArray();
        }
        finally { _lock.ExitReadLock(); }
    }

    /// <inheritdoc />
    public KeyMetadata Rotate(int rsaBits = 4096)
    {
        _lock.EnterWriteLock();
        try
        {
            var (key, meta) = _generator.Generate(rsaBits);
            _keys[meta.Kid] = (key, meta);
            _activeKid = meta.Kid;
            Persist();
            return meta;
        }
        finally { _lock.ExitWriteLock(); }
    }
    
    /// <inheritdoc />
    public bool Revoke(string kid)
    {
        _lock.EnterWriteLock();
        try
        {
            if (!_keys.TryGetValue(kid, out var e)) return false;
            _keys[kid] = (e.Key, e.Meta with { Revoked = true });
            Persist();
            return true;
        }
        finally { _lock.ExitWriteLock(); }
    }
    
    /// <inheritdoc />
    public KeyMetadata? GetMetadata(string kid)
    {
        _lock.EnterReadLock();
        try
        {
            return _keys.TryGetValue(kid, out var e) ? e.Meta : null;
        }
        finally { _lock.ExitReadLock(); }
    }

    private void Persist()
    {
        var data = new KeystoreOnDisk(
            ActiveKid: _activeKid,
            Records: _keys.Select(kv => new KeystoreRecordOnDisk(
                Metadata: kv.Value.Meta,
                PrivateKeyBase64: Convert.ToBase64String(kv.Value.Key.Rsa.ExportRSAPrivateKey())
            )).ToList()
        );

        var json = JsonSerializer.Serialize(data);
        var enc = _encryptor.Encrypt(json);
        _persistence.Save(enc);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _lock.Dispose();
        _disposed = true;
    }
}
