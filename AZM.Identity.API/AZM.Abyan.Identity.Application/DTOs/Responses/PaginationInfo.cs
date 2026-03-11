namespace AZM.Abyan.Identity.Application.DTOs.Responses;

public class PaginationInfo
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalRecords { get; set; }

    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalRecords / PageSize) : 0;
    public bool HasNext => PageNumber < TotalPages;
    public bool HasPrevious => PageNumber > 1;
}

