using eLibrary.Identity.Application.Commands.Auth;
using eLibrary.Identity.Application.DTOs;
using eLibrary.Common;
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
        var passwordError = AuthInputValidator.ValidatePassword(request.Password);
        if (passwordError is not null)
            return BadRequest(new { message = passwordError });

        try
        {
            var result = await _mediator.Send(new RegisterUserCommand(request));
            return Ok(new
            {
                result.Email,
                result.Username,
                Role = "User"
            });
        }
        catch (UserAlreadyExistsException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    // Same behavior as the current root app: check username, verify password, and issue JWT.
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginInputDto request)
    {
        var passwordError = AuthInputValidator.ValidatePassword(request.Password);
        if (passwordError is not null)
            return BadRequest(new { message = passwordError });

        try
        {
            var result = await _mediator.Send(new AuthenticateUserCommand(request));
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { message = "Invalid username or password." });
        }
    }

    // Same behavior as the current root app: generate OTP and send it by email.
    [HttpPost("forget-password")]
    public async Task<IActionResult> ForgetPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            var result = await _mediator.Send(new ForgotPasswordCommand(request.Email));
            return Ok(new { message = result });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpPost("verify-reset-otp")]
    public async Task<IActionResult> VerifyPasswordOtp([FromBody] VerifyPasswordOtpRequest request)
    {
        if (!AuthInputValidator.IsValidSixDigitCode(request.Otp))
            return BadRequest(new { message = "Enter correct One-time code get on email" });

        try
        {
            var isValid = await _mediator.Send(new VerifyPasswordOtpCommand
            {
                Email = request.Email,
                Otp = request.Otp
            });
            if (!isValid)
                return BadRequest(new { message = "Enter correct One-time code get on email" });

            return Ok(new { message = "One-time code verified." });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    // Same behavior as the current root app: verify OTP and update the password.
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var passwordError = AuthInputValidator.ValidatePassword(request.NewPassword);
        if (passwordError is not null)
            return BadRequest(new { message = passwordError });
        if (!AuthInputValidator.IsValidSixDigitCode(request.Otp))
            return BadRequest(new { message = "Code must be exactly 6 numeric digits." });

        try
        {
            var result = await _mediator.Send(new ResetPasswordCommand
            {
                Email = request.Email,
                Otp = request.Otp,
                NewPassword = request.NewPassword
            });

            return Ok(new { message = result });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
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

public class VerifyPasswordOtpRequest
{
    public string Email { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
}
