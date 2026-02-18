using AZM.Abyan.Identity.Domain.Entities.Base;

namespace AZM.Abyan.Identity.Domain.Entities;

public class Scope : BaseEntity
{
    public string Name { get; set; } = null!;
    // view, create, update, delete
    public string Description { get; set; } = null!;
}
