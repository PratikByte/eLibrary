namespace eLibrary.Application.Commands.Books;

using System.Threading;
using System.Threading.Tasks;
using eLibrary.Application.Interfaces.Repositories;
using eLibrary.Shared;
using eLibrary.Application.DTOs;
using MediatR;
using eLibrary.Application.Common.Helpers;

public class UpdateBookPriceCommand : IRequest<ApiResponse<BookDto>>
{
    public UpdateBookPriceCommand(int bookId, float newPrice)
    {
        BookId = bookId;
        NewPrice = newPrice;
    }

    public int BookId { get; set; }
    public float NewPrice { get; set; }
}

public class UpdateBookPriceHandler : IRequestHandler<UpdateBookPriceCommand, ApiResponse<BookDto>>
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<UpdateBookPriceHandler> _logger;

    public UpdateBookPriceHandler(IBookRepository bookRepository, ILogger<UpdateBookPriceHandler> logger)
    {
        _bookRepository = bookRepository;
        _logger = logger;
    }


    public async Task<ApiResponse<BookDto>> Handle(UpdateBookPriceCommand request, CancellationToken cancellationToken)
    {
        try
        {   
            //get book from repository
            var book = await _bookRepository.GetBookByIdAsync(request.BookId, cancellationToken);
            if(book == null)
                return ApiResponse<BookDto>.Fail("Book not found.");
            //check for valid price
            if (request.NewPrice <= 0)
                return ApiResponse<BookDto>.Fail("Price must be greater than zero.");

            //update book price
            book.Price = request.NewPrice;
            await _bookRepository.UpdateAsync(book, cancellationToken);
            //Convert updated book to DTO
            var bookDto = BookHelper.MapBookToDto(book);
            return ApiResponse<BookDto>.Ok(bookDto, $"Book price updated to ₹{request.NewPrice}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update price for book Id {BookId}", request.BookId);
            return ApiResponse<BookDto>.Fail("An error occurred while updating the book price");
        }
    }
}

