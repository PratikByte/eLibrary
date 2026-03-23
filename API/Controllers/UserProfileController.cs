using eLibrary.Application.Commands.Auth;
using eLibrary.Application.DTOs;
using eLibrary.Application.Queries.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eLibrary.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserProfileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Endpoint to get user profile by ID
    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult> GetUserProfileById(int id)
    {
       if (!CanAccessUser(id))
       {
           return Forbid();
       }

       var response = await _mediator.Send(new GetUserByIdQuery(id));
       if (!response.Success)
       {
           return NotFound(response);
       }
       return Ok(response);
    }
    
    // Endpoint to update user profile by ID
    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateProfile(int id, [FromForm] UserProfileDto userDto)
    {
        if (!CanAccessUser(id))
        {
            return Forbid();
        }

        var response = await _mediator.Send(new UpdateUserProfileCommand(id, userDto));
        if (!response.Success)
        {
            return NotFound(response);
        }
        return Ok(response);
    }

    // Step 1: keep the same access rule as profile read/update.
    // Step 2: ask the application layer for the physical file details.
    // Step 3: return PhysicalFile so the browser downloads the profile image.
    
    // [Authorize]
    // [HttpGet("{id}/profile-image/download")]
    // public async Task<ActionResult> DownloadProfileImage(int id, CancellationToken cancellationToken)
    // {
    //     if (!CanAccessUser(id))
    //     {
    //         return Forbid();
    //     }

    //     var response = await _mediator.Send(new GetProfileImageDownloadQuery(id), cancellationToken);
    //     if (!response.Success || response.Data == null)
    //     {
    //         return NotFound(response);
    //     }

    //     return PhysicalFile(response.Data.FilePath, response.Data.ContentType, response.Data.FileName);
    // }


    private bool CanAccessUser(int userId)
    {
        if (User.IsInRole("Admin"))
        {
            return true;
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(currentUserId, out var parsedUserId) && parsedUserId == userId;
    }
}
