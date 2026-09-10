using eLibrary.Identity.Application.DTOs;
using MediatR;

namespace eLibrary.Identity.Application.Commands.Auth;

public class AuthenticateUserCommand : IRequest<UserLoginOutputDto>
{
    public AuthenticateUserCommand(UserLoginInputDto loginData)
    {
        LoginData = loginData;
    }

    public UserLoginInputDto LoginData { get; set; }
}
