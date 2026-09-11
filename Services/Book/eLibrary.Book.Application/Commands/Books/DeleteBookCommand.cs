using eLibrary.Book.Application.Interfaces.Repositories;
using eLibrary.Book.Application.Shared;
using MediatR;
using Microsoft.Extensions.Logging;

namespace eLibrary.Book.Application.Commands.Books;

/// <summary>
/// Command to delete a book.
/// </summary>
public class DeleteBookCommand : IRequest<ApiResponse<bool>>
{
    public DeleteBookCommand(Guid bookId)
    {
        BookId = bookId;
    }

    public Guid BookId { get; }
}

public class DeleteBookCommandHandler : IRequestHandler<DeleteBookCommand, ApiResponse<bool>>
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<DeleteBookCommandHandler> _logger;

    public DeleteBookCommandHandler(IBookRepository bookRepository, ILogger<DeleteBookCommandHandler> logger)
    {
        _bookRepository = bookRepository;
        _logger = logger;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteBookCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _bookRepository.DeleteBookByIdAsync(request.BookId, cancellationToken);
            if (!deleted)
                return ApiResponse<bool>.Fail("Book not found.");

            return ApiResponse<bool>.Ok(true, "Book deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting book {BookId}", request.BookId);
            return ApiResponse<bool>.Fail("An error occurred while deleting the book.", ex.Message);
        }
    }
}
