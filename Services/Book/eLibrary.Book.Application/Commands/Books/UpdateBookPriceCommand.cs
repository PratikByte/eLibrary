using System.ComponentModel.DataAnnotations;
using eLibrary.Book.Application.DTOs;
using eLibrary.Book.Application.Interfaces.Repositories;
using eLibrary.Book.Application.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace eLibrary.Book.Application.Commands.Books;

public sealed class UpdateBookPriceCommand : IRequest<ApiResponse<BookDto>>
{
    [Range(1, int.MaxValue)]
    public int BookId { get; set; }

    [Range(0.01, 1000000)]
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
                return ApiResponse<BookDto>.Fail("Book not found.");
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
                CoverImageUrl = book.CoverImagePath is null
                    ? null
                    : "/" + book.CoverImagePath.Replace('\\', '/')
            }, "Book price updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating price for book {BookId}", request.BookId);
            return ApiResponse<BookDto>.Fail("An error occurred while updating the book price.", ex.Message);
        }
    }
}
