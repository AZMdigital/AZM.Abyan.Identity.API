using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AZM.Abyan.Identity.Domain.Entities.Base;

namespace AZM.Abyan.Identity.Domain.Entities
{
    public class Resource : BaseEntity
    {
        public string Name { get; set; } = null!;
        // users, orders, invoices
        public string Description { get; set; } = null!;
        public Guid ScopeId { get; set; }
        public Scope Scope { get; set; } = null!;
    }
}
