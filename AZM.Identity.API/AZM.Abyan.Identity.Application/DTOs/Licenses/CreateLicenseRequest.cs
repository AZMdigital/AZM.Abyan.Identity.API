namespace AZM.Abyan.Identity.Application.DTOs.Licenses;

public class CreateLicenseRequest
{
    public Guid TenantId { get; set; }
    public List<string> ClientNames { get; set; } = new();
    public DateTime ExpiryDate { get; set; }
    public int? MaxUsers { get; set; }
    public string PackageName { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string ServerIps { get; set; } = string.Empty;
}
