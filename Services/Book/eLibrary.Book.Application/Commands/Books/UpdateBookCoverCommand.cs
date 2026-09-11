using eLibrary.Book.Application.Interfaces.Repositories;
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
    private readonly string _coverFolder = Path.Combine("wwwroot", "book-covers");

    public UpdateBookCoverCommandHandler(
        IBookRepository bookRepository,
        ILogger<UpdateBookCoverCommandHandler> logger,
        IWebHostEnvironment environment)
    {
        _bookRepository = bookRepository;
        _logger = logger;
        _coverFolder = Path.Combine(environment.ContentRootPath, "wwwroot", "book-covers");

        if (!Directory.Exists(_coverFolder))
            Directory.CreateDirectory(_coverFolder);
    }

    public async Task<ApiResponse<bool>> Handle(UpdateBookCoverCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var book = await _bookRepository.GetBookByIdAsync(request.BookId, cancellationToken);
            if (book == null)
                return ApiResponse<bool>.Fail("Book not found.");

            var extension = Path.GetExtension(request.CoverImage.FileName);
            if (request.CoverImage.Length == 0 ||
                request.CoverImage.Length > MaxCoverSize ||
                !AllowedCoverExtensions.Contains(extension))
            {
                return ApiResponse<bool>.Fail("Cover image must be a non-empty JPG, PNG, or WebP file no larger than 5 MB.");
            }

            var oldCoverPath = book.CoverImagePath;
            var fileName = Guid.NewGuid() + extension.ToLowerInvariant();
            var fullPath = Path.Combine(_coverFolder, fileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await request.CoverImage.CopyToAsync(stream, cancellationToken);
            }

            book.CoverImagePath = Path.Combine("book-covers", fileName).Replace('\\', '/');
            await _bookRepository.UpdateAsync(book, cancellationToken);

            if (!string.IsNullOrWhiteSpace(oldCoverPath))
            {
                var oldFullPath = Path.Combine(
                    _coverFolder,
                    Path.GetFileName(oldCoverPath));
                if (File.Exists(oldFullPath) && !string.Equals(oldFullPath, fullPath, StringComparison.OrdinalIgnoreCase))
                {
                    File.Delete(oldFullPath);
                }
            }

            return ApiResponse<bool>.Ok(true, "Book cover updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cover for book {BookId}", request.BookId);
            return ApiResponse<bool>.Fail("An error occurred while updating the book cover.", ex.Message);
        }
    }
}
