namespace eLibrary.Book.Domain.Entities;

public class ActivityEvent : BaseEntity
{
    public string EventType { get; set; } = string.Empty;
    public Guid SubjectId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? BookId { get; set; }
    public string BookName { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public bool IsApproximate { get; set; }
}

public static class ActivityEventTypes
{
    public const string BookAdded = "book-added";
    public const string BookBorrowed = "book-borrowed";
    public const string BookReturned = "book-returned";
    public const string LoanOverdue = "loan-overdue";
}
