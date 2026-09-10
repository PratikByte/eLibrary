using eLibrary.Book.Application.Interfaces.Repositories;
using eLibrary.Book.Application.Shared;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace eLibrary.Book.Application.Commands.Books;

/// <summary>
/// Command to replace the cover image for an existing book.
/// </summary>
public class UpdateBookCoverCommand : IRequest<ApiResponse<bool>>
{
    public UpdateBookCoverCommand(int bookId, IFormFile coverImage)
    {
        BookId = bookId;
        CoverImage = coverImage;
    }

    public int BookId { get; }
    public IFormFile CoverImage { get; }
}

public class UpdateBookCoverCommandHandler : IRequestHandler<UpdateBookCoverCommand, ApiResponse<bool>>
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<UpdateBookCoverCommandHandler> _logger;
    private readonly string _coverFolder = Path.Combine("wwwroot", "book-covers");

    public UpdateBookCoverCommandHandler(IBookRepository bookRepository, ILogger<UpdateBookCoverCommandHandler> logger)
    {
        _bookRepository = bookRepository;
        _logger = logger;

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

            var fileName = Guid.NewGuid() + Path.GetExtension(request.CoverImage.FileName);
            var fullPath = Path.Combine(_coverFolder, fileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await request.CoverImage.CopyToAsync(stream, cancellationToken);
            }

            book.CoverImagePath = Path.Combine("book-covers", fileName).Replace('\\', '/');
            await _bookRepository.UpdateAsync(book, cancellationToken);

            return ApiResponse<bool>.Ok(true, "Book cover updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cover for book {BookId}", request.BookId);
            return ApiResponse<bool>.Fail("An error occurred while updating the book cover.", ex.Message);
        }
    }
}
