namespace AZM.Abyan.Identity.Application.DTOs.ProtocolMappers;

public class CreateProtocolMapperRequest
{
    public string Name { get; set; } = string.Empty;
    public string TokenClaimName { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;
    public bool AddToAccessToken { get; set; } = true;
    public bool AddToIdToken { get; set; } = true;
    public bool AddToUserInfo { get; set; } = true;
}
