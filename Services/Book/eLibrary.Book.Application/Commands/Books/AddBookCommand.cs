using eLibrary.Book.Application.DTOs;
using eLibrary.Book.Application.Interfaces.Repositories;
using eLibrary.Book.Application.Interfaces;
using eLibrary.Book.Application.Shared;
using BookEntity = eLibrary.Book.Domain.Entities.Book;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace eLibrary.Book.Application.Commands.Books;

/// <summary>
/// Command to create a new book.
/// Keeps creation logic inside the Book Service.
/// </summary>
public class AddBookCommand : IRequest<ApiResponse<Guid>>
{
    public AddBookCommand(CreateBookDto bookDto)
    {
        BookDto = bookDto;
    }

    public CreateBookDto BookDto { get; }
}

public class AddBookCommandHandler : IRequestHandler<AddBookCommand, ApiResponse<Guid>>
{
    private const long MaxCoverSize = 5 * 1024 * 1024;
    private static readonly HashSet<string> AllowedCoverExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp" };
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<AddBookCommandHandler> _logger;
    private readonly IMongoMediaStore _coverStore;

    public AddBookCommandHandler(
        IBookRepository bookRepository,
        ILogger<AddBookCommandHandler> logger,
        IMongoMediaStore coverStore)
    {
        _bookRepository = bookRepository;
        _logger = logger;
        _coverStore = coverStore;
    }

    public async Task<ApiResponse<Guid>> Handle(AddBookCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var isDuplicate = await _bookRepository.IsDuplicate(request.BookDto.Title, request.BookDto.Author);
            if (isDuplicate)
            {
                return ApiResponse<Guid>.Fail("Duplicate book entry.", "A book with the same title and author already exists.");
            }

            string? coverImageMongoId = null;

            if (request.BookDto.CoverImage is not null)
            {
                var extension = Path.GetExtension(request.BookDto.CoverImage.FileName);
                if (request.BookDto.CoverImage.Length == 0 ||
                    request.BookDto.CoverImage.Length > MaxCoverSize ||
                    !AllowedCoverExtensions.Contains(extension))
                {
                    return ApiResponse<Guid>.Fail("Cover image must be a non-empty JPG, PNG, or WebP file no larger than 5 MB.");
                }

                coverImageMongoId = await _coverStore.SaveAsync(request.BookDto.CoverImage, "book-cover", cancellationToken);
            }

            var newBook = new BookEntity
            {
                Title = request.BookDto.Title,
                Author = request.BookDto.Author,
                Price = request.BookDto.Price,
                PublishedYear = request.BookDto.PublishedYear,
                AvailableCount = request.BookDto.AvailableCount,
                CoverImageMongoId = coverImageMongoId
            };

            await _bookRepository.AddBookAsync(newBook);
            return ApiResponse<Guid>.Ok(newBook.Id, "Book added successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while adding book: {Title} by {Author}", request.BookDto.Title, request.BookDto.Author);
            return ApiResponse<Guid>.Fail("An error occurred while adding the book.", ex.Message);
        }
    }
}
