using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AZM.Abyan.Identity.Domain.Entities.Base;

namespace AZM.Abyan.Identity.Domain.Entities
{
    public class TenantUserPermission : BaseEntity
    {
        public Guid? TenantId { get; set; }
        public Tenant Tenant { get; set; } = null!;
        public Guid? UserId { get; set; }
        public User User { get; set; } = null!;
        public Guid PermissionId { get; set; }
        public Permission permission { get; set; } = null!;
    }
}
