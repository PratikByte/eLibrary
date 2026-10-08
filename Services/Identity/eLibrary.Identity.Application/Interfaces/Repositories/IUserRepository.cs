using eLibrary.Identity.Domain.Entities;

namespace eLibrary.Identity.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<(IReadOnlyList<User> Users, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        string? role = null);
    Task<bool> DeleteByIdAsync(Guid id);
    Task<bool> IsEmailDuplicateAsync(string email, Guid? excludeUserId = null);
    Task<bool> IsUsernameDuplicateAsync(string username, Guid? excludeUserId = null);
    Task AddAsync(User user);
    Task SaveChangesAsync();
    Task UpdateAsync(User user);
}
