using AZM.Abyan.Identity.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace AZM.Abyan.Identity.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid LicenseId { get; set; }
    [ForeignKey("LicenseId")]
    public License License { get; set; } = null!;

    public string Token { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }

    public static RefreshToken Create(Guid licenseId, string token, DateTime expiresAt)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            LicenseId = licenseId,
            Token = token,
            IssuedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            IsRevoked = false
        };
    }

    public bool IsValid()
    {
        return !IsRevoked && ExpiresAt > DateTime.UtcNow;
    }

    public void Revoke()
    {
        IsRevoked = true;
        RevokedAt = DateTime.UtcNow;
    }
}