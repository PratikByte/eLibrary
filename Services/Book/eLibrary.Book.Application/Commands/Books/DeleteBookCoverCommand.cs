using eLibrary.Book.Application.Interfaces.Repositories;
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
    public DeleteBookCoverCommand(int bookId)
    {
        BookId = bookId;
    }

    public int BookId { get; }
}

public class DeleteBookCoverCommandHandler : IRequestHandler<DeleteBookCoverCommand, ApiResponse<bool>>
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<DeleteBookCoverCommandHandler> _logger;
    private readonly string _coverFolder;

    public DeleteBookCoverCommandHandler(
        IBookRepository bookRepository,
        ILogger<DeleteBookCoverCommandHandler> logger,
        IWebHostEnvironment environment)
    {
        _bookRepository = bookRepository;
        _logger = logger;
        _coverFolder = Path.Combine(environment.ContentRootPath, "wwwroot", "book-covers");
    }

    public async Task<ApiResponse<bool>> Handle(DeleteBookCoverCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var book = await _bookRepository.GetBookByIdAsync(request.BookId, cancellationToken);
            if (book == null)
                return ApiResponse<bool>.Fail("Book not found.");

            if (!string.IsNullOrWhiteSpace(book.CoverImagePath))
            {
                var fullPath = Path.Combine(_coverFolder, Path.GetFileName(book.CoverImagePath));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }

            book.CoverImagePath = null;
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
