using Microsoft.AspNetCore.Http;

namespace eLibrary.Book.Application.DTOs;

/// <summary>
/// Input model for creating a new book.
/// The cover image is optional so the service can accept either a book with or without a cover.
/// </summary>
public class CreateBookDto
{
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public float Price { get; set; }
    public int PublishedYear { get; set; }
    public int AvailableCount { get; set; }
    public IFormFile? CoverImage { get; set; }
}
