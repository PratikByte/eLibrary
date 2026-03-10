using eLibrary.Application.Interfaces.Services;
using eLibrary.Infrastructure.Data;
using MediatR;

namespace eLibrary.Application.Commands.Auth;

public class ForgotPasswordCommandJWT: IRequest<string>
{
    public string Email { get; set; }=string.Empty;

}

public class ForgotPasswordHandlerJWT : IRequestHandler<ForgotPasswordCommandJWT, string>
{
    private readonly ITokenService _tokenService;
    private readonly EBookDBContext _context;

    public ForgotPasswordHandlerJWT(ITokenService tokenService, EBookDBContext context)
    {
        _tokenService = tokenService;
        _context = context;
    }

    public Task<string> Handle(ForgotPasswordCommandJWT request, CancellationToken cancellationToken)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
        if (user == null)
        {
            throw new Exception("User with this email does not exist");
        }
        // Generate short-lived reset token
        var token = _tokenService.GeneratePasswordResetToken(user.UserId);
        // In real life: send via email
        return token;
    }
}

