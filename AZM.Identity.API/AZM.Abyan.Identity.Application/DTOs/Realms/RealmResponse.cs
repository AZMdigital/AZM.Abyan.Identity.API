namespace AZM.Abyan.Identity.Application.DTOs.Realms;

public class RealmResponse
{
    public string Id { get; set; } = string.Empty;
    public string Realm { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string SslRequired { get; set; } = string.Empty;
    public bool RegistrationAllowed { get; set; }
    public bool LoginWithEmailAllowed { get; set; }
    public bool DuplicateEmailsAllowed { get; set; }
}
