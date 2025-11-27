namespace Domain.Features.KeyManagement.Entity;

public sealed record SigningKey(
    Guid Id, // kid
    string PublicKeyPem,
    string PrivateKeyEncrypted,
    string Algorithm,
    DateTime CreatedAt,
    DateTime? NotBefore,
    DateTime? ExpiresAt,
    DateTime? RevokedAt,
    bool IsActive
)
{
    public bool IsValidForSigning(DateTime now)
        => IsActive &&
           RevokedAt is null &&
           (NotBefore is null || now >= NotBefore) &&
           (ExpiresAt is null || now < ExpiresAt);

    public SigningKey Activate(DateTime now)
        => this with
        {
            IsActive = true,
            NotBefore = now,
            RevokedAt = null
        };

    public SigningKey Revoke(DateTime now)
        => this with
        {
            IsActive = false,
            RevokedAt = now
        };

    public SigningKey Expire(DateTime now)
        => this with
        {
            IsActive = false,
            ExpiresAt = now
        };
}