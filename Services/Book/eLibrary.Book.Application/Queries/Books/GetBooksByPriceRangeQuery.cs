using eLibrary.Book.Application.DTOs;
using eLibrary.Book.Application.Interfaces.Repositories;
using eLibrary.Book.Application.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace eLibrary.Book.Application.Queries.Books;

/// <summary>
/// Query to get books within a price range.
/// </summary>
public class GetBooksByPriceRangeQuery : IRequest<ApiResponse<List<BookDto>>>
{
    public GetBooksByPriceRangeQuery(float minPrice, float maxPrice)
    {
        MinPrice = minPrice;
        MaxPrice = maxPrice;
    }

    public float MinPrice { get; }
    public float MaxPrice { get; }
}

public class GetBooksByPriceRangeQueryHandler : IRequestHandler<GetBooksByPriceRangeQuery, ApiResponse<List<BookDto>>>
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<GetBooksByPriceRangeQueryHandler> _logger;

    public GetBooksByPriceRangeQueryHandler(IBookRepository bookRepository, ILogger<GetBooksByPriceRangeQueryHandler> logger)
    {
        _bookRepository = bookRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<List<BookDto>>> Handle(GetBooksByPriceRangeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var books = await _bookRepository.GetBooksByPriceRangeAsync(request.MinPrice, request.MaxPrice, cancellationToken);
            var items = books.Select(x => new BookDto
            {
                Id = x.Id,
                Title = x.Title ?? string.Empty,
                Author = x.Author ?? string.Empty,
                Price = x.Price,
                AvailableCount = x.AvailableCount,
                CoverImageUrl = x.CoverImageMongoId == null ? null : $"/api/Book/{x.Id}/cover"
            }).ToList();

            return ApiResponse<List<BookDto>>.Ok(items, "Books retrieved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving books by price range {MinPrice} to {MaxPrice}", request.MinPrice, request.MaxPrice);
            return ApiResponse<List<BookDto>>.Fail("An error occurred while retrieving books by price range.", ex.Message);
        }
    }
}
