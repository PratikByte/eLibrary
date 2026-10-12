using eLibrary.Identity.Application.Interfaces.Repositories;
using eLibrary.Identity.Application.Interfaces.Services;
using MediatR;

namespace eLibrary.Identity.Application.Commands.Auth;

public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, string>
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;

    public ForgotPasswordHandler(
        IUserRepository userRepository,
        IOtpService otpService,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _otpService = otpService;
        _emailService = emailService;
    }

    public async Task<string> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            throw new KeyNotFoundException("No account was found with that email address.");

        var otp = await _otpService.GenerateOtpAsync(request.Email);
        await _emailService.SendEmailAsync(request.Email, "Password Reset OTP", $"Your OTP is: {otp}. It is valid for 5 minutes.");

        return "A security code has been sent to your email.";
    }
}
