using eLibrary.Book.Application.DTOs;
using eLibrary.Book.Application.Interfaces.Repositories;
using eLibrary.Book.Application.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace eLibrary.Book.Application.Queries.Books;

/// <summary>
/// Query to get a single book by id.
/// </summary>
public class GetBookByIdQuery : IRequest<ApiResponse<BookDto>>
{
    public GetBookByIdQuery(Guid id)
    {
        Id = id;
    }

    public Guid Id { get; }
}

public class GetBookByIdQueryHandler : IRequestHandler<GetBookByIdQuery, ApiResponse<BookDto>>
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<GetBookByIdQueryHandler> _logger;

    public GetBookByIdQueryHandler(IBookRepository bookRepository, ILogger<GetBookByIdQueryHandler> logger)
    {
        _bookRepository = bookRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<BookDto>> Handle(GetBookByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var book = await _bookRepository.GetBookByIdAsync(request.Id, cancellationToken);
            if (book == null)
                return ApiResponse<BookDto>.Fail("Book not found.");

            var dto = new BookDto
            {
                Id = book.Id,
                Title = book.Title ?? string.Empty,
                Author = book.Author ?? string.Empty,
                Price = book.Price,
                AvailableCount = book.AvailableCount,
                CoverImageUrl = book.CoverImagePath == null ? null : "/" + book.CoverImagePath.Replace('\\', '/')
            };

            return ApiResponse<BookDto>.Ok(dto, "Book retrieved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving book {BookId}", request.Id);
            return ApiResponse<BookDto>.Fail("An error occurred while retrieving the book.", ex.Message);
        }
    }
}
