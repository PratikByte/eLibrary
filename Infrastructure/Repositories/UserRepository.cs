using eLibrary.Application.Interfaces.Repositories;
using eLibrary.Domain.Entities;
using eLibrary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace eLibrary.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly EBookDBContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(EBookDBContext context, ILogger<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> IsDuplicate(string email, string username, int? excludeUserId = null)
    {
        return await _context.Users.AnyAsync(u =>
            (u.Email == email || u.Username == username) &&
            (!excludeUserId.HasValue || u.UserId != excludeUserId.Value));
    }
    public async Task<bool> IsEmailDuplicate(string email, int? excludeUserId = null)
    {
        return await _context.Users.AnyAsync(u =>
            u.Email == email &&
            (!excludeUserId.HasValue || u.UserId != excludeUserId.Value));
    }

    public async Task<bool> IsUsernameDuplicate(string username, int? excludeUserId = null)
    {
        return await _context.Users.AnyAsync(u =>
            u.Username == username &&
            (!excludeUserId.HasValue || u.UserId != excludeUserId.Value));
    }

    public async Task AddRegisterAsync(User user)
    {
        await _context.Users.AddAsync(user);  // entity level data save
    }
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();//commit to DB
    }

    //check user by username
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users.
                     FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User> GetUserByIdAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id);
            return user;
        }
        catch (Exception ex)
        {

            _logger.LogError(ex, "Error fetching user by ID {UserId}", id);
            throw;
        }
    }

    public async Task<(List<User> users, int totalCount)> GetUsersAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        try
        {
            var totalRecords = await _context.Users.CountAsync(cancellationToken);
            var users = await _context.Users
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
            return (users, totalRecords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users for page {PageNumber} with size {PageSize}", pageNumber, pageSize);
            throw;
        }
    }

    public async Task<bool> DeleteByIdAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id, cancellationToken);
            if (user == null)
            {
                return false;
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);
            return true;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user by ID {UserId}", id);
            throw;
        }
    }

    public async Task<User?> GetUserByEmail(string email)
{
    return await _context.Users
        .FirstOrDefaultAsync(u => u.Email == email);
}

}

