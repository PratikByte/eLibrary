using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace eLibrary.Domain.Entities;

[Table("Books")]
public class Book
{
    [Key]
    [Column("BookId")]
    [JsonPropertyName("bookId")]
    public int Id { get; set; }          // Book ID
    public string? Title { get; set; }    // Book Title
    public string? Author { get; set; }   // Book Author

    public float Price { get; set; }  //price
    public int PublishedYear {  get; set; } //PublishedYear
    public  int AvailableCount { get; set; } //sold copy

    // New Property for Cover Image (stores relative file path or URL)
    public string? CoverImagePath { get; set; }


}

