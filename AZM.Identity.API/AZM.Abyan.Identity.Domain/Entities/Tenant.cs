using AZM.Abyan.Identity.Domain.Entities.Base;

namespace AZM.Abyan.Identity.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public Guid? KeycloakRealmId { get; set; }
    public ICollection<User> Users { get; set; } = [];
    public ICollection<TenantUserRole> TenantUserRoles { get; set; } = [];
    public ICollection<Client> Clients { get; set; } = [];
}
