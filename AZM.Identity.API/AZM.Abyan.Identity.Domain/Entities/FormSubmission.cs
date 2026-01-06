namespace AzmFormBuilder.Domain.Entities;

public class FormSubmission : BaseEntity
{
    public Guid FormId { get; set; }
    public Form? Form { get; set; }
    public Guid FormVersionId { get; set; }
    public Guid TenantId { get; set; }
    public string DataJson { get; set; } = string.Empty;
    public SubmissionStatus Status { get; set; }
    public string InstanceId { get; set; } = string.Empty;

}
