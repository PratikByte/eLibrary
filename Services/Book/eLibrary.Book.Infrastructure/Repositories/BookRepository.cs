using eLibrary.Book.Application.Interfaces.Repositories;
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

    public async Task<(List<bookEntity> Books, int TotalRecords)> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var totalRecords = await _context.Books.CountAsync(cancellationToken);

        var books = await _context.Books
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
