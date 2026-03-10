using eLibrary.Application.Interfaces.Services;
using eLibrary.Domain.Entities;
using eLibrary.Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace eLibrary.Application.Commands.Auth;

public class ResetPasswordCommand: IRequest<string>
{
    public string Email { get; set; }
    public string Otp { get; set; }
    public string NewPassword { get; set; } 
}

public class ResetPasswordHandler:IRequestHandler<ResetPasswordCommand, string>
{
    private readonly EBookDBContext _context;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IOtpService _otpService;

    public ResetPasswordHandler(EBookDBContext context, IPasswordHasher<User> passwordHasher, IOtpService otpService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _otpService = otpService;
    }

    public async Task<string> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user= _context.Users.FirstOrDefault(u => u.Email == request.Email);
        if(user==null)
            return "User with this email does not exist";

       var isValidOtp= await _otpService.ValidateOtpAsync(request.Email, request.Otp);
        if(!isValidOtp)
          return  "Invalid or expired OTP";
    

        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
        await _context.SaveChangesAsync();
        return "Password has been reset successfully";
    }
}

