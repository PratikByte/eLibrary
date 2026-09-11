using System.ComponentModel.DataAnnotations;

namespace eLibrary.Book.Domain.Entities;

public class BorrowRecord
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int BookId { get; set; }
    public string BookName { get; set; } = string.Empty;
    public DateTime BorrowDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public bool IsReturned { get; set; }
    public decimal? Fine { get; set; }
}