using eLibrary.Application.DTOs;
using eLibrary.Application.Interfaces.Repositories;
using eLibrary.Shared;
using MediatR;
using Microsoft.AspNetCore.StaticFiles;

namespace eLibrary.Application.Queries.Books;

public record GetBookCoverDownloadQuery(int BookId) : IRequest<ApiResponse<FileDownloadDto>>;

public class GetBookCoverDownloadQueryHandler : IRequestHandler<GetBookCoverDownloadQuery, ApiResponse<FileDownloadDto>>
{
    private readonly IBookRepository _bookRepository;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<GetBookCoverDownloadQueryHandler> _logger;

    public GetBookCoverDownloadQueryHandler(
        IBookRepository bookRepository,
        IWebHostEnvironment environment,
        ILogger<GetBookCoverDownloadQueryHandler> logger)
    {
        _bookRepository = bookRepository;
        _environment = environment;
        _logger = logger;
    }

    public async Task<ApiResponse<FileDownloadDto>> Handle(GetBookCoverDownloadQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Step 1: load the book so we can read the stored cover path.
            var book = await _bookRepository.GetBookByIdAsync(request.BookId, cancellationToken);
            if (book == null)
            {
                return ApiResponse<FileDownloadDto>.Fail($"No book found with Id {request.BookId}");
            }

            // Step 2: ensure this book actually has a cover configured.
            if (string.IsNullOrWhiteSpace(book.CoverImagePath))
            {
                return ApiResponse<FileDownloadDto>.Fail("Book cover not found.");
            }

            // Step 3: map the relative path from DB to the physical file under wwwroot.
            var relativePath = book.CoverImagePath.Replace("/", Path.DirectorySeparatorChar.ToString());
            var fullPath = Path.Combine(_environment.WebRootPath, relativePath);
            if (!File.Exists(fullPath))
            {
                return ApiResponse<FileDownloadDto>.Fail("Book cover file not found on server.");
            }

            // Step 4: return the file metadata the controller needs to send a download response.
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fullPath, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            var download = new FileDownloadDto
            {
                FilePath = fullPath,
                FileName = Path.GetFileName(fullPath),
                ContentType = contentType
            };

            return ApiResponse<FileDownloadDto>.Ok(download);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while preparing cover download for BookId {BookId}", request.BookId);
            return ApiResponse<FileDownloadDto>.Fail("An error occurred while preparing the book cover download.");
        }
    }
}
