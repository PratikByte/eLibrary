using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using eLibrary.Identity.Application.Interfaces.Services;
using eLibrary.Identity.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace eLibrary.Identity.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        var issuer = _configuration["JwtSettings:Issuer"] ?? "eLibrary";
        var audience = _configuration["JwtSettings:Audience"] ?? "eLibraryUsers";
        var keyString = _configuration["JwtSettings:Key"] ?? "VeryStrongKey1234567890VeryStrongKey";

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GeneratePasswordResetToken(Guid userId)
    {
        var issuer = _configuration["JwtSettings:Issuer"] ?? "eLibrary";
        var audience = _configuration["JwtSettings:Audience"] ?? "eLibraryUsers";
        var keyString = _configuration["JwtSettings:Key"] ?? "VeryStrongKey1234567890VeryStrongKey";

        var claims = new[]
        {
            new Claim("userId", userId.ToString()),
            new Claim("purpose", "password_reset")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string? ValidatePasswordResetToken(string token)
    {
        try
        {
            var keyString = _configuration["JwtSettings:Key"] ?? "VeryStrongKey1234567890VeryStrongKey";
            var issuer = _configuration["JwtSettings:Issuer"] ?? "eLibrary";
            var audience = _configuration["JwtSettings:Audience"] ?? "eLibraryUsers";

            var principal = new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString)),
                ClockSkew = TimeSpan.Zero
            }, out _);

            var purpose = principal.FindFirst("purpose")?.Value;
            if (purpose != "password_reset") return null;

            return principal.FindFirst("userId")?.Value;
        }
        catch
        {
            return null;
        }
    }
}
