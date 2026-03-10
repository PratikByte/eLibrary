namespace eLibrary.Application.DTOs;

// DTO for creating/updating a user profile
public class UserProfileDto
{
    //public int UserId { get; set; }   // For update/delete
    public string? Email { get; set; } 
    public string? Username { get; set; }
   // public string Role { get; set; } = "User";

    // Optional profile picture
    public IFormFile? ProfileImage { get; set; }
}

// DTO returned to clients
public class UserProfileResponseDto
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public string? ProfileImageUrl { get; set; }
}

