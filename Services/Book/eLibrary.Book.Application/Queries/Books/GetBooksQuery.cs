using eLibrary.Book.Application.DTOs;
using eLibrary.Book.Application.Interfaces.Repositories;
using eLibrary.Book.Domain.Pagination;
using MediatR;
using Microsoft.Extensions.Logging;

namespace eLibrary.Book.Application.Queries.Books;

/// <summary>
/// Query to get a paginated list of books.
/// </summary>
public class GetBooksQuery : IRequest<PagedResponse<BookDto>>
{
    public GetBooksQuery(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        bool? inStock = null,
        float? minPrice = null,
        float? maxPrice = null)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        SearchTerm = searchTerm;
        InStock = inStock;
        MinPrice = minPrice;
        MaxPrice = maxPrice;
    }

    public int PageNumber { get; }
    public int PageSize { get; }
    public string? SearchTerm { get; }
    public bool? InStock { get; }
    public float? MinPrice { get; }
    public float? MaxPrice { get; }
}

public class GetBooksQueryHandler : IRequestHandler<GetBooksQuery, PagedResponse<BookDto>>
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<GetBooksQueryHandler> _logger;

    public GetBooksQueryHandler(IBookRepository bookRepository, ILogger<GetBooksQueryHandler> logger)
    {
        _bookRepository = bookRepository;
        _logger = logger;
    }

    public async Task<PagedResponse<BookDto>> Handle(GetBooksQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (books, totalRecords) = await _bookRepository.GetAllAsync(
                request.PageNumber,
                request.PageSize,
                request.SearchTerm,
                request.InStock,
                request.MinPrice,
                request.MaxPrice,
                cancellationToken);

            var items = books.Select(x => new BookDto
            {
                Id = x.Id,
                Title = x.Title ?? string.Empty,
                Author = x.Author ?? string.Empty,
                Price = x.Price,
                AvailableCount = x.AvailableCount,
                CoverImageUrl = x.CoverImageMongoId == null ? null : $"/api/Book/{x.Id}/cover"
            }).ToList();

            return new PagedResponse<BookDto>(items, totalRecords, request.PageNumber, request.PageSize, "Books retrieved successfully.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving books.");
            return new PagedResponse<BookDto>(new List<BookDto>(), 0, request.PageNumber, request.PageSize,
                "An error occurred while retrieving books.")
            {
                Success = false,
                ErrorCode = "INTERNAL_SERVER_ERROR"
            };
        }
    }
}
