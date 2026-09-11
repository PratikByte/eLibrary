using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace eLibrary.Book.Application.DTOs;

/// <summary>
/// Input model for creating a new book.
/// The cover image is optional so the service can accept either a book with or without a cover.
/// </summary>
public class CreateBookDto
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Author { get; set; } = string.Empty;

    [Range(0.01, 1000000)]
    public float Price { get; set; }

    [Range(1, 9999)]
    public int PublishedYear { get; set; }

    [Range(0, int.MaxValue)]
    public int AvailableCount { get; set; }

    public IFormFile? CoverImage { get; set; }
}
