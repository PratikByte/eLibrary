using eLibrary.Application.Commands.Auth;
using eLibrary.Application.DTOs;
using eLibrary.Application.Queries.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
    [HttpGet("{id}")]
    public async Task<ActionResult> GetUserProfileById(int id)
    {
       var response = await _mediator.Send(new GetUserByIdQuery(id));
       if (!response.Success)
       {
           return NotFound(response);
       }
       return Ok(response);
    }
    
    // Endpoint to update user profile by ID
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateProfile(int id, [FromForm] UserProfileDto userDto)
    {
        var response = await _mediator.Send(new UpdateUserProfileCommand(id, userDto));
        if (!response.Success)
        {
            return NotFound(response);
        }
        return Ok(response);
    }


}

