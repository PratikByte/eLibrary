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
    public GetBooksQuery(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public int PageNumber { get; }
    public int PageSize { get; }
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
            var (books, totalRecords) = await _bookRepository.GetAllAsync(request.PageNumber, request.PageSize, cancellationToken);

            var items = books.Select(x => new BookDto
            {
                Id = x.Id,
                Title = x.Title ?? string.Empty,
                Author = x.Author ?? string.Empty,
                Price = x.Price,
                AvailableCount = x.AvailableCount,
                CoverImageUrl = x.CoverImagePath == null ? null : "/" + x.CoverImagePath.Replace('\\', '/')
            }).ToList();

            return new PagedResponse<BookDto>(items, totalRecords, request.PageNumber, request.PageSize, "Books retrieved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving books.");
            return new PagedResponse<BookDto>(new List<BookDto>(), 0, request.PageNumber, request.PageSize, ex.Message)
            {
                Success = false,
                Errors = new[] { ex.Message }
            };
        }
    }
}
