namespace eLibrary.Book.Application.DTOs;

public class BorrowRecordDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int BookId { get; set; }
    public string BookName { get; set; } = string.Empty;
    public string BorrowDate { get; set; } = string.Empty;
    public string DueDate { get; set; } = string.Empty;
    public string? ReturnDate { get; set; }
    public bool IsReturned { get; set; }
    public decimal? Fine { get; set; }
}