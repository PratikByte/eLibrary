using eLibrary.Domain.Entities;

namespace eLibrary.Application.Interfaces.Repositories;

public interface IBookRepository
{
    Task<(List<Book> Books, int TotalRecords)> GetAllAsync(int pageNumber, int pageSize,
        CancellationToken cancellationToken);
    Task<(List<Book> Books, int TotalRecords)> GetAvailBookAsync(int pageNumber, int pageSize,
        CancellationToken cancellationToken);

    //Task<List<Book>> GetAllAsync(CancellationToken cancellationToken);
   // Task<List<Book>> GetBookByAuthorAsync(string author,CancellationToken cancellationToken);

    Task<(List<Book>Books ,int TotalRecords)> PagedGetBookByAuthorAsync(int pageNumber, int pageSize,string author,CancellationToken cancellationToken);//3
    Task<Book>GetBookByIdAsync(int id,CancellationToken cancellationToken);//2
    Task UpdateAsync(Book book, CancellationToken cancellationToken);//4

    Task<List<Book>> GetBooksByPriceRangeAsync(float minPrice,float maxPrice, CancellationToken cancellationToken);
    Task<bool> IsDuplicate(string title, string author);
    Task AddBookAsync(Book book);//1
    Task SaveChangesAsync(CancellationToken cancellationToken);
    Task <bool>DeleteBookByIdAsync(int id, CancellationToken cancellationToken);//5

    Task UpdateAvailableCountAsync(int bookId, int newAvailableCount, CancellationToken cancellationToken);

    Task<bool> IncreaseAvailBookCountAsync(int bookId,CancellationToken cancellationToken);
    Task<bool>  DecreaseAvailBookCountAsync(int bookId,CancellationToken cancellationToken);
}

