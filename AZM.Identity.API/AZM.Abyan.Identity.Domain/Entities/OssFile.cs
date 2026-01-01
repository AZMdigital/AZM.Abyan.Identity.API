namespace AzmFormBuilder.Domain.Entities;

public class OssFile : BaseEntity
{
    public string FileName { get; set; } = string.Empty;
    public string ObjectKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public bool IsConfirmed { get; set; } = false;
}