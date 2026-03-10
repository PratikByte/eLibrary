using eLibrary.Application.Interfaces.Repositories;
using eLibrary.Application.Interfaces.Services;
using eLibrary.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace eLibrary.Application.Commands.Auth;

public class ForgotPasswordCommand: IRequest<string>
{
    public string Email { get; set; }=string.Empty;

}
public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, string>
{
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;
    private readonly IUserRepository _userRepository;

    public ForgotPasswordHandler(IOtpService otpService, IEmailService emailService, IPasswordHasher<User> passwordHasher, IUserRepository userRepository)
    {
        _otpService = otpService;
        _emailService = emailService;
        _userRepository = userRepository;
    }

    public async Task<string> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByEmail(request.Email);
        if (user == null)
            return ("User with this email does not exist");
        var otp = await _otpService.GenerateOtpAsync(request.Email);

        //Email format 
        await _emailService
            .SendEmailAsync(request.Email, "Password Reset OTP", $"Your OTP is: {otp}. It is valid for 5 minutes."
            );
        return "OTP sent to your email ";
    }
}

