using eLibrary.Identity.Application.Interfaces.Repositories;
using eLibrary.Identity.Domain.Entities;
using eLibrary.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace eLibrary.Identity.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _context;

    public UserRepository(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(string username)
        => await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

    public async Task<User?> GetByEmailAsync(string email)
        => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetByIdAsync(Guid id)
        => await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<(IReadOnlyList<User> Users, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        string? role = null)
    {
        IQueryable<User> query = _context.Users.AsNoTracking();
        var normalizedSearchTerm = searchTerm?.Trim().ToLower();
        var normalizedRole = role?.Trim();
        if (!string.IsNullOrEmpty(normalizedSearchTerm))
        {
            query = query.Where(user =>
                user.Username.ToLower().Contains(normalizedSearchTerm) ||
                user.Email.ToLower().Contains(normalizedSearchTerm));
        }
        if (!string.IsNullOrEmpty(normalizedRole))
        {
            query = query.Where(user => user.Role.ToLower() == normalizedRole.ToLower());
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .OrderBy(user => user.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (users, totalCount);
    }

    public async Task<bool> DeleteByIdAsync(Guid id)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return false;
        user.IsDeleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsEmailDuplicateAsync(string email, Guid? excludeUserId = null)
        => await _context.Users.AnyAsync(u => u.Email == email && (!excludeUserId.HasValue || u.Id != excludeUserId.Value));

    public async Task<bool> IsUsernameDuplicateAsync(string username, Guid? excludeUserId = null)
        => await _context.Users.AnyAsync(u => u.Username == username && (!excludeUserId.HasValue || u.Id != excludeUserId.Value));

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }
}
