namespace eLibrary.Application.DTOs;

public class CreateBookDto
{
    public string Title { get; set; }
    public string Author { get; set; }
    public float Price { get; set; }
    public int PublishedYear { get; set; }
    public int AvailableCount { get; set; }

    // New → Optional book cover
    public IFormFile? CoverImage { get; set; }
}

