using System.Net;
using System.Net.Mail;
using eLibrary.Application.Interfaces.Services;

namespace eLibrary.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var host = _config["SmtpSettings:Host"];
        var port = int.Parse(_config["SmtpSettings:Port"]);
        var email = _config["SmtpSettings:Email"];
        var password = _config["SmtpSettings:Password"];

        var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(email, password),
            EnableSsl = true
        };

        var mail = new MailMessage
        {
            From = new MailAddress(email, "E-Library"),
            Subject = subject,
            Body = message,
            IsBodyHtml = true
        };

        mail.To.Add(toEmail);

        await client.SendMailAsync(mail);
    }
}