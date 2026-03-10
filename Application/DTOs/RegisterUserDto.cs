namespace eLibrary.Application.DTOs;

/// <summary>
/// Incoming registration request from client.
/// </summary>
public class RegisterUserDto
{
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password{ get; set; } = string.Empty;
}

