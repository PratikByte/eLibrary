using eLibrary.Book.Application.DTOs;
using eLibrary.Book.Application.Interfaces.Repositories;
using eLibrary.Book.Domain.Pagination;
using MediatR;
using Microsoft.Extensions.Logging;

namespace eLibrary.Book.Application.Queries.Books;

/// <summary>
/// Query to search books by exact author with pagination.
/// </summary>
public class GetBookByAuthorQuery : IRequest<PagedResponse<BookDto>>
{
    public GetBookByAuthorQuery(string author, int pageNumber, int pageSize)
    {
        Author = author;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public string Author { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
}

public class GetBookByAuthorQueryHandler : IRequestHandler<GetBookByAuthorQuery, PagedResponse<BookDto>>
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<GetBookByAuthorQueryHandler> _logger;

    public GetBookByAuthorQueryHandler(IBookRepository bookRepository, ILogger<GetBookByAuthorQueryHandler> logger)
    {
        _bookRepository = bookRepository;
        _logger = logger;
    }

    public async Task<PagedResponse<BookDto>> Handle(GetBookByAuthorQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (books, totalRecords) = await _bookRepository.PagedGetBookByAuthorAsync(request.PageNumber, request.PageSize, request.Author, cancellationToken);

            var items = books.Select(x => new BookDto
            {
                Id = x.Id,
                Title = x.Title ?? string.Empty,
                Author = x.Author ?? string.Empty,
                Price = x.Price,
                AvailableCount = x.AvailableCount,
                CoverImageUrl = x.CoverImageMongoId == null ? null : $"/api/Book/{x.Id}/cover"
            }).ToList();

            return new PagedResponse<BookDto>(items, totalRecords, request.PageNumber, request.PageSize, "Books by author retrieved successfully.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving books by author {Author}", request.Author);
            return new PagedResponse<BookDto>(new List<BookDto>(), 0, request.PageNumber, request.PageSize,
                "An error occurred while retrieving books by author.")
            {
                Success = false,
                ErrorCode = "INTERNAL_SERVER_ERROR"
            };
        }
    }
}
