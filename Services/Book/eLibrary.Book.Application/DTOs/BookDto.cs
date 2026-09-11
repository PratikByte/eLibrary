namespace eLibrary.Book.Application.DTOs;

/// <summary>
/// Read model used by clients after a book is retrieved from the Book Service.
/// </summary>
public class BookDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public float? Price { get; set; }
    public int AvailableCount { get; set; }
    public string? CoverImageUrl { get; set; }
}

public class BookWithPriceDto : BookDto
{
}
