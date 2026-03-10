using eLibrary.Domain.Entities;

namespace eLibrary.Application.Interfaces.Repositories;

public interface IUserRepository
{
     //Task<User> RegisterAsync(string email, string password);
    Task AddRegisterAsync(User user);
    Task<bool>IsDuplicate(string email, string username, int? excludeUserId = null);

    Task<bool> IsEmailDuplicate(string email, int? excludeUserId = null);
    Task <bool>IsUsernameDuplicate(string username, int? excludeUserId = null);
    Task SaveChangesAsync();
    Task<User?> GetByUsernameAsync(string Username);

     Task<User?> GetUserByEmail(string email);

    Task<User>GetUserByIdAsync(int id, CancellationToken cancellationToken);
    Task <(List<User> users, int totalCount)> GetUsersAsync(int pageNumber, int pageSize,CancellationToken cancellationToken);
    Task <bool>DeleteByIdAsync(int id, CancellationToken cancellationToken);
}

