using AZM.Abyan.Identity.Domain.Entities.Base;

namespace AZM.Abyan.Identity.Domain.Entities;

public class Permission : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    // Format: api:{controller}:{action}
    public Guid ScopeId { get; set; }
    public Scope Scope { get; set; } = null!;
    public Guid ResourceId { get; set; }
    public Resource Resources { get; set; } = null!;
    public Guid PolicyId { get; set; }
    public Policy Policy { get; set; } = null!;
}
