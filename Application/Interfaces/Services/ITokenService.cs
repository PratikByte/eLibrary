using eLibrary.Domain.Entities;

namespace eLibrary.Application.Interfaces.Services;

public interface ITokenService
{
    Task <string>GenerateToken(User user);
    Task<string> GeneratePasswordResetToken(int userId);
    string? ValidatePasswordResetToken(string token);

}

