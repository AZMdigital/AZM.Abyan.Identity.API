namespace AzmFormBuilder.Domain.Entities;

public class FormVersion : BaseEntity
{
    public Guid FormId { get; set; }
    public int Version { get; set; }
    public string FormBuilderJson { get; set; } = "{}";
    public string FormPreviewerJson { get; set; } = "{}";
    public string FormDataJson { get; set; } = "[]";
    public string FormDataSourceJson { get; set; } = "[]";
    public string ChangeNote { get; set; } = "";
    public string WorkflowId { get; set; } = "";
    [Timestamp]
    public uint xmin { get; set; }

    public Form? Form { get; set; }


}


