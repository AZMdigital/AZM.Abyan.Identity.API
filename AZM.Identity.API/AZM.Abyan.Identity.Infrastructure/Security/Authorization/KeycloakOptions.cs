namespace AZM.Abyan.Identity.Infrastructure.Security.Authorization;

public sealed record KeycloakOptions
{
    public required string Authority { get; init; }
    public required string Audience { get; init; }
    public string? ClientId { get; init; }
    public string? ClientSecret { get; init; }
}
