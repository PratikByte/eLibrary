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
    private readonly string _issuer;
    private readonly string _audience;
    private readonly string _key;
    private readonly int _expiryMinutes;
    private readonly int _passwordResetExpiryMinutes;

    public TokenService(IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings");

        _issuer = GetRequiredSetting(jwtSettings, "Issuer");
        _audience = GetRequiredSetting(jwtSettings, "Audience");
        _key = GetRequiredSetting(jwtSettings, "Key");
        _expiryMinutes = GetRequiredPositiveInt(jwtSettings, "ExpiryMinutes");
        _passwordResetExpiryMinutes = GetRequiredPositiveInt(jwtSettings, "PasswordResetExpiryMinutes");
    }

    public string GenerateToken(User user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GeneratePasswordResetToken(Guid userId)
    {
        var claims = new[]
        {
            new Claim("userId", userId.ToString()),
            new Claim("purpose", "password_reset")
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_passwordResetExpiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string? ValidatePasswordResetToken(string token)
    {
        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key)),
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

    private static string GetRequiredSetting(IConfigurationSection settings, string name)
    {
        return settings[name] is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException($"JwtSettings:{name} is not configured.");
    }

    private static int GetRequiredPositiveInt(IConfigurationSection settings, string name)
    {
        if (!int.TryParse(settings[name], out var value) || value <= 0)
        {
            throw new InvalidOperationException($"JwtSettings:{name} must be a positive integer.");
        }

        return value;
    }
}
