namespace AZM.Abyan.Identity.Application.DTOs.Licenses;

public class UpdateLicenseRequest
{
    public Guid LicenseId { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? MaxUsers { get; set; }
    public bool? IsRevoked { get; set; }
    public string? Domain { get; set; }
    public string? ServerIps { get; set; }
}
