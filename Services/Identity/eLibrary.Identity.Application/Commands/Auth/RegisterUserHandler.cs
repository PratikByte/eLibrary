using eLibrary.Identity.Application.DTOs;
using eLibrary.Identity.Application.Interfaces.Repositories;
using eLibrary.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace eLibrary.Identity.Application.Commands.Auth;

public sealed class UserAlreadyExistsException : Exception
{
    public UserAlreadyExistsException()
        : base("User already exists with this email or username.")
    {
    }
}

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, RegisterUserDto>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;

    public RegisterUserHandler(IUserRepository userRepository, IPasswordHasher<User> passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterUserDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Keep the same validation rule as the old app: do not allow duplicate email/username.
        if (await _userRepository.IsEmailDuplicateAsync(request.RegisterUserDto.Email) ||
            await _userRepository.IsUsernameDuplicateAsync(request.RegisterUserDto.Username))
        {
            throw new UserAlreadyExistsException();
        }

        var user = new User
        {
            Email = request.RegisterUserDto.Email,
            Username = request.RegisterUserDto.Username,
            Role = "User"
        };

        // Use ASP.NET Core password hashing, same as the existing identity flow.
        user.PasswordHash = _passwordHasher.HashPassword(user, request.RegisterUserDto.Password);

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return request.RegisterUserDto;
    }
}
