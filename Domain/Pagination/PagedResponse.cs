using eLibrary.Shared;

namespace eLibrary.Domain.Pagination;

public class PagedResponse<T>: ApiResponse<List<T>>
{

    public int PageNumber { get; set; }        // Current page
    public int PageSize { get; set; }          // Size of each page
    public int TotalRecords { get; set; }      // Total records in DB
    public int TotalPages { get; set; }        // How many pages available

    public PagedResponse(List<T> data, int count, int pageNumber, int pageSize, string? message = null)
    {
        Success = true;
        Data = data;
        Message = message;

        Data = data;
        TotalRecords = count;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
    }
}

