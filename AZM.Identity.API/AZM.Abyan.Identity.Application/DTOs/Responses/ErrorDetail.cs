namespace AZM.Abyan.Identity.Application.DTOs.Responses;

public class ErrorDetail
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Property { get; set; }
}
