using System.Text.Json.Serialization;

namespace AZM.Abyan.Identity.Application.DTOs.Organizations;

public class UpdateOrganizationRequest
{
    public string? Name { get; set; }
    [JsonIgnore]
    public string? Alias { get; set; }
    [JsonIgnore]
    public string? Description { get; set; }
    public List<string>? Domains { get; set; }
    [JsonIgnore]
    public bool? Enabled { get; set; }
}
