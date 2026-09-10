using BookEntity = eLibrary.Book.Domain.Entities.Book;

namespace eLibrary.Book.Application.Interfaces.Repositories;

/// <summary>
/// Data access contract for the Book Service.
/// This keeps the service domain logic isolated from EF Core details.
/// </summary>
public interface IBookRepository
{
    Task<(List<BookEntity> Books, int TotalRecords)> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<(List<BookEntity> Books, int TotalRecords)> GetAvailBookAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
    Task<(List<BookEntity> Books, int TotalRecords)> PagedGetBookByAuthorAsync(int pageNumber, int pageSize, string author, CancellationToken cancellationToken);
    Task<BookEntity?> GetBookByIdAsync(int id, CancellationToken cancellationToken);
    Task UpdateAsync(BookEntity book, CancellationToken cancellationToken);
    Task<List<BookEntity>> GetBooksByPriceRangeAsync(float minPrice, float maxPrice, CancellationToken cancellationToken);
    Task<bool> IsDuplicate(string title, string author);
    Task AddBookAsync(BookEntity book);
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task<bool> DeleteBookByIdAsync(int id, CancellationToken cancellationToken);
    Task UpdateAvailableCountAsync(int bookId, int newAvailableCount, CancellationToken cancellationToken);
    Task<bool> IncreaseAvailBookCountAsync(int bookId, CancellationToken cancellationToken);
    Task<bool> DecreaseAvailBookCountAsync(int bookId, CancellationToken cancellationToken);
}
