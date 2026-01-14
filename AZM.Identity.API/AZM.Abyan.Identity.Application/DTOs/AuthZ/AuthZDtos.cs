using System.Text.Json.Serialization;

namespace AZM.Abyan.Identity.Application.DTOs.AuthZ;

public class ResourceDto
{
    [JsonPropertyName("_id")]
    public Guid? Id { get; set; }
    
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("scopes")]
    public List<ScopeDto> Scopes { get; set; } = new();

    [JsonPropertyName("type")]
    public string Type { get; set; } = "urn:resource:api";

    [JsonPropertyName("uris")]
    public List<string> Uris { get; set; } = new();
}

public class ScopeDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; }
    [JsonPropertyName("name")]
    public required string Name { get; set; }
}
public class ScopeUpdateDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

}


public class PolicyDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; } // "role" for Role Policy

    [JsonPropertyName("logic")]
    public string Logic { get; set; } = "POSITIVE";

    [JsonPropertyName("decisionStrategy")]
    public string DecisionStrategy { get; set; } = "UNANIMOUS";
    
    [JsonPropertyName("config")]
    public Dictionary<string, object> Config { get; set; } = new();
}

public class PermissionDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; } // "scope" for Scope-based Permission

    [JsonPropertyName("logic")]
    public string Logic { get; set; } = "POSITIVE";

    [JsonPropertyName("decisionStrategy")]
    public string DecisionStrategy { get; set; } = "UNANIMOUS";
    
    [JsonPropertyName("resources")]
    public List<string> Resources { get; set; } = new(); // Resource Names or IDs
    
    [JsonPropertyName("scopes")]
    public List<string> Scopes { get; set; } = new();

    [JsonPropertyName("policies")]
    public List<string> Policies { get; set; } = new();
}
