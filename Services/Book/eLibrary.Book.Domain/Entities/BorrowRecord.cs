using System.ComponentModel.DataAnnotations;

namespace eLibrary.Book.Domain.Entities;

public class BorrowRecord : BaseEntity
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid BookId { get; set; }
    public string BookName { get; set; } = string.Empty;
    public DateTime BorrowDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public bool IsReturned { get; set; }
    public decimal? Fine { get; set; }
}
