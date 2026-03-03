using System.Text.Json.Serialization;

namespace AZM.Abyan.Identity.Application.DTOs.Roles;

public class ClientRoleResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("composite")]
    public bool Composite { get; set; }

    [JsonPropertyName("clientRole")]
    public bool ClientRole { get; set; }

    [JsonPropertyName("containerId")]
    public string ContainerId { get; set; } = string.Empty;

    [JsonPropertyName("attributes")]
    public Dictionary<string, string[]>? Attributes { get; set; }
}

