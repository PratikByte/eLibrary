using eLibrary.Identity.Application.Commands.Auth;
using eLibrary.Identity.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace eLibrary.Identity.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Same behavior as the current root app: register a user and save identity details.
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto request)
    {
        var result = await _mediator.Send(new RegisterUserCommand(request));
        return Ok(new
        {
            result.Email,
            result.Username,
            Role = "User"
        });
    }

    // Same behavior as the current root app: check username, verify password, and issue JWT.
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginInputDto request)
    {
        try
        {
            var result = await _mediator.Send(new AuthenticateUserCommand(request));
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    // Same behavior as the current root app: generate OTP and send it by email.
    [HttpPost("forget-password")]
    public async Task<IActionResult> ForgetPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _mediator.Send(new ForgotPasswordCommand(request.Email));
        return Ok(new { message = result });
    }

    // Same behavior as the current root app: verify OTP and update the password.
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _mediator.Send(new ResetPasswordCommand
        {
            Email = request.Email,
            Otp = request.Otp,
            NewPassword = request.NewPassword
        });

        return Ok(new { message = result });
    }
}

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
