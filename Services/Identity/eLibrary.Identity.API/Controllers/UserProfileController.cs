using System.Security.Claims;
using eLibrary.Identity.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eLibrary.Identity.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserProfileController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UserProfileController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (!CanAccessUser(id)) return Forbid();

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound();

        return Ok(new
        {
            user.UserId,
            user.Username,
            user.Email,
            user.Role,
            user.ProfileImagePath
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateProfileRequest request)
    {
        if (!CanAccessUser(id)) return Forbid();

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(request.Username))
            user.Username = request.Username;

        if (!string.IsNullOrWhiteSpace(request.Email))
            user.Email = request.Email;

        if (!string.IsNullOrWhiteSpace(request.ProfileImagePath))
            user.ProfileImagePath = request.ProfileImagePath;

        if (request.ProfileImage is not null)
        {
            if (request.ProfileImage.Length == 0)
                return BadRequest(new { message = "Profile image cannot be empty." });

            var webRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
            var directory = Path.Combine(webRoot, "profile-images");
            Directory.CreateDirectory(directory);
            var extension = Path.GetExtension(request.ProfileImage.FileName);
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(directory, fileName);
            await using var stream = System.IO.File.Create(filePath);
            await request.ProfileImage.CopyToAsync(stream);
            user.ProfileImagePath = $"/profile-images/{fileName}";
        }

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return Ok(new
        {
            user.UserId,
            user.Username,
            user.Email,
            user.Role,
            user.ProfileImagePath
        });
    }

    private bool CanAccessUser(int userId)
    {
        if (User.IsInRole("Admin")) return true;

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(currentUserId, out var parsed) && parsed == userId;
    }
}

public class UpdateProfileRequest
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? ProfileImagePath { get; set; }
    public IFormFile? ProfileImage { get; set; }
}
