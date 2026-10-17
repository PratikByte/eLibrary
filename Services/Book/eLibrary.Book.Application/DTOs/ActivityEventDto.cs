namespace eLibrary.Book.Application.DTOs;

public class ActivityEventDto
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string BookName { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public bool IsApproximate { get; set; }
}
