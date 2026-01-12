using AZM.Abyan.Identity.Domain.Entities.Base;

namespace AZM.Abyan.Identity.Domain.Entities;

public class User : BaseEntity
{
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Firstname { get; set; } = null!;
    public string Lastname { get; set; } = null!;
    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; } = null!;
    public ICollection<TenantUserRole> TenantRoles { get; set; } = [];
}
