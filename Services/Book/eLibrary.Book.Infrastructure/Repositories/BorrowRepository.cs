using eLibrary.Book.Application.Interfaces.Repositories;
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

    public void Update(BorrowRecordEntity borrowRecord)
    {
        _context.BorrowRecords.Update(borrowRecord);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _context.SaveChangesAsync(cancellationToken);
}
