namespace AzmFormBuilder.Domain.Entities;

public class Form : BaseEntity
{
    public Guid TenantId { get; set; }
    public Guid Category { get; set; }
    public string Tags { get; set; } = string.Empty;
    public FormStatus Status { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<FormVersion> FormVersions { get; set; } = [];


}