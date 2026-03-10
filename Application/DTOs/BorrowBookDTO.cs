namespace eLibrary.Application.DTOs;

public class BorrowBookDTO
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; }
    public int BookId { get; set; }
    public string BookName { get; set; }
    public string BorrowDate { get; set; }
    public string DueDate { get; set; }
}

