using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using eLibrary.API.Configurations;
using eLibrary.Application.Interfaces.Services;
using eLibrary.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace eLibrary.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;

    public TokenService(IOptions<JwtSettings> options)
    {
        _jwtSettings = options.Value;
    }

    public Task<string> GenerateToken(User user)
    {
        var normalizedRole = NormalizeRole(user.Role);

        // 🧠 Professor X reading user’s mind and extracting identity
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()), // 🕵️‍♂️ Assigning unique hero ID
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // 🎯 One-time mission ID (Prevents replay attacks)
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64), // ⏳ When the mission starts
            new Claim(ClaimTypes.Role, normalizedRole), // 🛡 Assigning hero rank (Admin/User)
            new Claim(JwtRegisteredClaimNames.Email, user.Email), // 📧 S.H.I.E.L.D email registration
            new Claim(ClaimTypes.Email, user.Email)
        };

        // 💎 Retrieving the secret Infinity Key to sign the token
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));

        // 🔐 Dr. Strange signs token with magical encryption
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Set token expiry
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes);

        // 🛰 Creating token blueprint to be launched across multiverse
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,          // 🌍 Who issues this (S.H.I.E.L.D)
            audience: _jwtSettings.Audience,      // 👥 Who will consume it (Avengers client)
            claims: claims,                       // 🧬 Identity markers
            notBefore: DateTime.UtcNow,           // 🚪 Door opens now
            expires: expires,                     // ⏳ Door closes after mission time
            signingCredentials: creds             // 🛡 Magic seal to protect token
        );

        // 🚀 Sending token to client like Tony’s AI Friday does
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return Task.FromResult(tokenString);
    }

    private static string NormalizeRole(string? role)
    {
        if (string.Equals(role, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return "Admin";
        }

        return "User";
    }

    public Task <string> GeneratePasswordResetToken(int userId)
    {
        //create claims for password reset token //generate a short lived token
        var claims = new[]
        {
            new  Claim("userId", userId.ToString()),
            new Claim("purpose", "password_reset"),
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(15), //short lived token 15 minutes
            signingCredentials: creds
            );
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return Task.FromResult(tokenString);
    }

    public string? ValidatePasswordResetToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            // Check if the token has the correct purpose
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

