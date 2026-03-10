using eLibrary.Application.Interfaces.Repositories;
using eLibrary.Domain.Entities;
using eLibrary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eLibrary.Infrastructure.Repositories;

public class BookRepository : IBookRepository
{
     private readonly EBookDBContext _context;
    private readonly ILogger _logger;

    public BookRepository(EBookDBContext context, ILogger<BookRepository> logger)
    {
        _context = context;
        _logger = logger;
    }



    public async Task<bool> IncreaseAvailBookCountAsync(int bookId,  CancellationToken cancellationToken)
    {
        try
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId, cancellationToken);
            if (book == null)
                    return false;
            book.AvailableCount = book.AvailableCount + 1;
            await _context.SaveChangesAsync(cancellationToken);
            return  true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error increasing  count for book Id {BookId}", bookId);
            throw;
        }
    }
    public async Task<bool> DecreaseAvailBookCountAsync(int bookId,  CancellationToken cancellationToken)
    {
        try
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId, cancellationToken);
            if (book == null)
                    return false;
            if (book.AvailableCount > 0)
            {
                book.AvailableCount = book.AvailableCount - 1;
            }else
                return false;
            await _context.SaveChangesAsync(cancellationToken);
            return  true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error decreasing  count for book Id {BookId}", bookId);
            throw;
        }
    }
    public async Task AddBookAsync(Book book)
    {
        try

        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding book with Title {BookTitle} for Author {BookAuthor}", book.Title, book.Author);
            throw;
        }
    }

    public async Task<bool> DeleteBookByIdAsync(int id, CancellationToken cancellationToken)
    {
        try
        {

            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
            if (book == null)
            {
                return false; // Book not found
            }
            _context.Books.Remove(book);
            await _context.SaveChangesAsync(cancellationToken);
            return true; // Book deleted successfully
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting book with Id {BookId}", id);
            throw;
        }

    }



    //public async Task<List<Book>> GetAllAsync(CancellationToken cancellationToken)
    //{
    //    try
    //    {

    //        _logger.LogInformation("Fetching all books from the database.");
    //        return await _context.Books.ToListAsync();
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error fetching all books.");
    //        throw;
    //    }

    //}


    //pagination

    public async Task<(List<Book> Books, int TotalRecords)> GetAllAsync(int pageNumber, int pageSize, 
                    CancellationToken cancellationToken )
    {
        try
        {
      
            var totalRecords = await _context.Books.CountAsync(cancellationToken);

            var books = await _context.Books
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (books, totalRecords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching paged books.");
            throw;
        }
    }
    public async Task<(List<Book> Books, int TotalRecords)> GetAvailBookAsync(int pageNumber, int pageSize, 
                    CancellationToken cancellationToken )
    {
        try
        {

            var query = _context.Books
        .Where(b => b.AvailableCount > 0);   //  Only books in stock

            var totalRecords = await query.CountAsync(cancellationToken);

            var books = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (books, totalRecords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching available  books.");
            throw;
        }
    }

    public async Task<(List<Book> Books, int TotalRecords)> PagedGetBookByAuthorAsync(int pageNumber, int pageSize, string author, CancellationToken cancellationToken)
    {
        try
        {
            // Create a query to filter books by author
            var query = _context.Books
                                .Where(b => b.Author.Trim().ToLower() == author.Trim().ToLower());
            // Get the total number of records for the specified author
            var totalRecords= await query.CountAsync(cancellationToken);
            // Apply pagination to the query
            var books=await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
            return (books, totalRecords);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching paged books by author {Author}", author);
            throw;
        }
    }

    //public async Task<List<Book>> GetBookByAuthorAsync(string author, CancellationToken cancellationToken)
    //{
    //    try
    //    {
    //        return await _context.Book
    //                      .Where(b => b.Author.Trim().ToLower() == author.Trim().ToLower())
    //                      .ToListAsync(cancellationToken);
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, "Error fetching books by author {Author}", author);
    //        throw;
    //    }
    //}

    public async Task<Book> GetBookByIdAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            return await _context.Books.FirstOrDefaultAsync
                (b => b.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching book with Id {BookId}", id);
            throw;
        }
    }

    public async Task<List<Book>> GetBooksByPriceRangeAsync(float minPrice, float maxPrice, CancellationToken cancellationToken)
    {
        try
        {
            return await _context.Books
                                .Where(b => b.Price >= minPrice &&
                                            b.Price <= maxPrice)
                                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching books by price range {MinPrice} - {MaxPrice}", minPrice, maxPrice);
            throw;
        }
    }




    public async Task<bool> IsDuplicate(string title, string author)
    {
        try
        {
            string normalisedTitle = title.Trim().ToLower();
            string normalisedAuthor = author.Trim().ToLower();

            return await _context.Books.AnyAsync(b =>
            b.Title.ToLower().Trim() == normalisedTitle &&
            b.Author.ToLower().Trim() == normalisedAuthor);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for duplicate book with Title {BookTitle} and Author {BookAuthor}", title, author);
            throw;
        }
    }


    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes to the database.");
            throw;
        }
    }

    public async Task UpdateAsync(Book book, CancellationToken cancellationToken)
    {
        try
        {
            _context.Books.Update(book);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating book with Id {BookId}", book.Id);
            throw;
        }
    }

    public async Task UpdateAvailableCountAsync(int bookId, int newAvailableCount, CancellationToken cancellationToken)
    {
        try
        {
            var book = await _context.Books.FirstOrDefaultAsync(b => b.Id == bookId, cancellationToken);
            if (book == null)
                throw new KeyNotFoundException($"Book with Id {bookId} not found.");

            book.AvailableCount = newAvailableCount;
            _context.Books.Update(book);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating available count for book Id {BookId}", bookId);
            throw;
        }
    }

}

