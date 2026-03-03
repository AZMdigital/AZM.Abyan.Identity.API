using System.Text.Json.Serialization;

namespace AZM.Abyan.Identity.Application.DTOs.Organizations;

public class OrganizationResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    [JsonIgnore]
    public string? Alias { get; set; }
    [JsonIgnore]
    public string? Description { get; set; }
    public List<OrganizationDomainResponse>? Domains { get; set; }
    [JsonIgnore]
    public bool Enabled { get; set; } = true;
}

public class OrganizationDomainResponse
{
    public string? Name { get; set; }
    [JsonIgnore]
    public bool Verified { get; set; }
}
