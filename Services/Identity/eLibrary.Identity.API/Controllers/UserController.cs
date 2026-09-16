using System.Security.Claims;
using eLibrary.Identity.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace eLibrary.Identity.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UserController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet("CurrentUser")]
    public IActionResult GetCurrentUser()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Unauthorized(new { message = "No user logged in" });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var role = User.FindFirstValue(ClaimTypes.Role);

        return Ok(new { userId, email, role });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound();

        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email,
            user.Role
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 5)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { message = "Page number must be positive and page size must be between 1 and 100." });

        var (users, totalCount) = await _userRepository.GetPagedAsync(pageNumber, pageSize);
        return Ok(new
        {
            data = users.Select(user => new
            {
                user.Id,
                user.Username,
                user.Email,
                user.Role,
                ProfileImageUrl = user.ProfileImageMongoId is null ? "/profile-images/DefaultUser.svg" : $"/api/UserProfile/{user.Id}/profile-image"
            }),
            totalRecords = totalCount,
            pageNumber,
            pageSize,
            success = true,
            message = "Users fetched successfully."
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] Guid id)
    {
        var deleted = await _userRepository.DeleteByIdAsync(id);
        return deleted
            ? Ok(new { success = true, data = true, message = "User deleted successfully." })
            : NotFound(new { success = false, data = false, message = $"User not found with {id}." });
    }
}
