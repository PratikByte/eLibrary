using eLibrary.Identity.Domain.Entities;

namespace eLibrary.Identity.Application.Interfaces.Services;

public interface ITokenService
{
    string GenerateToken(User user);
    string GeneratePasswordResetToken(Guid userId);
    string? ValidatePasswordResetToken(string token);
}
