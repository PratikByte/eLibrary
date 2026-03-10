using eLibrary.Application.Interfaces.Services;
using eLibrary.Domain.Entities;
using eLibrary.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
        var otp = new Random().Next(100000, 999999).ToString();
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
        var record = await _context.OtpRecords.FirstOrDefaultAsync(r => r.Email == email && r.Code == code);
        if (record == null || record.ExpiryTime < DateTime.UtcNow)
            return false;

        // OTP can be used only once
        _context.OtpRecords.Remove(record);
        await _context.SaveChangesAsync();
        return true;
    }

}

