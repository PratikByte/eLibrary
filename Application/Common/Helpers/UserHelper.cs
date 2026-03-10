using eLibrary.Application.DTOs;
using eLibrary.Domain.Entities;

namespace EBook.Model;

public class UserHelper
{
    public static List<UserDto> MapUsersDto(List<User> users)
    {   //multiple users to userdto
        return users.Select(u => MapUserDto(u)).ToList();
    }
    //single user to userdto
    public static UserDto MapUserDto(User user)
    {
        return new UserDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role
        };
    }
}

