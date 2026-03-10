namespace eLibrary.Application.DTOs;

public class BookDto
{   
    public int Id { get; set; }             // Unique identifier for the book
    public string Title { get; set; }        // Book title
    public string Author { get; set; }       // Author name

    public int AvailableCount { get; set; }

    // ✅ For returning cover to user
    public string? CoverImageUrl { get; set; }
}

// When price is needed
public class BookWithPriceDto : BookDto
{
    public float Price { get; set; }
}

