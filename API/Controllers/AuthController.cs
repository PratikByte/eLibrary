using eLibrary.Application.Commands.Auth;
using eLibrary.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace eLibrary.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto request)
    {
        // Send command to CQRS handler
        var result = await _mediator.Send(new RegisterUserCommand(request));

        // Return created user info (without password)
        return Ok(new
        {
            result.Email,
            result.Username,
            Role = "User"
        });
    }



    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginInputDto loginDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var command = new AuthenticateUserCommand(loginDto);
            var result = await _mediator.Send(command);

            return Ok(result);  // returns UserLoginOutputDto (with token)
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }


    [HttpPost("forget-password-jwt")]
    public async Task<IActionResult> ForgetPasswordJWT([FromBody] ForgotPasswordCommandJWT  command)
    {
       var result = await _mediator.Send(command);
       return Ok(new { Message = "If email exists, reset token generated.", Token = result }); // For backend demo
    }

    [HttpPost("reset-password-jwt")]
    public async Task<IActionResult> ResetPasswordJWT([FromBody] ResetPasswordCommandJWT command)
    {
       var result = await _mediator.Send(command);
       
       return Ok(new { Message = result });
  
    }



    [HttpPost("forget-password")]
    public async Task<IActionResult> ForgetPassword([FromBody] ForgotPasswordCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result); // For backend demo
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        var result = await _mediator.Send(command);

        return Ok(result);

    }





}

