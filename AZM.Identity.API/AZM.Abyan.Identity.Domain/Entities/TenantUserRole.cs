using AZM.Abyan.Identity.Domain.Entities.Base;

namespace AZM.Abyan.Identity.Domain.Entities;

public class TenantUserRole : BaseEntity
{
    public Guid? TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    public Guid? UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
}
