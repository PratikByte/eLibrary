using MediatR;
using eLibrary.Application.Commands.Books;
using eLibrary.Application.Interfaces.Repositories;
using eLibrary.Application.Interfaces.Services;
using eLibrary.Shared;

namespace eLibrary.Application.Commands.Books
{
    public class DeleteBookByIDCommand:IRequest<ApiResponse<bool>>
    {
        public int Id { get; set; }
        public DeleteBookByIDCommand(int id)
        {
            Id = id;
        }
    }
      
}
public class DeleteBookByIDHandler : IRequestHandler<DeleteBookByIDCommand, ApiResponse<bool>>
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<DeleteBookByIDHandler> _logger;
    private readonly IFileStorage _fileStorage;


    public DeleteBookByIDHandler(
        IBookRepository bookRepository,
        IFileStorage fileStorage,
        ILogger<DeleteBookByIDHandler> logger)
    {
        _bookRepository = bookRepository;
        _fileStorage = fileStorage;
        _logger = logger;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteBookByIDCommand request, CancellationToken cancellationToken)
    {
        try
        {   // 1️⃣ Fetch book 
            var book = await _bookRepository.GetBookByIdAsync(request.Id, cancellationToken);
            if (book == null)
            {
                return ApiResponse<bool>.Fail($"Book with Id {request.Id} not found.");
            }
            // 2️⃣ Store cover path before deletion
            var coverPath = book.CoverImagePath;
            // 3️⃣ Delete book from database
            var deleted = await _bookRepository.DeleteBookByIdAsync(request.Id, cancellationToken);
            if (!deleted)
            {
                return ApiResponse<bool>.Fail($"Book with Id {request.Id} not found or could not be deleted.");
            }
            

            // 4️⃣ Delete cover from file storage
            if (!string.IsNullOrEmpty(coverPath))
                {
                    await _fileStorage.DeleteAsync(coverPath,cancellationToken);
                }
                return ApiResponse<bool>.Ok(true, $"Book with Id {request.Id} deleted successfully.");
        }
        catch(Exception ex)
        {
                       
            _logger.LogError(ex, "An error occurred while deleting the book with Id {BookId}", request.Id);
            return ApiResponse<bool>.Fail("An error occurred while deleting the book.");

        }
    }
}
