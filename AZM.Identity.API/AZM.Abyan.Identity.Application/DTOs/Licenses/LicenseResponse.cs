namespace AZM.Abyan.Identity.Application.DTOs.Licenses;

public class LicenseResponse
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public List<string> ClientNames { get; set; } = new();
    public string LicenseKeyHash { get; set; } = string.Empty;
    public string PublicKey { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int? MaxUsers { get; set; }
    public bool IsActive { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string ServerIps { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
}
