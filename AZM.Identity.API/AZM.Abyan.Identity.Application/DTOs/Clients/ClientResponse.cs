namespace AZM.Identity.Application.DTOs.Clients;

public class ClientResponse
{
    public string Id { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string Protocol { get; set; } = string.Empty;
}

