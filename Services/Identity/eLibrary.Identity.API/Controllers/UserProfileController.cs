using System.Security.Claims;
using eLibrary.Identity.Application.Interfaces.Repositories;
using eLibrary.Identity.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eLibrary.Identity.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserProfileController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IMongoMediaStore _profileImageStore;

    public UserProfileController(IUserRepository userRepository, IMongoMediaStore profileImageStore)
    {
        _userRepository = userRepository;
        _profileImageStore = profileImageStore;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        if (!CanAccessUser(id)) return Forbid();

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound();

        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email,
            user.Role,
            ProfileImageUrl = user.ProfileImageMongoId is null ? "/profile-images/DefaultUser.svg" : $"/api/UserProfile/{user.Id}/profile-image"
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateProfileRequest request)
    {
        if (!CanAccessUser(id)) return Forbid();

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(request.Username))
            user.Username = request.Username;

        if (!string.IsNullOrWhiteSpace(request.Email))
            user.Email = request.Email;

        if (request.ProfileImage is not null)
        {
            if (request.ProfileImage.Length == 0)
                return BadRequest(new { message = "Profile image cannot be empty." });

            if (user.ProfileImageMongoId is not null)
                await _profileImageStore.DeleteAsync(user.ProfileImageMongoId, HttpContext.RequestAborted);

            user.ProfileImageMongoId = await _profileImageStore.SaveAsync(
                request.ProfileImage,
                "profile-image",
                HttpContext.RequestAborted);
        }

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email,
            user.Role,
            ProfileImageUrl = user.ProfileImageMongoId is null ? "/profile-images/DefaultUser.svg" : $"/api/UserProfile/{user.Id}/profile-image"
        });
    }

    [AllowAnonymous]
    [HttpGet("{id}/profile-image")]
    public async Task<IActionResult> GetProfileImage(Guid id, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user?.ProfileImageMongoId is null) return NotFound();
        var image = await _profileImageStore.GetAsync(user.ProfileImageMongoId, cancellationToken);
        return image is null ? NotFound() : File(image.Value.Content, image.Value.ContentType);
    }

    private bool CanAccessUser(Guid userId)
    {
        if (User.IsInRole("Admin")) return true;

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(currentUserId, out var parsed) && parsed == userId;
    }
}

public class UpdateProfileRequest
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public IFormFile? ProfileImage { get; set; }
}
