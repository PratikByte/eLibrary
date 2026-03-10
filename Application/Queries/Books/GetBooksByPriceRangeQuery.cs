using BookStoreAPI_updated.Model;
using eLibrary.Application.DTOs;
using eLibrary.Application.Interfaces.Repositories;
using eLibrary.Shared;
using MediatR;

namespace eLibrary.Application.Queries.Books;

public class GetBooksByPriceRangeQuery : IRequest<ApiResponse<List<BookWithPriceDto>>>
{
    public GetBooksByPriceRangeQuery() { } //we need this constructor for getting data from query string we used FromQuery attribute in controller for it

    public float MinPrice { get; set; }
    public float MaxPrice { get; set; }


}
public class GetBooksByPriceRangeHandler : IRequestHandler<GetBooksByPriceRangeQuery, ApiResponse<List<BookWithPriceDto>>>
{
    public readonly IBookRepository _bookRepository;
    private readonly ILogger<GetBooksByPriceRangeHandler> _logger;

    public GetBooksByPriceRangeHandler(IBookRepository bookRepository, ILogger<GetBooksByPriceRangeHandler> logger)
    {
        _bookRepository = bookRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<List<BookWithPriceDto>>> Handle(GetBooksByPriceRangeQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.MaxPrice <= request.MinPrice)
            {
                return ApiResponse<List<BookWithPriceDto>>.Fail("Max price must be greater than Min price.");
            }

            //getBook from repository
            var books = await _bookRepository.GetBooksByPriceRangeAsync(request.MinPrice, request.MaxPrice, cancellationToken);
            //check if books are null or empty
            if (books == null || books.Count == 0)
            {
                return ApiResponse<List<BookWithPriceDto>>.Ok(new List<BookWithPriceDto>(), "No books found for  the price of ₹" + request.MinPrice + " TO ₹" + request.MaxPrice);
            }
            // Map books to DTOs
            var bookDtos = BookHelper.MapPriceBooksToDto(books);
            return ApiResponse<List<BookWithPriceDto>>.Ok(bookDtos, "Books retrieved successfully.");
        }
        catch (Exception ex)
        {

            return ApiResponse<List<BookWithPriceDto>>.Fail($"An error occurred while retrieving books by price range: {ex.Message}");
        }
    }
}

