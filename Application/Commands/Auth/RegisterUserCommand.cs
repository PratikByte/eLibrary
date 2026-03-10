using eLibrary.Application.DTOs;
using MediatR;

namespace eLibrary.Application.Commands.Auth;

public class RegisterUserCommand: IRequest<RegisterUserDto>
{
    public RegisterUserCommand(RegisterUserDto registerUserDto)
    {
        RegisterUserDto = registerUserDto;
    }

    public RegisterUserDto RegisterUserDto { get; set; }

}

