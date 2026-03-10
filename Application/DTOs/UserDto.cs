namespace eLibrary.Application.DTOs;

public class UserDto
{   
    public int UserId { get; set; } 
    public string Username { get; set; } = string.Empty;
   // public string Password { get; set; } = string.Empty;
    public string Email { get; set; }= string.Empty;
    public string Role { get; set; }    // Default role is User, can be changed to Admin or other roles  
}

