using eLibrary.Identity.Application.Interfaces.Repositories;
using eLibrary.Identity.Application.Interfaces.Services;
using eLibrary.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace eLibrary.Identity.Application.Commands.Auth;

public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, string>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IOtpService _otpService;

    public ResetPasswordHandler(
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher,
        IOtpService otpService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _otpService = otpService;
    }

    public async Task<string> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // Same rule as the current app: user must exist and OTP must be valid.
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            return "User with this email does not exist";

        var isValidOtp = await _otpService.ValidateOtpAsync(request.Email, request.Otp);
        if (!isValidOtp)
            return "Invalid or expired OTP";

        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return "Password has been reset successfully";
    }
}
