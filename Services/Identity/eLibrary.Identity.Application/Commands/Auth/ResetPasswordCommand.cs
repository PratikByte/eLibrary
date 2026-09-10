using MediatR;

namespace eLibrary.Identity.Application.Commands.Auth;

public class ResetPasswordCommand : IRequest<string>
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
