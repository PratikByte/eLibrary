using eLibrary.Book.Application.Interfaces.Repositories;
using eLibrary.Book.Domain.Entities;
using bookEntity = eLibrary.Book.Domain.Entities.Book;
using eLibrary.Book.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace eLibrary.Book.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Book Service.
/// This is the only place where DB access for books lives.
/// </summary>
public class BookRepository : IBookRepository
{
    private readonly BookDbContext _context;
    private readonly ILogger<BookRepository> _logger;

    public BookRepository(BookDbContext context, ILogger<BookRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task AddBookAsync(bookEntity book)
    {
        _context.Books.Add(book);
        _context.ActivityEvents.Add(new ActivityEvent
        {
            EventType = ActivityEventTypes.BookAdded,
            SubjectId = book.Id,
            BookId = book.Id,
            BookName = book.Title ?? "Unknown Book",
            OccurredAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteBookByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (book == null) return false;

        book.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<(List<bookEntity> Books, int TotalRecords)> GetAllAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm,
        bool? inStock,
        float? minPrice,
        float? maxPrice,
        CancellationToken cancellationToken)
    {
        var query = _context.Books.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            var hasId = Guid.TryParse(searchTerm.Trim(), out var bookId);
            query = query.Where(book =>
                (book.Title != null && book.Title.ToLower().Contains(term)) ||
                (book.Author != null && book.Author.ToLower().Contains(term)) ||
                (hasId && book.Id == bookId));
        }

        if (inStock.HasValue)
        {
            query = inStock.Value
                ? query.Where(book => book.AvailableCount > 0)
                : query.Where(book => book.AvailableCount <= 0);
        }

        if (minPrice.HasValue)
        {
            query = query.Where(book => book.Price >= minPrice.Value);
        }

        if (maxPrice.HasValue)
        {
            query = query.Where(book => book.Price <= maxPrice.Value);
        }

        var totalRecords = await query.CountAsync(cancellationToken);

        var books = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (books, totalRecords);
    }

    public async Task<(List<bookEntity> Books, int TotalRecords)> GetAvailBookAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.Books.Where(b => b.AvailableCount > 0);
        var totalRecords = await query.CountAsync(cancellationToken);

        var books = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (books, totalRecords);
    }

    public async Task<(List<bookEntity> Books, int TotalRecords)> PagedGetBookByAuthorAsync(int pageNumber, int pageSize, string author, CancellationToken cancellationToken)
    {
        var query = _context.Books
            .Where(b => b.Author != null && b.Author.Trim().ToLower() == author.Trim().ToLower());

        var totalRecords = await query.CountAsync(cancellationToken);

        var books = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (books, totalRecords);
    }

    public async Task<bookEntity?> GetBookByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<List<bookEntity>> GetBooksByPriceRangeAsync(float minPrice, float maxPrice, CancellationToken cancellationToken)
    {
        return await _context.Books
            .Where(b => b.Price >= minPrice && b.Price <= maxPrice)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsDuplicate(string title, string author)
    {
        var normalizedTitle = title.Trim();
        var normalizedAuthor = author.Trim();

        return await _context.Books.AnyAsync(b =>
            b.Title != null &&
            b.Author != null &&
            b.Title.Trim().ToLower() == normalizedTitle.ToLower() &&
            b.Author.Trim().ToLower() == normalizedAuthor.ToLower());
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(bookEntity book, CancellationToken cancellationToken)
    {
        _context.Books.Update(book);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAvailableCountAsync(Guid bookId, int newAvailableCount, CancellationToken cancellationToken)
    {
        var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId, cancellationToken);
        if (book == null)
            throw new KeyNotFoundException($"Book with Id {bookId} not found.");

        book.AvailableCount = newAvailableCount;
        _context.Books.Update(book);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IncreaseAvailBookCountAsync(Guid bookId, CancellationToken cancellationToken)
    {
        var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId, cancellationToken);
        if (book == null) return false;

        book.AvailableCount += 1;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DecreaseAvailBookCountAsync(Guid bookId, CancellationToken cancellationToken)
    {
        var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId, cancellationToken);
        if (book == null) return false;

        if (book.AvailableCount <= 0) return false;

        book.AvailableCount -= 1;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
