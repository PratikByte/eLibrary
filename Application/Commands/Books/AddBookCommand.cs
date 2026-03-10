using eLibrary.Application.DTOs;
using eLibrary.Application.Interfaces.Repositories;
using eLibrary.Domain.Entities;
using eLibrary.Shared;
using MediatR;

namespace eLibrary.Application.Commands.Books;


public class AddBookCommand : IRequest<ApiResponse<int>>
{

    public AddBookCommand(CreateBookDto bookDto)
    {
        BookDto = bookDto;
    }

    public CreateBookDto BookDto { get; }
}


public class AddBookCommandHandler  : IRequestHandler<AddBookCommand, ApiResponse<int>>
{
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<AddBookCommandHandler> _logger;
    private readonly string _coverFolder = Path.Combine("wwwroot", "book-covers");


    public AddBookCommandHandler(IBookRepository bookRepository, ILogger<AddBookCommandHandler> logger)
    {
        _bookRepository = bookRepository;
        _logger = logger;

        // Ensure cover folder exists
        if (!Directory.Exists(_coverFolder))
            Directory.CreateDirectory(_coverFolder);
    }

    public async Task<ApiResponse<int>> Handle(AddBookCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // ✅ Check for duplicate book (title + author)
            var isDuplicate = await _bookRepository.IsDuplicate(request.BookDto.Title, request.BookDto.Author);
            if (isDuplicate)
            {
                return ApiResponse<int>.Fail("Duplicate book entry.", "A book with the same title and author already exists.");
            }

            string ?coverImagePath = null;

            // ✅ Handle cover image if provided
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.BookDto.CoverImage.FileName);

            var fullPath=Path.Combine(_coverFolder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await request.BookDto.CoverImage.CopyToAsync(stream, cancellationToken);
            }
            // Store relative path
            coverImagePath = Path.Combine("book-covers", fileName);
            // ✅ Map DTO → Entity
            var newBook = new Book
            {
                Title = request.BookDto.Title,
                Author = request.BookDto.Author,
                Price = request.BookDto.Price,
                PublishedYear = request.BookDto.PublishedYear,
                AvailableCount = request.BookDto.AvailableCount,
                CoverImagePath = coverImagePath

            };

            // ✅ Save
            await _bookRepository.AddBookAsync(newBook);

            // ✅ Response wrapped
            return ApiResponse<int>.Ok(newBook.Id, "Book added successfully with cover image.");
        }
        catch (Exception ex)
        {

            return ApiResponse<int>.Fail($"An error occurred while adding the book: {ex.Message}");
        }
    }
}

