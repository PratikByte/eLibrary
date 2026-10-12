using eLibrary.Application.Interfaces.Services;
using eLibrary.Common;
using eLibrary.Domain.Entities;
using eLibrary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace eLibrary.Infrastructure.Services;

public class OtpService : IOtpService
{
    private readonly EBookDBContext _context;

    public OtpService(EBookDBContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateOtpAsync(string email)
    {
        var otp = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");
        var OtpRecord = new OtpRecord
        {
            Email = email,
            Code = otp,
            ExpiryTime = DateTime.UtcNow.AddMinutes(5) // OTP valid for 5 minutes
        };
        _context.OtpRecords.Add(OtpRecord);
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

        // OTP can be used only once
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
