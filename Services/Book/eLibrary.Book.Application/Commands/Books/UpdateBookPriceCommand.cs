using eLibrary.Book.Application.DTOs;
using eLibrary.Book.Application.Interfaces.Repositories;
using eLibrary.Book.Application.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace eLibrary.Book.Application.Commands.Books;

public sealed class UpdateBookPriceCommand : IRequest<ApiResponse<BookDto>>
{
    public Guid BookId { get; set; }

    public float NewPrice { get; set; }
}

public sealed class UpdateBookPriceCommandHandler : IRequestHandler<UpdateBookPriceCommand, ApiResponse<BookDto>>
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<UpdateBookPriceCommandHandler> _logger;

    public UpdateBookPriceCommandHandler(
        IBookRepository bookRepository,
        ILogger<UpdateBookPriceCommandHandler> logger)
    {
        _bookRepository = bookRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<BookDto>> Handle(
        UpdateBookPriceCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var book = await _bookRepository.GetBookByIdAsync(request.BookId, cancellationToken);
            if (book is null)
            {
                return ApiResponse<BookDto>.FailWithCode("NOT_FOUND", "Book not found.");
            }

            book.Price = request.NewPrice;
            await _bookRepository.UpdateAsync(book, cancellationToken);

            return ApiResponse<BookDto>.Ok(new BookDto
            {
                Id = book.Id,
                Title = book.Title ?? string.Empty,
                Author = book.Author ?? string.Empty,
                Price = book.Price,
                AvailableCount = book.AvailableCount,
                CoverImageUrl = book.CoverImageMongoId is null
                    ? null
                    : $"/api/Book/{book.Id}/cover"
            }, "Book price updated successfully.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating price for book {BookId}", request.BookId);
            return ApiResponse<BookDto>.FailWithCode(
                "INTERNAL_SERVER_ERROR",
                "An error occurred while updating the book price.");
        }
    }
}
