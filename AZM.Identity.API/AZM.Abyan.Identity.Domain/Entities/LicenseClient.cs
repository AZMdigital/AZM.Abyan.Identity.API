using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AZM.Abyan.Identity.Domain.Entities;

public class LicenseClient
{
    [Key]
    [Column(Order = 0)]
    public Guid LicenseId { get; set; }

    [Key]
    [Column(Order = 1)]
    public Guid ClientId { get; set; }

    [ForeignKey(nameof(LicenseId))]
    public License License { get; set; } = null!;

    [ForeignKey(nameof(ClientId))]
    public Client Client { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}