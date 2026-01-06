using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AZM.Abyan.Identity.Domain.Entities.Base;

namespace AZM.Abyan.Identity.Domain.Entities
{
    public class Scope : BaseEntity
    {
        public string Name { get; set; } = null!;
        // view, create, update, delete
        public string Description { get; set; } = null!;
        public Guid? KeycloakScopeId { get; set; }
    }
}
