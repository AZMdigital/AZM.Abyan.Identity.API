using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AZM.Abyan.Identity.Domain.Entities.Base;

namespace AZM.Abyan.Identity.Domain.Entities
{
    public class Client:BaseEntity
    {
        public string Name {  get; set; }
        public Guid? KeycloakClientId { get; set; }
        public Guid RealmId { get; set; }
        public Tenant tenant { get; set; } = null!;
    }
}
