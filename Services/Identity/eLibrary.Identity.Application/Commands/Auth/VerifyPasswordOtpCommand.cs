using eLibrary.Identity.Application.Interfaces.Repositories;
using eLibrary.Identity.Application.Interfaces.Services;
using MediatR;

namespace eLibrary.Identity.Application.Commands.Auth;

public class VerifyPasswordOtpCommand : IRequest<bool>
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
}

public class VerifyPasswordOtpHandler : IRequestHandler<VerifyPasswordOtpCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otpService;

    public VerifyPasswordOtpHandler(IUserRepository userRepository, IOtpService otpService)
    {
        _userRepository = userRepository;
        _otpService = otpService;
    }

    public async Task<bool> Handle(VerifyPasswordOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            throw new KeyNotFoundException("No account was found with that email address.");

        return await _otpService.IsOtpValidAsync(request.Email, request.Otp);
    }
}
