using eLibrary.Identity.Application.DTOs;
using eLibrary.Identity.Application.Interfaces.Repositories;
using eLibrary.Identity.Application.Interfaces.Services;
using eLibrary.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace eLibrary.Identity.Application.Commands.Auth;

public class AuthenticateUserHandler : IRequestHandler<AuthenticateUserCommand, UserLoginOutputDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthenticateUserHandler(
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<UserLoginOutputDto> Handle(AuthenticateUserCommand request, CancellationToken cancellationToken)
    {
        // Match the existing app flow: find by username, validate password, then issue JWT.
        var user = await _userRepository.GetByUsernameAsync(request.LoginData.Username);
        if (user == null)
            throw new Exception("Invalid username");

        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.LoginData.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new Exception("Invalid password");

        var token = _tokenService.GenerateToken(user);

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
