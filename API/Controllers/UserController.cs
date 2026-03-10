using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using eLibrary.Application.Commands.Auth;
using eLibrary.Application.DTOs;
using eLibrary.Shared;
using eLibrary.Application.Queries.Auth;

namespace eLibrary.API.Controllers;

[Route("api/[controller]")]
[ApiController]
// [Authorize(Roles = "Admin")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger _logger;
    public UserController(IMediator mediator, ILogger<UserController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
    //get user by id
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<UserDto>>>GetUserById(int id)
    {
        var response = await _mediator.Send(new GetUserByIdQuery(id));
        if(!response.Success)
        {
        
            return NotFound(response);
        }
        return Ok(response);

    }
    //get all users with pagination
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetAllUsers([FromQuery]int pageNumber=1, [FromQuery] int pageSize=5, CancellationToken cancellationToken = default)
    {
        try
        {
        
            var response = await _mediator.Send(new GetUserQuery(pageNumber,pageSize));
            if (!response.Success)
            {
                return NotFound(response);
            }
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching all users.");
            return StatusCode(500, new ApiResponse<List<UserDto>> { Success = false, Message = "An internal server error occurred." });
        }
    }


    //delete user by id
    [HttpDelete]
    public async Task<ActionResult<ApiResponse<bool>>>DeleteUSerById([FromQuery] int id)
    {
        var response=await _mediator.Send(new DeleteUserBYIdCommand(id));
    if(!response.Success)
        {
            return NotFound(response);
        }
        return Ok(response);
    }

    
    //get current logged in user
    [Authorize]
    [HttpGet("CurrentUser")]
    public IActionResult GetCurrentUser()
    {
        if (!User.Identity.IsAuthenticated)
            return Unauthorized(new { message = "No user logged in" });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var role = User.FindFirstValue(ClaimTypes.Role);

        return Ok(new { userId, email, role });
    }

}

