using AZM.Abyan.Identity.Domain.Entities.Base;

namespace AZM.Abyan.Identity.Domain.Entities;

public class License : BaseEntity
{
    public Guid     TenantId       { get; set; }
    public Tenant   Tenant         { get; set; } = null!;
    public string   LicenseKeyHash     { get; set; } = null!;
    public DateTime IssuedAt           { get; set; }
    public DateTime ExpiryDate         { get; set; }
    public int?     MaxUsers           { get; set; }
    public string   PackageName        { get; set; } = null!;
    public string?  Domain             { get; set; }
    public string?  ServerIps          { get; set; }
    public bool     IsActive           { get; set; }= false;

    // Many-to-many relationship with Client
    public ICollection<LicenseClient> LicenseClients { get; set; } = [];

    public static License Create(
        Guid     id,
        Guid     tenantId,
        string   licenseKeyHash,
        string   packageName,
        DateTime expiryDate)
    {
        return new License
        {
            Id             = id,
            TenantId       = tenantId,
            LicenseKeyHash = licenseKeyHash,
            PackageName    = packageName,
            ExpiryDate     = expiryDate,
            IssuedAt       = DateTime.UtcNow,
            IsActive       = false,
        };
    }

    public void AddClient(Guid clientId)
    {
        if (!LicenseClients.Any(lc => lc.ClientId == clientId))
        {
            LicenseClients.Add(new LicenseClient { LicenseId = Id, ClientId = clientId });
        }
    }

    public void RemoveClient(Guid clientId)
    {
        var licenseClient = LicenseClients.FirstOrDefault(lc => lc.ClientId == clientId);
        if (licenseClient != null)
        {
            LicenseClients.Remove(licenseClient);
        }
    }

    public void Activate()
    {
        if (ExpiryDate <= DateTime.UtcNow)
            throw new InvalidOperationException("License has expired.");

        IsActive = true;
    }

    public bool IsIpAllowed(string? currentIp)
    {
        if (string.IsNullOrWhiteSpace(ServerIps)) return true;
        if (string.IsNullOrWhiteSpace(currentIp)) return false;
        return ServerIps
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(ip => string.Equals(ip, currentIp, StringComparison.OrdinalIgnoreCase));
    }

    public bool IsDomainAllowed(string? currentDomain)
    {
        if (string.IsNullOrWhiteSpace(Domain)) return true;
        if (string.IsNullOrWhiteSpace(currentDomain)) return false;
        return Domain
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(d => string.Equals(d, currentDomain, StringComparison.OrdinalIgnoreCase));
    }
}
