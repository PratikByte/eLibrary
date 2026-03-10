using eLibrary.Application.DTOs;
using eLibrary.Application.Interfaces.Repositories;
using eLibrary.Application.Interfaces.Services;
using eLibrary.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace eLibrary.Application.Commands.Auth;

public class AuthenticateUserCommandHandler : IRequestHandler<AuthenticateUserCommand, UserLoginOutputDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ITokenService _tokenService;
 
    public AuthenticateUserCommandHandler(IUserRepository userRepository, IPasswordHasher<User> passwordHasher, ITokenService tokenService)
    { 
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    } 

    public async Task<UserLoginOutputDto> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
    {
        var loginData=request.LoginData;
        //  Find the user by username
        var user=await _userRepository.GetByUsernameAsync(loginData.Username);
        if (user == null)
        {
            throw new Exception("Invalid username");
        }
        // Verify the password
        var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, loginData.Password);
        if (passwordVerificationResult == PasswordVerificationResult.Failed)
        {
            throw new Exception("Invalid password");
        }
        // Generate JWT token
        var token= await _tokenService.GenerateToken(user);

       return new UserLoginOutputDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            Token = token
        };
    }
}

