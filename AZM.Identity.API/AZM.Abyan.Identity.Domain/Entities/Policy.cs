using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AZM.Abyan.Identity.Domain.Entities.Base;

namespace AZM.Abyan.Identity.Domain.Entities
{
    public class Policy : BaseEntity
    {
        public string Name { get; set; } = null!;
        // AdminPolicy
        public Guid? KeycloakPolicyId { get; set; }
        public Guid RoleId { get; set; }
        public Role Role { get; set; } = null!;
    }

}
