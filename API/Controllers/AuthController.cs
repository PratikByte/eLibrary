using eLibrary.Application.Commands.Auth;
using eLibrary.Application.DTOs;
using eLibrary.Common;
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
    // Registration endpoint
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto request)
    {
        var passwordError = AuthInputValidator.ValidatePassword(request.Password);
        if (passwordError is not null)
            return BadRequest(new { message = passwordError });

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


    // Login endpoint that returns JWT token on successful authentication
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginInputDto loginDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var passwordError = AuthInputValidator.ValidatePassword(loginDto.Password);
        if (passwordError is not null)
            return BadRequest(new { message = passwordError });

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

    // Endpoints for password reset using JWT token
    [HttpPost("forget-password-jwt")]
    public async Task<IActionResult> ForgetPasswordJWT([FromBody] ForgotPasswordCommandJWT  command)
    {
       var result = await _mediator.Send(command);
       return Ok(new { Message = "If email exists, reset token generated.", Token = result }); // For backend demo
    }
    //reset password using JWT token
    [HttpPost("reset-password-jwt")]
    public async Task<IActionResult> ResetPasswordJWT([FromBody] ResetPasswordCommandJWT command)
    {
       var passwordError = AuthInputValidator.ValidatePassword(command.NewPassword);
       if (passwordError is not null)
           return BadRequest(new { message = passwordError });

       var result = await _mediator.Send(command);
       
       return Ok(new { Message = result });
  
    }


    //for OTP based password reset 
    [HttpPost("forget-password")]
    public async Task<IActionResult> ForgetPassword([FromBody] ForgotPasswordCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(new { message = result });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    [HttpPost("verify-reset-otp")]
    public async Task<IActionResult> VerifyPasswordOtp([FromBody] VerifyPasswordOtpCommand command)
    {
        if (!AuthInputValidator.IsValidSixDigitCode(command.Otp))
            return BadRequest(new { message = "Enter correct One-time code get on email" });

        try
        {
            var isValid = await _mediator.Send(command);
            if (!isValid)
                return BadRequest(new { message = "Enter correct One-time code get on email" });

            return Ok(new { message = "One-time code verified." });
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new { message = exception.Message });
        }
    }

    //reset password using OTP
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        var passwordError = AuthInputValidator.ValidatePassword(command.NewPassword);
        if (passwordError is not null)
            return BadRequest(new { message = passwordError });
        if (!AuthInputValidator.IsValidSixDigitCode(command.Otp))
            return BadRequest(new { message = "Code must be exactly 6 numeric digits." });

        try
        {
            var result = await _mediator.Send(command);
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
