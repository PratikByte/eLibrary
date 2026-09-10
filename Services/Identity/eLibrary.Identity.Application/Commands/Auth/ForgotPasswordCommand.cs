using MediatR;

namespace eLibrary.Identity.Application.Commands.Auth;

public class ForgotPasswordCommand : IRequest<string>
{
    public ForgotPasswordCommand(string email)
    {
        Email = email;
    }

    public string Email { get; set; } = string.Empty;
}
