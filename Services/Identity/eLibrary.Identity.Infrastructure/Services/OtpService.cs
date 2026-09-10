using eLibrary.Identity.Application.Interfaces.Services;
using eLibrary.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using eLibrary.Identity.Infrastructure.Persistence;

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
        var otp = new Random().Next(100000, 999999).ToString();
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
        var record = await _context.OtpRecords.FirstOrDefaultAsync(r => r.Email == email && r.Code == code);
        if (record == null || record.ExpiryTime < DateTime.UtcNow)
            return false;

        _context.OtpRecords.Remove(record);
        await _context.SaveChangesAsync();
        return true;
    }
}
