using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AZM.Abyan.Identity.Application.DTOs.Roles
{
    public class AssignPermissionRequest
    {
        public string UserId { get; set; } = string.Empty;
        [JsonIgnore]
        public string ClientId { get; set; } = string.Empty;
        public string PermissionName { get; set; } = string.Empty;
    }
}
