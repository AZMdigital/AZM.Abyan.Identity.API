namespace AZM.Abyan.Identity.Application.DTOs.Responses;

public class Response
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public object? Data { get; set; }
    public object? Errors { get; set; }
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public PaginationInfo? Pagination { get; set; }
}

