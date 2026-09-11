using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace eLibrary.Book.Domain.Entities;

/// <summary>
/// Core book entity owned by the Book Service.
/// This model is intentionally focused only on book data and stock information.
/// </summary>
[Table("Books")]
public class Book : BaseEntity
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public float Price { get; set; }
    public int PublishedYear { get; set; }
    public int AvailableCount { get; set; }
    public string? CoverImagePath { get; set; }
}
