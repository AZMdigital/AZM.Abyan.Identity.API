using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AZM.Abyan.Identity.Application.DTOs.Roles
{
    public class UserRoleMappingsResponse
    {
        [JsonPropertyName("realmMappings")]
        public List<RealmRoleResponse>? RealmMappings { get; set; }

        [JsonPropertyName("clientMappings")]
        public Dictionary<string, ClientMapping>? ClientMappings { get; set; }
    }

    public class ClientMapping
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("mappings")]
        public List<ClientRoleResponse> Mappings { get; set; } = new();
    }

}
