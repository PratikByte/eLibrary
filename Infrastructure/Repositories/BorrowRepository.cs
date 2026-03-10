using eLibrary.Application.Interfaces.Repositories;
using eLibrary.Domain.Entities;
using eLibrary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eLibrary.Infrastructure.Repositories;

public class BorrowRepository : IBorrowRepository
{
    private readonly EBookDBContext _context;

    public BorrowRepository(EBookDBContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BorrowRecords>> GetAllAsync()
    {
        return await _context.BorrowRecords.ToListAsync();
    }
    public async Task AddAsync(BorrowRecords borrow)
    {
        await _context.BorrowRecords.AddAsync(borrow);
    }

    public async Task<IEnumerable<BorrowRecords>> GetActiveAsync()
    {
        return await _context.BorrowRecords
           .Where(b => !b.IsReturned).ToListAsync();
    }

    public async Task<BorrowRecords?> GetByIdAsync(int id)
                    => await _context.BorrowRecords.FindAsync(id);


    public async Task<IEnumerable<BorrowRecords>> GetOverdueAsync() =>
        await _context.BorrowRecords
        .AsNoTracking()
        .Include(b => b.User)
        .Include(b => b.Book)
        .Where(b => !b.IsReturned && b.DueDate < DateTime.UtcNow)
        .ToListAsync();




    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();


    public Task UpdateAsync(BorrowRecords borrow)
    {
        _context.BorrowRecords.Update(borrow);
        return Task.CompletedTask;
    }
}

