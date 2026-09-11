using BorrowRecordEntity = eLibrary.Book.Domain.Entities.BorrowRecord;

namespace eLibrary.Book.Application.Interfaces.Repositories;

public interface IBorrowRepository
{
    Task AddAsync(BorrowRecordEntity borrowRecord, CancellationToken cancellationToken);
    Task<BorrowRecordEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<BorrowRecordEntity>> GetOverdueAsync(CancellationToken cancellationToken);
    void Update(BorrowRecordEntity borrowRecord);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
