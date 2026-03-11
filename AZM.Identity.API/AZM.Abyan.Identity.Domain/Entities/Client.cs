using AZM.Abyan.Identity.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace AZM.Abyan.Identity.Domain.Entities;

public class Client : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid RealmId { get; set; }

    [ForeignKey("RealmId")]
    public Tenant Tenant { get; set; } = null!;

    // Many-to-many relationship with License
    public ICollection<LicenseClient> LicenseClients { get; set; } = new List<LicenseClient>();
}
