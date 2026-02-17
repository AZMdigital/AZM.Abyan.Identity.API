using System.Text.Json.Serialization;

namespace AZM.Abyan.Identity.Application.DTOs.ProtocolMappers;

public class ProtocolMapperResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("protocol")]
    public string Protocol { get; set; } = string.Empty;

    [JsonPropertyName("protocolMapper")]
    public string ProtocolMapper { get; set; } = string.Empty;

    [JsonPropertyName("config")]
    public Dictionary<string, string> Config { get; set; } = new();
}
