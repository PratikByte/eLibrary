namespace eLibrary.Book.Domain.Pagination;

/// <summary>
/// Standard pagination response used by Book Service queries.
/// </summary>
public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalRecords / PageSize) : 0;
    public bool Success { get; set; } = true;
    public string Message { get; set; } = "Success";
    public string[] Errors { get; set; } = Array.Empty<string>();

    public PagedResponse() { }

    public PagedResponse(List<T> items, int totalRecords, int pageNumber, int pageSize, string message = "Success")
    {
        Items = items;
        TotalRecords = totalRecords;
        PageNumber = pageNumber;
        PageSize = pageSize;
        Message = message;
    }
}
