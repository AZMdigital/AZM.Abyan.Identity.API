using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AZM.Abyan.Identity.Application.DTOs
{
    public class UserDto { public string Id { get; set; } = ""; }

    public class ClientDto
    {
        public string Id { get; set; } = "";       // internal UUID
        public string ClientId { get; set; } = ""; // visible clientId
    }

    public class RoleDto { public string Name { get; set; } = ""; }

    public class ClientSecretDto { public string Value { get; set; } = ""; }

    public class ClientWithRoles
    {
        public string ClientId { get; set; } = "";
        public string ClientInternalId { get; set; } = "";
        public List<string> Roles { get; set; } = new();
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; } = "";
        public string RefreshToken { get; set; } = "";
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; } = "Bearer";
    }

}
