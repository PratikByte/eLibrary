using BookStoreAPI_updated.Model;
using MediatR;
using eLibrary.Application.DTOs;
using eLibrary.Application.Interfaces.Repositories;
using eLibrary.Application.Interfaces.Services;
using eLibrary.Shared;

namespace eLibrary.Application.Commands.Books;

public class UpdateBookCoverCommand:IRequest<ApiResponse<BookDto>>
{
    public UpdateBookCoverCommand(int bookId, IFormFile coverImage)
    {
        BookId = bookId;
        CoverImage = coverImage;
    }

    public int BookId { get; }
    public IFormFile CoverImage { get; }

}

public class UpdateBookCoverHandler
: IRequestHandler<UpdateBookCoverCommand, ApiResponse<BookDto>>
{
// Repository → talks to database (Book table)
private readonly IBookRepository _bookRepository;

// File storage → handles file system / uploads (disk, S3, etc.)
private readonly IFileStorage _fileStorage;

// Logger → records errors and diagnostics
private readonly ILogger<UpdateBookCoverHandler> _logger;

// Constructor injection keeps handler loosely coupled
public UpdateBookCoverHandler(
    IBookRepository bookRepository,
    IFileStorage fileStorage,
    ILogger<UpdateBookCoverHandler> logger)
{
    _bookRepository = bookRepository;
    _fileStorage = fileStorage;
    _logger = logger;
}

public async Task<ApiResponse<BookDto>> Handle(
    UpdateBookCoverCommand request,
    CancellationToken cancellationToken)
{
    try
    {
        // 1️⃣ Validate incoming file (business validation)
        // Prevent null file or empty upload
        if (request.CoverImage == null || request.CoverImage.Length == 0)
        {
            return ApiResponse<BookDto>.Fail("Invalid cover image file.");
        }

        // 2️⃣ Fetch book from database
        // If book does not exist, no point continuing
        var book = await _bookRepository
            .GetBookByIdAsync(request.BookId, cancellationToken);

        if (book == null)
        {
            return ApiResponse<BookDto>.Fail(
                $"No book found with Id {request.BookId}");
        }

        // 3️⃣ Decide where this file belongs (logical grouping)
        // Example: uploads/books/12/
        var scope = $"books/{book.Id}";

        // Extract safe file name (prevents path injection)
        var safeName = Path.GetFileName(request.CoverImage.FileName);

        // 4️⃣ Open the file stream
        // Handler owns the stream → must dispose it
        using var stream = request.CoverImage.OpenReadStream();

        // 5️⃣ Save file via storage abstraction
        // Storage decides disk path, naming, and structure
        var newPath = await _fileStorage.SaveAsync(
            scope,
            safeName,
            stream,
            cancellationToken);

        // 6️⃣ Preserve old cover path (for cleanup later)
        var oldPath = book.CoverImagePath;

        // 7️⃣ Update book entity with new cover path
        book.CoverImagePath = newPath;

        // 8️⃣ Persist changes to database
        await _bookRepository.UpdateAsync(book, cancellationToken);

        // 9️⃣ Best-effort cleanup of old file
        // Happens AFTER DB update to avoid data loss
        if (!string.IsNullOrEmpty(oldPath))
        {
            await _fileStorage.DeleteAsync(oldPath, cancellationToken);
        }

        // 🔟 Map entity to DTO and return success response
        return ApiResponse<BookDto>.Ok(
            BookHelper.MapBookToDto(book),
            "Book cover updated successfully.");
    }
    catch (Exception ex)
    {
        // Log detailed error for developers/support
        _logger.LogError(
            ex,
            "Error updating cover for BookId {BookId}",
            request.BookId);

        // Return safe, generic message to client
        return ApiResponse<BookDto>.Fail(
            "An error occurred while updating the book cover.");
    }
}
}

