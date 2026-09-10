namespace eLibrary.Identity.Application.Interfaces.Services;

public interface IOtpService
{
    Task<string> GenerateOtpAsync(string email);
    Task<bool> ValidateOtpAsync(string email, string code);
}
