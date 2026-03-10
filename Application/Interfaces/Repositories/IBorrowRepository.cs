using eLibrary.Domain.Entities;

namespace eLibrary.Application.Interfaces.Repositories;

public interface IBorrowRepository
{
    Task<BorrowRecords?> GetByIdAsync(int id);
    Task<IEnumerable<BorrowRecords>> GetActiveAsync();
    Task<IEnumerable<BorrowRecords>> GetAllAsync();
    Task<IEnumerable<BorrowRecords>> GetOverdueAsync();
    Task AddAsync(BorrowRecords borrow);
    Task UpdateAsync(BorrowRecords borrow);
    Task SaveChangesAsync();
}

