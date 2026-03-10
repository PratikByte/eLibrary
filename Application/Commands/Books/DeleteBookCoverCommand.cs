using MediatR;
using eLibrary.Application.Interfaces.Repositories;
using eLibrary.Shared;


namespace eLibrary.Application.Commands.Books;

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

    public DeleteBookCoverCommandHandler(IBookRepository bookRepository, ILogger<DeleteBookCoverCommandHandler> logger)
    {
        _bookRepository = bookRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteBookCoverCommand request, CancellationToken cancellationToken)
    {
        try
        {
            //check book
            var book = await _bookRepository.GetBookByIdAsync(request.BookId, cancellationToken);
            if (book == null)
            {
                return ApiResponse<bool>.Fail($"No book found with Id {request.BookId}");
            }
            //check cover
            if (string.IsNullOrEmpty(book.CoverImagePath))
            {
                return ApiResponse<bool>.Fail("Book has no cover to delete.");

            }
            //delete old cover
            var coverPath = Path.Combine("wwwroot", book.CoverImagePath);
            if (File.Exists(coverPath)) File.Delete(coverPath);

            //update new cover
            book.CoverImagePath = null;
            await _bookRepository.UpdateAsync(book,cancellationToken);

            return ApiResponse<bool>.Ok(true, "Book cover deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting cover for BookId {BookId}", request.BookId);
            return ApiResponse<bool>.Fail($"An error occurred while deleting cover: {ex.Message}");
        }

    }
   }

