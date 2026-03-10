using BookStoreAPI_updated.Model;
using eLibrary.Application.Interfaces.Repositories;
using eLibrary.Shared;
using eLibrary.Application.DTOs;
using MediatR;

namespace eLibrary.Application.Queries.Books;

public class GetBookByIdQuery:IRequest<ApiResponse<BookDto>>
{
    public int Id { get; set; }

    public GetBookByIdQuery(int id)
    {
        Id = id;
    }
}

class GetBookByIdHandler : IRequestHandler<GetBookByIdQuery, ApiResponse<BookDto>>
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<GetBookByIdHandler> _logger;


    public GetBookByIdHandler(IBookRepository bookRepository, ILogger<GetBookByIdHandler> logger)
    {
        _bookRepository = bookRepository;
        _logger = logger;   
    
    }

    public async Task<ApiResponse<BookDto>> Handle(GetBookByIdQuery request, CancellationToken cancellationToken)
    {
        try { 
        _logger.LogInformation("GET/api/books/{BookId} called - Fetching book for id: {BookId}", request.Id, request.Id);

        var book = await _bookRepository.GetBookByIdAsync(request.Id, cancellationToken);


        if (book == null)
        {
            return ApiResponse<BookDto>.Fail($"No book found with Id {request.Id}");
        }
            var bookDto = BookHelper.MapBookToDto(book);

        return ApiResponse<BookDto>.Ok(bookDto, $"Book with Id {request.Id} retrieved successfully.");
            }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving book with Id {BookId}", request.Id);

            return ApiResponse<BookDto>.Fail($"An error occurred while retrieving book with Id {request.Id}: {ex.Message}");
        } }
    }

