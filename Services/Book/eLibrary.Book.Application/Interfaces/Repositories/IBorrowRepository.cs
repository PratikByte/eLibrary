using BorrowRecordEntity = eLibrary.Book.Domain.Entities.BorrowRecord;
using eLibrary.Book.Application.DTOs;

namespace eLibrary.Book.Application.Interfaces.Repositories;

public interface IBorrowRepository
{
    Task AddAsync(BorrowRecordEntity borrowRecord, CancellationToken cancellationToken);
    Task<BorrowRecordEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<ActiveBorrowRecordDto>> GetActiveAsync(CancellationToken cancellationToken);
    Task<List<BorrowTransactionDto>> GetRecentAsync(int count, CancellationToken cancellationToken);
    Task<List<BorrowRecordEntity>> GetOverdueAsync(CancellationToken cancellationToken);
    Task<bool> TryReturnAsync(
        Guid borrowId,
        Guid bookId,
        DateTime returnDate,
        decimal? fine,
        CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
