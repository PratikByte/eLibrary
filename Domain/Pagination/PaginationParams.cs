namespace eLibrary.Domain.Pagination;

public class PaginationParams
{
    public int PageNumber { get; set; } = 1;  // default first page
    public int PageSize { get; set; } = 5;   // default 10 records per page
}

