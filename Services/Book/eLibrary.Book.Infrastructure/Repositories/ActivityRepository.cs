using eLibrary.Book.Application.DTOs;
using eLibrary.Book.Application.Interfaces.Repositories;
using eLibrary.Book.Domain.Entities;
using eLibrary.Book.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace eLibrary.Book.Infrastructure.Repositories;

public class ActivityRepository : IActivityRepository
{
    private readonly BookDbContext _context;

    public ActivityRepository(BookDbContext context)
    {
        _context = context;
    }

    public Task<List<ActivityEventDto>> GetRecentAsync(int count, CancellationToken cancellationToken) =>
        _context.ActivityEvents
            .AsNoTracking()
            .OrderByDescending(activity => activity.OccurredAt)
            .ThenByDescending(activity => activity.Id)
            .Take(count)
            .Select(activity => new ActivityEventDto
            {
                Id = activity.Id,
                EventType = activity.EventType,
                UserId = activity.UserId,
                BookName = activity.BookName,
                OccurredAt = activity.OccurredAt,
                IsApproximate = activity.IsApproximate
            })
            .ToListAsync(cancellationToken);

    public async Task BackfillHistoryAsync(CancellationToken cancellationToken)
    {
        var existingKeys = await _context.ActivityEvents
            .AsNoTracking()
            .Select(activity => new { activity.EventType, activity.SubjectId })
            .ToListAsync(cancellationToken);
        var knownKeys = existingKeys
            .Select(activity => (activity.EventType, activity.SubjectId))
            .ToHashSet();
        var books = await _context.Books
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Select(book => new
            {
                book.Id,
                book.Title,
                book.CreatedBy,
                book.CreatedAt
            })
            .ToListAsync(cancellationToken);

        foreach (var book in books)
        {
            AddIfMissing(
                ActivityEventTypes.BookAdded,
                book.Id,
                book.CreatedBy,
                book.Id,
                book.Title ?? "Unknown Book",
                book.CreatedAt);
        }

        var bookNames = books.ToDictionary(book => book.Id, book => book.Title ?? "Unknown Book");
        var records = await _context.BorrowRecords
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;

        foreach (var record in records)
        {
            var bookName = bookNames.GetValueOrDefault(record.BookId, "Unknown Book");
            AddIfMissing(
                ActivityEventTypes.BookBorrowed,
                record.Id,
                record.UserId,
                record.BookId,
                bookName,
                record.BorrowDate);

            if (record.IsReturned)
            {
                AddIfMissing(
                    ActivityEventTypes.BookReturned,
                    record.Id,
                    record.UserId,
                    record.BookId,
                    bookName,
                    record.ReturnDate ?? record.UpdatedAt,
                    isApproximate: !record.ReturnDate.HasValue);
            }

            var loanEndedAt = record.ReturnDate ?? now;
            if (record.DueDate < loanEndedAt)
            {
                AddIfMissing(
                    ActivityEventTypes.LoanOverdue,
                    record.Id,
                    record.UserId,
                    record.BookId,
                    bookName,
                    record.DueDate,
                    isApproximate: true);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        void AddIfMissing(
            string eventType,
            Guid subjectId,
            Guid? userId,
            Guid? bookId,
            string bookName,
            DateTime occurredAt,
            bool isApproximate = false)
        {
            if (!knownKeys.Add((eventType, subjectId)))
                return;

            _context.ActivityEvents.Add(new ActivityEvent
            {
                EventType = eventType,
                SubjectId = subjectId,
                UserId = userId,
                BookId = bookId,
                BookName = bookName,
                OccurredAt = occurredAt,
                IsApproximate = isApproximate
            });
        }
    }

    public async Task RecordNewOverdueAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var overdueLoans = await (
            from record in _context.BorrowRecords.AsNoTracking()
            join book in _context.Books.AsNoTracking() on record.BookId equals book.Id
            where !record.IsReturned && record.DueDate < now
            select new
            {
                record.Id,
                record.UserId,
                record.BookId,
                BookName = book.Title ?? "Unknown Book",
                record.DueDate
            }
        ).ToListAsync(cancellationToken);
        var overdueIds = overdueLoans.Select(loan => loan.Id).ToList();
        var existingIds = await _context.ActivityEvents
            .AsNoTracking()
            .Where(activity =>
                activity.EventType == ActivityEventTypes.LoanOverdue &&
                overdueIds.Contains(activity.SubjectId))
            .Select(activity => activity.SubjectId)
            .ToListAsync(cancellationToken);
        var existingSet = existingIds.ToHashSet();

        foreach (var loan in overdueLoans)
        {
            if (existingSet.Contains(loan.Id))
                continue;

            _context.ActivityEvents.Add(new ActivityEvent
            {
                EventType = ActivityEventTypes.LoanOverdue,
                SubjectId = loan.Id,
                UserId = loan.UserId,
                BookId = loan.BookId,
                BookName = loan.BookName,
                OccurredAt = loan.DueDate,
                IsApproximate = false
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
