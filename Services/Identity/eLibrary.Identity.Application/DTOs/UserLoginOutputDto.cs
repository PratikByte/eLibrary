namespace eLibrary.Identity.Application.DTOs;

public class UserLoginOutputDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public string Token { get; set; } = string.Empty;
}
