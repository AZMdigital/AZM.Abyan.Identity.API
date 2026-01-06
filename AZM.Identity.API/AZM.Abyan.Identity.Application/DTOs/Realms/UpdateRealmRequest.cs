namespace AZM.Abyan.Identity.Application.DTOs.Realms;

public class UpdateRealmRequest
{
    public string DisplayName { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public string SslRequired { get; set; } = "external";
    public bool RegistrationAllowed { get; set; } = false;
    public bool LoginWithEmailAllowed { get; set; } = true;
    public bool DuplicateEmailsAllowed { get; set; } = false;
}
