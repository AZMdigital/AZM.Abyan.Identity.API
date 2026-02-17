using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AZM.Abyan.Identity.Application.DTOs.Roles
{
    public class ClientRoleMapping
    {
        public string Id { get; set; } = null!;
        public List<ClientRoleResponse> Mappings { get; set; } = [];
    }
}
