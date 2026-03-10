using eLibrary.Application.DTOs;
using MediatR;

namespace eLibrary.Application.Commands.Auth;

public class AuthenticateUserCommand:IRequest<UserLoginOutputDto>
{
    public UserLoginInputDto LoginData { get; set; }
    public AuthenticateUserCommand(UserLoginInputDto input)
    {
        LoginData = input;
    }

}

