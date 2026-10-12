using eLibrary.Application.Interfaces.Services;
using eLibrary.Domain.Entities;
using eLibrary.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace eLibrary.Application.Commands.Auth;

public class VerifyPasswordOtpCommand : IRequest<bool>
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
}

public class VerifyPasswordOtpHandler : IRequestHandler<VerifyPasswordOtpCommand, bool>
{
    private readonly EBookDBContext _context;
    private readonly IOtpService _otpService;

    public VerifyPasswordOtpHandler(EBookDBContext context, IOtpService otpService)
    {
        _context = context;
        _otpService = otpService;
    }

    public async Task<bool> Handle(VerifyPasswordOtpCommand request, CancellationToken cancellationToken)
    {
        var userExists = await _context.Users.AnyAsync(user => user.Email == request.Email, cancellationToken);
        if (!userExists)
            throw new KeyNotFoundException("No account was found with that email address.");

        return await _otpService.IsOtpValidAsync(request.Email, request.Otp);
    }
}
