using eLibrary.Book.Application.Interfaces.Repositories;
using eLibrary.Book.Application.DTOs;
using eLibrary.Book.Application.Common.Helpers;
using eLibrary.Book.Domain.Entities;
using BorrowRecordEntity = eLibrary.Book.Domain.Entities.BorrowRecord;
using eLibrary.Book.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace eLibrary.Book.Infrastructure.Repositories;

public class BorrowRepository : IBorrowRepository
{
    private readonly BookDbContext _context;

    public BorrowRepository(BookDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(BorrowRecordEntity borrowRecord, CancellationToken cancellationToken)
    {
        await _context.BorrowRecords.AddAsync(borrowRecord, cancellationToken);
        await _context.ActivityEvents.AddAsync(new ActivityEvent
        {
            EventType = ActivityEventTypes.BookBorrowed,
            SubjectId = borrowRecord.Id,
            UserId = borrowRecord.UserId,
            BookId = borrowRecord.BookId,
            BookName = borrowRecord.BookName,
            OccurredAt = borrowRecord.BorrowDate
        }, cancellationToken);
    }

    public Task<BorrowRecordEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _context.BorrowRecords.FirstOrDefaultAsync(record => record.Id == id, cancellationToken);

    public Task<List<ActiveBorrowRecordDto>> GetActiveAsync(CancellationToken cancellationToken)
        => GetActiveRecordsQuery()
            .OrderBy(record => record.DueDate)
            .ToListAsync(cancellationToken);

    public async Task<(List<ActiveBorrowRecordDto> Records, int TotalRecords)> GetActivePagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = GetActiveRecordsQuery();
        var totalRecords = await query.CountAsync(cancellationToken);
        var records = await query
            .OrderBy(record => record.DueDate)
            .ThenBy(record => record.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (records, totalRecords);
    }

    private IQueryable<ActiveBorrowRecordDto> GetActiveRecordsQuery()
    {
        var now = DateTime.UtcNow;
        return (
            from record in _context.BorrowRecords.AsNoTracking()
            where !record.IsReturned
            join book in _context.Books.AsNoTracking() on record.BookId equals book.Id
            select new ActiveBorrowRecordDto
            {
                Id = record.Id,
                UserId = record.UserId,
                UserName = record.UserName,
                BookId = book.Id,
                BookName = book.Title ?? "Unknown Book",
                BorrowDate = record.BorrowDate,
                DueDate = record.DueDate,
                IsOverdue = record.DueDate < now
            });
    }

    public async Task<(List<BorrowTransactionDto> Transactions, int TotalRecords)> GetRecentPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = GetBorrowTransactionsQuery();
        var totalRecords = await query.CountAsync(cancellationToken);
        var transactions = await query
            .OrderByDescending(transaction => transaction.BorrowDate)
            .ThenByDescending(transaction => transaction.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (CalculateTransactionFines(transactions), totalRecords);
    }

    private IQueryable<BorrowTransactionDto> GetBorrowTransactionsQuery() =>
        from record in _context.BorrowRecords.AsNoTracking()
        join book in _context.Books.AsNoTracking() on record.BookId equals book.Id
        select new BorrowTransactionDto
        {
            Id = record.Id,
            UserId = record.UserId,
            UserName = record.UserName,
            BookId = book.Id,
            BookName = book.Title ?? "Unknown Book",
            Author = book.Author ?? "Unknown author",
            CoverImageUrl = book.CoverImageMongoId == null ? null : $"/api/Book/{book.Id}/cover",
            BorrowDate = record.BorrowDate,
            DueDate = record.DueDate,
            ReturnDate = record.ReturnDate,
            Fine = record.Fine ?? 0m,
            IsReturned = record.IsReturned,
        };

    private static List<BorrowTransactionDto> CalculateTransactionFines(
        List<BorrowTransactionDto> transactions)
    {
        var now = DateTime.UtcNow;
        foreach (var transaction in transactions)
        {
            transaction.OverdueDays = transaction.IsReturned
                ? 0
                : BorrowFineCalculator.GetDaysOverdue(transaction.DueDate, now);
            transaction.IsOverdue = transaction.OverdueDays > 0;
            if (!transaction.IsReturned)
                transaction.Fine = BorrowFineCalculator.Calculate(transaction.DueDate, now);
        }

        return transactions;
    }

    public Task<List<BorrowRecordEntity>> GetOverdueAsync(CancellationToken cancellationToken) =>
        GetOverdueWithBookNamesAsync(cancellationToken);

    private async Task<List<BorrowRecordEntity>> GetOverdueWithBookNamesAsync(CancellationToken cancellationToken)
    {
        var records = await _context.BorrowRecords
            .AsNoTracking()
            .Where(record => !record.IsReturned && record.DueDate < DateTime.UtcNow)
            .OrderBy(record => record.DueDate)
            .ToListAsync(cancellationToken);

        var bookIds = records.Select(record => record.BookId).Distinct().ToList();
        var bookNames = await _context.Books
            .AsNoTracking()
            .Where(book => bookIds.Contains(book.Id))
            .ToDictionaryAsync(book => book.Id, book => book.Title ?? "Unknown Book", cancellationToken);

        foreach (var record in records)
            record.BookName = bookNames.GetValueOrDefault(record.BookId, "Unknown Book");

        return records;
    }

    public async Task<bool> TryReturnAsync(
        Guid borrowId,
        Guid bookId,
        DateTime returnDate,
        decimal? fine,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        var loanDetails = await (
            from record in _context.BorrowRecords.AsNoTracking()
            join book in _context.Books.AsNoTracking() on record.BookId equals book.Id
            where record.Id == borrowId
            select new { record.UserId, BookName = book.Title ?? "Unknown Book" }
        ).FirstOrDefaultAsync(cancellationToken);

        var updatedRecords = await _context.BorrowRecords
            .Where(record => record.Id == borrowId && !record.IsReturned)
            .ExecuteUpdateAsync(updates => updates
                .SetProperty(record => record.IsReturned, true)
                .SetProperty(record => record.ReturnDate, returnDate)
                .SetProperty(record => record.Fine, fine)
                .SetProperty(record => record.UpdatedAt, returnDate), cancellationToken);

        if (updatedRecords == 0)
            return false;

        var updatedBooks = await _context.Books
            .Where(book => book.Id == bookId)
            .ExecuteUpdateAsync(updates => updates
                .SetProperty(book => book.AvailableCount, book => book.AvailableCount + 1)
                .SetProperty(book => book.UpdatedAt, returnDate), cancellationToken);

        if (updatedBooks == 0)
            throw new InvalidOperationException($"Book with ID {bookId} could not be found while recording its return.");

        if (loanDetails is null)
            throw new InvalidOperationException($"Borrow record with ID {borrowId} could not be found while recording its return.");

        _context.ActivityEvents.Add(new ActivityEvent
        {
            EventType = ActivityEventTypes.BookReturned,
            SubjectId = borrowId,
            UserId = loanDetails.UserId,
            BookId = bookId,
            BookName = loanDetails.BookName,
            OccurredAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _context.SaveChangesAsync(cancellationToken);
}
