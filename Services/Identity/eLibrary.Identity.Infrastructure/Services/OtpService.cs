using eLibrary.Identity.Application.Interfaces.Services;
using eLibrary.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using eLibrary.Identity.Infrastructure.Persistence;
using eLibrary.Common;
using System.Security.Cryptography;

namespace eLibrary.Identity.Infrastructure.Services;

public class OtpService : IOtpService
{
    private readonly IdentityDbContext _context;

    public OtpService(IdentityDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateOtpAsync(string email)
    {
        var otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var record = await _context.OtpRecords.FirstOrDefaultAsync(r => r.Email == email);

        if (record == null)
        {
            _context.OtpRecords.Add(new OtpRecord
            {
                Email = email,
                Code = otp,
                ExpiryTime = DateTime.UtcNow.AddMinutes(5)
            });
        }
        else
        {
            record.Code = otp;
            record.ExpiryTime = DateTime.UtcNow.AddMinutes(5);
        }

        await _context.SaveChangesAsync();
        return otp;
    }

    public async Task<bool> ValidateOtpAsync(string email, string code)
    {
        if (!AuthInputValidator.IsValidSixDigitCode(code))
            return false;

        var record = await _context.OtpRecords.FirstOrDefaultAsync(r => r.Email == email && r.Code == code);
        if (record == null || record.ExpiryTime < DateTime.UtcNow)
            return false;

        _context.OtpRecords.Remove(record);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsOtpValidAsync(string email, string code)
    {
        if (!AuthInputValidator.IsValidSixDigitCode(code))
            return false;

        return await _context.OtpRecords.AnyAsync(r =>
            r.Email == email && r.Code == code && r.ExpiryTime >= DateTime.UtcNow);
    }
}
