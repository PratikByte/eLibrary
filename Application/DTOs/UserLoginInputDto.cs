namespace eLibrary.Application.DTOs;

/// <summary>
/// Incoming login request data from client.
/// </summary>
public class UserLoginInputDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

