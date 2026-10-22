using eLibrary.Book.Application.Interfaces.Repositories;
using eLibrary.Book.Application.Interfaces;
using eLibrary.Book.Application.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace eLibrary.Book.Application.Commands.Books;

/// <summary>
/// Command to replace the cover image for an existing book.
/// </summary>
public class UpdateBookCoverCommand : IRequest<ApiResponse<bool>>
{
    public UpdateBookCoverCommand(Guid bookId, IFormFile coverImage)
    {
        BookId = bookId;
        CoverImage = coverImage;
    }

    public Guid BookId { get; }
    public IFormFile CoverImage { get; }
}

public class UpdateBookCoverCommandHandler : IRequestHandler<UpdateBookCoverCommand, ApiResponse<bool>>
{
    private const long MaxCoverSize = 5 * 1024 * 1024;
    private static readonly HashSet<string> AllowedCoverExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp" };
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<UpdateBookCoverCommandHandler> _logger;
    private readonly IMongoMediaStore _coverStore;

    public UpdateBookCoverCommandHandler(
        IBookRepository bookRepository,
        ILogger<UpdateBookCoverCommandHandler> logger,
        IMongoMediaStore coverStore)
    {
        _bookRepository = bookRepository;
        _logger = logger;
        _coverStore = coverStore;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateBookCoverCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var book = await _bookRepository.GetBookByIdAsync(request.BookId, cancellationToken);
            if (book == null)
                return ApiResponse<bool>.FailWithCode("NOT_FOUND", "Book not found.");

            var extension = Path.GetExtension(request.CoverImage.FileName);
            if (request.CoverImage.Length == 0 ||
                request.CoverImage.Length > MaxCoverSize ||
                !AllowedCoverExtensions.Contains(extension))
            {
                return ApiResponse<bool>.FailWithCode(
                    "VALIDATION_ERROR",
                    "Cover image must be a non-empty JPG, PNG, or WebP file no larger than 5 MB.");
            }

            var oldCoverId = book.CoverImageMongoId;
            book.CoverImageMongoId = await _coverStore.SaveAsync(request.CoverImage, "book-cover", cancellationToken);
            await _bookRepository.UpdateAsync(book, cancellationToken);

            if (!string.IsNullOrWhiteSpace(oldCoverId))
                await _coverStore.DeleteAsync(oldCoverId, cancellationToken);

            return ApiResponse<bool>.Ok(true, "Book cover updated successfully.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cover for book {BookId}", request.BookId);
            return ApiResponse<bool>.FailWithCode(
                "INTERNAL_SERVER_ERROR",
                "An error occurred while updating the book cover.");
        }
    }
}
