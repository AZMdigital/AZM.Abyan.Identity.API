using AZM.Abyan.Identity.Domain.Entities.Base;

namespace AZM.Abyan.Identity.Domain.Entities;

public class Policy : BaseEntity
{
    public string Name { get; set; } = null!;
    // AdminPolicy
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
}
