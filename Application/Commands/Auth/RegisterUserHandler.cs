using eLibrary.Application.DTOs;
using eLibrary.Application.Interfaces.Repositories;
using eLibrary.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace eLibrary.Application.Commands.Auth;

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
        //check if user already exists
        if (await _userRepository.IsDuplicate(request.RegisterUserDto.Email, request.RegisterUserDto.Username))
        {   
            throw new Exception("User already exists with this email or username");
        }
    
        var user = new User
        {
            Email = request.RegisterUserDto.Email,
            Username = request.RegisterUserDto.Username,
        };

        user.PasswordHash=_passwordHasher.HashPassword(user, request.RegisterUserDto.Password);
    
        await _userRepository.AddRegisterAsync(user);//Entity save
        await _userRepository.SaveChangesAsync();//DB save

        return request.RegisterUserDto;
    }
}

