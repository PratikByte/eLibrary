using eLibrary.Book.Application.Interfaces.Repositories;
using eLibrary.Book.Application.DTOs;
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
    }

    public Task<BorrowRecordEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _context.BorrowRecords.FirstOrDefaultAsync(record => record.Id == id, cancellationToken);

    public Task<List<ActiveBorrowRecordDto>> GetActiveAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        return (
            from record in _context.BorrowRecords.AsNoTracking()
            where !record.IsReturned
            join book in _context.Books.AsNoTracking() on record.BookId equals book.Id
            orderby record.DueDate
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
            }).ToListAsync(cancellationToken);
    }

    public Task<List<BorrowTransactionDto>> GetRecentAsync(
        int count,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        return (
            from record in _context.BorrowRecords.AsNoTracking()
            join book in _context.Books.AsNoTracking() on record.BookId equals book.Id
            orderby record.BorrowDate descending
            select new BorrowTransactionDto
            {
                Id = record.Id,
                UserId = record.UserId,
                BookId = book.Id,
                BookName = book.Title ?? "Unknown Book",
                BorrowDate = record.BorrowDate,
                DueDate = record.DueDate,
                ReturnDate = record.ReturnDate,
                IsReturned = record.IsReturned,
                IsOverdue = !record.IsReturned && record.DueDate < now
            }).Take(count).ToListAsync(cancellationToken);
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

        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _context.SaveChangesAsync(cancellationToken);
}
