using eLibrary.Identity.Application.Interfaces.Services;

namespace eLibrary.Identity.Infrastructure.Services;

public class EmailService : IEmailService
{
    public Task SendEmailAsync(string toEmail, string subject, string body)
    {
        // placeholder implementation for now
        Console.WriteLine($"[Email] To: {toEmail} | Subject: {subject} | Body: {body}");
        return Task.CompletedTask;
    }
}
