namespace eLibrary.Book.Application.DTOs;

public class ActiveBorrowRecordDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid BookId { get; set; }
    public string BookName { get; set; } = string.Empty;
    public DateTime BorrowDate { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsOverdue { get; set; }
}
