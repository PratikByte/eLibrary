using eLibrary.Application.Interfaces.Repositories;
using eLibrary.Shared;
using eLibrary.Application.DTOs;
using MediatR;
using eLibrary.Application.Common.Helpers;

namespace eLibrary.Application.Commands.Books;

public class UpdateBookAvailableCountCommand : IRequest<ApiResponse<BookDto>>
{
    public UpdateBookAvailableCountCommand(int bookId, int newAvailableCount)
    {
        BookId = bookId;
        NewAvailableCount = newAvailableCount;
    }

    public int BookId { get; set; }
    public int NewAvailableCount { get; set; }
}

public class UpdateBookAvailableCountHandler : IRequestHandler<UpdateBookAvailableCountCommand, ApiResponse<BookDto>>
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<UpdateBookAvailableCountHandler> _logger;

    public UpdateBookAvailableCountHandler(IBookRepository bookRepository, ILogger<UpdateBookAvailableCountHandler> logger)
    {
        _bookRepository = bookRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<BookDto>> Handle(UpdateBookAvailableCountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate input
            if (request.NewAvailableCount < 0)
                return ApiResponse<BookDto>.Fail("AvailableCount cannot be negative.");

            // Get book from repository
            var book = await _bookRepository.GetBookByIdAsync(request.BookId, cancellationToken);
            if (book == null)
                return ApiResponse<BookDto>.Fail($"Book with Id {request.BookId} not found.");

            // Update available count
            book.AvailableCount = request.NewAvailableCount;

            // Save changes
            await _bookRepository.UpdateAsync(book, cancellationToken);

            // Convert updated book to DTO
            var bookDto = BookHelper.MapBookToDto(book);
            return ApiResponse<BookDto>.Ok(bookDto, $"Book available count updated to {request.NewAvailableCount}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update available count for book Id {BookId}", request.BookId);
            return ApiResponse<BookDto>.Fail($"An error occurred while updating the available count: {ex.Message}");
        }
    }
}

