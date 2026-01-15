using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AZM.Abyan.Identity.Application.DTOs.Resources;

public class CreateResourceRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? DisplayName { get; set; }
    [JsonIgnore]
    public string Type { get; set; } = "urn:resource:api";
    
    public List<string> Uris { get; set; } = new();
    
    public List<string> ScopeNames { get; set; } = new();
}
