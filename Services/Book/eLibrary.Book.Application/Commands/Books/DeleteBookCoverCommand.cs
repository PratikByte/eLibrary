using eLibrary.Book.Application.Interfaces.Repositories;
using eLibrary.Book.Application.Interfaces;
using eLibrary.Book.Application.Shared;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

namespace eLibrary.Book.Application.Commands.Books;

/// <summary>
/// Command to remove a book cover.
/// </summary>
public class DeleteBookCoverCommand : IRequest<ApiResponse<bool>>
{
    public DeleteBookCoverCommand(Guid bookId)
    {
        BookId = bookId;
    }

    public Guid BookId { get; }
}

public class DeleteBookCoverCommandHandler : IRequestHandler<DeleteBookCoverCommand, ApiResponse<bool>>
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<DeleteBookCoverCommandHandler> _logger;
    private readonly IMongoMediaStore _coverStore;

    public DeleteBookCoverCommandHandler(
        IBookRepository bookRepository,
        ILogger<DeleteBookCoverCommandHandler> logger,
        IMongoMediaStore coverStore)
    {
        _bookRepository = bookRepository;
        _logger = logger;
        _coverStore = coverStore;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteBookCoverCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var book = await _bookRepository.GetBookByIdAsync(request.BookId, cancellationToken);
            if (book == null)
                return ApiResponse<bool>.Fail("Book not found.");

            if (!string.IsNullOrWhiteSpace(book.CoverImageMongoId))
                await _coverStore.DeleteAsync(book.CoverImageMongoId, cancellationToken);

            book.CoverImageMongoId = null;
            await _bookRepository.UpdateAsync(book, cancellationToken);

            return ApiResponse<bool>.Ok(true, "Book cover deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting cover for book {BookId}", request.BookId);
            return ApiResponse<bool>.Fail("An error occurred while deleting the book cover.", ex.Message);
        }
    }
}
