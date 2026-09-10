using eLibrary.Identity.Domain.Entities;

namespace eLibrary.Identity.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task<(IReadOnlyList<User> Users, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
    Task<bool> DeleteByIdAsync(int id);
    Task<bool> IsEmailDuplicateAsync(string email, int? excludeUserId = null);
    Task<bool> IsUsernameDuplicateAsync(string username, int? excludeUserId = null);
    Task AddAsync(User user);
    Task SaveChangesAsync();
    Task UpdateAsync(User user);
}
