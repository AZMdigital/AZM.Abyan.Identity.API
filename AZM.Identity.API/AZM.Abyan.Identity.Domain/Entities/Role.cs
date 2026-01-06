using AZM.Abyan.Identity.Domain.Entities.Base;

namespace AZM.Abyan.Identity.Domain.Entities;
public class Role:BaseEntity
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public Guid? KeycloakRoleId { get; set; } = null!;
    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;
}
